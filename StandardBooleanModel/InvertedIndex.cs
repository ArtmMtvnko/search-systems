using System.Text.Json;

internal sealed class InvertedIndex
{
    private readonly string _termsPath;
    private readonly string _docsDirectoryPath;

    private Dictionary<string, HashSet<string>> _index = new(StringComparer.OrdinalIgnoreCase);

    public InvertedIndex(string termsPath, string docsDirectoryPath)
    {
        _termsPath = termsPath;
        _docsDirectoryPath = docsDirectoryPath;
    }

    public Dictionary<string, HashSet<string>> BuildIndex()
    {
        var termsJson = File.ReadAllText(_termsPath);
        var terms = JsonSerializer.Deserialize<List<string>>(termsJson) ?? [];

        InitializeTerms(terms);
        InitializeDocuments(_docsDirectoryPath, terms);

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
            char[]? whitespaceSeparator = null; // Or we can use [' ', '\r\n', '\n', '\r'] for new line chars

            var allWords = content.Split(whitespaceSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
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
