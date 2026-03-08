namespace ElasticSearch;

class FilterParams
{
    public string? Name { get; set; } = default;

    public bool? IsStaticallyTyped { get; set; } = default;

    public DateTime FirstAppearedAfter { get; set; } = default;

    public DateTime FirstAppearedBefore { get; set; } = default;

    public int ActiveUsersMoreThan { get; set; } = default;

    public int ActiveUsersLessThan { get; set; } = default;

    public List<string> Paradigms { get; set; } = [];
}
