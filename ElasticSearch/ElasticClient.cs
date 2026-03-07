using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
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
}
