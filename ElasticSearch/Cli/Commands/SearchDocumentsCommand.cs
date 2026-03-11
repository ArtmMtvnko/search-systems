using Spectre.Console;

namespace ElasticSearch.Cli.Commands;

class SearchDocumentsCommand(ElasticClient elasticClient) : ICliCommand
{
    private static readonly Dictionary<string, string> SearchFieldMappings = new()
    {
        ["Description"] = "description",
        ["History"] = "history",
        ["CodeExample"] = "code_example"
    };

    public string Name => "Search documents";

    public async Task ExecuteAsync()
    {
        var options = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("Select fields to search by (leave empty options to return back)")
                .InstructionsText("[grey](Press [blue]<space>[/] to toggle a field, [green]<enter>[/] to accept)[/]")
                .NotRequired()
                .AddChoices(["Description", "History", "CodeExample"]));

        if (options.Count == 0)
        {
            return;
        }

        var fields = options.Select(option => SearchFieldMappings[option]).ToArray();
        var query = AnsiConsole.Ask<string>("Search text:");
        var documents = await elasticClient.SearchByTextAsync(fields, query);

        CliPrompts.ShowDocuments(documents);
        CliPrompts.ConfirmContinue();
    }
}
