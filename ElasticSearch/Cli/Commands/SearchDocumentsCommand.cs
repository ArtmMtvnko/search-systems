using Spectre.Console;
using System.Text.Json;

namespace ElasticSearch.Cli.Commands;

class SearchDocumentsCommand(ElasticClient elasticClient, JsonSerializerOptions serializerOptions) : ICliCommand
{
    private static readonly Dictionary<string, string> SearchFieldMappings = new()
    {
        ["Description"] = "description",
        ["History"] = "history",
        ["CodeExample"] = "code_example",
        ["All"] = "all"
    };

    public string Name => "Search documents";

    public async Task ExecuteAsync()
    {
        var option = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Search by field")
                .AddChoices(["Description", "History", "CodeExample", "All", "Cancel"]));

        if (option == "Cancel")
        {
            return;
        }

        var query = AnsiConsole.Ask<string>("Search text:");
        var documents = await elasticClient.SearchByTextAsync(SearchFieldMappings[option], query);

        AnsiConsole.WriteLine(JsonSerializer.Serialize(documents, serializerOptions));
        CliPrompts.ConfirmContinue();
    }
}
