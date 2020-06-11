using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace B33Assembler
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
            COMMENT = 1,
            STRING = 2,
            WHITESPACE = 3,
            NEWLINE = 4,
            BINARYNUMBER = 5,
            HEXNUMBER = 6,
            NUMBER = 7,
            POUND = 8,
            DOUBLEPLUS = 9,
            PLUS = 10,
            DOUBLEMINUS = 11,
            MINUS = 12,
            COMMA = 13,
            STR = 14,
            CHR = 15,
            LDA = 16,
            LDB = 17,
            LDD = 18,
            LDX = 19,
            LDY = 20,
            STA = 21,
            STB = 22,
            STD = 23,
            STX = 24,
            STY = 25,
            CMPA = 26,
            CMPB = 27,
            CMPD = 28,
            CMPX = 29,
            CMPY = 30,
            JEQ = 31,
            JNE = 32,
            JLE = 33,
            JGE = 34,
            JLT = 35,
            JGT = 36,
            JOS = 37,
            JOC = 38,
            JMP = 39,
            END = 40,
            PUSH = 41,
            POP = 42,
            CALL = 43,
            RET = 44,
            RMB = 45,
            ADDA = 47,
            ADDB = 48,
            ADDD = 49,
            ADDX = 50,
            ADDY = 51,
            SUBA = 52,
            SUBB = 53,
            SUBD = 54,
            SUBY = 55,
            SUBX = 56,
            LSFT = 57,
            RSFT = 58,
            ANDA = 59,
            ANDB = 60,
            ANDD = 61,
            ANDX = 62,
            ANDY = 63,
            ORA = 64,
            ORB = 65,
            ORD = 66,
            ORX = 67,
            ORY = 68,
            XORA = 69,
            XORB = 70,
            XORD = 71,
            XORX = 72,
            XORY = 73,
            TFR = 74,
            RND = 75,
            MUL8 = 76,
            MUL16 = 77,
            DIV8 = 78,
            DIV16 = 79,
            NOP = 83,
            NEG = 84,
            CLS = 85,
            SCRUP = 86,
            BRK = 87,
            LABEL = 88
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

            _tokens.Add(Tokens.COMMENT, ";.+");
            _tokens.Add(Tokens.STRING, "\".+?\"");
            _tokens.Add(Tokens.WHITESPACE, "[ \\t]+");
            _tokens.Add(Tokens.NEWLINE, "[\\r\\n]+");
            _tokens.Add(Tokens.BINARYNUMBER, "\\%[01]+");
            _tokens.Add(Tokens.HEXNUMBER, "\\$[A-Fa-f0-9]+");
            _tokens.Add(Tokens.NUMBER, "[0-9]+");
            _tokens.Add(Tokens.POUND, "#");
            _tokens.Add(Tokens.DOUBLEPLUS, "\\+\\+");
            _tokens.Add(Tokens.PLUS, "\\+");
            _tokens.Add(Tokens.DOUBLEMINUS, "\\-\\-");
            _tokens.Add(Tokens.MINUS, "\\-");
            _tokens.Add(Tokens.COMMA, "\\,");
            _tokens.Add(Tokens.STR, "[Ss][Tt][Rr](?=[ \\t])");
            _tokens.Add(Tokens.CHR, "[Cc][Hh][Rr](?=[ \\t])");
            _tokens.Add(Tokens.LDA, "[Ll][Dd][Aa](?=[ \\t])");
            _tokens.Add(Tokens.LDB, "[Ll][Dd][Bb](?=[ \\t])");
            _tokens.Add(Tokens.LDD, "[Ll][Dd][Dd](?=[ \\t])");
            _tokens.Add(Tokens.LDX, "[Ll][Dd][Xx](?=[ \\t])");
            _tokens.Add(Tokens.LDY, "[Ll][Dd][Yy](?=[ \\t])");
            _tokens.Add(Tokens.STA, "[Ss][Tt][Aa](?=[ \\t])");
            _tokens.Add(Tokens.STB, "[Ss][Tt][Bb](?=[ \\t])");
            _tokens.Add(Tokens.STD, "[Ss][Tt][Dd](?=[ \\t])");
            _tokens.Add(Tokens.STX, "[Ss][Tt][Xx](?=[ \\t])");
            _tokens.Add(Tokens.STY, "[Ss][Tt][Yy](?=[ \\t])");
            _tokens.Add(Tokens.CMPA, "[Cc][Mm][Pp][Aa](?=[ \\t])");
            _tokens.Add(Tokens.CMPB, "[Cc][Mm][Pp][Bb](?=[ \\t])");
            _tokens.Add(Tokens.CMPD, "[Cc][Mm][Pp][Dd](?=[ \\t])");
            _tokens.Add(Tokens.CMPX, "[Cc][Mm][Pp][Xx](?=[ \\t])");
            _tokens.Add(Tokens.CMPY, "[Cc][Mm][Pp][Yy](?=[ \\t])");
            _tokens.Add(Tokens.JEQ, "[Jj][Ee][Qq](?=[ \\t])");
            _tokens.Add(Tokens.JNE, "[Jj][Nn][Ee](?=[ \\t])");
            _tokens.Add(Tokens.JLE, "[Jj][Ll][Ee](?=[ \\t])");
            _tokens.Add(Tokens.JGE, "[Jj][Gg][Ee](?=[ \\t])");
            _tokens.Add(Tokens.JLT, "[Jj][Ll][Tt](?=[ \\t])");
            _tokens.Add(Tokens.JGT, "[Jj][Gg][Tt](?=[ \\t])");
            _tokens.Add(Tokens.JOS, "[Jj][Oo][Ss](?=[ \\t])");
            _tokens.Add(Tokens.JOC, "[Jj][Oo][Cc](?=[ \\t])");
            _tokens.Add(Tokens.JMP, "[Jj][Mm][Pp](?=[ \\t])");
            _tokens.Add(Tokens.END, "[Ee][Nn][Dd](?=[ \\t])");
            _tokens.Add(Tokens.PUSH, "[Pp][Uu][Ss][Hh](?=[ \\t])");
            _tokens.Add(Tokens.POP, "[Pp][Oo][Pp](?=[ \\t])");
            _tokens.Add(Tokens.CALL, "[Cc][Aa][Ll][Ll](?=[ \\t])");
            _tokens.Add(Tokens.RET, "[Rr][Ee][Tt](?=[\\r\\n\\t ]+)");
            _tokens.Add(Tokens.RMB, "[Rr][Mm][Bb](?=[ \\t])");
            _tokens.Add(Tokens.ADDA, "[Aa][Dd][Dd][Aa](?=[ \\t])");
            _tokens.Add(Tokens.ADDB, "[Aa][Dd][Dd][Bb](?=[ \\t])");
            _tokens.Add(Tokens.ADDD, "[Aa][Dd][Dd][Dd](?=[ \\t])");
            _tokens.Add(Tokens.ADDX, "[Aa][Dd][Dd][Xx](?=[ \\t])");
            _tokens.Add(Tokens.ADDY, "[Aa][Dd][Dd][Yy](?=[ \\t])");
            _tokens.Add(Tokens.SUBA, "[Ss][Uu][Bb][Aa](?=[ \\t])");
            _tokens.Add(Tokens.SUBB, "[Ss][Uu][Bb][Bb](?=[ \\t])");
            _tokens.Add(Tokens.SUBD, "[Ss][Uu][Bb][Dd](?=[ \\t])");
            _tokens.Add(Tokens.SUBY, "[Ss][Uu][Bb][Yy](?=[ \\t])");
            _tokens.Add(Tokens.SUBX, "[Ss][Uu][Bb][Xx](?=[ \\t])");
            _tokens.Add(Tokens.LSFT, "[Ll][Ss][Ff][Tt](?=[ \\t])");
            _tokens.Add(Tokens.RSFT, "[Rr][Ss][Ff][Tt](?=[ \\t])");
            _tokens.Add(Tokens.ANDA, "[Aa][Nn][Dd][Aa](?=[ \\t])");
            _tokens.Add(Tokens.ANDB, "[Aa][Nn][Dd][Bb](?=[ \\t])");
            _tokens.Add(Tokens.ANDD, "[Aa][Nn][Dd][Dd](?=[ \\t])");
            _tokens.Add(Tokens.ANDX, "[Aa][Nn][Dd][Xx](?=[ \\t])");
            _tokens.Add(Tokens.ANDY, "[Aa][Nn][Dd][Yy](?=[ \\t])");
            _tokens.Add(Tokens.ORA, "[Oo][Rr][Aa](?=[ \\t])");
            _tokens.Add(Tokens.ORB, "[Oo][Rr][Bb](?=[ \\t])");
            _tokens.Add(Tokens.ORD, "[Oo][Rr][Dd](?=[ \\t])");
            _tokens.Add(Tokens.ORX, "[Oo][Rr][Xx](?=[ \\t])");
            _tokens.Add(Tokens.ORY, "[OO][Rr][Yy](?=[ \\t])");
            _tokens.Add(Tokens.XORA, "[Xx][Oo][Rr][Aa](?=[ \\t])");
            _tokens.Add(Tokens.XORB, "[Xx][Oo][Rr][Bb](?=[ \\t])");
            _tokens.Add(Tokens.XORD, "[Xx][Oo][Rr][Dd](?=[ \\t])");
            _tokens.Add(Tokens.XORX, "[Xx][Oo][Rr][Xx](?=[ \\t])");
            _tokens.Add(Tokens.XORY, "[Xx][OO][Rr][Yy](?=[ \\t])");
            _tokens.Add(Tokens.TFR, "[Tt][Ff][Rr](?=[ \\t])");
            _tokens.Add(Tokens.RND, "[Rr][Nn][Dd](?=[ \\t])");
            _tokens.Add(Tokens.MUL8, "[Mm][Uu][Ll]8(?=[ \\t])");
            _tokens.Add(Tokens.MUL16, "[Mm][Uu][Ll]16(?=[ \\t])");
            _tokens.Add(Tokens.DIV8, "[Dd][Ii][Vv]8(?=[ \\t])");
            _tokens.Add(Tokens.DIV16, "[Dd][Ii][Vv]16(?=[ \\t])");
            _tokens.Add(Tokens.NOP, "[Nn][Oo][Pp](?=[\\r\\n\\t ]+)");
            _tokens.Add(Tokens.NEG, "[Nn][Ee][Gg](?=[ \\t])");
            _tokens.Add(Tokens.CLS, "[Cc][Ll][Ss](?=[\\r\\n\\t ]+)");
            _tokens.Add(Tokens.BRK, "[Bb][Rr][Kk](?=[\\r\\n\\t ]+)");
            _tokens.Add(Tokens.SCRUP, "[Ss][Cc][Rr][Uu][Pp](?=[\\r\\n\\t ]+)");
            _tokens.Add(Tokens.LABEL, "[a-zA-Z]?[a-zA-Z0-9]+");
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
