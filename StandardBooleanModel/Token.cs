namespace StandardBooleanModel;

readonly record struct Token(TokenType Type, string Value);

enum TokenType
{
    Term,
    And,
    Or,
    Not,
    LeftParen,
    RightParen
}
