using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace B33VirtualMachineAssembler
{
    public partial class MainForm : Form
    {
       private bool _isDirty;

        public MainForm()
        {
            InitializeComponent();
            txtEditor.ShowLineNumbers = true;
            _isDirty = false;
        }

        private void MainFormLoad(object sender, EventArgs e)
        {

        }

        private void BtnSaveClick(object sender, EventArgs e)
        {
            DialogResult dr = saveFileDialog1.ShowDialog(this);
            if (dr == DialogResult.OK)
            {
                using (StreamWriter sw = File.CreateText(saveFileDialog1.FileName))
                {
                    sw.Write(txtEditor.Text);
                    sw.Close();
                }
            }
        }

        private void TxtEditorTextChanged(object sender, EventArgs e)
        {
            if (_isDirty)
                return;
            _isDirty = true;
        }

        private void MainFormFormClosing(object sender, FormClosingEventArgs e)
        {
            if (_isDirty)
            {
                DialogResult dr = MessageBox.Show(@"Changes have not been saved! Save now?", @"Warning",
                                               MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question,
                                               MessageBoxDefaultButton.Button1);
                if (dr == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                if (dr == DialogResult.Yes)
                {
                    DialogResult dr2 = saveFileDialog1.ShowDialog(this);
                    if (dr2 == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                    if (dr2 == DialogResult.OK)
                    {
                        using (StreamWriter sw = File.CreateText(saveFileDialog1.FileName))
                        {
                            sw.Write(txtEditor.Text);
                            sw.Close();
                        }
                    }
                }
            }
        }

        private void BtnLoadClick(object sender, EventArgs e)
        {
            DialogResult dr = openFileDialog1.ShowDialog(this);
            if (dr == DialogResult.OK)
            {
                using (StreamReader sr = File.OpenText(openFileDialog1.FileName))
                {
                    txtEditor.Text = sr.ReadToEnd();
                    sr.Close();
                }
            }
        }

        private void BtnLoadSampleClick(object sender, EventArgs e)
        {
            var samplesDialog = new SamplesBrowser();
            DialogResult dr = samplesDialog.ShowDialog(this);
            if (dr == DialogResult.OK)
            {
                txtEditor.Text = samplesDialog.Program;
            }
        }

        private void BtnAssembleClick(object sender, EventArgs e)
        {
            var af = new AssembleForm();
            
            DialogResult dr = af.ShowDialog(this);
            if (dr == DialogResult.Cancel)
            {
                return;
            }

            var assembler = new B33Assembler.Assembler(txtEditor.Text)
            {
                IncludeDebugInformation = af.ExposeDebugInfo,
                RequiresDualMonitors = af.DualMonitors,
                Origin = af.Origin
            };

            B33Assembler.Assembler.B33Program prog = assembler.Assemble(B33Assembler.OutputTypes.B33Executable);
            if ((prog == null) || (!assembler.Successful))
            {
                Error(assembler.ErrorMessage);
                return;
            }
            using (var bw = new BinaryWriter(File.Open(af.OutputFileName, FileMode.Create)))
            {
                bw.Write(prog.ProgramBytes);
                bw.Close();
            }

            if (af.ShowLabelResolves)
            {
                var labelResolves = new LabelResolves();
                string s = prog.LabelDictionary.OrderBy(z => z.Key).Aggregate("",
                                                     (current, kp) =>
                                                     current + kp.Key + " --> $" + kp.Value.ToString("X4") +
                                                     Environment.NewLine);
                labelResolves.Resolves = s;
                labelResolves.ShowDialog(this);
            }
        }

        private void Error(string errorMessage)
        {
            const string errorCaption = "Error";

            MessageBox.Show(errorMessage, errorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void btnAnalyzeCode_Click(object sender, EventArgs e)
        {
            var codeAnalyzer = new CodeAnalyzer {SourceCode = txtEditor.Text};
            codeAnalyzer.ShowDialog();
        }
    }
}
