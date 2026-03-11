using ElasticSearch.Models;
using Spectre.Console;
using System.Text.Json;

namespace ElasticSearch.Cli.Commands;

class SeedDatabaseCommand(ElasticClient elasticClient) : ICliCommand
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public string Name => "Seed the database";

    public async Task ExecuteAsync()
    {
        var seedFilePath = Path.Combine(AppContext.BaseDirectory, "seed.json");

        if (!File.Exists(seedFilePath))
        {
            AnsiConsole.MarkupLine($"[Red]Seed file not found:[/] {seedFilePath}");
            CliPrompts.ConfirmContinue();
            return;
        }

        await using var stream = File.OpenRead(seedFilePath);
        var documents = await JsonSerializer.DeserializeAsync<List<ElasticDocument>>(stream, SerializerOptions) ?? [];

        if (documents.Count == 0)
        {
            AnsiConsole.MarkupLine("[Yellow]No documents found in seed.json[/]");
            CliPrompts.ConfirmContinue();
            return;
        }

        AnsiConsole.MarkupLine("[Cyan]Inserting documents into the database...[/]");

        var insertedCount = 0;

        foreach (var document in documents)
        {
            using var response = await elasticClient.InsertDocumentAsync(document);
            response.EnsureSuccessStatusCode();
            insertedCount++;
        }

        AnsiConsole.MarkupLine($"[Green]Inserted {insertedCount} documents into the database.[/]");
        CliPrompts.ConfirmContinue();
    }
}
