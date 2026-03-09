namespace ElasticSearch;

interface ICliCommand
{
    string Name { get; }

    Task ExecuteAsync();
}
