using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using B33Cpu;
using B33Cpu.Hardware;

namespace B33VirtualMachine
{
    public partial class MainForm : Form
    {
        private const int SingleMonitorHeight = 427;
        private const int DualMonitorHeight = 747;

        private readonly B33Cpu.B33Cpu _cpu;

        private delegate void SetCpuInfoTextCallBack(string text);
        private delegate void SetDebugInfoTextCallBack();
        private delegate void ShowStopMessageCallBack();
        private delegate void ShowBreakPointHitCallBack();

        private bool _hasDebugInfo;

        private readonly MemoryViewerForm _memoryViewer;

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Handle Arrow Keys
            // 0x01 = left arrow key
            // 0x02 = up arrow key
            // 0x03 = right arrow key
            // 0x04 = down arrow key

            if (keyData == Keys.Left)
            {
                OnKeyPress(new KeyPressEventArgs((char)1));
            }
            if (keyData == Keys.Up)
            {
                OnKeyPress(new KeyPressEventArgs((char)2));
            }
            if (keyData == Keys.Right)
            {
                OnKeyPress(new KeyPressEventArgs((char)3));
            }
            if (keyData == Keys.Down)
            {
                OnKeyPress(new KeyPressEventArgs((char)4));
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        public MainForm()
        {
            InitializeComponent();

            Size = new Size(Width, SingleMonitorHeight);
            _cpu = new B33Cpu.B33Cpu();
            _cpu.RegistersChanged += RegistersChangedHandler;
            _cpu.B33PreOpcodeExecute += PreOpcodeExecuteHandler;
            _cpu.B33Stopped += StoppedHandler;
            _cpu.B33BreakPointHit += BreakPointHandler;

            _cpu.Hardware.Add(b33Screen1);
            _cpu.Hardware.Add(b33Screen2);
            _cpu.Hardware.Add(new B33Cpu.Hardware.B33Keyboard(this));
            _cpu.Hardware.Add(new B33Cpu.Hardware.B33Sound());

            UpdateCpuInfoLabel();
            updateRegisterStatusInRealTimeToolStripMenuItem.Checked = false;

            _hasDebugInfo = false;

            _memoryViewer = new MemoryViewerForm {Cpu = _cpu};

            _memoryViewer.ViewerClosed += MemoryViewerClosed;
        }

        private void MemoryViewerClosed(object sender, EventArgs e)
        {
            showMemoryViewerToolStripMenuItem.Checked = false;
        }

        private void BreakPointHandler(B33Cpu.B33Cpu sender, bool isStore, ushort storeAddress)
        {
            if (InvokeRequired)
            {
                var z = new ShowBreakPointHitCallBack(ShowBreakPointMessage);
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
                ShowBreakPointMessage();
            }
            startToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem.Enabled = true;
            pauseToolStripMenuItem.Enabled = true;

            tsbPlay.Enabled = false;
            tsbStop.Enabled = true;
            tsbPause.Enabled = true;
        }

        private void StoppedHandler(B33Cpu.B33Cpu sender, bool isStore, ushort storeAddress)
        {
            if (InvokeRequired)
            {
                var z = new ShowStopMessageCallBack(ShowStopMessage);
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
                ShowStopMessage();
            }
        }

        private void ShowBreakPointMessage()
        {
            UpdateCpuInfoLabel(true);
            UpdateDebugInfo();

            MessageBox.Show(this, @"Break Point Hit! At Address 0x"+_cpu.Registers.Pc.ToString("X4"), @"B33 Virtual Machine", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void ShowStopMessage()
        {
            UpdateCpuInfoLabel(true);
            UpdateDebugInfo();

            startToolStripMenuItem.Enabled = true;
            stopToolStripMenuItem.Enabled = false;
            pauseToolStripMenuItem.Enabled = false;

            tsbPlay.Enabled = true;
            tsbStop.Enabled = false;
            tsbPause.Enabled = false;

            foreach (B33Screen h in _cpu.Hardware.OfType<B33Screen>())
            {
                h.Poke(0xefa0, 0);
                h.Poke(0xe000, h.Peek(0xe000));
            }

            MessageBox.Show(this, @"Virtual Machine Stopped!", @"B33 Virtual Machine", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void PreOpcodeExecuteHandler(B33Cpu.B33Cpu sender, bool isStore, ushort storeAddress)
        {
            UpdateDebugInfo();
        }

        private void RegistersChangedHandler(B33Cpu.B33Cpu sender)
        {
            UpdateCpuInfoLabel();
        }
        
        private void MainFormLoad(object sender, EventArgs e)
        {
            startToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem.Enabled = false;
            pauseToolStripMenuItem.Enabled = false;

            tsbPlay.Enabled = false;
            tsbStop.Enabled = false;
            tsbPause.Enabled = false;

            if (showMemoryViewerToolStripMenuItem.Checked)
                _memoryViewer.Show(this);
        }

        private void SetDebugInfo()
        {
            lock (_cpu.Program.DebugData)
            {
                foreach (DebugData d in _cpu.Program.DebugData)
                {
                    txtSourceCode.Text = string.Format("{0}${1} ==>{2}{3}", txtSourceCode.Text, d.Address.ToString("X4"), d.SourceCodeLine,
                        Environment.NewLine);
                }
            }
        }

        private void UpdateDebugInfo()
        {
            if (!_hasDebugInfo)
                return;
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
        }

        private void DoCpuUpdate()
        {
            StringBuilder label = new StringBuilder(string.Format("Program Name: {0}\n", string.IsNullOrEmpty(_cpu.Program.FileName) ? "<none>" : Path.GetFileNameWithoutExtension(_cpu.Program.FileName)));

            label.Append(string.Format("A : ${0}  B : ${1}  D : ${2}  X : ${3}  Y : ${4}  PC : ${5}  CC : ${6}", _cpu.Registers.A.ToString("X2"),
                                          _cpu.Registers.B.ToString("X2"), _cpu.Registers.D.ToString("X4"), _cpu.Registers.X.ToString("X4"), _cpu.Registers.Y.ToString("X4"),
                                          _cpu.Registers.Pc.ToString("X4"), _cpu.Registers.Cc.ToString("X2")));

            if (lblCpuInfo.InvokeRequired)
            {
                var z = new SetCpuInfoTextCallBack(SetCpuInfoLabel);
                try
                {
                    Invoke(z, label.ToString());
                }
                catch
                {
                }
            }
            else
            {
                SetCpuInfoLabel(label.ToString());
            }

        }

        private void UpdateCpuInfoLabel(bool forceUpdate)
        {
            if (!forceUpdate)
            {
                if (!updateRegisterStatusInRealTimeToolStripMenuItem.Checked)
                    return;
            }

            //Thread thread = new Thread(DoCpuUpdate);
            //thread.Start();
            DoCpuUpdate();
        }

        private void UpdateCpuInfoLabel()
        {
            UpdateCpuInfoLabel(false);
        }

        private void SetCpuInfoLabel(string text)
        {
            if (lblCpuInfo.Text.Equals(text))
                return;
            ScrollDebugTo(_cpu.Registers.Pc);
            lblCpuInfo.Text = text;
        }

        private void OpenB33BinaryToolStripMenuItemClick(object sender, EventArgs e)
        {
            DialogResult dr = ofdOpenBinary.ShowDialog(this);

            if (dr == DialogResult.OK && _cpu.IsValidFile(ofdOpenBinary.FileName))
            {
                _cpu.LoadProgram(ofdOpenBinary.FileName);
                tsbPlay.Enabled = true;
                startToolStripMenuItem.Enabled = true;
                Size = _cpu.Program.DualMonitorRequired ? new Size(Width, DualMonitorHeight) : new Size(Width, SingleMonitorHeight);
                _hasDebugInfo = _cpu.Program.HasDebugInfo;
                UpdateCpuInfoLabel();
                UpdateDebugInfo();

                startToolStripMenuItem.Enabled = true;
                stopToolStripMenuItem.Enabled = false;
                pauseToolStripMenuItem.Enabled = false;

                tsbPlay.Enabled = true;
                tsbStop.Enabled = false;
                tsbPause.Enabled = false;
            } else if (!_cpu.IsValidFile(ofdOpenBinary.FileName) && dr == DialogResult.OK)
            {
                startToolStripMenuItem.Enabled = false;
                stopToolStripMenuItem.Enabled = false;
                pauseToolStripMenuItem.Enabled = false;

                tsbPlay.Enabled = false;
                tsbStop.Enabled = false;
                tsbPause.Enabled = false;
                MessageBox.Show(this, @"This is not a valid B33 program!", @"B33 Virtual Machine");
            }
        }

        private void TsbOpenClick(object sender, EventArgs e)
        {
            OpenB33BinaryToolStripMenuItemClick(sender, e);
        }

        private void StartToolStripMenuItemClick(object sender, EventArgs e)
        {
            _cpu.Start();

            startToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem.Enabled = true;
            pauseToolStripMenuItem.Enabled = true;

            tsbPlay.Enabled = false;
            tsbStop.Enabled = true;
            tsbPause.Enabled = true;
        }

        private void tsbPlay_Click(object sender, EventArgs e)
        {
            StartToolStripMenuItemClick(sender, e);
        }

        private void StopToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (_cpu.State == B33Cpu.B33Cpu.States.Paused)
                _cpu.Pause();
            _cpu.Stop();

            startToolStripMenuItem.Enabled = true;
            stopToolStripMenuItem.Enabled = false;
            pauseToolStripMenuItem.Enabled = false;

            tsbPlay.Enabled = true;
            tsbStop.Enabled = false;
            tsbPause.Enabled = false;
        }

        private void TsbStopClick(object sender, EventArgs e)
        {
            StopToolStripMenuItemClick(sender, e);
        }

        private void PauseToolStripMenuItemClick(object sender, EventArgs e)
        {
            _cpu.Pause();

            startToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem.Enabled = true;
            pauseToolStripMenuItem.Enabled = true;

            tsbPlay.Enabled = false;
            tsbStop.Enabled = true;
            tsbPause.Enabled = true;
        }

        private void TsbPauseClick(object sender, EventArgs e)
        {
            PauseToolStripMenuItemClick(sender, e);
        }

        private void MainFormFormClosing(object sender, FormClosingEventArgs e)
        {
            if (_cpu != null)
            {
                if (_cpu.State == B33Cpu.B33Cpu.States.Paused)
                    _cpu.Pause();
                
                _cpu.Stop();

                while (_cpu != null && _cpu.State != B33Cpu.B33Cpu.States.Stopped)
                {

                }
            }
        }

        private void ms1SecondToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _cpu.Speed = 1000;
            ClearSpeedItems();
            ms1SecondToolStripMenuItem.Checked = true;
        }

        private void ms34SecondToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _cpu.Speed = 750;
            ClearSpeedItems();
            ms34SecondToolStripMenuItem.Checked = true;
        }

        private void ms12SecondToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _cpu.Speed = 500;
            ClearSpeedItems();
            ms12SecondToolStripMenuItem.Checked = true;
        }

        private void ms14SecondToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _cpu.Speed = 250;
            ClearSpeedItems();
            ms14SecondToolStripMenuItem.Checked = true;
        }

        private void ms18SecondToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _cpu.Speed = 125;
            ClearSpeedItems();
            ms18SecondToolStripMenuItem.Checked = true;
        }

        private void ms116SecondToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _cpu.Speed = 64;
            ClearSpeedItems();
            ms116SecondToolStripMenuItem.Checked = true;
        }

        private void ms132SecondToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _cpu.Speed = 32;
            ClearSpeedItems();
            ms132SecondToolStripMenuItem.Checked = true;
        }

        private void ms164SecondToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _cpu.Speed = 16;
            ClearSpeedItems();
            ms164SecondToolStripMenuItem.Checked = true;
        }

        private void realTimeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _cpu.Speed = 0;
            ClearSpeedItems();
            realTimeToolStripMenuItem.Checked = true;
        }

        private void showMemoryViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showMemoryViewerToolStripMenuItem.Checked = !showMemoryViewerToolStripMenuItem.Checked;

            if (showMemoryViewerToolStripMenuItem.Checked)
            {
                _memoryViewer.Show(this);
            }
            else
            {
                _memoryViewer.Hide();
            }
        }

        private void ClearSpeedItems()
        {
            realTimeToolStripMenuItem.Checked = false;
            ms164SecondToolStripMenuItem.Checked = false;
            ms132SecondToolStripMenuItem.Checked = false;
            ms116SecondToolStripMenuItem.Checked = false;
            ms18SecondToolStripMenuItem.Checked = false;
            ms14SecondToolStripMenuItem.Checked = false;
            ms12SecondToolStripMenuItem.Checked = false;
            ms34SecondToolStripMenuItem.Checked = false;
            ms1SecondToolStripMenuItem.Checked = false;
        }
    }
}
