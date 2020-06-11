using System;
using System.Collections.Generic;
using System.Linq;
using B33Assembler;

namespace B33VirtualMachineTests
{
    class Program
    {
        private static List<string> _results;
        private static int _testCounter;

        static void Main()
        {
            _testCounter = 0;
            _results = new List<string>();

            TestLdaImmediate();
            TestLdbImmediate();
            TestLddImmediate();
            TestLdxImmediate();
            TestLdyImmediate();

            TestStaExtended();
            TestStbExtended();
            TestStdExtended();
            TestStxExtended();
            TestStyExtended();

            TestLdaExtended();
            TestLdbExtended();
            TestLddExtended();
            TestLdxExtended();
            TestLdyExtended();

            TestSta0IndexedNoIncDec();
            TestSta0Indexed1Inc();
            TestSta0Indexed2Inc();
            TestStaBIndexedNoIncDec();

            TestStb0IndexedNoIncDec();
            TestStb0Indexed1Inc();
            TestStb0Indexed2Inc();
            TestStbAIndexedNoIncDec();

            TestCallAndRet();
            TestPushAndPop();

            TestAddaRegister();
            TestAddaImmediate();

            TestSubaRegister();
            TestSubaImmediate();

            //TestSound();

            while (_testCounter != 0)
            {
                System.Threading.Thread.Sleep(200);
            }

            PrintResults();
            Console.ReadKey();
        }
        // 03 00 FE 01 E8 10 00 28 01 03 10 00 28 01 E8 10 00 28 01 03 10 00 28 01 01 10 00 08 00
        private static void TestSound()
        {
            CreateTest("Sound", new byte[] { 0x03, 0x00, 0xFE, 0x01, 0xE8, 0x10, 0x00, 0x28, 0x01, 0x03, 0x10, 0x00, 0x28, 0x01, 0xE8, 0x10, 0x00, 0x28, 0x01, 0x03, 0x10, 0x00, 0x28, 0x01, 0x01, 0x10, 0x00, 0x08, 0x03, 0x00, 0xFE, 0x01, 0xE8, 0x10, 0x00, 0x28, 0x01, 0x04, 0x10, 0x00, 0x28, 0x01, 0xE8, 0x10, 0x00, 0x28, 0x01, 0x03, 0x10, 0x00, 0x28, 0x01, 0x01, 0x10, 0x00, 0x08, 0x00 });
        }

        private static void TestSubaRegister()
        {
            // Start LDA #49
            //  LDB #32
            //  SUBA B
            //  END Start

            string program = string.Format("Start LDA #49{0} LDB #32{0} SUBA B{0} END Start", Environment.NewLine);
            CreateTest("SUBA Register Test", AssembleProgram(program).ProgramBytes);
        }

        private static void TestSubaImmediate()
        {
            // Start LDA #49
            //  SUBA #32
            //  END Start

            string program = string.Format("Start LDA #49{0} SUBA #32{0} END Start", Environment.NewLine);
            CreateTest("SUBA Immediate Test", AssembleProgram(program).ProgramBytes);
        }

        private static void TestAddaRegister()
        {
            // Start LDA #32
            //  LDB #49
            //  ADDA B
            //  END Start

            string program = string.Format("Start LDA #32{0} LDB #49{0} ADDA B{0} END Start", Environment.NewLine);
            CreateTest("ADDA Register Test", AssembleProgram(program).ProgramBytes);
        }

        private static void TestAddaImmediate()
        {
            // Start LDA #32
            //  ADDA #49
            //  END Start

            string program = string.Format("Start LDA #32{0} ADDA #49{0} END Start", Environment.NewLine);
            CreateTest("ADDA Immediate Test", AssembleProgram(program).ProgramBytes);
        }

        private static void TestPushAndPop()
        {
            // Start LDX #$1234
            //  LDA #$69
            //  LDB #$93
            //  PUSH A
            //  PUSH B
            //  PUSH X
            //  LDX #0
            //  LDD #0
            //  POP X
            //  POP B
            //  POP A
            //  END Start

            string program = string.Format("Start LDX #$1234{0} LDA #$69{0} LDB #$93{0} PUSH A{0} PUSH B{0} PUSH X{0} LDX #0{0} LDD #0{0} POP X{0} POP B{0} POP A{0} END Start", Environment.NewLine);
            CreateTest("Push/Pop Test", AssembleProgram(program).ProgramBytes);
        }

        private static void TestCallAndRet()
        {
            // Start LDX #$ABCD
            //  CALL MySub
            //  JMP Finish
            // MySub LDX #$9876
            //  LDY #$5432
            //  RET
            // Finish END Start

            string program = string.Format("Start LDX #$ABCD{0} CALL MySub{0} JMP Finish{0}MySub LDX #$9876{0} LDY #$5432{0} RET{0}Finish END Start", Environment.NewLine);
            CreateTest("Call/Ret Test", AssembleProgram(program).ProgramBytes);
        }

        private static void TestSta0IndexedNoIncDec()
        {
            // Start LDX #$2000
            //  LDA #100
            //  STA ,X
            //  END Start
            string program = string.Format("Start LDX #$2000{0} LDA #100{0} STA ,X{0} END Start", Environment.NewLine);
            CreateTest("STA Indexed Test (0 Offset, no inc/dec)", AssembleProgram(program).ProgramBytes);
        }

        private static void TestStb0IndexedNoIncDec()
        {
            // Start LDX #$2000
            //  LDB #100
            //  STB ,X
            //  END Start
            string program = string.Format("Start LDX #$2000{0} LDB #100{0} STB ,X{0} END Start", Environment.NewLine);
            CreateTest("STB Indexed Test (0 Offset, no inc/dec)", AssembleProgram(program).ProgramBytes);
        }

        private static void TestSta0Indexed1Inc()
        {
            // Start LDX #$2000
            //  LDA #100
            //  STA ,X+
            //  END Start
            string program = string.Format("Start LDX #$2000{0} LDA #100{0} STA ,X+{0} END Start", Environment.NewLine);
            CreateTest("STA Indexed Test (0 Offset, +1 Increment)", AssembleProgram(program).ProgramBytes);
        }

        private static void TestStb0Indexed1Inc()
        {
            // Start LDX #$2000
            //  LDB #100
            //  STB ,X+
            //  END Start
            string program = string.Format("Start LDX #$2000{0} LDB #100{0} STB ,X+{0} END Start", Environment.NewLine);
            CreateTest("STB Indexed Test (0 Offset, +1 Increment)", AssembleProgram(program).ProgramBytes);
        }

        private static void TestSta0Indexed2Inc()
        {
            // Start LDX #$2000
            //  LDA #100
            //  STA ,X++
            //  END Start
            string program = string.Format("Start LDX #$2000{0} LDA #100{0} STA ,X++{0} END Start", Environment.NewLine);
            CreateTest("STA Indexed Test (0 Offset, +2 Increment)", AssembleProgram(program).ProgramBytes);
        }

        private static void TestStb0Indexed2Inc()
        {
            // Start LDX #$2000
            //  LDB #100
            //  STB ,X++
            //  END Start
            string program = string.Format("Start LDX #$2000{0} LDB #100{0} STB ,X++{0} END Start", Environment.NewLine);
            CreateTest("STB Indexed Test (0 Offset, +2 Increment)", AssembleProgram(program).ProgramBytes);
        }

        private static void TestStaBIndexedNoIncDec()
        {
            // Start LDX #$2000
            //  LDA #100
            //  LDB #$0A
            //  STA B,X
            //  END Start
            string program = string.Format("Start LDX #$2000{0} LDA #100{0} LDB #$0A{0} STA B,X{0} END Start", Environment.NewLine);
            CreateTest("STA Indexed Test (Offset by B, no inc/dec)", AssembleProgram(program).ProgramBytes);
        }

        private static void TestStbAIndexedNoIncDec()
        {
            // Start LDX #$2000
            //  LDA #$0A
            //  LDB #100
            //  STB A,X
            //  END Start
            string program = string.Format("Start LDX #$2000{0} LDA #$0A{0} LDB #100{0} STB A,X{0} END Start", Environment.NewLine);
            CreateTest("STB Indexed Test (Offset by A, no inc/dec)", AssembleProgram(program).ProgramBytes);
        }

        private static void TestLdaExtended()
        {
            // Start LDX #$1234
            //  STX $2000
            //  LDA $2000
            //  END Start
            string program = string.Format("Start LDX #$1234{0} STX $2000{0} LDA $2000{0} END Start", Environment.NewLine);
            CreateTest("LDA Extended Test ", AssembleProgram(program).ProgramBytes);
        }

        private static void TestLdbExtended()
        {
            // Start LDX #$5678
            //  STX $2000
            //  LDB $2000
            //  END Start
            string program = string.Format("Start LDX #$5678{0} STX $2000{0} LDB $2000{0} END Start", Environment.NewLine);
            CreateTest("LDB Extended Test ", AssembleProgram(program).ProgramBytes);
        }

        private static void TestLddExtended()
        {
            // Start LDX #$90A1
            //  STX $3000
            //  LDD $3000
            //  END Start
            string program = string.Format("Start LDX #$90A1{0} STX $3000{0} LDD $3000{0} END Start", Environment.NewLine);
            CreateTest("LDD Extended Test ", AssembleProgram(program).ProgramBytes );
        }

        private static void TestLdxExtended()
        {
            // Start LDY #$D56F
            //  STY $4000
            //  LDX $4000
            //  END Start
            string program = string.Format("Start LDY #$D56F{0} STY $4000{0} LDX $4000{0} END Start", Environment.NewLine);
            CreateTest("LDX Extended Test ", AssembleProgram(program).ProgramBytes);
        }

        private static void TestLdyExtended()
        {
            // Start LDX #$EA19
            //  STX $5000
            //  LDY $5000
            //  END Start
            string program = string.Format("Start LDX #$EA19{0} STX $5000{0} LDY $5000{0} END Start", Environment.NewLine);
            CreateTest("LDY Extended Test ", AssembleProgram(program).ProgramBytes);
        }

        private static void TestStaExtended()
        {
            // Start LDA #99
            //  STA $2000
            //  END Start
            string program = string.Format("Start LDA #99{0} STA $2000{0} END Start", Environment.NewLine);
            CreateTest("STA Extended Test ", AssembleProgram(program).ProgramBytes);
        }

        private static void TestStbExtended()
        {
            // Start LDB #45
            //  STB $3000
            //  END Start
            string program = string.Format("Start LDB #45{0} STB $3000{0} END Start", Environment.NewLine);
            CreateTest("STB Extended Test ", AssembleProgram(program).ProgramBytes);
        }

        private static void TestStdExtended()
        {
            // Start LDD #$5678
            //  STD $5000
            //  END Start
            string program = string.Format("Start LDD #$5678{0} STD $5000{0} END Start", Environment.NewLine);
            CreateTest("STD Extended Test ", AssembleProgram(program).ProgramBytes);
        }

        private static void TestStxExtended()
        {
            // Start LDX #$1234
            //  STX $4000
            //  END Start
            string program = string.Format("Start LDX #$1234{0} STX $4000{0} END Start", Environment.NewLine);
            CreateTest("STX Extended Test ", AssembleProgram(program).ProgramBytes);
        }

        private static void TestStyExtended()
        {
            // Start LDY #$90A1
            //  STY $6000
            //  END Start
            string program = string.Format("Start LDY #$90A1{0} STY $6000{0} END Start", Environment.NewLine);
            CreateTest("STY Extended Test ", AssembleProgram(program).ProgramBytes);
        }

        private static void TestLdaImmediate()
        {
            // Start LDA #99
            //  END Start
            string program = string.Format("Start LDA #99{0} END Start", Environment.NewLine);

            CreateTest("LDA Immediate Test", AssembleProgram(program).ProgramBytes);
        }

        private static void TestLdbImmediate()
        {
            // Start LDB #12
            // END Start
            string program = string.Format("Start LDB #12{0} END Start", Environment.NewLine);

            CreateTest("LDB Immediate Test", AssembleProgram(program).ProgramBytes);
        }

        private static void TestLddImmediate()
        {
            // Start LDD #$A742
            //  END Start
            string program = string.Format("Start LDD #$A742{0} END Start", Environment.NewLine);

            CreateTest("LDD Immediate Test", AssembleProgram(program).ProgramBytes);
        }

        private static void TestLdxImmediate()
        {
            // Start LDX #$FE23
            //  END Start
            string program = string.Format("Start LDX #$FE23{0} End Start", Environment.NewLine);
            
            CreateTest("LDX Immediate Test", AssembleProgram(program).ProgramBytes);
        }

        private static void TestLdyImmediate()
        {
            // Start LDY #$920A
            //  END Start
            string program = string.Format("Start LDY #$920A{0} End Start", Environment.NewLine);

            CreateTest("LDY Immediate Test", AssembleProgram(program).ProgramBytes);
        }

        private static void CreateTest(string name, byte[] program)
        {
            var cpu = new B33Cpu.B33Cpu { Name = name };
            cpu.Hardware.Add(new B33Cpu.Hardware.B33Sound());

            cpu.B33Stopped += B33Stopped;
            cpu.LoadProgram(program, 0x1000, 0x1000);
            cpu.Start();
            _testCounter++;
        }

        private static void B33Stopped(B33Cpu.B33Cpu sender, bool isStore, ushort storeAddress)
        {
            _testCounter--;
            if (sender.Name == "LDA Immediate Test")
            {
                if (RegistersEqual(sender.Registers, 99, 0, 0, 0))
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Passed!");
                    }
                }
                else
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Failed :(");
                    }
                }
            }
            if (sender.Name == "LDB Immediate Test")
            {
                if (RegistersEqual(sender.Registers, 0, 12, 0, 0))
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Passed!");
                    }
                }
                else
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Failed :(");
                    }
                }
            }
            if (sender.Name == "LDD Immediate Test")
            {
                if (RegistersEqual(sender.Registers, 0xA7, 0x42, 0, 0))
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Passed!");
                    }
                }
                else
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Failed :(");
                    }
                }
            }
            if (sender.Name == "LDX Immediate Test")
            {
                if (RegistersEqual(sender.Registers, 0, 0, 0xFE23, 0))
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Passed!");
                    }
                }
                else
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Failed :(");
                    }
                }
            }
            if (sender.Name == "LDY Immediate Test")
            {
                if (RegistersEqual(sender.Registers, 0, 0, 0, 0x920A))
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Passed!");
                    }
                }
                else
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Failed :(");
                    }
                }
            }
            if (sender.Name == "STA Extended Test ")
            {
                if (RegistersEqual(sender.Registers, 99, 0, 0, 0) && sender.Peek(0x2000) == 99)
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Passed!");
                    }
                }
                else
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Failed :(");
                    }
                }
            }
            if (sender.Name == "STB Extended Test ")
            {
                if (RegistersEqual(sender.Registers, 0, 45, 0, 0) && sender.Peek(0x3000) == 45)
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Passed!");
                    }
                }
                else
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Failed :(");
                    }
                }
            }
            if (sender.Name == "STD Extended Test ")
            {
                if (RegistersEqual(sender.Registers, 0x56, 0x78, 0, 0) && sender.Peek(0x5000) == 0x78 && sender.Peek(0x5001) == 0x56)
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Passed!");
                    }
                }
                else
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Failed :(");
                    }
                }
            }
            if (sender.Name == "STX Extended Test ")
            {
                if (RegistersEqual(sender.Registers, 0, 0, 0x1234, 0) && sender.Peek(0x4000) == 0x34 && sender.Peek(0x4001) == 0x12)
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Passed!");
                    }
                }
                else
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Failed :(");
                    }
                }
            }
            if (sender.Name == "STY Extended Test ")
            {
                if (RegistersEqual(sender.Registers, 0, 0, 0, 0x90A1) && sender.Peek(0x6000) == 0xA1 && sender.Peek(0x6001) == 0x90)
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Passed!");
                    }
                }
                else
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Failed :(");
                    }
                }
            }
            if (sender.Name == "LDA Extended Test ")
            {
                if (RegistersEqual(sender.Registers, 0x34, 0, 0x1234, 0))
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Passed!");
                    }
                }
                else
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Failed :(");
                    }
                }
            }
            if (sender.Name == "LDB Extended Test ")
            {
                if (RegistersEqual(sender.Registers, 0, 0x78, 0x5678, 0))
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Passed!");
                    }
                }
                else
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Failed :(");
                    }
                }
            }
            if (sender.Name == "LDD Extended Test ")
            {
                if (RegistersEqual(sender.Registers, 0x90, 0xA1, 0x90A1, 0))
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Passed!");
                    }
                }
                else
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Failed :(");
                    }
                }
            }
            if (sender.Name == "LDX Extended Test ")
            {
                if (RegistersEqual(sender.Registers, 0x00, 0x00, 0xD56F, 0xD56F))
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Passed!");
                    }
                }
                else
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Failed :(");
                    }
                }
            }
            if (sender.Name == "LDY Extended Test ")
            {
                if (RegistersEqual(sender.Registers, 0x00, 0x00, 0xEA19, 0xEA19))
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Passed!");
                    }
                }
                else
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Failed :(");
                    }
                }
            }
            if (sender.Name == "STA Indexed Test (0 Offset, no inc/dec)")
            {
                if (RegistersEqual(sender.Registers, 100, 0x00, 0x2000, 0x0000) && sender.Peek(0x2000) == 100)
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Passed!");
                    }
                }
                else
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Failed :(");
                    }
                }
            }
            if (sender.Name == "STB Indexed Test (0 Offset, no inc/dec)")
            {
                if (RegistersEqual(sender.Registers, 0x00, 100, 0x2000, 0x0000) && sender.Peek(0x2000) == 100)
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Passed!");
                    }
                }
                else
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Failed :(");
                    }
                }
            }
            if (sender.Name == "STA Indexed Test (0 Offset, +1 Increment)")
            {
                if (RegistersEqual(sender.Registers, 100, 0x00, 0x2001, 0x0000) && sender.Peek(0x2000) == 100)
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Passed!");
                    }
                }
                else
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Failed :(");
                    }
                }
            }
            if (sender.Name == "STB Indexed Test (0 Offset, +1 Increment)")
            {
                if (RegistersEqual(sender.Registers, 0x00, 100, 0x2001, 0x0000) && sender.Peek(0x2000) == 100)
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Passed!");
                    }
                }
                else
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Failed :(");
                    }
                }
            }
            if (sender.Name == "STA Indexed Test (0 Offset, +2 Increment)")
            {
                if (RegistersEqual(sender.Registers, 100, 0x00, 0x2002, 0x0000) && sender.Peek(0x2000) == 100)
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Passed!");
                    }
                }
                else
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Failed :(");
                    }
                }
            }
            if (sender.Name == "STB Indexed Test (0 Offset, +2 Increment)")
            {
                if (RegistersEqual(sender.Registers, 0x00, 100, 0x2002, 0x0000) && sender.Peek(0x2000) == 100)
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Passed!");
                    }
                }
                else
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Failed :(");
                    }
                }
            }
            if (sender.Name == "STA Indexed Test (Offset by B, no inc/dec)")
            {
                if (RegistersEqual(sender.Registers, 100, 0x0A, 0x2000, 0x0000) && sender.Peek(0x200A) == 100)
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Passed!");
                    }
                }
                else
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Failed :(");
                    }
                }
            }
            if (sender.Name == "STB Indexed Test (Offset by A, no inc/dec)")
            {
                if (RegistersEqual(sender.Registers, 0x0A, 100, 0x2000, 0x0000) && sender.Peek(0x200A) == 100)
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Passed!");
                    }
                }
                else
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Failed :(");
                    }
                }
            }
            if (sender.Name == "Call/Ret Test")
            {
                if (RegistersEqual(sender.Registers, 0, 0, 0x9876, 0x5432))
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Passed!");
                    }
                }
                else
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Failed :(");
                    }
                }
            }
            if (sender.Name == "Push/Pop Test")
            {
                if (RegistersEqual(sender.Registers, 0x69, 0x93, 0x1234, 0))
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Passed!");
                    }
                }
                else
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Failed :(");
                    }
                }
            }
            if (sender.Name == "ADDA Register Test")
            {
                if (RegistersEqual(sender.Registers, 81, 49, 0, 0))
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Passed!");
                    }
                }
                else
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Failed :(");
                    }
                }
            }
            if (sender.Name == "ADDA Immediate Test")
            {
                if (RegistersEqual(sender.Registers, 81, 0, 0, 0))
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Passed!");
                    }
                }
                else
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Failed :(");
                    }
                }
            }
            if (sender.Name == "SUBA Register Test")
            {
                if (RegistersEqual(sender.Registers, 17, 32, 0, 0))
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Passed!");
                    }
                }
                else
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Failed :(");
                    }
                }
            }
            if (sender.Name == "SUBA Immediate Test")
            {
                if (RegistersEqual(sender.Registers, 17, 0, 0, 0))
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Passed!");
                    }
                }
                else
                {
                    lock (_results)
                    {
                        _results.Add(sender.Name.PadRight(50, ' ') + " ==> Failed :(");
                    }
                }
            }
        }

        private static bool RegistersEqual(B33Cpu.B33Registers r1, B33Cpu.B33Registers r2)
        {
            return r1.A == r2.A && r1.B == r2.B && r1.D == r2.D && r1.X == r2.X && r1.Y == r2.Y;
        }

        private static bool RegistersEqual(B33Cpu.B33Registers r1, byte a, byte b, ushort x, ushort y)
        {
            var registers = new B33Cpu.B33Registers {A = a, B = b, X = x, Y = y};

            return RegistersEqual(r1, registers);
        }

        private static Assembler.B33Program AssembleProgram(string program)
        {
            var assembler = new Assembler
            {
                IncludeDebugInformation = false,
                Origin = 0x1000,
                OutputType = OutputTypes.RawBinary,
                Program = program
            };

            return assembler.Assemble();
        }

        private static void PrintResults()
        {
            foreach (var result in _results.OrderByDescending(z => z.EndsWith("Passed!")).ThenBy(z => z))
            {
                Console.WriteLine(result);
            }
        }
    }
}
