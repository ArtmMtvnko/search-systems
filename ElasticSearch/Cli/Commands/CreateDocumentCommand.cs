using Spectre.Console;
using ElasticSearch.Models;

namespace ElasticSearch.Cli.Commands;

class CreateDocumentCommand(ElasticClient elasticClient) : ICliCommand
{
    public string Name => "Create document";

    public async Task ExecuteAsync()
    {
        AnsiConsole.MarkupLine("[Cyan]Fill the form[/]");

        var name = AnsiConsole.Ask<string>("Name:");
        var firstAppeared = AnsiConsole.Ask<DateTime>("First appeared (YYYY-MM-DD):");
        var isTyped = AnsiConsole.Confirm("Is it statically typed language?");
        var users = AnsiConsole.Ask<int>("Active users:");
        var paradigms = CliPrompts.AskParadigms();
        var description = AnsiConsole.Ask<string>("Description:");
        var history = AnsiConsole.Ask<string>("History:");
        var codeExample = AnsiConsole.Ask<string>("Code example:");

        var doc = new ElasticDocument
        {
            Name = name,
            FirstAppeared = firstAppeared,
            IsStaticallyTyped = isTyped,
            ActiveUsers = users,
            Paradigms = paradigms,
            Description = description,
            History = history,
            CodeExample = codeExample
        };

        var response = await elasticClient.InsertDocumentAsync(doc);
        AnsiConsole.MarkupLine(response.ToString());
        CliPrompts.ConfirmContinue();
    }
}
