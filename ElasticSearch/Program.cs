using ElasticSearch;
using Microsoft.Extensions.Configuration;
using Spectre.Console;
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

while (true)
{
    AnsiConsole.Clear();

    var option = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("Main menu")
            .AddChoices(
            [
                "Create document",
                "Search documents",
                "Delete document",
                "Exit"
            ]));

    if (option == "Exit")
    {
        break;
    }

    if (option == "Create document")
    {
        AnsiConsole.MarkupLine("[Cyan]Fill the form[/]");

        var name = AnsiConsole.Ask<string>("Name:");
        var firstAppeared = AnsiConsole.Ask<DateTime>("First appeared (YYYY-MM-DD):");
        var isTyped = AnsiConsole.Confirm("Is it statically typed language?");
        var users = AnsiConsole.Ask<int>("Active users:");
        var paradigms = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("Select paradigms: ")
                .InstructionsText("[grey](Press [blue]<space>[/] to toggle a paradigm, [green]<enter>[/] to accept)[/]")
                .AddChoices(
                [
                    "Object-oriented",
                    "Functional",
                    "Procedural",
                    "Generic",
                    "Reflective",
                    "Concurrent",
                    "Imperative",
                    "Event-driven"
                ]));

        AnsiConsole.MarkupLine($"Selected paradigms: {string.Join(", ", paradigms)}\n");

        var doc = new ElasticDocument
        {
            Name = name,
            FirstAppeared = firstAppeared,
            IsStaticallyTyped = isTyped,
            ActiveUsers = users,
            Paradigms = paradigms
        };

        var response = await elasticClient.InsertDocument(doc);
        AnsiConsole.MarkupLine(response.ToString());
        AnsiConsole.Confirm("Continue");
    }

    if (option == "Delete document")
    {
        var documents = await elasticClient.GetAllDocuments();

        var names = documents.Select(doc => doc.ElasticDocument.Name);

        var nameToRemoveBy = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Pick a document to remove")
                .AddChoices([.. names, "Cancel"]));

        if (nameToRemoveBy == "Cancel")
        {
            continue;
        }

        var docToRemove = documents.Find(doc => doc.ElasticDocument.Name == nameToRemoveBy);

        if (docToRemove is not null)
        {
            var response = await elasticClient.RemoveDocument(docToRemove.Id);
            AnsiConsole.MarkupLine(response.ToString());
        }

        AnsiConsole.Confirm("Continue");
    }

    if (option == "Search documents")
    {
        var nameSearch = AnsiConsole.Ask<string?>("Name (wildcard syntax):", default);
        var isTyped = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Language typing")
                .AddChoices([
                    "Static",
                    "Dymanic",
                    "Doesn't matter"
                ]));
        AnsiConsole.MarkupLine($"Selected typing: {isTyped}");
        var firstAppearedAfter = AnsiConsole.Ask("First appeared after:", new DateTime(1843, 1, 1));
        var firstAppearedBefore = AnsiConsole.Ask("First appeared before:", DateTime.Today);
        var activeUsersMoreThan = AnsiConsole.Ask("Active users more than:", 0);
        var activeUsersLessThan = AnsiConsole.Ask("Active users less than:", int.MaxValue);
        var paradigms = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("Select paradigms: ")
                .InstructionsText("[grey](Press [blue]<space>[/] to toggle a paradigm, [green]<enter>[/] to accept)[/]")
                .NotRequired()
                .AddChoices(
                [
                    "Object-oriented",
                    "Functional",
                    "Procedural",
                    "Generic",
                    "Reflective",
                    "Concurrent",
                    "Imperative",
                    "Event-driven"
                ]));
        AnsiConsole.MarkupLine($"Selected paradigms: {string.Join(", ", paradigms)}\n");

        var filterDto = new FilterParams
        {
            Name = nameSearch,
            IsStaticallyTyped = isTyped switch
            {
                "Static" => true,
                "Dymanic" => false,
                _ => null
            },
            FirstAppearedAfter = firstAppearedAfter,
            FirstAppearedBefore = firstAppearedBefore,
            ActiveUsersMoreThan = activeUsersMoreThan,
            ActiveUsersLessThan = activeUsersLessThan,
            Paradigms = paradigms
        };

        var documents = await elasticClient.Filter(filterDto);

        AnsiConsole.WriteLine(JsonSerializer.Serialize(documents, serializerOptions));
        AnsiConsole.Confirm("Continue");
    }
}