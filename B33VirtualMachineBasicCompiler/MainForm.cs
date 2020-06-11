using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace B33VirtualMachineBasicCompiler
{
    public partial class MainForm : Form
    {
        private string _initcode;
        private readonly Dictionary<string, string> _variables;

        public MainForm()
        {
            InitializeComponent();

            _initcode = "";
            _variables = new Dictionary<string, string>();
            Width = 1281;
        }

        private string GetVariablesInitializationCode()
        {
            return _variables.Aggregate("", (current, kvp) => current + kvp.Value);
        }

        private void SetupInitializationCode()
        {
            _initcode = "curX rmb 1" + Environment.NewLine;
            _initcode = _initcode + "curY rmb 1" + Environment.NewLine;
            _initcode = _initcode + "valStore rmb 2" + Environment.NewLine;
            _initcode = _initcode + "TheBasicStrBuf rmb 1200" + Environment.NewLine;
            _initcode = _initcode + "InputBuffer rmb 250" + Environment.NewLine;
            _initcode = _initcode + "revbuffer rmb 512" + Environment.NewLine;
            _initcode = _initcode + "tmpbuffer rmb 512" + Environment.NewLine;
            _initcode = _initcode + "bgcolor rmb 1" + Environment.NewLine;
            _initcode = _initcode + "fgcolor rmb 1" + Environment.NewLine;
            _initcode = _initcode + "colorattr rmb 1" + Environment.NewLine;
            _initcode = _initcode + "compbuf1 rmb 255" + Environment.NewLine;
            _initcode = _initcode + "compbuf2 rmb 255" + Environment.NewLine;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            txtBasicProgram.Text = @"10 print ucase$(""teSt"")
20 x$=""Hello World!""
30 print ucase$(x$) + "" ""
40 d$=ucase$(x$)
50 e$=ucase$(x$)+"" Alan!""
60 print d$+"" ""
70 print e$+"" ""
80 print ""     ""
90 print ""start --> "" + d$ +"" ""+e$+"" ""+ucase$(e$)
'100 print ""    ""
'110 print ucase$(x$)+"" ""+ucase$(x$)

";
            txtBasicProgram.Text = "";

            //txtBasicProgram.Text = txtBasicProgram.Text + "5 a$=\"Hello\"" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "7 b$=\"World\"" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "10 print mid$(a$,1,1)+mid$(b$,1,1)" + Environment.NewLine;

            //txtBasicProgram.Text = txtBasicProgram.Text + "4 locate 1,4" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "5 print mid$(\"blah\", 1, 1)" + Environment.NewLine;

            //txtBasicProgram.Text = txtBasicProgram.Text + "10 locate 1,0" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "20 a=7" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "25 b=3" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "30 if a=7 AND b=3 then print \"a equals 7 AND b equals 3!\"" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "40 if a<>7 then print \"a does not equal 7!\"" + Environment.NewLine;

            //txtBasicProgram.Text = txtBasicProgram.Text + "10 a$=\"Testing\"" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "20 if a$=\"Testing\" then goto 40" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "30 print \"Not equal!\"" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "35 end" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "40 print \"Equal!\"" + Environment.NewLine;

            //txtBasicProgram.Text = txtBasicProgram.Text + "5 a$=\"hello\"" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "10 y=5" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "20 z=1+2*3" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "30 x = 2*y + (4*z) / 2 - len (a$)" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "40 locate 1,1" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "50 printnum y" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "60 locate 2,1" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "70 printnum z" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "80 locate 3,1" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "90 printnum x" + Environment.NewLine;

            //txtBasicProgram.Text = txtBasicProgram.Text + "10 x=10" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "20 while x>5" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "30 locate x,2" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "35 print\"test\"" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "37 x=x-1" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "40 loop" + Environment.NewLine;

            //txtBasicProgram.Text = txtBasicProgram.Text + "10 dim x(10)" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "20 x(1) = 15" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "30 x(2) = 44" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "40 x(3) = 99" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "50 printnum x(1)" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "60 print \" \"" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "70 printnum x(2)" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "80 print \" \"" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "90 printnum x(3)" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "100 print \" \"" + Environment.NewLine;

//            txtBasicProgram.Text = txtBasicProgram.Text + @"4 z=&h18C1
//5 cls
//6 locate 9,1
//7 call 200
//8 locate 1,1
//10 dim x(10)
//20 x(1) = 15
//30 x(2) = 44
//40 x(3) = 99
//50 printnum x(1)
//60 print "" ""
//70 printnum x(2)
//80 print "" ""
//90 printnum x(3)
//100 print "" ""
//101 locate 10,1
//102 call 200
//110 end
//200 printnum peek(z)
//210 print"" ""
//220 printnum peek(z+1)
//230 print "" ""
//240 printnum peek(z+2)
//250 print "" ""
//260 printnum peek(z+3)
//270 return
//";
            //txtBasicProgram.Text = txtBasicProgram.Text + "10 cls" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "20 dim a(2)" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "30 a(1)=4" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "40 a(2)=7" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "50 printnum a(1)" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "60 print \"  \"" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "70 printnum a(2)" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "80 print \"  \"" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "90 printnum a(1)+a(2)" + Environment.NewLine;

            //txtBasicProgram.Text = txtBasicProgram.Text + "10 cls" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "20 dim a$(5)" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "30 a$(1) = \"dog\"" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "40 a$(2) = \"kitty\"" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "50 a$(3) = \"joker\"" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "60 print a$(1) + \" \" + a$(2) + \" \" + a$(3)" + Environment.NewLine;

            //txtBasicProgram.Text = txtBasicProgram.Text + "10 cls" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "15 locate 1,1" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "20 a$=input$()" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "25 locate 2,1" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "30 print \"You said: \" + a$" + Environment.NewLine;

            //txtBasicProgram.Text = txtBasicProgram.Text + "5 cls:locate 1,1" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "10 a$=\"15\"" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "20 printnum val(a$)" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "30 locate 3,1" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "40 print \"Type a number -->\":b$=input$()" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "50 z = val(b$)+99" + Environment.NewLine; 
            //txtBasicProgram.Text = txtBasicProgram.Text + "60 locate 5,1" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "70 print \"Your number plus 99 equals \"" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "80 printnum z" + Environment.NewLine; 

            //txtBasicProgram.Text = txtBasicProgram.Text + "101 f$=\"alan\"" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "102 f$=ucase$(mid$(f$,1,1)) + lcase$(mid$(f$,2,len(f$)-1))" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "113 print f$" + Environment.NewLine;

            //txtBasicProgram.Text = txtBasicProgram.Text + "10 cls:a$=\"alan\"" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "20 a$=\"Bryan, \" + a$" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "30 print a$" + Environment.NewLine;

            //txtBasicProgram.Text = txtBasicProgram.Text + "10 a$ = \"Hello\"" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "20 b$ = \"World!\"" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "30 print mid$(mid$(a$,2,len(a$)-1), 2, len(mid$(a$,2,len(a$)-2)))" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "30 print ucase$(mid$(a$,1,2))+mid$(a$,3,3)" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "30 print mid$(a$,1,len(a$)-1)+chr$(65)" + Environment.NewLine;

            //txtBasicProgram.Text = txtBasicProgram.Text + "10 cls" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "20 f$=\"alan\"" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "30 if f$=\"zoo\" then print \"f$ does equal \"+chr$(34)+\"zoo\"+chr$(34)" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "40 if f$<>\"\" then print \"f$ does NOT equal \"+chr$(34)+chr$(34)" + Environment.NewLine;

            //txtBasicProgram.Text = txtBasicProgram.Text + "10 dim a$(5)" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "20 a$(1)=\"Hello\"" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "30 a$(2)=\", World\"" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "40 print a$(1) + a$(2)" + Environment.NewLine;

            //txtBasicProgram.Text = txtBasicProgram.Text + "10 a$=\"   spaces!         \"" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "20 print \"         Without trim: \" + chr$(34)+a$+chr$(34)" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "30 locate 1,0" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "40 print \"           With LTRIM: \" + chr$(34)+ltrim$(a$)+chr$(34)" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "50 locate 2,0" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "60 print \"           With RTRIM: \" + chr$(34)+rtrim$(a$)+chr$(34)" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "70 locate 3,0" + Environment.NewLine;
            //txtBasicProgram.Text = txtBasicProgram.Text + "80 print \"  With LTRIM(RTRIM()): \" + chr$(34)+ltrim$(rtrim$(a$))+chr$(34)" + Environment.NewLine;

            txtBasicProgram.Text = txtBasicProgram.Text + "10 a$=\"Hello World!\"" + Environment.NewLine;
            txtBasicProgram.Text = txtBasicProgram.Text + "20 printnum instr(1,a$,\"orl\")" + Environment.NewLine;
        }

        private void btnTranslate_Click(object sender, EventArgs e)
        {
            txtAssembly.Text = "";
            _variables.Clear();
            var translator = new Translator(_variables);
            
            string[] lines = txtBasicProgram.Text.Replace('\r', '\n').Replace("\n\n", "\n").Split('\n');
            string assembly = "TheBasicStart call TheBasicInit" + Environment.NewLine;

            _initcode = "";
            SetupInitializationCode();
            
            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line) || string.IsNullOrEmpty(line.Trim()))
                {
                    continue;
                }

                AssemblyRetVal retval = translator.TranslateLine(line, ref _initcode, true);

                _initcode = _initcode + retval.InitializationCode;
                assembly = assembly + retval.AssemblyCode;
            }

            assembly = assembly + " jmp TheBasicEnd" + Environment.NewLine;

            txtAssembly.Text = _initcode + GetVariablesInitializationCode() + assembly +
                               translator.Functions.Aggregate("", (current, ss) => current + Routines.GetRoutine(ss)) +
                               Routines.GetRoutine(Functions.FuncInit) +
                               @"TheBasicEnd end TheBasicStart";
        }

        private void btnLoadFromSample_Click(object sender, EventArgs e)
        {
            var samplesDialog = new SamplesBrowser();
            DialogResult dr = samplesDialog.ShowDialog(this);
            if (dr == DialogResult.OK)
            {
                txtBasicProgram.Text = samplesDialog.Program;
            }
        }

        private void btnCompile_Click(object sender, EventArgs e)
        {
            var form = new AssembleForm();
            DialogResult dr = form.ShowDialog();

            if (dr == DialogResult.OK)
            {
                btnTranslate_Click(sender, e);

                var assembler = new B33Assembler.Assembler(txtAssembly.Text)
                {
                    IncludeDebugInformation = form.ExposeDebugInfo,
                    RequiresDualMonitors = form.DualMonitors,
                    Origin = form.Origin
                };

                B33Assembler.Assembler.B33Program prog = assembler.Assemble(B33Assembler.OutputTypes.B33Executable);
                if ((prog == null) || (!assembler.Successful))
                {
                    Error(assembler.ErrorMessage);
                    return;
                }
                using (var bw = new BinaryWriter(File.Open(form.OutputFileName, FileMode.Create)))
                {
                    bw.Write(prog.ProgramBytes);
                    bw.Close();
                }

                if (form.ShowLabelResolves)
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
        }

        private void Error(string errorMessage)
        {
            const string errorCaption = "Error";

            MessageBox.Show(errorMessage, errorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
