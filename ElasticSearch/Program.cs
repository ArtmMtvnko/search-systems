using ElasticSearch;
using ElasticSearch.Cli;
using ElasticSearch.Cli.Commands;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

var serializerOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    WriteIndented = true
};

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false)
    .AddUserSecrets<Program>(optional: true)
    .Build();

var elasticClient = new ElasticClient(configuration);

var app = new CliApp([
    new CreateDocumentCommand(elasticClient),
    new FilterDocumentsCommand(elasticClient, serializerOptions),
    new DeleteDocumentCommand(elasticClient),
    new SeedDatabaseCommand(elasticClient)
]);

await app.RunAsync();
