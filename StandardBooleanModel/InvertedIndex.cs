using System.Text.Json;

internal sealed class InvertedIndex
{
    private readonly string _termsFileName;
    private readonly string _docsDirectoryName;

    private Dictionary<string, HashSet<string>> _index = new(StringComparer.OrdinalIgnoreCase);

    public InvertedIndex(string termsFileName, string docsDirectoryName)
    {
        _termsFileName = termsFileName;
        _docsDirectoryName = docsDirectoryName;
    }

    public Dictionary<string, HashSet<string>> BuildIndex()
    {
        var termsPath = Path.Combine(AppContext.BaseDirectory, _termsFileName);
        var docsPath = Path.Combine(AppContext.BaseDirectory, _docsDirectoryName);

        var termsJson = File.ReadAllText(termsPath);
        var terms = JsonSerializer.Deserialize<List<string>>(termsJson) ?? [];

        InitializeTerms(terms);
        InitializeDocuments(docsPath, terms);

        return _index;
    }

    private void InitializeTerms(List<string> terms)
    {
        foreach (var term in terms)
        {
            if (!_index.ContainsKey(term))
            {
                _index[term] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
        }
    }

    private void InitializeDocuments(string docsPath, List<string> terms)
    {
        foreach (var docPath in Directory.EnumerateFiles(docsPath, "*.txt"))
        {
            var docName = Path.GetFileName(docPath);
            var content = File.ReadAllText(docPath);

            var allWords = content.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var words = new HashSet<string>(allWords, StringComparer.OrdinalIgnoreCase);

            foreach (var term in terms)
            {
                if (words.Contains(term))
                {
                    _index[term].Add(docName);
                }
            }
        }
    }
}
