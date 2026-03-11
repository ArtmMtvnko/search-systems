using ElasticSearch.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ElasticSearch;

class ElasticClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

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

    public Task<List<ElasticSearchHit>> GetAllDocumentsAsync() => SearchAsync(new
    {
        query = new
        {
            match_all = new { }
        }
    });

    public Task<HttpResponseMessage> InsertDocumentAsync(ElasticDocument doc) => PostJsonAsync($"{_indexName}/_doc", doc);

    public Task<HttpResponseMessage> RemoveDocumentAsync(string id) => _httpClient.DeleteAsync($"{_indexName}/_doc/{id}");

    public Task<List<ElasticSearchHit>> SearchByTextAsync(string field, string query)
    {
        object payload = field == "all"
            ? new
            {
                query = new
                {
                    multi_match = new
                    {
                        query,
                        fields = new[] { "description", "history", "code_example" }
                    }
                }
            }
            : new
            {
                query = new
                {
                    match = new Dictionary<string, string>
                    {
                        [field] = query
                    }
                }
            };

        return SearchAsync(payload);
    }

    public Task<List<ElasticSearchHit>> FilterAsync(FilterParams filterParams)
    {
        var filters = BuildFilters(filterParams);

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

        return SearchAsync(query);
    }

    private static List<object> BuildFilters(FilterParams filterParams)
    {
        var filters = new List<object>
        {
            CreateRangeFilter("first_appeared", filterParams.FirstAppearedAfter, filterParams.FirstAppearedBefore),
            CreateRangeFilter("active_users", filterParams.ActiveUsersMoreThan, filterParams.ActiveUsersLessThan)
        };

        if (!string.IsNullOrWhiteSpace(filterParams.Name))
        {
            filters.Add(CreateWildcardFilter("name", filterParams.Name));
        }

        if (filterParams.IsStaticallyTyped is not null)
        {
            filters.Add(CreateTermFilter("is_statically_typed", filterParams.IsStaticallyTyped.Value));
        }

        if (filterParams.Paradigms.Count > 0)
        {
            filters.Add(CreateTermsFilter("paradigms", filterParams.Paradigms));
        }

        return filters;
    }

    private async Task<List<ElasticSearchHit>> SearchAsync(object payload)
    {
        using var response = await PostJsonAsync($"{_indexName}/_search", payload);
        response.EnsureSuccessStatusCode();

        var searchResponse = await ParseElasticHitsAsync(response);
        return searchResponse.Hits.Documents;
    }

    private Task<HttpResponseMessage> PostJsonAsync(string requestUri, object payload) => 
        _httpClient.PostAsJsonAsync(requestUri, payload, SerializerOptions);
    

    private static object CreateRangeFilter<T>(string field, T? gte, T lte) => new
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

    private static object CreateTermFilter(string field, bool value) => new
    {
        term = new Dictionary<string, object>
        {
            [field] = value
        }
    };

    private static async Task<ElasticSearchResponse> ParseElasticHitsAsync(HttpResponseMessage response)
    {
        return await response.Content.ReadFromJsonAsync<ElasticSearchResponse>(SerializerOptions) ?? CreateEmptySearchResponse();
    }

    private static ElasticSearchResponse CreateEmptySearchResponse() => new()
    {
        Hits = new ElasticSearchHits
        {
            Documents = []
        }
    };
}


