namespace B33VirtualMachineAssembler
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnLoadSample = new System.Windows.Forms.Button();
            this.btnAssemble = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.btnAnalyzeCode = new System.Windows.Forms.Button();
            this.txtEditor = new B33VirtualMachineAssembler.B33RichTextEditor();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(4, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(832, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "The B33 Assembler will take a B33 assembly language file and convert it into a B3" +
    "3 binary.";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Verdana", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(195, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(171, 23);
            this.label2.TabIndex = 1;
            this.label2.Text = "B33 Assembler";
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(14, 52);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(822, 18);
            this.label3.TabIndex = 2;
            this.label3.Text = "To use this, simply load or type a program into the editor and then click Assembl" +
    "e to assemble it!";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(172, 507);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(155, 23);
            this.btnLoad.TabIndex = 4;
            this.btnLoad.Text = "Load from File...";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.BtnLoadClick);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(555, 507);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(155, 23);
            this.btnSave.TabIndex = 5;
            this.btnSave.Text = "Save assembly File...";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.BtnSaveClick);
            // 
            // btnLoadSample
            // 
            this.btnLoadSample.Location = new System.Drawing.Point(363, 507);
            this.btnLoadSample.Name = "btnLoadSample";
            this.btnLoadSample.Size = new System.Drawing.Size(155, 23);
            this.btnLoadSample.TabIndex = 7;
            this.btnLoadSample.Text = "Load from Sample...";
            this.btnLoadSample.UseVisualStyleBackColor = true;
            this.btnLoadSample.Click += new System.EventHandler(this.BtnLoadSampleClick);
            // 
            // btnAssemble
            // 
            this.btnAssemble.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAssemble.Location = new System.Drawing.Point(172, 535);
            this.btnAssemble.Name = "btnAssemble";
            this.btnAssemble.Size = new System.Drawing.Size(255, 23);
            this.btnAssemble.TabIndex = 8;
            this.btnAssemble.Text = "Assemble...";
            this.btnAssemble.UseVisualStyleBackColor = true;
            this.btnAssemble.Click += new System.EventHandler(this.BtnAssembleClick);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.DefaultExt = "asm";
            this.openFileDialog1.Filter = "Assembly Files|*.asm|All Files|*.*";
            this.openFileDialog1.Title = "Load B33 Assembly Files";
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "asm";
            this.saveFileDialog1.Filter = "Assembly Files|*.asm|All Files|*.*";
            this.saveFileDialog1.Title = "Save B33 Assembly File";
            // 
            // btnAnalyzeCode
            // 
            this.btnAnalyzeCode.Location = new System.Drawing.Point(452, 536);
            this.btnAnalyzeCode.Name = "btnAnalyzeCode";
            this.btnAnalyzeCode.Size = new System.Drawing.Size(258, 23);
            this.btnAnalyzeCode.TabIndex = 9;
            this.btnAnalyzeCode.Text = "Analyze Code...";
            this.btnAnalyzeCode.UseVisualStyleBackColor = true;
            this.btnAnalyzeCode.Click += new System.EventHandler(this.btnAnalyzeCode_Click);
            // 
            // txtEditor
            // 
            this.txtEditor.AcceptsTab = true;
            this.txtEditor.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtEditor.Location = new System.Drawing.Point(10, 75);
            this.txtEditor.Name = "txtEditor";
            this.txtEditor.NumberAlignment = System.Drawing.StringAlignment.Center;
            this.txtEditor.NumberBackground1 = System.Drawing.Color.White;
            this.txtEditor.NumberBackground2 = System.Drawing.SystemColors.Window;
            this.txtEditor.NumberBorder = System.Drawing.Color.Gainsboro;
            this.txtEditor.NumberBorderThickness = 2F;
            this.txtEditor.NumberColor = System.Drawing.Color.DarkGray;
            this.txtEditor.NumberFont = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtEditor.NumberLeadingZeroes = false;
            this.txtEditor.NumberLineCounting = B33VirtualMachineAssembler.B33RichTextEditor.LineCounting.Crlf;
            this.txtEditor.NumberPadding = 2;
            this.txtEditor.ShowLineNumbers = false;
            this.txtEditor.Size = new System.Drawing.Size(826, 427);
            this.txtEditor.TabIndex = 3;
            this.txtEditor.Text = "";
            this.txtEditor.WordWrap = false;
            this.txtEditor.TextChanged += new System.EventHandler(this.TxtEditorTextChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(848, 562);
            this.Controls.Add(this.btnAnalyzeCode);
            this.Controls.Add(this.btnAssemble);
            this.Controls.Add(this.btnLoadSample);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.txtEditor);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "MainForm";
            this.Text = "B33 Assembler";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainFormFormClosing);
            this.Load += new System.EventHandler(this.MainFormLoad);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private B33RichTextEditor txtEditor;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnLoadSample;
        private System.Windows.Forms.Button btnAssemble;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Button btnAnalyzeCode;
    }
}

