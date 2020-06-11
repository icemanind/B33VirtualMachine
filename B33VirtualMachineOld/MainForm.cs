using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Linq;

namespace B33VirtualMachine
{
    public partial class MainForm : Form
    {
        private string _b33File;
        private int _lastDebugIndex;
        private readonly CpuRegisters _registers;
        private readonly List<IB33Hardware> _hardware;
        private readonly Stack<byte> _8BitStack;
        private readonly Stack<ushort> _16BitStack;
        private readonly Stack<ushort> _callStack;
        private readonly byte[] _memory;
        private Thread _progThread;
        private readonly Queue<byte> _keyQueue;
        private ushort _debugInfoAddress;
        private List<DebugData> _debugData;
        private ManualResetEvent _pauseEvent;
        private bool _isPaused;
        private ushort _execAddress;
        private volatile bool _shouldStop;
        private B33Processor _processor;
        
        private const int SingleMonitorHeight = 407;
        private const int DualMonitorHeight = 692;

        private delegate void PokeCallBack(ushort addr, byte value);

        private delegate void RefreshCallBack();

        private delegate void SetCpuInfoTextCallBack(string text);

        private delegate void SetDebugInfoTextCallBack();

        public MainForm()
        {
            InitializeComponent();

            Size = new Size(Width, SingleMonitorHeight);

            _processor = new B33Processor();
            _memory = new byte[65535];
            _lastDebugIndex = 0;
            _debugData = new List<DebugData>();
            _shouldStop = false;
            _debugInfoAddress = 0;
            _isPaused = false;
            ClearMemory();
            _registers = new CpuRegisters {A = 0, B = 0, Pc = 0, X = 0, Y = 0, Cc = 0};
            _hardware = new List<IB33Hardware>();
            _8BitStack = new Stack<byte>();
            _16BitStack = new Stack<ushort>();
            _callStack = new Stack<ushort>();
            _keyQueue = new Queue<byte>();
            RegisterHardware();
            UpdateCpuInfoLabel();

            startToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem.Enabled = false;
            pauseToolStripMenuItem.Enabled = false;

            tsbPlay.Enabled = false;
            tsbPause.Enabled = false;
            tsbStop.Enabled = false;
        }

        private void MainFormLoad(object sender, EventArgs e)
        {
            _b33File = "";
        }

        private void RegisterHardware()
        {
            _hardware.Add(b33Screen1);
            _hardware.Add(b33Screen2);
        }

        private void ClearMemory()
        {
            for (int i = 0; i < 65535; i++)
            {
                _memory[i] = 0;
            }
        }

        private bool IsValidFile(string fileName)
        {
            byte[] magicNumbers;

            if (!File.Exists(fileName))
                return false;

            using (var reader = new BinaryReader(File.Open(fileName, FileMode.Open), Encoding.ASCII))
            {
                magicNumbers = reader.ReadBytes(3);
                reader.Close();
            }

            if (magicNumbers[0] == 66 && magicNumbers[1] == 51 && magicNumbers[2] == 51)
                return true;

            return false;
        }

        private void ReadFileIntoMemory(string fileName)
        {
            _debugData.Clear();
            using (var reader = new BinaryReader(File.Open(fileName, FileMode.Open), Encoding.ASCII)){
                reader.BaseStream.Seek(3, SeekOrigin.Begin);
                ushort startAddress = reader.ReadUInt16();
                ushort execAddress = reader.ReadUInt16();
                _debugInfoAddress = reader.ReadUInt16();
                byte dualMonitor = reader.ReadByte();
                Size = dualMonitor == 0 ? new Size(Width, SingleMonitorHeight) : new Size(Width, DualMonitorHeight);

                while (reader.PeekChar() != -1)
                {
                    _memory[startAddress] = reader.ReadByte();
                    startAddress++;
                }
                
                reader.Close();

                _registers.Pc = execAddress;
                _execAddress = execAddress;

                if (_debugInfoAddress > 0)
                {
                    ushort pointer = _debugInfoAddress;

                    byte a1 = _memory[pointer];
                    byte a2 = _memory[pointer + 1];
                    while (!(a1 == 0 && a2 == 0))
                    {
                        pointer += 2;
                        var addr = (ushort) ((a2 << 8) + a1);
                        string line = "";
                        byte c = _memory[pointer];

                        while (c != 0)
                        {
                            line = line + System.Text.Encoding.ASCII.GetString(new[] {c});
                            pointer++;
                            c = _memory[pointer];
                        }
                        _debugData.Add(new DebugData {Address = addr, SourceCodeLine = line});
                        pointer++;
                        a1 = _memory[pointer];
                        a2 = _memory[pointer + 1];
                    }
                    _debugData = _debugData.OrderBy(z => z.Address).ToList();
                    txtSourceCode.Text = "";
                    foreach (DebugData d in _debugData)
                    {
                        txtSourceCode.Text = string.Format("{0}${1} ==>{2}{3}", txtSourceCode.Text,
                                                           d.Address.ToString("X4"), d.SourceCodeLine,
                                                           Environment.NewLine);
                    }
                    ScrollDebugTo(execAddress);
                }
                else
                {
                    txtSourceCode.Text = "No Debug Info Available!";
                }
            }
        }

        private void ScrollDebugTo(ushort address)
        {
            if (!showDebugFlowInRealTimeSlowToolStripMenuItem.Checked)
                return;

            txtSourceCode.SelectAll();
            txtSourceCode.SelectionColor = Color.Black;
            txtSourceCode.SelectionFont = new Font("Courier New", 10, FontStyle.Regular);

            string hexAddress = address.ToString("X4");
            var m = Regex.Match(txtSourceCode.Text, @"^\$" + hexAddress, RegexOptions.Multiline);

            txtSourceCode.SelectionStart = m.Index;
            txtSourceCode.ScrollToCaret();

            int b = txtSourceCode.Text.IndexOf('\n', m.Index);
            txtSourceCode.Select(m.Index, b - m.Index);
            txtSourceCode.SelectionColor = Color.CornflowerBlue;
            txtSourceCode.SelectionFont = new Font("Courier New", 10, FontStyle.Bold);

            _lastDebugIndex = m.Index;
        }

        private void OpenB33BinaryToolStripMenuItemClick(object sender, EventArgs e)
        {
            DialogResult dr = ofdOpenBinary.ShowDialog(this);

            if (dr == DialogResult.OK && IsValidFile(ofdOpenBinary.FileName))
            {
                _b33File = ofdOpenBinary.FileName;
                ReadFileIntoMemory(_b33File);
                tsbPlay.Enabled = true;
                startToolStripMenuItem.Enabled = true;
                UpdateCpuInfoLabel();
            }
        }

        private void TsbOpenClick(object sender, EventArgs e)
        {
            OpenB33BinaryToolStripMenuItemClick(sender, e);
        }

        private void ExitToolStripMenuItemClick(object sender, EventArgs e)
        {
            _shouldStop = true;
            _pauseEvent.Set();
            Application.Exit();
        }

        private void SetCpuInfoLabel(string text)
        {
            if (lblCpuInfo.Text.Equals(text))
                return;
            lblCpuInfo.Text = text;
        }

        private void SetDebugInfo()
        {
            ScrollDebugTo(_registers.Pc);
        }

        private void UpdateDebugInfo()
        {
            if (txtSourceCode.InvokeRequired)
            {
                var z = new SetDebugInfoTextCallBack(SetDebugInfo);
                try
                {
                    Invoke(z);
                }
                catch
                {

                }
            }
            else
            {
                SetDebugInfo();
            }
        }

        private void UpdateCpuInfoLabel()
        {
            if (!updateRegisterStatusInRealTimeToolStripMenuItem.Checked)
                return;
            string label = string.Format("Program Name: {0}\n", string.IsNullOrEmpty(_b33File) ? "<none>" : Path.GetFileNameWithoutExtension(_b33File));

            label = label + string.Format("A : ${0}  B : ${1}  D : ${2}  X : ${3}  Y : ${4}  PC : ${5}  CC : ${6}", _processor.Registers.A.ToString("X2"),
                                          _processor.Registers.B.ToString("X2"), _processor.Registers.D.ToString("X4"), _processor.Registers.X.ToString("X4"), _processor.Registers.Y.ToString("X4"),
                                          _processor.Registers.Pc.ToString("X4"), _processor.Registers.Cc.ToString("X2"));

            if (lblCpuInfo.InvokeRequired)
            {
                var z = new SetCpuInfoTextCallBack(SetCpuInfoLabel);
                try
                {
                    Invoke(z, new object[] {label});
                }
                catch
                {
                }
            }
            else
            {
                SetCpuInfoLabel(label);
            }
        }

        private void ThreadPoke(ushort addr, byte value)
        {
            foreach (IB33Hardware h in _hardware)
            {
                if (h.ThreadInvokeRequired)
                {
                    var pcb = new PokeCallBack(Poke);
                    Invoke(pcb, new object[] {addr, value});
                }
                else
                {
                    Poke(addr, value);
                }
            }
        }

        private void Poke(ushort addr, byte value)
        {
            lock (_memory)
            {
                _memory[addr] = value;
                foreach (IB33Hardware h in _hardware)
                {
                    lock (h)
                    {
                        h.Poke(addr, value);
                    }
                }
            }
        }

        private void ThreadForceScreenRefresh()
        {
            if (b33Screen1.ThreadInvokeRequired)
            {
                var rcb = new RefreshCallBack(ForceScreenRefresh);
                Invoke(rcb);
            }
            else
            {
                ForceScreenRefresh();
            }
        }

        private void ForceScreenRefresh()
        {
            lock (_memory)
            {
                foreach (IB33Hardware h in _hardware)
                {
                    lock (h)
                    {
                        h.ForceRefresh();
                    }
                }
            }
        }

        private byte Peek(ushort addr)
        {
            return _memory[addr];
        }

        private void TmrPulseTick(object sender, EventArgs e)
        {
            if (!_progThread.IsAlive)
            {
                tmrPulse.Enabled = false;

                tsbPlay.Enabled = true;
                tsbStop.Enabled = false;
                tsbPause.Enabled = false;

                startToolStripMenuItem.Enabled = true;
                stopToolStripMenuItem.Enabled = false;
                pauseToolStripMenuItem.Enabled = false;

                MessageBox.Show(this, "Program has Terminated!", "Program Ended", MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            }
        }

        private void MainFormFormClosing(object sender, FormClosingEventArgs e)
        {
            _shouldStop = true;
            if (_pauseEvent != null)
                _pauseEvent.Set();
        }

        private void B33Screen1KeyPress(object sender, KeyPressEventArgs e)
        {
            var k = (byte)e.KeyChar;
            _keyQueue.Enqueue(k);
        }

        private void StopToolStripMenuItemClick(object sender, EventArgs e)
        {
            _shouldStop = true;
            _isPaused = false;
            _pauseEvent.Set();
        }

        private void TsbStopClick(object sender, EventArgs e)
        {
            StopToolStripMenuItemClick(sender, e);
        }

        private void PauseToolStripMenuItemClick(object sender, EventArgs e)
        {
            _isPaused = !_isPaused;
            if (_isPaused)
            {
                _pauseEvent.Reset();
            }
            else
            {
                _pauseEvent.Set();
            }
        }

        private void TsbPauseClick(object sender, EventArgs e)
        {
            PauseToolStripMenuItemClick(sender, e);
        }

        private void TsbPlayClick(object sender, EventArgs e)
        {
            StartToolStripMenuItemClick(sender, e);
        }

        private void StartToolStripMenuItemClick(object sender, EventArgs e)
        {
            _processor = new B33Processor();

            _progThread = new Thread(() => _processor.Start(_execAddress, UpdateCpuInfoLabel, UpdateDebugInfo));

            _processor.TransferMemory(_memory);
            _progThread.Start();
            return;
            foreach (var q in _hardware)
            {
                if (q is B33Screen)
                {
                    var w = q as B33Screen;
                    w.Reset();
                }
            }

            _8BitStack.Clear();
            _16BitStack.Clear();
            _callStack.Clear();
            _keyQueue.Clear();

            _shouldStop = false;

            _progThread = new Thread(() => ExecuteProgram(_execAddress));
            _pauseEvent = new ManualResetEvent(true);
            _progThread.Start();
            tmrPulse.Enabled = true;

            tsbPlay.Enabled = false;
            tsbStop.Enabled = true;
            tsbPause.Enabled = true;

            startToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem.Enabled = true;
            pauseToolStripMenuItem.Enabled = true;
        }

        private void TxtSourceCodeKeyPress(object sender, KeyPressEventArgs e)
        {
            var k = (byte)e.KeyChar;
            _keyQueue.Enqueue(k);
        }

        private void ResetSpeedChecks()
        {
            ms12SecondToolStripMenuItem.Checked = false;
            ms14SecondToolStripMenuItem.Checked = false;
            ms18SecondToolStripMenuItem.Checked = false;
            ms1SecondToolStripMenuItem.Checked = false;
            ms34SecondToolStripMenuItem.Checked = false;
            ms116SecondToolStripMenuItem.Checked = false;
            realTimeToolStripMenuItem.Checked = false;
        }

        private void RealTimeToolStripMenuItemClick(object sender, EventArgs e)
        {
            ResetSpeedChecks();
            realTimeToolStripMenuItem.Checked = true;
        }

        private void Ms34SecondToolStripMenuItemClick(object sender, EventArgs e)
        {
            ResetSpeedChecks();
            ms34SecondToolStripMenuItem.Checked = true;
        }

        private void Ms12SecondToolStripMenuItemClick(object sender, EventArgs e)
        {
            ResetSpeedChecks();
            ms12SecondToolStripMenuItem.Checked = true;
        }

        private void Ms14SecondToolStripMenuItemClick(object sender, EventArgs e)
        {
            ResetSpeedChecks();
            ms14SecondToolStripMenuItem.Checked = true;
        }

        private void Ms18SecondToolStripMenuItemClick(object sender, EventArgs e)
        {
            ResetSpeedChecks();
            ms18SecondToolStripMenuItem.Checked = true;
        }

        private void Ms1SecondToolStripMenuItemClick(object sender, EventArgs e)
        {
            ResetSpeedChecks();
            ms1SecondToolStripMenuItem.Checked = true;
        }

        private void Ms116SecondToolStripMenuItemClick(object sender, EventArgs e)
        {
            ResetSpeedChecks();
            ms116SecondToolStripMenuItem.Checked = true;
        }

        private void B33Screen2KeyPress(object sender, KeyPressEventArgs e)
        {
            var k = (byte)e.KeyChar;
            _keyQueue.Enqueue(k);
        }

        private void Push(byte val)
        {
            _8BitStack.Push(val);
        }

        private void Push(ushort val)
        {
            _16BitStack.Push(val);
        }

        private byte PopByte()
        {
            return _8BitStack.Pop();
        }

        private ushort PopUshort()
        {
            return _16BitStack.Pop();
        }

        private void ExecuteProgram(ushort startAddress)
        {
            _registers.Pc = startAddress;

            while (true)
            {
                UpdateDebugInfo();
                byte opcode = _memory[_registers.Pc];
                if (_shouldStop)
                {
                    _registers.Pc = startAddress;
                    break;
                }

                if (!realTimeToolStripMenuItem.Checked)
                {
                    if (ms12SecondToolStripMenuItem.Checked)
                        Thread.Sleep(500);
                    if (ms14SecondToolStripMenuItem.Checked)
                        Thread.Sleep(250);
                    if (ms18SecondToolStripMenuItem.Checked)
                        Thread.Sleep(125);
                    if (ms1SecondToolStripMenuItem.Checked)
                        Thread.Sleep(1000);
                    if (ms34SecondToolStripMenuItem.Checked)
                        Thread.Sleep(750);
                    if (ms116SecondToolStripMenuItem.Checked)
                        Thread.Sleep(62);
                }

                _pauseEvent.WaitOne(Timeout.Infinite);
                
                if (opcode == 0) // END
                {
                    _registers.Pc = startAddress;
                    
                    return;
                }

                if (opcode == 1) // LDA
                {
                    _registers.A = _memory[_registers.Pc + 1];
                    _registers.Pc = (ushort) (_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }

                if (opcode == 2) // LDB
                {
                    _registers.B = _memory[_registers.Pc + 1];
                    _registers.Pc = (ushort) (_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }

                if (opcode == 3) // LDX
                {
                    _registers.X = BitConverter.ToUInt16(new[] {_memory[_registers.Pc + 1], _memory[_registers.Pc + 2]}, 0);
                    _registers.Pc = (ushort) (_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }

                if (opcode == 4) // LDY
                {
                    _registers.Y = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);
                    _registers.Pc = (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }

                if (opcode == 5) // LDD
                {
                    _registers.D = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);
                    _registers.Pc = (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }

                if (opcode == 6) // STA
                {
                    ushort addr = BitConverter.ToUInt16(new[] {_memory[_registers.Pc + 1], _memory[_registers.Pc + 2]}, 0);
                    ThreadPoke(addr, _registers.A);
                    _registers.Pc = (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }

                if (opcode == 7) // STB
                {
                    ushort addr = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);
                    ThreadPoke(addr, _registers.B);
                    _registers.Pc = (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }

                if (opcode == 8) // STX
                {
                    ushort addr = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);
                    ThreadPoke(addr, (byte)(_registers.X >> 8));
                    ThreadPoke((ushort) (addr + 1), (byte) (_registers.X & 0xff));
                    _registers.Pc = (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }

                if (opcode == 9) // STY
                {
                    ushort addr = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);
                    ThreadPoke(addr, (byte)(_registers.Y >> 8));
                    ThreadPoke((ushort)(addr + 1), (byte)(_registers.Y & 0xff));
                    _registers.Pc = (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }

                if (opcode == 0x0A) // STD
                {
                    ushort addr = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);
                    ThreadPoke(addr, (byte)(_registers.D >> 8));
                    ThreadPoke((ushort)(addr + 1), (byte)(_registers.D & 0xff));
                    _registers.Pc = (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }

                if (opcode == 0x0B) // LDA Extended
                {
                    ushort addr = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);
                    _registers.A = Peek(addr);
                    _registers.Pc = (ushort) (_registers.Pc + 3);
                }

                if (opcode == 0x0C) // LDB Extended
                {
                    ushort addr = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);
                    _registers.B = Peek(addr);
                    _registers.Pc = (ushort)(_registers.Pc + 3);
                }

                if (opcode == 0x0D) // LDX Extended
                {
                    ushort addr = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);
                    _registers.X = (ushort)((Peek(addr) << 8) + Peek((ushort)(addr + 1)));
                    _registers.Pc = (ushort)(_registers.Pc + 3);
                }

                if (opcode == 0x0E) // LDY Extended
                {
                    ushort addr = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);
                    _registers.Y = (ushort)((Peek(addr) << 8) + Peek((ushort)(addr + 1)));
                    _registers.Pc = (ushort)(_registers.Pc + 3);
                }

                if (opcode == 0x0F) // LDD Extended
                {
                    ushort addr = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);
                    _registers.D = (ushort) ((Peek(addr) << 8) + Peek((ushort) (addr + 1)));
                    _registers.Pc = (ushort)(_registers.Pc + 3);
                }

                if (opcode == 0x10) // STA
                {
                    byte ro = _memory[_registers.Pc + 1];
                    byte r = _memory[_registers.Pc + 2];
                    
                    byte offset = 0;
                    if (ro == 1)
                        offset = _registers.A;
                    if (ro == 2)
                        offset = _registers.B;
                    if ((r&4) == 4)
                    {
                        ThreadPoke((ushort) (_registers.D + offset), _registers.A);
                        if ((r & 32) == 32)
                        {
                            _registers.D = (ushort) (_registers.D + 1);
                            if ((r & 128) == 128)
                            {
                                _registers.D = (ushort)(_registers.D + 1);
                            }
                        }
                        if ((r & 64) == 64)
                        {
                            _registers.D = (ushort) (_registers.D - 1);
                            if ((r & 128) == 128)
                            {
                                _registers.D = (ushort)(_registers.D - 1);
                            }
                        }
                    }
                    if ((r & 8) == 8)
                    {
                        ThreadPoke((ushort)(_registers.X + offset), _registers.A);
                        if ((r & 32) == 32)
                        {
                            _registers.X = (ushort) (_registers.X + 1);
                            if ((r & 128) == 128)
                            {
                                _registers.X = (ushort)(_registers.X + 1);
                            }
                        }
                        if ((r & 64) == 64)
                        {
                            _registers.X = (ushort) (_registers.X - 1);
                            if ((r & 128) == 128)
                            {
                                _registers.X = (ushort)(_registers.X - 1);
                            }
                        }
                    }
                    if ((r & 16) == 16)
                    {
                        ThreadPoke((ushort)(_registers.Y + offset), _registers.A);
                        if ((r & 32) == 32)
                        {
                            _registers.Y = (ushort) (_registers.Y + 1);
                            if ((r & 128) == 128)
                            {
                                _registers.Y = (ushort)(_registers.Y + 1);
                            }
                        }
                        if ((r & 64) == 64)
                        {
                            _registers.Y = (ushort) (_registers.Y - 1);
                            if ((r & 128) == 128)
                            {
                                _registers.Y = (ushort)(_registers.Y - 1);
                            }
                        }
                    }
                    _registers.Pc = (ushort) (_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x11) // STB
                {
                    byte ro = _memory[_registers.Pc + 1];
                    byte r = _memory[_registers.Pc + 2];

                    byte offset = 0;
                    if (ro == 1)
                        offset = _registers.A;
                    if (ro == 2)
                        offset = _registers.B;
                    if ((r & 4) == 4)
                    {
                        ThreadPoke((ushort)(_registers.D + offset), _registers.B);
                        if ((r & 32) == 32)
                        {
                            _registers.D = (ushort)(_registers.D + 1);
                            if ((r & 128) == 128)
                            {
                                _registers.D = (ushort)(_registers.D + 1);
                            }
                        }
                        if ((r & 64) == 64)
                        {
                            _registers.D = (ushort)(_registers.D - 1);
                            if ((r & 128) == 128)
                            {
                                _registers.D = (ushort)(_registers.D - 1);
                            }
                        }
                    }
                    if ((r & 8) == 8)
                    {
                        ThreadPoke((ushort)(_registers.X + offset), _registers.B);
                        if ((r & 32) == 32)
                        {
                            _registers.X = (ushort)(_registers.X + 1);
                            if ((r & 128) == 128)
                            {
                                _registers.X = (ushort)(_registers.X + 1);
                            }
                        }
                        if ((r & 64) == 64)
                        {
                            _registers.X = (ushort)(_registers.X - 1);
                            if ((r & 128) == 128)
                            {
                                _registers.X = (ushort)(_registers.X - 1);
                            }
                        }
                    }
                    if ((r & 16) == 16)
                    {
                        ThreadPoke((ushort)(_registers.Y + offset), _registers.B);
                        if ((r & 32) == 32)
                        {
                            _registers.Y = (ushort)(_registers.Y + 1);
                            if ((r & 128) == 128)
                            {
                                _registers.Y = (ushort)(_registers.Y + 1);
                            }
                        }
                        if ((r & 64) == 64)
                        {
                            _registers.Y = (ushort)(_registers.Y - 1);
                            if ((r & 128) == 128)
                            {
                                _registers.Y = (ushort)(_registers.Y - 1);
                            }
                        }
                    }
                    _registers.Pc = (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x12) // CMPA
                {
                    byte val = _memory[_registers.Pc + 1];

                    if (_registers.A == val)
                        _registers.Cc = (byte) (_registers.Cc | 2);
                    else _registers.Cc = (byte) (_registers.Cc & 0xFD);

                    if (_registers.A > val)
                        _registers.Cc = (byte)(_registers.Cc | 4);
                    else _registers.Cc = (byte)(_registers.Cc & 0xFB);

                    if (_registers.A < val)
                        _registers.Cc = (byte)(_registers.Cc | 8);
                    else _registers.Cc = (byte)(_registers.Cc & 0xF7);

                    if (_registers.A != val)
                        _registers.Cc = (byte)(_registers.Cc | 16);
                    else _registers.Cc = (byte)(_registers.Cc & 0xEF);

                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x13) // CMPB
                {
                    byte val = _memory[_registers.Pc + 1];

                    if (_registers.B == val)
                        _registers.Cc = (byte)(_registers.Cc | 2);
                    else _registers.Cc = (byte)(_registers.Cc & 0xFD);

                    if (_registers.B > val)
                        _registers.Cc = (byte)(_registers.Cc | 4);
                    else _registers.Cc = (byte)(_registers.Cc & 0xFB);

                    if (_registers.B < val)
                        _registers.Cc = (byte)(_registers.Cc | 8);
                    else _registers.Cc = (byte)(_registers.Cc & 0xF7);

                    if (_registers.B != val)
                        _registers.Cc = (byte)(_registers.Cc | 16);
                    else _registers.Cc = (byte)(_registers.Cc & 0xEF);

                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x14) // CMPD
                {
                    ushort val = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);

                    if (_registers.D == val)
                        _registers.Cc = (byte)(_registers.Cc | 2);
                    else _registers.Cc = (byte)(_registers.Cc & 0xFD);

                    if (_registers.D > val)
                        _registers.Cc = (byte)(_registers.Cc | 4);
                    else _registers.Cc = (byte)(_registers.Cc & 0xFB);

                    if (_registers.D < val)
                        _registers.Cc = (byte)(_registers.Cc | 8);
                    else _registers.Cc = (byte)(_registers.Cc & 0xF7);

                    if (_registers.D != val)
                        _registers.Cc = (byte)(_registers.Cc | 16);
                    else _registers.Cc = (byte)(_registers.Cc & 0xEF);

                    _registers.Pc = (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x15) // CMPX
                {
                    ushort val = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);

                    if (_registers.X == val)
                        _registers.Cc = (byte)(_registers.Cc | 2);
                    else _registers.Cc = (byte)(_registers.Cc & 0xFD);

                    if (_registers.X > val)
                        _registers.Cc = (byte)(_registers.Cc | 4);
                    else _registers.Cc = (byte)(_registers.Cc & 0xFB);

                    if (_registers.X < val)
                        _registers.Cc = (byte)(_registers.Cc | 8);
                    else _registers.Cc = (byte)(_registers.Cc & 0xF7);

                    if (_registers.X != val)
                        _registers.Cc = (byte)(_registers.Cc | 16);
                    else _registers.Cc = (byte)(_registers.Cc & 0xEF);

                    _registers.Pc = (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x16) // CMPY
                {
                    ushort val = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);

                    if (_registers.Y == val)
                        _registers.Cc = (byte)(_registers.Cc | 2);
                    else _registers.Cc = (byte)(_registers.Cc & 0xFD);

                    if (_registers.Y > val)
                        _registers.Cc = (byte)(_registers.Cc | 4);
                    else _registers.Cc = (byte)(_registers.Cc & 0xFB);

                    if (_registers.Y < val)
                        _registers.Cc = (byte)(_registers.Cc | 8);
                    else _registers.Cc = (byte)(_registers.Cc & 0xF7);

                    if (_registers.Y != val)
                        _registers.Cc = (byte)(_registers.Cc | 16);
                    else _registers.Cc = (byte)(_registers.Cc & 0xEF);

                    _registers.Pc = (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x17) // JEQ
                {
                    ushort addr = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);
                    var jump = (byte) (_registers.Cc & 2);

                    _registers.Pc = jump == 2 ? addr : (ushort) (_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x18) // JNE
                {
                    ushort addr = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);
                    var jump = (byte)(_registers.Cc & 16);

                    _registers.Pc = jump == 16 ? addr : (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x19) // LDA indexed
                {
                    byte ro = _memory[_registers.Pc + 1];
                    byte r = _memory[_registers.Pc + 2];

                    byte offset = 0;
                    if (ro == 1)
                        offset = _registers.A;
                    if (ro == 2)
                        offset = _registers.B;

                    if ((r & 8) == 8)
                    {
                        _registers.A = Peek((ushort) (_registers.X + offset));
                        if ((r & 32) == 32)
                        {
                            _registers.X = (ushort)(_registers.X + 1);
                            if ((r & 128) == 128)
                            {
                                _registers.X = (ushort)(_registers.X + 1);
                            }
                        }
                        if ((r & 64) == 64)
                        {
                            _registers.X = (ushort)(_registers.X - 1);
                            if ((r & 128) == 128)
                            {
                                _registers.X = (ushort)(_registers.X - 1);
                            }
                        }
                    }
                    if ((r & 4) == 4)
                    {
                        _registers.A = Peek((ushort) (_registers.D + offset));
                        if ((r & 32) == 32)
                        {
                            _registers.D = (ushort)(_registers.D + 1);
                            if ((r & 128) == 128)
                            {
                                _registers.D = (ushort)(_registers.D + 1);
                            }
                        }
                        if ((r & 64) == 64)
                        {
                            _registers.D = (ushort)(_registers.D - 1);
                            if ((r & 128) == 128)
                            {
                                _registers.D = (ushort)(_registers.D - 1);
                            }
                        }
                    }
                    if ((r & 16) == 16)
                    {
                        _registers.A = Peek((ushort) (_registers.Y + offset));
                        if ((r & 32) == 32)
                        {
                            _registers.Y = (ushort) (_registers.Y + 1);
                            if ((r & 128) == 128)
                            {
                                _registers.Y = (ushort)(_registers.Y + 1);
                            }
                        }
                        if ((r & 64) == 64)
                        {
                            _registers.Y = (ushort)(_registers.Y - 1);
                            if ((r & 128) == 128)
                            {
                                _registers.Y = (ushort)(_registers.Y - 1);
                            }
                        }
                    }
                    _registers.Pc = (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x1A) // LDB indexed
                {
                    byte ro = _memory[_registers.Pc + 1];
                    byte r = _memory[_registers.Pc + 2];

                    byte offset = 0;
                    if (ro == 1)
                        offset = _registers.A;
                    if (ro == 2)
                        offset = _registers.B;

                    if ((r & 8) == 8)
                    {
                        _registers.B = Peek((ushort)(_registers.X + offset));
                        if ((r & 32) == 32)
                        {
                            _registers.X = (ushort)(_registers.X + 1);
                            if ((r & 128) == 128)
                            {
                                _registers.X = (ushort)(_registers.X + 1);
                            }
                        }
                        if ((r & 64) == 64)
                        {
                            _registers.X = (ushort)(_registers.X - 1);
                            if ((r & 128) == 128)
                            {
                                _registers.X = (ushort)(_registers.X - 1);
                            }
                        }
                    }
                    if ((r & 4) == 4)
                    {
                        _registers.B = Peek((ushort)(_registers.D + offset));
                        if ((r & 32) == 32)
                        {
                            _registers.D = (ushort)(_registers.D + 1);
                            if ((r & 128) == 128)
                            {
                                _registers.D = (ushort)(_registers.D + 1);
                            }
                        }
                        if ((r & 64) == 64)
                        {
                            _registers.D = (ushort)(_registers.D - 1);
                            if ((r & 128) == 128)
                            {
                                _registers.D = (ushort)(_registers.D - 1);
                            }
                        }
                    }
                    if ((r & 16) == 16)
                    {
                        _registers.B = Peek((ushort)(_registers.Y + offset));
                        if ((r & 32) == 32)
                        {
                            _registers.Y = (ushort)(_registers.Y + 1);
                            if ((r & 128) == 128)
                            {
                                _registers.Y = (ushort)(_registers.Y + 1);
                            }
                        }
                        if ((r & 64) == 64)
                        {
                            _registers.Y = (ushort)(_registers.Y - 1);
                            if ((r & 128) == 128)
                            {
                                _registers.Y = (ushort)(_registers.Y - 1);
                            }
                        }
                    }
                    _registers.Pc = (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x20) // JGE
                {
                    ushort addr = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);
                    var jump = (byte)(_registers.Cc & 6);

                    _registers.Pc = jump > 0 ? addr : (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x21) // JLE
                {
                    ushort addr = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);
                    var jump = (byte)(_registers.Cc & 10);

                    _registers.Pc = jump > 0 ? addr : (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x22) // JLT
                {
                    ushort addr = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);
                    var jump = (byte)(_registers.Cc & 8);

                    _registers.Pc = jump == 8 ? addr : (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x23) // JGT
                {
                    ushort addr = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);
                    var jump = (byte)(_registers.Cc & 4);

                    _registers.Pc = jump == 4 ? addr : (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x24) // JMP
                {
                    ushort addr = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);

                    _registers.Pc = addr;
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x25) // PUSH
                {
                    byte r = _memory[_registers.Pc + 1];

                    if ((r & 1) == 1)
                        Push(_registers.A);
                    if ((r & 2) == 2)
                        Push(_registers.B);
                    if ((r & 4) == 4)
                        Push(_registers.D);
                    if ((r & 8) == 8)
                        Push(_registers.X);
                    if ((r & 16) == 16)
                        Push(_registers.Y);

                    _registers.Pc = (ushort) (_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x26) // POP
                {
                    byte r = _memory[_registers.Pc + 1];

                    if ((r & 1) == 1)
                        _registers.A = PopByte();
                    if ((r & 2) == 2)
                        _registers.B = PopByte();
                    if ((r & 4) == 4)
                        _registers.D = PopUshort();
                    if ((r & 8) == 8)
                        _registers.X = PopUshort();
                    if ((r & 16) == 16)
                        _registers.Y = PopUshort();
                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x27) // CALL
                {
                    ushort addr = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);
                    _registers.Pc = (ushort)(_registers.Pc + 3);
                    _callStack.Push(_registers.Pc);
                    _registers.Pc = addr;
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x28) // RET
                {
                    _registers.Pc = _callStack.Pop();
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x29) // KEY
                {
                    byte vv = _memory[_registers.Pc + 1];

                    if (vv == 1)
                    {
                        _registers.A = _keyQueue.Count == 0 ? (byte)0 : _keyQueue.Dequeue();
                    }

                    if (vv == 2)
                    {
                        _registers.B = _keyQueue.Count == 0 ? (byte)0 : _keyQueue.Dequeue();
                    }

                    _registers.Pc = (ushort) (_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x2A) // SUBA
                {
                    byte val = _memory[_registers.Pc + 1];

                    _registers.A = (byte) (_registers.A - val);

                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x2B) // SUBB
                {
                    byte val = _memory[_registers.Pc + 1];

                    _registers.B = (byte)(_registers.B - val);

                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x2C) // SUBD
                {
                    ushort val = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);
                    
                    _registers.D = (ushort)(_registers.D - val);

                    _registers.Pc = (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x2D) // SUBX
                {
                    ushort val = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);

                    _registers.X = (ushort)(_registers.X - val);

                    _registers.Pc = (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x2E) // SUBY
                {
                    ushort val = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);

                    _registers.Y = (ushort)(_registers.Y - val);

                    _registers.Pc = (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x2F) // ADDA
                {
                    byte val = _memory[_registers.Pc + 1];

                    _registers.A = (byte)(_registers.A + val);

                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x30) // ADDB
                {
                    byte val = _memory[_registers.Pc + 1];

                    _registers.B = (byte)(_registers.B + val);

                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x31) // ADDD
                {
                    ushort val = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);

                    _registers.D = (ushort)(_registers.D + val);

                    _registers.Pc = (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x32) // ADDX
                {
                    ushort val = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);

                    _registers.X = (ushort)(_registers.X + val);

                    _registers.Pc = (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x33) // ADDY
                {
                    ushort val = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);

                    _registers.Y = (ushort)(_registers.Y + val);

                    _registers.Pc = (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x34) // SUBA register
                {
                    byte r = _memory[_registers.Pc + 1];

                    if (r == 1)
                        _registers.A = (byte) (_registers.A - _registers.A);
                    if (r == 2)
                        _registers.A = (byte) (_registers.A - _registers.B);
                    _registers.Pc = (ushort) (_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x35) // SUBB register
                {
                    byte r = _memory[_registers.Pc + 1];

                    if (r == 1)
                        _registers.B = (byte)(_registers.B - _registers.A);
                    if (r == 2)
                        _registers.B = (byte)(_registers.B - _registers.B);
                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x36) // SUBD register
                {
                    byte r = _memory[_registers.Pc + 1];

                    if (r == 1)
                        _registers.D = (ushort) (_registers.D - _registers.A);
                    if (r == 2)
                        _registers.D = (ushort)(_registers.D - _registers.B);
                    if (r == 4)
                        _registers.D = (ushort)(_registers.D - _registers.D);
                    if (r == 8)
                        _registers.D = (ushort)(_registers.D - _registers.X);
                    if (r == 16)
                        _registers.D = (ushort)(_registers.D - _registers.Y);
                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x37) // SUBX register
                {
                    byte r = _memory[_registers.Pc + 1];

                    if (r == 1)
                        _registers.X = (ushort)(_registers.X - _registers.A);
                    if (r == 2)
                        _registers.X = (ushort)(_registers.X - _registers.B);
                    if (r == 4)
                        _registers.X = (ushort)(_registers.X - _registers.D);
                    if (r == 8)
                        _registers.X = (ushort)(_registers.X - _registers.X);
                    if (r == 16)
                        _registers.X = (ushort)(_registers.X - _registers.Y);
                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x38) // SUBY register
                {
                    byte r = _memory[_registers.Pc + 1];

                    if (r == 1)
                        _registers.Y = (ushort)(_registers.Y - _registers.A);
                    if (r == 2)
                        _registers.Y = (ushort)(_registers.Y - _registers.B);
                    if (r == 4)
                        _registers.Y = (ushort)(_registers.Y - _registers.D);
                    if (r == 8)
                        _registers.Y = (ushort)(_registers.Y - _registers.X);
                    if (r == 16)
                        _registers.Y = (ushort)(_registers.Y - _registers.Y);
                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x39) // ADDA register
                {
                    byte r = _memory[_registers.Pc + 1];

                    if (r == 1)
                        _registers.A = (byte)(_registers.A + _registers.A);
                    if (r == 2)
                        _registers.A = (byte)(_registers.A + _registers.B);
                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x3A) // ADDB register
                {
                    byte r = _memory[_registers.Pc + 1];

                    if (r == 1)
                        _registers.B = (byte)(_registers.B + _registers.A);
                    if (r == 2)
                        _registers.B = (byte)(_registers.B + _registers.B);
                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x3B) // ADDD register
                {
                    byte r = _memory[_registers.Pc + 1];

                    if (r == 1)
                        _registers.D = (ushort)(_registers.D + _registers.A);
                    if (r == 2)
                        _registers.D = (ushort)(_registers.D + _registers.B);
                    if (r == 4)
                        _registers.D = (ushort)(_registers.D + _registers.D);
                    if (r == 8)
                        _registers.D = (ushort)(_registers.D + _registers.X);
                    if (r == 16)
                        _registers.D = (ushort)(_registers.D + _registers.Y);
                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x3C) // ADDX register
                {
                    byte r = _memory[_registers.Pc + 1];

                    if (r == 1)
                        _registers.X = (ushort)(_registers.X + _registers.A);
                    if (r == 2)
                        _registers.X = (ushort)(_registers.X + _registers.B);
                    if (r == 4)
                        _registers.X = (ushort)(_registers.X + _registers.D);
                    if (r == 8)
                        _registers.X = (ushort)(_registers.X + _registers.X);
                    if (r == 16)
                        _registers.X = (ushort)(_registers.X + _registers.Y);
                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x3D) // ADDY register
                {
                    byte r = _memory[_registers.Pc + 1];

                    if (r == 1)
                        _registers.Y = (ushort)(_registers.Y + _registers.A);
                    if (r == 2)
                        _registers.Y = (ushort)(_registers.Y + _registers.B);
                    if (r == 4)
                        _registers.Y = (ushort)(_registers.Y + _registers.D);
                    if (r == 8)
                        _registers.Y = (ushort)(_registers.Y + _registers.X);
                    if (r == 16)
                        _registers.Y = (ushort)(_registers.Y + _registers.Y);
                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x3E) // TFR
                {
                    byte sourceRegister = _memory[_registers.Pc + 1];
                    byte destinarionRegister = _memory[_registers.Pc + 2];

                    if (sourceRegister == 1)
                    {
                        switch (destinarionRegister)
                        {
                            case 1:
                                _registers.A = _registers.A;
                                break;
                            case 2:
                                _registers.B = _registers.A;
                                break;
                            case 4:
                                _registers.D = _registers.A;
                                break;
                            case 8:
                                _registers.X = _registers.A;
                                break;
                            case 16:
                                _registers.Y = _registers.A;
                                break;
                        }
                    }
                    if (sourceRegister == 2)
                    {
                        switch (destinarionRegister)
                        {
                            case 1:
                                _registers.A = _registers.B;
                                break;
                            case 2:
                                _registers.B = _registers.B;
                                break;
                            case 4:
                                _registers.D = _registers.B;
                                break;
                            case 8:
                                _registers.X = _registers.B;
                                break;
                            case 16:
                                _registers.Y = _registers.B;
                                break;
                        }
                    }
                    if (sourceRegister == 4)
                    {
                        switch (destinarionRegister)
                        {
                            case 4:
                                _registers.D = _registers.D;
                                break;
                            case 8:
                                _registers.X = _registers.D;
                                break;
                            case 16:
                                _registers.Y = _registers.D;
                                break;
                        }
                    }
                    if (sourceRegister == 8)
                    {
                        switch (destinarionRegister)
                        {
                            case 4:
                                _registers.D = _registers.X;
                                break;
                            case 8:
                                _registers.X = _registers.X;
                                break;
                            case 16:
                                _registers.Y = _registers.X;
                                break;
                        }
                    }
                    if (sourceRegister == 16)
                    {
                        switch (destinarionRegister)
                        {
                            case 4:
                                _registers.D = _registers.Y;
                                break;
                            case 8:
                                _registers.X = _registers.Y;
                                break;
                            case 16:
                                _registers.Y = _registers.Y;
                                break;
                        }
                    }
                    _registers.Pc = (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x3F) // CMPA Register
                {
                    byte r = _memory[_registers.Pc + 1];
                    ushort val = 0;

                    if (r == 1)
                        val = _registers.A;
                    if (r == 2)
                        val = _registers.B;
                    if (r == 4)
                        val = _registers.D;
                    if (r == 8)
                        val = _registers.X;
                    if (r == 16)
                        val = _registers.Y;

                    if (_registers.A == val)
                        _registers.Cc = (byte)(_registers.Cc | 2);
                    else _registers.Cc = (byte)(_registers.Cc & 0xFD);

                    if (_registers.A > val)
                        _registers.Cc = (byte)(_registers.Cc | 4);
                    else _registers.Cc = (byte)(_registers.Cc & 0xFB);

                    if (_registers.A < val)
                        _registers.Cc = (byte)(_registers.Cc | 8);
                    else _registers.Cc = (byte)(_registers.Cc & 0xF7);

                    if (_registers.A != val)
                        _registers.Cc = (byte)(_registers.Cc | 16);
                    else _registers.Cc = (byte)(_registers.Cc & 0xEF);

                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x40) // CMPB Register
                {
                    byte r = _memory[_registers.Pc + 1];
                    ushort val = 0;

                    if (r == 1)
                        val = _registers.A;
                    if (r == 2)
                        val = _registers.B;
                    if (r == 4)
                        val = _registers.D;
                    if (r == 8)
                        val = _registers.X;
                    if (r == 16)
                        val = _registers.Y;

                    if (_registers.B == val)
                        _registers.Cc = (byte)(_registers.Cc | 2);
                    else _registers.Cc = (byte)(_registers.Cc & 0xFD);

                    if (_registers.B > val)
                        _registers.Cc = (byte)(_registers.Cc | 4);
                    else _registers.Cc = (byte)(_registers.Cc & 0xFB);

                    if (_registers.B < val)
                        _registers.Cc = (byte)(_registers.Cc | 8);
                    else _registers.Cc = (byte)(_registers.Cc & 0xF7);

                    if (_registers.B != val)
                        _registers.Cc = (byte)(_registers.Cc | 16);
                    else _registers.Cc = (byte)(_registers.Cc & 0xEF);

                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x41) // CMPD Register
                {
                    byte r = _memory[_registers.Pc + 1];
                    ushort val = 0;

                    if (r == 1)
                        val = _registers.A;
                    if (r == 2)
                        val = _registers.B;
                    if (r == 4)
                        val = _registers.D;
                    if (r == 8)
                        val = _registers.X;
                    if (r == 16)
                        val = _registers.Y;

                    if (_registers.D == val)
                        _registers.Cc = (byte) (_registers.Cc | 2);
                    else _registers.Cc = (byte) (_registers.Cc & 0xFD);

                    if (_registers.D > val)
                        _registers.Cc = (byte) (_registers.Cc | 4);
                    else _registers.Cc = (byte) (_registers.Cc & 0xFB);

                    if (_registers.D < val)
                        _registers.Cc = (byte) (_registers.Cc | 8);
                    else _registers.Cc = (byte) (_registers.Cc & 0xF7);

                    if (_registers.D != val)
                        _registers.Cc = (byte) (_registers.Cc | 16);
                    else _registers.Cc = (byte) (_registers.Cc & 0xEF);

                    _registers.Pc = (ushort) (_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x42) // CMPX Register
                {
                    byte r = _memory[_registers.Pc + 1];
                    ushort val = 0;

                    if (r == 1)
                        val = _registers.A;
                    if (r == 2)
                        val = _registers.B;
                    if (r == 4)
                        val = _registers.D;
                    if (r == 8)
                        val = _registers.X;
                    if (r == 16)
                        val = _registers.Y;

                    if (_registers.X == val)
                        _registers.Cc = (byte)(_registers.Cc | 2);
                    else _registers.Cc = (byte)(_registers.Cc & 0xFD);

                    if (_registers.X > val)
                        _registers.Cc = (byte)(_registers.Cc | 4);
                    else _registers.Cc = (byte)(_registers.Cc & 0xFB);

                    if (_registers.X < val)
                        _registers.Cc = (byte)(_registers.Cc | 8);
                    else _registers.Cc = (byte)(_registers.Cc & 0xF7);

                    if (_registers.X != val)
                        _registers.Cc = (byte)(_registers.Cc | 16);
                    else _registers.Cc = (byte)(_registers.Cc & 0xEF);

                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x43) // CMPY Register
                {
                    byte r = _memory[_registers.Pc + 1];
                    ushort val = 0;

                    if (r == 1)
                        val = _registers.A;
                    if (r == 2)
                        val = _registers.B;
                    if (r == 4)
                        val = _registers.D;
                    if (r == 8)
                        val = _registers.X;
                    if (r == 16)
                        val = _registers.Y;

                    if (_registers.Y == val)
                        _registers.Cc = (byte)(_registers.Cc | 2);
                    else _registers.Cc = (byte)(_registers.Cc & 0xFD);

                    if (_registers.Y > val)
                        _registers.Cc = (byte)(_registers.Cc | 4);
                    else _registers.Cc = (byte)(_registers.Cc & 0xFB);

                    if (_registers.Y < val)
                        _registers.Cc = (byte)(_registers.Cc | 8);
                    else _registers.Cc = (byte)(_registers.Cc & 0xF7);

                    if (_registers.Y != val)
                        _registers.Cc = (byte)(_registers.Cc | 16);
                    else _registers.Cc = (byte)(_registers.Cc & 0xEF);

                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x44) // LSFT
                {
                    byte r = _memory[_registers.Pc + 1];

                    if (r == 1)
                    {
                        _registers.Cc = (_registers.A & 0x80) == 0x80
                                            ? (byte) (_registers.Cc | 1)
                                            : (byte) (_registers.Cc & 0xFE);
                        _registers.A = (byte) (_registers.A << 1);
                    }
                    if (r == 2)
                    {
                        _registers.Cc = (_registers.B & 0x80) == 0x80
                                           ? (byte)(_registers.Cc | 1)
                                           : (byte)(_registers.Cc & 0xFE);
                        _registers.B = (byte) (_registers.B << 1);
                    }
                    if (r == 4)
                    {
                        _registers.Cc = (_registers.D & 0x8000) == 0x8000
                                           ? (byte)(_registers.Cc | 1)
                                           : (byte)(_registers.Cc & 0xFE);
                        _registers.D = (ushort) (_registers.D << 1);
                    }
                    if (r == 8)
                    {
                        _registers.Cc = (_registers.X & 0x8000) == 0x8000
                                           ? (byte)(_registers.Cc | 1)
                                           : (byte)(_registers.Cc & 0xFE);
                        _registers.X = (ushort) (_registers.X << 1);
                    }
                    if (r == 16)
                    {
                        _registers.Cc = (_registers.Y & 0x8000) == 0x8000
                                           ? (byte)(_registers.Cc | 1)
                                           : (byte)(_registers.Cc & 0xFE);
                        _registers.Y = (ushort) (_registers.Y << 1);
                    }
                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x45) // RSFT
                {
                    byte r = _memory[_registers.Pc + 1];

                    if (r == 1)
                    {
                        _registers.Cc = (_registers.A & 0x01) == 0x01
                                            ? (byte)(_registers.Cc | 1)
                                            : (byte)(_registers.Cc & 0xFE);
                        _registers.A = (byte)(_registers.A >> 1);
                    }
                    if (r == 2)
                    {
                        _registers.Cc = (_registers.B & 0x01) == 0x01
                                           ? (byte)(_registers.Cc | 1)
                                           : (byte)(_registers.Cc & 0xFE);
                        _registers.B = (byte)(_registers.B >> 1);
                    }
                    if (r == 4)
                    {
                        _registers.Cc = (_registers.D & 0x01) == 0x01
                                           ? (byte)(_registers.Cc | 1)
                                           : (byte)(_registers.Cc & 0xFE);
                        _registers.D = (ushort)(_registers.D >> 1);
                    }
                    if (r == 8)
                    {
                        _registers.Cc = (_registers.X & 0x01) == 0x01
                                           ? (byte)(_registers.Cc | 1)
                                           : (byte)(_registers.Cc & 0xFE);
                        _registers.X = (ushort)(_registers.X >> 1);
                    }
                    if (r == 16)
                    {
                        _registers.Cc = (_registers.Y & 0x01) == 0x01
                                           ? (byte)(_registers.Cc | 1)
                                           : (byte)(_registers.Cc & 0xFE);
                        _registers.Y = (ushort)(_registers.Y >> 1);
                    }
                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x46) // JOS
                {
                    ushort addr = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);
                    var jump = (byte)(_registers.Cc & 1);

                    _registers.Pc = jump == 1 ? addr : (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x47) // JOC
                {
                    ushort addr = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);
                    var jump = (byte)(_registers.Cc & 1);

                    _registers.Pc = jump == 0 ? addr : (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x48) // MUL8
                {
                    byte sourceRegister = _memory[_registers.Pc + 1];
                    byte destinarionRegister = _memory[_registers.Pc + 2];
                    byte m = 0;

                    switch (destinarionRegister)
                    {
                        case 1:
                            m = _registers.A;
                            break;
                        case 2:
                            m = _registers.B;
                            break;
                    }

                    if (sourceRegister == 1)
                    {
                        _registers.A = (byte) (_registers.A * m);
                    }
                    if (sourceRegister == 2)
                    {
                        _registers.B = (byte) (_registers.B*m);
                    }
                    _registers.Pc = (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x49) // MUL16
                {
                    byte sourceRegister = _memory[_registers.Pc + 1];
                    byte destinarionRegister = _memory[_registers.Pc + 2];
                    ushort m = 0;

                    switch (destinarionRegister)
                    {
                        case 1:
                            m = _registers.A;
                            break;
                        case 2:
                            m = _registers.B;
                            break;
                        case 4:
                            m = _registers.D;
                            break;
                        case 8:
                            m = _registers.X;
                            break;
                        case 16:
                            m = _registers.Y;
                            break;
                    }

                    if (sourceRegister == 4)
                    {
                        _registers.D = (ushort)(_registers.D * m);
                    }
                    if (sourceRegister == 8)
                    {
                        _registers.X = (ushort)(_registers.X * m);
                    }
                    if (sourceRegister == 16)
                    {
                        _registers.Y = (ushort)(_registers.Y * m);
                    }
                    _registers.Pc = (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x4A) // ANDA Immediate
                {
                    byte val = _memory[_registers.Pc + 1];

                    _registers.A = (byte) (_registers.A & val);
                    _registers.Pc = (ushort) (_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x4B) // ANDB Immediate
                {
                    byte val = _memory[_registers.Pc + 1];

                    _registers.B = (byte)(_registers.B & val);
                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x4C) // ANDD Immediate
                {
                    ushort val = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);

                    _registers.D = (ushort)(_registers.D & val);
                    _registers.Pc = (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x4D) // ANDX Immediate
                {
                    ushort val = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);

                    _registers.X = (ushort)(_registers.X & val);
                    _registers.Pc = (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x4E) // ANDY Immediate
                {
                    ushort val = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);

                    _registers.Y = (ushort)(_registers.Y & val);
                    _registers.Pc = (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x4F) // ANDA Register
                {
                    byte r = _memory[_registers.Pc + 1];

                    if (r == 1)
                    {
                        _registers.A = (byte) (_registers.A & _registers.A);
                    }
                    if (r == 2)
                    {
                        _registers.A = (byte) (_registers.A & _registers.B);
                    }

                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x50) // ANDB Register
                {
                    byte r = _memory[_registers.Pc + 1];

                    if (r == 1)
                    {
                        _registers.B = (byte)(_registers.B & _registers.A);
                    }
                    if (r == 2)
                    {
                        _registers.B = (byte)(_registers.B & _registers.B);
                    }

                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x51) // ANDD Register
                {
                    byte r = _memory[_registers.Pc + 1];

                    if (r == 1)
                    {
                        _registers.D = (ushort)(_registers.D & _registers.A);
                    }
                    if (r == 2)
                    {
                        _registers.D = (ushort)(_registers.D & _registers.B);
                    }
                    if (r == 4)
                    {
                        _registers.D = (ushort)(_registers.D & _registers.D);
                    }
                    if (r == 8)
                    {
                        _registers.D = (ushort)(_registers.D & _registers.X);
                    }
                    if (r == 16)
                    {
                        _registers.D = (ushort)(_registers.D & _registers.Y);
                    }

                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x52) // ANDX Register
                {
                    byte r = _memory[_registers.Pc + 1];

                    if (r == 1)
                    {
                        _registers.X = (ushort)(_registers.X & _registers.A);
                    }
                    if (r == 2)
                    {
                        _registers.X = (ushort)(_registers.X & _registers.B);
                    }
                    if (r == 4)
                    {
                        _registers.X = (ushort)(_registers.X & _registers.D);
                    }
                    if (r == 8)
                    {
                        _registers.X = (ushort)(_registers.X & _registers.X);
                    }
                    if (r == 16)
                    {
                        _registers.X = (ushort)(_registers.X & _registers.Y);
                    }

                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x53) // ANDY Register
                {
                    byte r = _memory[_registers.Pc + 1];

                    if (r == 1)
                    {
                        _registers.Y = (ushort)(_registers.Y & _registers.A);
                    }
                    if (r == 2)
                    {
                        _registers.Y = (ushort)(_registers.Y & _registers.B);
                    }
                    if (r == 4)
                    {
                        _registers.Y = (ushort)(_registers.Y & _registers.D);
                    }
                    if (r == 8)
                    {
                        _registers.Y = (ushort)(_registers.Y & _registers.X);
                    }
                    if (r == 16)
                    {
                        _registers.Y = (ushort)(_registers.Y & _registers.Y);
                    }

                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x54) // ORA Immediate
                {
                    byte val = _memory[_registers.Pc + 1];

                    _registers.A = (byte)(_registers.A | val);
                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x55) // ORB Immediate
                {
                    byte val = _memory[_registers.Pc + 1];

                    _registers.B = (byte)(_registers.B | val);
                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x56) // ORD Immediate
                {
                    ushort val = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);

                    _registers.D = (ushort)(_registers.D | val);
                    _registers.Pc = (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x57) // ORX Immediate
                {
                    ushort val = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);

                    _registers.X = (ushort)(_registers.X | val);
                    _registers.Pc = (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x58) // ORY Immediate
                {
                    ushort val = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);

                    _registers.Y = (ushort)(_registers.Y | val);
                    _registers.Pc = (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x59) // ORA Register
                {
                    byte r = _memory[_registers.Pc + 1];

                    if (r == 1)
                    {
                        _registers.A = (byte)(_registers.A | _registers.A);
                    }
                    if (r == 2)
                    {
                        _registers.A = (byte)(_registers.A | _registers.B);
                    }

                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x5A) // ORB Register
                {
                    byte r = _memory[_registers.Pc + 1];

                    if (r == 1)
                    {
                        _registers.B = (byte)(_registers.B | _registers.A);
                    }
                    if (r == 2)
                    {
                        _registers.B = (byte)(_registers.B | _registers.B);
                    }

                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x5B) // ORD Register
                {
                    byte r = _memory[_registers.Pc + 1];

                    if (r == 1)
                    {
                        _registers.D = (ushort)(_registers.D | _registers.A);
                    }
                    if (r == 2)
                    {
                        _registers.D = (ushort)(_registers.D | _registers.B);
                    }
                    if (r == 4)
                    {
                        _registers.D = (ushort)(_registers.D | _registers.D);
                    }
                    if (r == 8)
                    {
                        _registers.D = (ushort)(_registers.D | _registers.X);
                    }
                    if (r == 16)
                    {
                        _registers.D = (ushort)(_registers.D | _registers.Y);
                    }

                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x5C) // ORX Register
                {
                    byte r = _memory[_registers.Pc + 1];

                    if (r == 1)
                    {
                        _registers.X = (ushort)(_registers.X | _registers.A);
                    }
                    if (r == 2)
                    {
                        _registers.X = (ushort)(_registers.X | _registers.B);
                    }
                    if (r == 4)
                    {
                        _registers.X = (ushort)(_registers.X | _registers.D);
                    }
                    if (r == 8)
                    {
                        _registers.X = (ushort)(_registers.X | _registers.X);
                    }
                    if (r == 16)
                    {
                        _registers.X = (ushort)(_registers.X | _registers.Y);
                    }

                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x5D) // ORY Register
                {
                    byte r = _memory[_registers.Pc + 1];

                    if (r == 1)
                    {
                        _registers.Y = (ushort)(_registers.Y | _registers.A);
                    }
                    if (r == 2)
                    {
                        _registers.Y = (ushort)(_registers.Y | _registers.B);
                    }
                    if (r == 4)
                    {
                        _registers.Y = (ushort)(_registers.Y | _registers.D);
                    }
                    if (r == 8)
                    {
                        _registers.Y = (ushort)(_registers.Y | _registers.X);
                    }
                    if (r == 16)
                    {
                        _registers.Y = (ushort)(_registers.Y | _registers.Y);
                    }

                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x5E) // DIV8
                {
                    byte sourceRegister = _memory[_registers.Pc + 1];
                    byte destinarionRegister = _memory[_registers.Pc + 2];
                    byte q;
                    byte r;

                    if (sourceRegister == 1)
                    {
                        if (destinarionRegister == 1)
                        {
                            q = (byte) (_registers.A/_registers.A);
                            r = (byte) (_registers.A%_registers.A);

                            _registers.A = q;
                            _registers.B = r;
                        }
                        if (destinarionRegister == 2)
                        {
                            q = (byte) (_registers.A/_registers.B);
                            r = (byte) (_registers.A%_registers.B);

                            _registers.A = q;
                            _registers.B = r;
                        }
                    }

                    if (sourceRegister == 2)
                    {
                        if (destinarionRegister == 1)
                        {
                            q = (byte)(_registers.B / _registers.A);
                            r = (byte)(_registers.B % _registers.A);

                            _registers.A = q;
                            _registers.B = r;
                        }
                        if (destinarionRegister == 2)
                        {
                            q = (byte)(_registers.B / _registers.B);
                            r = (byte)(_registers.B % _registers.B);

                            _registers.A = q;
                            _registers.B = r;
                        }
                    }

                    _registers.Pc = (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x5F) // DIV16
                {
                    byte sourceRegister = _memory[_registers.Pc + 1];
                    byte destinarionRegister = _memory[_registers.Pc + 2];
                    ushort q;
                    ushort r;

                    if (sourceRegister == 4)
                    {
                        if (destinarionRegister == 1)
                        {
                            q = (ushort) (_registers.D/_registers.A);
                            r = (ushort)(_registers.D % _registers.A);
                            _registers.X = q;
                            _registers.Y = r;
                        }
                        if (destinarionRegister == 2)
                        {
                            q = (ushort)(_registers.D / _registers.B);
                            r = (ushort)(_registers.D % _registers.B);
                            _registers.X = q;
                            _registers.Y = r;
                        }
                        if (destinarionRegister == 4)
                        {
                            q = (ushort)(_registers.D / _registers.D);
                            r = (ushort)(_registers.D % _registers.D);
                            _registers.X = q;
                            _registers.Y = r;
                        }
                        if (destinarionRegister == 8)
                        {
                            q = (ushort)(_registers.D / _registers.X);
                            r = (ushort)(_registers.D % _registers.X);
                            _registers.X = q;
                            _registers.Y = r;
                        }
                        if (destinarionRegister == 16)
                        {
                            q = (ushort)(_registers.D / _registers.Y);
                            r = (ushort)(_registers.D % _registers.Y);
                            _registers.X = q;
                            _registers.Y = r;
                        }
                    }
                    if (sourceRegister == 8)
                    {
                        if (destinarionRegister == 1)
                        {
                            q = (ushort)(_registers.X / _registers.A);
                            r = (ushort)(_registers.X % _registers.A);
                            _registers.X = q;
                            _registers.Y = r;
                        }
                        if (destinarionRegister == 2)
                        {
                            q = (ushort)(_registers.X / _registers.B);
                            r = (ushort)(_registers.X % _registers.B);
                            _registers.X = q;
                            _registers.Y = r;
                        }
                        if (destinarionRegister == 4)
                        {
                            q = (ushort)(_registers.X / _registers.D);
                            r = (ushort)(_registers.X % _registers.D);
                            _registers.X = q;
                            _registers.Y = r;
                        }
                        if (destinarionRegister == 8)
                        {
                            q = (ushort)(_registers.X / _registers.X);
                            r = (ushort)(_registers.X % _registers.X);
                            _registers.X = q;
                            _registers.Y = r;
                        }
                        if (destinarionRegister == 16)
                        {
                            q = (ushort)(_registers.X / _registers.Y);
                            r = (ushort)(_registers.X % _registers.Y);
                            _registers.X = q;
                            _registers.Y = r;
                        }
                    }
                    if (sourceRegister == 16)
                    {
                        if (destinarionRegister == 1)
                        {
                            q = (ushort)(_registers.Y / _registers.A);
                            r = (ushort)(_registers.Y % _registers.A);
                            _registers.X = q;
                            _registers.Y = r;
                        }
                        if (destinarionRegister == 2)
                        {
                            q = (ushort)(_registers.Y / _registers.B);
                            r = (ushort)(_registers.Y % _registers.B);
                            _registers.X = q;
                            _registers.Y = r;
                        }
                        if (destinarionRegister == 4)
                        {
                            q = (ushort)(_registers.Y / _registers.D);
                            r = (ushort)(_registers.Y % _registers.D);
                            _registers.X = q;
                            _registers.Y = r;
                        }
                        if (destinarionRegister == 8)
                        {
                            q = (ushort)(_registers.Y / _registers.X);
                            r = (ushort)(_registers.Y % _registers.X);
                            _registers.X = q;
                            _registers.Y = r;
                        }
                        if (destinarionRegister == 16)
                        {
                            q = (ushort)(_registers.Y / _registers.Y);
                            r = (ushort)(_registers.Y % _registers.Y);
                            _registers.X = q;
                            _registers.Y = r;
                        }
                    }

                    _registers.Pc = (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x60) // XORA Immediate
                {
                    byte val = _memory[_registers.Pc + 1];

                    _registers.A = (byte)(_registers.A ^ val);
                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x61) // XORB Immediate
                {
                    byte val = _memory[_registers.Pc + 1];

                    _registers.B = (byte)(_registers.B ^ val);
                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x62) // XORD Immediate
                {
                    ushort val = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);

                    _registers.D = (ushort)(_registers.D ^ val);
                    _registers.Pc = (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x63) // XORX Immediate
                {
                    ushort val = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);

                    _registers.X = (ushort)(_registers.X ^ val);
                    _registers.Pc = (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x64) // XORY Immediate
                {
                    ushort val = BitConverter.ToUInt16(new[] { _memory[_registers.Pc + 1], _memory[_registers.Pc + 2] }, 0);

                    _registers.Y = (ushort)(_registers.Y ^ val);
                    _registers.Pc = (ushort)(_registers.Pc + 3);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x65) // XORA Register
                {
                    byte r = _memory[_registers.Pc + 1];

                    if (r == 1)
                    {
                        _registers.A = (byte)(_registers.A ^ _registers.A);
                    }
                    if (r == 2)
                    {
                        _registers.A = (byte)(_registers.A ^ _registers.B);
                    }

                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x66) // XORB Register
                {
                    byte r = _memory[_registers.Pc + 1];

                    if (r == 1)
                    {
                        _registers.B = (byte)(_registers.B ^ _registers.A);
                    }
                    if (r == 2)
                    {
                        _registers.B = (byte)(_registers.B ^ _registers.B);
                    }

                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x67) // XORD Register
                {
                    byte r = _memory[_registers.Pc + 1];

                    if (r == 1)
                    {
                        _registers.D = (ushort)(_registers.D ^ _registers.A);
                    }
                    if (r == 2)
                    {
                        _registers.D = (ushort)(_registers.D ^ _registers.B);
                    }
                    if (r == 4)
                    {
                        _registers.D = (ushort)(_registers.D ^ _registers.D);
                    }
                    if (r == 8)
                    {
                        _registers.D = (ushort)(_registers.D ^ _registers.X);
                    }
                    if (r == 16)
                    {
                        _registers.D = (ushort)(_registers.D ^ _registers.Y);
                    }

                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x68) // XORX Register
                {
                    byte r = _memory[_registers.Pc + 1];

                    if (r == 1)
                    {
                        _registers.X = (ushort)(_registers.X ^ _registers.A);
                    }
                    if (r == 2)
                    {
                        _registers.X = (ushort)(_registers.X ^ _registers.B);
                    }
                    if (r == 4)
                    {
                        _registers.X = (ushort)(_registers.X ^ _registers.D);
                    }
                    if (r == 8)
                    {
                        _registers.X = (ushort)(_registers.X ^ _registers.X);
                    }
                    if (r == 16)
                    {
                        _registers.X = (ushort)(_registers.X ^ _registers.Y);
                    }

                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x69) // XORY Register
                {
                    byte r = _memory[_registers.Pc + 1];

                    if (r == 1)
                    {
                        _registers.Y = (ushort)(_registers.Y ^ _registers.A);
                    }
                    if (r == 2)
                    {
                        _registers.Y = (ushort)(_registers.Y ^ _registers.B);
                    }
                    if (r == 4)
                    {
                        _registers.Y = (ushort)(_registers.Y ^ _registers.D);
                    }
                    if (r == 8)
                    {
                        _registers.Y = (ushort)(_registers.Y ^ _registers.X);
                    }
                    if (r == 16)
                    {
                        _registers.Y = (ushort)(_registers.Y ^ _registers.Y);
                    }

                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x6A) // RND Register
                {
                    byte r = _memory[_registers.Pc + 1];
                    var rnd = new Random();

                    if (r == 1)
                    {
                        _registers.A = (byte) rnd.Next(0, _registers.A);
                    }
                    if (r == 2)
                    {
                        _registers.B = (byte)rnd.Next(0, _registers.B);
                    }
                    if (r == 4)
                    {
                        _registers.D = (ushort)rnd.Next(0, _registers.D);
                    }
                    if (r == 8)
                    {
                        _registers.X = (ushort)rnd.Next(0, _registers.X);
                    }
                    if (r == 16)
                    {
                        _registers.Y = (ushort)rnd.Next(0, _registers.Y);
                    }

                    _registers.Pc = (ushort)(_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x6B) // STCLR
                {
                    b33Screen1.GraphicsList = "COLOR " + _registers.A + "," + _registers.B + "," + _registers.Y;
                    _registers.Pc++;
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x6C) // MVTO
                {
                    b33Screen1.GraphicsList = "MOVETO " + _registers.X + "," + _registers.Y;
                    _registers.Pc++;
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x6D) // LNTO
                {
                    b33Screen1.GraphicsList = "LINETO " + _registers.X + "," + _registers.Y;
                    _registers.Pc++;
                    ThreadForceScreenRefresh();
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x6E) // NOP
                {
                    _registers.Pc++;
                    UpdateCpuInfoLabel();
                }
                if (opcode == 0x6F) // NEG
                {
                    byte r = _memory[_registers.Pc + 1];

                    if (r == 1)
                    {
                        _registers.A = (byte) (~_registers.A);
                        _registers.A = (byte) (_registers.A + 1);
                    }
                    if (r == 2)
                    {
                        _registers.B = (byte) (~_registers.B);
                        _registers.B = (byte) (_registers.B + 1);
                    }
                    if (r == 4)
                    {
                        _registers.D = (ushort) (~_registers.D);
                        _registers.D = (ushort) (_registers.D + 1);
                    }
                    if (r == 8)
                    {
                        _registers.X = (ushort) (~_registers.X);
                        _registers.X = (ushort) (_registers.X + 1);
                    }
                    if (r == 16)
                    {
                        _registers.Y = (ushort) (~_registers.Y);
                        _registers.Y = (ushort) (_registers.Y + 1);
                    }

                    _registers.Pc = (ushort) (_registers.Pc + 2);
                    UpdateCpuInfoLabel();
                }
            }
        }
    }
}
