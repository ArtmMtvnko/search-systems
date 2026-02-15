using System.Text.RegularExpressions;

namespace VectorSpaceModel;

public class VSM
{
    private const char[]? documentSeparator = null;

    private readonly string[] _files;
    private readonly Dictionary<string, string[]> _documentTerms = [];
    private readonly Dictionary<string, double[]> _documentVectors = [];

    private readonly double[] _idfWeights;
    private readonly string[] _vocabulary;

    public VSM(string documentsDirectoryPath)
    {
        _files = Directory.GetFiles(documentsDirectoryPath, "*.txt");

        var vocabSet = CreateVocabularySet();
        _vocabulary = [.. vocabSet];

        InitializeDocuments();

        _idfWeights = new double[_vocabulary.Length];

        CalculateAllIdf();
        CalculateAllTf();
    }

    public void PrintVectors()
    {
        Console.WriteLine("Vocabulary weights: [{0}]\n", string.Join(", ", _idfWeights));

        foreach (var (docName, vector) in _documentVectors)
        {
            Console.WriteLine($"Document: {docName}");
            Console.WriteLine($"Vector: [{string.Join(", ", vector)}]\n");
        }
    }

    public Dictionary<string, double> Search(string query)
    {
        var queryTerms = Regex.Split(query, @"\s+");
        var searchVector = CaclulateTfIdf(queryTerms);

        var searchResults = new Dictionary<string, double>();

        foreach (var (docName, docVector) in _documentVectors)
        {
            searchResults[docName] = Similarity(searchVector, docVector);
        }

        return searchResults;
    }

    private static double Similarity(double[] a, double[] b)
    {
        double dot = 0;
        double normA = 0;
        double normB = 0;

        for (int i = 0; i < a.Length; i++)
        {
            var ai = a[i];
            var bi = b[i];
            dot += ai * bi;
            normA += ai * ai;
            normB += bi * bi;
        }

        if (normA == 0 || normB == 0)
        {
            return 0;
        }

        return dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }

    private HashSet<string> CreateVocabularySet()
    {
        var vocabSet = new HashSet<string>();

        foreach (var file in _files)
        {
            var content = File.ReadAllText(file);
            var words = content.Split(documentSeparator, StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                vocabSet.Add(word);
            }
        }

        return vocabSet;
    }

    private void InitializeDocuments()
    {
        foreach (var file in _files)
        {
            var filename = Path.GetFileName(file);
            var content = File.ReadAllText(file);
            var words = content.Split(documentSeparator, StringSplitOptions.RemoveEmptyEntries);

            _documentTerms[filename] = words;
            _documentVectors[filename] = new double[_vocabulary.Length];
        }
    }

    private void CalculateAllIdf()
    {
        double N = _documentVectors.Count;

        for (int i = 0; i < _vocabulary.Length; i++)
        {
            var term = _vocabulary[i];
            double nt = _documentTerms.Count(entry => entry.Value.Contains(term));
            _idfWeights[i] = 1 + Math.Log(N / (1 + nt));
        }
    }

    private void CalculateAllTf()
    {
        foreach (var (docName, weights) in _documentVectors)
        {
            for (int i = 0; i < _vocabulary.Length; i++)
            {
                var term = _vocabulary[i];
                var frequency = _documentTerms[docName].Count(term);
                var tf = Math.Log(1 + frequency);
                var idf = _idfWeights[i];

                weights[i] = tf * idf;
            }
        }
    }

    private double[] CaclulateTfIdf(string[] terms)
    {
        var weights = new double[_vocabulary.Length];

        for (int i = 0; i < _vocabulary.Length; i++)
        {
            var vocabTerm = _vocabulary[i];
            var frequency = terms.Count(vocabTerm);
            var tf = Math.Log(1 + frequency);
            var idf = _idfWeights[i];

            weights[i] = tf * idf;
        }

        return weights;
    }
}
