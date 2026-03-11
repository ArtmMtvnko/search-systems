using ElasticSearch;
using ElasticSearch.Cli;
using ElasticSearch.Cli.Commands;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false)
    .AddUserSecrets<Program>(optional: true)
    .Build();

var elasticClient = new ElasticClient(configuration);

var app = new CliApp([
    new CreateDocumentCommand(elasticClient),
    new SearchDocumentsCommand(elasticClient),
    new FilterDocumentsCommand(elasticClient),
    new DeleteDocumentCommand(elasticClient),
    new SeedDatabaseCommand(elasticClient)
]);

await app.RunAsync();
