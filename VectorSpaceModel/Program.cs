using VectorSpaceModel;

var docsPath = @"C:\Users\artem\Local Documents\University\Search Engine\VectorSpaceModel\docs\";

var model = new VSM(docsPath);
model.PrintVectors();

while (true)
{
    Console.WriteLine("Enter search query (or hit enter to quit):");
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