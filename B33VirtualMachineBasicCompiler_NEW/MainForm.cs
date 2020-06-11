using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace B33VirtualMachineBasicCompiler
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            Width = 1281;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            txtBasicProgram.Focus();

            txtBasicProgram.Text = "";

            txtBasicProgram.Text = txtBasicProgram.Text + "10 cls" + Environment.NewLine;
            txtBasicProgram.Text = txtBasicProgram.Text + "20 print chr$(65)" + Environment.NewLine;
        }

        private void btnTranslate_Click(object sender, EventArgs e)
        {
            Translator translator = new Translator();
            
            translator.Translate(txtBasicProgram.Text);
            txtAssembly.Text = translator.InitCode + translator.Output;
        }
    }
}
