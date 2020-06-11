namespace B33VirtualMachineBasicCompiler
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
            this.btnCompile = new System.Windows.Forms.Button();
            this.btnSaveFile = new System.Windows.Forms.Button();
            this.btnLoadFromFile = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnLoadFromSample = new System.Windows.Forms.Button();
            this.btnTranslate = new System.Windows.Forms.Button();
            this.txtBasicProgram = new B33VirtualMachineBasicCompiler.B33RichTextEditor();
            this.txtAssembly = new B33VirtualMachineBasicCompiler.B33RichTextEditor();
            this.SuspendLayout();
            // 
            // btnCompile
            // 
            this.btnCompile.Location = new System.Drawing.Point(253, 759);
            this.btnCompile.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnCompile.Name = "btnCompile";
            this.btnCompile.Size = new System.Drawing.Size(448, 35);
            this.btnCompile.TabIndex = 21;
            this.btnCompile.Text = "Compile to a B33 Binary";
            this.btnCompile.UseVisualStyleBackColor = true;
            // 
            // btnSaveFile
            // 
            this.btnSaveFile.Location = new System.Drawing.Point(481, 804);
            this.btnSaveFile.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnSaveFile.Name = "btnSaveFile";
            this.btnSaveFile.Size = new System.Drawing.Size(220, 35);
            this.btnSaveFile.TabIndex = 20;
            this.btnSaveFile.Text = "Save Basic Program to File";
            this.btnSaveFile.UseVisualStyleBackColor = true;
            // 
            // btnLoadFromFile
            // 
            this.btnLoadFromFile.Location = new System.Drawing.Point(253, 804);
            this.btnLoadFromFile.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnLoadFromFile.Name = "btnLoadFromFile";
            this.btnLoadFromFile.Size = new System.Drawing.Size(220, 35);
            this.btnLoadFromFile.TabIndex = 19;
            this.btnLoadFromFile.Text = "Load from File";
            this.btnLoadFromFile.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(967, 16);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(933, 35);
            this.label2.TabIndex = 18;
            this.label2.Text = "Assembly Language Output";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(29, 16);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(933, 35);
            this.label1.TabIndex = 17;
            this.label1.Text = "BASIC Program";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // btnLoadFromSample
            // 
            this.btnLoadFromSample.Location = new System.Drawing.Point(25, 804);
            this.btnLoadFromSample.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnLoadFromSample.Name = "btnLoadFromSample";
            this.btnLoadFromSample.Size = new System.Drawing.Size(220, 35);
            this.btnLoadFromSample.TabIndex = 16;
            this.btnLoadFromSample.Text = "Load from Sample";
            this.btnLoadFromSample.UseVisualStyleBackColor = true;
            // 
            // btnTranslate
            // 
            this.btnTranslate.Location = new System.Drawing.Point(25, 759);
            this.btnTranslate.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnTranslate.Name = "btnTranslate";
            this.btnTranslate.Size = new System.Drawing.Size(220, 35);
            this.btnTranslate.TabIndex = 15;
            this.btnTranslate.Text = "Translate to Assembly Language";
            this.btnTranslate.UseVisualStyleBackColor = true;
            this.btnTranslate.Click += new System.EventHandler(this.btnTranslate_Click);
            // 
            // txtBasicProgram
            // 
            this.txtBasicProgram.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBasicProgram.Location = new System.Drawing.Point(25, 67);
            this.txtBasicProgram.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtBasicProgram.Name = "txtBasicProgram";
            this.txtBasicProgram.NumberAlignment = System.Drawing.StringAlignment.Center;
            this.txtBasicProgram.NumberBackground1 = System.Drawing.SystemColors.ControlLight;
            this.txtBasicProgram.NumberBackground2 = System.Drawing.SystemColors.Window;
            this.txtBasicProgram.NumberBorder = System.Drawing.SystemColors.ControlDark;
            this.txtBasicProgram.NumberBorderThickness = 1F;
            this.txtBasicProgram.NumberColor = System.Drawing.Color.DarkGray;
            this.txtBasicProgram.NumberFont = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBasicProgram.NumberLeadingZeroes = false;
            this.txtBasicProgram.NumberLineCounting = B33VirtualMachineBasicCompiler.B33RichTextEditor.LineCounting.Crlf;
            this.txtBasicProgram.NumberPadding = 2;
            this.txtBasicProgram.ShowLineNumbers = true;
            this.txtBasicProgram.Size = new System.Drawing.Size(931, 682);
            this.txtBasicProgram.TabIndex = 13;
            this.txtBasicProgram.Text = "";
            // 
            // txtAssembly
            // 
            this.txtAssembly.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtAssembly.Location = new System.Drawing.Point(967, 67);
            this.txtAssembly.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtAssembly.Name = "txtAssembly";
            this.txtAssembly.NumberAlignment = System.Drawing.StringAlignment.Center;
            this.txtAssembly.NumberBackground1 = System.Drawing.SystemColors.ControlLight;
            this.txtAssembly.NumberBackground2 = System.Drawing.SystemColors.Window;
            this.txtAssembly.NumberBorder = System.Drawing.SystemColors.ControlDark;
            this.txtAssembly.NumberBorderThickness = 1F;
            this.txtAssembly.NumberColor = System.Drawing.Color.DarkGray;
            this.txtAssembly.NumberFont = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtAssembly.NumberLeadingZeroes = false;
            this.txtAssembly.NumberLineCounting = B33VirtualMachineBasicCompiler.B33RichTextEditor.LineCounting.Crlf;
            this.txtAssembly.NumberPadding = 2;
            this.txtAssembly.ShowLineNumbers = true;
            this.txtAssembly.Size = new System.Drawing.Size(931, 682);
            this.txtAssembly.TabIndex = 14;
            this.txtAssembly.Text = "";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1924, 855);
            this.Controls.Add(this.btnCompile);
            this.Controls.Add(this.btnSaveFile);
            this.Controls.Add(this.btnLoadFromFile);
            this.Controls.Add(this.txtBasicProgram);
            this.Controls.Add(this.txtAssembly);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnLoadFromSample);
            this.Controls.Add(this.btnTranslate);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnCompile;
        private System.Windows.Forms.Button btnSaveFile;
        private System.Windows.Forms.Button btnLoadFromFile;
        private B33RichTextEditor txtBasicProgram;
        private B33RichTextEditor txtAssembly;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnLoadFromSample;
        private System.Windows.Forms.Button btnTranslate;
    }
}

