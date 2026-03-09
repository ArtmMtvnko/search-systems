using Spectre.Console;
using System.Text.Json;

namespace ElasticSearch.Cli.Commands;

class SearchDocumentsCommand(ElasticClient elasticClient, JsonSerializerOptions serializerOptions) : ICliCommand
{
    public string Name => "Search documents";

    public async Task ExecuteAsync()
    {
        var nameSearch = AnsiConsole.Ask<string?>("Name (wildcard syntax):", default);
        var isTyped = CliPrompts.AskStaticallyTypedFilter();
        var firstAppearedAfter = AnsiConsole.Ask("First appeared after:", new DateTime(1843, 1, 1));
        var firstAppearedBefore = AnsiConsole.Ask("First appeared before:", DateTime.Today);
        var activeUsersMoreThan = AnsiConsole.Ask("Active users more than:", 0);
        var activeUsersLessThan = AnsiConsole.Ask("Active users less than:", int.MaxValue);
        var paradigms = CliPrompts.AskParadigms(required: false);

        var filterParams = new FilterParams
        {
            Name = nameSearch,
            IsStaticallyTyped = isTyped,
            FirstAppearedAfter = firstAppearedAfter,
            FirstAppearedBefore = firstAppearedBefore,
            ActiveUsersMoreThan = activeUsersMoreThan,
            ActiveUsersLessThan = activeUsersLessThan,
            Paradigms = paradigms
        };

        var documents = await elasticClient.Filter(filterParams);

        AnsiConsole.WriteLine(JsonSerializer.Serialize(documents, serializerOptions));
        CliPrompts.ConfirmContinue();
    }
}
