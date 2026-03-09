using Spectre.Console;

namespace ElasticSearch.Cli;

class CliApp(IEnumerable<ICliCommand> commands)
{
    private readonly List<ICliCommand> _commands = [.. commands];

    public async Task RunAsync()
    {
        while (true)
        {
            AnsiConsole.Clear();

            var option = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Main menu")
                    .AddChoices([.. _commands.Select(command => command.Name), "Exit"]));

            if (option == "Exit")
            {
                break;
            }

            var command = _commands.First(command => command.Name == option);
            await command.ExecuteAsync();
        }
    }
}
