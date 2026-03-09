using ElasticSearch.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ElasticSearch;

class ElasticClient
{
    private readonly HttpClient _httpClient;
    private readonly string _indexName;

    public ElasticClient(IConfiguration configuration)
    {
        var elasticSearchUrl = configuration["ElasticSearchUrl"] ?? throw new KeyNotFoundException("ElasticSearchUrl is not provided");
        var apiKey = configuration["ElasticApiKey"] ?? throw new KeyNotFoundException("ElasticApiKey is not provided");

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(elasticSearchUrl),
            DefaultRequestHeaders =
            {
                Authorization = new AuthenticationHeaderValue("ApiKey", apiKey)
            }
        };

        _indexName = configuration["IndexName"] ?? throw new KeyNotFoundException("IndexName is not provided");
    }

    public async Task<List<ElasticSearchHit>> GetAllDocuments()
    {
        var payload = new Dictionary<string, object>
        {
            ["query"] = new Dictionary<string, object>
            {
                ["match_all"] = new { }
            }
        };

        var jsonPayload = JsonSerializer.Serialize(payload);
        using var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync($"{_indexName}/_search", content);
        response.EnsureSuccessStatusCode();

        var searchResponse = await ParseElasticHitsAsync(response);
        return searchResponse.Hits.Documents;
    }

    public async Task<HttpResponseMessage> InsertDocument(ElasticDocument doc)
    {
        var serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = true
        };
        var jsonDoc = JsonSerializer.Serialize(doc, serializerOptions);
        using var content = new StringContent(jsonDoc, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_indexName}/_doc", content);
        return response;
    }

    public async Task<HttpResponseMessage> RemoveDocument(string id)
    {
        return await _httpClient.DeleteAsync($"{_indexName}/_doc/{id}");
    }

    public async Task<List<ElasticSearchHit>> Filter(FilterParams filterParams)
    {
        var filters = new List<object>
        {
            CreateRangeFilter("first_appeared", filterParams.FirstAppearedAfter, filterParams.FirstAppearedBefore),
            CreateRangeFilter("active_users", filterParams.ActiveUsersMoreThan, filterParams.ActiveUsersLessThan),
        };

        if (!string.IsNullOrWhiteSpace(filterParams.Name))
        {
            filters.Add(CreateWildcardFilter("name", filterParams.Name));
        }

        if (filterParams.IsStaticallyTyped is not null)
        {
            filters.Add(CreateBoolFilter("is_statically_typed", filterParams.IsStaticallyTyped.Value));
        }

        if (filterParams.Paradigms.Count > 0)
        {
            filters.Add(CreateTermsFilter("paradigms", filterParams.Paradigms));
        }

        var query = new
        {
            query = new
            {
                constant_score = new
                {
                    filter = new
                    {
                        @bool = new
                        {
                            filter = filters
                        }
                    }
                }
            }
        };

        var jsonQuery = JsonSerializer.Serialize(query);
        using var content = new StringContent(jsonQuery, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync($"{_indexName}/_search", content);
        response.EnsureSuccessStatusCode();

        var searchResponse = await ParseElasticHitsAsync(response);
        return searchResponse.Hits.Documents;
    }

    private static object CreateRangeFilter<T>(string field, T gte, T lte) => new
    {
        range = new Dictionary<string, object>
        {
            [field] = new
            {
                gte,
                lte
            }
        }
    };

    private static object CreateTermsFilter(string field, IEnumerable<string> values) => new
    {
        terms = new Dictionary<string, IEnumerable<string>>
        {
            [field] = values
        }
    };

    private static object CreateWildcardFilter(string field, string value) => new
    {
        wildcard = new Dictionary<string, object>
        {
            [field] = new
            {
                value,
                case_insensitive = true
            }
        }
    };

    private static object CreateBoolFilter(string field, bool value) => new
    {
        term = new Dictionary<string, object>
        {
            [field] = value
        }
    };

    private static async Task<ElasticSearchResponse> ParseElasticHitsAsync(HttpResponseMessage response)
    {
        var fallbackResponse = new ElasticSearchResponse
        {
            Hits = new ElasticSearchHits
            {
                Documents = []
            }
        };

        var serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = true
        };

        return await response.Content.ReadFromJsonAsync<ElasticSearchResponse>(serializerOptions) ?? fallbackResponse;
    }
}


