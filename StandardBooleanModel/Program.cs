Console.WriteLine("Enter the path to the terms .json file:");
// Return back to ReadLine()
var termsPath = @"C:\Users\artem\Local Documents\University\Search Engine\StandardBooleanModel\terms.json";

Console.WriteLine("Enter the path to the documents folder:");
var docsPath = @"C:\Users\artem\Local Documents\University\Search Engine\StandardBooleanModel\docs";

var index = new InvertedIndex(termsPath, docsPath);
var invertedIndex = index.BuildIndex();

foreach (var (term, documents) in invertedIndex.OrderBy(kvp => kvp.Key))
{
    Console.WriteLine($"{term}: {string.Join(", ", documents.OrderBy(d => d))}");
}

Console.WriteLine("Enter the query:");
var queryText = Console.ReadLine() ?? string.Empty;

var query = new IndexQuery(invertedIndex);
var results = query.Search(queryText);

Console.WriteLine($"Results: {string.Join(", ", results.OrderBy(r => r))}");

