using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace B33VirtualMachine
{
    public partial class MemoryViewerForm : Form
    {
        private B33Cpu.B33Cpu _cpu;

        private delegate void UpdateMemoryCallBack();

        public B33Cpu.B33Cpu Cpu { get { return _cpu; } set { _cpu = value; Cpu.B33PostOpcodeExecute += Cpu_B33PostOpcodeExecute; } }

        public delegate void ViewerClosedEventArgs(object sender, EventArgs e);

        public event ViewerClosedEventArgs ViewerClosed;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool LockWindowUpdate(IntPtr hWndLock);

        public MemoryViewerForm()
        {
            InitializeComponent();

            txtMemory.MouseWheel += TxtMemoryMouseWheel;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
                return cp;
            }
        } 

        private void MemoryViewerForm_Load(object sender, EventArgs e)
        {
            UpdateMemoryView();
        }

        public void Cpu_B33PostOpcodeExecute(B33Cpu.B33Cpu sender, bool isStore, ushort storeAddress)
        {
            if (!chkLiveUpdate.Checked)
                return;
            if (!isStore)
                return;
            if (storeAddress < 16 * sbMemory.Value || storeAddress > 16 * sbMemory.Value + 16*16+1)
                return;
            if (Application.OpenForms["MemoryViewerForm"] == null)
                return;
            UpdateMemoryView();
        }

        private void TxtMemoryMouseWheel(object sender, MouseEventArgs e)
        {
            if (Application.OpenForms["MemoryViewerForm"] == null)
                return;
            const int scrollChange = 16;
            int oldValue = sbMemory.Value;

            if (Math.Sign(e.Delta) == -1)
            {
                if (sbMemory.Value <= sbMemory.Maximum - scrollChange)
                    sbMemory.Value += scrollChange;
                else sbMemory.Value = sbMemory.Maximum - 1;
            } else if (Math.Sign(e.Delta) == 1)
            {
                if (sbMemory.Value >= scrollChange)
                    sbMemory.Value -= scrollChange;
                else sbMemory.Value = 0;
            }

            if (oldValue != sbMemory.Value)
                txtMemory.Text = GetData(sbMemory.Value);
        }

        private string GetData(int scrollPos)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder bytes = new StringBuilder();

            for (int x = 16 * scrollPos; x < 16 * scrollPos + 16*16+1; x++)
            {
                if (x > 0xFFFF)
                    continue;
                byte b = Cpu.Peek((ushort)x, true);
                if (x % 16 == 0)
                {
                    if (x != 16 * scrollPos)
                    {
                        sb.Append(" " + bytes);
                        sb.AppendLine();
                    }
                    sb.Append(x.ToString("X4") + " : ");
                    bytes = new StringBuilder();
                }
                bytes.Append(GetPrintableCharacter(b));
                sb.Append(b.ToString("X2") + " ");

                if (x == 0xFFFF)
                    sb.Append(" " + bytes);
            }

            return sb.ToString();
        }

        public void UpdateMemoryView()
        {
            if (txtMemory.InvokeRequired || sbMemory.InvokeRequired)
            {
                var z = new UpdateMemoryCallBack(UpdateMemoryText);

                Invoke(z);
            }
            else
            {
                UpdateMemoryText();
            }
        }

        public void UpdateMemoryText()
        {
            try
            {
                lock (txtMemory)
                {
                    LockWindowUpdate(txtMemory.Handle);
                    txtMemory.Text = GetData(sbMemory.Value);
                }
            }
            finally
            {
                LockWindowUpdate(IntPtr.Zero);
            }
            
        }

        private char GetPrintableCharacter(byte b)
        {
            if (b >= 32 && b <= 126)
                return (char)b;

            return '.';
        }

        private void MemoryViewerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();

                if (ViewerClosed != null)
                    ViewerClosed(this, new EventArgs());
            }
        }

        private void sbMemory_Scroll(object sender, ScrollEventArgs e)
        {
            if (Application.OpenForms["MemoryViewerForm"] == null)
                return;
            UpdateMemoryView();
        }

        private void txtScrollToAddress_TextChanged(object sender, EventArgs e)
        {
            string item = txtScrollToAddress.Text;
            int n;

            if (!int.TryParse(item, System.Globalization.NumberStyles.HexNumber,
                    System.Globalization.NumberFormatInfo.CurrentInfo, out n) &&
                item != String.Empty)
            {
                txtScrollToAddress.Text = item.Remove(item.Length - 1, 1);
                txtScrollToAddress.SelectionStart = txtScrollToAddress.Text.Length;
            }
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            int n;

            if (String.IsNullOrEmpty(txtScrollToAddress.Text) || String.IsNullOrEmpty(txtScrollToAddress.Text.Trim()))
                return;

            int.TryParse(txtScrollToAddress.Text, System.Globalization.NumberStyles.HexNumber,
                System.Globalization.NumberFormatInfo.CurrentInfo, out n);

            while (n > 0)
            {
                if (n % 16 == 0)
                    break;
                n--;
            }

            sbMemory.Value = n / 16;

            UpdateMemoryView();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            UpdateMemoryView();
        }

        private void chkLiveUpdate_CheckedChanged(object sender, EventArgs e)
        {
            btnRefresh.Enabled = !chkLiveUpdate.Checked;
        }
    }
}
