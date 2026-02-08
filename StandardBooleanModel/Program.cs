using StandardBooleanModel;

Console.WriteLine("Enter the path to the terms .json file:");
var termsPath = Console.ReadLine() ?? string.Empty;

Console.WriteLine("\nEnter the path to the documents folder:");
var docsPath = Console.ReadLine() ?? string.Empty;

var indexBuilder = new InvertedIndexBuilder(termsPath, docsPath);
var invertedIndex = indexBuilder.BuildIndex();

InvertedIndexBuilder.PrintIndex(invertedIndex);

var queryParser = new QueryParser(invertedIndex);

while (true)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("\nEnter the query (empty line to exit):");
    Console.ForegroundColor = ConsoleColor.Cyan;
    var queryText = Console.ReadLine() ?? string.Empty;

    if (string.IsNullOrWhiteSpace(queryText))
    {
        Console.ResetColor();
        break;
    }

    var results = queryParser.Search(queryText);
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Results: {string.Join(", ", results.OrderBy(r => r))}");
}

