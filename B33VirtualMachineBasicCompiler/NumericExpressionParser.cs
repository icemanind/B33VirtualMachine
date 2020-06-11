using System;
using System.Collections.Generic;
using System.Linq;

namespace B33VirtualMachineBasicCompiler
{
    internal class NumericExpressionParser : IExpressionParser
    {
        public ExpressionParserOutput Output { get; set; }
        public Dictionary<string, string> Variables { get; set; }
        
        private Token _token;
        private bool _hasLabel;

        private void AddVariable(string variableName)
        {
            if (!Variables.ContainsKey(variableName))
            {
                Variables.Add(variableName, variableName + " rmb 2" + Environment.NewLine);
            }
        }

        private void SkipWhiteSpace(TokenParser parser)
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
        
        private void Factor(TokenParser parser)
        {
            if (_token == null)
                return;

            if (_token.TokenName == TokenParser.Tokens.LPAREN)
            {
                SkipWhiteSpace(parser);
                _token = parser.GetToken();
                SkipWhiteSpace(parser);
                Expression(parser);
                if (_token == null)
                    return;
                SkipWhiteSpace(parser);
                _token = parser.GetToken();
                SkipWhiteSpace(parser);
            } else if (_token.TokenName == TokenParser.Tokens.LABEL || TokenParser.GetNumericFunctionsList().Contains(_token.TokenName))
            {
                string name = _token.TokenValue.ToUpper().Trim();
                bool isArray = false;
                _hasLabel = true;

                switch (_token.TokenName)
                {
                    case TokenParser.Tokens.LEN:
                        name = "LEN";
                        break;
                    case TokenParser.Tokens.ASC:
                        name = "ASC";
                        break;
                    case TokenParser.Tokens.RND:
                        name = "RND";
                        break;
                    case TokenParser.Tokens.INSTR:
                        name = "INSTR";
                        break;
                }

                SkipWhiteSpace(parser);
                if (parser.Peek() != null && parser.Peek().TokenPeek != null &&
                    (parser.Peek().TokenPeek.TokenName == TokenParser.Tokens.LPAREN) || TokenParser.GetNumericFunctionsList().Contains(_token.TokenName))
                {
                    if (Variables.ContainsKey("varArray" + name) && Variables["varArray" + name].StartsWith("varArray" + name + " rmb "))
                    {
                        isArray = true;
                        int paren = 1;
                        string exp = "";

                        while (_token != null && paren >= 1)
                        {
                            var peek = parser.Peek();

                            if (peek != null && peek.TokenPeek != null && peek.TokenPeek.TokenValue != null)
                            {
                                if (peek.TokenPeek.TokenName == TokenParser.Tokens.LPAREN)
                                {
                                    paren++;
                                }
                                if (peek.TokenPeek.TokenName == TokenParser.Tokens.RPAREN)
                                {
                                    paren--;
                                    if (paren == 1)
                                    {
                                        SkipWhiteSpace(parser);
                                        parser.GetToken();
                                        SkipWhiteSpace(parser);
                                        exp = exp + ")";
                                        _token = parser.GetToken();
                                        SkipWhiteSpace(parser);
                                        break;
                                    }
                                }
                                exp = exp + peek.TokenPeek.TokenValue;
                                _token = parser.GetToken();
                            }
                            else break;
                        }
                        var nc2 = new NumericExpressionParser {Variables = Variables};
                        nc2.Compile("(" + exp + ")", Output.StringCounter, Output.LoopCounter);

                        Output.StringCounter = nc2.Output.StringCounter;
                        Output.LoopCounter = nc2.Output.LoopCounter;
                        Output.InitCode = Output.InitCode + nc2.Output.InitCode;

                        foreach (KeyValuePair<string, string> kvp in nc2.Variables)
                        {
                            if (!Variables.ContainsKey(kvp.Key))
                            {
                                Variables.Add(kvp.Key, kvp.Value);
                            }
                        }

                        Output.Output = Output.Output + nc2.Output.Output;
                        Output.Output = Output.Output + " tfr x,d" + Environment.NewLine;
                        Output.Output = Output.Output + " tfr b,a" + Environment.NewLine;
                        Output.Output = Output.Output + " ldx #varArray" + name + Environment.NewLine;
                        Output.Output = Output.Output + " call ArrayIndexNumeric" + Environment.NewLine;
                        Output.Output = Output.Output + " ldb ,x+" + Environment.NewLine;
                        Output.Output = Output.Output + " lda ,x" + Environment.NewLine;
                        Output.Output = Output.Output + " tfr d,x" + Environment.NewLine;
                    }
                }
                if (!isArray && !TokenParser.GetNumericFunctionsList().Contains(_token.TokenName))
                    _token = parser.GetToken();
                SkipWhiteSpace(parser);

                if (_token != null && (_token.TokenName == TokenParser.Tokens.LPAREN || TokenParser.GetNumericFunctionsList().Contains(_token.TokenName)))
                {
                    if (_token.TokenName == TokenParser.Tokens.INSTR)
                    {
                        DoInstr(parser);

                        SkipWhiteSpace(parser);
                        _token = parser.GetToken();
                        return;
                    }
                    int paren = 1;
                    string exp = "";

                    while (_token != null && paren > 0)
                    {
                        var peek = parser.Peek();
                        if (peek != null && peek.TokenPeek != null && peek.TokenPeek.TokenValue != null)
                        {
                            if (peek.TokenPeek.TokenName == TokenParser.Tokens.LPAREN)
                            {
                                paren++;
                            }
                            if (peek.TokenPeek.TokenName == TokenParser.Tokens.RPAREN)
                            {
                                paren--;
                                if (paren == 0)
                                {
                                    _token = parser.GetToken();
                                    break;
                                }
                            }
                            exp = exp + peek.TokenPeek.TokenValue;
                            _token = parser.GetToken();
                        }
                        else break;
                    }
                    IExpressionParser eParser = BooleanExpressionParser.IsString(exp)
                        ? (IExpressionParser) new StringExpressionParser()
                        : new NumericExpressionParser();

                    eParser.Variables = Variables;
                    eParser.Compile(exp, Output.StringCounter, Output.LoopCounter);

                    Output.StringCounter = eParser.Output.StringCounter;
                    Output.LoopCounter = eParser.Output.LoopCounter;
                    Output.InitCode = Output.InitCode + eParser.Output.InitCode;

                    foreach (KeyValuePair<string, string> kvp in eParser.Variables)
                    {
                        if (!Variables.ContainsKey(kvp.Key))
                        {
                            Variables.Add(kvp.Key, kvp.Value);
                        }
                    }
                    
                    //Output.Output = Output.Output + " push y" + Environment.NewLine;
                    //if (BooleanExpressionParser.IsString(exp))
                    //    Output.Output = Output.Output + " ldy #TheBasicStrBuf" + Environment.NewLine;
                    //Output.Output = Output.Output + eParser.Output.Output;
                    //if (BooleanExpressionParser.IsString(exp))
                    //    Output.Output = Output.Output + " ldx #TheBasicStrBuf" + Environment.NewLine;
                    //if (name.Trim().ToUpper() == "INPUT")
                    //    Output.Output = Output.Output + " call func" + name + "2" + Environment.NewLine;
                    //else Output.Output = Output.Output + " call func" + name + Environment.NewLine;
                    //Output.Output = Output.Output + " pop y" + Environment.NewLine;

                    Output.Output = Output.Output + eParser.Output.Output;
                    if (BooleanExpressionParser.IsString(exp))
                    {
                        Output.Output = Output.Output + " push y" + Environment.NewLine;
                        Output.Output = Output.Output + " push x" + Environment.NewLine;
                        Output.Output = Output.Output + " call func" + name + Environment.NewLine;
                        Output.Output = Output.Output + " tfr x,y" + Environment.NewLine;
                        Output.Output = Output.Output + " pop x" + Environment.NewLine;
                        Output.Output = Output.Output + " call freememory" + Environment.NewLine;
                        Output.Output = Output.Output + " tfr y,x" + Environment.NewLine;
                        Output.Output = Output.Output + " pop y" + Environment.NewLine;
                    }
                    else
                    {
                        
                        Output.Output = Output.Output + " call func" + name + Environment.NewLine;
                    }

                    SkipWhiteSpace(parser);
                    _token = parser.GetToken();
                    SkipWhiteSpace(parser);
                }
                else
                {
                    if (!isArray)
                        Output.Output = Output.Output + " ldx var" + name + Environment.NewLine;
                    AddVariable("var" + name);
                }
            }
            else
            {
                if (_token.TokenName == TokenParser.Tokens.HEXNUMBER)
                    Output.Output = Output.Output + " ldx #$" + _token.TokenValue.Replace("&h", "").Replace("&H", "") + Environment.NewLine;
                else Output.Output = Output.Output + " ldx #" + _token.TokenValue + Environment.NewLine;
                SkipWhiteSpace(parser);
                _token = parser.GetToken();
                SkipWhiteSpace(parser);
            }
        }

        private string GetNumericExpression(TokenParser parser, bool inParenthesis)
        {
            string retval = "";
            int outerParens = 0;

            if (parser == null || parser.Peek() == null || parser.Peek().TokenPeek == null)
                return "";

            if (parser.Peek() == null)
                return "";

            Token ptToken = parser.Peek().TokenPeek;

            while (ptToken != null && ptToken.TokenName != TokenParser.Tokens.NEWLINE)
            {
                if (ptToken.TokenName == TokenParser.Tokens.LINENUMBER || ptToken.TokenName == TokenParser.Tokens.PLUS ||
                    ptToken.TokenName == TokenParser.Tokens.MINUS || ptToken.TokenName == TokenParser.Tokens.ASTERISK ||
                    ptToken.TokenName == TokenParser.Tokens.SLASH || ptToken.TokenName == TokenParser.Tokens.PERCENT ||
                    ptToken.TokenName == TokenParser.Tokens.LABEL || ptToken.TokenName == TokenParser.Tokens.WHITESPACE ||
                    TokenParser.GetNumericFunctionsList().Contains(ptToken.TokenName))
                {
                    retval = retval + ptToken.TokenValue;
                    parser.GetToken();
                    if (parser.Peek() == null)
                        break;

                    if (TokenParser.GetNumericFunctionsList().Contains(ptToken.TokenName))
                    {
                        int parens = 1;

                        while (parens != 0)
                        {
                            Token token = parser.GetToken();
                            if (token.TokenName == TokenParser.Tokens.LPAREN)
                                parens++;
                            if (token.TokenName == TokenParser.Tokens.RPAREN)
                                parens--;

                            retval = retval + token.TokenValue;
                        }
                    }
                    if (parser.Peek() == null)
                        break;
                    ptToken = parser.Peek().TokenPeek;
                    continue;
                }

                if (ptToken.TokenName == TokenParser.Tokens.LPAREN)
                {
                    outerParens++;

                    parser.GetToken();
                    retval = retval + "(";
                    if (parser.Peek() == null)
                        break;
                    ptToken = parser.Peek().TokenPeek;
                    continue;
                }

                if (ptToken.TokenName == TokenParser.Tokens.RPAREN && outerParens > 0)
                {
                    outerParens--;

                    parser.GetToken();
                    retval = retval + ")";
                    if (parser.Peek() == null)
                        break;
                    if (inParenthesis && outerParens == 0)
                        break;
                    ptToken = parser.Peek().TokenPeek;
                    continue;
                }

                if (outerParens == 0)
                    break;

                retval = retval + parser.Peek().TokenPeek.TokenValue;
                parser.GetToken();
                if (parser.Peek() == null)
                    break;
                ptToken = parser.Peek().TokenPeek;
            }

            return retval;
        }

        private string GetStringExpression(TokenParser parser)
        {
            if (parser == null || parser.Peek() == null || parser.Peek().TokenPeek == null)
                return "";

            string retval = "";
            int parens = 0;

            while (parser.Peek() != null && parser.Peek().TokenPeek != null &&
                   parser.Peek().TokenPeek.TokenName != TokenParser.Tokens.NEWLINE &&
                   TokenParser.GetStatementsList().All(z => z != parser.Peek().TokenPeek.TokenName))
            {
                if (parser.Peek().TokenPeek.TokenName == TokenParser.Tokens.COMMA && parens == 0)
                    break;
                if (parser.Peek().TokenPeek.TokenValue.Trim().EndsWith("("))
                    parens++;
                if (parser.Peek().TokenPeek.TokenValue.Trim().EndsWith(")"))
                {
                    parens--;
                    if (parens < 0)
                        break;
                }

                Token token = parser.GetToken();
                retval = retval + token.TokenValue;
            }

            return retval;
        }

        private void DoInstr(TokenParser parser)
        {
            SkipWhiteSpace(parser);

            string exp1 = GetNumericExpression(parser, false);

            SkipWhiteSpace(parser);
            // Should be a comma
            parser.GetToken();
            SkipWhiteSpace(parser);

            string exp2 = GetStringExpression(parser);

            SkipWhiteSpace(parser);
            // Should be a comma
            parser.GetToken();
            SkipWhiteSpace(parser);

            string exp3 = GetStringExpression(parser).Trim();
            //exp3 = exp3.Remove(exp3.Length - 1, 1);

            SkipWhiteSpace(parser);
            // Should be closing parenthesis
            parser.GetToken();
            SkipWhiteSpace(parser);

            StringExpressionParser sourceString = new StringExpressionParser {Variables = Variables};

            sourceString.Compile(exp2, Output.StringCounter, Output.LoopCounter);

            Output.StringCounter = sourceString.Output.StringCounter;
            Output.LoopCounter = sourceString.Output.LoopCounter;
            Output.BoolCounter = sourceString.Output.BoolCounter;
            Output.InitCode = Output.InitCode + sourceString.Output.InitCode;

            foreach (Functions f in sourceString.Output.Functions)
            {
                if (!Output.Functions.Contains(f))
                    Output.Functions.Add(f);
            }

            string param2Code = sourceString.Output.Output;

            StringExpressionParser destString = new StringExpressionParser { Variables = Variables };

            destString.Compile(exp3, Output.StringCounter, Output.LoopCounter);

            Output.StringCounter = destString.Output.StringCounter;
            Output.LoopCounter = destString.Output.LoopCounter;
            Output.BoolCounter = destString.Output.BoolCounter;
            Output.InitCode = Output.InitCode + destString.Output.InitCode;

            foreach (Functions f in destString.Output.Functions)
            {
                if (!Output.Functions.Contains(f))
                    Output.Functions.Add(f);
            }

            string param3Code = destString.Output.Output;

            NumericExpressionParser startIndex = new NumericExpressionParser {Variables = Variables};

            startIndex.Compile(exp1, Output.StringCounter, Output.LoopCounter);

            Output.StringCounter = startIndex.Output.StringCounter;
            Output.LoopCounter = startIndex.Output.LoopCounter;
            Output.BoolCounter = startIndex.Output.BoolCounter;
            Output.InitCode = Output.InitCode + startIndex.Output.InitCode;

            Output.Output = Output.Output + startIndex.Output.Output;
            Output.Output = Output.Output + " push x" + Environment.NewLine;
            Output.Output = Output.Output + param2Code;
            Output.Output = Output.Output + " push x" + Environment.NewLine;
            Output.Output = Output.Output + param3Code;
            Output.Output = Output.Output + " call funcINSTR" + Environment.NewLine;
        }

        private void Term(TokenParser parser)
        {
            if (_token == null)
                return;
            Factor(parser);
            if (_token == null)
                return;
            while (_token.TokenName == TokenParser.Tokens.ASTERISK || _token.TokenName == TokenParser.Tokens.SLASH ||
                   _token.TokenName == TokenParser.Tokens.PERCENT)
            {
                Output.Output = Output.Output + " push x" + Environment.NewLine;
                if (_token.TokenName == TokenParser.Tokens.ASTERISK)
                {
                    SkipWhiteSpace(parser);
                    _token = parser.GetToken();
                    SkipWhiteSpace(parser);
                    Factor(parser);
                    Output.Output = Output.Output + " pop y" + Environment.NewLine;
                    Output.Output = Output.Output + " mul16 x,y" + Environment.NewLine;
                } else if (_token.TokenName == TokenParser.Tokens.SLASH)
                {
                    SkipWhiteSpace(parser);
                    _token = parser.GetToken();
                    SkipWhiteSpace(parser);
                    Factor(parser);
                    Output.Output = Output.Output + " pop y" + Environment.NewLine;
                    Output.Output = Output.Output + " div16 y,x" + Environment.NewLine;
                } else if (_token.TokenName == TokenParser.Tokens.PERCENT)
                {
                    SkipWhiteSpace(parser);
                    _token = parser.GetToken();
                    SkipWhiteSpace(parser);
                    Factor(parser);
                    Output.Output = Output.Output + " pop y" + Environment.NewLine;
                    Output.Output = Output.Output + " div16 y,x" + Environment.NewLine;
                    Output.Output = Output.Output + " tfr y,x" + Environment.NewLine;
                }
                if (_token == null)
                    return;
            }
        }

        private void Expression(TokenParser parser)
        {
            if (_token == null)
                return;

            if (_token.TokenName == TokenParser.Tokens.PLUS || _token.TokenName == TokenParser.Tokens.MINUS)
            {
                Output.Output = Output.Output + " ldx #0" + Environment.NewLine;
            }
            else
            {
                Term(parser);
            }
            if (_token == null)
                return;

            while (_token.TokenName == TokenParser.Tokens.PLUS || _token.TokenName == TokenParser.Tokens.MINUS)
            {
                Output.Output = Output.Output + " push x" + Environment.NewLine;

                if (_token.TokenName == TokenParser.Tokens.PLUS)
                {
                    SkipWhiteSpace(parser);
                    _token = parser.GetToken();
                    SkipWhiteSpace(parser);
                    Term(parser);
                    Output.Output = Output.Output + " pop y" + Environment.NewLine;
                    Output.Output = Output.Output + " addx y" + Environment.NewLine;
                } else if (_token.TokenName == TokenParser.Tokens.MINUS)
                {
                    SkipWhiteSpace(parser);
                    _token = parser.GetToken();
                    SkipWhiteSpace(parser);
                    Term(parser);
                    Output.Output = Output.Output + " pop y" + Environment.NewLine;
                    Output.Output = Output.Output + " subx y" + Environment.NewLine;
                    Output.Output = Output.Output + " neg x" + Environment.NewLine;
                }
                if (_token == null)
                    return;

            }
        }

        public void Compile(string expression, int stringCounter, int loopCounter)
        {
            _hasLabel = false;
            ExpressionParserOutput output = new ExpressionParserOutput
            {
                StringCounter = stringCounter,
                LoopCounter = loopCounter
            };

            Output = output;
            TokenParser parser = new TokenParser {InputString = expression};
            SkipWhiteSpace(parser);

            _token = parser.GetToken();
            SkipWhiteSpace(parser);
            Expression(parser);

            Output.Output = " push y" + Environment.NewLine + Output.Output + " pop y" + Environment.NewLine;

            //if (!_hasLabel)
            //{
            //    try
            //    {
            //        var native = new NativeExpressionParser();
            //        Output.Output = " push y" + Environment.NewLine + " ldx #" + native.Compile(expression) +
            //                        Environment.NewLine + " pop y" + Environment.NewLine;
            //    }
            //    catch
            //    {
            //        Output.Output = " push y" + Environment.NewLine + Output.Output + " pop y" + Environment.NewLine;
            //    }
            //}
            //else
            //{
            //    Output.Output = " push y" + Environment.NewLine + Output.Output + " pop y" + Environment.NewLine;
            //}
        }
    }
}
