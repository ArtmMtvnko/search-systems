var index = new InvertedIndex("terms.json", "docs");
var invertedIndex = index.BuildIndex();

foreach (var (term, documents) in invertedIndex.OrderBy(kvp => kvp.Key))
{
    Console.WriteLine($"{term}: {string.Join(", ", documents.OrderBy(d => d))}");
}
