using Spectre.Console;

namespace ElasticSearch;

class DeleteDocumentCommand(ElasticClient elasticClient) : ICliCommand
{
    public string Name => "Delete document";

    public async Task ExecuteAsync()
    {
        var documents = await elasticClient.GetAllDocuments();
        var names = documents.Select(doc => doc.ElasticDocument.Name);

        var nameToRemoveBy = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Pick a document to remove")
                .AddChoices([.. names, "Cancel"]));

        if (nameToRemoveBy == "Cancel")
        {
            return;
        }

        var docToRemove = documents.Find(doc => doc.ElasticDocument.Name == nameToRemoveBy);

        if (docToRemove is not null)
        {
            AnsiConsole.MarkupLine($"Selected document: Removing [Yellow]{nameToRemoveBy}[/] with id [Yellow]{docToRemove.Id}[/] ...");
            var response = await elasticClient.RemoveDocument(docToRemove.Id);
            AnsiConsole.MarkupLine("[Green]Success![/]");
            AnsiConsole.MarkupLine(response.ToString());
        }

        CliPrompts.ConfirmContinue();
    }
}
