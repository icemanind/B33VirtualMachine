using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B33VirtualMachineBasicCompiler
{
    internal class StringExpressionParser : IExpressionParser
    {
        public ExpressionParserOutput Output { get; set; }
        public Dictionary<string, string> Variables { get; set; }

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

        private string GetStringExpression(TokenParser parser)
        {
            SkipWhiteSpace(parser);
            // Should be a '('
            Token token = parser.GetToken();
            int parens = 1;
            string retval = "";

            token = parser.GetToken();
            while (token != null && parens != 0)
            {
                if (token.TokenName == TokenParser.Tokens.LPAREN)
                {
                    parens++;
                }
                if (token.TokenName == TokenParser.Tokens.RPAREN)
                {
                    parens--;

                    if (parens == 0)
                        break;
                }
                retval = retval + token.TokenValue;
                if (parens == 1 && parser.Peek() != null && parser.Peek().TokenPeek != null && parser.Peek().TokenPeek.TokenName == TokenParser.Tokens.COMMA)
                    break;
                token = parser.GetToken();
            }

            return ParenthesisBalanced(retval) ? retval : retval.TrimEnd(')');
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

            //string retval = "";
            //int parens = 0;

            //if (parser.Peek() == null)
            //    return "";
            //Token ptToken = parser.Peek().TokenPeek;

            //while (ptToken != null && ptToken.TokenName != TokenParser.Tokens.NEWLINE)
            //{
            //    if (ptToken.TokenName == TokenParser.Tokens.COMMA && parens == 0)
            //        break;
            //    if (ptToken.TokenName == TokenParser.Tokens.LPAREN)
            //    {
            //        parens++;
            //    }
            //    if (ptToken.TokenName == TokenParser.Tokens.RPAREN)
            //    {
            //        parens--;
            //        //if (parens == 0)
            //        //{
            //        //    retval = retval + ptToken.TokenValue;
            //        //    parser.GetToken();

            //        //    break;
            //        //}
            //        if (parens < 0)
            //        {
            //            break;
            //        }
            //    }
            //    Token token = parser.GetToken();

            //    retval = retval + token.TokenValue;

            //    if (parser.Peek() == null)
            //        break;
            //    ptToken = parser.Peek().TokenPeek;
//        }

            return retval;
        }

        /*private string GetNumericExpression(TokenParser parser)
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
                if (parser.Peek().TokenPeek.TokenName == TokenParser.Tokens.RPAREN && parens == 0)
                    break;
                
                Token token = parser.GetToken();
                if (token.TokenName == TokenParser.Tokens.LPAREN)
                    parens++;
                if (token.TokenName == TokenParser.Tokens.RPAREN)
                    parens--;

                retval = retval + token.TokenValue;
            }

            return retval;
        }*/

        private bool ParenthesisBalanced(string exp)
        {
            TokenParser parser = new TokenParser {InputString = exp};
            int paren = 0;

            Token token = parser.GetToken();

            while (token != null)
            {
                if (token.TokenName == TokenParser.Tokens.LPAREN)
                {
                    paren++;
                }
                if (token.TokenName == TokenParser.Tokens.RPAREN)
                {
                    paren--;
                }
                token = parser.GetToken();
            }

            return paren == 0;
        }

        private ExpressionParserOutput ParseStringExpression(string expression)
        {
            var sc = new StringExpressionParser { Variables = Variables };

            sc.Compile(expression, Output.StringCounter, Output.LoopCounter);

            Output.StringCounter = sc.Output.StringCounter;
            Output.LoopCounter = sc.Output.LoopCounter;
            Output.InitCode = Output.InitCode + sc.Output.InitCode;

            foreach (var f in sc.Output.Functions)
            {
                if (!Output.Functions.Contains(f))
                    Output.Functions.Add(f);
            }

            foreach (KeyValuePair<string, string> kvp in sc.Variables)
            {
                if (!Variables.ContainsKey(kvp.Key))
                {
                    Variables.Add(kvp.Key, kvp.Value);
                }
            }

            return sc.Output;
        }

        private ExpressionParserOutput ParseNumericExpression(string expression)
        {
            var nc = new NumericExpressionParser { Variables = Variables };

            nc.Compile(expression, Output.StringCounter, Output.LoopCounter);

            Output.StringCounter = nc.Output.StringCounter;
            Output.LoopCounter = nc.Output.LoopCounter;
            Output.InitCode = Output.InitCode + nc.Output.InitCode;

            foreach (var f in nc.Output.Functions)
            {
                if (!Output.Functions.Contains(f))
                    Output.Functions.Add(f);
            }

            foreach (KeyValuePair<string, string> kvp in nc.Variables)
            {
                if (!Variables.ContainsKey(kvp.Key))
                {
                    Variables.Add(kvp.Key, kvp.Value);
                }
            }

            return nc.Output;
        }

        public void Compile(string expression, int stringCounter, int loopCounter)
        {
            var parser = new TokenParser();

            ExpressionParserOutput output = new ExpressionParserOutput
            {
                StringCounter = stringCounter,
                LoopCounter = loopCounter
            };

            Output = output;

            parser.InputString = expression;

            SkipWhiteSpace(parser);
            Token token = parser.GetToken();

            Output.Output = Output.Output + " push y" + Environment.NewLine;
            Output.Output = Output.Output + " call allocatememory" + Environment.NewLine;
            Output.Output = Output.Output + " push x" + Environment.NewLine;
            Output.Output = Output.Output + " tfr x,y" + Environment.NewLine;

            while (token != null && token.TokenName != TokenParser.Tokens.NEWLINE)
            {
                SkipWhiteSpace(parser);

                if (token.TokenName == TokenParser.Tokens.PLUS)
                {
                    token = parser.GetToken();
                    continue;
                }

                if (token.TokenName == TokenParser.Tokens.STRING)
                {
                    if (token.TokenValue == "\"\"")
                    {
                        Output.InitCode = Output.InitCode + "st" + Output.StringCounter + " chr 0" + Environment.NewLine;
                    }
                    else
                    {
                        Output.InitCode = Output.InitCode + "st" + Output.StringCounter + " str " + token.TokenValue +
                                          Environment.NewLine;
                        Output.InitCode = Output.InitCode + " chr 0" + Environment.NewLine;
                    }

                    Output.Output = Output.Output + " ldx #st"+ Output.StringCounter + Environment.NewLine;
                    Output.Output = Output.Output + " call copystring" + Environment.NewLine;

                    Output.StringCounter++;

                    SkipWhiteSpace(parser);
                    token = parser.GetToken();
                    SkipWhiteSpace(parser);
                    continue;
                }

                if (token.TokenName == TokenParser.Tokens.STRINGLABEL)
                {
                    SkipWhiteSpace(parser);
                    string varName = token.TokenValue.ToUpper().Replace("$", "");
                    
                    if (parser.Peek() != null && parser.Peek().TokenPeek.TokenName == TokenParser.Tokens.LPAREN)
                    {
                        parser.GetToken();
                        string exp = GetNumericExpression(parser, true);

                        Output.Output = Output.Output + " push y" + Environment.NewLine;
                        Output.Output = Output.Output + ParseNumericExpression(exp).Output;
                        Output.Output = Output.Output + " tfr x,d" + Environment.NewLine;
                        Output.Output = Output.Output + " tfr b,a" + Environment.NewLine;
                        Output.Output = Output.Output + " ldx #varStrArray" + varName + Environment.NewLine;
                        Output.Output = Output.Output + " call ArrayIndexString" + Environment.NewLine;
                        Output.Output = Output.Output + " pop y" + Environment.NewLine;
                        Output.Output = Output.Output + " call copystring" + Environment.NewLine;

                        SkipWhiteSpace(parser);
                        parser.GetToken();
                        SkipWhiteSpace(parser);
                        token = parser.GetToken();
                        SkipWhiteSpace(parser);
                        continue;
                    }
                    
                    
                    Output.Output = Output.Output + " ldx #varStr" + varName + Environment.NewLine;
                    Output.Output = Output.Output + " call copystring" + Environment.NewLine;

                    if (!Variables.ContainsKey("varStr" + varName))
                    {
                        Variables.Add("varStr" + varName, "varStr" + varName + " rmb 250" + Environment.NewLine);
                    }

                    SkipWhiteSpace(parser);
                    token = parser.GetToken();
                    SkipWhiteSpace(parser);
                    continue;
                }

                if (token.TokenName == TokenParser.Tokens.LTRIM || token.TokenName == TokenParser.Tokens.RTRIM)
                {
                    string exp = GetStringExpression(parser);

                    ExpressionParserOutput o = ParseStringExpression(exp);
                    SkipWhiteSpace(parser);

                    Output.Output = Output.Output + o.Output;
                    Output.Output = token.TokenName == TokenParser.Tokens.LTRIM
                        ? Output.Output + " call funcLTRIM" + Environment.NewLine
                        : Output.Output + " call funcRTRIM" + Environment.NewLine;
                    Output.Output = Output.Output + " call copystring" + Environment.NewLine;
                    Output.Output = Output.Output + " call freememory" + Environment.NewLine;

                    if (token.TokenName == TokenParser.Tokens.LTRIM)
                    {
                        if (!Output.Functions.Contains(Functions.FuncLtrim))
                            Output.Functions.Add(Functions.FuncLtrim);
                    }
                    else
                    {
                        if (!Output.Functions.Contains(Functions.FuncRtrim))
                            Output.Functions.Add(Functions.FuncRtrim);
                    }

                    SkipWhiteSpace(parser);
                    // Should be a right parenthesis
                    parser.GetToken();
                    SkipWhiteSpace(parser);
                    token = parser.GetToken();
                    SkipWhiteSpace(parser);
                    continue;
                }

                if (token.TokenName == TokenParser.Tokens.UCASE)
                {
                    string exp = GetStringExpression(parser);

                    ExpressionParserOutput o = ParseStringExpression(exp);
                    SkipWhiteSpace(parser);

                    Output.Output = Output.Output + o.Output;
                    Output.Output = Output.Output + " call funcUCASE" + Environment.NewLine;
                    Output.Output = Output.Output + " call copystring"+Environment.NewLine;
                    Output.Output = Output.Output + " call freememory" + Environment.NewLine;

                    if (!Output.Functions.Contains(Functions.FuncUcase))
                        Output.Functions.Add(Functions.FuncUcase);

                    SkipWhiteSpace(parser);
                    // Should be a right parenthesis
                    parser.GetToken();
                    SkipWhiteSpace(parser);
                    token = parser.GetToken();
                    SkipWhiteSpace(parser);
                    continue;
                }

                if (token.TokenName == TokenParser.Tokens.LCASE)
                {
                    string exp = GetStringExpression(parser);

                    ExpressionParserOutput o = ParseStringExpression(exp);
                    SkipWhiteSpace(parser);

                    Output.Output = Output.Output + o.Output;
                    Output.Output = Output.Output + " call funcLCASE" + Environment.NewLine;
                    Output.Output = Output.Output + " call copystring" + Environment.NewLine;
                    Output.Output = Output.Output + " call freememory" + Environment.NewLine;

                    if (!Output.Functions.Contains(Functions.FuncLcase))
                        Output.Functions.Add(Functions.FuncLcase);

                    SkipWhiteSpace(parser);
                    // Should be a right parenthesis
                    parser.GetToken();
                    SkipWhiteSpace(parser);
                    token = parser.GetToken();
                    SkipWhiteSpace(parser);
                    continue;
                }

                if (token.TokenName == TokenParser.Tokens.STR)
                {
                    string exp = GetNumericExpression(parser, true);

                    ExpressionParserOutput expOutput = ParseNumericExpression(exp);

                    Output.Output = Output.Output + expOutput.Output;
                    Output.Output = Output.Output + " call funcSTR" + Environment.NewLine;
                    Output.Output = Output.Output + " call copystring" + Environment.NewLine;
                    Output.Output = Output.Output + " call freememory" + Environment.NewLine;

                    if (!Output.Functions.Contains(Functions.FuncStr))
                        Output.Functions.Add(Functions.FuncStr);

                    SkipWhiteSpace(parser);
                    token = parser.GetToken();
                    SkipWhiteSpace(parser);
                    continue;
                }

                if (token.TokenName == TokenParser.Tokens.CHR)
                {
                    string exp = GetNumericExpression(parser, true);

                    ExpressionParserOutput expOutput = ParseNumericExpression(exp);

                    Output.Output = Output.Output + expOutput.Output;
                    Output.Output = Output.Output + " tfr x,d" + Environment.NewLine;
                    Output.Output = Output.Output + " stb ,y+" + Environment.NewLine;
                    Output.Output = Output.Output + " sta ,y" + Environment.NewLine;

                    SkipWhiteSpace(parser);
                    token = parser.GetToken();
                    SkipWhiteSpace(parser);
                    continue;
                }

                if (token.TokenName == TokenParser.Tokens.INPUT)
                {
                    SkipWhiteSpace(parser);
                    // should be '('
                    parser.GetToken();

                    if (!Output.Functions.Contains(Functions.FuncInput)) 
                        Output.Functions.Add(Functions.FuncInput);
                    if (!Output.Functions.Contains(Functions.FuncPrint))
                        Output.Functions.Add(Functions.FuncPrint);

                    SkipWhiteSpace(parser);
                    // should be ')'
                    parser.GetToken();
                    SkipWhiteSpace(parser);

                    Output.Output = Output.Output + " call funcINPUT" + Environment.NewLine;
                    Output.Output = Output.Output + " call copystring" + Environment.NewLine;
                    Output.Output = Output.Output + " call freememory" + Environment.NewLine;

                    token = parser.GetToken();
                    SkipWhiteSpace(parser);
                    continue;
                }

                if (token.TokenName == TokenParser.Tokens.MID)
                {
                    string exp = GetStringExpression(parser);

                    ExpressionParserOutput o = ParseStringExpression(exp);
                    SkipWhiteSpace(parser);
                    var midVar = o.Output;

                    // Should be a comma
                    parser.GetToken();
                    SkipWhiteSpace(parser);

                    exp = GetNumericExpression(parser, false);
                    o = ParseNumericExpression(exp);
                    var midStart = o.Output;
                    SkipWhiteSpace(parser);

                    // Should be a comma
                    parser.GetToken();
                    SkipWhiteSpace(parser);

                    exp = GetNumericExpression(parser, false);
                    o = ParseNumericExpression(exp);
                    var midLength = o.Output;
                    SkipWhiteSpace(parser);

                    string midcode = "";

                    midcode = midcode + midVar;
                    midcode = midcode + " push x" + Environment.NewLine;
                    midcode = midcode + midLength;
                    midcode = midcode + " push x" + Environment.NewLine;
                    midcode = midcode + midStart;
                    midcode = midcode + " call funcMID" + Environment.NewLine;
                    midcode = midcode + " call copystring" + Environment.NewLine;
                    midcode = midcode + " call freememory" + Environment.NewLine;

                    Output.Output = Output.Output + midcode;

                    if (!Output.Functions.Contains(Functions.FuncMid))
                        Output.Functions.Add(Functions.FuncMid);

                    SkipWhiteSpace(parser);
                    // Should be a right parenthesis
                    parser.GetToken();
                    SkipWhiteSpace(parser);
                    token = parser.GetToken();
                    SkipWhiteSpace(parser);
                    continue;
                }
            }

            Output.Output = Output.Output + " pop x" + Environment.NewLine;
            Output.Output = Output.Output + " pop y" + Environment.NewLine;
        }

        //public void Compile(string expression, int stringCounter, int loopCounter)
        //{
        //    var parser = new TokenParser();

        //    ExpressionParserOutput output = new ExpressionParserOutput
        //    {
        //        StringCounter = stringCounter,
        //        LoopCounter = loopCounter
        //    };

        //    Output = output;

        //    parser.InputString = expression;

        //    SkipWhiteSpace(parser);
        //    Token token = parser.GetToken();

        //    Output.Output = Output.Output + " push y" + Environment.NewLine;
        //    Output.Output = Output.Output + " call allocatememory" + Environment.NewLine;
        //    Output.Output = Output.Output + " push x" + Environment.NewLine;
        //    Output.Output = Output.Output + " tfr x,y" + Environment.NewLine;
        //    while (token != null && token.TokenName != TokenParser.Tokens.NEWLINE)
        //    {
        //        if (token.TokenName == TokenParser.Tokens.PLUS)
        //        {
        //            SkipWhiteSpace(parser);
        //            token = parser.GetToken();
        //            SkipWhiteSpace(parser);
        //            continue;
        //        }

        //        if (token.TokenName == TokenParser.Tokens.MID)
        //        {
        //            string exp = GetStringExpression(parser);

        //            ExpressionParserOutput o = ParseStringExpression(exp);
        //            SkipWhiteSpace(parser);
        //            var midVar = o.Output;
                   
        //            // Should be a comma
        //            parser.GetToken();
        //            SkipWhiteSpace(parser);

        //            exp = GetNumericExpression(parser);
        //            o = ParseNumericExpression(exp);
        //            var midStart = o.Output;
        //            SkipWhiteSpace(parser);

        //            // Should be a comma
        //            parser.GetToken();
        //            SkipWhiteSpace(parser);

        //            exp = GetNumericExpression(parser);
        //            o = ParseNumericExpression(exp);
        //            var midLength = o.Output;
        //            SkipWhiteSpace(parser);

        //            string midcode = "";


        //            midcode = midcode + midLength;
        //            midcode = midcode + " push x" + Environment.NewLine;
        //            midcode = midcode + midStart;
        //            midcode = midcode + " push x" + Environment.NewLine;
        //            midcode = midcode + midVar;
        //            midcode = midcode + " call funcMID" + Environment.NewLine;
        //            midcode = midcode + " push x" + Environment.NewLine;
        //            midcode = midcode + "loop" + output.LoopCounter + " lda ,x+" + Environment.NewLine;
        //            midcode = midcode + " cmpa #0" + Environment.NewLine;
        //            midcode = midcode + " jeq exitLoop" + Output.LoopCounter + Environment.NewLine;
        //            midcode = midcode + " sta ,y+" + Environment.NewLine;
        //            midcode = midcode + " jmp loop" + Output.LoopCounter + Environment.NewLine;
        //            midcode = midcode + "exitLoop" + Output.LoopCounter + " sta ,y" + Environment.NewLine;
        //            midcode = midcode + " pop x" + Environment.NewLine;
        //            midcode = midcode + " call freememory" + Environment.NewLine;

        //            if (!Output.Functions.Contains(Functions.FuncMid))
        //                Output.Functions.Add(Functions.FuncMid);

        //            Output.Output = Output.Output + midcode;

        //            Output.LoopCounter++;

        //            SkipWhiteSpace(parser);
        //            // Should be right parenthesis
        //            token = parser.GetToken();
        //            SkipWhiteSpace(parser);

        //            SkipWhiteSpace(parser);
        //            token = parser.GetToken();
        //            SkipWhiteSpace(parser);
        //            continue;
        //        }

        //        if (token.TokenName == TokenParser.Tokens.STRINGLABEL)
        //        {
        //            string varName = token.TokenValue.ToUpper().Replace("$", "");
        //            SkipWhiteSpace(parser);

        //            Output.Output = Output.Output + " push y" + Environment.NewLine;
        //            Output.Output = Output.Output + " push d" + Environment.NewLine;
        //            Output.Output = Output.Output + " call allocatememory" + Environment.NewLine;
        //            Output.Output = Output.Output + " push x" + Environment.NewLine;
        //            Output.Output = Output.Output + " ldy #varStr" + varName + Environment.NewLine;
        //            Output.Output = Output.Output + "loop" + Output.LoopCounter + " lda ,y+" + Environment.NewLine;
        //            Output.Output = Output.Output + " cmpa #0" + Environment.NewLine;
        //            Output.Output = Output.Output + " jeq exitLoop" + Output.LoopCounter + Environment.NewLine;
        //            Output.Output = Output.Output + " sta ,x+" + Environment.NewLine;
        //            Output.Output = Output.Output + " jmp loop" + Output.LoopCounter + Environment.NewLine;
        //            Output.Output = Output.Output + "exitLoop" + Output.LoopCounter + " sta ,x" + Environment.NewLine;
        //            Output.Output = Output.Output + " pop x" + Environment.NewLine;
        //            Output.Output = Output.Output + " pop d" + Environment.NewLine;
        //            Output.Output = Output.Output + " pop y" + Environment.NewLine;

        //            Output.LoopCounter++;

        //            if (!Variables.ContainsKey("varStr" + varName))
        //            {
        //                Variables.Add("varStr" + varName, "varStr" + varName + " rmb 250" + Environment.NewLine);
        //            }

        //            SkipWhiteSpace(parser);
        //            token = parser.GetToken();
        //            SkipWhiteSpace(parser);
        //            continue;
        //        }

        //        if (token.TokenName == TokenParser.Tokens.STRING)
        //        {
        //            SkipWhiteSpace(parser);

        //            if (token.TokenValue == "\"\"")
        //            {
        //                Output.InitCode = Output.InitCode + "st" + Output.StringCounter + " chr 0" + Environment.NewLine;
        //            }
        //            else
        //            {
        //                Output.InitCode = Output.InitCode + "st" + Output.StringCounter + " str " + token.TokenValue +
        //                                  Environment.NewLine;
        //                Output.InitCode = Output.InitCode + " chr 0" + Environment.NewLine;
        //            }

        //            Output.Output = Output.Output + " push y" + Environment.NewLine;
        //            Output.Output = Output.Output + " push d" + Environment.NewLine;
        //            Output.Output = Output.Output + " call allocatememory" + Environment.NewLine;
        //            Output.Output = Output.Output + " push x" + Environment.NewLine;
        //            Output.Output = Output.Output + " ldy #st" + Output.StringCounter + Environment.NewLine;
        //            Output.Output = Output.Output + "loop" + Output.LoopCounter + " lda ,y+" + Environment.NewLine;
        //            Output.Output = Output.Output + " cmpa #0" + Environment.NewLine;
        //            Output.Output = Output.Output + " jeq exitLoop" + Output.LoopCounter + Environment.NewLine;
        //            Output.Output = Output.Output + " sta ,x+" + Environment.NewLine;
        //            Output.Output = Output.Output + " jmp loop" + Output.LoopCounter + Environment.NewLine;
        //            Output.Output = Output.Output + "exitLoop" + Output.LoopCounter + " sta ,x" + Environment.NewLine;
        //            Output.Output = Output.Output + " pop x" + Environment.NewLine;
        //            Output.Output = Output.Output + " pop d" + Environment.NewLine;
        //            Output.Output = Output.Output + " pop y" + Environment.NewLine;

        //            Output.StringCounter++;
        //            Output.LoopCounter++;

        //            SkipWhiteSpace(parser);
        //            token = parser.GetToken();
        //            SkipWhiteSpace(parser);
        //            continue;
        //        }

        //        SkipWhiteSpace(parser);
        //        token = parser.GetToken();
        //        SkipWhiteSpace(parser);

        //        Output.Output = Output.Output + " push x" + Environment.NewLine;
        //        Output.Output = Output.Output + "loop" + Output.LoopCounter + " lda ,x+" + Environment.NewLine;
        //        Output.Output = Output.Output + " cmpa #0" + Environment.NewLine;
        //        Output.Output = Output.Output + " jeq exitLoop" + Output.LoopCounter + Environment.NewLine;
        //        Output.Output = Output.Output + " sta ,y+" + Environment.NewLine;
        //        Output.Output = Output.Output + " jmp loop" + Output.LoopCounter + Environment.NewLine;
        //        Output.Output = Output.Output + "exitLoop" + Output.LoopCounter + " sta ,y" + Environment.NewLine;
        //        Output.Output = Output.Output + " pop x" + Environment.NewLine;
        //        Output.Output = Output.Output + " call freememory" + Environment.NewLine;

        //        output.LoopCounter++;
        //    }

        //    Output.Output = Output.Output + " pop x" + Environment.NewLine;
        //    Output.Output = Output.Output + " pop y" + Environment.NewLine;
        //}
    }
}
