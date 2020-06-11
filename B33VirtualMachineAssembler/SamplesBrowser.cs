using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace B33VirtualMachineAssembler
{
    public partial class SamplesBrowser : Form
    {
        public string Program
        {
            get;
            private set;
        }

        public SamplesBrowser()
        {
            InitializeComponent();
        }

        private void SamplesBrowserLoad(object sender, EventArgs e)
        {
            var samples = Assembly.GetExecutingAssembly()
                         .GetManifestResourceNames()
                         .Where(z => z.StartsWith("B33VirtualMachineAssembler.Samples.")).ToList();
            var samplesList = new List<Sample>();

            foreach (string sample in samples)
            {
                var s = new Sample();
                string s2 = sample.Replace("B33VirtualMachineAssembler.Samples.", "");
                string prog;
                using (var sr = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(sample)))
                {
                    prog = sr.ReadToEnd();
                    sr.Close();
                }
                string[] s3 = s2.Split('-');
                s.SampleNumber = int.Parse(s3[0].Trim());
                s.SampleName = s3[1].Trim().Replace(".txt", "");
                s.SampleProgram = prog;

                samplesList.Add(s);
            }

            gvSamples.DataSource = samplesList.OrderBy(z => z.SampleNumber).ToList();

            gvSamples.Columns[0].HeaderText = "";
            gvSamples.Columns[0].Width = 30;

            gvSamples.Columns[1].HeaderText = string.Format("Sample");
            gvSamples.Columns[1].Width = 300;

            gvSamples.Columns[2].Visible = false;
        }

        private void GvSamplesCellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            var row = gvSamples.Rows[e.RowIndex];

            var sample = (Sample)row.DataBoundItem;
            Program = sample.SampleProgram;
            DialogResult = DialogResult.OK;
        }

        private class Sample
        {
            public int SampleNumber
            {
                get;
                set;
            }

            public string SampleName
            {
                get;
                set;
            }

            public string SampleProgram
            {
                get;
                set;
            }
        }
    }
}
