using System;
using System.Collections.Generic;
using System.Text;
using B33Cpu;

namespace B33Assembler
{
    public class Assembler
    {
        private readonly Dictionary<string, ushort> _labelDictionary;
        private readonly List<DebugData> _debugInfo;
        private readonly List<byte> _programData;
        private string _program;

        public bool Successful
        {
            get;
            private set;
        }

        public string ErrorMessage
        {
            get;
            private set;
        }

        public bool RequiresDualMonitors
        {
            get;
            set;
        }

        public bool IncludeDebugInformation
        {
            get;
            set;
        }

        public ushort Origin
        {
            get;
            set;
        }

        public string Program
        {
            get { return _program; }
            set
            {
                _program = value;
                _program = _program + Environment.NewLine;
                _program = _program.Replace("\r\n", "\n");
            }
        }

        public OutputTypes OutputType
        {
            get;
            set;
        }

        public Assembler()
        {
            _labelDictionary = new Dictionary<string, ushort>();
            _debugInfo = new List<DebugData>();
            _programData = new List<byte>();
            Program = "";
            Origin = 0x1000;
            OutputType = OutputTypes.B33Executable;
            RequiresDualMonitors = false;
            IncludeDebugInformation = false;
        }

        public Assembler(string program)
        {
            _labelDictionary = new Dictionary<string, ushort>();
            _debugInfo = new List<DebugData>();
            _programData = new List<byte>();
            Program = program;
            Origin = 0x1000;
            OutputType = OutputTypes.B33Executable;
            RequiresDualMonitors = false;
            IncludeDebugInformation = false;
        }

        public Assembler(string program, ushort origin)
        {
            _labelDictionary = new Dictionary<string, ushort>();
            _debugInfo = new List<DebugData>();
            _programData = new List<byte>();
            Program = program;
            Origin = origin;
            OutputType = OutputTypes.B33Executable;
            RequiresDualMonitors = false;
            IncludeDebugInformation = false;
        }

        public B33Program Assemble(OutputTypes outputType)
        {
            OutputType = outputType;
            return Assemble();
        }

        public B33Program Assemble()
        {
            _labelDictionary.Clear();
            _debugInfo.Clear();
            _programData.Clear();
            Successful = true;
            ErrorMessage = "";

            if (OutputType == OutputTypes.B33Executable)
            {
                // 3 Magic Bytes Header
                _programData.Add(66); // B
                _programData.Add(51); // 3
                _programData.Add(51); // 3

                // Write Origin Address
                _programData.Add(BitConverter.GetBytes(Origin)[0]); // LSB
                _programData.Add(BitConverter.GetBytes(Origin)[1]); // MSB

                // Write Execution Address
                // It is unknown at this time, so fill with 0's for now
                // and we will come back to it later
                _programData.Add(0);
                _programData.Add(0);

                // Debug Info Adddress
                // It is unknown at this time, so fill with 0's for now
                // and we will come back to it later
                _programData.Add(0);
                _programData.Add(0);

                // Write a 1 if this program requires dual monitors
                // Otherwise, write a 0
                _programData.Add(RequiresDualMonitors ? (byte) 1 : (byte) 0);
            }

            B33Program program = AssembleProgram();

            byte[] bytes;

            if (IncludeDebugInformation)
            {
                foreach (DebugData dd in _debugInfo)
                {
                    byte[] line = Encoding.ASCII.GetBytes(dd.SourceCodeLine);
                    var termLine = new byte[line.Length + 1];
                    line.CopyTo(termLine, 0);
                    termLine[line.Length] = 0;
                    _programData.Add(BitConverter.GetBytes(dd.Address)[0]);
                    _programData.Add(BitConverter.GetBytes(dd.Address)[1]);

                    foreach (var q in termLine)
                    {
                        _programData.Add(q);
                    }
                }

                bytes = _programData.ToArray();
                if (OutputType == OutputTypes.B33Executable)
                {
                    bytes[7] = BitConverter.GetBytes(program.DebugStartAddress)[0];
                    bytes[8] = BitConverter.GetBytes(program.DebugStartAddress)[1];
                }
                _programData.Clear();
                foreach (var q in bytes)
                {
                    _programData.Add(q);
                }
            }

            if (!Successful)
                return null;
            bytes = _programData.ToArray();
            if (OutputType == OutputTypes.B33Executable)
            {
                bytes[5] = BitConverter.GetBytes(program.ExecutionAddress)[0];
                bytes[6] = BitConverter.GetBytes(program.ExecutionAddress)[1];
            }

            program.ProgramBytes = bytes;
            program.StartAddress = Origin;
            program.ProgramType = OutputType;
            return program;
        }

        private B33Program AssembleProgram()
        {
            ushort executionAddress = 0;
            ushort debugStart = 0;

            // Assembly is done in 2 phases:
            //
            // Phase 1 : Label Scan
            // The Label Scan phase scans the program for labels and puts them into
            // a dictionary containing the address of the label. The label scanning
            // must be done first, before assembling, in case there is a forward
            // reference to a label.
            //
            // Phase 2 : Actual Assembly
            //

            for (int phase = 1; phase < 3; phase++)
            {
                int lineNumber = 1;
                var tokenParser = new TokenParser { InputString = Program };
                ushort progLoc = Origin;

                Token token = tokenParser.GetToken();
                while (token != null)
                {
                    if (token.TokenName == TokenParser.Tokens.COMMENT || token.TokenName == TokenParser.Tokens.NEWLINE)
                    {
                        lineNumber++;
                        token = tokenParser.GetToken();
                        continue;
                    }

                    if (token.TokenName != TokenParser.Tokens.WHITESPACE && token.TokenName != TokenParser.Tokens.LABEL)
                    {
                        Error(string.Format("Expected Label or Whitespace at line {0}!", lineNumber));
                        return null;
                    }

                    if (token.TokenName == TokenParser.Tokens.LABEL)
                    {
                        if (phase == 1)
                        {
                            if (_labelDictionary.ContainsKey(token.TokenValue.ToUpper()))
                            {
                                Error(string.Format("Label \"{0}\" already defined at line {1}!",
                                                    token.TokenValue.ToUpper(), lineNumber));
                                return null;
                            }
                            _labelDictionary.Add(token.TokenValue.ToUpper(), progLoc);
                        }
                        token = tokenParser.GetToken();
                        continue;
                    }

                    if (token.TokenName == TokenParser.Tokens.WHITESPACE)
                    {
                        token = tokenParser.GetToken();
                        if (token == null)
                        {
                            Error(string.Format("Unexpected End of File at line {0}!", lineNumber));
                            return null;
                        }
                        switch (token.TokenName)
                        {
                            case TokenParser.Tokens.STR:
                                //OpcodeRetVal retval = Opcodes.Str(tokenParser, phase, ref progLoc, _programData, lineNumber);
                                //if (!retval.Success)
                                //{
                                //    Error(retval.ErrorMessage);
                                //    return null;
                                //}
                                token = tokenParser.GetToken();
                                if (token == null)
                                {
                                    Error(string.Format("Unexpected End of File at line {0}!", lineNumber));
                                    return null;
                                }
                                if (token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected Whitespace at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.STRING)
                                {
                                    Error(string.Format("Expected string at line {0}!", lineNumber));
                                    return null;
                                }
                                byte[] bt = Encoding.ASCII.GetBytes(token.TokenValue.Remove(token.TokenValue.Length - 1, 1).Remove(0, 1));
                                if (phase == 2)
                                {
                                    for (int index = 0; index < bt.Length; index++)
                                    {
                                        byte bty = bt[index];
                                        _programData.Add(bty);
                                    }
                                }
                                progLoc = (ushort) (progLoc + bt.Length);
                                token = tokenParser.GetToken();
                                    while (token != null &&
                                           (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                            token.TokenName == TokenParser.Tokens.COMMENT))
                                    {
                                        token = tokenParser.GetToken();
                                    }
                                continue;
                            case TokenParser.Tokens.CHR:
                                token = tokenParser.GetToken();
                                if (token == null)
                                {
                                    Error(string.Format("Unexpected End of File at line {0}!", lineNumber));
                                    return null;
                                }
                                if (token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected Whitespace at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || (token.TokenName != TokenParser.Tokens.NUMBER && token.TokenName != TokenParser.Tokens.HEXNUMBER && token.TokenName != TokenParser.Tokens.BINARYNUMBER))
                                {
                                    Error(string.Format("Expected number at line {0}!", lineNumber));
                                    return null;
                                }
                                byte bt2 = Get8BitNumber(token);
                                if (phase == 2)
                                {
                                    _programData.Add(bt2);
                                }
                                progLoc = (ushort)(progLoc + 1);
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.LDA:
                                token = tokenParser.GetToken();
                                if (token == null)
                                {
                                    Error(string.Format("Unexpected End of File at line {0}!", lineNumber));
                                    return null;
                                }
                                if (token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected Whitespace at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null)
                                {
                                    Error(string.Format("Unexpected End of File at line {0}!", lineNumber));
                                    return null;
                                }
                                if (token.TokenName == TokenParser.Tokens.POUND) // LDA Immediate
                                {
                                    token = tokenParser.GetToken();
                                    if (token == null)
                                    {
                                        Error(string.Format("Unexpected End of File at line {0}!", lineNumber));
                                        return null;
                                    }
                                    if (token.TokenName != TokenParser.Tokens.NUMBER &&
                                        token.TokenName != TokenParser.Tokens.HEXNUMBER &&
                                        token.TokenName != TokenParser.Tokens.BINARYNUMBER)
                                    {
                                        Error(string.Format("Expected number at line {0}!", lineNumber));
                                        return null;
                                    }
                                    byte b = Get8BitNumber(token);
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x01);
                                        _programData.Add(b);
                                        _debugInfo.Add(new DebugData {Address = progLoc, SourceCodeLine = "    LDA    #$" + b.ToString("X2")});
                                    }
                                    progLoc = (ushort) (progLoc + 2);
                                    token = tokenParser.GetToken();
                                    while (token != null &&
                                           (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                            token.TokenName == TokenParser.Tokens.COMMENT))
                                    {
                                        token = tokenParser.GetToken();
                                    }
                                } else if (token.TokenName == TokenParser.Tokens.NUMBER ||
                                           token.TokenName == TokenParser.Tokens.HEXNUMBER ||
                                           token.TokenName == TokenParser.Tokens.BINARYNUMBER) // LDA Extended
                                {
                                    ushort s = Get16BitNumber(token);
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x0B);
                                        _programData.Add(BitConverter.GetBytes(s)[0]);
                                        _programData.Add(BitConverter.GetBytes(s)[1]);
                                        
                                        _debugInfo.Add(new DebugData { Address = progLoc, SourceCodeLine = "    LDA    $" + s.ToString("X4") });
                                    }
                                    progLoc = (ushort) (progLoc + 3);
                                    token = tokenParser.GetToken();
                                    while (token != null &&
                                           (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                            token.TokenName == TokenParser.Tokens.COMMENT))
                                    {
                                        token = tokenParser.GetToken();
                                    }
                                } else if (token.TokenName == TokenParser.Tokens.LABEL || token.TokenName == TokenParser.Tokens.COMMA) // LDA Indexed
                                {
                                    string indexLabel = ",";
                                    if (token.TokenValue != "A" && token.TokenValue != "a" && token.TokenValue != "B" &&
                                        token.TokenValue != "b" && token.TokenValue != ",")
                                    {
                                        Error(string.Format("Expected A or B register at line {0}!", lineNumber));
                                        return null;
                                    }
                                    byte ro = 0;
                                    if (token.TokenValue == "A" || token.TokenValue == "a")
                                    {
                                        ro = 1;
                                        token = tokenParser.GetToken();
                                        indexLabel = indexLabel.Insert(0, "A");
                                    }
                                    if (token.TokenValue == "B" || token.TokenValue == "b")
                                    {
                                        ro = 2;
                                        token = tokenParser.GetToken();
                                        indexLabel = indexLabel.Insert(0, "B");
                                    }
                                    if (token == null || token.TokenName != TokenParser.Tokens.COMMA)
                                    {
                                        Error(string.Format("Expected ',' at line {0}!", lineNumber));
                                        return null;
                                    }
                                    byte r = 0;
                                    token = tokenParser.GetToken();
                                    if (token == null || token.TokenName != TokenParser.Tokens.LABEL)
                                    {
                                        Error(string.Format("Expected register at line {0}!", lineNumber));
                                        return null;
                                    }
                                    if (token.TokenValue.ToUpper() != "X" && token.TokenValue.ToUpper() != "Y" &&
                                        token.TokenValue.ToUpper() != "D")
                                    {
                                        Error(string.Format("Expected 16-bit register name at line {0}!", lineNumber));
                                        return null;
                                    }
                                    indexLabel = indexLabel + token.TokenValue.ToUpper();
                                    if (token.TokenValue.ToUpper() == "X")
                                    {
                                        r = 8;
                                    }
                                    if (token.TokenValue.ToUpper() == "Y")
                                    {
                                        r = 16;
                                    }
                                    if (token.TokenValue.ToUpper() == "D")
                                    {
                                        r = 4;
                                    }
                                    if (tokenParser.Peek().TokenPeek.TokenName == TokenParser.Tokens.PLUS ||
                                        tokenParser.Peek().TokenPeek.TokenName == TokenParser.Tokens.MINUS ||
                                        tokenParser.Peek().TokenPeek.TokenName == TokenParser.Tokens.DOUBLEMINUS ||
                                        tokenParser.Peek().TokenPeek.TokenName == TokenParser.Tokens.DOUBLEPLUS)
                                    {
                                        token = tokenParser.GetToken();
                                        indexLabel = indexLabel + token.TokenValue;
                                        if (token.TokenName == TokenParser.Tokens.PLUS)
                                        {
                                            r = (byte) (r + 32);
                                            indexLabel = indexLabel + "+";
                                        }
                                        if (token.TokenName == TokenParser.Tokens.DOUBLEPLUS)
                                        {
                                            r = (byte)(r + 32);
                                            r = (byte)(r + 128);
                                            indexLabel = indexLabel + "++";
                                        }
                                        if (token.TokenName == TokenParser.Tokens.MINUS)
                                        {
                                            r = (byte)(r + 64);
                                            indexLabel = indexLabel + "-";
                                        }
                                        if (token.TokenName == TokenParser.Tokens.DOUBLEMINUS)
                                        {
                                            r = (byte)(r + 64);
                                            r = (byte)(r + 128);
                                            indexLabel = indexLabel + "-";
                                        }
                                    }
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x19);
                                        _programData.Add(ro);
                                        _programData.Add(r);
                                        
                                        _debugInfo.Add(new DebugData { Address = progLoc, SourceCodeLine = "    LDA    " + indexLabel });
                                    }
                                    progLoc = (ushort) (progLoc + 3);
                                    token = tokenParser.GetToken();
                                    while (token != null &&
                                           (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                            token.TokenName == TokenParser.Tokens.COMMENT))
                                    {
                                        token = tokenParser.GetToken();
                                    }
                                }
                                continue;
                            case TokenParser.Tokens.LDB:
                                token = tokenParser.GetToken();
                                if (token == null)
                                {
                                    Error(string.Format("Unexpected End of File at line {0}!", lineNumber));
                                    return null;
                                }
                                if (token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected Whitespace at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null)
                                {
                                    Error(string.Format("Unexpected End of File at line {0}!", lineNumber));
                                    return null;
                                }
                                if (token.TokenName == TokenParser.Tokens.POUND) // LDB Immediate
                                {
                                    token = tokenParser.GetToken();
                                    if (token == null)
                                    {
                                        Error(string.Format("Unexpected End of File at line {0}!", lineNumber));
                                        return null;
                                    }
                                    if (token.TokenName != TokenParser.Tokens.NUMBER &&
                                        token.TokenName != TokenParser.Tokens.HEXNUMBER &&
                                        token.TokenName != TokenParser.Tokens.BINARYNUMBER)
                                    {
                                        Error(string.Format("Expected number at line {0}!", lineNumber));
                                        return null;
                                    }
                                    byte b = Get8BitNumber(token);
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x02);
                                        _programData.Add(b);
                                        
                                        _debugInfo.Add(new DebugData { Address = progLoc, SourceCodeLine = "    LDB    #$" + b.ToString("X2") });
                                    }
                                    progLoc = (ushort)(progLoc + 2);
                                    token = tokenParser.GetToken();
                                    while (token != null &&
                                           (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                            token.TokenName == TokenParser.Tokens.COMMENT))
                                    {
                                        token = tokenParser.GetToken();
                                    }
                                }
                                else if (token.TokenName == TokenParser.Tokens.NUMBER ||
                                         token.TokenName == TokenParser.Tokens.HEXNUMBER ||
                                         token.TokenName == TokenParser.Tokens.BINARYNUMBER) // LDB Extended
                                {
                                    ushort s = Get16BitNumber(token);
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x0C);
                                        _programData.Add(BitConverter.GetBytes(s)[0]);
                                        _programData.Add(BitConverter.GetBytes(s)[1]);
                                        _debugInfo.Add(new DebugData { Address = progLoc, SourceCodeLine = "    LDB    $" + s.ToString("X4") });
                                    }
                                    progLoc = (ushort)(progLoc + 3);
                                    token = tokenParser.GetToken();
                                    while (token != null &&
                                           (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                            token.TokenName == TokenParser.Tokens.COMMENT))
                                    {
                                        token = tokenParser.GetToken();
                                    }
                                }
                                else if (token.TokenName == TokenParser.Tokens.LABEL || token.TokenName == TokenParser.Tokens.COMMA) // LDB Indexed
                                {
                                    string indexLabel = ",";
                                    if (token.TokenValue != "A" && token.TokenValue != "a" && token.TokenValue != "B" &&
                                        token.TokenValue != "b" && token.TokenValue != ",")
                                    {
                                        Error(string.Format("Expected A or B register at line {0}!", lineNumber));
                                        return null;
                                    }
                                    byte ro = 0;
                                    if (token.TokenValue == "A" || token.TokenValue == "a")
                                    {
                                        ro = 1;
                                        token = tokenParser.GetToken();
                                        indexLabel = indexLabel.Insert(0, "A");
                                    }
                                    if (token.TokenValue == "B" || token.TokenValue == "b")
                                    {
                                        ro = 2;
                                        token = tokenParser.GetToken();
                                        indexLabel = indexLabel.Insert(0, "B");
                                    }
                                    if (token == null || token.TokenName != TokenParser.Tokens.COMMA)
                                    {
                                        Error(string.Format("Expected ',' at line {0}!", lineNumber));
                                        return null;
                                    }
                                    byte r = 0;
                                    token = tokenParser.GetToken();
                                    if (token == null || token.TokenName != TokenParser.Tokens.LABEL)
                                    {
                                        Error(string.Format("Expected register at line {0}!", lineNumber));
                                        return null;
                                    }
                                    if (token.TokenValue.ToUpper() != "X" && token.TokenValue.ToUpper() != "Y" &&
                                        token.TokenValue.ToUpper() != "D")
                                    {
                                        Error(string.Format("Expected 16-bit register name at line {0}!", lineNumber));
                                        return null;
                                    }
                                    indexLabel = indexLabel + token.TokenValue.ToUpper();
                                    if (token.TokenValue.ToUpper() == "X")
                                    {
                                        r = 8;
                                    }
                                    if (token.TokenValue.ToUpper() == "Y")
                                    {
                                        r = 16;
                                    }
                                    if (token.TokenValue.ToUpper() == "D")
                                    {
                                        r = 4;
                                    }
                                    if (tokenParser.Peek().TokenPeek.TokenName == TokenParser.Tokens.PLUS ||
                                        tokenParser.Peek().TokenPeek.TokenName == TokenParser.Tokens.MINUS ||
                                        tokenParser.Peek().TokenPeek.TokenName == TokenParser.Tokens.DOUBLEMINUS ||
                                        tokenParser.Peek().TokenPeek.TokenName == TokenParser.Tokens.DOUBLEPLUS)
                                    {
                                        token = tokenParser.GetToken();
                                        indexLabel = indexLabel + token.TokenValue;
                                        if (token.TokenName == TokenParser.Tokens.PLUS)
                                        {
                                            r = (byte)(r + 32);
                                            indexLabel = indexLabel + "+";
                                        }
                                        if (token.TokenName == TokenParser.Tokens.DOUBLEPLUS)
                                        {
                                            r = (byte)(r + 32);
                                            r = (byte)(r + 128);
                                            indexLabel = indexLabel + "++";
                                        }
                                        if (token.TokenName == TokenParser.Tokens.MINUS)
                                        {
                                            r = (byte)(r + 64);
                                            indexLabel = indexLabel + "-";
                                        }
                                        if (token.TokenName == TokenParser.Tokens.DOUBLEMINUS)
                                        {
                                            r = (byte)(r + 64);
                                            r = (byte)(r + 128);
                                            indexLabel = indexLabel + "--";
                                        }
                                    }
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x1A);
                                        _programData.Add(ro);
                                        _programData.Add(r);

                                        _debugInfo.Add(new DebugData { Address = progLoc, SourceCodeLine = "    LDB    " + indexLabel });
                                    }
                                    progLoc = (ushort)(progLoc + 3);
                                    token = tokenParser.GetToken();
                                    while (token != null &&
                                           (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                            token.TokenName == TokenParser.Tokens.COMMENT))
                                    {
                                        token = tokenParser.GetToken();
                                    }
                                }
                                continue;
                            case TokenParser.Tokens.END:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected white space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.LABEL)
                                {
                                    Error(string.Format("Expected label at line {0}!", lineNumber));
                                    return null;
                                }
                                
                                if (!_labelDictionary.ContainsKey(token.TokenValue.ToUpper()))
                                {
                                    Error(string.Format("Undefined Label \"{0}\" at line {1}!", token.TokenValue.ToUpper(), lineNumber));
                                    return null;
                                }
                                executionAddress = _labelDictionary[token.TokenValue.ToUpper()];
                                if (phase == 2)
                                {
                                    _programData.Add(0x00);
                                    
                                    _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    END    $" + executionAddress.ToString("X4")
                                        });
                                }
                                progLoc = (ushort) (progLoc + 1);
                                token = tokenParser.GetToken();
                                    while (token != null &&
                                           (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                            token.TokenName == TokenParser.Tokens.COMMENT))
                                    {
                                        token = tokenParser.GetToken();
                                    }
                                continue;
                            case TokenParser.Tokens.STA:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected white space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token.TokenName == TokenParser.Tokens.NUMBER ||
                                    token.TokenName == TokenParser.Tokens.HEXNUMBER ||
                                    token.TokenName == TokenParser.Tokens.BINARYNUMBER ||
                                    token.TokenName == TokenParser.Tokens.POUND) // STA extended
                                {
                                    ushort addr = 0;
                                    if (token.TokenName == TokenParser.Tokens.POUND)
                                    {
                                        token = tokenParser.GetToken();
                                        if (token.TokenName == TokenParser.Tokens.LABEL)
                                        {
                                            if (phase == 2)
                                            {
                                                if (!_labelDictionary.ContainsKey(token.TokenValue.ToUpper()))
                                                {
                                                    Error(string.Format("Undefined label \"{0}\" at line {1}!",
                                                                        token.TokenValue.ToUpper(), lineNumber));
                                                    return null;
                                                }
                                                addr = _labelDictionary[token.TokenValue.ToUpper()];
                                            }
                                        }
                                        else
                                        {
                                            addr = Get16BitNumber(token);
                                        }
                                    }
                                    else
                                    {
                                        addr = Get16BitNumber(token);
                                    }
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x06);
                                        _programData.Add(BitConverter.GetBytes(addr)[0]);
                                        _programData.Add(BitConverter.GetBytes(addr)[1]);

                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    STA    $" + addr.ToString("X4")
                                        });
                                    }
                                    progLoc = (ushort) (progLoc + 3);
                                }
                                else if (token.TokenName == TokenParser.Tokens.LABEL || token.TokenName == TokenParser.Tokens.COMMA) // STA Indexed
                                {
                                    string indexString = ",";
                                    if (token.TokenValue != "A" && token.TokenValue != "a" && token.TokenValue != "B" &&
                                       token.TokenValue != "b" && token.TokenValue != ",")
                                    {
                                        Error(string.Format("Expected A or B register at line {0}!", lineNumber));
                                        return null;
                                    }
                                    byte ro = 0;
                                    if (token.TokenValue == "A" || token.TokenValue == "a")
                                    {
                                        ro = 1;
                                        token = tokenParser.GetToken();
                                        indexString = indexString.Insert(0, "A");
                                    }
                                    if (token.TokenValue == "B" || token.TokenValue == "b")
                                    {
                                        ro = 2;
                                        token = tokenParser.GetToken();
                                        indexString = indexString.Insert(0, "B");
                                    }
                                    if (token == null || token.TokenName != TokenParser.Tokens.COMMA)
                                    {
                                        Error(string.Format("Expected ',' at line {0}!", lineNumber));
                                        return null;
                                    }
                                    byte r = 0;
                                    token = tokenParser.GetToken();
                                    if (token == null || token.TokenName != TokenParser.Tokens.LABEL)
                                    {
                                        Error(string.Format("Expected register at line {0}!", lineNumber));
                                        return null;
                                    }
                                    if (token.TokenValue.ToUpper() != "X" && token.TokenValue.ToUpper() != "Y" &&
                                        token.TokenValue.ToUpper() != "D")
                                    {
                                        Error(string.Format("Expected 16-bit register name at line {0}!", lineNumber));
                                        return null;
                                    }
                                    indexString = indexString + token.TokenValue.ToUpper();
                                    if (token.TokenValue.ToUpper() == "X")
                                    {
                                        r = 8;
                                    }
                                    if (token.TokenValue.ToUpper() == "Y")
                                    {
                                        r = 16;
                                    }
                                    if (token.TokenValue.ToUpper() == "D")
                                    {
                                        r = 4;
                                    }
                                    if (tokenParser.Peek().TokenPeek.TokenName == TokenParser.Tokens.PLUS ||
                                        tokenParser.Peek().TokenPeek.TokenName == TokenParser.Tokens.MINUS ||
                                        tokenParser.Peek().TokenPeek.TokenName == TokenParser.Tokens.DOUBLEMINUS ||
                                        tokenParser.Peek().TokenPeek.TokenName == TokenParser.Tokens.DOUBLEPLUS)
                                    {
                                        token = tokenParser.GetToken();
                                        indexString = indexString + token.TokenValue;
                                        if (token.TokenName == TokenParser.Tokens.PLUS)
                                        {
                                            r = (byte)(r + 32);
                                            indexString = indexString + "+";
                                        }
                                        if (token.TokenName == TokenParser.Tokens.DOUBLEPLUS)
                                        {
                                            r = (byte)(r + 32);
                                            r = (byte)(r + 128);
                                            indexString = indexString + "++";
                                        }
                                        if (token.TokenName == TokenParser.Tokens.MINUS)
                                        {
                                            r = (byte)(r + 64);
                                            indexString = indexString + "-";
                                        }
                                        if (token.TokenName == TokenParser.Tokens.DOUBLEMINUS)
                                        {
                                            r = (byte)(r + 64);
                                            r = (byte)(r + 128);
                                            indexString = indexString + "--";
                                        }
                                    }
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x10);
                                        _programData.Add(ro);
                                        _programData.Add(r);
                                        
                                        _debugInfo.Add(new DebugData
                                            {
                                                Address = progLoc,
                                                SourceCodeLine = "    STA    " + indexString
                                            });
                                    }
                                    progLoc = (ushort)(progLoc + 3);
                                }
                                 token = tokenParser.GetToken();
                                    while (token != null &&
                                           (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                            token.TokenName == TokenParser.Tokens.COMMENT))
                                    {
                                        token = tokenParser.GetToken();
                                    }
                                continue;
                            case TokenParser.Tokens.STB:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected white space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token.TokenName == TokenParser.Tokens.NUMBER ||
                                    token.TokenName == TokenParser.Tokens.HEXNUMBER ||
                                    token.TokenName == TokenParser.Tokens.BINARYNUMBER ||
                                    token.TokenName == TokenParser.Tokens.POUND) // STB extended
                                {
                                    ushort addr = 0;
                                    if (token.TokenName == TokenParser.Tokens.POUND)
                                    {
                                        token = tokenParser.GetToken();
                                        if (token.TokenName == TokenParser.Tokens.LABEL)
                                        {
                                            if (phase == 2)
                                            {
                                                if (!_labelDictionary.ContainsKey(token.TokenValue.ToUpper()))
                                                {
                                                    Error(string.Format("Undefined label \"{0}\" at line {1}!",
                                                                        token.TokenValue.ToUpper(), lineNumber));
                                                    return null;
                                                }
                                                addr = _labelDictionary[token.TokenValue.ToUpper()];
                                            }
                                        }
                                        else
                                        {
                                            addr = Get16BitNumber(token);
                                        }
                                    }
                                    else
                                    {
                                        addr = Get16BitNumber(token);
                                    }
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x07);
                                        _programData.Add(BitConverter.GetBytes(addr)[0]);
                                        _programData.Add(BitConverter.GetBytes(addr)[1]);

                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    STB    $" + addr.ToString("X4")
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 3);
                                }
                                else if (token.TokenName == TokenParser.Tokens.LABEL || token.TokenName == TokenParser.Tokens.COMMA) // STB Indexed
                                {
                                    string indexString = ",";
                                    if (token.TokenValue != "A" && token.TokenValue != "a" && token.TokenValue != "B" &&
                                       token.TokenValue != "b" && token.TokenValue != ",")
                                    {
                                        Error(string.Format("Expected A or B register at line {0}!", lineNumber));
                                        return null;
                                    }
                                    byte ro = 0;
                                    if (token.TokenValue == "A" || token.TokenValue == "a")
                                    {
                                        ro = 1;
                                        token = tokenParser.GetToken();
                                        indexString = indexString.Insert(0, "A");
                                    }
                                    if (token.TokenValue == "B" || token.TokenValue == "b")
                                    {
                                        ro = 2;
                                        token = tokenParser.GetToken();
                                        indexString = indexString.Insert(0, "B");
                                    }
                                    if (token == null || token.TokenName != TokenParser.Tokens.COMMA)
                                    {
                                        Error(string.Format("Expected ',' at line {0}!", lineNumber));
                                        return null;
                                    }
                                    byte r = 0;
                                    token = tokenParser.GetToken();
                                    if (token == null || token.TokenName != TokenParser.Tokens.LABEL)
                                    {
                                        Error(string.Format("Expected register at line {0}!", lineNumber));
                                        return null;
                                    }
                                    if (token.TokenValue.ToUpper() != "X" && token.TokenValue.ToUpper() != "Y" &&
                                        token.TokenValue.ToUpper() != "D")
                                    {
                                        Error(string.Format("Expected 16-bit register name at line {0}!", lineNumber));
                                        return null;
                                    }
                                    indexString = indexString + token.TokenValue.ToUpper();
                                    if (token.TokenValue.ToUpper() == "X")
                                    {
                                        r = 8;
                                    }
                                    if (token.TokenValue.ToUpper() == "Y")
                                    {
                                        r = 16;
                                    }
                                    if (token.TokenValue.ToUpper() == "D")
                                    {
                                        r = 4;
                                    }
                                    if (tokenParser.Peek().TokenPeek.TokenName == TokenParser.Tokens.PLUS ||
                                        tokenParser.Peek().TokenPeek.TokenName == TokenParser.Tokens.MINUS ||
                                        tokenParser.Peek().TokenPeek.TokenName == TokenParser.Tokens.DOUBLEMINUS ||
                                        tokenParser.Peek().TokenPeek.TokenName == TokenParser.Tokens.DOUBLEPLUS)
                                    {
                                        token = tokenParser.GetToken();
                                        indexString = indexString + token.TokenValue;
                                        if (token.TokenName == TokenParser.Tokens.PLUS)
                                        {
                                            r = (byte)(r + 32);
                                            indexString = indexString + "+";
                                        }
                                        if (token.TokenName == TokenParser.Tokens.DOUBLEPLUS)
                                        {
                                            r = (byte)(r + 32);
                                            r = (byte)(r + 128);
                                            indexString = indexString + "++";
                                        }
                                        if (token.TokenName == TokenParser.Tokens.MINUS)
                                        {
                                            r = (byte)(r + 64);
                                            indexString = indexString + "-";
                                        }
                                        if (token.TokenName == TokenParser.Tokens.DOUBLEMINUS)
                                        {
                                            r = (byte)(r + 64);
                                            r = (byte)(r + 128);
                                            indexString = indexString + "--";
                                        }
                                    }
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x11);
                                        _programData.Add(ro);
                                        _programData.Add(r);

                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    STB    " + indexString
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 3);
                                }
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                                
                            case TokenParser.Tokens.STD:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected white space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token.TokenName == TokenParser.Tokens.NUMBER ||
                                    token.TokenName == TokenParser.Tokens.HEXNUMBER ||
                                    token.TokenName == TokenParser.Tokens.BINARYNUMBER ||
                                    token.TokenName == TokenParser.Tokens.POUND) // STD extended
                                {
                                    ushort addr = 0;
                                    if (token.TokenName == TokenParser.Tokens.POUND)
                                    {
                                        token = tokenParser.GetToken();
                                        if (token.TokenName == TokenParser.Tokens.LABEL)
                                        {
                                            if (phase == 2)
                                            {
                                                if (!_labelDictionary.ContainsKey(token.TokenValue.ToUpper()))
                                                {
                                                    Error(string.Format("Undefined label \"{0}\" at line {1}!",
                                                                        token.TokenValue.ToUpper(), lineNumber));
                                                    return null;
                                                }
                                                addr = _labelDictionary[token.TokenValue.ToUpper()];
                                            }
                                        }
                                        else
                                        {
                                            addr = Get16BitNumber(token);
                                        }
                                    }
                                    else
                                    {
                                        addr = Get16BitNumber(token);
                                    }
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x0A);
                                        _programData.Add(BitConverter.GetBytes(addr)[0]);
                                        _programData.Add(BitConverter.GetBytes(addr)[1]);

                                        _debugInfo.Add(new DebugData
                                            {
                                                Address = progLoc,
                                                SourceCodeLine = "    STD    $" + addr.ToString("X4")
                                            });
                                    }
                                    progLoc = (ushort)(progLoc + 3);
                                }
                                
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.STX:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected white space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token.TokenName == TokenParser.Tokens.NUMBER ||
                                    token.TokenName == TokenParser.Tokens.HEXNUMBER ||
                                    token.TokenName == TokenParser.Tokens.BINARYNUMBER ||
                                    token.TokenName == TokenParser.Tokens.POUND) // STX extended
                                {
                                    ushort addr = 0;
                                    if (token.TokenName == TokenParser.Tokens.POUND)
                                    {
                                        token = tokenParser.GetToken();
                                        if (token.TokenName == TokenParser.Tokens.LABEL)
                                        {
                                            if (phase == 2)
                                            {
                                                if (!_labelDictionary.ContainsKey(token.TokenValue.ToUpper()))
                                                {
                                                    Error(string.Format("Undefined label \"{0}\" at line {1}!",
                                                                        token.TokenValue.ToUpper(), lineNumber));
                                                    return null;
                                                }
                                                addr = _labelDictionary[token.TokenValue.ToUpper()];
                                            }
                                        }
                                        else
                                        {
                                            addr = Get16BitNumber(token);
                                        }
                                    }
                                    else
                                    {
                                        addr = Get16BitNumber(token);
                                    }
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x08);
                                        _programData.Add(BitConverter.GetBytes(addr)[0]);
                                        _programData.Add(BitConverter.GetBytes(addr)[1]);

                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    STX    $" + addr.ToString("X4")
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 3);
                                }

                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.STY:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected white space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token.TokenName == TokenParser.Tokens.NUMBER ||
                                    token.TokenName == TokenParser.Tokens.HEXNUMBER ||
                                    token.TokenName == TokenParser.Tokens.BINARYNUMBER ||
                                    token.TokenName == TokenParser.Tokens.POUND) // STY extended
                                {
                                    ushort addr = 0;
                                    if (token.TokenName == TokenParser.Tokens.POUND)
                                    {
                                        token = tokenParser.GetToken();
                                        if (token.TokenName == TokenParser.Tokens.LABEL)
                                        {
                                            if (phase == 2)
                                            {
                                                if (!_labelDictionary.ContainsKey(token.TokenValue.ToUpper()))
                                                {
                                                    Error(string.Format("Undefined label \"{0}\" at line {1}!",
                                                                        token.TokenValue.ToUpper(), lineNumber));
                                                    return null;
                                                }
                                                addr = _labelDictionary[token.TokenValue.ToUpper()];
                                            }
                                        }
                                        else
                                        {
                                            addr = Get16BitNumber(token);
                                        }
                                    }
                                    else
                                    {
                                        addr = Get16BitNumber(token);
                                    }
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x09);
                                        _programData.Add(BitConverter.GetBytes(addr)[0]);
                                        _programData.Add(BitConverter.GetBytes(addr)[1]);

                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    STY    $" + addr.ToString("X4")
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 3);
                                }

                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.LDX:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected white space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token.TokenName == TokenParser.Tokens.POUND) // LDX Immediate
                                {
                                    ushort addr = 0;
                                    if (token.TokenName == TokenParser.Tokens.POUND)
                                    {
                                        token = tokenParser.GetToken();
                                        if (token == null)
                                        {
                                            Error(string.Format("Expected number or label at line {0}!", lineNumber));
                                            return null;
                                        }
                                        if (token.TokenName != TokenParser.Tokens.NUMBER &&
                                            token.TokenName != TokenParser.Tokens.HEXNUMBER &&
                                            token.TokenName != TokenParser.Tokens.BINARYNUMBER &&
                                            token.TokenName != TokenParser.Tokens.LABEL)
                                        {
                                            Error(string.Format("Expected number or label at line {0}!", lineNumber));
                                            return null;
                                        }

                                        if (token.TokenName == TokenParser.Tokens.LABEL)
                                        {
                                            if (!_labelDictionary.ContainsKey(token.TokenValue.ToUpper()))
                                            {
                                                Error(string.Format("No such label \"{0}\" at line {1}!",
                                                                    token.TokenValue.ToUpper(), lineNumber));
                                                return null;
                                            }
                                            addr = _labelDictionary[token.TokenValue.ToUpper()];
                                        }
                                        else
                                        {
                                            addr = Get16BitNumber(token);
                                        }
                                    }
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x03);
                                        _programData.Add(BitConverter.GetBytes(addr)[0]);
                                        _programData.Add(BitConverter.GetBytes(addr)[1]);

                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    LDX    #$" + addr.ToString("X4")
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 3);
                                }
                                else if (token.TokenName == TokenParser.Tokens.NUMBER ||
                                         token.TokenName == TokenParser.Tokens.HEXNUMBER ||
                                         token.TokenName == TokenParser.Tokens.BINARYNUMBER || token.TokenName == TokenParser.Tokens.LABEL) // LDX Extended
                                {
                                    ushort addr = 0;
                                    if (token.TokenName == TokenParser.Tokens.LABEL) {
                                        if (phase == 2)
                                        {
                                            if (!_labelDictionary.ContainsKey(token.TokenValue.ToUpper()))
                                            {
                                                Error(string.Format("No such label \"{0}\" at line {1}", token.TokenValue.ToUpper(), lineNumber));
                                                return null;
                                            }
                                            addr = _labelDictionary[token.TokenValue.ToUpper()];
                                        }
                                    } else addr = Get16BitNumber(token);
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x0D);
                                        _programData.Add(BitConverter.GetBytes(addr)[0]);
                                        _programData.Add(BitConverter.GetBytes(addr)[1]);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    LDX    $" + addr.ToString("X4")
                                        });
                                    }
                                    progLoc = (ushort) (progLoc + 3);
                                }
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.LDY:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected white space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token.TokenName == TokenParser.Tokens.POUND) // LDY Immediate
                                {
                                    ushort addr = 0;
                                    if (token.TokenName == TokenParser.Tokens.POUND)
                                    {
                                        token = tokenParser.GetToken();
                                        if (token == null)
                                        {
                                            Error(string.Format("Expected number or label at line {0}!", lineNumber));
                                            return null;
                                        }
                                        if (token.TokenName != TokenParser.Tokens.NUMBER &&
                                            token.TokenName != TokenParser.Tokens.HEXNUMBER &&
                                            token.TokenName != TokenParser.Tokens.BINARYNUMBER &&
                                            token.TokenName != TokenParser.Tokens.LABEL)
                                        {
                                            Error(string.Format("Expected number or label at line {0}!", lineNumber));
                                            return null;
                                        }

                                        if (token.TokenName == TokenParser.Tokens.LABEL)
                                        {
                                            if (!_labelDictionary.ContainsKey(token.TokenValue.ToUpper()))
                                            {
                                                Error(string.Format("No such label \"{0}\" at line {1}!",
                                                                    token.TokenValue.ToUpper(), lineNumber));
                                                return null;
                                            }
                                            addr = _labelDictionary[token.TokenValue.ToUpper()];
                                        }
                                        else
                                        {
                                            addr = Get16BitNumber(token);
                                        }
                                    }
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x04);
                                        _programData.Add(BitConverter.GetBytes(addr)[0]);
                                        _programData.Add(BitConverter.GetBytes(addr)[1]);

                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    LDY    #$" + addr.ToString("X4")
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 3);
                                }
                                else if (token.TokenName == TokenParser.Tokens.NUMBER ||
                                         token.TokenName == TokenParser.Tokens.HEXNUMBER ||
                                         token.TokenName == TokenParser.Tokens.BINARYNUMBER || token.TokenName == TokenParser.Tokens.LABEL) // LDY Extended
                                {
                                    ushort addr = 0;
                                    if (token.TokenName == TokenParser.Tokens.LABEL) {
                                        if (phase == 2)
                                        {
                                            if (!_labelDictionary.ContainsKey(token.TokenValue.ToUpper()))
                                            {
                                                Error(string.Format("No such label \"{0}\" at line {1}", token.TokenValue.ToUpper(), lineNumber));
                                                return null;
                                            }
                                            addr = _labelDictionary[token.TokenValue.ToUpper()];
                                        }
                                    } else addr = Get16BitNumber(token);
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x0E);
                                        _programData.Add(BitConverter.GetBytes(addr)[0]);
                                        _programData.Add(BitConverter.GetBytes(addr)[1]);

                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    LDY    $" + addr.ToString("X4")
                                        });
                                    }
                                    progLoc = (ushort) (progLoc + 3);
                                }
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.LDD:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected white space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token.TokenName == TokenParser.Tokens.POUND) // LDD Immediate
                                {
                                    ushort addr = 0;
                                    if (token.TokenName == TokenParser.Tokens.POUND)
                                    {
                                        token = tokenParser.GetToken();
                                        if (token == null)
                                        {
                                            Error(string.Format("Expected number or label at line {0}!", lineNumber));
                                            return null;
                                        }
                                        if (token.TokenName != TokenParser.Tokens.NUMBER &&
                                            token.TokenName != TokenParser.Tokens.HEXNUMBER &&
                                            token.TokenName != TokenParser.Tokens.BINARYNUMBER &&
                                            token.TokenName != TokenParser.Tokens.LABEL)
                                        {
                                            Error(string.Format("Expected number or label at line {0}!", lineNumber));
                                            return null;
                                        }

                                        if (token.TokenName == TokenParser.Tokens.LABEL)
                                        {
                                            if (!_labelDictionary.ContainsKey(token.TokenValue.ToUpper()))
                                            {
                                                Error(string.Format("No such label \"{0}\" at line {1}!",
                                                                    token.TokenValue.ToUpper(), lineNumber));
                                                return null;
                                            }
                                            addr = _labelDictionary[token.TokenValue.ToUpper()];
                                        }
                                        else
                                        {
                                            addr = Get16BitNumber(token);
                                        }
                                    }
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x05);
                                        _programData.Add(BitConverter.GetBytes(addr)[0]);
                                        _programData.Add(BitConverter.GetBytes(addr)[1]);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    LDD    #$" + addr.ToString("X4")
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 3);
                                }
                                else if (token.TokenName == TokenParser.Tokens.NUMBER ||
                                         token.TokenName == TokenParser.Tokens.HEXNUMBER ||
                                         token.TokenName == TokenParser.Tokens.BINARYNUMBER || token.TokenName == TokenParser.Tokens.LABEL) // LDD Extended
                                {
                                    ushort addr = 0;
                                    if (token.TokenName == TokenParser.Tokens.LABEL) {
                                        if (phase == 2)
                                        {
                                            if (!_labelDictionary.ContainsKey(token.TokenValue.ToUpper()))
                                            {
                                                Error(string.Format("No such label \"{0}\" at line {1}", token.TokenValue.ToUpper(), lineNumber));
                                                return null;
                                            }
                                            addr = _labelDictionary[token.TokenValue.ToUpper()];
                                        }
                                    } else addr = Get16BitNumber(token);
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x0F);
                                        _programData.Add(BitConverter.GetBytes(addr)[0]);
                                        _programData.Add(BitConverter.GetBytes(addr)[1]);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    LDD    $" + addr.ToString("X4")
                                        });
                                    }
                                    progLoc = (ushort) (progLoc + 3);
                                }
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.CMPA:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected whitespace at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || (token.TokenName != TokenParser.Tokens.POUND && token.TokenName != TokenParser.Tokens.LABEL))
                                {
                                    Error(string.Format("Compares require immediate value (missing '#' sign) or register at line {0}!", lineNumber));
                                    return null;
                                }
                                if (token.TokenName == TokenParser.Tokens.POUND)
                                {
                                    token = tokenParser.GetToken();
                                    if (token == null)
                                    {
                                        Error(string.Format("Expected number at line {0}!", lineNumber));
                                        return null;
                                    }
                                    if (token.TokenName != TokenParser.Tokens.NUMBER &&
                                        token.TokenName != TokenParser.Tokens.HEXNUMBER &&
                                        token.TokenName != TokenParser.Tokens.BINARYNUMBER)
                                    {
                                        Error(string.Format("Expected number at line {0}!", lineNumber));
                                        return null;
                                    }
                                    byte num = Get8BitNumber(token);
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x12);
                                        _programData.Add(num);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    CMPA    #$" + num.ToString("X2")
                                        });
                                    }
                                    progLoc = (ushort) (progLoc + 2);
                                } else if (token.TokenName == TokenParser.Tokens.LABEL)
                                {
                                    byte r = 0;
                                    if (token.TokenValue.ToUpper() == "A")
                                    {
                                        r = 1;
                                    }
                                    if (token.TokenValue.ToUpper() == "B")
                                    {
                                        r = 2;
                                    }
                                    if (token.TokenValue.ToUpper() == "D")
                                    {
                                        r = 4;
                                    }
                                    if (token.TokenValue.ToUpper() == "X")
                                    {
                                        r = 8;
                                    }
                                    if (token.TokenValue.ToUpper() == "Y")
                                    {
                                        r = 16;
                                    }
                                    if (r == 0)
                                    {
                                        Error(string.Format("Expected register at line {0}", lineNumber));
                                        return null;
                                    }
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x3F);
                                        _programData.Add(r);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    CMPA    " + token.TokenValue.ToUpper()
                                        });
                                    }
                                    progLoc = (ushort) (progLoc + 2);
                                }
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.CMPB:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected whitespace at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || (token.TokenName != TokenParser.Tokens.POUND && token.TokenName != TokenParser.Tokens.LABEL))
                                {
                                    Error(string.Format("Compares require immediate value (missing '#' sign) or register at line {0}!", lineNumber));
                                    return null;
                                }
                                if (token.TokenName == TokenParser.Tokens.POUND)
                                {
                                    token = tokenParser.GetToken();
                                    if (token == null)
                                    {
                                        Error(string.Format("Expected number at line {0}!", lineNumber));
                                        return null;
                                    }
                                    if (token.TokenName != TokenParser.Tokens.NUMBER &&
                                        token.TokenName != TokenParser.Tokens.HEXNUMBER &&
                                        token.TokenName != TokenParser.Tokens.BINARYNUMBER)
                                    {
                                        Error(string.Format("Expected number at line {0}!", lineNumber));
                                        return null;
                                    }
                                    byte num2 = Get8BitNumber(token);
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x13);
                                        _programData.Add(num2);

                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    CMPB    #$" + num2.ToString("X2")
                                        });
                                    }
                                    progLoc = (ushort) (progLoc + 2);
                                }
                                else if (token.TokenName == TokenParser.Tokens.LABEL)
                                {
                                    byte r = 0;
                                    if (token.TokenValue.ToUpper() == "A")
                                    {
                                        r = 1;
                                    }
                                    if (token.TokenValue.ToUpper() == "B")
                                    {
                                        r = 2;
                                    }
                                    if (token.TokenValue.ToUpper() == "D")
                                    {
                                        r = 4;
                                    }
                                    if (token.TokenValue.ToUpper() == "X")
                                    {
                                        r = 8;
                                    }
                                    if (token.TokenValue.ToUpper() == "Y")
                                    {
                                        r = 16;
                                    }
                                    if (r == 0)
                                    {
                                        Error(string.Format("Expected register at line {0}", lineNumber));
                                        return null;
                                    }
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x40);
                                        _programData.Add(r);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    CMPB    " + token.TokenValue.ToUpper()
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 2);
                                }

                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.CMPD:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected whitespace at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || (token.TokenName != TokenParser.Tokens.POUND && token.TokenName != TokenParser.Tokens.LABEL))
                                {
                                    Error(string.Format("Compares require immediate value (missing '#' sign) or register at line {0}!", lineNumber));
                                    return null;
                                }
                                if (token.TokenName == TokenParser.Tokens.POUND)
                                {
                                    token = tokenParser.GetToken();
                                    if (token == null)
                                    {
                                        Error(string.Format("Expected number at line {0}!", lineNumber));
                                        return null;
                                    }
                                    if (token.TokenName != TokenParser.Tokens.NUMBER &&
                                        token.TokenName != TokenParser.Tokens.HEXNUMBER &&
                                        token.TokenName != TokenParser.Tokens.BINARYNUMBER)
                                    {
                                        Error(string.Format("Expected number at line {0}!", lineNumber));
                                        return null;
                                    }
                                    ushort num3 = Get16BitNumber(token);
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x14);
                                        _programData.Add(BitConverter.GetBytes(num3)[0]);
                                        _programData.Add(BitConverter.GetBytes(num3)[1]);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    CMPD    #$" + num3.ToString("X4")
                                        });
                                    }
                                    progLoc = (ushort) (progLoc + 3);
                                }
                                else if (token.TokenName == TokenParser.Tokens.LABEL)
                                {
                                    byte r = 0;
                                    if (token.TokenValue.ToUpper() == "A")
                                    {
                                        r = 1;
                                    }
                                    if (token.TokenValue.ToUpper() == "B")
                                    {
                                        r = 2;
                                    }
                                    if (token.TokenValue.ToUpper() == "D")
                                    {
                                        r = 4;
                                    }
                                    if (token.TokenValue.ToUpper() == "X")
                                    {
                                        r = 8;
                                    }
                                    if (token.TokenValue.ToUpper() == "Y")
                                    {
                                        r = 16;
                                    }
                                    if (r == 0)
                                    {
                                        Error(string.Format("Expected register at line {0}", lineNumber));
                                        return null;
                                    }
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x41);
                                        _programData.Add(r);

                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    CMPD    " + token.TokenValue.ToUpper()
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 2);
                                }

                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.CMPX:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected whitespace at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || (token.TokenName != TokenParser.Tokens.POUND && token.TokenName != TokenParser.Tokens.LABEL))
                                {
                                    Error(string.Format("Compares require immediate value (missing '#' sign) or register at line {0}!", lineNumber));
                                    return null;
                                }
                                if (token.TokenName == TokenParser.Tokens.POUND)
                                {
                                    token = tokenParser.GetToken();
                                    if (token == null)
                                    {
                                        Error(string.Format("Expected number at line {0}!", lineNumber));
                                        return null;
                                    }
                                    if (token.TokenName != TokenParser.Tokens.NUMBER &&
                                        token.TokenName != TokenParser.Tokens.HEXNUMBER &&
                                        token.TokenName != TokenParser.Tokens.BINARYNUMBER)
                                    {
                                        Error(string.Format("Expected number at line {0}!", lineNumber));
                                        return null;
                                    }
                                    ushort num4 = Get16BitNumber(token);
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x15);
                                        _programData.Add(BitConverter.GetBytes(num4)[0]);
                                        _programData.Add(BitConverter.GetBytes(num4)[1]);

                                       _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    CMPX    #$" + num4.ToString("X4")
                                        });
                                    }
                                    progLoc = (ushort) (progLoc + 3);
                                }
                                else if (token.TokenName == TokenParser.Tokens.LABEL)
                                {
                                    byte r = 0;
                                    if (token.TokenValue.ToUpper() == "A")
                                    {
                                        r = 1;
                                    }
                                    if (token.TokenValue.ToUpper() == "B")
                                    {
                                        r = 2;
                                    }
                                    if (token.TokenValue.ToUpper() == "D")
                                    {
                                        r = 4;
                                    }
                                    if (token.TokenValue.ToUpper() == "X")
                                    {
                                        r = 8;
                                    }
                                    if (token.TokenValue.ToUpper() == "Y")
                                    {
                                        r = 16;
                                    }
                                    if (r == 0)
                                    {
                                        Error(string.Format("Expected register at line {0}", lineNumber));
                                        return null;
                                    }
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x42);
                                        _programData.Add(r);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    CMPX    " + token.TokenValue.ToUpper()
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 2);
                                }
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.CMPY:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected whitespace at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || (token.TokenName != TokenParser.Tokens.POUND && token.TokenName != TokenParser.Tokens.LABEL))
                                {
                                    Error(string.Format("Compares require immediate value (missing '#' sign) or register at line {0}!", lineNumber));
                                    return null;
                                }
                                if (token.TokenName == TokenParser.Tokens.POUND)
                                {
                                    token = tokenParser.GetToken();
                                    if (token == null)
                                    {
                                        Error(string.Format("Expected number at line {0}!", lineNumber));
                                        return null;
                                    }
                                    if (token.TokenName != TokenParser.Tokens.NUMBER &&
                                        token.TokenName != TokenParser.Tokens.HEXNUMBER &&
                                        token.TokenName != TokenParser.Tokens.BINARYNUMBER)
                                    {
                                        Error(string.Format("Expected number at line {0}!", lineNumber));
                                        return null;
                                    }
                                    ushort num5 = Get16BitNumber(token);
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x16);
                                        _programData.Add(BitConverter.GetBytes(num5)[0]);
                                        _programData.Add(BitConverter.GetBytes(num5)[1]);

                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    CMPY    #$" + num5.ToString("X4")
                                        });
                                    }
                                    progLoc = (ushort) (progLoc + 3);
                                }
                                else if (token.TokenName == TokenParser.Tokens.LABEL)
                                {
                                    byte r = 0;
                                    if (token.TokenValue.ToUpper() == "A")
                                    {
                                        r = 1;
                                    }
                                    if (token.TokenValue.ToUpper() == "B")
                                    {
                                        r = 2;
                                    }
                                    if (token.TokenValue.ToUpper() == "D")
                                    {
                                        r = 4;
                                    }
                                    if (token.TokenValue.ToUpper() == "X")
                                    {
                                        r = 8;
                                    }
                                    if (token.TokenValue.ToUpper() == "Y")
                                    {
                                        r = 16;
                                    }
                                    if (r == 0)
                                    {
                                        Error(string.Format("Expected register at line {0}", lineNumber));
                                        return null;
                                    }
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x43);
                                        _programData.Add(r);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    CMPY    " + token.TokenValue.ToUpper()
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 2);
                                }

                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.JLT:
                                 token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected whitespace at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.LABEL)
                                {
                                    Error(string.Format("All jumps require a label at line {0}", lineNumber));
                                    return null;
                                }
                                if (phase == 2)
                                {
                                    if (!_labelDictionary.ContainsKey(token.TokenValue.ToUpper()))
                                    {
                                        Error(string.Format("No such label \"{0}\" at line {1}!", token.TokenValue.ToUpper(), lineNumber));
                                        return null;
                                    }
                                    ushort addr2 = _labelDictionary[token.TokenValue.ToUpper()];
                                    _programData.Add(0x22);
                                    _programData.Add(BitConverter.GetBytes(addr2)[0]);
                                    _programData.Add(BitConverter.GetBytes(addr2)[1]);
                                    
                                    _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    JLT    $" + addr2.ToString("X4")
                                        });
                                }
                                progLoc = (ushort)(progLoc + 3);

                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.JLE:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected whitespace at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.LABEL)
                                {
                                    Error(string.Format("All jumps require a label at line {0}", lineNumber));
                                    return null;
                                }
                                if (phase == 2)
                                {
                                    if (!_labelDictionary.ContainsKey(token.TokenValue.ToUpper()))
                                    {
                                        Error(string.Format("No such label \"{0}\" at line {1}!", token.TokenValue.ToUpper(), lineNumber));
                                        return null;
                                    }
                                    ushort addr2 = _labelDictionary[token.TokenValue.ToUpper()];
                                    _programData.Add(0x21);
                                    _programData.Add(BitConverter.GetBytes(addr2)[0]);
                                    _programData.Add(BitConverter.GetBytes(addr2)[1]);
                                   
                                    _debugInfo.Add(new DebugData
                                    {
                                        Address = progLoc,
                                        SourceCodeLine = "    JLE    $" + addr2.ToString("X4")
                                    });
                                }
                                progLoc = (ushort)(progLoc + 3);

                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.JGT:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected whitespace at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.LABEL)
                                {
                                    Error(string.Format("All jumps require a label at line {0}", lineNumber));
                                    return null;
                                }
                                if (phase == 2)
                                {
                                    if (!_labelDictionary.ContainsKey(token.TokenValue.ToUpper()))
                                    {
                                        Error(string.Format("No such label \"{0}\" at line {1}!", token.TokenValue.ToUpper(), lineNumber));
                                        return null;
                                    }
                                    ushort addr2 = _labelDictionary[token.TokenValue.ToUpper()];
                                    _programData.Add(0x23);
                                    _programData.Add(BitConverter.GetBytes(addr2)[0]);
                                    _programData.Add(BitConverter.GetBytes(addr2)[1]);

                                    _debugInfo.Add(new DebugData
                                    {
                                        Address = progLoc,
                                        SourceCodeLine = "    JGT    $" + addr2.ToString("X4")
                                    });
                                }
                                progLoc = (ushort)(progLoc + 3);

                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.JGE:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected whitespace at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.LABEL)
                                {
                                    Error(string.Format("All jumps require a label at line {0}", lineNumber));
                                    return null;
                                }
                                if (phase == 2)
                                {
                                    if (!_labelDictionary.ContainsKey(token.TokenValue.ToUpper()))
                                    {
                                        Error(string.Format("No such label \"{0}\" at line {1}!", token.TokenValue.ToUpper(), lineNumber));
                                        return null;
                                    }
                                    ushort addr2 = _labelDictionary[token.TokenValue.ToUpper()];
                                    _programData.Add(0x20);
                                    _programData.Add(BitConverter.GetBytes(addr2)[0]);
                                    _programData.Add(BitConverter.GetBytes(addr2)[1]);

                                    _debugInfo.Add(new DebugData
                                    {
                                        Address = progLoc,
                                        SourceCodeLine = "    JGE    $" + addr2.ToString("X4")
                                    });
                                }
                                progLoc = (ushort)(progLoc + 3);

                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.JEQ:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected whitespace at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.LABEL)
                                {
                                    Error(string.Format("All jumps require a label at line {0}", lineNumber));
                                    return null;
                                }
                                if (phase == 2)
                                {
                                    if (!_labelDictionary.ContainsKey(token.TokenValue.ToUpper()))
                                    {
                                        Error(string.Format("No such label \"{0}\" at line {1}!", token.TokenValue.ToUpper(), lineNumber));
                                        return null;
                                    }
                                    ushort addr2 = _labelDictionary[token.TokenValue.ToUpper()];
                                    _programData.Add(0x17);
                                    _programData.Add(BitConverter.GetBytes(addr2)[0]);
                                    _programData.Add(BitConverter.GetBytes(addr2)[1]);

                                    _debugInfo.Add(new DebugData
                                    {
                                        Address = progLoc,
                                        SourceCodeLine = "    JEQ    $" + addr2.ToString("X4")
                                    });
                                }
                                progLoc = (ushort)(progLoc + 3);

                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.JNE:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected whitespace at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.LABEL)
                                {
                                    Error(string.Format("All jumps require a label at line {0}", lineNumber));
                                    return null;
                                }
                                if (phase == 2)
                                {
                                    if (!_labelDictionary.ContainsKey(token.TokenValue.ToUpper()))
                                    {
                                        Error(string.Format("No such label \"{0}\" at line {1}!", token.TokenValue.ToUpper(), lineNumber));
                                        return null;
                                    }
                                    ushort addr2 = _labelDictionary[token.TokenValue.ToUpper()];
                                    _programData.Add(0x18);
                                    _programData.Add(BitConverter.GetBytes(addr2)[0]);
                                    _programData.Add(BitConverter.GetBytes(addr2)[1]);

                                    _debugInfo.Add(new DebugData
                                    {
                                        Address = progLoc,
                                        SourceCodeLine = "    JNE    $" + addr2.ToString("X4")
                                    });
                                }
                                progLoc = (ushort)(progLoc + 3);

                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.JMP:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected whitespace at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.LABEL)
                                {
                                    Error(string.Format("All jumps require a label at line {0}", lineNumber));
                                    return null;
                                }
                                if (phase == 2)
                                {
                                    if (!_labelDictionary.ContainsKey(token.TokenValue.ToUpper()))
                                    {
                                        Error(string.Format("No such label \"{0}\" at line {1}!", token.TokenValue.ToUpper(), lineNumber));
                                        return null;
                                    }
                                    ushort addr2 = _labelDictionary[token.TokenValue.ToUpper()];
                                    _programData.Add(0x24);
                                    _programData.Add(BitConverter.GetBytes(addr2)[0]);
                                    _programData.Add(BitConverter.GetBytes(addr2)[1]);

                                    _debugInfo.Add(new DebugData
                                    {
                                        Address = progLoc,
                                        SourceCodeLine = "    JMP    $" + addr2.ToString("X4")
                                    });
                                }
                                progLoc = (ushort)(progLoc + 3);

                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.PUSH:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected whitespace at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.LABEL)
                                {
                                    Error(string.Format("Expected Register Name at line {0}!", lineNumber));
                                    return null;
                                }
                                byte pReg = 0;
                                if (token.TokenValue == "A" || token.TokenValue == "a")
                                    pReg = 1;
                                if (token.TokenValue == "B" || token.TokenValue == "b")
                                    pReg = 2;
                                if (token.TokenValue == "D" || token.TokenValue == "d")
                                    pReg = 4;
                                if (token.TokenValue == "X" || token.TokenValue == "x")
                                    pReg = 8;
                                if (token.TokenValue == "Y" || token.TokenValue == "y")
                                    pReg = 16;
                                if (pReg == 0)
                                {
                                    Error(string.Format("Expected Register Name at line {0}!", lineNumber));
                                    return null;
                                }
                                string pushReg = token.TokenValue.ToUpper();
                                if (phase == 2)
                                {
                                    _programData.Add(0x25);
                                    _programData.Add(pReg);
                                    
                                    _debugInfo.Add(new DebugData
                                    {
                                        Address = progLoc,
                                        SourceCodeLine = "    PUSH    " + pushReg
                                    });
                                }
                                progLoc = (ushort)(progLoc + 2);

                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.POP:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected whitespace at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.LABEL)
                                {
                                    Error(string.Format("Expected Register Name at line {0}!", lineNumber));
                                    return null;
                                }
                                byte pReg2 = 0;
                                if (token.TokenValue == "A" || token.TokenValue == "a")
                                    pReg2 = 1;
                                if (token.TokenValue == "B" || token.TokenValue == "b")
                                    pReg2 = 2;
                                if (token.TokenValue == "D" || token.TokenValue == "d")
                                    pReg2 = 4;
                                if (token.TokenValue == "X" || token.TokenValue == "x")
                                    pReg2 = 8;
                                if (token.TokenValue == "Y" || token.TokenValue == "y")
                                    pReg2 = 16;
                                if (pReg2 == 0)
                                {
                                    Error(string.Format("Expected Register Name at line {0}!", lineNumber));
                                    return null;
                                }
                                string popReg = token.TokenValue.ToUpper();
                                if (phase == 2)
                                {
                                    _programData.Add(0x26);
                                    _programData.Add(pReg2);
                                    
                                    _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    POP    " + popReg
                                        });
                                }
                                progLoc = (ushort)(progLoc + 2);

                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.CALL:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected whitespace at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.LABEL)
                                {
                                    Error(string.Format("All calls require a label at line {0}", lineNumber));
                                    return null;
                                }
                                if (phase == 2)
                                {
                                    if (!_labelDictionary.ContainsKey(token.TokenValue.ToUpper()))
                                    {
                                        Error(string.Format("No such label \"{0}\" at line {1}!", token.TokenValue.ToUpper(), lineNumber));
                                        return null;
                                    }
                                    ushort addr2 = _labelDictionary[token.TokenValue.ToUpper()];
                                    _programData.Add(0x27);
                                    _programData.Add(BitConverter.GetBytes(addr2)[0]);
                                    _programData.Add(BitConverter.GetBytes(addr2)[1]);

                                    _debugInfo.Add(new DebugData
                                    {
                                        Address = progLoc,
                                        SourceCodeLine = "    CALL    $" + addr2.ToString("X4")
                                    });
                                }
                                progLoc = (ushort)(progLoc + 3);

                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.RET:
                                if (phase == 2)
                                {
                                    _programData.Add(0x28);
                                    
                                    _debugInfo.Add(new DebugData
                                    {
                                        Address = progLoc,
                                        SourceCodeLine = "    RET"
                                    });
                                }
                                progLoc = (ushort) (progLoc + 1);
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.NEG:
                                token = tokenParser.GetToken();
                                if (token != null && token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected White Space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || (token.TokenName != TokenParser.Tokens.LABEL))
                                {
                                    Error(string.Format("Expected Register at line {0}!", lineNumber));
                                    return null;
                                }
                                if (token.TokenName == TokenParser.Tokens.LABEL)
                                {
                                    byte r = 0;

                                    if (token.TokenValue.ToUpper() == "A")
                                        r = 1;
                                    if (token.TokenValue.ToUpper() == "B")
                                        r = 2;
                                    if (token.TokenValue.ToUpper() == "D")
                                        r = 4;
                                    if (token.TokenValue.ToUpper() == "X")
                                        r = 8;
                                    if (token.TokenValue.ToUpper() == "Y")
                                        r = 16;
                                    if (r == 0)
                                    {
                                        Error(string.Format("Expected Register name at line {0}", lineNumber));
                                        return null;
                                    }
                                    string negReg = token.TokenValue.ToUpper();
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x6F);
                                        _programData.Add(r);
 
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    NEG    " + negReg
                                        });
                                    }
                                    progLoc = (ushort) (progLoc + 2);
                                }
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.NOP:
                                if (phase == 2)
                                {
                                    _programData.Add(0x6E);
                                    
                                    _debugInfo.Add(new DebugData
                                    {
                                        Address = progLoc,
                                        SourceCodeLine = "    NOP"
                                    });
                                }
                                progLoc = (ushort)(progLoc + 1);
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.CLS:
                                if (phase == 2)
                                {
                                    _programData.Add(0x70);

                                    _debugInfo.Add(new DebugData
                                    {
                                        Address = progLoc,
                                        SourceCodeLine = "    CLS"
                                    });
                                }
                                progLoc = (ushort)(progLoc + 1);
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.BRK:
                                if (phase == 2)
                                {
                                    _programData.Add(0xFF);

                                    _debugInfo.Add(new DebugData
                                    {
                                        Address = progLoc,
                                        SourceCodeLine = "    BRK"
                                    });
                                }
                                progLoc = (ushort)(progLoc + 1);
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.SCRUP:
                                if (phase == 2)
                                {
                                    _programData.Add(0x71);

                                    _debugInfo.Add(new DebugData
                                    {
                                        Address = progLoc,
                                        SourceCodeLine = "    SCRUP"
                                    });
                                }
                                progLoc = (ushort)(progLoc + 1);
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.RND:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected White Space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.LABEL)
                                {
                                    Error(string.Format("Expected A or B register at line {0}!", lineNumber));
                                    return null;
                                }
                                byte reg9 = 0;
                                if (token.TokenValue == "A" || token.TokenValue == "a")
                                    reg9 = 1;
                                if (token.TokenValue == "B" || token.TokenValue == "b")
                                    reg9 = 2;
                                if (token.TokenValue == "D" || token.TokenValue == "d")
                                    reg9 = 4;
                                if (token.TokenValue == "X" || token.TokenValue == "x")
                                    reg9 = 8;
                                if (token.TokenValue == "Y" || token.TokenValue == "y")
                                    reg9 = 16;

                                if (reg9 == 0)
                                {
                                    Error(string.Format("Expected register at line {0}!", lineNumber));
                                    return null;
                                }
                                string keyReg9 = token.TokenValue.ToUpper();
                                if (phase == 2)
                                {
                                    _programData.Add(0x6A);
                                    _programData.Add(reg9);

                                    _debugInfo.Add(new DebugData
                                    {
                                        Address = progLoc,
                                        SourceCodeLine = "    RND    " + keyReg9
                                    });
                                }
                                progLoc = (ushort)(progLoc + 2);
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.RMB:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected White Space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null ||
                                    (token.TokenName != TokenParser.Tokens.NUMBER &&
                                     token.TokenName != TokenParser.Tokens.HEXNUMBER &&
                                     token.TokenName != TokenParser.Tokens.BINARYNUMBER))
                                {
                                    Error(string.Format("Expected number at line {0}!", lineNumber));
                                    return null;
                                }
                                ushort rmbSize = Get16BitNumber(token);
                                if (phase == 2)
                                {
                                    for (int r = 0; r < rmbSize; r++)
                                        _programData.Add(0);
                                }
                                progLoc = (ushort) (progLoc + rmbSize);
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.SUBX:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected White Space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || (token.TokenName != TokenParser.Tokens.POUND && token.TokenName != TokenParser.Tokens.LABEL))
                                {
                                    Error(string.Format("Expected immediate value (missing '#') or register at line {0}", lineNumber));
                                    return null;
                                }
                                if (token.TokenName == TokenParser.Tokens.POUND)
                                {
                                    token = tokenParser.GetToken();
                                    if (token == null ||
                                        (token.TokenName != TokenParser.Tokens.NUMBER &&
                                         token.TokenName != TokenParser.Tokens.HEXNUMBER &&
                                         token.TokenName != TokenParser.Tokens.BINARYNUMBER))
                                    {
                                        Error(string.Format("Expected number at line {0}!", lineNumber));
                                        return null;
                                    }
                                    ushort val2 = Get16BitNumber(token);
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x2D);
                                        _programData.Add(BitConverter.GetBytes(val2)[0]);
                                        _programData.Add(BitConverter.GetBytes(val2)[1]);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    SUBX    #$" + val2.ToString("X4")
                                        });
                                    }
                                    progLoc = (ushort) (progLoc + 3);
                                }
                                else if (token.TokenName == TokenParser.Tokens.LABEL)
                                {
                                    byte r = 0;
                                    
                                    if (token.TokenValue.ToUpper() == "A")
                                        r = 1;
                                    if (token.TokenValue.ToUpper() == "B")
                                        r = 2;
                                    if (token.TokenValue.ToUpper() == "D")
                                        r = 4;
                                    if (token.TokenValue.ToUpper() == "X")
                                        r = 8;
                                    if (token.TokenValue.ToUpper() == "Y")
                                        r = 16;
                                    if (r == 0)
                                    {
                                        Error(string.Format("Expected Register name at line {0}", lineNumber));
                                        return null;
                                    }
                                    string subxreg = token.TokenValue.ToUpper();

                                    if (phase == 2)
                                    {
                                        _programData.Add(0x37);
                                        _programData.Add(r);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    SUBX    " + subxreg
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 2);
                                }
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.SUBY:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected White Space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || (token.TokenName != TokenParser.Tokens.POUND && token.TokenName != TokenParser.Tokens.LABEL))
                                {
                                    Error(string.Format("Expected immediate value (missing '#') or register at line {0}", lineNumber));
                                    return null;
                                }
                                if (token.TokenName == TokenParser.Tokens.POUND)
                                {
                                    token = tokenParser.GetToken();
                                    if (token == null ||
                                        (token.TokenName != TokenParser.Tokens.NUMBER &&
                                         token.TokenName != TokenParser.Tokens.HEXNUMBER &&
                                         token.TokenName != TokenParser.Tokens.BINARYNUMBER))
                                    {
                                        Error(string.Format("Expected number at line {0}!", lineNumber));
                                        return null;
                                    }
                                    ushort val3 = Get16BitNumber(token);
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x2E);
                                        _programData.Add(BitConverter.GetBytes(val3)[0]);
                                        _programData.Add(BitConverter.GetBytes(val3)[1]);
                                       
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    SUBY    #$" + val3.ToString("X4")
                                        });
                                    }
                                    progLoc = (ushort) (progLoc + 3);
                                }
                                else if (token.TokenName == TokenParser.Tokens.LABEL)
                                {
                                    byte r = 0;

                                    if (token.TokenValue.ToUpper() == "A")
                                        r = 1;
                                    if (token.TokenValue.ToUpper() == "B")
                                        r = 2;
                                    if (token.TokenValue.ToUpper() == "D")
                                        r = 4;
                                    if (token.TokenValue.ToUpper() == "X")
                                        r = 8;
                                    if (token.TokenValue.ToUpper() == "Y")
                                        r = 16;
                                    if (r == 0)
                                    {
                                        Error(string.Format("Expected Register name at line {0}", lineNumber));
                                        return null;
                                    }
                                    string subyreg = token.TokenValue.ToUpper();
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x38);
                                        _programData.Add(r);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    SUBY    " + subyreg
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 2);
                                }
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.SUBD:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected White Space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || (token.TokenName != TokenParser.Tokens.POUND && token.TokenName != TokenParser.Tokens.LABEL))
                                {
                                    Error(string.Format("Expected immediate value (missing '#') or register at line {0}", lineNumber));
                                    return null;
                                }
                                if (token.TokenName == TokenParser.Tokens.POUND)
                                {
                                    token = tokenParser.GetToken();
                                    if (token == null ||
                                        (token.TokenName != TokenParser.Tokens.NUMBER &&
                                         token.TokenName != TokenParser.Tokens.HEXNUMBER &&
                                         token.TokenName != TokenParser.Tokens.BINARYNUMBER))
                                    {
                                        Error(string.Format("Expected number at line {0}!", lineNumber));
                                        return null;
                                    }
                                    ushort val4 = Get16BitNumber(token);
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x2C);
                                        _programData.Add(BitConverter.GetBytes(val4)[0]);
                                        _programData.Add(BitConverter.GetBytes(val4)[1]);

                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    SUBD    #$" + val4.ToString("X4")
                                        });
                                    }
                                    progLoc = (ushort) (progLoc + 3);
                                }
                                else if (token.TokenName == TokenParser.Tokens.LABEL)
                                {
                                    byte r = 0;

                                    if (token.TokenValue.ToUpper() == "A")
                                        r = 1;
                                    if (token.TokenValue.ToUpper() == "B")
                                        r = 2;
                                    if (token.TokenValue.ToUpper() == "D")
                                        r = 4;
                                    if (token.TokenValue.ToUpper() == "X")
                                        r = 8;
                                    if (token.TokenValue.ToUpper() == "Y")
                                        r = 16;
                                    if (r == 0)
                                    {
                                        Error(string.Format("Expected Register name at line {0}", lineNumber));
                                        return null;
                                    }
                                    string subdreg = token.TokenValue.ToUpper();
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x36);
                                        _programData.Add(r);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    SUBD    " + subdreg
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 2);
                                }
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.SUBA:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected White Space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || (token.TokenName != TokenParser.Tokens.POUND && token.TokenName != TokenParser.Tokens.LABEL))
                                {
                                    Error(string.Format("Expected immediate value (missing '#') or register at line {0}", lineNumber));
                                    return null;
                                }
                                if (token.TokenName == TokenParser.Tokens.POUND)
                                {
                                    token = tokenParser.GetToken();
                                    if (token == null ||
                                        (token.TokenName != TokenParser.Tokens.NUMBER &&
                                         token.TokenName != TokenParser.Tokens.HEXNUMBER &&
                                         token.TokenName != TokenParser.Tokens.BINARYNUMBER))
                                    {
                                        Error(string.Format("Expected number at line {0}!", lineNumber));
                                        return null;
                                    }
                                    byte val5 = Get8BitNumber(token);
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x2A);
                                        _programData.Add(val5);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    SUBA    #$" + val5.ToString("X2")
                                        });
                                    }
                                    progLoc = (ushort) (progLoc + 2);
                                }
                                else if (token.TokenName == TokenParser.Tokens.LABEL)
                                {
                                    byte r = 0;

                                    if (token.TokenValue.ToUpper() == "A")
                                        r = 1;
                                    if (token.TokenValue.ToUpper() == "B")
                                        r = 2;
                                    if (r == 0)
                                    {
                                        Error(string.Format("Expected 8-bit Register name at line {0}", lineNumber));
                                        return null;
                                    }
                                    string subareg = token.TokenValue.ToUpper();
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x34);
                                        _programData.Add(r);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    SUBA    " + subareg
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 2);
                                }
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.SUBB:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected White Space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || (token.TokenName != TokenParser.Tokens.POUND && token.TokenName != TokenParser.Tokens.LABEL))
                                {
                                    Error(string.Format("Expected immediate value (missing '#') or register at line {0}", lineNumber));
                                    return null;
                                }
                                if (token.TokenName == TokenParser.Tokens.POUND)
                                {
                                    token = tokenParser.GetToken();
                                    if (token == null ||
                                        (token.TokenName != TokenParser.Tokens.NUMBER &&
                                         token.TokenName != TokenParser.Tokens.HEXNUMBER &&
                                         token.TokenName != TokenParser.Tokens.BINARYNUMBER))
                                    {
                                        Error(string.Format("Expected number at line {0}!", lineNumber));
                                        return null;
                                    }
                                    byte val6 = Get8BitNumber(token);
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x2B);
                                        _programData.Add(val6);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    SUBB    #$" + val6.ToString("X2")
                                        });
                                    }
                                    progLoc = (ushort) (progLoc + 2);
                                }
                                else if (token.TokenName == TokenParser.Tokens.LABEL)
                                {
                                    byte r = 0;

                                    if (token.TokenValue.ToUpper() == "A")
                                        r = 1;
                                    if (token.TokenValue.ToUpper() == "B")
                                        r = 2;
                                    if (r == 0)
                                    {
                                        Error(string.Format("Expected 8-bit Register name at line {0}", lineNumber));
                                        return null;
                                    }
                                    string subbreg = token.TokenValue.ToUpper();
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x35);
                                        _programData.Add(r);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    SUBB    " + subbreg
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 2);
                                }
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.ADDA:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected White Space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || (token.TokenName != TokenParser.Tokens.POUND && token.TokenName != TokenParser.Tokens.LABEL))
                                {
                                    Error(string.Format("Expected immediate value (missing '#') or register at line {0}", lineNumber));
                                    return null;
                                }
                                if (token.TokenName == TokenParser.Tokens.POUND)
                                {
                                    token = tokenParser.GetToken();
                                    if (token == null ||
                                        (token.TokenName != TokenParser.Tokens.NUMBER &&
                                         token.TokenName != TokenParser.Tokens.HEXNUMBER &&
                                         token.TokenName != TokenParser.Tokens.BINARYNUMBER))
                                    {
                                        Error(string.Format("Expected number at line {0}!", lineNumber));
                                        return null;
                                    }
                                    byte val7 = Get8BitNumber(token);
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x2F);
                                        _programData.Add(val7);
                                       
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    ADDA    #$" + val7.ToString("X2")
                                        });
                                    }
                                    progLoc = (ushort) (progLoc + 2);
                                }
                                else if (token.TokenName == TokenParser.Tokens.LABEL)
                                {
                                    byte r = 0;

                                    if (token.TokenValue.ToUpper() == "A")
                                        r = 1;
                                    if (token.TokenValue.ToUpper() == "B")
                                        r = 2;
                                    if (r == 0)
                                    {
                                        Error(string.Format("Expected 8-bit Register name at line {0}", lineNumber));
                                        return null;
                                    }
                                    string addareg = token.TokenValue.ToUpper();

                                    if (phase == 2)
                                    {
                                        _programData.Add(0x39);
                                        _programData.Add(r);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    ADDA    " + addareg
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 2);
                                }
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.ADDB:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected White Space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || (token.TokenName != TokenParser.Tokens.POUND && token.TokenName != TokenParser.Tokens.LABEL))
                                {
                                    Error(string.Format("Expected immediate value (missing '#') or register at line {0}", lineNumber));
                                    return null;
                                }
                                if (token.TokenName == TokenParser.Tokens.POUND)
                                {
                                    token = tokenParser.GetToken();
                                    if (token == null ||
                                        (token.TokenName != TokenParser.Tokens.NUMBER &&
                                         token.TokenName != TokenParser.Tokens.HEXNUMBER &&
                                         token.TokenName != TokenParser.Tokens.BINARYNUMBER))
                                    {
                                        Error(string.Format("Expected number at line {0}!", lineNumber));
                                        return null;
                                    }
                                    byte val8 = Get8BitNumber(token);
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x30);
                                        _programData.Add(val8);

                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    ADDB    #$" + val8.ToString("X2")
                                        });
                                    }
                                    progLoc = (ushort) (progLoc + 2);
                                }
                                else if (token.TokenName == TokenParser.Tokens.LABEL)
                                {
                                    byte r = 0;
                                    
                                    if (token.TokenValue.ToUpper() == "A")
                                        r = 1;
                                    if (token.TokenValue.ToUpper() == "B")
                                        r = 2;
                                    if (r == 0)
                                    {
                                        Error(string.Format("Expected 8-bit Register name at line {0}", lineNumber));
                                        return null;
                                    }
                                    string addbreg = token.TokenValue.ToUpper();

                                    if (phase == 2)
                                    {
                                        _programData.Add(0x3A);
                                        _programData.Add(r);

                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    ADDB    " + addbreg
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 2);
                                }
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.ADDD:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected White Space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || (token.TokenName != TokenParser.Tokens.POUND && token.TokenName != TokenParser.Tokens.LABEL))
                                {
                                    Error(string.Format("Expected immediate value (missing '#') or register at line {0}", lineNumber));
                                    return null;
                                }
                                if (token.TokenName == TokenParser.Tokens.POUND)
                                {
                                    token = tokenParser.GetToken();
                                    if (token == null ||
                                        (token.TokenName != TokenParser.Tokens.NUMBER &&
                                         token.TokenName != TokenParser.Tokens.HEXNUMBER &&
                                         token.TokenName != TokenParser.Tokens.BINARYNUMBER))
                                    {
                                        Error(string.Format("Expected number at line {0}!", lineNumber));
                                        return null;
                                    }
                                    ushort val9 = Get16BitNumber(token);
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x31);
                                        _programData.Add(BitConverter.GetBytes(val9)[0]);
                                        _programData.Add(BitConverter.GetBytes(val9)[1]);

                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    ADDD    #$" + val9.ToString("X4")
                                        });
                                    }
                                    progLoc = (ushort) (progLoc + 3);
                                }
                                else if (token.TokenName == TokenParser.Tokens.LABEL)
                                {
                                    byte r = 0;
                                    
                                    if (token.TokenValue.ToUpper() == "A")
                                        r = 1;
                                    if (token.TokenValue.ToUpper() == "B")
                                        r = 2;
                                    if (token.TokenValue.ToUpper() == "D")
                                        r = 4;
                                    if (token.TokenValue.ToUpper() == "X")
                                        r = 8;
                                    if (token.TokenValue.ToUpper() == "Y")
                                        r = 16;
                                    if (r == 0)
                                    {
                                        Error(string.Format("Expected Register name at line {0}", lineNumber));
                                        return null;
                                    }
                                    string adddreg = token.TokenValue.ToUpper();
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x3B);
                                        _programData.Add(r);

                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    ADDD    " + adddreg
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 2);
                                }
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.ADDX:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected White Space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || (token.TokenName != TokenParser.Tokens.POUND && token.TokenName != TokenParser.Tokens.LABEL))
                                {
                                    Error(string.Format("Expected immediate value (missing '#') or Register at line {0}", lineNumber));
                                    return null;
                                }
                                if (token.TokenName == TokenParser.Tokens.POUND)
                                {
                                    token = tokenParser.GetToken();
                                    if (token == null ||
                                        (token.TokenName != TokenParser.Tokens.NUMBER &&
                                         token.TokenName != TokenParser.Tokens.HEXNUMBER &&
                                         token.TokenName != TokenParser.Tokens.BINARYNUMBER))
                                    {
                                        Error(string.Format("Expected number at line {0}!", lineNumber));
                                        return null;
                                    }
                                    ushort val10 = Get16BitNumber(token);
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x32);
                                        _programData.Add(BitConverter.GetBytes(val10)[0]);
                                        _programData.Add(BitConverter.GetBytes(val10)[1]);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    ADDX    #$" + val10.ToString("X4")
                                        });
                                    }
                                    progLoc = (ushort) (progLoc + 3);
                                }
                                else if (token.TokenName == TokenParser.Tokens.LABEL)
                                {
                                    byte r = 0;

                                    if (token.TokenValue.ToUpper() == "A")
                                        r = 1;
                                    if (token.TokenValue.ToUpper() == "B")
                                        r = 2;
                                    if (token.TokenValue.ToUpper() == "D")
                                        r = 4;
                                    if (token.TokenValue.ToUpper() == "X")
                                        r = 8;
                                    if (token.TokenValue.ToUpper() == "Y")
                                        r = 16;
                                    if (r == 0)
                                    {
                                        Error(string.Format("Expected Register name at line {0}", lineNumber));
                                        return null;
                                    }
                                    string addxreg = token.TokenValue.ToUpper();
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x3C);
                                        _programData.Add(r);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    ADDX    " + addxreg
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 2);
                                }
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.ADDY:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected White Space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || (token.TokenName != TokenParser.Tokens.POUND && token.TokenName != TokenParser.Tokens.LABEL))
                                {
                                    Error(string.Format("Expected immediate value (missing '#') or Register at line {0}", lineNumber));
                                    return null;
                                }
                                if (token.TokenName == TokenParser.Tokens.POUND)
                                {
                                    token = tokenParser.GetToken();
                                    if (token == null ||
                                        (token.TokenName != TokenParser.Tokens.NUMBER &&
                                         token.TokenName != TokenParser.Tokens.HEXNUMBER &&
                                         token.TokenName != TokenParser.Tokens.BINARYNUMBER))
                                    {
                                        Error(string.Format("Expected number at line {0}!", lineNumber));
                                        return null;
                                    }
                                    ushort val11 = Get16BitNumber(token);
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x33);
                                        _programData.Add(BitConverter.GetBytes(val11)[0]);
                                        _programData.Add(BitConverter.GetBytes(val11)[1]);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    ADDY    #$" + val11.ToString("X4")
                                        });
                                    }
                                    progLoc = (ushort) (progLoc + 3);
                                } else if (token.TokenName == TokenParser.Tokens.LABEL)
                                {
                                    byte r = 0;

                                    if (token.TokenValue.ToUpper() == "A")
                                        r = 1;
                                    if (token.TokenValue.ToUpper() == "B")
                                        r = 2;
                                    if (token.TokenValue.ToUpper() == "D")
                                        r = 4;
                                    if (token.TokenValue.ToUpper() == "X")
                                        r = 8;
                                    if (token.TokenValue.ToUpper() == "Y")
                                        r = 16;
                                    if (r == 0)
                                    {
                                        Error(string.Format("Expected Register name at line {0}", lineNumber));
                                        return null;
                                    }
                                    string addyreg = token.TokenValue.ToUpper();
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x3D);
                                        _programData.Add(r);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    ADDY    " + addyreg
                                        });
                                    }
                                    progLoc = (ushort) (progLoc + 2);
                                }
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.TFR:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected White Space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.LABEL)
                                {
                                    Error(string.Format("Expected Register name at line {0}!", lineNumber));
                                    return null;
                                }
                                string sourceRegister = token.TokenValue.ToUpper();
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.COMMA)
                                {
                                    Error(string.Format("Expected comma at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.LABEL)
                                {
                                    Error(string.Format("Expected Register name at line {0}!", lineNumber));
                                    return null;
                                }
                                string destinationRegister = token.TokenValue.ToUpper();
                                if ((sourceRegister == "X" || sourceRegister == "Y" || sourceRegister == "D") &&
                                    (destinationRegister == "A" || destinationRegister == "B"))
                                {
                                    Error(string.Format("Cannot transfer 16-bit register to 8-bit register at line {0}!", lineNumber));
                                    return null;
                                }
                                byte sr = 0;
                                byte dr = 0;
                                if (sourceRegister == "A")
                                    sr = 1;
                                if (sourceRegister == "B")
                                    sr = 2;
                                if (sourceRegister == "D")
                                    sr = 4;
                                if (sourceRegister == "X")
                                    sr = 8;
                                if (sourceRegister == "Y")
                                    sr = 16;
                                if (destinationRegister == "A")
                                    dr = 1;
                                if (destinationRegister == "B")
                                    dr = 2;
                                if (destinationRegister == "D")
                                    dr = 4;
                                if (destinationRegister == "X")
                                    dr = 8;
                                if (destinationRegister == "Y")
                                    dr = 16;

                                if (sr == 0 || dr == 0)
                                {
                                    Error(string.Format("Invalid register name at line {0}!", lineNumber));
                                    return null;
                                }
                                
                                if (phase == 2)
                                {
                                    _programData.Add(0x3E);
                                    _programData.Add(sr);
                                    _programData.Add(dr);
                                    
                                    _debugInfo.Add(new DebugData
                                    {
                                        Address = progLoc,
                                        SourceCodeLine = "    TFR    " + sourceRegister + "," + destinationRegister
                                    });
                                }
                                progLoc = (ushort) (progLoc + 3);
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.LSFT:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected White Space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.LABEL)
                                {
                                    Error(string.Format("Expected Register name at line {0}!", lineNumber));
                                    return null;
                                }
                                byte lsReg = 0;
                                if (token.TokenValue.ToUpper() == "A")
                                    lsReg = 1;
                                if (token.TokenValue.ToUpper() == "B")
                                    lsReg = 2;
                                if (token.TokenValue.ToUpper() == "D")
                                    lsReg = 4;
                                if (token.TokenValue.ToUpper() == "X")
                                    lsReg = 8;
                                if (token.TokenValue.ToUpper() == "Y")
                                    lsReg = 16;
                                if (lsReg == 0)
                                {
                                    Error(string.Format("Invalid register name at line {0}!", lineNumber));
                                    return null;
                                }
                                if (phase == 2)
                                {
                                    _programData.Add(0x44);
                                    _programData.Add(lsReg);
                                    
                                    _debugInfo.Add(new DebugData
                                    {
                                        Address = progLoc,
                                        SourceCodeLine = "    LSFT    " + token.TokenValue.ToUpper()
                                    });
                                }
                                progLoc = (ushort) (progLoc + 2);
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.RSFT:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected White Space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.LABEL)
                                {
                                    Error(string.Format("Expected Register name at line {0}!", lineNumber));
                                    return null;
                                }
                                byte rsReg = 0;
                                if (token.TokenValue.ToUpper() == "A")
                                    rsReg = 1;
                                if (token.TokenValue.ToUpper() == "B")
                                    rsReg = 2;
                                if (token.TokenValue.ToUpper() == "D")
                                    rsReg = 4;
                                if (token.TokenValue.ToUpper() == "X")
                                    rsReg = 8;
                                if (token.TokenValue.ToUpper() == "Y")
                                    rsReg = 16;
                                if (rsReg == 0)
                                {
                                    Error(string.Format("Invalid register name at line {0}!", lineNumber));
                                    return null;
                                }
                                if (phase == 2)
                                {
                                    _programData.Add(0x45);
                                    _programData.Add(rsReg);
                                    
                                    _debugInfo.Add(new DebugData
                                    {
                                        Address = progLoc,
                                        SourceCodeLine = "    RSFT    " + token.TokenValue.ToUpper()
                                    });
                                }
                                progLoc = (ushort)(progLoc + 2);
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.JOS:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected whitespace at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.LABEL)
                                {
                                    Error(string.Format("All jumps require a label at line {0}", lineNumber));
                                    return null;
                                }
                                if (phase == 2)
                                {
                                    if (!_labelDictionary.ContainsKey(token.TokenValue.ToUpper()))
                                    {
                                        Error(string.Format("No such label \"{0}\" at line {1}!", token.TokenValue.ToUpper(), lineNumber));
                                        return null;
                                    }
                                    ushort addr2 = _labelDictionary[token.TokenValue.ToUpper()];
                                    _programData.Add(0x46);
                                    _programData.Add(BitConverter.GetBytes(addr2)[0]);
                                    _programData.Add(BitConverter.GetBytes(addr2)[1]);

                                    _debugInfo.Add(new DebugData
                                    {
                                        Address = progLoc,
                                        SourceCodeLine = "    JOS    $" + addr2.ToString("X4")
                                    });
                                }
                                progLoc = (ushort)(progLoc + 3);

                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.JOC:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected whitespace at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.LABEL)
                                {
                                    Error(string.Format("All jumps require a label at line {0}", lineNumber));
                                    return null;
                                }
                                if (phase == 2)
                                {
                                    if (!_labelDictionary.ContainsKey(token.TokenValue.ToUpper()))
                                    {
                                        Error(string.Format("No such label \"{0}\" at line {1}!", token.TokenValue.ToUpper(), lineNumber));
                                        return null;
                                    }
                                    ushort addr2 = _labelDictionary[token.TokenValue.ToUpper()];
                                    _programData.Add(0x47);
                                    _programData.Add(BitConverter.GetBytes(addr2)[0]);
                                    _programData.Add(BitConverter.GetBytes(addr2)[1]);

                                    _debugInfo.Add(new DebugData
                                    {
                                        Address = progLoc,
                                        SourceCodeLine = "    JOC    $" + addr2.ToString("X4")
                                    });
                                }
                                progLoc = (ushort)(progLoc + 3);

                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.MUL8:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected White Space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.LABEL)
                                {
                                    Error(string.Format("Expected Register name at line {0}!", lineNumber));
                                    return null;
                                }
                                string sourceRegister2 = token.TokenValue.ToUpper();
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.COMMA)
                                {
                                    Error(string.Format("Expected comma at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.LABEL)
                                {
                                    Error(string.Format("Expected Register name at line {0}!", lineNumber));
                                    return null;
                                }
                                string destinationRegister2 = token.TokenValue.ToUpper();
                                if (sourceRegister2 != "A" && sourceRegister2 != "B")
                                {
                                    Error(string.Format("Register must be an 8-bit register (A or B) at line {0}!", lineNumber));
                                    return null;
                                }
                                if (destinationRegister2 != "A" && destinationRegister2 != "B")
                                {
                                    Error(string.Format("Register must be an 8-bit register (A or B) at line {0}!", lineNumber));
                                    return null;
                                }
                                byte sr2 = 0;
                                byte dr2 = 0;
                                if (sourceRegister2 == "A")
                                    sr2 = 1;
                                if (sourceRegister2 == "B")
                                    sr2 = 2;
                                if (sourceRegister2 == "D")
                                    sr2 = 4;
                                if (sourceRegister2 == "X")
                                    sr2 = 8;
                                if (sourceRegister2 == "Y")
                                    sr2 = 16;
                                if (destinationRegister2 == "A")
                                    dr2 = 1;
                                if (destinationRegister2 == "B")
                                    dr2 = 2;
                                if (destinationRegister2 == "D")
                                    dr2 = 4;
                                if (destinationRegister2 == "X")
                                    dr2 = 8;
                                if (destinationRegister2 == "Y")
                                    dr2 = 16;

                                if (sr2 == 0 || dr2 == 0)
                                {
                                    Error(string.Format("Invalid register name at line {0}!", lineNumber));
                                    return null;
                                }
                                if (phase == 2)
                                {
                                    _programData.Add(0x48);
                                    _programData.Add(sr2);
                                    _programData.Add(dr2);

                                    _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    MUL8    " + sourceRegister2 + "," + destinationRegister2
                                        });
                                }
                                progLoc = (ushort)(progLoc + 3);
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.MUL16:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected White Space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.LABEL)
                                {
                                    Error(string.Format("Expected Register name at line {0}!", lineNumber));
                                    return null;
                                }
                                string sourceRegister3 = token.TokenValue.ToUpper();
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.COMMA)
                                {
                                    Error(string.Format("Expected comma at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.LABEL)
                                {
                                    Error(string.Format("Expected Register name at line {0}!", lineNumber));
                                    return null;
                                }
                                string destinationRegister3 = token.TokenValue.ToUpper();
                                if (sourceRegister3 != "X" && sourceRegister3 != "Y" && sourceRegister3 != "D")
                                {
                                    Error(string.Format("Register must be a 16-bit register (X or Y or D) at line {0}!", lineNumber));
                                    return null;
                                }
                                if (destinationRegister3 != "A" && destinationRegister3 != "B" && destinationRegister3 != "D" && destinationRegister3 != "X" && destinationRegister3 != "Y")
                                {
                                    Error(string.Format("Register must be valid at line {0}!", lineNumber));
                                    return null;
                                }
                                byte sr3 = 0;
                                byte dr3 = 0;
                                if (sourceRegister3 == "A")
                                    sr3 = 1;
                                if (sourceRegister3 == "B")
                                    sr3 = 2;
                                if (sourceRegister3 == "D")
                                    sr3 = 4;
                                if (sourceRegister3 == "X")
                                    sr3 = 8;
                                if (sourceRegister3 == "Y")
                                    sr3 = 16;
                                if (destinationRegister3 == "A")
                                    dr3 = 1;
                                if (destinationRegister3 == "B")
                                    dr3 = 2;
                                if (destinationRegister3 == "D")
                                    dr3 = 4;
                                if (destinationRegister3 == "X")
                                    dr3 = 8;
                                if (destinationRegister3 == "Y")
                                    dr3 = 16;

                                if (phase == 2)
                                {
                                    _programData.Add(0x49);
                                    _programData.Add(sr3);
                                    _programData.Add(dr3);

                                    _debugInfo.Add(new DebugData
                                    {
                                        Address = progLoc,
                                        SourceCodeLine = "    MUL16    " + sourceRegister3 + "," + destinationRegister3
                                    });
                                }
                                progLoc = (ushort)(progLoc + 3);
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.ANDA:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected White Space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || (token.TokenName != TokenParser.Tokens.LABEL && token.TokenName != TokenParser.Tokens.POUND))
                                {
                                    Error(string.Format("Expected Immediate value or Register name at line {0}!", lineNumber));
                                    return null;
                                }
                                if (token.TokenName == TokenParser.Tokens.POUND)
                                {
                                    token = tokenParser.GetToken();
                                    if (token == null ||
                                        (token.TokenName != TokenParser.Tokens.NUMBER &&
                                         token.TokenName != TokenParser.Tokens.BINARYNUMBER &&
                                         token.TokenName != TokenParser.Tokens.HEXNUMBER))
                                    {
                                        Error(string.Format("Expected number at line {0}!", lineNumber));
                                        return null;
                                    }
                                    byte andaval = Get8BitNumber(token);
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x4A);
                                        _programData.Add(andaval);

                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    ANDA    #$" + andaval.ToString("X2")
                                        });
                                    }
                                    progLoc = (ushort) (progLoc + 2);
                                } else if (token.TokenName == TokenParser.Tokens.LABEL)
                                {
                                    byte andareg = 0;
                                    if (token.TokenValue.ToUpper() == "A")
                                        andareg = 1;
                                    if (token.TokenValue.ToUpper() == "B")
                                        andareg = 2;
                                    if (andareg == 0)
                                    {
                                        Error(string.Format("Expected 8-bit register name at line {0}", lineNumber));
                                        return null;
                                    }
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x4F);
                                        _programData.Add(andareg);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    ANDA    " + token.TokenValue.ToUpper()
                                        });
                                    }
                                    progLoc = (ushort) (progLoc + 2);
                                }
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.ANDB:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected White Space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || (token.TokenName != TokenParser.Tokens.LABEL && token.TokenName != TokenParser.Tokens.POUND))
                                {
                                    Error(string.Format("Expected Immediate value or Register name at line {0}!", lineNumber));
                                    return null;
                                }
                                if (token.TokenName == TokenParser.Tokens.POUND)
                                {
                                    token = tokenParser.GetToken();
                                    if (token == null ||
                                        (token.TokenName != TokenParser.Tokens.NUMBER &&
                                         token.TokenName != TokenParser.Tokens.BINARYNUMBER &&
                                         token.TokenName != TokenParser.Tokens.HEXNUMBER))
                                    {
                                        Error(string.Format("Expected number at line {0}!", lineNumber));
                                        return null;
                                    }
                                    byte andaval = Get8BitNumber(token);
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x4B);
                                        _programData.Add(andaval);

                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    ANDB    #$" + andaval.ToString("X2")
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 2);
                                }
                                else if (token.TokenName == TokenParser.Tokens.LABEL)
                                {
                                    byte andareg = 0;
                                    if (token.TokenValue.ToUpper() == "A")
                                        andareg = 1;
                                    if (token.TokenValue.ToUpper() == "B")
                                        andareg = 2;
                                    if (andareg == 0)
                                    {
                                        Error(string.Format("Expected 8-bit register name at line {0}", lineNumber));
                                        return null;
                                    }
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x50);
                                        _programData.Add(andareg);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    ANDB    " + token.TokenValue.ToUpper()
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 2);
                                }
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.ANDD:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected White Space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || (token.TokenName != TokenParser.Tokens.LABEL && token.TokenName != TokenParser.Tokens.POUND))
                                {
                                    Error(string.Format("Expected Immediate value or Register name at line {0}!", lineNumber));
                                    return null;
                                }
                                if (token.TokenName == TokenParser.Tokens.POUND)
                                {
                                    token = tokenParser.GetToken();
                                    if (token == null ||
                                        (token.TokenName != TokenParser.Tokens.NUMBER &&
                                         token.TokenName != TokenParser.Tokens.BINARYNUMBER &&
                                         token.TokenName != TokenParser.Tokens.HEXNUMBER))
                                    {
                                        Error(string.Format("Expected number at line {0}!", lineNumber));
                                        return null;
                                    }
                                    ushort andaval = Get16BitNumber(token);
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x4C);
                                        _programData.Add(BitConverter.GetBytes(andaval)[0]);
                                        _programData.Add(BitConverter.GetBytes(andaval)[1]);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    ANDD    #$" + andaval.ToString("X4")
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 3);
                                }
                                else if (token.TokenName == TokenParser.Tokens.LABEL)
                                {
                                    byte andareg = 0;
                                    if (token.TokenValue.ToUpper() == "A")
                                        andareg = 1;
                                    if (token.TokenValue.ToUpper() == "B")
                                        andareg = 2;
                                    if (token.TokenValue.ToUpper() == "D")
                                        andareg = 4;
                                    if (token.TokenValue.ToUpper() == "X")
                                        andareg = 8;
                                    if (token.TokenValue.ToUpper() == "Y")
                                        andareg = 16;
                                    if (andareg == 0)
                                    {
                                        Error(string.Format("Expected 8-bit register name at line {0}", lineNumber));
                                        return null;
                                    }
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x51);
                                        _programData.Add(andareg);

                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    ANDD    " + token.TokenValue.ToUpper()
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 2);
                                }
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.ANDX:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected White Space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || (token.TokenName != TokenParser.Tokens.LABEL && token.TokenName != TokenParser.Tokens.POUND))
                                {
                                    Error(string.Format("Expected Immediate value or Register name at line {0}!", lineNumber));
                                    return null;
                                }
                                if (token.TokenName == TokenParser.Tokens.POUND)
                                {
                                    token = tokenParser.GetToken();
                                    if (token == null ||
                                        (token.TokenName != TokenParser.Tokens.NUMBER &&
                                         token.TokenName != TokenParser.Tokens.BINARYNUMBER &&
                                         token.TokenName != TokenParser.Tokens.HEXNUMBER))
                                    {
                                        Error(string.Format("Expected number at line {0}!", lineNumber));
                                        return null;
                                    }
                                    ushort andaval = Get16BitNumber(token);
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x4D);
                                        _programData.Add(BitConverter.GetBytes(andaval)[0]);
                                        _programData.Add(BitConverter.GetBytes(andaval)[1]);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    ANDX    #$" + andaval.ToString("X4")
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 3);
                                }
                                else if (token.TokenName == TokenParser.Tokens.LABEL)
                                {
                                    byte andareg = 0;
                                    if (token.TokenValue.ToUpper() == "A")
                                        andareg = 1;
                                    if (token.TokenValue.ToUpper() == "B")
                                        andareg = 2;
                                    if (token.TokenValue.ToUpper() == "D")
                                        andareg = 4;
                                    if (token.TokenValue.ToUpper() == "X")
                                        andareg = 8;
                                    if (token.TokenValue.ToUpper() == "Y")
                                        andareg = 16;
                                    if (andareg == 0)
                                    {
                                        Error(string.Format("Expected 8-bit register name at line {0}", lineNumber));
                                        return null;
                                    }
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x52);
                                        _programData.Add(andareg);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    ANDX    " + token.TokenValue.ToUpper()
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 2);
                                }
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.ANDY:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected White Space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || (token.TokenName != TokenParser.Tokens.LABEL && token.TokenName != TokenParser.Tokens.POUND))
                                {
                                    Error(string.Format("Expected Immediate value or Register name at line {0}!", lineNumber));
                                    return null;
                                }
                                if (token.TokenName == TokenParser.Tokens.POUND)
                                {
                                    token = tokenParser.GetToken();
                                    if (token == null ||
                                        (token.TokenName != TokenParser.Tokens.NUMBER &&
                                         token.TokenName != TokenParser.Tokens.BINARYNUMBER &&
                                         token.TokenName != TokenParser.Tokens.HEXNUMBER))
                                    {
                                        Error(string.Format("Expected number at line {0}!", lineNumber));
                                        return null;
                                    }
                                    ushort andaval = Get16BitNumber(token);
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x4E);
                                        _programData.Add(BitConverter.GetBytes(andaval)[0]);
                                        _programData.Add(BitConverter.GetBytes(andaval)[1]);
                                       
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    ANDY    #$" + andaval.ToString("X4")
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 3);
                                }
                                else if (token.TokenName == TokenParser.Tokens.LABEL)
                                {
                                    byte andareg = 0;
                                    if (token.TokenValue.ToUpper() == "A")
                                        andareg = 1;
                                    if (token.TokenValue.ToUpper() == "B")
                                        andareg = 2;
                                    if (token.TokenValue.ToUpper() == "D")
                                        andareg = 4;
                                    if (token.TokenValue.ToUpper() == "X")
                                        andareg = 8;
                                    if (token.TokenValue.ToUpper() == "Y")
                                        andareg = 16;
                                    if (andareg == 0)
                                    {
                                        Error(string.Format("Expected 8-bit register name at line {0}", lineNumber));
                                        return null;
                                    }
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x53);
                                        _programData.Add(andareg);

                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    ANDY    " + token.TokenValue.ToUpper()
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 2);
                                }
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.ORA:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected White Space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || (token.TokenName != TokenParser.Tokens.LABEL && token.TokenName != TokenParser.Tokens.POUND))
                                {
                                    Error(string.Format("Expected Immediate value or Register name at line {0}!", lineNumber));
                                    return null;
                                }
                                if (token.TokenName == TokenParser.Tokens.POUND)
                                {
                                    token = tokenParser.GetToken();
                                    if (token == null ||
                                        (token.TokenName != TokenParser.Tokens.NUMBER &&
                                         token.TokenName != TokenParser.Tokens.BINARYNUMBER &&
                                         token.TokenName != TokenParser.Tokens.HEXNUMBER))
                                    {
                                        Error(string.Format("Expected number at line {0}!", lineNumber));
                                        return null;
                                    }
                                    byte andaval = Get8BitNumber(token);
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x54);
                                        _programData.Add(andaval);

                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    ORA    #$" + andaval.ToString("X2")
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 2);
                                }
                                else if (token.TokenName == TokenParser.Tokens.LABEL)
                                {
                                    byte andareg = 0;
                                    if (token.TokenValue.ToUpper() == "A")
                                        andareg = 1;
                                    if (token.TokenValue.ToUpper() == "B")
                                        andareg = 2;
                                    if (andareg == 0)
                                    {
                                        Error(string.Format("Expected 8-bit register name at line {0}", lineNumber));
                                        return null;
                                    }
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x59);
                                        _programData.Add(andareg);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    ORA    " + token.TokenValue.ToUpper()
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 2);
                                }
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.ORB:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected White Space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || (token.TokenName != TokenParser.Tokens.LABEL && token.TokenName != TokenParser.Tokens.POUND))
                                {
                                    Error(string.Format("Expected Immediate value or Register name at line {0}!", lineNumber));
                                    return null;
                                }
                                if (token.TokenName == TokenParser.Tokens.POUND)
                                {
                                    token = tokenParser.GetToken();
                                    if (token == null ||
                                        (token.TokenName != TokenParser.Tokens.NUMBER &&
                                         token.TokenName != TokenParser.Tokens.BINARYNUMBER &&
                                         token.TokenName != TokenParser.Tokens.HEXNUMBER))
                                    {
                                        Error(string.Format("Expected number at line {0}!", lineNumber));
                                        return null;
                                    }
                                    byte andaval = Get8BitNumber(token);
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x55);
                                        _programData.Add(andaval);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    ORB    #$" + andaval.ToString("X2")
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 2);
                                }
                                else if (token.TokenName == TokenParser.Tokens.LABEL)
                                {
                                    byte andareg = 0;
                                    if (token.TokenValue.ToUpper() == "A")
                                        andareg = 1;
                                    if (token.TokenValue.ToUpper() == "B")
                                        andareg = 2;
                                    if (andareg == 0)
                                    {
                                        Error(string.Format("Expected 8-bit register name at line {0}", lineNumber));
                                        return null;
                                    }
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x5A);
                                        _programData.Add(andareg);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    ORB    " + token.TokenValue.ToUpper()
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 2);
                                }
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.ORD:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected White Space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || (token.TokenName != TokenParser.Tokens.LABEL && token.TokenName != TokenParser.Tokens.POUND))
                                {
                                    Error(string.Format("Expected Immediate value or Register name at line {0}!", lineNumber));
                                    return null;
                                }
                                if (token.TokenName == TokenParser.Tokens.POUND)
                                {
                                    token = tokenParser.GetToken();
                                    if (token == null ||
                                        (token.TokenName != TokenParser.Tokens.NUMBER &&
                                         token.TokenName != TokenParser.Tokens.BINARYNUMBER &&
                                         token.TokenName != TokenParser.Tokens.HEXNUMBER))
                                    {
                                        Error(string.Format("Expected number at line {0}!", lineNumber));
                                        return null;
                                    }
                                    ushort andaval = Get16BitNumber(token);
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x56);
                                        _programData.Add(BitConverter.GetBytes(andaval)[0]);
                                        _programData.Add(BitConverter.GetBytes(andaval)[1]);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    ORD    #$" + andaval.ToString("X4")
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 3);
                                }
                                else if (token.TokenName == TokenParser.Tokens.LABEL)
                                {
                                    byte andareg = 0;
                                    if (token.TokenValue.ToUpper() == "A")
                                        andareg = 1;
                                    if (token.TokenValue.ToUpper() == "B")
                                        andareg = 2;
                                    if (token.TokenValue.ToUpper() == "D")
                                        andareg = 4;
                                    if (token.TokenValue.ToUpper() == "X")
                                        andareg = 8;
                                    if (token.TokenValue.ToUpper() == "Y")
                                        andareg = 16;
                                    if (andareg == 0)
                                    {
                                        Error(string.Format("Expected 8-bit register name at line {0}", lineNumber));
                                        return null;
                                    }
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x5B);
                                        _programData.Add(andareg);

                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    ORD    " + token.TokenValue.ToUpper()
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 2);
                                }
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.ORX:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected White Space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || (token.TokenName != TokenParser.Tokens.LABEL && token.TokenName != TokenParser.Tokens.POUND))
                                {
                                    Error(string.Format("Expected Immediate value or Register name at line {0}!", lineNumber));
                                    return null;
                                }
                                if (token.TokenName == TokenParser.Tokens.POUND)
                                {
                                    token = tokenParser.GetToken();
                                    if (token == null ||
                                        (token.TokenName != TokenParser.Tokens.NUMBER &&
                                         token.TokenName != TokenParser.Tokens.BINARYNUMBER &&
                                         token.TokenName != TokenParser.Tokens.HEXNUMBER))
                                    {
                                        Error(string.Format("Expected number at line {0}!", lineNumber));
                                        return null;
                                    }
                                    ushort andaval = Get16BitNumber(token);
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x57);
                                        _programData.Add(BitConverter.GetBytes(andaval)[0]);
                                        _programData.Add(BitConverter.GetBytes(andaval)[1]);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    ORX    #$" + andaval.ToString("X4")
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 3);
                                }
                                else if (token.TokenName == TokenParser.Tokens.LABEL)
                                {
                                    byte andareg = 0;
                                    if (token.TokenValue.ToUpper() == "A")
                                        andareg = 1;
                                    if (token.TokenValue.ToUpper() == "B")
                                        andareg = 2;
                                    if (token.TokenValue.ToUpper() == "D")
                                        andareg = 4;
                                    if (token.TokenValue.ToUpper() == "X")
                                        andareg = 8;
                                    if (token.TokenValue.ToUpper() == "Y")
                                        andareg = 16;
                                    if (andareg == 0)
                                    {
                                        Error(string.Format("Expected 8-bit register name at line {0}", lineNumber));
                                        return null;
                                    }
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x5C);
                                        _programData.Add(andareg);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    ORX    " + token.TokenValue.ToUpper()
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 2);
                                }
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.ORY:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected White Space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || (token.TokenName != TokenParser.Tokens.LABEL && token.TokenName != TokenParser.Tokens.POUND))
                                {
                                    Error(string.Format("Expected Immediate value or Register name at line {0}!", lineNumber));
                                    return null;
                                }
                                if (token.TokenName == TokenParser.Tokens.POUND)
                                {
                                    token = tokenParser.GetToken();
                                    if (token == null ||
                                        (token.TokenName != TokenParser.Tokens.NUMBER &&
                                         token.TokenName != TokenParser.Tokens.BINARYNUMBER &&
                                         token.TokenName != TokenParser.Tokens.HEXNUMBER))
                                    {
                                        Error(string.Format("Expected number at line {0}!", lineNumber));
                                        return null;
                                    }
                                    ushort andaval = Get16BitNumber(token);
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x58);
                                        _programData.Add(BitConverter.GetBytes(andaval)[0]);
                                        _programData.Add(BitConverter.GetBytes(andaval)[1]);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    ORY    #$" + andaval.ToString("X4")
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 3);
                                }
                                else if (token.TokenName == TokenParser.Tokens.LABEL)
                                {
                                    byte andareg = 0;
                                    if (token.TokenValue.ToUpper() == "A")
                                        andareg = 1;
                                    if (token.TokenValue.ToUpper() == "B")
                                        andareg = 2;
                                    if (token.TokenValue.ToUpper() == "D")
                                        andareg = 4;
                                    if (token.TokenValue.ToUpper() == "X")
                                        andareg = 8;
                                    if (token.TokenValue.ToUpper() == "Y")
                                        andareg = 16;
                                    if (andareg == 0)
                                    {
                                        Error(string.Format("Expected 8-bit register name at line {0}", lineNumber));
                                        return null;
                                    }
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x5D);
                                        _programData.Add(andareg);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    ORY    " + token.TokenValue.ToUpper()
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 2);
                                }
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.DIV8:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected White Space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.LABEL)
                                {
                                    Error(string.Format("Expected Register name at line {0}!", lineNumber));
                                    return null;
                                }
                                string sourceRegister42 = token.TokenValue.ToUpper();
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.COMMA)
                                {
                                    Error(string.Format("Expected comma at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.LABEL)
                                {
                                    Error(string.Format("Expected Register name at line {0}!", lineNumber));
                                    return null;
                                }
                                string destinationRegister42 = token.TokenValue.ToUpper();
                                if (sourceRegister42 != "A" && sourceRegister42 != "B")
                                {
                                    Error(string.Format("Register must be an 8-bit register (A or B) at line {0}!", lineNumber));
                                    return null;
                                }
                                if (destinationRegister42 != "A" && destinationRegister42 != "B")
                                {
                                    Error(string.Format("Register must be an 8-bit register (A or B) at line {0}!", lineNumber));
                                    return null;
                                }
                                byte sr42 = 0;
                                byte dr42 = 0;
                                if (sourceRegister42 == "A")
                                    sr42 = 1;
                                if (sourceRegister42 == "B")
                                    sr42 = 2;
                                if (sourceRegister42 == "D")
                                    sr42 = 4;
                                if (sourceRegister42 == "X")
                                    sr42 = 8;
                                if (sourceRegister42 == "Y")
                                    sr42 = 16;
                                if (destinationRegister42 == "A")
                                    dr42 = 1;
                                if (destinationRegister42 == "B")
                                    dr42 = 2;
                                if (destinationRegister42 == "D")
                                    dr42 = 4;
                                if (destinationRegister42 == "X")
                                    dr42 = 8;
                                if (destinationRegister42 == "Y")
                                    dr42 = 16;

                                if (sr42 == 0 || dr42 == 0)
                                {
                                    Error(string.Format("Invalid register name at line {0}!", lineNumber));
                                    return null;
                                }
                                if (phase == 2)
                                {
                                    _programData.Add(0x5E);
                                    _programData.Add(sr42);
                                    _programData.Add(dr42);

                                    _debugInfo.Add(new DebugData
                                    {
                                        Address = progLoc,
                                        SourceCodeLine = "    DIV8    " +sourceRegister42+"," + destinationRegister42
                                    });
                                }
                                progLoc = (ushort)(progLoc + 3);
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.DIV16:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected White Space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.LABEL)
                                {
                                    Error(string.Format("Expected Register name at line {0}!", lineNumber));
                                    return null;
                                }
                                string sourceRegister43 = token.TokenValue.ToUpper();
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.COMMA)
                                {
                                    Error(string.Format("Expected comma at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.LABEL)
                                {
                                    Error(string.Format("Expected Register name at line {0}!", lineNumber));
                                    return null;
                                }
                                string destinationRegister43 = token.TokenValue.ToUpper();
                                if (sourceRegister43 != "X" && sourceRegister43 != "Y" && sourceRegister43 != "D")
                                {
                                    Error(string.Format("Register must be a 16-bit register (X or Y or D) at line {0}!", lineNumber));
                                    return null;
                                }
                                if (destinationRegister43 != "A" && destinationRegister43 != "B" && destinationRegister43 != "D" && destinationRegister43 != "X" && destinationRegister43 != "Y")
                                {
                                    Error(string.Format("Register must be valid at line {0}!", lineNumber));
                                    return null;
                                }
                                byte sr43 = 0;
                                byte dr43 = 0;
                                if (sourceRegister43 == "A")
                                    sr43 = 1;
                                if (sourceRegister43 == "B")
                                    sr43 = 2;
                                if (sourceRegister43 == "D")
                                    sr43 = 4;
                                if (sourceRegister43 == "X")
                                    sr43 = 8;
                                if (sourceRegister43 == "Y")
                                    sr43 = 16;
                                if (destinationRegister43 == "A")
                                    dr43 = 1;
                                if (destinationRegister43 == "B")
                                    dr43 = 2;
                                if (destinationRegister43 == "D")
                                    dr43 = 4;
                                if (destinationRegister43 == "X")
                                    dr43 = 8;
                                if (destinationRegister43 == "Y")
                                    dr43 = 16;

                                if (phase == 2)
                                {
                                    _programData.Add(0x5F);
                                    _programData.Add(sr43);
                                    _programData.Add(dr43);
                                    
                                    _debugInfo.Add(new DebugData
                                    {
                                        Address = progLoc,
                                        SourceCodeLine = "    DIV16    " + sourceRegister43 + "," + destinationRegister43
                                    });
                                }
                                progLoc = (ushort)(progLoc + 3);
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.XORA:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected White Space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || (token.TokenName != TokenParser.Tokens.LABEL && token.TokenName != TokenParser.Tokens.POUND))
                                {
                                    Error(string.Format("Expected Immediate value or Register name at line {0}!", lineNumber));
                                    return null;
                                }
                                if (token.TokenName == TokenParser.Tokens.POUND)
                                {
                                    token = tokenParser.GetToken();
                                    if (token == null ||
                                        (token.TokenName != TokenParser.Tokens.NUMBER &&
                                         token.TokenName != TokenParser.Tokens.BINARYNUMBER &&
                                         token.TokenName != TokenParser.Tokens.HEXNUMBER))
                                    {
                                        Error(string.Format("Expected number at line {0}!", lineNumber));
                                        return null;
                                    }
                                    byte andaval = Get8BitNumber(token);
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x60);
                                        _programData.Add(andaval);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    XORA    #$" + andaval.ToString("X2")
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 2);
                                }
                                else if (token.TokenName == TokenParser.Tokens.LABEL)
                                {
                                    byte andareg = 0;
                                    if (token.TokenValue.ToUpper() == "A")
                                        andareg = 1;
                                    if (token.TokenValue.ToUpper() == "B")
                                        andareg = 2;
                                    if (andareg == 0)
                                    {
                                        Error(string.Format("Expected 8-bit register name at line {0}", lineNumber));
                                        return null;
                                    }
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x65);
                                        _programData.Add(andareg);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    XORA    " + token.TokenValue.ToUpper()
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 2);
                                }
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.XORB:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected White Space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || (token.TokenName != TokenParser.Tokens.LABEL && token.TokenName != TokenParser.Tokens.POUND))
                                {
                                    Error(string.Format("Expected Immediate value or Register name at line {0}!", lineNumber));
                                    return null;
                                }
                                if (token.TokenName == TokenParser.Tokens.POUND)
                                {
                                    token = tokenParser.GetToken();
                                    if (token == null ||
                                        (token.TokenName != TokenParser.Tokens.NUMBER &&
                                         token.TokenName != TokenParser.Tokens.BINARYNUMBER &&
                                         token.TokenName != TokenParser.Tokens.HEXNUMBER))
                                    {
                                        Error(string.Format("Expected number at line {0}!", lineNumber));
                                        return null;
                                    }
                                    byte andaval = Get8BitNumber(token);
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x61);
                                        _programData.Add(andaval);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    XORB    #$" + andaval.ToString("X2")
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 2);
                                }
                                else if (token.TokenName == TokenParser.Tokens.LABEL)
                                {
                                    byte andareg = 0;
                                    if (token.TokenValue.ToUpper() == "A")
                                        andareg = 1;
                                    if (token.TokenValue.ToUpper() == "B")
                                        andareg = 2;
                                    if (andareg == 0)
                                    {
                                        Error(string.Format("Expected 8-bit register name at line {0}", lineNumber));
                                        return null;
                                    }
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x66);
                                        _programData.Add(andareg);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    XORB    " + token.TokenValue.ToUpper()
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 2);
                                }
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.XORD:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected White Space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || (token.TokenName != TokenParser.Tokens.LABEL && token.TokenName != TokenParser.Tokens.POUND))
                                {
                                    Error(string.Format("Expected Immediate value or Register name at line {0}!", lineNumber));
                                    return null;
                                }
                                if (token.TokenName == TokenParser.Tokens.POUND)
                                {
                                    token = tokenParser.GetToken();
                                    if (token == null ||
                                        (token.TokenName != TokenParser.Tokens.NUMBER &&
                                         token.TokenName != TokenParser.Tokens.BINARYNUMBER &&
                                         token.TokenName != TokenParser.Tokens.HEXNUMBER))
                                    {
                                        Error(string.Format("Expected number at line {0}!", lineNumber));
                                        return null;
                                    }
                                    ushort andaval = Get16BitNumber(token);
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x62);
                                        _programData.Add(BitConverter.GetBytes(andaval)[0]);
                                        _programData.Add(BitConverter.GetBytes(andaval)[1]);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    XORD    #$" + andaval.ToString("X4")
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 3);
                                }
                                else if (token.TokenName == TokenParser.Tokens.LABEL)
                                {
                                    byte andareg = 0;
                                    if (token.TokenValue.ToUpper() == "A")
                                        andareg = 1;
                                    if (token.TokenValue.ToUpper() == "B")
                                        andareg = 2;
                                    if (token.TokenValue.ToUpper() == "D")
                                        andareg = 4;
                                    if (token.TokenValue.ToUpper() == "X")
                                        andareg = 8;
                                    if (token.TokenValue.ToUpper() == "Y")
                                        andareg = 16;
                                    if (andareg == 0)
                                    {
                                        Error(string.Format("Expected 8-bit register name at line {0}", lineNumber));
                                        return null;
                                    }
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x67);
                                        _programData.Add(andareg);
                                    }
                                    progLoc = (ushort)(progLoc + 2);
                                }
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.XORX:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected White Space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || (token.TokenName != TokenParser.Tokens.LABEL && token.TokenName != TokenParser.Tokens.POUND))
                                {
                                    Error(string.Format("Expected Immediate value or Register name at line {0}!", lineNumber));
                                    return null;
                                }
                                if (token.TokenName == TokenParser.Tokens.POUND)
                                {
                                    token = tokenParser.GetToken();
                                    if (token == null ||
                                        (token.TokenName != TokenParser.Tokens.NUMBER &&
                                         token.TokenName != TokenParser.Tokens.BINARYNUMBER &&
                                         token.TokenName != TokenParser.Tokens.HEXNUMBER))
                                    {
                                        Error(string.Format("Expected number at line {0}!", lineNumber));
                                        return null;
                                    }
                                    ushort andaval = Get16BitNumber(token);
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x63);
                                        _programData.Add(BitConverter.GetBytes(andaval)[0]);
                                        _programData.Add(BitConverter.GetBytes(andaval)[1]);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    XORX    #$" + andaval.ToString("X4")
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 3);
                                }
                                else if (token.TokenName == TokenParser.Tokens.LABEL)
                                {
                                    byte andareg = 0;
                                    if (token.TokenValue.ToUpper() == "A")
                                        andareg = 1;
                                    if (token.TokenValue.ToUpper() == "B")
                                        andareg = 2;
                                    if (token.TokenValue.ToUpper() == "D")
                                        andareg = 4;
                                    if (token.TokenValue.ToUpper() == "X")
                                        andareg = 8;
                                    if (token.TokenValue.ToUpper() == "Y")
                                        andareg = 16;
                                    if (andareg == 0)
                                    {
                                        Error(string.Format("Expected 8-bit register name at line {0}", lineNumber));
                                        return null;
                                    }
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x68);
                                        _programData.Add(andareg);

                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    XORX    " + token.TokenValue.ToUpper()
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 2);
                                }
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                            case TokenParser.Tokens.XORY:
                                token = tokenParser.GetToken();
                                if (token == null || token.TokenName != TokenParser.Tokens.WHITESPACE)
                                {
                                    Error(string.Format("Expected White Space at line {0}!", lineNumber));
                                    return null;
                                }
                                token = tokenParser.GetToken();
                                if (token == null || (token.TokenName != TokenParser.Tokens.LABEL && token.TokenName != TokenParser.Tokens.POUND))
                                {
                                    Error(string.Format("Expected Immediate value or Register name at line {0}!", lineNumber));
                                    return null;
                                }
                                if (token.TokenName == TokenParser.Tokens.POUND)
                                {
                                    token = tokenParser.GetToken();
                                    if (token == null ||
                                        (token.TokenName != TokenParser.Tokens.NUMBER &&
                                         token.TokenName != TokenParser.Tokens.BINARYNUMBER &&
                                         token.TokenName != TokenParser.Tokens.HEXNUMBER))
                                    {
                                        Error(string.Format("Expected number at line {0}!", lineNumber));
                                        return null;
                                    }
                                    ushort andaval = Get16BitNumber(token);
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x64);
                                        _programData.Add(BitConverter.GetBytes(andaval)[0]);
                                        _programData.Add(BitConverter.GetBytes(andaval)[1]);
                                         
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    XORY    #$" + andaval.ToString("X4")
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 3);
                                }
                                else if (token.TokenName == TokenParser.Tokens.LABEL)
                                {
                                    byte andareg = 0;
                                    if (token.TokenValue.ToUpper() == "A")
                                        andareg = 1;
                                    if (token.TokenValue.ToUpper() == "B")
                                        andareg = 2;
                                    if (token.TokenValue.ToUpper() == "D")
                                        andareg = 4;
                                    if (token.TokenValue.ToUpper() == "X")
                                        andareg = 8;
                                    if (token.TokenValue.ToUpper() == "Y")
                                        andareg = 16;
                                    if (andareg == 0)
                                    {
                                        Error(string.Format("Expected 8-bit register name at line {0}", lineNumber));
                                        return null;
                                    }
                                    if (phase == 2)
                                    {
                                        _programData.Add(0x69);
                                        _programData.Add(andareg);
                                        
                                        _debugInfo.Add(new DebugData
                                        {
                                            Address = progLoc,
                                            SourceCodeLine = "    XORY    " + token.TokenValue.ToUpper()
                                        });
                                    }
                                    progLoc = (ushort)(progLoc + 2);
                                }
                                token = tokenParser.GetToken();
                                while (token != null &&
                                       (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                                        token.TokenName == TokenParser.Tokens.COMMENT))
                                {
                                    token = tokenParser.GetToken();
                                }
                                continue;
                        }
                    }
                }
                debugStart = progLoc;
            }

            var prog = new B33Program
            {
                DebugStartAddress = debugStart,
                ExecutionAddress = executionAddress,
                LabelDictionary = _labelDictionary
            };

            return prog;
        }

        private void Error(string errorMessage)
        {
            ErrorMessage = errorMessage;
            Successful = false;
        }

        private byte Get8BitNumber(Token token)
        {
            if (token.TokenName == TokenParser.Tokens.NUMBER || token.TokenName == TokenParser.Tokens.HEXNUMBER ||
                token.TokenName == TokenParser.Tokens.BINARYNUMBER)
            {
                if (token.TokenName == TokenParser.Tokens.NUMBER)
                {
                    return byte.Parse(token.TokenValue);
                }
                if (token.TokenName == TokenParser.Tokens.HEXNUMBER)
                {
                    return Convert.ToByte(token.TokenValue.Remove(0, 1), 16);
                }
                if (token.TokenName == TokenParser.Tokens.BINARYNUMBER)
                {
                    return Convert.ToByte(token.TokenValue.Remove(0, 1), 2);
                }
            }
            return 0;
        }

        private ushort Get16BitNumber(Token token)
        {
            if (token.TokenName == TokenParser.Tokens.NUMBER || token.TokenName == TokenParser.Tokens.HEXNUMBER ||
                token.TokenName == TokenParser.Tokens.BINARYNUMBER)
            {
                if (token.TokenName == TokenParser.Tokens.NUMBER)
                {
                    return ushort.Parse(token.TokenValue);
                }
                if (token.TokenName == TokenParser.Tokens.HEXNUMBER)
                {
                    return Convert.ToUInt16(token.TokenValue.Remove(0, 1), 16);
                }
                if (token.TokenName == TokenParser.Tokens.BINARYNUMBER)
                {
                    return Convert.ToUInt16(token.TokenValue.Remove(0, 1), 2);
                }
            }
            return 0;
        }

        public class B33Program
        {
            public ushort ExecutionAddress { get; set; }
            public ushort DebugStartAddress { get; set; }
            public ushort StartAddress { get; set; }
            public ushort EndAddress { get; set; }
            public byte[] ProgramBytes { get; set; }
            public OutputTypes ProgramType { get; set; }

            public Dictionary<string, ushort> LabelDictionary { get; set; }
        }
    }
}
