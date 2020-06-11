using System;
using System.Windows.Forms;

namespace B33VirtualMachineBasicCompiler
{
    public partial class LabelResolves : Form
    {
        public string Resolves
        {
            get { return txtLabelResolves.Text; }
            set { txtLabelResolves.Text = value; }
        }

        public LabelResolves()
        {
            InitializeComponent();
        }

        private void LabelResolves_Load(object sender, EventArgs e)
        {

        }
    }
}
