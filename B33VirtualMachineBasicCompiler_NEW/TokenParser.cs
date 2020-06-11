using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace B33VirtualMachineBasicCompiler
{
    internal class TokenParser
    {
        private readonly Dictionary<Tokens, string> _tokens;
        private readonly Dictionary<Tokens, MatchCollection> _regExMatchCollection;
        private string _inputString;
        private int _index;

        public enum Tokens
        {
            UNDEFINED = 0,
            LINENUMBER = 1,
            COMMA = 2,
            PLUS = 3,
            MINUS = 4,
            ASTERISK = 5,
            WHITESPACE = 6,
            NEWLINE = 7,
            COLON = 8,
            CLS = 9,
            PRINT = 10,
            LOCATE = 11,
            GOTO = 12,
            END = 13,
            STRING = 14,
            LABEL = 15,
            LPAREN = 16,
            RPAREN = 17,
            SLASH = 18,
            PERCENT = 19,
            GREATERTHAN = 20,
            LESSTHAN = 21,
            GREATERTHENOREQUAL = 22,
            LESSTHANOREQUAL = 23,
            EQUAL = 24,
            IF = 25,
            THEN = 26,
            NOTEQUAL = 27,
            CALL = 28,
            RETURN = 29,
            WHILE = 30,
            LOOP = 31,
            BGCOLOR = 32,
            FGCOLOR = 33,
            FOR = 34,
            TO = 35,
            NEXT = 36,
            POKE = 37,
            COMMENT = 38,
            STRINGLABEL = 39,
            CHR = 40,
            PRINTSTR = 41,
            PRINTNUM = 42,
            MID = 43,
            UCASE = 44,
            LCASE = 45,
            STEP = 46,
            DIM = 47,
            AND = 48,
            OR = 49,
            HEXNUMBER = 50,
            CURSOR = 51,
            ON = 52,
            OFF = 53,
            INPUT = 54,
            LEN = 55,
            BREAK = 56
        }

        public string InputString
        {
            set
            {
                _inputString = value;
                PrepareRegex();
            }
        }

        public TokenParser()
        {
            _tokens = new Dictionary<Tokens, string>();
            _regExMatchCollection = new Dictionary<Tokens, MatchCollection>();
            _index = 0;
            _inputString = string.Empty;

            _tokens.Add(Tokens.HEXNUMBER, "\\&[hH][0-9A-Fa-f]{1,4}");
            _tokens.Add(Tokens.LINENUMBER, "[0-9]+");
            _tokens.Add(Tokens.COMMA, ",");
            _tokens.Add(Tokens.LPAREN, "\\(");
            _tokens.Add(Tokens.RPAREN, "\\)");
            _tokens.Add(Tokens.PLUS, "\\+");
            _tokens.Add(Tokens.MINUS, "-");
            _tokens.Add(Tokens.ASTERISK, "\\*");
            _tokens.Add(Tokens.SLASH, "/");
            _tokens.Add(Tokens.PERCENT, "\\%");
            _tokens.Add(Tokens.WHITESPACE, "[ \\t]+");
            _tokens.Add(Tokens.NEWLINE, "([\\r\\n])|(\\n)+");
            _tokens.Add(Tokens.COLON, "\\:");
            _tokens.Add(Tokens.COMMENT, "([Rr][Ee][Mm])|(\\')");
            _tokens.Add(Tokens.CHR, "[Cc][Hh][Rr][\\$][ \\t]*\\(");
            _tokens.Add(Tokens.MID, "[Mm][Ii][Dd][\\$][ \\t]*\\(");
            _tokens.Add(Tokens.INPUT, "[Ii][Nn][Pp][Uu][Tt][\\$][ \\t]*\\(");
            _tokens.Add(Tokens.UCASE, "[Uu][Cc][Aa][Ss][Ee][\\$][ \\t]*\\(");
            _tokens.Add(Tokens.LCASE, "[Ll][Cc][Aa][Ss][Ee][\\$][ \\t]*\\(");
            _tokens.Add(Tokens.CLS, "[Cc][Ll][Ss]");
            _tokens.Add(Tokens.BREAK, "[Bb][Rr][Ee][Aa][Kk]");
            _tokens.Add(Tokens.PRINTNUM, "[Pp][Rr][Ii][Nn][Tt][Nn][Uu][Mm]");
            _tokens.Add(Tokens.PRINTSTR, "[Pp][Rr][Ii][Nn][Tt][Ss][Tt][Rr]");
            _tokens.Add(Tokens.PRINT, "[Pp][Rr][Ii][Nn][Tt]");
            _tokens.Add(Tokens.LOCATE, "[Ll][Oo][Cc][Aa][Tt][Ee]");
            _tokens.Add(Tokens.GOTO, "[Gg][Oo][Tt][Oo]");
            _tokens.Add(Tokens.CALL, "[Cc][Aa][Ll][Ll]");
            _tokens.Add(Tokens.WHILE, "[Ww][Hh][Ii][Ll][Ee]");
            _tokens.Add(Tokens.LOOP, "[Ll][Oo][Oo][Pp]");
            _tokens.Add(Tokens.FOR, "[Ff][Oo][Rr]");
            _tokens.Add(Tokens.NEXT, "[Nn][Ee][Xx][Tt]");
            _tokens.Add(Tokens.TO, "[Tt][Oo]");
            _tokens.Add(Tokens.STEP, "[Ss][Tt][Ee][Pp]");
            _tokens.Add(Tokens.RETURN, "[Rr][Ee][Tt][Uu][Rr][Nn]");
            _tokens.Add(Tokens.DIM, "[Dd][Ii][Mm]");
            _tokens.Add(Tokens.BGCOLOR, "[Bb][Gg][Cc][Oo][Ll][Oo][Rr]");
            _tokens.Add(Tokens.FGCOLOR, "[Ff][Gg][Cc][Oo][Ll][Oo][Rr]");
            _tokens.Add(Tokens.POKE, "[Pp][Oo][Kk][Ee]");
            _tokens.Add(Tokens.IF, "[Ii][Ff]");
            _tokens.Add(Tokens.THEN, "[Tt][Hh][Ee][Nn]");
            _tokens.Add(Tokens.END, "[Ee][Nn][Dd]");
            _tokens.Add(Tokens.OR, "[Oo][Rr]");
            _tokens.Add(Tokens.ON, "[Oo][Nn]");
            _tokens.Add(Tokens.OFF, "[Oo][Ff][Ff]");
            _tokens.Add(Tokens.CURSOR, "[Cc][Uu][Rr][Ss][Oo][Rr]");
            _tokens.Add(Tokens.AND, "[Aa][Nn][Dd]");
            _tokens.Add(Tokens.STRING, "\".*?\"");
            _tokens.Add(Tokens.STRINGLABEL, "[a-zA-Z_]?[a-zA-Z0-9]+\\$");
            _tokens.Add(Tokens.LEN, "[Ll][Ee][Nn][ \\t]*\\(");
            _tokens.Add(Tokens.LABEL, "[a-zA-Z_]?[a-zA-Z0-9]+");
            _tokens.Add(Tokens.NOTEQUAL, "<>");
            _tokens.Add(Tokens.LESSTHANOREQUAL, "<=");
            _tokens.Add(Tokens.LESSTHAN, "<");
            _tokens.Add(Tokens.GREATERTHENOREQUAL, ">=");
            _tokens.Add(Tokens.GREATERTHAN, ">");
            _tokens.Add(Tokens.EQUAL, "=");
        }

        private void PrepareRegex()
        {
            _regExMatchCollection.Clear();
            foreach (KeyValuePair<Tokens, string> pair in _tokens)
            {
                _regExMatchCollection.Add(pair.Key, Regex.Matches(_inputString, pair.Value));
            }
        }

        public void ResetParser()
        {
            _index = 0;
            _inputString = string.Empty;
            _regExMatchCollection.Clear();
        }

        public Token GetToken()
        {
            if (_index >= _inputString.Length)
                return null;

            foreach (KeyValuePair<Tokens, MatchCollection> pair in _regExMatchCollection)
            {
                foreach (Match match in pair.Value)
                {
                    if (match.Index == _index)
                    {
                        _index += match.Length;
                        return new Token(pair.Key, match.Value);
                    }

                    if (match.Index > _index)
                    {
                        break;
                    }
                }
            }
            _index++;
            return new Token(Tokens.UNDEFINED, string.Empty);
        }

        public PeekToken Peek()
        {
            return Peek(new PeekToken(_index, new Token(Tokens.UNDEFINED, string.Empty)));
        }

        public PeekToken Peek(PeekToken peekToken)
        {
            int oldIndex = _index;

            _index = peekToken.TokenIndex;

            if (_index >= _inputString.Length)
            {
                _index = oldIndex;
                return null;
            }

            foreach (KeyValuePair<Tokens, string> pair in _tokens)
            {
                var r = new Regex(pair.Value);
                Match m = r.Match(_inputString, _index);

                if (m.Success && m.Index == _index)
                {
                    _index += m.Length;
                    var pt = new PeekToken(_index, new Token(pair.Key, m.Value));
                    _index = oldIndex;
                    return pt;
                }
            }
            var pt2 = new PeekToken(_index + 1, new Token(Tokens.UNDEFINED, string.Empty));
            _index = oldIndex;
            return pt2;
        }
    }

    internal class PeekToken
    {
        internal int TokenIndex { get; set; }

        public Token TokenPeek { get; set; }

        public PeekToken(int index, Token value)
        {
            TokenIndex = index;
            TokenPeek = value;
        }
    }

    internal class Token
    {
        public TokenParser.Tokens TokenName { get; set; }

        public string TokenValue { get; set; }

        public Token(TokenParser.Tokens name, string value)
        {
            TokenName = name;
            TokenValue = value;
        }
    }
}
