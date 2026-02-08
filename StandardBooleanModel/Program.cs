using StandardBooleanModel;

Console.WriteLine("Enter the path to the terms .json file:");
// Return back to ReadLine()
var termsPath = @"C:\Users\artem\Local Documents\University\Search Engine\StandardBooleanModel\terms.json";

Console.WriteLine("Enter the path to the documents folder:");
var docsPath = @"C:\Users\artem\Local Documents\University\Search Engine\StandardBooleanModel\docs";

var index = new InvertedIndex(termsPath, docsPath);
var invertedIndex = index.BuildIndex();

index.PrintIndex();

var queryParser = new QueryParser(invertedIndex);

while (true)
{
    Console.WriteLine("Enter the query (empty line to exit):");
    var queryText = Console.ReadLine() ?? string.Empty;

    if (string.IsNullOrWhiteSpace(queryText))
    {
        break;
    }

    var results = queryParser.Search(queryText);
    Console.WriteLine($"Results: {string.Join(", ", results.OrderBy(r => r))}");
}

