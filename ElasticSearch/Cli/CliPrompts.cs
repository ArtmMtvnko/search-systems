using Spectre.Console;

namespace ElasticSearch;

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
}
