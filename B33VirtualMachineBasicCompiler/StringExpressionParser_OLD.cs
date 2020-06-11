using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B33VirtualMachineBasicCompiler
{
    public class StringExpressionParser_OLD : IExpressionParser
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
                }
                retval = retval + token.TokenValue;
                if (parens == 1 && parser.Peek() != null && parser.Peek().TokenPeek != null && parser.Peek().TokenPeek.TokenName == TokenParser.Tokens.COMMA)
                    break;
                token = parser.GetToken();
            }

            return retval.TrimEnd(')');
        }

        private string GetNumericExpression(TokenParser parser)
        {
            if (parser == null || parser.Peek() == null || parser.Peek().TokenPeek == null)
                return "";

            string retval = "";
            int parens = 0;

            while (parser.Peek() != null && parser.Peek().TokenPeek != null &&
                   parser.Peek().TokenPeek.TokenName != TokenParser.Tokens.NEWLINE &&
                   parser.Peek().TokenPeek.TokenName != TokenParser.Tokens.COMMA &&
                   TokenParser.GetStatementsList().All(z => z != parser.Peek().TokenPeek.TokenName))
            {
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
        }

        private void AddFunction(Functions function)
        {
            if (!Output.Functions.Contains(function))
                Output.Functions.Add(function);
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
                AddFunction(f);
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
                AddFunction(f);
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

            while (token != null && token.TokenName != TokenParser.Tokens.NEWLINE)
            {
                SkipWhiteSpace(parser);
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

                    Output.Output = Output.Output + " ldx #st" + Output.StringCounter + Environment.NewLine;
                    Output.Output = Output.Output + "loop" + Output.LoopCounter + " lda ,x+" + Environment.NewLine;
                    Output.Output = Output.Output + " cmpa #0" + Environment.NewLine;
                    Output.Output = Output.Output + " jeq exitLoop" + Output.LoopCounter + Environment.NewLine;
                    Output.Output = Output.Output + " sta ,y+" + Environment.NewLine;
                    Output.Output = Output.Output + " jmp loop" + Output.LoopCounter + Environment.NewLine;
                    Output.Output = Output.Output + "exitLoop" + Output.LoopCounter + " sta ,y" + Environment.NewLine;
                    Output.LoopCounter++;
                    Output.StringCounter++;
                }
                if (token.TokenName == TokenParser.Tokens.PLUS)
                {
                    //token = parser.GetToken();
                    //continue;
                }
                if (token.TokenName == TokenParser.Tokens.STRINGLABEL)
                {
                    string varName = token.TokenValue.Replace("$", "").ToUpper();
                    SkipWhiteSpace(parser);

                    if (parser.Peek() != null && parser.Peek().TokenPeek != null &&
                        parser.Peek().TokenPeek.TokenName == TokenParser.Tokens.LPAREN)
                    {
                        parser.GetToken();
                        SkipWhiteSpace(parser);
                        string exp = GetNumericExpression(parser);

                        var nc = new NumericExpressionParser { Variables = Variables };
                        nc.Compile(exp, Output.StringCounter, Output.LoopCounter);

                        Output.StringCounter = nc.Output.StringCounter;
                        Output.LoopCounter = nc.Output.LoopCounter;

                        foreach (var f in nc.Output.Functions)
                        {
                            AddFunction(f);
                        }
                        string ndxCode = nc.Output.Output;

                        Output.Output = Output.Output + ndxCode;
                        Output.Output = Output.Output + " tfr x,d" + Environment.NewLine;
                        Output.Output = Output.Output + " tfr b,a" + Environment.NewLine;
                        Output.Output = Output.Output + " ldx #varStrArray" + varName + Environment.NewLine;
                        Output.Output = Output.Output + " call ArrayIndexString" + Environment.NewLine;
                        Output.Output = Output.Output + "loop" + Output.LoopCounter + " lda ,x+" + Environment.NewLine;
                        Output.Output = Output.Output + " cmpa #0" + Environment.NewLine;
                        Output.Output = Output.Output + " jeq exitLoop" + Output.LoopCounter + Environment.NewLine;
                        Output.Output = Output.Output + " sta ,y+" + Environment.NewLine;
                        Output.Output = Output.Output + " jmp loop" + Output.LoopCounter + Environment.NewLine;
                        Output.Output = Output.Output + "exitLoop" + Output.LoopCounter + " sta ,y" +
                                        Environment.NewLine;
                        Output.LoopCounter++;
                    }
                    else
                    {
                        Output.Output = Output.Output + " ldx #varStr" + varName + Environment.NewLine;
                        Output.Output = Output.Output + "loop" + Output.LoopCounter + " lda ,x+" + Environment.NewLine;
                        Output.Output = Output.Output + " cmpa #0" + Environment.NewLine;
                        Output.Output = Output.Output + " jeq exitLoop" + Output.LoopCounter + Environment.NewLine;
                        Output.Output = Output.Output + " sta ,y+" + Environment.NewLine;
                        Output.Output = Output.Output + " jmp loop" + Output.LoopCounter + Environment.NewLine;
                        Output.Output = Output.Output + "exitLoop" + Output.LoopCounter + " sta ,y" +
                                        Environment.NewLine;
                        Output.LoopCounter++;
                    }
                }

                if (token.TokenName == TokenParser.Tokens.UCASE)
                {
                    string exp = GetStringExpression(parser);

                    ExpressionParserOutput o = ParseStringExpression(exp);

                    AddFunction(Functions.FuncUcase);

                    Output.Output = Output.Output + " push y" + Environment.NewLine;
                    Output.Output = Output.Output + o.Output;
                    Output.Output = Output.Output + " pop y" + Environment.NewLine;
                    Output.Output = Output.Output + " call funcUCASE" + Environment.NewLine;
                }
                if (token.TokenName == TokenParser.Tokens.LCASE)
                {
                    string exp = GetStringExpression(parser);

                    ExpressionParserOutput o = ParseStringExpression(exp);

                    AddFunction(Functions.FuncLcase);

                    Output.Output = Output.Output + " push y" + Environment.NewLine;
                    Output.Output = Output.Output + o.Output;
                    Output.Output = Output.Output + " pop y" + Environment.NewLine;
                    Output.Output = Output.Output + " call funcLCASE" + Environment.NewLine;
                }
                if (token.TokenName == TokenParser.Tokens.INPUT)
                {
                    SkipWhiteSpace(parser);
                    // Should be a left parenthesis
                    parser.GetToken();
                    // Should be a right parenthesis
                    parser.GetToken();

                    Output.Output = Output.Output + " call funcINPUT" + Environment.NewLine;

                    Output.Output = Output.Output + " ldx #InputBuffer" + Environment.NewLine;
                    Output.Output = Output.Output + "loop" + Output.LoopCounter + " lda ,x+" + Environment.NewLine;
                    Output.Output = Output.Output + " cmpa #0" + Environment.NewLine;
                    Output.Output = Output.Output + " jeq exitLoop" + Output.LoopCounter + Environment.NewLine;
                    Output.Output = Output.Output + " sta ,y+" + Environment.NewLine;
                    Output.Output = Output.Output + " jmp loop" + Output.LoopCounter + Environment.NewLine;
                    Output.Output = Output.Output + "exitLoop" + Output.LoopCounter + " sta ,y" + Environment.NewLine;
                    Output.LoopCounter++;

                    AddFunction(Functions.FuncInput);
                    AddFunction(Functions.FuncPrint);
                }
                if (token.TokenName == TokenParser.Tokens.CHR)
                {
                    SkipWhiteSpace(parser);
                    // Should be a left parenthesis
                    parser.GetToken();
                    string exp = GetNumericExpression(parser);
                    ExpressionParserOutput o = ParseNumericExpression(exp);
                    SkipWhiteSpace(parser);

                    // Should be a right parenthesis
                    parser.GetToken();
                    SkipWhiteSpace(parser);

                    Output.Output = Output.Output + o.Output;
                    Output.Output = Output.Output + " tfr x,d" + Environment.NewLine;
                    Output.Output = Output.Output + " stb ,y+" + Environment.NewLine;
                    Output.Output = Output.Output + " sta ,y" + Environment.NewLine;
                }
                if (token.TokenName == TokenParser.Tokens.MID)
                {
                    string exp = GetStringExpression(parser);

                    ExpressionParserOutput o = ParseStringExpression(exp);
                    SkipWhiteSpace(parser);

                    // Should be a comma
                    parser.GetToken();
                    SkipWhiteSpace(parser);

                    Output.Output = Output.Output + " push y" + Environment.NewLine;
                    Output.Output = Output.Output + o.Output;
                    Output.Output = Output.Output + " pop y" + Environment.NewLine;

                    exp = GetNumericExpression(parser);
                    o = ParseNumericExpression(exp);
                    SkipWhiteSpace(parser);

                    // Should be a comma
                    parser.GetToken();
                    SkipWhiteSpace(parser);

                    Output.Output = Output.Output + "" + Environment.NewLine;
                    Output.Output = Output.Output + o.Output;
                    Output.Output = Output.Output + " tfr x,d" + Environment.NewLine;
                    Output.Output = Output.Output + " push d" + Environment.NewLine;

                    exp = GetNumericExpression(parser);
                    o = ParseNumericExpression(exp);
                    SkipWhiteSpace(parser);

                    Output.Output = Output.Output + o.Output;
                    Output.Output = Output.Output + " pop d" + Environment.NewLine;

                    Output.Output = Output.Output + " call funcMID" + Environment.NewLine;

                    AddFunction(Functions.FuncMid);
                }
                token = parser.GetToken();
            }
        }
    }
}
