using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B33VirtualMachineBasicCompiler
{
    public class Translator
    {
        public string InitCode { get; private set; }
        public string Output { get; private set; }

        private readonly string _crLf = Environment.NewLine;

        private readonly List<Functions> _functions;

        public Translator()
        {
            _functions = new List<Functions>();
        }

        private void AddFunction(Functions function)
        {
            if (!_functions.Contains(function))
                _functions.Add(function);
        }

        private void StartInitCode()
        {
            InitCode = "curX rmb 1" + Environment.NewLine;
            InitCode = InitCode + "curY rmb 1" + Environment.NewLine;
            InitCode = InitCode + "bgcolor rmb 1" + Environment.NewLine;
            InitCode = InitCode + "fgcolor rmb 1" + Environment.NewLine;
            InitCode = InitCode + "colorattr rmb 1" + Environment.NewLine;
        }

        public TranslatorRetval Translate(string program)
        {
            int lineNumber = 1;
            TranslatorRetval retval = new TranslatorRetval();

            program = program + _crLf;
            _functions.Clear();

            AddFunction(Functions.FuncInit);
            StartInitCode();

            string[] lines = program.Replace('\r', '\n').Replace("\n\n", "\n").Split('\n');

            Output = Output + "TheBasicStart nop" + _crLf;

            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line) || string.IsNullOrEmpty(line.Trim()))
                {
                    lineNumber++;
                    continue;
                }
                TranslatorRetval lineRetval = TranslateLine(line, true);
                if (lineRetval.HasErrors)
                {
                    retval.HasErrors = true;
                    retval.ErrorLineNumber = lineNumber;
                    retval.ErrorMessage = retval.ErrorMessage;

                    return retval;
                }

                lineNumber++;
            }
            
            Output = Output + " jmp TheBasicEnd" + _crLf;
            Output = Output + _functions.Aggregate("", (current, ss) => current + Routines.GetRoutine(ss));
            Output = Output + "TheBasicEnd end TheBasicStart";
            return retval;
        }

        private TranslatorRetval TranslateLine(string line, bool requireLineNumber)
        {
            TokenParser parser = new TokenParser {InputString = line};
            TranslatorRetval retval = new TranslatorRetval();

            parser.SkipWhiteSpace();
            Token token = parser.GetToken();

            while (token != null)
            {
                parser.SkipWhiteSpace();
                
                if (requireLineNumber)
                {
                    if (token.TokenName != TokenParser.Tokens.LINENUMBER)
                    {
                        retval.HasErrors = true;
                        retval.ErrorMessage = "Missing Line Number!";
                        return retval;
                    }

                    Output = Output + "ln" + token.TokenValue + " nop" + _crLf;

                    parser.SkipWhiteSpace();
                    token = parser.GetToken();
                    parser.SkipWhiteSpace();
                }

                if (token.TokenName == TokenParser.Tokens.CLS)
                {
                    Output = Output + " call funcCLS" + _crLf;

                    AddFunction(Functions.FuncCls);
                }

                if (token.TokenName == TokenParser.Tokens.BREAK)
                {
                    Output = Output + " brk" + _crLf;
                }

                if (token.TokenName == TokenParser.Tokens.PRINT || token.TokenName == TokenParser.Tokens.PRINTSTR)
                {
                    parser.SkipWhiteSpace();
                    
                    string exp = parser.GetStringExpression();

                }

                parser.SkipWhiteSpace();
                token = parser.GetToken();
                parser.SkipWhiteSpace();
            }

            return retval;
        }
    }
}
