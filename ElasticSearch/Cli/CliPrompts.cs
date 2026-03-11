using ElasticSearch.Models;
using Spectre.Console;

namespace ElasticSearch.Cli;

static class CliPrompts
{
    private static readonly string[] ParadigmChoices =
    [
        "Object-oriented",
        "Functional",
        "Procedural",
        "Generic",
        "Reflective",
        "Concurrent",
        "Imperative",
        "Event-driven"
    ];

    public static List<string> AskParadigms(bool required = true)
    {
        var prompt = new MultiSelectionPrompt<string>()
            .Title("Select paradigms: ")
            .InstructionsText("[grey](Press [blue]<space>[/] to toggle a paradigm, [green]<enter>[/] to accept)[/]")
            .AddChoices(ParadigmChoices);

        if (!required)
        {
            prompt.NotRequired();
        }

        var paradigms = AnsiConsole.Prompt(prompt);
        AnsiConsole.MarkupLine($"Selected paradigms: {string.Join(", ", paradigms)}\n");

        return [.. paradigms];
    }

    public static bool? AskStaticallyTypedFilter()
    {
        var isTyped = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Language typing")
                .AddChoices([
                    "Static",
                    "Dymanic",
                    "Doesn't matter"
                ]));

        AnsiConsole.MarkupLine($"Selected typing: {isTyped}");

        return isTyped switch
        {
            "Static" => true,
            "Dymanic" => false,
            _ => null
        };
    }

    public static void ConfirmContinue()
    {
        AnsiConsole.Confirm("Continue");
    }

    public static void ShowDocuments(IReadOnlyCollection<ElasticSearchHit> documents)
    {
        if (documents.Count == 0)
        {
            AnsiConsole.MarkupLine("[Yellow]No documents found.[/]");
            return;
        }

        foreach (var hit in documents)
        {
            var document = hit.ElasticDocument;

            var panel = new Panel(new Rows(
                new Markup($"[Grey]Id:[/] {Markup.Escape(hit.Id)}"),
                new Markup($"[Grey]Score:[/] {hit.Score:F2}"),
                new Markup($"[Grey]First appeared:[/] {document.FirstAppeared:yyyy-MM-dd}"),
                new Markup($"[Grey]Statically typed:[/] {document.IsStaticallyTyped}"),
                new Markup($"[Grey]Active users:[/] {document.ActiveUsers:N0}"),
                new Markup($"[Grey]Paradigms:[/] {Markup.Escape(string.Join(", ", document.Paradigms))}"),
                new Markup("[Grey]Description:[/]"),
                new Text(document.Description),
                new Markup("[Grey]History:[/]"),
                new Text(document.History),
                new Markup("[Grey]Code example:[/]"),
                new Text(document.CodeExample)))
            {
                Header = new PanelHeader(Markup.Escape(document.Name)),
                Expand = true
            };

            AnsiConsole.Write(panel);
            AnsiConsole.WriteLine();
        }
    }
}
