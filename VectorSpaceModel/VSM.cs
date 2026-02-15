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
        Console.WriteLine("Vocabulary weights: [{0}]", string.Join(", ", _idfWeights));

        foreach (var (docName, vector) in _documentVectors)
        {
            Console.WriteLine($"Document: {docName}");
            Console.WriteLine($"Vector: [{string.Join(", ", vector)}]");
            Console.WriteLine();
        }
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
}
