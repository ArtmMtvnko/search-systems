using System.Text.Json.Serialization;

namespace ElasticSearch.Models;

class ElasticSearchResponse
{
    [JsonPropertyName("hits")]
    public required ElasticSearchHits Hits { get; set; }
}

class ElasticSearchHits
{
    [JsonPropertyName("hits")]
    public required List<ElasticSearchHit> Documents { get; set; }

    [JsonPropertyName("max_score")]
    public required double MaxScore { get; set; }
}

class ElasticSearchHit
{
    [JsonPropertyName("_id")]
    public required string Id { get; set; }

    [JsonPropertyName("_score")]
    public required double Score { get; set; }

    [JsonPropertyName("_source")]
    public required ElasticDocument ElasticDocument { get; set; }
}
