using VectorSpaceModel;

Console.WriteLine("Enter path to directory with documents:");
var docsPath = Console.ReadLine() ?? string.Empty;

var model = new VSM(docsPath);
//model.PrintVectors();

while (true)
{
    Console.WriteLine("\nEnter search query (or hit enter to quit):");
    var query = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(query))
    {
        break;
    }

    var results = model.Search(query);

    results
        .OrderByDescending(entry => entry.Value)
        .ToList()
        .ForEach(entry =>
        {
            var textColor = entry.Value > 0.25
                ? ConsoleColor.Green
                : entry.Value > 0.1
                    ? ConsoleColor.Yellow
                    : ConsoleColor.Red;

            Console.ForegroundColor = textColor;
            Console.WriteLine($"Document: {entry.Key}, Similarity: {entry.Value}");
            Console.ResetColor();
        }
        );
}