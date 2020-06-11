namespace B33VirtualMachineAssembler
{
    partial class CodeAnalyzer
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
            this.txtEditor = new B33VirtualMachineAssembler.B33RichTextEditor();
            this.SuspendLayout();
            // 
            // txtEditor
            // 
            this.txtEditor.AcceptsTab = true;
            this.txtEditor.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtEditor.Location = new System.Drawing.Point(12, 12);
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
            this.txtEditor.Size = new System.Drawing.Size(636, 427);
            this.txtEditor.TabIndex = 4;
            this.txtEditor.Text = "";
            this.txtEditor.WordWrap = false;
            // 
            // CodeAnalyzer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(988, 498);
            this.Controls.Add(this.txtEditor);
            this.Name = "CodeAnalyzer";
            this.Text = "CodeAnalyzer";
            this.Load += new System.EventHandler(this.CodeAnalyzer_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private B33RichTextEditor txtEditor;
    }
}