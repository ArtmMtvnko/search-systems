using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

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

        var fallbackResponse = new ElasticSearchResponse
        {
            Hits = new ElasticSearchHits
            {
                Documents = []
            }
        };
        var searchResponse = await response.Content.ReadFromJsonAsync<ElasticSearchResponse>() ?? fallbackResponse;

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
}

class ElasticSearchResponse
{
    [JsonPropertyName("hits")]
    public required ElasticSearchHits Hits { get; set; }
}

class ElasticSearchHits
{
    [JsonPropertyName("hits")]
    public required List<ElasticSearchHit> Documents { get; set; }
}

class ElasticSearchHit
{
    [JsonPropertyName("_id")]
    public required string Id { get; set; }

    [JsonPropertyName("_source")]
    public required ElasticDocument ElasticDocument { get; set; }
}

