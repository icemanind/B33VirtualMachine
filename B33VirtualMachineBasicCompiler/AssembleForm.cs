using System;
using System.Windows.Forms;

namespace B33VirtualMachineBasicCompiler
{
    public partial class AssembleForm : Form
    {
        public ushort Origin
        {
            get;
            private set;
        }

        public string OutputFileName
        {
            get;
            private set;
        }

        public bool ShowLabelResolves
        {
            get { return chkShowLabelResolve.Checked; }
        }

        public bool ExposeDebugInfo
        {
            get { return chkExposeDebugInfo.Checked; }
        }

        public bool DualMonitors
        {
            get { return chkDualMonitors.Checked; }
        }

        public AssembleForm()
        {
            InitializeComponent();
        }

        private void AssembleFormLoad(object sender, EventArgs e)
        {
            txtOrigin.Text = "4000";
        }

        private void BtnCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void BtnAssembleSourceClick(object sender, EventArgs e)
        {
            int num;
            try
            {
                num = Convert.ToInt32(txtOrigin.Text, 16);
            }
            catch
            {
                MessageBox.Show("Invalid Origin Value!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtOrigin.Focus();
                return;
            }
            if (num > 0xC000 || num < 0x4000)
            {
                MessageBox.Show("Origin Value Must be Between $4000 and $C000", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtOrigin.Focus();
                return;
            }

            Origin = (ushort)num;
            OutputFileName = txtOutputFilename.Text.Trim();
            if (string.IsNullOrEmpty(OutputFileName))
            {
                MessageBox.Show("File Name cannot be blank!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtOrigin.Focus();
                return;
            }
            DialogResult = DialogResult.OK;
        }

        private void BtnBrowseClick(object sender, EventArgs e)
        {
            DialogResult dr = saveFileDialog1.ShowDialog(this);
            if (dr == DialogResult.OK)
            {
                txtOutputFilename.Text = saveFileDialog1.FileName;
            }
        }
    }
}
