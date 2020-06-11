using System;
using System.Collections.Generic;
using System.Linq;

namespace B33VirtualMachineBasicCompiler
{
    internal class BooleanExpressionParser : IExpressionParser
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

        public void Compile(string expression, int stringCounter, int loopCounter)
        {
            Compile(expression, stringCounter, loopCounter, 1);
        }

        public void Compile(string expression, int stringCounter, int loopCounter, int boolCounter)
        {
            ExpressionParserOutput output = new ExpressionParserOutput
            {
                StringCounter = stringCounter,
                LoopCounter = loopCounter,
                BoolCounter = boolCounter
            };

            var chains = new List<BoolExpressionChain>();
            int id = 1;
            Output = output;
            TokenParser parser = new TokenParser { InputString = expression };
            TokenParser.Tokens connOp;

            do
            {
                string leftExpression = "";
                string rightExpression = "";
                Token token;
                while (parser.Peek() != null && parser.Peek().TokenPeek != null &&
                       parser.Peek().TokenPeek.TokenName != TokenParser.Tokens.LESSTHAN &&
                       parser.Peek().TokenPeek.TokenName != TokenParser.Tokens.LESSTHANOREQUAL &&
                       parser.Peek().TokenPeek.TokenName != TokenParser.Tokens.GREATERTHAN &&
                       parser.Peek().TokenPeek.TokenName != TokenParser.Tokens.GREATERTHENOREQUAL &&
                       parser.Peek().TokenPeek.TokenName != TokenParser.Tokens.NOTEQUAL &&
                       parser.Peek().TokenPeek.TokenName != TokenParser.Tokens.EQUAL)
                {
                    token = parser.GetToken();
                    leftExpression = leftExpression + token.TokenValue;
                }
                leftExpression = leftExpression.Trim();

                token = parser.GetToken();
                var equalityToken = token.TokenName;

                while (parser.Peek() != null && parser.Peek().TokenPeek != null &&
                       parser.Peek().TokenPeek.TokenName != TokenParser.Tokens.OR &&
                       parser.Peek().TokenPeek.TokenName != TokenParser.Tokens.AND &&
                       parser.Peek().TokenPeek.TokenName != TokenParser.Tokens.LESSTHAN &&
                       parser.Peek().TokenPeek.TokenName != TokenParser.Tokens.LESSTHANOREQUAL &&
                       parser.Peek().TokenPeek.TokenName != TokenParser.Tokens.GREATERTHAN &&
                       parser.Peek().TokenPeek.TokenName != TokenParser.Tokens.GREATERTHENOREQUAL &&
                       parser.Peek().TokenPeek.TokenName != TokenParser.Tokens.NOTEQUAL &&
                       parser.Peek().TokenPeek.TokenName != TokenParser.Tokens.EQUAL)
                {
                    token = parser.GetToken();
                    rightExpression = rightExpression + token.TokenValue;
                }
                rightExpression = rightExpression.Trim();

                IExpressionParser eParser;
                string pre = "";
                if (!IsString(leftExpression,rightExpression))
                {
                    eParser = new NumericExpressionParser();
                }
                else
                {
                    eParser = new StringExpressionParser();
                }

                eParser.Variables = Variables;
                eParser.Compile(leftExpression, Output.StringCounter, Output.LoopCounter);

                Output.StringCounter = eParser.Output.StringCounter;
                Output.LoopCounter = eParser.Output.LoopCounter;

                Output.InitCode = Output.InitCode + eParser.Output.InitCode;

                foreach (var f in eParser.Output.Functions)
                {
                    AddFunction(f);
                }

                foreach (KeyValuePair<string, string> kvp in eParser.Variables)
                {
                    if (!Variables.ContainsKey(kvp.Key))
                    {
                        Variables.Add(kvp.Key, kvp.Value);
                    }
                }

                string leftOutput = pre + eParser.Output.Output;

                pre = "";

                if (!IsString(leftExpression, rightExpression))
                {
                    eParser = new NumericExpressionParser();
                }
                else
                {
                    eParser = new StringExpressionParser();
                }

                eParser.Variables = Variables;
                eParser.Compile(rightExpression, Output.StringCounter, Output.LoopCounter);

                Output.StringCounter = eParser.Output.StringCounter;
                Output.LoopCounter = eParser.Output.LoopCounter;

                Output.InitCode = Output.InitCode + eParser.Output.InitCode;

                foreach (var f in eParser.Output.Functions)
                {
                    AddFunction(f);
                }

                foreach (KeyValuePair<string, string> kvp in eParser.Variables)
                {
                    if (!Variables.ContainsKey(kvp.Key))
                    {
                        Variables.Add(kvp.Key, kvp.Value);
                    }
                }

                string rightOutput = pre + eParser.Output.Output;

                SkipWhiteSpace(parser);

                connOp = TokenParser.Tokens.UNDEFINED;

                if (parser.Peek() != null && parser.Peek().TokenPeek != null &&
                    parser.Peek().TokenPeek.TokenName == TokenParser.Tokens.AND)
                {
                    connOp = TokenParser.Tokens.AND;
                }
                if (parser.Peek() != null && parser.Peek().TokenPeek != null &&
                    parser.Peek().TokenPeek.TokenName == TokenParser.Tokens.OR)
                {
                    connOp = TokenParser.Tokens.OR;
                }

                chains.Add(new BoolExpressionChain
                {
                    Id = id,
                    ConnectionOperator = connOp,
                    Expression = new BoolExpression
                    {
                        EquityOperator = equalityToken,
                        LeftSideCode = leftOutput,
                        RightSideCode = rightOutput,
                        LeftSideExpression = leftExpression,
                        RightSideExpression = rightExpression,
                        IsString = IsString(leftExpression, rightExpression)
                    }
                });
                id++;

                if (connOp != TokenParser.Tokens.UNDEFINED)
                    parser.GetToken();
            } while (connOp != TokenParser.Tokens.UNDEFINED);

            //if (!leftExpression.Contains('$') && !rightExpression.Contains('$'))
            //{
            //    Output.Output = DoCompare(leftOutput, rightOutput, equalityToken);
            //}
            string output2 = "";
            foreach (BoolExpressionChain chain in chains.OrderBy(z => z.Id))
            {
                if (chain.ConnectionOperator == TokenParser.Tokens.AND || chain.ConnectionOperator == TokenParser.Tokens.UNDEFINED)
                {
                    output2 = output2 + DoCompare(chain.Expression);
                    output2 = output2 + " cmpx #0" + Environment.NewLine;
                    output2 = output2 + " jeq boolexit" + Output.BoolCounter + Environment.NewLine;
                } else if (chain.ConnectionOperator == TokenParser.Tokens.OR)
                {
                    output2 = output2 + DoCompare(chain.Expression);
                    output2 = output2 + " cmpx #1" + Environment.NewLine;
                    output2 = output2 + " jeq boolexitor" + Output.BoolCounter + Environment.NewLine;
                }
            }
            output2 = output2 + "boolexitor" + Output.BoolCounter + " ldx #1" + Environment.NewLine;
            output2 = output2 + " jmp booltrue" + Output.BoolCounter + Environment.NewLine;
            output2 = output2 + "boolexit" + output.BoolCounter + " ldx #0" + Environment.NewLine;
            output2 = output2 + "booltrue" + output.BoolCounter + " nop" + Environment.NewLine;

            Output.Output = output2;
            Output.BoolCounter++;
        }

        private string DoCompare(BoolExpression exp)
        {
            return DoCompare(exp.LeftSideCode, exp.RightSideCode, exp.EquityOperator, exp.IsString);
        }

        private string DoCompare(string leftOutput, string rightOutput, TokenParser.Tokens equalityToken, bool isString)
        {
            string output = "";

            output = output + leftOutput + Environment.NewLine;
            if (isString)
            {
                //output = output + " ldy #compbuf1" + Environment.NewLine;
                //output = output + " call tmpstrcopy" + Environment.NewLine;
            }
            output = output + " tfr x,y" + Environment.NewLine;
            output = output + rightOutput + Environment.NewLine;
            if (isString)
            {
                //output = output + " ldy #compbuf2" + Environment.NewLine;
                //output = output + " call tmpstrcopy" + Environment.NewLine;
            }
            if (isString)
            {
                //output = output + " ldx #compbuf1" + Environment.NewLine;
                //output = output + " ldy #compbuf2" + Environment.NewLine;
                output = output + " push x" + Environment.NewLine;
                output = output + " push y" + Environment.NewLine;
                output = output + " call bufcomp" + Environment.NewLine;
                output = output + " tfr x,d" + Environment.NewLine;
                output = output + " pop x" + Environment.NewLine;
                output = output + " call freememory" + Environment.NewLine;
                output = output + " pop x" + Environment.NewLine;
                output = output + " call freememory" + Environment.NewLine;
                output = output + " tfr d,x" + Environment.NewLine;
                output = output + " cmpx #1" + Environment.NewLine;
            }
            else
            {
                output = output + " cmpy x" + Environment.NewLine;
            }

            string opcode = "";
            switch (equalityToken)
            {
                case TokenParser.Tokens.EQUAL:
                    opcode = "jeq";
                    break;
                case TokenParser.Tokens.NOTEQUAL:
                    opcode = "jne";
                    break;
                case TokenParser.Tokens.LESSTHANOREQUAL:
                    opcode = "jle";
                    break;
                case TokenParser.Tokens.LESSTHAN:
                    opcode = "jlt";
                    break;
                case TokenParser.Tokens.GREATERTHENOREQUAL:
                    opcode = "jge";
                    break;
                case TokenParser.Tokens.GREATERTHAN:
                    opcode = "jgt";
                    break;
            }

            output = output + " " + opcode + " ifstate" + Output.LoopCounter + Environment.NewLine;
            output = output + " ldx #0" + Environment.NewLine;
            output = output + " jmp ifxit" + Output.LoopCounter + Environment.NewLine;
            output = output + "ifstate" + Output.LoopCounter + " ldx #1" + Environment.NewLine;
            output = output + "ifxit" + Output.LoopCounter + " nop" + Environment.NewLine;

            Output.LoopCounter++;

            return output;
        }

        private void AddFunction(Functions function)
        {
            if (!Output.Functions.Contains(function))
                Output.Functions.Add(function);
        }

        internal static bool IsString(string expression)
        {
            if (expression.Contains('$') || expression.Contains('"'))
            {
                var ndx = expression.ToUpper().IndexOf("LEN", 0, StringComparison.Ordinal);

                if (ndx >= 0)
                {
                    ndx += 3;
                    while (expression[ndx] == ' ')
                    {
                        ndx++;
                    }
                    if (expression[ndx] == '(')
                    {
                        return false;
                    }
                }

                ndx = expression.ToUpper().IndexOf("ASC", 0, StringComparison.Ordinal);

                if (ndx >= 0)
                {
                    ndx += 3;
                    while (expression[ndx] == ' ')
                    {
                        ndx++;
                    }
                    if (expression[ndx] == '(')
                    {
                        return false;
                    }
                }
            }

            return !(!expression.Contains('$') && !expression.Contains('"'));
        }

        private bool IsString(string leftExpression, string rightExpression)
        {
            return IsString(leftExpression) && IsString(rightExpression);
        }

        private class BoolExpression
        {
            public string LeftSideCode { get; set; }
            public string RightSideCode { get; set; }
            public string LeftSideExpression { get; set; }
            public string RightSideExpression { get; set; }
            public TokenParser.Tokens EquityOperator { get; set; }
            public bool IsString { get; set; }
        }

        private class BoolExpressionChain
        {
            public int Id { get; set; }
            public BoolExpression Expression { get; set; }
            public TokenParser.Tokens ConnectionOperator { get; set; }
        }
    }
}
