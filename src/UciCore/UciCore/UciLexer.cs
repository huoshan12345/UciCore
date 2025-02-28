namespace UciCore;

/// <summary>
/// Lexical analyzer for UCI (Unified Configuration Interface) configuration files.
/// Converts input text into a stream of tokens that can be consumed by the parser.
/// The lexer handles tokenization of UCI syntax elements including keywords, 
/// identifiers, strings, and comments.
/// </summary>
public class UciLexer
{
    private readonly string _input; // the string being scanned
    private int _start; // start position of the current item
    private int _position; // current position in the input
    private int _line; // current line number in the input
    private int _column; // current column number in the current line
    private NextState? _state; // current state
    private readonly Queue<UciToken> _tokens = []; // channel of scanned items

    /// <summary>
    /// Initializes a new instance of the <see cref="UciLexer"/> class with the specified input text.
    /// </summary>
    /// <param name="input">The UCI configuration text to tokenize.</param>
    public UciLexer(string input)
    {
        _input = input;
        _state = LexStart;
    }

    /// <summary>
    /// Processes the input text and yields a sequence of UCI tokens.
    /// Tokens represent the lexical elements of the UCI configuration syntax, such as
    /// keywords (package, config, option, list), identifiers, values, and comments.
    /// </summary>
    /// <returns>
    /// An enumerable sequence of <see cref="UciToken"/> objects representing
    /// the lexical elements found in the input text. The sequence ends with an EOF token.
    /// </returns>
    public IEnumerable<UciToken> LexTokens()
    {
        while (true)
        {
            if (_tokens.Count > 0)
            {
                var item = _tokens.Dequeue();
                if (item.Type == UciTokenType.Eof)
                {
                    yield break;
                }
                else
                {
                    yield return item;
                }
            }
            else if (_state == null)
            {
                yield break;
            }
            else
            {
                _state = _state();
            }
        }
    }

    /// <summary>
    /// Directly returns an EOF token.
    /// </summary>
    /// <returns></returns>

    private NextState? EmitEof()
    {
        Emit(UciTokenType.Eof);
        return null;
    }

    private UciToken EnqueueToken(UciTokenType type, string value)
    {
        var token = new UciToken(type, value, _position, _line, _column);
        _tokens.Enqueue(token);
        return token;
    }

    /// <summary>
    /// Emits a token
    /// </summary>
    /// <param name="t"></param>
    private void Emit(UciTokenType t)
    {
        Debug.Assert(_start <= _position);
        Emit(t, _input[_start.._position]);
    }

    private void Emit(UciTokenType type, string value)
    {
        Debug.Assert(_start <= _position);
        EnqueueToken(type, value);
        _start = _position;
    }

    private void EmitLineFeed()
    {
        Emit(UciTokenType.LineFeed, "\n");
        _line++;
        _column = 0;
    }

    /// <summary>
    /// Returns the next rune in the input
    /// </summary>
    /// <returns></returns>
    private char? Next()
    {
        var ch = Peek();
        if (ch is not null)
        {
            _position++;
            _column++;
        }
        return ch;
    }

    /// <summary>
    /// Returns but does not consume the next rune in the input.
    /// </summary>
    /// <returns></returns>
    private char? Peek()
    {
        return _position >= _input.Length
            ? null
            : _input[_position];
    }

    /// <summary>
    /// Skip space and tab characters.
    /// </summary>
    private void SkipBlankSpaces()
    {
        SkipWhile(m => m.IsBlankSpace());
    }

    /// <summary>
    /// Skip white space characters.
    /// </summary>
    private void SkipWhiteSpaces()
    {
        SkipWhile(m => m.IsWhiteSpace());
    }

    /// <summary>
    /// Skip chars until a specified condition is false
    /// </summary>
    /// <param name="predicate"></param>
    private void SkipWhile(Func<char?, bool> predicate)
    {
        while (true)
        {
            var ch = Peek();
            if (predicate(ch) == false)
                break;

            if (ch.IsLineFeed())
                EmitLineFeed();

            _position++;
            _column++;
        }
        _start = _position;
    }

    /// <summary>
    /// Returns an error token and terminates the scan by passing back
    /// a null pointer that will be the next state, terminating Run.
    /// </summary>
    /// <param name="format"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private NextState? Error([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args)
    {
        EnqueueToken(UciTokenType.Error, string.Format(format, args));
        return null;
    }

    private NextState? LexStart()
    {
        SkipWhiteSpaces();

        if (Peek().IsEof())
            return EmitEof();

        if (AdvanceIfStartsWith("#"))
            return LexComment();
        if (AdvanceIfStartsWith("package"))
            return LexPackage();
        if (AdvanceIfStartsWith("config"))
            return LexSection();
        if (AdvanceIfStartsWith("option"))
            return LexOption();
        if (AdvanceIfStartsWith("list"))
            return LexList();

        var rest = Rest();
        var unexpected = rest.Length > 50
            ? rest[..50].ToString() + "..."
            : rest.ToString();
        Error("expected keyword (package, config, option, list) or eof but got: {0}", unexpected);

        return null;
    }

    private NextState? LexComment()
    {
        while (true)
        {
            var ch = Next();
            if (ch.IsNewLine())
            {
                _position--;
                break;
            }
            else if (ch.IsEof())
            {
                break;
            }
        }
        Emit(UciTokenType.Comment);
        return LexStart();
    }

    private NextState? LexPackage()
    {
        return LexString(UciTokenType.PackageName);
    }

    private NextState? LexSection()
    {
        return LexString(UciTokenType.SectionType);
    }

    private NextState? LexOption()
    {
        return LexString(UciTokenType.OptionName, () => LexString(UciTokenType.OptionValue));
    }

    private NextState? LexList()
    {
        return LexString(UciTokenType.ListName, () => LexString(UciTokenType.OptionValue));
    }

    private NextState? LexString(UciTokenType type, NextState? next = null)
    {
        SkipBlankSpaces();

        var hasLineFeed = false;
        var inQuotes = false;
        var quotes = new Stack<char>();
        using var disposable = StringBuilderHelper.GetCached();
        var builder = disposable.Value;
        while (true)
        {
            // TODO: add support for escape. The quote ' or " can be escaped by \
            var ch = Next();

            if (ch.IsQuote())
            {
                if (quotes.Count > 0 && ch == quotes.Peek())
                {
                    quotes.Pop();
                    inQuotes = false;
                }
                else
                {
                    quotes.Push(ch.Value);
                    inQuotes = true;
                }
            }
            else if (ch.IsEof())
            {
                if (inQuotes)
                    return Error("unterminated quoted string");

                break;
            }
            else
            {
                if (inQuotes == false && ch == '#')
                {
                    _position--;
                    next = LexComment + next;
                    break;
                }

                if (inQuotes == false && ch.IsWhiteSpace())
                {
                    if (ch.IsLineFeed())
                        hasLineFeed = true;

                    if (type == UciTokenType.SectionType)
                    {
                        if (ch.IsNewLine())
                        {
                            next = () =>
                            {
                                Emit(UciTokenType.SectionName, "");
                                return LexStart();
                            };
                        }
                        else
                        {
                            next = () => LexString(UciTokenType.SectionName);
                        }
                    }

                    break;
                }

                if (type is UciTokenType.PackageName or UciTokenType.SectionName
                    && ch.IsValidForIdentifier() == false)
                {
                    return Error("{0} is not allowed for an identifier", ch);
                }

                builder.Append(ch.Value);
            }
        }

        if (type is UciTokenType.PackageName or UciTokenType.OptionName
            && builder.Length == 0)
        {
            return Error("An identifier cannot be empty");
        }

        var str = builder.ToString();
        Emit(type, str);

        if (hasLineFeed)
            EmitLineFeed();

        return next ?? LexStart();
    }

    private ReadOnlySpan<char> Rest()
    {
        return _input.AsSpan(_position);
    }

    private bool AdvanceIfStartsWith(string value)
    {
        if (_position + value.Length > _input.Length)
            return false;

        if (Rest().StartsWith(value.AsSpan()) == false)
            return false;

        _position += value.Length;
        return true;
    }
}

file static class Extensions
{
    /// <summary>
    /// Check a char is space, tab, or newline.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsWhiteSpace([NotNullWhen(true)] this char? value)
    {
        return value is { } ch && char.IsWhiteSpace(ch);
    }

    /// <summary>
    /// Check a char is space, tab.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsBlankSpace([NotNullWhen(true)] this char? value)
    {
        return value.IsWhiteSpace() && value.IsNewLine() == false;
    }

    /// <summary>
    /// Checks if the given character is a quote character (' or ").
    /// </summary>
    /// <param name="value">The character to check.</param>
    /// <returns>True if the character is a quote; otherwise, false.</returns>
    public static bool IsQuote([NotNullWhen(true)] this char? value)
    {
        return value is '\'' or '"';
    }

    /// <summary>
    /// Checks if the given character is a carriage return character (\r).
    /// </summary>
    /// <param name="value">The character to check.</param>
    /// <returns>True if the character is a carriage return; otherwise, false.</returns>
    public static bool IsCarriageReturn([NotNullWhen(true)] this char? value)
    {
        return value is '\r';
    }

    /// <summary>
    /// Checks if the given character is a line feed character (\n).
    /// </summary>
    /// <param name="value">The character to check.</param>
    /// <returns>True if the character is a line feed; otherwise, false.</returns>
    public static bool IsLineFeed([NotNullWhen(true)] this char? value)
    {
        return value is '\n';
    }

    /// <summary>
    /// Checks if the given character is a new line character, 
    /// which can be either a carriage return (\r) or a line feed (\n).
    /// </summary>
    /// <param name="value">The character to check.</param>
    /// <returns>True if the character is a new line; otherwise, false.</returns>
    public static bool IsNewLine([NotNullWhen(true)] this char? value)
    {
        return value.IsCarriageReturn() || value.IsLineFeed();
    }

    public static bool IsEof([NotNullWhen(false)] this char? value)
    {
        return value is null;
    }

    public static bool IsNewLineOrEof(this char? value)
    {
        return value.IsNewLine() || value.IsEof();
    }

    public static bool IsValidForIdentifier(this char? value)
    {
        // NOTE: strictly, '-' is allowed for name of package or config,
        // but not allowed for name of option or list.
        return value is '-' or '_'
            or >= 'a' and <= 'z'
            or >= 'A' and <= 'Z'
            or >= '0' and <= '9';
    }

}