using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B33VirtualMachineBasicCompiler
{
    public static class Extensions
    {
        internal static void SkipWhiteSpace(this TokenParser parser)
        {
            if (parser == null || parser.Peek() == null || parser.Peek().TokenPeek == null)
                return;
            Token token = parser.Peek().TokenPeek;

            while (token.TokenName == TokenParser.Tokens.WHITESPACE)
            {
                parser.GetToken();
                if (parser.Peek() == null || parser.Peek().TokenPeek == null)
                    return;
                token = parser.Peek().TokenPeek;
            }
        }

        internal static string GetStringExpression(this TokenParser parser)
        {
            string retval = "";
            int parens = 0;

            parser.SkipWhiteSpace();

            PeekToken pToken = parser.Peek();

            while (pToken != null && pToken.TokenPeek.TokenName != TokenParser.Tokens.NEWLINE)
            {
                if (pToken.TokenPeek.TokenName == TokenParser.Tokens.STRING || pToken.TokenPeek.TokenName == TokenParser.Tokens.PLUS ||
                    pToken.TokenPeek.TokenName == TokenParser.Tokens.STRINGLABEL)
                {
                    retval = retval + pToken.TokenPeek.TokenValue;
                    parser.GetToken();
                    parser.SkipWhiteSpace();
                    pToken = parser.Peek();
                    continue;
                }

                if (pToken.TokenPeek.TokenName == TokenParser.Tokens.LPAREN)
                {
                    parens++;
                    retval = retval + pToken.TokenPeek.TokenValue;
                    parser.GetToken();
                    parser.SkipWhiteSpace();
                    pToken = parser.Peek();
                    continue;
                }

                if (pToken.TokenPeek.TokenName == TokenParser.Tokens.RPAREN)
                {
                    parens--;
                    retval = retval + pToken.TokenPeek.TokenValue;
                    parser.GetToken();

                    if (parens == 0)
                        break;
                }

                if (pToken.TokenPeek.TokenName == TokenParser.Tokens.COLON && parens == 0)
                {
                    break;
                }

                
                parser.SkipWhiteSpace();
                parser.GetToken();
                parser.SkipWhiteSpace();
                pToken = parser.Peek();
            }

            return retval;
        }

        internal static string GetNumericExpression(this TokenParser parser)
        {
            string retval = "";

            parser.SkipWhiteSpace();

            return retval;
        }
    }
}
