namespace ElasticSearch.Models;

class ElasticDocument
{
    public required string Name { get; set; }

    public DateTime FirstAppeared { get; set; }

    public bool IsStaticallyTyped { get; set; }

    public int ActiveUsers { get; set; }

    public required List<string> Paradigms { get; set; }
}
