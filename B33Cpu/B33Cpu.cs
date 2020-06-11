using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using B33Cpu.Hardware;

namespace B33Cpu
{
    public class B33Cpu
    {
        #region "Enumerations"
        /// <summary>
        /// Various states the cpu can be in
        /// </summary>
        public enum States
        {
            /// <summary>
            /// The cpu is running
            /// </summary>
            Running,
            /// <summary>
            /// The cpu is stopped
            /// </summary>
            Stopped,
            /// <summary>
            /// The cpu is paused
            /// </summary>
            Paused
        };
        #endregion

        #region "Delegates"
        public delegate void B33EventArgs(B33Cpu sender, bool isOpcodeStore, ushort storeAddress);
        public delegate void RegistersChangedEvent(B33Cpu sender);
        #endregion

        #region "Events"
        /// <summary>
        /// Occurs when the B33 Virtual Machine has stopped.
        /// </summary>
        public event B33EventArgs B33Stopped;
        /// <summary>
        /// Occurs when the B33 Virtual Machine hits a break point
        /// and the BRK opcode get hit.
        /// </summary>
        public event B33EventArgs B33BreakPointHit;
        /// <summary>
        /// Occurs when the B33 Virtual Machine has started.
        /// </summary>
        public event B33EventArgs B33Started;
        /// <summary>
        /// Occurs when the B33 Virtual Machine has paused.
        /// </summary>
        public event B33EventArgs B33Paused;
        /// <summary>
        /// Occurs when the B33 Virtual Machine has resumed from being paused.
        /// </summary>
        public event B33EventArgs B33Resumed;
        /// <summary>
        /// Occurs when the B33 Virtual Machine is about to execute an instruction
        /// </summary>
        public event B33EventArgs B33PreOpcodeExecute;
        /// <summary>
        /// Occurs right after the B33 Virtual Machine has executed an instruction
        /// </summary>
        public event B33EventArgs B33PostOpcodeExecute;
        /// <summary>
        /// Occurs when a register has changed value.
        /// </summary>
        public event RegistersChangedEvent RegistersChanged;
        #endregion

        #region "Constants"
        internal const ushort MemorySize = 64 * 1024 - 1; //64K of memory
        #endregion

        #region "Properties"
        /// <summary>
        /// Gets or sets the registers of the Cpu (X, Y, A, B, D)
        /// </summary>
        /// <value>The registers (X, Y, A, B, D).</value>
        public B33Registers Registers { private set; get; }
        /// <summary>
        /// Gets or sets B33 hardware devices.
        /// </summary>
        /// <value>The hardware device.</value>
        public List<IB33Hardware> Hardware { private set; get; }
        /// <summary>
        /// Gets or sets the program.
        /// </summary>
        /// <value>The program.</value>
        public B33Program Program { private set; get; }
        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>The state.</value>
        public States State { private set; get; }
        /// <summary>
        /// Gets or sets the speed.
        /// </summary>
        /// <value>The speed (in ms). Set to 0 for real time.</value>
        public uint Speed { set; get; }
        /// <summary>
        /// Gets or sets the name of this specific B33 Cpu instance.
        /// </summary>
        /// <value>The name of this B33 Cpu instance.</value>
        public string Name { get; set; }
        #endregion

        #region "Fields"
        private readonly byte[] _memory;
        private Thread _progThread;
        private bool _stopRequested;
        private bool _isPaused;
        private readonly Stack<byte> _8BitStack;
        private readonly Stack<ushort> _16BitStack;
        private readonly Stack<ushort> _callStack;
        private readonly Random _rnd;
        private ManualResetEvent _pauseEvent;
        #endregion

        #region "Constructor"
        public B33Cpu()
        {
            // Initialize all registers to 0
            Registers = new B33Registers { A = 0, B = 0, Cc = 0, Pc = 0, X = 0, Y = 0 };
            // Initialize a new hardware list
            Hardware = new List<IB33Hardware>();
            // Create a new empty program
            Program = new B33Program();
            // Initialize a new 8 bit stack
            _8BitStack = new Stack<byte>();
            // Initialize a new 16 bit stack
            _16BitStack = new Stack<ushort>();
            // Initialize a new call stack
            _callStack = new Stack<ushort>();
            // Allocate our memory
            _memory = new byte[MemorySize];
            _stopRequested = false;
            _rnd = new Random();
            // Set default speed to 0 (real time)
            Speed = 0;
            // Set the state of the CPU to stopped
            State = States.Stopped;
            _isPaused = false;
        }
        #endregion

        // This method is called whenever a register has changed.
        // It raises the RegistersChanged event
        private void InvokeRegistersChanged()
        {
            // Raise the RegistersChanged event
            if (RegistersChanged != null)
                RegistersChanged(this);
        }

        /// <summary>
        /// Stops this instances of the B33 Cpu.
        /// </summary>
        public void Stop()
        {
            _stopRequested = true;
            if (_pauseEvent != null)
                _pauseEvent.Reset();
            State = States.Stopped;
        }

        /// <summary>
        /// Pauses or resumes this instance of the B33 Cpu.
        /// </summary>
        public void Pause()
        {

            if (_isPaused)
            {
                State = States.Running;
                _pauseEvent.Set();
                if (B33Resumed != null)
                    B33Resumed(this, false, 0);
            }
            else
            {
                State = States.Paused;
                _pauseEvent.Reset();
                if (B33Paused != null)
                    B33Paused(this, false, 0);
            }
            _isPaused = !_isPaused;
        }

        /// <summary>
        /// Breaks (pauses) this instance of the B33 Cpu.
        /// </summary>
        public void Break()
        {
            State = States.Paused;
            _pauseEvent.Reset();
            if (B33BreakPointHit != null)
                B33BreakPointHit(this, false, 0);

            _isPaused = true;
        }

        /// <summary>
        /// Resets this instance of the B33 virtual machine.
        /// </summary>
        public void Reset()
        {
            Registers.A = 0;
            Registers.B = 0;
            Registers.Cc = 0;
            Registers.Pc = 0;
            Registers.X = 0;
            Registers.Y = 0;

            _8BitStack.Clear();
            _16BitStack.Clear();
            _callStack.Clear();
            _stopRequested = false;

            foreach (IB33Hardware h in Hardware)
            {
                h.Reset();
            }
            InvokeRegistersChanged();
        }

        /// <summary>
        /// Determines whether a given file is a valid B33 binary file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns><c>true</c> if the file is a valid B33 binary file; otherwise, <c>false</c>.</returns>
        public bool IsValidFile(string fileName)
        {
            byte[] magicNumbers;

            // If the file doesn't exist, it's obviously invalid
            if (!File.Exists(fileName))
                return false;

            // Open the file
            using (var reader = new BinaryReader(File.Open(fileName, FileMode.Open), Encoding.ASCII))
            {
                // Read the first 3 bytes (magic numbers)
                magicNumbers = reader.ReadBytes(3);
                reader.Close();
            }

            // If the magic numbers are 66, 51 and 51, then its looks valid!
            if (magicNumbers[0] == 66 && magicNumbers[1] == 51 && magicNumbers[2] == 51)
                return true;

            return false;
        }

        /// <summary>
        /// Loads a program from a byte array into this virtual machine, optionally setting whether it requires dual monitors.
        /// </summary>
        /// <param name="program">The program.</param>
        /// <param name="startAddress">The start address of the program.</param>
        /// <param name="execAddress">The execute address of the program.</param>
        /// <param name="dualMonitors">if set to <c>true</c>, then this program requires dual monitors.</param>
        public void LoadProgram(byte[] program, ushort startAddress, ushort execAddress, bool dualMonitors)
        {
            // Reset B33 virtual machine
            Reset();
            // Set filename to <none> since it was loaded from a byte array and not from a file
            Program.FileName = "<none>";
            // No debug data since it was loaded from a byte array
            Program.DebugData.Clear();
            // Set the start address
            Program.StartAddress = startAddress;
            // Set the execute address
            Program.ExecAddress = execAddress;

            // Load the program into the B33 Program structure
            for (int i = 0; i < program.Length; i++)
            {
                Program.Program[startAddress + i] = program[i];
            }

            // Set the ending address by calculating the starting address plus the length
            Program.EndAddress = (ushort)(startAddress + program.Length);
            // No debug information since it was loaded from a byte array
            Program.HasDebugInfo = false;

            // Set weather it requires dual monitors
            Program.DualMonitorRequired = dualMonitors;

            // Set the program counter to the execution address
            Registers.Pc = execAddress;
            // Load the program into the B33 virtual machine's memory
            Array.Copy(Program.Program, _memory, MemorySize);
        }

        /// <summary>
        /// Loads a program from a byte array into this virtual machine.
        /// </summary>
        /// <param name="program">The program.</param>
        /// <param name="startAddress">The start address of the program.</param>
        /// <param name="execAddress">The execute address of the program.</param>
        public void LoadProgram(byte[] program, ushort startAddress, ushort execAddress)
        {
            LoadProgram(program, startAddress, execAddress, false);
        }

        /// <summary>
        /// Loads the program from a file into this virtual machine.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public void LoadProgram(string fileName)
        {
            // Reset B33 Virtual Machine
            Reset();
            // Set filename to 
            Program.FileName = fileName;
            // Clear Debug Data
            Program.DebugData.Clear();

            // Open File
            using (var reader = new BinaryReader(File.Open(fileName, FileMode.Open), Encoding.ASCII))
            {
                // Skip past "magic numbers"
                reader.BaseStream.Seek(3, SeekOrigin.Begin);
                // Read in start address
                Program.StartAddress = reader.ReadUInt16();
                // Read in Execution address
                Program.ExecAddress = reader.ReadUInt16();
                // Read in Debug Info Address (will be 0 if there is no debug info)
                Program.DebugInfoAddress = reader.ReadUInt16();
                // Read dual monitor byte (true or false. 0 or 1)
                byte dualMonitor = reader.ReadByte();
                // Set whether dual monitors are required
                Program.DualMonitorRequired = dualMonitor != 0;

                // Temporarily save the start address
                ushort tmp = Program.StartAddress;

                // Read until end of file reached
                while (reader.PeekChar() != -1)
                {
                    // Read the program bytes from the file into the B33 Program structure
                    Program.Program[Program.StartAddress] = reader.ReadByte();
                    // Increment by one
                    Program.StartAddress = (ushort)(Program.StartAddress + 1);
                }

                // Close the file
                reader.Close();

                // Set the program counter equal to the execution address
                Registers.Pc = Program.ExecAddress;
                // Set the ending address
                Program.EndAddress = (ushort)(Program.StartAddress - 1);
                // Set the starting addres
                Program.StartAddress = tmp;

                // Read the program bytes from the Program structure into the B33 virtual machine memory
                Array.Copy(Program.Program, _memory, MemorySize);

                // Set whether the program has debug information
                Program.HasDebugInfo = Program.DebugInfoAddress > 0;
            }

            // If the program has debugging info, we need to process that now
            if (Program.HasDebugInfo)
            {
                // Get a pointer to the debug info
                ushort pointer = Program.DebugInfoAddress;

                byte a1 = _memory[pointer];
                byte a2 = _memory[pointer + 1];
                // Read in the debug information and store it into a DebugData structure
                while (!(a1 == 0 && a2 == 0))
                {
                    pointer += 2;
                    var addr = (ushort)((a2 << 8) + a1);
                    string line = "";
                    byte c = _memory[pointer];

                    while (c != 0)
                    {
                        line = line + Encoding.ASCII.GetString(new[] { c });
                        pointer++;
                        c = _memory[pointer];
                    }
                    Program.DebugData.Add(new DebugData { Address = addr, SourceCodeLine = line });
                    pointer++;
                    a1 = _memory[pointer];
                    a2 = _memory[pointer + 1];
                }
                Program.DebugData = Program.DebugData.OrderBy(z => z.Address).ToList();
            }
        }

        // Starts the B33 virtual machine
        public void Start()
        {
            // Reset all hardware attached to the B33 virtual machine
            foreach (IB33Hardware h in Hardware)
            {
                h.Reset();
            }

            // Create a new thread to execute the program
            _progThread = new Thread(ExecuteProgram);

            // Save the pause event. The machine is not paused right now
            _isPaused = false;
            _pauseEvent = new ManualResetEvent(true);
            // Set the state to "Running"
            State = States.Running;
            // Start the thread
            _progThread.Start();
            // Raise the B33Started event
            if (B33Started != null)
                B33Started(this, false, 0);
        }

        /// <summary>
        /// Pushes the specified 8-bit value onto the 8-bit stack.
        /// </summary>
        /// <param name="val">The 8-bit value.</param>
        private void Push(byte val)
        {
            _8BitStack.Push(val);
        }

        /// <summary>
        /// Pushes the specified 16-bit value onto the 16-bit stack.
        /// </summary>
        /// <param name="val">The 16-bit value.</param>
        private void Push(ushort val)
        {
            _16BitStack.Push(val);
        }

        /// <summary>
        /// Pops an 8-bit value off the 8-bit stack.
        /// </summary>
        /// <returns>System.Byte.</returns>
        private byte PopByte()
        {
            return _8BitStack.Pop();
        }

        /// <summary>
        /// Pops a 16-bit value off the 16-bit stack.
        /// </summary>
        /// <returns>System.UInt16.</returns>
        private ushort PopUshort()
        {
            return _16BitStack.Pop();
        }

        /// <summary>
        /// Executes the program. This is the heart of the B33 Virtual Machine.
        /// </summary>
        private void ExecuteProgram()
        {
            // We don't want to immediate exit the loop, so set this to false
            bool exitLoop = false;

            // Set the Program Counter to the execution address of the program
            Registers.Pc = Program.ExecAddress;

            // A stop has not been requested yet
            _stopRequested = false;

            // Start the execution loop
            while (!exitLoop)
            {
                // Read an opcode from the address pointed to by the program counter
                byte opcode = _memory[Registers.Pc];

                // If a stop was requested, then reset the program counter back to
                // the execution address, then break out of the loop immediately
                if (_stopRequested)
                {
                    Registers.Pc = Program.ExecAddress;
                    break;
                }

                // If the B33 virtual machine is paused, then we stop this thread
                // right here until we resume.
                _pauseEvent.WaitOne(Timeout.Infinite);

                // If our Speed value is not 0, then sleep for the duration of "Speed"
                if (Speed > 0)
                    Thread.Sleep((int)Speed);

                ushort storeAddress = 0;
                byte r;
                byte ro;
                byte offset;

                bool isStore = opcode == 0x06 || opcode == 0x07 || opcode == 0x08 || opcode == 0x09 ||
                               opcode == 0x0A || opcode == 0x10 || opcode == 0x11;

                if (isStore)
                {
                    switch (opcode)
                    {
                        case 0x06:
                        case 0x07:
                        case 0x08:
                        case 0x09:
                        case 0x0A:
                            storeAddress = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);
                            break;
                        case 0x10:
                        case 0x11:
                            ro = _memory[Registers.Pc + 1];
                            r = _memory[Registers.Pc + 2];
                            offset = 0;
                            if (ro == 1)
                                offset = Registers.A;
                            if (ro == 2)
                                offset = Registers.B;

                            if ((r & 4) == 4)
                                storeAddress = (ushort)(Registers.D + offset);
                            if ((r & 8) == 8)
                                storeAddress = (ushort)(Registers.X + offset);
                            if ((r & 16) == 16)
                                storeAddress = (ushort)(Registers.Y + offset);
                            break;
                    }
                }
                // We will execute our opcode now, so let's raise the B33PreOpcodeExecute event
                if (B33PreOpcodeExecute != null)
                {
                    
                    B33PreOpcodeExecute(this, isStore, storeAddress);
                }

                ushort addr;
                byte valByte;
                ushort valShort;
                byte jumpByte;
                byte sourceRegister;
                byte destinationRegister;

                switch (opcode)
                {
                    case 0:     // END
                        Registers.Pc = Program.ExecAddress;
                        exitLoop = true;
                        break;
                    case 1:     // LDA
                        Registers.A = _memory[Registers.Pc + 1];
                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 2:     // LDB
                        Registers.B = _memory[Registers.Pc + 1];
                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 3:     // LDX
                        Registers.X = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);
                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 4:     // LDY
                        Registers.Y = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);
                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 5:     // LDD
                        Registers.D = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);
                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 6:     // STA
                        addr = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);
                        ThreadPoke(addr, Registers.A);
                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 7:     // STB
                        addr = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);
                        ThreadPoke(addr, Registers.B);
                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 8:     // STX
                        addr = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);
                        ThreadPoke((ushort)(addr + 1), (byte)(Registers.X >> 8));
                        ThreadPoke(addr, (byte)(Registers.X & 0xFF));
                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 9:     // STY
                        addr = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);
                        ThreadPoke((ushort)(addr + 1), (byte)(Registers.Y >> 8));
                        ThreadPoke(addr, (byte)(Registers.Y & 0xff));
                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 0x0A:  // STD
                        addr = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);
                        ThreadPoke((ushort)(addr + 1), (byte)(Registers.D >> 8));
                        ThreadPoke(addr, (byte)(Registers.D & 0xff));
                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 0x0B:  // LDA Extended
                        addr = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);
                        Registers.A = Peek(addr);
                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 0x0C:  // LDB Extended
                        addr = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);
                        Registers.B = Peek(addr);
                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 0x0D:  // LDX Extended
                        ushort d1 = Registers.D;
                        addr = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);
                        Registers.D = (ushort)((Peek(addr) << 8) + Peek((ushort)(addr + 1)));
                        byte tmp3 = Registers.A;
                        Registers.A = Registers.B;
                        Registers.B = tmp3;
                        Registers.X = Registers.D;
                        Registers.D = d1;
                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 0x0E:  // LDY Extended
                        ushort d2 = Registers.D;
                        addr = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);
                        Registers.D = (ushort)((Peek(addr) << 8) + Peek((ushort)(addr + 1)));
                        byte tmp1 = Registers.A;
                        Registers.A = Registers.B;
                        Registers.B = tmp1;
                        Registers.Y = Registers.D;
                        Registers.D = d2;
                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 0x0F:  // LDD Extended
                        addr = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);
                        Registers.D = (ushort)((Peek(addr) << 8) + Peek((ushort)(addr + 1)));
                        byte tmp2 = Registers.A;
                        Registers.A = Registers.B;
                        Registers.B = tmp2;
                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 0x10:  // STA
                        ro = _memory[Registers.Pc + 1];
                        r = _memory[Registers.Pc + 2];

                        offset = 0;
                        if (ro == 1)
                            offset = Registers.A;
                        if (ro == 2)
                            offset = Registers.B;
                        if ((r & 4) == 4)
                        {
                            ThreadPoke((ushort)(Registers.D + offset), Registers.A);
                            if ((r & 32) == 32)
                            {
                                Registers.D = (ushort)(Registers.D + 1);
                                if ((r & 128) == 128)
                                {
                                    Registers.D = (ushort)(Registers.D + 1);
                                }
                            }
                            if ((r & 64) == 64)
                            {
                                Registers.D = (ushort)(Registers.D - 1);
                                if ((r & 128) == 128)
                                {
                                    Registers.D = (ushort)(Registers.D - 1);
                                }
                            }
                        }
                        if ((r & 8) == 8)
                        {
                            ThreadPoke((ushort)(Registers.X + offset), Registers.A);
                            if ((r & 32) == 32)
                            {
                                Registers.X = (ushort)(Registers.X + 1);
                                if ((r & 128) == 128)
                                {
                                    Registers.X = (ushort)(Registers.X + 1);
                                }
                            }
                            if ((r & 64) == 64)
                            {
                                Registers.X = (ushort)(Registers.X - 1);
                                if ((r & 128) == 128)
                                {
                                    Registers.X = (ushort)(Registers.X - 1);
                                }
                            }
                        }
                        if ((r & 16) == 16)
                        {
                            ThreadPoke((ushort)(Registers.Y + offset), Registers.A);
                            if ((r & 32) == 32)
                            {
                                Registers.Y = (ushort)(Registers.Y + 1);
                                if ((r & 128) == 128)
                                {
                                    Registers.Y = (ushort)(Registers.Y + 1);
                                }
                            }
                            if ((r & 64) == 64)
                            {
                                Registers.Y = (ushort)(Registers.Y - 1);
                                if ((r & 128) == 128)
                                {
                                    Registers.Y = (ushort)(Registers.Y - 1);
                                }
                            }
                        }
                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 0x11:  // STB
                        ro = _memory[Registers.Pc + 1];
                        r = _memory[Registers.Pc + 2];

                        offset = 0;
                        if (ro == 1)
                            offset = Registers.A;
                        if (ro == 2)
                            offset = Registers.B;
                        if ((r & 4) == 4)
                        {
                            ThreadPoke((ushort)(Registers.D + offset), Registers.B);
                            if ((r & 32) == 32)
                            {
                                Registers.D = (ushort)(Registers.D + 1);
                                if ((r & 128) == 128)
                                {
                                    Registers.D = (ushort)(Registers.D + 1);
                                }
                            }
                            if ((r & 64) == 64)
                            {
                                Registers.D = (ushort)(Registers.D - 1);
                                if ((r & 128) == 128)
                                {
                                    Registers.D = (ushort)(Registers.D - 1);
                                }
                            }
                        }
                        if ((r & 8) == 8)
                        {
                            ThreadPoke((ushort)(Registers.X + offset), Registers.B);
                            if ((r & 32) == 32)
                            {
                                Registers.X = (ushort)(Registers.X + 1);
                                if ((r & 128) == 128)
                                {
                                    Registers.X = (ushort)(Registers.X + 1);
                                }
                            }
                            if ((r & 64) == 64)
                            {
                                Registers.X = (ushort)(Registers.X - 1);
                                if ((r & 128) == 128)
                                {
                                    Registers.X = (ushort)(Registers.X - 1);
                                }
                            }
                        }
                        if ((r & 16) == 16)
                        {
                            ThreadPoke((ushort)(Registers.Y + offset), Registers.B);
                            if ((r & 32) == 32)
                            {
                                Registers.Y = (ushort)(Registers.Y + 1);
                                if ((r & 128) == 128)
                                {
                                    Registers.Y = (ushort)(Registers.Y + 1);
                                }
                            }
                            if ((r & 64) == 64)
                            {
                                Registers.Y = (ushort)(Registers.Y - 1);
                                if ((r & 128) == 128)
                                {
                                    Registers.Y = (ushort)(Registers.Y - 1);
                                }
                            }
                        }
                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 0x12:  // CMPA
                        valByte = _memory[Registers.Pc + 1];

                        if (Registers.A == valByte)
                            Registers.Cc = (byte)(Registers.Cc | 2);
                        else Registers.Cc = (byte)(Registers.Cc & 0xFD);

                        if (Registers.A > valByte)
                            Registers.Cc = (byte)(Registers.Cc | 4);
                        else Registers.Cc = (byte)(Registers.Cc & 0xFB);

                        if (Registers.A < valByte)
                            Registers.Cc = (byte)(Registers.Cc | 8);
                        else Registers.Cc = (byte)(Registers.Cc & 0xF7);

                        if (Registers.A != valByte)
                            Registers.Cc = (byte)(Registers.Cc | 16);
                        else Registers.Cc = (byte)(Registers.Cc & 0xEF);

                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x13:  // CMPB
                        valByte = _memory[Registers.Pc + 1];

                        if (Registers.B == valByte)
                            Registers.Cc = (byte)(Registers.Cc | 2);
                        else Registers.Cc = (byte)(Registers.Cc & 0xFD);

                        if (Registers.B > valByte)
                            Registers.Cc = (byte)(Registers.Cc | 4);
                        else Registers.Cc = (byte)(Registers.Cc & 0xFB);

                        if (Registers.B < valByte)
                            Registers.Cc = (byte)(Registers.Cc | 8);
                        else Registers.Cc = (byte)(Registers.Cc & 0xF7);

                        if (Registers.B != valByte)
                            Registers.Cc = (byte)(Registers.Cc | 16);
                        else Registers.Cc = (byte)(Registers.Cc & 0xEF);

                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x14:  // CMPD
                        valShort = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);

                        if (Registers.D == valShort)
                            Registers.Cc = (byte)(Registers.Cc | 2);
                        else Registers.Cc = (byte)(Registers.Cc & 0xFD);

                        if (Registers.D > valShort)
                            Registers.Cc = (byte)(Registers.Cc | 4);
                        else Registers.Cc = (byte)(Registers.Cc & 0xFB);

                        if (Registers.D < valShort)
                            Registers.Cc = (byte)(Registers.Cc | 8);
                        else Registers.Cc = (byte)(Registers.Cc & 0xF7);

                        if (Registers.D != valShort)
                            Registers.Cc = (byte)(Registers.Cc | 16);
                        else Registers.Cc = (byte)(Registers.Cc & 0xEF);

                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 0x15:  // CMPX
                        valShort = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);

                        if (Registers.X == valShort)
                            Registers.Cc = (byte)(Registers.Cc | 2);
                        else Registers.Cc = (byte)(Registers.Cc & 0xFD);

                        if (Registers.X > valShort)
                            Registers.Cc = (byte)(Registers.Cc | 4);
                        else Registers.Cc = (byte)(Registers.Cc & 0xFB);

                        if (Registers.X < valShort)
                            Registers.Cc = (byte)(Registers.Cc | 8);
                        else Registers.Cc = (byte)(Registers.Cc & 0xF7);

                        if (Registers.X != valShort)
                            Registers.Cc = (byte)(Registers.Cc | 16);
                        else Registers.Cc = (byte)(Registers.Cc & 0xEF);

                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 0x16:  // CMPY
                        valShort = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);

                        if (Registers.Y == valShort)
                            Registers.Cc = (byte)(Registers.Cc | 2);
                        else Registers.Cc = (byte)(Registers.Cc & 0xFD);

                        if (Registers.Y > valShort)
                            Registers.Cc = (byte)(Registers.Cc | 4);
                        else Registers.Cc = (byte)(Registers.Cc & 0xFB);

                        if (Registers.Y < valShort)
                            Registers.Cc = (byte)(Registers.Cc | 8);
                        else Registers.Cc = (byte)(Registers.Cc & 0xF7);

                        if (Registers.Y != valShort)
                            Registers.Cc = (byte)(Registers.Cc | 16);
                        else Registers.Cc = (byte)(Registers.Cc & 0xEF);

                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 0x17:  // JEQ
                        addr = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);
                        jumpByte = (byte)(Registers.Cc & 2);

                        Registers.Pc = jumpByte == 2 ? addr : (ushort)(Registers.Pc + 3);
                        break;
                    case 0x18: // JNE
                        addr = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);
                        jumpByte = (byte)(Registers.Cc & 16);

                        Registers.Pc = jumpByte == 16 ? addr : (ushort)(Registers.Pc + 3);
                        break;
                    case 0x19:  // LDA indexed
                        ro = _memory[Registers.Pc + 1];
                        r = _memory[Registers.Pc + 2];

                        offset = 0;
                        if (ro == 1)
                            offset = Registers.A;
                        if (ro == 2)
                            offset = Registers.B;

                        if ((r & 8) == 8)
                        {
                            Registers.A = Peek((ushort)(Registers.X + offset));
                            if ((r & 32) == 32)
                            {
                                Registers.X = (ushort)(Registers.X + 1);
                                if ((r & 128) == 128)
                                {
                                    Registers.X = (ushort)(Registers.X + 1);
                                }
                            }
                            if ((r & 64) == 64)
                            {
                                Registers.X = (ushort)(Registers.X - 1);
                                if ((r & 128) == 128)
                                {
                                    Registers.X = (ushort)(Registers.X - 1);
                                }
                            }
                        }
                        if ((r & 4) == 4)
                        {
                            Registers.A = Peek((ushort)(Registers.D + offset));
                            if ((r & 32) == 32)
                            {
                                Registers.D = (ushort)(Registers.D + 1);
                                if ((r & 128) == 128)
                                {
                                    Registers.D = (ushort)(Registers.D + 1);
                                }
                            }
                            if ((r & 64) == 64)
                            {
                                Registers.D = (ushort)(Registers.D - 1);
                                if ((r & 128) == 128)
                                {
                                    Registers.D = (ushort)(Registers.D - 1);
                                }
                            }
                        }
                        if ((r & 16) == 16)
                        {
                            Registers.A = Peek((ushort)(Registers.Y + offset));
                            if ((r & 32) == 32)
                            {
                                Registers.Y = (ushort)(Registers.Y + 1);
                                if ((r & 128) == 128)
                                {
                                    Registers.Y = (ushort)(Registers.Y + 1);
                                }
                            }
                            if ((r & 64) == 64)
                            {
                                Registers.Y = (ushort)(Registers.Y - 1);
                                if ((r & 128) == 128)
                                {
                                    Registers.Y = (ushort)(Registers.Y - 1);
                                }
                            }
                        }
                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 0x1A:  // LDB indexed
                        ro = _memory[Registers.Pc + 1];
                        r = _memory[Registers.Pc + 2];

                        offset = 0;
                        if (ro == 1)
                            offset = Registers.A;
                        if (ro == 2)
                            offset = Registers.B;

                        if ((r & 8) == 8)
                        {
                            Registers.B = Peek((ushort)(Registers.X + offset));
                            if ((r & 32) == 32)
                            {
                                Registers.X = (ushort)(Registers.X + 1);
                                if ((r & 128) == 128)
                                {
                                    Registers.X = (ushort)(Registers.X + 1);
                                }
                            }
                            if ((r & 64) == 64)
                            {
                                Registers.X = (ushort)(Registers.X - 1);
                                if ((r & 128) == 128)
                                {
                                    Registers.X = (ushort)(Registers.X - 1);
                                }
                            }
                        }
                        if ((r & 4) == 4)
                        {
                            Registers.B = Peek((ushort)(Registers.D + offset));
                            if ((r & 32) == 32)
                            {
                                Registers.D = (ushort)(Registers.D + 1);
                                if ((r & 128) == 128)
                                {
                                    Registers.D = (ushort)(Registers.D + 1);
                                }
                            }
                            if ((r & 64) == 64)
                            {
                                Registers.D = (ushort)(Registers.D - 1);
                                if ((r & 128) == 128)
                                {
                                    Registers.D = (ushort)(Registers.D - 1);
                                }
                            }
                        }
                        if ((r & 16) == 16)
                        {
                            Registers.B = Peek((ushort)(Registers.Y + offset));
                            if ((r & 32) == 32)
                            {
                                Registers.Y = (ushort)(Registers.Y + 1);
                                if ((r & 128) == 128)
                                {
                                    Registers.Y = (ushort)(Registers.Y + 1);
                                }
                            }
                            if ((r & 64) == 64)
                            {
                                Registers.Y = (ushort)(Registers.Y - 1);
                                if ((r & 128) == 128)
                                {
                                    Registers.Y = (ushort)(Registers.Y - 1);
                                }
                            }
                        }
                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 0x20:  // JGE
                        addr = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);
                        jumpByte = (byte)(Registers.Cc & 6);

                        Registers.Pc = jumpByte > 0 ? addr : (ushort)(Registers.Pc + 3);
                        break;
                    case 0x21:  // JLE
                        addr = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);
                        jumpByte = (byte)(Registers.Cc & 10);

                        Registers.Pc = jumpByte > 0 ? addr : (ushort)(Registers.Pc + 3);
                        break;
                    case 0x22: // JLT
                        addr = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);
                        jumpByte = (byte)(Registers.Cc & 8);

                        Registers.Pc = jumpByte == 8 ? addr : (ushort)(Registers.Pc + 3);
                        break;
                    case 0x23: // JGT
                        addr = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);
                        jumpByte = (byte)(Registers.Cc & 4);

                        Registers.Pc = jumpByte == 4 ? addr : (ushort)(Registers.Pc + 3);
                        break;
                    case 0x24: // JMP
                        addr = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);

                        Registers.Pc = addr;
                        break;
                    case 0x25:  // PUSH
                        r = _memory[Registers.Pc + 1];

                        if ((r & 1) == 1)
                            Push(Registers.A);
                        if ((r & 2) == 2)
                            Push(Registers.B);
                        if ((r & 4) == 4)
                            Push(Registers.D);
                        if ((r & 8) == 8)
                            Push(Registers.X);
                        if ((r & 16) == 16)
                            Push(Registers.Y);

                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x26:  // POP
                        r = _memory[Registers.Pc + 1];

                        if ((r & 1) == 1)
                            Registers.A = PopByte();
                        if ((r & 2) == 2)
                            Registers.B = PopByte();
                        if ((r & 4) == 4)
                            Registers.D = PopUshort();
                        if ((r & 8) == 8)
                            Registers.X = PopUshort();
                        if ((r & 16) == 16)
                            Registers.Y = PopUshort();
                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x27:  // CALL
                        addr = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);
                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        _callStack.Push(Registers.Pc);
                        Registers.Pc = addr;
                        break;
                    case 0x28:  // RET
                        Registers.Pc = _callStack.Pop();
                        break;
                    case 0x2A:  // SUBA
                        valByte = _memory[Registers.Pc + 1];

                        Registers.A = (byte)(Registers.A - valByte);

                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x2B:  // SUBB
                        valByte = _memory[Registers.Pc + 1];

                        Registers.B = (byte)(Registers.B - valByte);

                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x2C:  // SUBD
                        valShort = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);

                        Registers.D = (ushort)(Registers.D - valShort);

                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 0x2D:  // SUBX
                        valShort = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);

                        Registers.X = (ushort)(Registers.X - valShort);

                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 0x2E:  // SUBY
                        valShort = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);

                        Registers.Y = (ushort)(Registers.Y - valShort);

                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 0x2F:  // ADDA
                        valByte = _memory[Registers.Pc + 1];

                        Registers.A = (byte)(Registers.A + valByte);

                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x30:  // ADDB
                        valByte = _memory[Registers.Pc + 1];

                        Registers.B = (byte)(Registers.B + valByte);

                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x31:  // ADDD
                        valShort = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);

                        Registers.D = (ushort)(Registers.D + valShort);

                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 0x32:  // ADDX
                        valShort = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);

                        Registers.X = (ushort)(Registers.X + valShort);

                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 0x33:  // ADDY
                        valShort = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);

                        Registers.Y = (ushort)(Registers.Y + valShort);

                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 0x34:  // SUBA register
                        r = _memory[Registers.Pc + 1];

                        if (r == 1)
                            Registers.A = (byte)(Registers.A - Registers.A);
                        if (r == 2)
                            Registers.A = (byte)(Registers.A - Registers.B);
                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x35:  // SUBB register
                        r = _memory[Registers.Pc + 1];

                        if (r == 1)
                            Registers.B = (byte)(Registers.B - Registers.A);
                        if (r == 2)
                            Registers.B = (byte)(Registers.B - Registers.B);
                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x36:  // SUBD register
                        r = _memory[Registers.Pc + 1];

                        if (r == 1)
                            Registers.D = (ushort)(Registers.D - Registers.A);
                        if (r == 2)
                            Registers.D = (ushort)(Registers.D - Registers.B);
                        if (r == 4)
                            Registers.D = (ushort)(Registers.D - Registers.D);
                        if (r == 8)
                            Registers.D = (ushort)(Registers.D - Registers.X);
                        if (r == 16)
                            Registers.D = (ushort)(Registers.D - Registers.Y);
                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x37:  // SUBX register
                        r = _memory[Registers.Pc + 1];

                        if (r == 1)
                            Registers.X = (ushort)(Registers.X - Registers.A);
                        if (r == 2)
                            Registers.X = (ushort)(Registers.X - Registers.B);
                        if (r == 4)
                            Registers.X = (ushort)(Registers.X - Registers.D);
                        if (r == 8)
                            Registers.X = (ushort)(Registers.X - Registers.X);
                        if (r == 16)
                            Registers.X = (ushort)(Registers.X - Registers.Y);
                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x38:  // SUBY register
                        r = _memory[Registers.Pc + 1];

                        if (r == 1)
                            Registers.Y = (ushort)(Registers.Y - Registers.A);
                        if (r == 2)
                            Registers.Y = (ushort)(Registers.Y - Registers.B);
                        if (r == 4)
                            Registers.Y = (ushort)(Registers.Y - Registers.D);
                        if (r == 8)
                            Registers.Y = (ushort)(Registers.Y - Registers.X);
                        if (r == 16)
                            Registers.Y = (ushort)(Registers.Y - Registers.Y);
                        Registers.Pc = (ushort)(Registers.Pc + 2);

                        break;
                    case 0x39:  // ADDA register
                        r = _memory[Registers.Pc + 1];

                        if (r == 1)
                            Registers.A = (byte)(Registers.A + Registers.A);
                        if (r == 2)
                            Registers.A = (byte)(Registers.A + Registers.B);
                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x3A:  // ADDB register
                        r = _memory[Registers.Pc + 1];

                        if (r == 1)
                            Registers.B = (byte)(Registers.B + Registers.A);
                        if (r == 2)
                            Registers.B = (byte)(Registers.B + Registers.B);
                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x3B:  // ADDD register
                        r = _memory[Registers.Pc + 1];

                        if (r == 1)
                            Registers.D = (ushort)(Registers.D + Registers.A);
                        if (r == 2)
                            Registers.D = (ushort)(Registers.D + Registers.B);
                        if (r == 4)
                            Registers.D = (ushort)(Registers.D + Registers.D);
                        if (r == 8)
                            Registers.D = (ushort)(Registers.D + Registers.X);
                        if (r == 16)
                            Registers.D = (ushort)(Registers.D + Registers.Y);
                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x3C:  // ADDX register
                        r = _memory[Registers.Pc + 1];

                        if (r == 1)
                            Registers.X = (ushort)(Registers.X + Registers.A);
                        if (r == 2)
                            Registers.X = (ushort)(Registers.X + Registers.B);
                        if (r == 4)
                            Registers.X = (ushort)(Registers.X + Registers.D);
                        if (r == 8)
                            Registers.X = (ushort)(Registers.X + Registers.X);
                        if (r == 16)
                            Registers.X = (ushort)(Registers.X + Registers.Y);
                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x3D:  // ADDY register
                        r = _memory[Registers.Pc + 1];

                        if (r == 1)
                            Registers.Y = (ushort)(Registers.Y + Registers.A);
                        if (r == 2)
                            Registers.Y = (ushort)(Registers.Y + Registers.B);
                        if (r == 4)
                            Registers.Y = (ushort)(Registers.Y + Registers.D);
                        if (r == 8)
                            Registers.Y = (ushort)(Registers.Y + Registers.X);
                        if (r == 16)
                            Registers.Y = (ushort)(Registers.Y + Registers.Y);
                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x3E:  // TFR
                        sourceRegister = _memory[Registers.Pc + 1];
                        destinationRegister = _memory[Registers.Pc + 2];

                        if (sourceRegister == 1)
                        {
                            switch (destinationRegister)
                            {
                                case 1:
                                    Registers.A = Registers.A;
                                    break;
                                case 2:
                                    Registers.B = Registers.A;
                                    break;
                                case 4:
                                    Registers.D = Registers.A;
                                    break;
                                case 8:
                                    Registers.X = Registers.A;
                                    break;
                                case 16:
                                    Registers.Y = Registers.A;
                                    break;
                            }
                        }
                        if (sourceRegister == 2)
                        {
                            switch (destinationRegister)
                            {
                                case 1:
                                    Registers.A = Registers.B;
                                    break;
                                case 2:
                                    Registers.B = Registers.B;
                                    break;
                                case 4:
                                    Registers.D = Registers.B;
                                    break;
                                case 8:
                                    Registers.X = Registers.B;
                                    break;
                                case 16:
                                    Registers.Y = Registers.B;
                                    break;
                            }
                        }
                        if (sourceRegister == 4)
                        {
                            switch (destinationRegister)
                            {
                                case 4:
                                    Registers.D = Registers.D;
                                    break;
                                case 8:
                                    Registers.X = Registers.D;
                                    break;
                                case 16:
                                    Registers.Y = Registers.D;
                                    break;
                            }
                        }
                        if (sourceRegister == 8)
                        {
                            switch (destinationRegister)
                            {
                                case 4:
                                    Registers.D = Registers.X;
                                    break;
                                case 8:
                                    Registers.X = Registers.X;
                                    break;
                                case 16:
                                    Registers.Y = Registers.X;
                                    break;
                            }
                        }
                        if (sourceRegister == 16)
                        {
                            switch (destinationRegister)
                            {
                                case 4:
                                    Registers.D = Registers.Y;
                                    break;
                                case 8:
                                    Registers.X = Registers.Y;
                                    break;
                                case 16:
                                    Registers.Y = Registers.Y;
                                    break;
                            }
                        }
                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 0x3F: // CMPA Register
                        r = _memory[Registers.Pc + 1];
                        valShort = 0;

                        if (r == 1)
                            valShort = Registers.A;
                        if (r == 2)
                            valShort = Registers.B;
                        if (r == 4)
                            valShort = Registers.D;
                        if (r == 8)
                            valShort = Registers.X;
                        if (r == 16)
                            valShort = Registers.Y;

                        if (Registers.A == valShort)
                            Registers.Cc = (byte)(Registers.Cc | 2);
                        else Registers.Cc = (byte)(Registers.Cc & 0xFD);

                        if (Registers.A > valShort)
                            Registers.Cc = (byte)(Registers.Cc | 4);
                        else Registers.Cc = (byte)(Registers.Cc & 0xFB);

                        if (Registers.A < valShort)
                            Registers.Cc = (byte)(Registers.Cc | 8);
                        else Registers.Cc = (byte)(Registers.Cc & 0xF7);

                        if (Registers.A != valShort)
                            Registers.Cc = (byte)(Registers.Cc | 16);
                        else Registers.Cc = (byte)(Registers.Cc & 0xEF);

                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x40:  // CMPB Register
                        r = _memory[Registers.Pc + 1];
                        valShort = 0;

                        if (r == 1)
                            valShort = Registers.A;
                        if (r == 2)
                            valShort = Registers.B;
                        if (r == 4)
                            valShort = Registers.D;
                        if (r == 8)
                            valShort = Registers.X;
                        if (r == 16)
                            valShort = Registers.Y;

                        if (Registers.B == valShort)
                            Registers.Cc = (byte)(Registers.Cc | 2);
                        else Registers.Cc = (byte)(Registers.Cc & 0xFD);

                        if (Registers.B > valShort)
                            Registers.Cc = (byte)(Registers.Cc | 4);
                        else Registers.Cc = (byte)(Registers.Cc & 0xFB);

                        if (Registers.B < valShort)
                            Registers.Cc = (byte)(Registers.Cc | 8);
                        else Registers.Cc = (byte)(Registers.Cc & 0xF7);

                        if (Registers.B != valShort)
                            Registers.Cc = (byte)(Registers.Cc | 16);
                        else Registers.Cc = (byte)(Registers.Cc & 0xEF);

                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x41:  // CMPD Register
                        r = _memory[Registers.Pc + 1];
                        valShort = 0;

                        if (r == 1)
                            valShort = Registers.A;
                        if (r == 2)
                            valShort = Registers.B;
                        if (r == 4)
                            valShort = Registers.D;
                        if (r == 8)
                            valShort = Registers.X;
                        if (r == 16)
                            valShort = Registers.Y;

                        if (Registers.D == valShort)
                            Registers.Cc = (byte)(Registers.Cc | 2);
                        else Registers.Cc = (byte)(Registers.Cc & 0xFD);

                        if (Registers.D > valShort)
                            Registers.Cc = (byte)(Registers.Cc | 4);
                        else Registers.Cc = (byte)(Registers.Cc & 0xFB);

                        if (Registers.D < valShort)
                            Registers.Cc = (byte)(Registers.Cc | 8);
                        else Registers.Cc = (byte)(Registers.Cc & 0xF7);

                        if (Registers.D != valShort)
                            Registers.Cc = (byte)(Registers.Cc | 16);
                        else Registers.Cc = (byte)(Registers.Cc & 0xEF);

                        Registers.Pc = (ushort)(Registers.Pc + 2);

                        break;
                    case 0x42:  // CMPX Register
                        r = _memory[Registers.Pc + 1];
                        valShort = 0;

                        if (r == 1)
                            valShort = Registers.A;
                        if (r == 2)
                            valShort = Registers.B;
                        if (r == 4)
                            valShort = Registers.D;
                        if (r == 8)
                            valShort = Registers.X;
                        if (r == 16)
                            valShort = Registers.Y;

                        if (Registers.X == valShort)
                            Registers.Cc = (byte)(Registers.Cc | 2);
                        else Registers.Cc = (byte)(Registers.Cc & 0xFD);

                        if (Registers.X > valShort)
                            Registers.Cc = (byte)(Registers.Cc | 4);
                        else Registers.Cc = (byte)(Registers.Cc & 0xFB);

                        if (Registers.X < valShort)
                            Registers.Cc = (byte)(Registers.Cc | 8);
                        else Registers.Cc = (byte)(Registers.Cc & 0xF7);

                        if (Registers.X != valShort)
                            Registers.Cc = (byte)(Registers.Cc | 16);
                        else Registers.Cc = (byte)(Registers.Cc & 0xEF);

                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x43:  // CMPY Register
                        r = _memory[Registers.Pc + 1];
                        valShort = 0;

                        if (r == 1)
                            valShort = Registers.A;
                        if (r == 2)
                            valShort = Registers.B;
                        if (r == 4)
                            valShort = Registers.D;
                        if (r == 8)
                            valShort = Registers.X;
                        if (r == 16)
                            valShort = Registers.Y;

                        if (Registers.Y == valShort)
                            Registers.Cc = (byte)(Registers.Cc | 2);
                        else Registers.Cc = (byte)(Registers.Cc & 0xFD);

                        if (Registers.Y > valShort)
                            Registers.Cc = (byte)(Registers.Cc | 4);
                        else Registers.Cc = (byte)(Registers.Cc & 0xFB);

                        if (Registers.Y < valShort)
                            Registers.Cc = (byte)(Registers.Cc | 8);
                        else Registers.Cc = (byte)(Registers.Cc & 0xF7);

                        if (Registers.Y != valShort)
                            Registers.Cc = (byte)(Registers.Cc | 16);
                        else Registers.Cc = (byte)(Registers.Cc & 0xEF);

                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x44:  // LSFT
                        r = _memory[Registers.Pc + 1];

                        if (r == 1)
                        {
                            Registers.Cc = (Registers.A & 0x80) == 0x80
                                                ? (byte)(Registers.Cc | 1)
                                                : (byte)(Registers.Cc & 0xFE);
                            Registers.A = (byte)(Registers.A << 1);
                        }
                        if (r == 2)
                        {
                            Registers.Cc = (Registers.B & 0x80) == 0x80
                                               ? (byte)(Registers.Cc | 1)
                                               : (byte)(Registers.Cc & 0xFE);
                            Registers.B = (byte)(Registers.B << 1);
                        }
                        if (r == 4)
                        {
                            Registers.Cc = (Registers.D & 0x8000) == 0x8000
                                               ? (byte)(Registers.Cc | 1)
                                               : (byte)(Registers.Cc & 0xFE);
                            Registers.D = (ushort)(Registers.D << 1);
                        }
                        if (r == 8)
                        {
                            Registers.Cc = (Registers.X & 0x8000) == 0x8000
                                               ? (byte)(Registers.Cc | 1)
                                               : (byte)(Registers.Cc & 0xFE);
                            Registers.X = (ushort)(Registers.X << 1);
                        }
                        if (r == 16)
                        {
                            Registers.Cc = (Registers.Y & 0x8000) == 0x8000
                                               ? (byte)(Registers.Cc | 1)
                                               : (byte)(Registers.Cc & 0xFE);
                            Registers.Y = (ushort)(Registers.Y << 1);
                        }
                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x45:  // RSFT
                        r = _memory[Registers.Pc + 1];

                        if (r == 1)
                        {
                            Registers.Cc = (Registers.A & 0x01) == 0x01
                                                ? (byte)(Registers.Cc | 1)
                                                : (byte)(Registers.Cc & 0xFE);
                            Registers.A = (byte)(Registers.A >> 1);
                        }
                        if (r == 2)
                        {
                            Registers.Cc = (Registers.B & 0x01) == 0x01
                                               ? (byte)(Registers.Cc | 1)
                                               : (byte)(Registers.Cc & 0xFE);
                            Registers.B = (byte)(Registers.B >> 1);
                        }
                        if (r == 4)
                        {
                            Registers.Cc = (Registers.D & 0x01) == 0x01
                                               ? (byte)(Registers.Cc | 1)
                                               : (byte)(Registers.Cc & 0xFE);
                            Registers.D = (ushort)(Registers.D >> 1);
                        }
                        if (r == 8)
                        {
                            Registers.Cc = (Registers.X & 0x01) == 0x01
                                               ? (byte)(Registers.Cc | 1)
                                               : (byte)(Registers.Cc & 0xFE);
                            Registers.X = (ushort)(Registers.X >> 1);
                        }
                        if (r == 16)
                        {
                            Registers.Cc = (Registers.Y & 0x01) == 0x01
                                               ? (byte)(Registers.Cc | 1)
                                               : (byte)(Registers.Cc & 0xFE);
                            Registers.Y = (ushort)(Registers.Y >> 1);
                        }
                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x46:  // JOS
                        addr = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);
                        jumpByte = (byte)(Registers.Cc & 1);

                        Registers.Pc = jumpByte == 1 ? addr : (ushort)(Registers.Pc + 3);
                        break;
                    case 0x47:  // JOC
                        addr = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);
                        jumpByte = (byte)(Registers.Cc & 1);

                        Registers.Pc = jumpByte == 0 ? addr : (ushort)(Registers.Pc + 3);
                        break;
                    case 0x48:  // MUL8
                        sourceRegister = _memory[Registers.Pc + 1];
                        destinationRegister = _memory[Registers.Pc + 2];
                        byte m = 0;

                        switch (destinationRegister)
                        {
                            case 1:
                                m = Registers.A;
                                break;
                            case 2:
                                m = Registers.B;
                                break;
                        }

                        if (sourceRegister == 1)
                        {
                            Registers.A = (byte)(Registers.A * m);
                        }
                        if (sourceRegister == 2)
                        {
                            Registers.B = (byte)(Registers.B * m);
                        }
                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 0x49:  // MUL16
                        sourceRegister = _memory[Registers.Pc + 1];
                        destinationRegister = _memory[Registers.Pc + 2];
                        ushort m2 = 0;

                        switch (destinationRegister)
                        {
                            case 1:
                                m2 = Registers.A;
                                break;
                            case 2:
                                m2 = Registers.B;
                                break;
                            case 4:
                                m2 = Registers.D;
                                break;
                            case 8:
                                m2 = Registers.X;
                                break;
                            case 16:
                                m2 = Registers.Y;
                                break;
                        }

                        if (sourceRegister == 4)
                        {
                            Registers.D = (ushort)(Registers.D * m2);
                        }
                        if (sourceRegister == 8)
                        {
                            Registers.X = (ushort)(Registers.X * m2);
                        }
                        if (sourceRegister == 16)
                        {
                            Registers.Y = (ushort)(Registers.Y * m2);
                        }
                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 0x4A:  // ANDA Immediate
                        valByte = _memory[Registers.Pc + 1];

                        Registers.A = (byte)(Registers.A & valByte);
                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x4B:  // ANDB Immediate
                        valByte = _memory[Registers.Pc + 1];

                        Registers.B = (byte)(Registers.B & valByte);
                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x4C:  // ANDD Immediate
                        valShort = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);

                        Registers.D = (ushort)(Registers.D & valShort);
                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 0x4D:  // ANDX Immediate
                        valShort = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);

                        Registers.X = (ushort)(Registers.X & valShort);
                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 0x4E:  // ANDY Immediate
                        valShort = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);

                        Registers.Y = (ushort)(Registers.Y & valShort);
                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 0x4F:  // ANDA Register
                        r = _memory[Registers.Pc + 1];

                        if (r == 1)
                        {
                            Registers.A = (byte)(Registers.A & Registers.A);
                        }
                        if (r == 2)
                        {
                            Registers.A = (byte)(Registers.A & Registers.B);
                        }

                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x50:  // ANDB Register
                        r = _memory[Registers.Pc + 1];

                        if (r == 1)
                        {
                            Registers.B = (byte)(Registers.B & Registers.A);
                        }
                        if (r == 2)
                        {
                            Registers.B = (byte)(Registers.B & Registers.B);
                        }

                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x51:  // ANDD Register
                        r = _memory[Registers.Pc + 1];

                        if (r == 1)
                        {
                            Registers.D = (ushort)(Registers.D & Registers.A);
                        }
                        if (r == 2)
                        {
                            Registers.D = (ushort)(Registers.D & Registers.B);
                        }
                        if (r == 4)
                        {
                            Registers.D = (ushort)(Registers.D & Registers.D);
                        }
                        if (r == 8)
                        {
                            Registers.D = (ushort)(Registers.D & Registers.X);
                        }
                        if (r == 16)
                        {
                            Registers.D = (ushort)(Registers.D & Registers.Y);
                        }

                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x52:  // ANDX Register
                        r = _memory[Registers.Pc + 1];

                        if (r == 1)
                        {
                            Registers.X = (ushort)(Registers.X & Registers.A);
                        }
                        if (r == 2)
                        {
                            Registers.X = (ushort)(Registers.X & Registers.B);
                        }
                        if (r == 4)
                        {
                            Registers.X = (ushort)(Registers.X & Registers.D);
                        }
                        if (r == 8)
                        {
                            Registers.X = (ushort)(Registers.X & Registers.X);
                        }
                        if (r == 16)
                        {
                            Registers.X = (ushort)(Registers.X & Registers.Y);
                        }

                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x53:  // ANDY Register
                        r = _memory[Registers.Pc + 1];

                        if (r == 1)
                        {
                            Registers.Y = (ushort)(Registers.Y & Registers.A);
                        }
                        if (r == 2)
                        {
                            Registers.Y = (ushort)(Registers.Y & Registers.B);
                        }
                        if (r == 4)
                        {
                            Registers.Y = (ushort)(Registers.Y & Registers.D);
                        }
                        if (r == 8)
                        {
                            Registers.Y = (ushort)(Registers.Y & Registers.X);
                        }
                        if (r == 16)
                        {
                            Registers.Y = (ushort)(Registers.Y & Registers.Y);
                        }

                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x54:  // ORA Immediate
                        valByte = _memory[Registers.Pc + 1];

                        Registers.A = (byte)(Registers.A | valByte);
                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x55:  // ORB Immediate
                        valByte = _memory[Registers.Pc + 1];

                        Registers.B = (byte)(Registers.B | valByte);
                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x56:  // ORD Immediate
                        valShort = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);

                        Registers.D = (ushort)(Registers.D | valShort);
                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 0x57:  // ORX Immediate
                        valShort = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);

                        Registers.X = (ushort)(Registers.X | valShort);
                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 0x58:  // ORY Immediate
                        valShort = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);

                        Registers.Y = (ushort)(Registers.Y | valShort);
                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 0x59:  // ORA Register
                        r = _memory[Registers.Pc + 1];

                        if (r == 1)
                        {
                            Registers.A = (byte)(Registers.A | Registers.A);
                        }
                        if (r == 2)
                        {
                            Registers.A = (byte)(Registers.A | Registers.B);
                        }

                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x5A:  // ORB Register
                        r = _memory[Registers.Pc + 1];

                        if (r == 1)
                        {
                            Registers.B = (byte)(Registers.B | Registers.A);
                        }
                        if (r == 2)
                        {
                            Registers.B = (byte)(Registers.B | Registers.B);
                        }

                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x5B:  // ORD Register
                        r = _memory[Registers.Pc + 1];

                        if (r == 1)
                        {
                            Registers.D = (ushort)(Registers.D | Registers.A);
                        }
                        if (r == 2)
                        {
                            Registers.D = (ushort)(Registers.D | Registers.B);
                        }
                        if (r == 4)
                        {
                            Registers.D = (ushort)(Registers.D | Registers.D);
                        }
                        if (r == 8)
                        {
                            Registers.D = (ushort)(Registers.D | Registers.X);
                        }
                        if (r == 16)
                        {
                            Registers.D = (ushort)(Registers.D | Registers.Y);
                        }

                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x5C:  // ORX Register
                        r = _memory[Registers.Pc + 1];

                        if (r == 1)
                        {
                            Registers.X = (ushort)(Registers.X | Registers.A);
                        }
                        if (r == 2)
                        {
                            Registers.X = (ushort)(Registers.X | Registers.B);
                        }
                        if (r == 4)
                        {
                            Registers.X = (ushort)(Registers.X | Registers.D);
                        }
                        if (r == 8)
                        {
                            Registers.X = (ushort)(Registers.X | Registers.X);
                        }
                        if (r == 16)
                        {
                            Registers.X = (ushort)(Registers.X | Registers.Y);
                        }

                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x5D:  // ORY Register
                        r = _memory[Registers.Pc + 1];

                        if (r == 1)
                        {
                            Registers.Y = (ushort)(Registers.Y | Registers.A);
                        }
                        if (r == 2)
                        {
                            Registers.Y = (ushort)(Registers.Y | Registers.B);
                        }
                        if (r == 4)
                        {
                            Registers.Y = (ushort)(Registers.Y | Registers.D);
                        }
                        if (r == 8)
                        {
                            Registers.Y = (ushort)(Registers.Y | Registers.X);
                        }
                        if (r == 16)
                        {
                            Registers.Y = (ushort)(Registers.Y | Registers.Y);
                        }

                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x5E:  // DIV8
                        sourceRegister = _memory[Registers.Pc + 1];
                        destinationRegister = _memory[Registers.Pc + 2];
                        byte q1;
                        byte r1;

                        if (sourceRegister == 1)
                        {
                            if (destinationRegister == 1)
                            {
                                q1 = (byte)(Registers.A / Registers.A);
                                r1 = (byte)(Registers.A % Registers.A);

                                Registers.A = q1;
                                Registers.B = r1;
                            }
                            if (destinationRegister == 2)
                            {
                                q1 = (byte)(Registers.A / Registers.B);
                                r1 = (byte)(Registers.A % Registers.B);

                                Registers.A = q1;
                                Registers.B = r1;
                            }
                        }

                        if (sourceRegister == 2)
                        {
                            if (destinationRegister == 1)
                            {
                                q1 = (byte)(Registers.B / Registers.A);
                                r1 = (byte)(Registers.B % Registers.A);

                                Registers.A = q1;
                                Registers.B = r1;
                            }
                            if (destinationRegister == 2)
                            {
                                q1 = (byte)(Registers.B / Registers.B);
                                r1 = (byte)(Registers.B % Registers.B);

                                Registers.A = q1;
                                Registers.B = r1;
                            }
                        }

                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 0x5F:  // DIV16
                        sourceRegister = _memory[Registers.Pc + 1];
                        destinationRegister = _memory[Registers.Pc + 2];
                        ushort q2;
                        ushort r2;

                        if (sourceRegister == 4)
                        {
                            if (destinationRegister == 1)
                            {
                                q2 = (ushort)(Registers.D / Registers.A);
                                r2 = (ushort)(Registers.D % Registers.A);
                                Registers.X = q2;
                                Registers.Y = r2;
                            }
                            if (destinationRegister == 2)
                            {
                                q2 = (ushort)(Registers.D / Registers.B);
                                r2 = (ushort)(Registers.D % Registers.B);
                                Registers.X = q2;
                                Registers.Y = r2;
                            }
                            if (destinationRegister == 4)
                            {
                                q2 = (ushort)(Registers.D / Registers.D);
                                r2 = (ushort)(Registers.D % Registers.D);
                                Registers.X = q2;
                                Registers.Y = r2;
                            }
                            if (destinationRegister == 8)
                            {
                                q2 = (ushort)(Registers.D / Registers.X);
                                r2 = (ushort)(Registers.D % Registers.X);
                                Registers.X = q2;
                                Registers.Y = r2;
                            }
                            if (destinationRegister == 16)
                            {
                                q2 = (ushort)(Registers.D / Registers.Y);
                                r2 = (ushort)(Registers.D % Registers.Y);
                                Registers.X = q2;
                                Registers.Y = r2;
                            }
                        }
                        if (sourceRegister == 8)
                        {
                            if (destinationRegister == 1)
                            {
                                q2 = (ushort)(Registers.X / Registers.A);
                                r2 = (ushort)(Registers.X % Registers.A);
                                Registers.X = q2;
                                Registers.Y = r2;
                            }
                            if (destinationRegister == 2)
                            {
                                q2 = (ushort)(Registers.X / Registers.B);
                                r2 = (ushort)(Registers.X % Registers.B);
                                Registers.X = q2;
                                Registers.Y = r2;
                            }
                            if (destinationRegister == 4)
                            {
                                q2 = (ushort)(Registers.X / Registers.D);
                                r2 = (ushort)(Registers.X % Registers.D);
                                Registers.X = q2;
                                Registers.Y = r2;
                            }
                            if (destinationRegister == 8)
                            {
                                q2 = (ushort)(Registers.X / Registers.X);
                                r2 = (ushort)(Registers.X % Registers.X);
                                Registers.X = q2;
                                Registers.Y = r2;
                            }
                            if (destinationRegister == 16)
                            {
                                q2 = (ushort)(Registers.X / Registers.Y);
                                r2 = (ushort)(Registers.X % Registers.Y);
                                Registers.X = q2;
                                Registers.Y = r2;
                            }
                        }
                        if (sourceRegister == 16)
                        {
                            if (destinationRegister == 1)
                            {
                                q2 = (ushort)(Registers.Y / Registers.A);
                                r2 = (ushort)(Registers.Y % Registers.A);
                                Registers.X = q2;
                                Registers.Y = r2;
                            }
                            if (destinationRegister == 2)
                            {
                                q2 = (ushort)(Registers.Y / Registers.B);
                                r2 = (ushort)(Registers.Y % Registers.B);
                                Registers.X = q2;
                                Registers.Y = r2;
                            }
                            if (destinationRegister == 4)
                            {
                                q2 = (ushort)(Registers.Y / Registers.D);
                                r2 = (ushort)(Registers.Y % Registers.D);
                                Registers.X = q2;
                                Registers.Y = r2;
                            }
                            if (destinationRegister == 8)
                            {
                                q2 = (ushort)((short)(Registers.Y) / Registers.X);
                                r2 = (ushort)(Registers.Y % Registers.X);
                                Registers.X = q2;
                                Registers.Y = r2;
                            }
                            if (destinationRegister == 16)
                            {
                                q2 = (ushort)(Registers.Y / Registers.Y);
                                r2 = (ushort)(Registers.Y % Registers.Y);
                                Registers.X = q2;
                                Registers.Y = r2;
                            }
                        }

                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 0x60:  // XORA Immediate
                        valByte = _memory[Registers.Pc + 1];

                        Registers.A = (byte)(Registers.A ^ valByte);
                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x61:  // XORB Immediate
                        valByte = _memory[Registers.Pc + 1];

                        Registers.B = (byte)(Registers.B ^ valByte);
                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x62:  // XORD Immediate
                        valShort = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);

                        Registers.D = (ushort)(Registers.D ^ valShort);
                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 0x63:  // XORX Immediate
                        valShort = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);

                        Registers.X = (ushort)(Registers.X ^ valShort);
                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 0x64:  // XORY Immediate
                        valShort = BitConverter.ToUInt16(new[] { _memory[Registers.Pc + 1], _memory[Registers.Pc + 2] }, 0);

                        Registers.Y = (ushort)(Registers.Y ^ valShort);
                        Registers.Pc = (ushort)(Registers.Pc + 3);
                        break;
                    case 0x65:  // XORA Register
                        r = _memory[Registers.Pc + 1];

                        if (r == 1)
                        {
                            Registers.A = (byte)(Registers.A ^ Registers.A);
                        }
                        if (r == 2)
                        {
                            Registers.A = (byte)(Registers.A ^ Registers.B);
                        }

                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x66:  // XORB Register
                        r = _memory[Registers.Pc + 1];

                        if (r == 1)
                        {
                            Registers.B = (byte)(Registers.B ^ Registers.A);
                        }
                        if (r == 2)
                        {
                            Registers.B = (byte)(Registers.B ^ Registers.B);
                        }

                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x67:  // XORD Register
                        r = _memory[Registers.Pc + 1];

                        if (r == 1)
                        {
                            Registers.D = (ushort)(Registers.D ^ Registers.A);
                        }
                        if (r == 2)
                        {
                            Registers.D = (ushort)(Registers.D ^ Registers.B);
                        }
                        if (r == 4)
                        {
                            Registers.D = (ushort)(Registers.D ^ Registers.D);
                        }
                        if (r == 8)
                        {
                            Registers.D = (ushort)(Registers.D ^ Registers.X);
                        }
                        if (r == 16)
                        {
                            Registers.D = (ushort)(Registers.D ^ Registers.Y);
                        }

                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x68:  // XORX Register
                        r = _memory[Registers.Pc + 1];

                        if (r == 1)
                        {
                            Registers.X = (ushort)(Registers.X ^ Registers.A);
                        }
                        if (r == 2)
                        {
                            Registers.X = (ushort)(Registers.X ^ Registers.B);
                        }
                        if (r == 4)
                        {
                            Registers.X = (ushort)(Registers.X ^ Registers.D);
                        }
                        if (r == 8)
                        {
                            Registers.X = (ushort)(Registers.X ^ Registers.X);
                        }
                        if (r == 16)
                        {
                            Registers.X = (ushort)(Registers.X ^ Registers.Y);
                        }

                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x69:  // XORY Register
                        r = _memory[Registers.Pc + 1];

                        if (r == 1)
                        {
                            Registers.Y = (ushort)(Registers.Y ^ Registers.A);
                        }
                        if (r == 2)
                        {
                            Registers.Y = (ushort)(Registers.Y ^ Registers.B);
                        }
                        if (r == 4)
                        {
                            Registers.Y = (ushort)(Registers.Y ^ Registers.D);
                        }
                        if (r == 8)
                        {
                            Registers.Y = (ushort)(Registers.Y ^ Registers.X);
                        }
                        if (r == 16)
                        {
                            Registers.Y = (ushort)(Registers.Y ^ Registers.Y);
                        }

                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x6A:  // RND Register
                        r = _memory[Registers.Pc + 1];

                        if (r == 1)
                        {
                            Registers.A = (byte)_rnd.Next(0, Registers.A);
                        }
                        if (r == 2)
                        {
                            Registers.B = (byte)_rnd.Next(0, Registers.B);
                        }
                        if (r == 4)
                        {
                            Registers.D = (ushort)_rnd.Next(0, Registers.D);
                        }
                        if (r == 8)
                        {
                            Registers.X = (ushort)_rnd.Next(0, Registers.X);
                        }
                        if (r == 16)
                        {
                            Registers.Y = (ushort)_rnd.Next(0, Registers.Y);
                        }

                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x6E:  // NOP
                        Registers.Pc++;
                        break;
                    case 0x6F:  // NEG
                        r = _memory[Registers.Pc + 1];

                        if (r == 1)
                        {
                            Registers.A = (byte)(~Registers.A);
                            Registers.A = (byte)(Registers.A + 1);
                        }
                        if (r == 2)
                        {
                            Registers.B = (byte)(~Registers.B);
                            Registers.B = (byte)(Registers.B + 1);
                        }
                        if (r == 4)
                        {
                            Registers.D = (ushort)(~Registers.D);
                            Registers.D = (ushort)(Registers.D + 1);
                        }
                        if (r == 8)
                        {
                            Registers.X = (ushort)(~Registers.X);
                            Registers.X = (ushort)(Registers.X + 1);
                        }
                        if (r == 16)
                        {
                            Registers.Y = (ushort)(~Registers.Y);
                            Registers.Y = (ushort)(Registers.Y + 1);
                        }

                        Registers.Pc = (ushort)(Registers.Pc + 2);
                        break;
                    case 0x70:  // CLS
                        foreach (B33Screen h in Hardware.OfType<B33Screen>())
                        {
                            h.Clear();
                        }
                        foreach (B33ScreenWpf h in Hardware.OfType<B33ScreenWpf>())
                        {
                            h.Clear();
                        }
                        Registers.Pc++;
                        break;
                    case 0x71: // SCRUP
                        foreach (B33Screen h in Hardware.OfType<B33Screen>())
                        {
                            h.ScrollUp();
                        }
                        foreach (B33ScreenWpf h in Hardware.OfType<B33ScreenWpf>())
                        {
                            h.ScrollUp();
                        }
                        Registers.Pc++;
                        break;
                    case 0xFF: // BRK
                        Registers.Pc++;
                        Break();
                        break;
                }

                InvokeRegistersChanged();

                // Raise the B33PostOpcodeExecute event to signal that
                // an opcode was just executed
                if (B33PostOpcodeExecute != null)
                {
                    B33PostOpcodeExecute(this, isStore, storeAddress);
                }
            }

            // Loop is over, so virtual machine must be stopped
            State = States.Stopped;
            // Since the virtual machine has stopped, Raise the B33Stopped event
            if (B33Stopped != null)
                B33Stopped(this, false, 0);
        }

        // A thread-safe way to Poke
        private void ThreadPoke(ushort addr, byte data)
        {
            foreach (IB33Hardware h in Hardware)
            {
                lock (h)
                {
                    h.Poke(addr, data);
                }
            }

            lock (_memory)
            {
                _memory[addr] = data;
            }
        }

        /// <summary>
        /// Stores a value at the given address in the B33 virtual machine memory
        /// </summary>
        /// <param name="addr">The address in the B33 virtual machine memory.</param>
        /// <param name="value">The value to store at the address.</param>
        public void Poke(ushort addr, byte value)
        {
            // Iterate through each piece of hardware and "poke it"
            foreach (IB33Hardware h in Hardware)
            {
                h.Poke(addr, value);
            }

            // Store the value at the memory address
            _memory[addr] = value;
        }

        /// <summary>
        /// Returns the value stored at the given address in the B33 virtual machine memory.
        /// </summary>
        /// <param name="addr">The address in the B33 virtual machine memory.</param>
        /// <returns>System.Byte.</returns>
        public byte Peek(ushort addr)
        {
            return Peek(addr, false);
        }

        public byte Peek(ushort addr, bool memoryViewer)
        {
            // Look to see if any of our attached hardware uses memory
            // at the given address and if so, return that value instead
            foreach (IB33Hardware hardware in Hardware.Where(
                                  hardware => (addr >= hardware.MemoryLocation) &&
                                  (addr <= hardware.MemoryLocation + hardware.RequiredMemory)))
            {
                return hardware.Peek(addr, memoryViewer);
            }

            // Return the value stored in the B33 virtual machine memory at the address specified
            return _memory[addr];
        }
    }
}
