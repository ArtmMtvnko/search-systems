namespace ElasticSearch.Cli;

interface ICliCommand
{
    string Name { get; }

    Task ExecuteAsync();
}
