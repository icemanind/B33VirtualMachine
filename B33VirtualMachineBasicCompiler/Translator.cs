using System;
using System.Collections.Generic;
using System.Linq;

namespace B33VirtualMachineBasicCompiler
{
    internal class Translator
    {
        private int _stringCounter;
        private int _loopCounter;
        private int _forLoopCounter;
        private int _boolCounter;

        private readonly List<Functions> _functions;
        private readonly Stack<ForLoopData> _forLoopLabelStack;
        private readonly Stack<ForLoopData> _whileLabelStack;
        private int _lineNumber;

        public Dictionary<string, string> Variables
        {
            get;
            private set;
        }

        public IEnumerable<Functions> Functions
        {
            get { return _functions; }
        }

        public int StringCounter { get { return _stringCounter; } set { _stringCounter = value; } }
        public int ForLoopCounter { get { return _forLoopCounter; } set { _forLoopCounter = value; } }
        public int LoopCounter { get { return _loopCounter; } set { _loopCounter = value; } }
        public string InitCode { get; set; }

        public Translator(Dictionary<string, string> variables)
        {
            Variables = variables;
            _stringCounter = 1;
            _forLoopCounter = 1;
            _boolCounter = 1;
            _lineNumber = 1;
            _loopCounter = 1;
            _functions = new List<Functions>();
            _forLoopLabelStack = new Stack<ForLoopData>();
            _whileLabelStack = new Stack<ForLoopData>();
        }

        private ExpressionParserOutput ParseNumericExpression(string expression, ref string initCode)
        {
            var nc = new NumericExpressionParser { Variables = Variables };

            nc.Compile(expression, _stringCounter, _loopCounter);

            _stringCounter = nc.Output.StringCounter;
            _loopCounter = nc.Output.LoopCounter;

            initCode = initCode + nc.Output.InitCode;

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

        private ExpressionParserOutput ParseStringExpression(string expression, ref string initCode)
        {
            var sc = new StringExpressionParser { Variables = Variables };

            sc.Compile(expression, _stringCounter, _loopCounter);

            _stringCounter = sc.Output.StringCounter;
            _loopCounter = sc.Output.LoopCounter;

            initCode = initCode + sc.Output.InitCode;

            foreach (var f in sc.Output.Functions)
            {
                AddFunction(f);
            }

            return sc.Output;
        }

        public AssemblyRetVal TranslateLine(string line, ref string initCode, bool requireLineNumbers)
        {
            string output = "";
            AssemblyRetVal retval = new AssemblyRetVal
            {
                HasError = false,
                ErrorMessage = "",
                AssemblyCode = "",
                LineNumber = 0,
                InitializationCode = ""
            };

            TokenParser parser = new TokenParser { InputString = line };

            SkipWhiteSpace(parser);
            Token token = parser.GetToken();

            while (token != null && token.TokenName != TokenParser.Tokens.NEWLINE)
            {
                // If line begins with a comment, ignore
                if (token.TokenName == TokenParser.Tokens.COMMENT)
                {
                    return retval;
                }

                // Line must begin with a line number, or else its an error!
                if (token.TokenName != TokenParser.Tokens.LINENUMBER && requireLineNumbers && token.TokenName != TokenParser.Tokens.COLON)
                {
                    retval.HasError = true;
                    retval.ErrorMessage = "[" + _lineNumber + "]: All lines must begin with a line number!";
                    return retval;
                }

                if (requireLineNumbers && token.TokenName != TokenParser.Tokens.COLON)
                {
                    output = output + "ln" + token.TokenValue + " nop" + Environment.NewLine;
                    retval.LineNumber = int.Parse(token.TokenValue);
                    SkipWhiteSpace(parser);
                    token = parser.GetToken();
                }
                SkipWhiteSpace(parser);

                // check for CALL
                if (token.TokenName == TokenParser.Tokens.CALL)
                {
                    SkipWhiteSpace(parser);

                    token = parser.GetToken();
                    output = output + " push a" + Environment.NewLine;
                    output = output + " push b" + Environment.NewLine;
                    output = output + " push x" + Environment.NewLine;
                    output = output + " push y" + Environment.NewLine;
                    output = output + " call ln" + token.TokenValue + Environment.NewLine;
                    output = output + " pop y" + Environment.NewLine;
                    output = output + " pop x" + Environment.NewLine;
                    output = output + " pop b" + Environment.NewLine;
                    output = output + " pop a" + Environment.NewLine;

                    SkipWhiteSpace(parser);
                    token = parser.GetToken();

                    continue;
                }

                // check for RETURN
                if (token.TokenName == TokenParser.Tokens.RETURN)
                {
                    SkipWhiteSpace(parser);

                    output = output + " ret" + Environment.NewLine;
                    token = parser.GetToken();

                    continue;
                }

                // check for GOTO
                if (token.TokenName == TokenParser.Tokens.GOTO)
                {
                    SkipWhiteSpace(parser);

                    token = parser.GetToken();

                    output = output + " jmp ln" + token.TokenValue + Environment.NewLine;

                    SkipWhiteSpace(parser);
                    token = parser.GetToken();

                    continue;
                }

                // check for END
                if (token.TokenName == TokenParser.Tokens.END)
                {
                    SkipWhiteSpace(parser);

                    output = output + " jmp TheBasicEnd" + Environment.NewLine;
                    token = parser.GetToken();

                    continue;
                }

                // check for WHILE
                if (token.TokenName == TokenParser.Tokens.WHILE)
                {
                    SkipWhiteSpace(parser);
                    string exp = "";
                    while (parser.Peek() != null && parser.Peek().TokenPeek != null &&
                           parser.Peek().TokenPeek.TokenName != TokenParser.Tokens.NEWLINE &&
                           parser.Peek().TokenPeek.TokenName != TokenParser.Tokens.COLON)
                    {
                        token = parser.GetToken();
                        exp = exp + token.TokenValue;
                    }
                    SkipWhiteSpace(parser);
                    var bp = new BooleanExpressionParser { Variables = Variables };
                    bp.Compile(exp, _stringCounter, _loopCounter, _boolCounter);

                    _loopCounter = bp.Output.LoopCounter;
                    _stringCounter = bp.Output.StringCounter;
                    _boolCounter = bp.Output.BoolCounter;

                    initCode = initCode + bp.Output.InitCode;
                    foreach (var f in bp.Output.Functions)
                    {
                        AddFunction(f);
                    }

                    output = output + "whileloopstart" + _loopCounter + " nop" + Environment.NewLine;
                    output = output + bp.Output.Output;
                    output = output + " cmpx #0" + Environment.NewLine;
                    output = output + " jeq whileloopexit" + _loopCounter + Environment.NewLine;
                    _whileLabelStack.Push(new ForLoopData { VariableLabel = _loopCounter.ToString() });
                    _loopCounter++;

                    SkipWhiteSpace(parser);
                    token = parser.GetToken();

                    continue;
                }

                // check for LOOP
                if (token.TokenName == TokenParser.Tokens.LOOP)
                {
                    ForLoopData data = _whileLabelStack.Pop();

                    output = output + " jmp whileloopstart" + data.VariableLabel + Environment.NewLine;
                    output = output + "whileloopexit" + data.VariableLabel + " nop" + Environment.NewLine;

                    SkipWhiteSpace(parser);
                    token = parser.GetToken();

                    continue;
                }

                // check for FOR
                if (token.TokenName == TokenParser.Tokens.FOR)
                {
                    SkipWhiteSpace(parser);

                    // Label comes next (a variable name)
                    token = parser.GetToken();
                    SkipWhiteSpace(parser);

                    string varName = token.TokenValue.ToUpper();

                    if (!Variables.ContainsKey("var" + varName))
                    {
                        Variables.Add("var" + varName, "var" + varName + " rmb 2" + Environment.NewLine);
                    }

                    // Equal sign is next
                    parser.GetToken();
                    SkipWhiteSpace(parser);

                    string exp = GetNumericExpression(parser);
                    ExpressionParserOutput o = ParseNumericExpression(exp, ref initCode);

                    // TO is next
                    SkipWhiteSpace(parser);
                    parser.GetToken();
                    SkipWhiteSpace(parser);

                    output = output + o.Output;
                    output = output + " ldy #var" + varName + Environment.NewLine;
                    output = output + " tfr x,d" + Environment.NewLine;
                    output = output + " stb ,y+" + Environment.NewLine;
                    output = output + " sta ,y" + Environment.NewLine;

                    exp = GetNumericExpression(parser);
                    o = ParseNumericExpression(exp, ref initCode);


                    SkipWhiteSpace(parser);
                    string stepCode;
                    if (parser.Peek() != null && parser.Peek().TokenPeek != null &&
                        parser.Peek().TokenPeek.TokenName == TokenParser.Tokens.STEP)
                    {
                        parser.GetToken();
                        SkipWhiteSpace(parser);
                        exp = GetNumericExpression(parser);

                        if (!exp.Trim().StartsWith("+") && !exp.Trim().StartsWith("-"))
                            exp = varName + "+" + exp;
                        else exp = varName + exp;

                        ExpressionParserOutput o2 = ParseNumericExpression(exp, ref initCode);
                        stepCode = o2.Output;
                    }
                    else
                    {
                        ExpressionParserOutput o2 = ParseNumericExpression(varName + "+1", ref initCode);
                        stepCode = o2.Output;
                    }

                    _forLoopLabelStack.Push(new ForLoopData { Counter = _forLoopCounter, VariableLabel = varName, ToCodeData = o.Output, StepCodeData = stepCode });

                    output = output + "forloop" + _forLoopCounter + " nop" + Environment.NewLine;
                    output = output + o.Output;
                    output = output + " tfr x,y" + Environment.NewLine;
                    output = output + " ldx var" + varName + Environment.NewLine;
                    output = output + " cmpx y" + Environment.NewLine;
                    if (stepCode.Contains(" neg "))
                        output = output + " jlt forloopexit" + _forLoopCounter + Environment.NewLine;
                    else output = output + " jgt forloopexit" + _forLoopCounter + Environment.NewLine;

                    SkipWhiteSpace(parser);
                    token = parser.GetToken();

                    _forLoopCounter++;
                    continue;
                }

                // check for NEXT
                if (token.TokenName == TokenParser.Tokens.NEXT)
                {
                    ForLoopData data = _forLoopLabelStack.Pop();

                    output = output + data.StepCodeData;
                    output = output + " ldy #var" + data.VariableLabel + Environment.NewLine;
                    output = output + " tfr x,d" + Environment.NewLine;
                    output = output + " stb ,y+" + Environment.NewLine;
                    output = output + " sta ,y" + Environment.NewLine;
                    output = output + " jmp forloop" + data.Counter + Environment.NewLine;
                    output = output + "forloopexit" + data.Counter + " nop" + Environment.NewLine;

                    SkipWhiteSpace(parser);
                    token = parser.GetToken();

                    // optional label
                    if (token != null && token.TokenName == TokenParser.Tokens.LABEL)
                    {
                        SkipWhiteSpace(parser);
                        token = parser.GetToken();
                    }
                    continue;
                }

                // check for BREAK
                if (token.TokenName == TokenParser.Tokens.BREAK)
                {
                    output = output + " brk" + Environment.NewLine;

                    SkipWhiteSpace(parser);
                    token = parser.GetToken();
                    continue;
                }

                // check for LOCATE
                if (token.TokenName == TokenParser.Tokens.LOCATE)
                {
                    string exp = GetNumericExpression(parser);
                    SkipWhiteSpace(parser);

                    // Should be a comma
                    parser.GetToken();
                    SkipWhiteSpace(parser);
                    ExpressionParserOutput o = ParseNumericExpression(exp, ref initCode);
                    output = output + o.Output;
                    output = output + " tfr x,d" + Environment.NewLine;
                    output = output + " ldx #curY" + Environment.NewLine;
                    output = output + " stb ,x" + Environment.NewLine;

                    exp = GetNumericExpression(parser);
                    SkipWhiteSpace(parser);

                    o = ParseNumericExpression(exp, ref initCode);
                    output = output + o.Output;
                    output = output + " tfr x,d" + Environment.NewLine;
                    output = output + " ldx #curX" + Environment.NewLine;
                    output = output + " stb ,x" + Environment.NewLine;

                    output = output + " call updatecursorpos" + Environment.NewLine;

                    token = parser.GetToken();
                    continue;
                }

                // check for CLS
                if (token.TokenName == TokenParser.Tokens.CLS)
                {
                    output = output + " cls" + Environment.NewLine;
                    output = output + " ldd #0" + Environment.NewLine;
                    output = output + " ldx #curY" + Environment.NewLine;
                    output = output + " stb ,x" + Environment.NewLine;
                    output = output + " ldx #curX" + Environment.NewLine;
                    output = output + " stb ,x" + Environment.NewLine;

                    output = output + " call updatecursorpos" + Environment.NewLine;

                    SkipWhiteSpace(parser);
                    token = parser.GetToken();
                    continue;
                }

                // check for CURSOR
                if (token.TokenName == TokenParser.Tokens.CURSOR)
                {
                    SkipWhiteSpace(parser);
                    token = parser.GetToken();

                    byte val = 0;
                    if (token.TokenName == TokenParser.Tokens.ON)
                    {
                        val = 1;
                    }

                    output = output + " ldx #$efa0" + Environment.NewLine;
                    output = output + " lda #" + val + Environment.NewLine;
                    output = output + " sta ,x" + Environment.NewLine;

                    SkipWhiteSpace(parser);
                    token = parser.GetToken();
                    continue;
                }

                // Check for GOTO
                if (token.TokenName == TokenParser.Tokens.GOTO)
                {
                    SkipWhiteSpace(parser);
                    token = parser.GetToken();

                    output = output + " jmp ln" + token.TokenValue + Environment.NewLine;

                    SkipWhiteSpace(parser);
                    token = parser.GetToken();
                    continue;
                }

                // Check for Colon
                if (token.TokenName == TokenParser.Tokens.COLON)
                {
                    SkipWhiteSpace(parser);
                    token = parser.GetToken();
                    string command = "";
                    while (token != null && token.TokenName != TokenParser.Tokens.NEWLINE)
                    {
                        command = command + token.TokenValue;
                        token = parser.GetToken();
                    }

                    AssemblyRetVal retval2 = TranslateLine(command, ref initCode, false);

                    output = output + retval2.AssemblyCode;

                    SkipWhiteSpace(parser);
                    token = parser.GetToken();
                    continue;
                }

                // Check for IF
                if (token.TokenName == TokenParser.Tokens.IF)
                {
                    SkipWhiteSpace(parser);

                    string exp = "";
                    while (parser.Peek().TokenPeek.TokenName != TokenParser.Tokens.THEN)
                    {
                        token = parser.GetToken();
                        exp = exp + token.TokenValue;
                    }

                    var bp = new BooleanExpressionParser { Variables = Variables };
                    bp.Compile(exp, _stringCounter, _loopCounter, _boolCounter);

                    _loopCounter = bp.Output.LoopCounter;
                    _stringCounter = bp.Output.StringCounter;
                    _boolCounter = bp.Output.BoolCounter;

                    initCode = initCode + bp.Output.InitCode;
                    foreach (var f in bp.Output.Functions)
                    {
                        AddFunction(f);
                    }

                    output = output + bp.Output.Output;

                    SkipWhiteSpace(parser);
                    parser.GetToken();
                    SkipWhiteSpace(parser);
                    token = parser.GetToken();

                    string command = "";
                    while (token != null && token.TokenName != TokenParser.Tokens.NEWLINE)
                    {
                        command = command + token.TokenValue;
                        token = parser.GetToken();
                    }

                    AssemblyRetVal retval2 = TranslateLine(command, ref initCode, false);

                    output = output + " cmpx #1" + Environment.NewLine;
                    output = output + " jne ifjump" + _loopCounter + Environment.NewLine;
                    output = output + retval2.AssemblyCode;
                    output = output + "ifjump" + _loopCounter + " nop" + Environment.NewLine;
                    _loopCounter++;
                    continue;
                }

                // check for PRINTNUM
                if (token.TokenName == TokenParser.Tokens.PRINTNUM)
                {
                    SkipWhiteSpace(parser);
                    string exp = GetNumericExpression(parser);
                    ExpressionParserOutput o = ParseNumericExpression(exp, ref initCode);

                    output = output + o.Output;
                    output = output + " ldy #TheBasicStrBuf" + Environment.NewLine;
                    output = output + " call num2txt" + Environment.NewLine;
                    output = output + " ldx #TheBasicStrBuf" + Environment.NewLine;
                    output = output + " call funcPRINT" + Environment.NewLine;

                    AddFunction(B33VirtualMachineBasicCompiler.Functions.FuncPrintNum);
                    AddFunction(B33VirtualMachineBasicCompiler.Functions.FuncPrint);

                    SkipWhiteSpace(parser);
                    token = parser.GetToken();
                    continue;
                }

                // check for PRINT
                if (token.TokenName == TokenParser.Tokens.PRINT || token.TokenName == TokenParser.Tokens.PRINTSTR)
                {
                    SkipWhiteSpace(parser);
                    string exp = GetStringExpression(parser);
                    var sc = new StringExpressionParser { Variables = Variables };
                    sc.Compile(exp, _stringCounter, _loopCounter);
                    _stringCounter = sc.Output.StringCounter;
                    _loopCounter = sc.Output.LoopCounter;

                    //output = output + " ldy #TheBasicStrBuf" + Environment.NewLine;
                    //output = output + " lda #0" + Environment.NewLine;
                    //output = output + " sta ,y" + Environment.NewLine;
                    output = output + sc.Output.Output;
                    output = output + " push x" + Environment.NewLine;
                    //output = output + " ldx #TheBasicStrBuf" + Environment.NewLine;
                    output = output + " call funcPRINT" + Environment.NewLine;
                    output = output + " pop x" + Environment.NewLine;
                    output = output + " call freememory" + Environment.NewLine;
                    initCode = initCode + sc.Output.InitCode;
                    AddFunction(B33VirtualMachineBasicCompiler.Functions.FuncPrint);
                    foreach (var f in sc.Output.Functions)
                    {
                        AddFunction(f);
                    }

                    SkipWhiteSpace(parser);
                    token = parser.GetToken();
                    continue;
                }

                // check for BGCOLOR
                if (token.TokenName == TokenParser.Tokens.BGCOLOR)
                {
                    SkipWhiteSpace(parser);
                    Token peek = parser.Peek().TokenPeek;
                    string v = peek.TokenValue.ToUpper().Trim();

                    if (peek != null && peek.TokenName == TokenParser.Tokens.LABEL &&
                        (v == "BLACK" || v == "BLUE" || v == "GREEN" || v == "CYAN" ||
                         v == "RED" || v == "MAGENTA" || v == "BROWN" || v == "WHITE" ||
                         v == "DARKGREY" || v == "DARKGRAY" || v == "GRAY" || v == "GREY" ||
                         v == "BRIGHTBLUE" || v == "LIGHTBLUE" || v == "BRIGHTGREEN" || v == "LIGHTGREEN" ||
                         v == "BRIGHTCYAN" || v == "LIGHTCYAN" || v == "BRIGHTRED" || v == "LIGHTRED" ||
                         v == "PINK" || v == "BRIGHTMAGENTA" || v == "LIGHTMAGENTA" || v == "YELLOW" ||
                         v == "BRIGHTWHITE" || v == "LIGHTWHITE"))
                    {
                        parser.GetToken();
                        switch (peek.TokenValue.ToUpper())
                        {
                            case "BLACK":
                                output = output + " lda #0" + Environment.NewLine;
                                break;
                            case "BLUE":
                                output = output + " lda #1" + Environment.NewLine;
                                break;
                            case "GREEN":
                                output = output + " lda #2" + Environment.NewLine;
                                break;
                            case "CYAN":
                                output = output + " lda #3" + Environment.NewLine;
                                break;
                            case "RED":
                                output = output + " lda #4" + Environment.NewLine;
                                break;
                            case "MAGENTA":
                                output = output + " lda #5" + Environment.NewLine;
                                break;
                            case "BROWN":
                                output = output + " lda #6" + Environment.NewLine;
                                break;
                            case "WHITE":
                                output = output + " lda #7" + Environment.NewLine;
                                break;
                            case "DARKGREY":
                            case "DARKGRAY":
                            case "GRAY":
                            case "GREY":
                                output = output + " lda #8" + Environment.NewLine;
                                break;
                            case "BRIGHTBLUE":
                            case "LIGHTBLUE":
                                output = output + " lda #9" + Environment.NewLine;
                                break;
                            case "BRIGHTGREEN":
                            case "LIGHTGREEN":
                                output = output + " lda #10" + Environment.NewLine;
                                break;
                            case "BRIGHTCYAN":
                            case "LIGHTCYAN":
                                output = output + " lda #11" + Environment.NewLine;
                                break;
                            case "PINK":
                            case "BRIGHTRED":
                            case "LIGHTRED":
                                output = output + " lda #12" + Environment.NewLine;
                                break;
                            case "BRIGHTMAGENTA":
                            case "LIGHTMAGENTA":
                                output = output + " lda #13" + Environment.NewLine;
                                break;
                            case "YELLOW":
                                output = output + " lda #14" + Environment.NewLine;
                                break;
                            case "BRIGHTWHITE":
                            case "LIGHTWHITE":
                                output = output + " lda #15" + Environment.NewLine;
                                break;
                        }
                    }
                    else
                    {
                        string exp = GetNumericExpression(parser);
                        ExpressionParserOutput o = ParseNumericExpression(exp, ref initCode);
                        _boolCounter = o.BoolCounter;
                        _loopCounter = o.LoopCounter;
                        _stringCounter = o.StringCounter;
                        foreach (var f in o.Functions)
                        {
                            AddFunction(f);
                        }

                        output = output + o.Output;
                        output = output + " tfr x,d" + Environment.NewLine;
                        output = output + " tfr b,a" + Environment.NewLine;
                    }
                    output = output + " ldx #bgcolor" + Environment.NewLine;
                    output = output + " sta ,x" + Environment.NewLine;
                    output = output + " call setattrib" + Environment.NewLine;

                    AddFunction(B33VirtualMachineBasicCompiler.Functions.FuncAttribs);

                    SkipWhiteSpace(parser);
                    token = parser.GetToken();
                    continue;
                }

                // check for FGCOLOR
                if (token.TokenName == TokenParser.Tokens.FGCOLOR)
                {
                    SkipWhiteSpace(parser);
                    Token peek = parser.Peek().TokenPeek;
                    string v = peek.TokenValue.ToUpper().Trim();

                    if (peek != null && peek.TokenName == TokenParser.Tokens.LABEL &&
                        (v == "BLACK" || v == "BLUE" || v == "GREEN" || v == "CYAN" ||
                         v == "RED" || v == "MAGENTA" || v == "BROWN" || v == "WHITE" ||
                         v == "DARKGREY" || v == "DARKGRAY" || v == "GRAY" || v == "GREY" ||
                         v == "BRIGHTBLUE" || v == "LIGHTBLUE" || v == "BRIGHTGREEN" || v == "LIGHTGREEN" ||
                         v == "BRIGHTCYAN" || v == "LIGHTCYAN" || v == "BRIGHTRED" || v == "LIGHTRED" ||
                         v == "PINK" || v == "BRIGHTMAGENTA" || v == "LIGHTMAGENTA" || v == "YELLOW" ||
                         v == "BRIGHTWHITE" || v == "LIGHTWHITE"))
                    {
                        parser.GetToken();
                        switch (peek.TokenValue.ToUpper())
                        {
                            case "BLACK":
                                output = output + " lda #0" + Environment.NewLine;
                                break;
                            case "BLUE":
                                output = output + " lda #1" + Environment.NewLine;
                                break;
                            case "GREEN":
                                output = output + " lda #2" + Environment.NewLine;
                                break;
                            case "CYAN":
                                output = output + " lda #3" + Environment.NewLine;
                                break;
                            case "RED":
                                output = output + " lda #4" + Environment.NewLine;
                                break;
                            case "MAGENTA":
                                output = output + " lda #5" + Environment.NewLine;
                                break;
                            case "BROWN":
                                output = output + " lda #6" + Environment.NewLine;
                                break;
                            case "WHITE":
                                output = output + " lda #7" + Environment.NewLine;
                                break;
                            case "DARKGREY":
                            case "DARKGRAY":
                            case "GRAY":
                            case "GREY":
                                output = output + " lda #8" + Environment.NewLine;
                                break;
                            case "BRIGHTBLUE":
                            case "LIGHTBLUE":
                                output = output + " lda #9" + Environment.NewLine;
                                break;
                            case "BRIGHTGREEN":
                            case "LIGHTGREEN":
                                output = output + " lda #10" + Environment.NewLine;
                                break;
                            case "BRIGHTCYAN":
                            case "LIGHTCYAN":
                                output = output + " lda #11" + Environment.NewLine;
                                break;
                            case "PINK":
                            case "BRIGHTRED":
                            case "LIGHTRED":
                                output = output + " lda #12" + Environment.NewLine;
                                break;
                            case "BRIGHTMAGENTA":
                            case "LIGHTMAGENTA":
                                output = output + " lda #13" + Environment.NewLine;
                                break;
                            case "YELLOW":
                                output = output + " lda #14" + Environment.NewLine;
                                break;
                            case "BRIGHTWHITE":
                            case "LIGHTWHITE":
                                output = output + " lda #15" + Environment.NewLine;
                                break;
                        }
                    }
                    else
                    {
                        string exp = GetNumericExpression(parser);
                        ExpressionParserOutput o = ParseNumericExpression(exp, ref initCode);
                        _boolCounter = o.BoolCounter;
                        _loopCounter = o.LoopCounter;
                        _stringCounter = o.StringCounter;
                        foreach (var f in o.Functions)
                        {
                            AddFunction(f);
                        }

                        output = output + o.Output;
                        output = output + " tfr x,d" + Environment.NewLine;
                        output = output + " tfr b,a" + Environment.NewLine;
                    }
                    output = output + " ldx #fgcolor" + Environment.NewLine;
                    output = output + " sta ,x" + Environment.NewLine;
                    output = output + " call setattrib" + Environment.NewLine;

                    AddFunction(B33VirtualMachineBasicCompiler.Functions.FuncAttribs);
                }

                // check for DIM
                if (token.TokenName == TokenParser.Tokens.DIM)
                {
                    SkipWhiteSpace(parser);
                    token = parser.GetToken();
                    SkipWhiteSpace(parser);

                    if (token.TokenName == TokenParser.Tokens.LABEL)
                    {
                        string varName = token.TokenValue.ToUpper().Trim();

                        string exp = GetNumericExpression(parser);
                        var native = new NativeExpressionParser();
                        ushort val = native.Compile(exp);
                        Variables.Add("varArray" + varName, "varArray" + varName + " rmb " + val*2 + Environment.NewLine);

                        SkipWhiteSpace(parser);
                        token = parser.GetToken();
                        continue;
                    }

                    if (token.TokenName == TokenParser.Tokens.STRINGLABEL)
                    {
                        string varName = token.TokenValue.ToUpper().Trim().Replace("$", "");

                        string exp = GetNumericExpression(parser);
                        var native = new NativeExpressionParser();
                        ushort val = native.Compile(exp);
                        Variables.Add("varStrArray" + varName, "varStrArray" + varName + " rmb " + val*250 + Environment.NewLine);

                        SkipWhiteSpace(parser);
                        token = parser.GetToken();
                        continue;
                    }
                }

                // check for a string assignment
                if (token.TokenName == TokenParser.Tokens.STRINGLABEL)
                {
                    string varName = token.TokenValue.ToUpper().Replace("$", "");
                    bool isArray = false;
                    string arrayIndexCode = "";

                    SkipWhiteSpace(parser);
                    token = parser.GetToken();
                    SkipWhiteSpace(parser);

                    if (token.TokenName == TokenParser.Tokens.LPAREN)
                    {
                        isArray = true;
                        string exp2 = GetNumericExpression(parser);

                        exp2 = exp2.Trim().Remove(exp2.Trim().Length - 1);

                        arrayIndexCode = ParseNumericExpression(exp2, ref initCode).Output;

                        SkipWhiteSpace(parser);
                        token = parser.GetToken();
                    }

                    if (token.TokenName != TokenParser.Tokens.EQUAL)
                    {
                        retval.HasError = true;
                        retval.ErrorMessage = "[" + _lineNumber + "]: Unsure what to do?!";
                        return retval;
                    }

                    string exp = GetStringExpression(parser);

                    var sc = new StringExpressionParser { Variables = Variables };

                    sc.Compile(exp, _stringCounter, _loopCounter);

                    _stringCounter = sc.Output.StringCounter;
                    _loopCounter = sc.Output.LoopCounter;
                    initCode = initCode + sc.Output.InitCode;

                    foreach (var v in sc.Variables)
                    {
                        if (!Variables.ContainsKey(v.Key))
                        {
                            Variables.Add(v.Key, v.Value);
                        }
                    }

                    foreach (var f in sc.Output.Functions)
                    {
                        AddFunction(f);
                    }

                    if (!isArray)
                    {
                        output = output + sc.Output.Output;
                        output = output + " ldy #varStr" + varName + Environment.NewLine;
                        output = output + " call copystring" + Environment.NewLine;
                        output = output + " call freememory" + Environment.NewLine;

                        if (!Variables.ContainsKey("varStr" + varName))
                        {
                            Variables.Add("varStr" + varName, "varStr" + varName + " rmb 250" + Environment.NewLine);
                        }
                    }
                    else
                    {
                        output = output + arrayIndexCode;
                        output = output + " tfr x,d" + Environment.NewLine;
                        output = output + " tfr b,a" + Environment.NewLine;
                        output = output + " ldx #varStrArray" + varName + Environment.NewLine;
                        output = output + " call ArrayIndexString" + Environment.NewLine;
                        output = output + " tfr x,y" + Environment.NewLine;
                        output = output + sc.Output.Output;
                        output = output + " call copystring" + Environment.NewLine;
                        output = output + " call freememory" + Environment.NewLine;
                    }

                    SkipWhiteSpace(parser);
                    token = parser.GetToken();
                    continue;

                    //string varName = token.TokenValue.ToUpper().Replace("$", "");
                    //bool isArray = false;
                    //string ndxcode = "";

                    //SkipWhiteSpace(parser);

                    //if (parser.Peek() != null && parser.Peek().TokenPeek != null &&
                    //    parser.Peek().TokenPeek.TokenName == TokenParser.Tokens.LPAREN)
                    //{
                    //    isArray = true;
                    //    string exp2 = "(" + GetNumericExpression(parser) + ")";

                    //    NumericExpressionParser ec2 = new NumericExpressionParser { Variables = Variables };
                    //    ec2.Compile(exp2, _stringCounter, _loopCounter);
                    //    _loopCounter = ec2.Output.LoopCounter;
                    //    _stringCounter = ec2.Output.StringCounter;
                    //    initCode = initCode + ec2.Output.InitCode;
                    //    ndxcode = ec2.Output.Output;

                    //    SkipWhiteSpace(parser);
                    //}

                    //token = parser.GetToken();
                    //SkipWhiteSpace(parser);
                    //if (token.TokenName != TokenParser.Tokens.EQUAL)
                    //{
                    //    retval.HasError = true;
                    //    retval.ErrorMessage = "[" + _lineNumber + "]: Unsure what to do?!";
                    //    return retval;
                    //}
                    //SkipWhiteSpace(parser);
                    //string exp = GetStringExpression(parser);
                    //var sc = new StringExpressionParser { Variables = Variables };

                    //sc.Compile(exp, _stringCounter, _loopCounter);

                    //_stringCounter = sc.Output.StringCounter;
                    //_loopCounter = sc.Output.LoopCounter;
                    //initCode = initCode + sc.Output.InitCode;

                    //output = output + " ldy #TheBasicStrBuf" + Environment.NewLine;
                    //output = output + " lda #0" + Environment.NewLine;
                    //output = output + " sta ,y" + Environment.NewLine;

                    //if (!isArray)
                    //{
                    //    output = output + " ldy #tmpbuffer" + Environment.NewLine + " sta ,y" + Environment.NewLine + sc.Output.Output;
                    //    //output = output + " ldy #varStr" + varName + Environment.NewLine + sc.Output.Output;
                    //}
                    //else output = output + sc.Output.Output;

                    //foreach (var f in sc.Output.Functions)
                    //{
                    //    AddFunction(f);
                    //}

                    //if (!Variables.ContainsKey("varStr" + varName) && !isArray)
                    //{
                    //    Variables.Add("varStr" + varName, "varStr" + varName + " rmb 250" + Environment.NewLine);
                    //}

                    //if (isArray)
                    //{   
                    //    output = output + " push x" + Environment.NewLine;
                    //    output = output + ndxcode;
                    //    output = output + " tfr x,d" + Environment.NewLine;
                    //    output = output + " tfr b,a" + Environment.NewLine;
                    //    output = output + " ldx #varStrArray" + varName + Environment.NewLine;
                    //    output = output + " call ArrayIndexString" + Environment.NewLine;
                    //    output = output + " tfr x,y" + Environment.NewLine;
                    //    output = output + " pop x" + Environment.NewLine;
                    //}

                    //output = output + " ldx #TheBasicStrBuf" + Environment.NewLine;
                    //output = output + "loop" + _loopCounter + " lda ,x+" + Environment.NewLine;
                    //output = output + " cmpa #0" + Environment.NewLine;
                    //output = output + " jeq exitLoop" + _loopCounter + Environment.NewLine;
                    //output = output + " sta ,y+" + Environment.NewLine;
                    //output = output + " jmp loop" + _loopCounter + Environment.NewLine;
                    //output = output + "exitLoop" + _loopCounter + " sta ,y" + Environment.NewLine;
                    //if (!isArray)
                    //{
                    //    output = output + " ldx #tmpbuffer" + Environment.NewLine;
                    //    output = output + " ldy #varStr" + varName + Environment.NewLine;
                    //    output = output + " call tmpstrcopy" + Environment.NewLine;
                    //}
                    //_loopCounter++;

                    //SkipWhiteSpace(parser);
                    //token = parser.GetToken();
                    //continue;
                }

                // Check for a number assignment
                if (token.TokenName == TokenParser.Tokens.LABEL)
                {
                    string varName = token.TokenValue.ToUpper();
                    bool isArray = false;
                    string ndxcode = "";

                    SkipWhiteSpace(parser);
                    if (parser.Peek() != null && parser.Peek().TokenPeek != null &&
                        parser.Peek().TokenPeek.TokenName == TokenParser.Tokens.LPAREN)
                    {
                        isArray = true;
                        string exp2 = "(" + GetNumericExpression(parser) + ")";

                        NumericExpressionParser ec2 = new NumericExpressionParser {Variables = Variables};
                        ec2.Compile(exp2, _stringCounter, _loopCounter);
                        _loopCounter = ec2.Output.LoopCounter;
                        _stringCounter = ec2.Output.StringCounter;
                        initCode = initCode + ec2.Output.InitCode;
                        ndxcode = ec2.Output.Output;

                        SkipWhiteSpace(parser);
                    }
                    token = parser.GetToken();
                    SkipWhiteSpace(parser);
                    
                    if (token.TokenName != TokenParser.Tokens.EQUAL)
                    {
                        retval.HasError = true;
                        retval.ErrorMessage = "[" + _lineNumber + "]: Unsure what to do?!";
                        return retval;
                    }
                    SkipWhiteSpace(parser);
                    string exp = GetNumericExpression(parser);
                    NumericExpressionParser ec = new NumericExpressionParser { Variables = Variables };

                    ec.Compile(exp, _stringCounter, _loopCounter);

                    _loopCounter = ec.Output.LoopCounter;
                    _stringCounter = ec.Output.StringCounter;
                    initCode = initCode + ec.Output.InitCode;

                    output = output + ec.Output.Output;
                    if (isArray)
                    {
                        output = output + " push x" + Environment.NewLine;
                        output = output + " push y" + Environment.NewLine;
                        output = output + " push d" + Environment.NewLine;
                        output = output + " push x" + Environment.NewLine;
                        output = output + ndxcode;
                        output = output + " tfr x,d" + Environment.NewLine;
                        output = output + " tfr b,a" + Environment.NewLine;
                        output = output + " ldx #varArray" + varName + Environment.NewLine;
                        output = output + " call ArrayIndexNumeric" + Environment.NewLine;
                        output = output + " tfr x,y" + Environment.NewLine;
                        output = output + " pop x" + Environment.NewLine;
                        output = output + " tfr x,d" + Environment.NewLine;
                        output = output + " stb ,y+" + Environment.NewLine;
                        output = output + " sta ,y" + Environment.NewLine;
                        output = output + " pop d" + Environment.NewLine;
                        output = output + " pop y" + Environment.NewLine;
                        output = output + " pop x" + Environment.NewLine;
                    }
                    else
                    {
                        output = output + " ldy #var" + varName + Environment.NewLine;
                        output = output + " tfr x,d" + Environment.NewLine;
                        output = output + " stb ,y+" + Environment.NewLine;
                        output = output + " sta ,y" + Environment.NewLine;
                    }

                    if (!Variables.ContainsKey("var" + varName))
                    {
                        Variables.Add("var" + varName, "var" + varName + " rmb 2" + Environment.NewLine);
                    }

                    SkipWhiteSpace(parser);
                    token = parser.GetToken();
                    continue;
                }

                // Grab next token
                token = parser.GetToken();
            }

            if (output.Contains("call funcLEN"))
                AddFunction(B33VirtualMachineBasicCompiler.Functions.FuncLen);
            if (output.Contains("call funcRND"))
                AddFunction(B33VirtualMachineBasicCompiler.Functions.FuncRnd);
            if (output.Contains("call funcPEEK"))
                AddFunction(B33VirtualMachineBasicCompiler.Functions.FuncPeek);
            if (output.Contains("call funcASC"))
                AddFunction(B33VirtualMachineBasicCompiler.Functions.FuncAsc);
            if (output.Contains("call funcKEY"))
                AddFunction(B33VirtualMachineBasicCompiler.Functions.FuncKey);
            if (output.Contains("call funcVAL") || output.Contains("call funcINPUT2"))
                AddFunction(B33VirtualMachineBasicCompiler.Functions.FuncVal);
            if (output.Contains("call funcINPUT2"))
                AddFunction(B33VirtualMachineBasicCompiler.Functions.FuncInput2);
            if (output.Contains("call funcGETX"))
                AddFunction(B33VirtualMachineBasicCompiler.Functions.FuncGetX);
            if (output.Contains("call funcGETY"))
                AddFunction(B33VirtualMachineBasicCompiler.Functions.FuncGetY);

            retval.AssemblyCode = output;

            _lineNumber++;
            return retval;
        }

        private string GetStringExpression(TokenParser parser)
        {
            if (parser == null || parser.Peek() == null || parser.Peek().TokenPeek == null)
                return "";

            string retval = "";

            while (parser.Peek() != null && parser.Peek().TokenPeek != null &&
                   parser.Peek().TokenPeek.TokenName != TokenParser.Tokens.NEWLINE &&
                   TokenParser.GetStatementsList().All(z => z != parser.Peek().TokenPeek.TokenName))
            {
                Token token = parser.GetToken();
                retval = retval + token.TokenValue;
            }

            //parser.GetToken();
            return retval;
        }

        private string GetNumericExpression(TokenParser parser)
        {
            int parens = 0;
            if (parser == null || parser.Peek() == null || parser.Peek().TokenPeek == null)
                return "";

            string retval = "";

            while (parser.Peek() != null && parser.Peek().TokenPeek != null &&
                   parser.Peek().TokenPeek.TokenName != TokenParser.Tokens.NEWLINE &&
                   parser.Peek().TokenPeek.TokenName != TokenParser.Tokens.COLON &&
                   parser.Peek().TokenPeek.TokenName != TokenParser.Tokens.EQUAL &&
                   TokenParser.GetStatementsList().All(z => z != parser.Peek().TokenPeek.TokenName))
            {
                if (parser.Peek().TokenPeek.TokenName == TokenParser.Tokens.COMMA)
                {
                    if (parens == 0)
                        break;
                }
                Token token = parser.GetToken();
                retval = retval + token.TokenValue;
                if (token.TokenValue.Trim().EndsWith("("))
                    parens++;
                if (token.TokenValue.Trim().EndsWith(")"))
                    parens--;
            }

            return retval;
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

        private void AddFunction(Functions function)
        {
            if (!_functions.Contains(function))
                _functions.Add(function);
        }
    }
}
