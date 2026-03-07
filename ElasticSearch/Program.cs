using ElasticSearch;
using Microsoft.Extensions.Configuration;
using Spectre.Console;

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
                    "Imperative"
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
                .AddChoices([..names, "Cancel"]));

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
}