namespace StandardBooleanModel;

sealed class QueryParser
{
    private readonly IReadOnlyDictionary<string, HashSet<string>> _index;
    private readonly HashSet<string> _allDocuments;
    private List<Token> _tokens = [];
    private int _position;

    public QueryParser(IReadOnlyDictionary<string, HashSet<string>> index)
    {
        _index = index;
        _allDocuments = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var documents in _index.Values)
        {
            _allDocuments.UnionWith(documents);
        }
    }

    public HashSet<string> Search(string query)
    {
        _tokens = Tokenize(query);
        _position = 0;

        var result = ParseAndExpression();
        return result ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    private HashSet<string> GetTermSet(string term)
    {
        return _index.TryGetValue(term, out var documents)
            ? new HashSet<string>(documents, StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    private HashSet<string> Negate(HashSet<string> documents)
    {
        var result = new HashSet<string>(_allDocuments, StringComparer.OrdinalIgnoreCase);
        result.ExceptWith(documents);
        return result;
    }

    private static List<Token> Tokenize(string query)
    {
        var tokens = new List<Token>();
        var index = 0;

        while (index < query.Length)
        {
            var current = query[index];

            if (char.IsWhiteSpace(current))
            {
                index++;
                continue;
            }

            if (current == '(')
            {
                tokens.Add(new Token(TokenType.LeftParen, "("));
                index++;
                continue;
            }

            if (current == ')')
            {
                tokens.Add(new Token(TokenType.RightParen, ")"));
                index++;
                continue;
            }

            if (char.IsLetterOrDigit(current) || current == '_')
            {
                var start = index;
                while (index < query.Length && (char.IsLetterOrDigit(query[index]) || query[index] == '_'))
                {
                    index++;
                }

                var word = query[start..index];
                var type = word.ToUpperInvariant() switch
                {
                    "AND" => TokenType.And,
                    "OR" => TokenType.Or,
                    "NOT" => TokenType.Not,
                    _ => TokenType.Term
                };

                tokens.Add(new Token(type, word));
                continue;
            }

            index++;
        }

        return tokens;
    }

    private HashSet<string>? ParseAndExpression()
    {
        var left = ParseOrExpression();

        while (Match(TokenType.And))
        {
            var right = ParseOrExpression();
            left = Intersect(left, right);
        }

        return left;
    }

    private HashSet<string> ParseOrExpression()
    {
        var left = ParseUnaryExpression();

        while (Match(TokenType.Or))
        {
            var right = ParseUnaryExpression();
            left = Union(left, right);
        }

        return left;
    }

    private HashSet<string> ParseUnaryExpression()
    {
        if (Match(TokenType.Not))
        {
            var value = ParseUnaryExpression();
            return Negate(value);
        }

        if (Match(TokenType.LeftParen))
        {
            var value = ParseAndExpression();
            Expect(TokenType.RightParen);
            return value ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        if (Match(TokenType.Term, out var token))
        {
            return GetTermSet(token.Value);
        }

        return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    private bool Match(TokenType type)
    {
        if (_position < _tokens.Count && _tokens[_position].Type == type)
        {
            _position++;
            return true;
        }

        return false;
    }

    private bool Match(TokenType type, out Token token)
    {
        if (_position < _tokens.Count && _tokens[_position].Type == type)
        {
            token = _tokens[_position];
            _position++;
            return true;
        }

        token = default;
        return false;
    }

    private void Expect(TokenType type)
    {
        if (!Match(type))
        {
            _position = _tokens.Count;
        }
    }

    private static HashSet<string> Union(HashSet<string> left, HashSet<string> right)
    {
        var result = new HashSet<string>(left, StringComparer.OrdinalIgnoreCase);
        result.UnionWith(right);
        return result;
    }

    private static HashSet<string> Intersect(HashSet<string> left, HashSet<string> right)
    {
        var result = new HashSet<string>(left, StringComparer.OrdinalIgnoreCase);
        result.IntersectWith(right);
        return result;
    }
}
