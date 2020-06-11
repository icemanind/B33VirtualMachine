using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace B33VirtualMachineAssembler
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
