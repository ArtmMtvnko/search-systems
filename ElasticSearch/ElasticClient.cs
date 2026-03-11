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

    public Task<List<ElasticSearchHit>> GetAllDocumentsAsync() => SearchAsync(ElasticQueryBuilder.CreateMatchAllQuery());

    public Task<HttpResponseMessage> InsertDocumentAsync(ElasticDocument doc) => PostJsonAsync($"{_indexName}/_doc", doc);

    public Task<HttpResponseMessage> RemoveDocumentAsync(string id) => _httpClient.DeleteAsync($"{_indexName}/_doc/{id}");

    public Task<List<ElasticSearchHit>> SearchByTextAsync(IEnumerable<string> fields, string query) =>
        SearchAsync(ElasticQueryBuilder.CreateTextSearchQuery(fields, query));

    public Task<List<ElasticSearchHit>> FilterAsync(FilterParams filterParams) =>
        SearchAsync(ElasticQueryBuilder.CreateFilterQuery(filterParams));

    private async Task<List<ElasticSearchHit>> SearchAsync(object payload)
    {
        using var response = await PostJsonAsync($"{_indexName}/_search", payload);
        response.EnsureSuccessStatusCode();

        var searchResponse = await ParseElasticHitsAsync(response);
        return searchResponse.Hits.Documents;
    }

    private Task<HttpResponseMessage> PostJsonAsync(string requestUri, object payload) => 
        _httpClient.PostAsJsonAsync(requestUri, payload, SerializerOptions);
    

    private static async Task<ElasticSearchResponse> ParseElasticHitsAsync(HttpResponseMessage response)
    {
        return await response.Content.ReadFromJsonAsync<ElasticSearchResponse>(SerializerOptions) ?? CreateEmptySearchResponse();
    }

    private static ElasticSearchResponse CreateEmptySearchResponse() => new()
    {
        Hits = new ElasticSearchHits
        {
            Documents = [],
            MaxScore = null
        }
    };
}


