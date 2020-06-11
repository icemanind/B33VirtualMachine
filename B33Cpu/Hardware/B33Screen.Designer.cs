namespace B33Cpu.Hardware
{
    partial class B33Screen
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // B33Screen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "B33Screen";
            this.Size = new System.Drawing.Size(960, 431);
            this.Load += new System.EventHandler(this.B33Screen_Load);
            this.SizeChanged += new System.EventHandler(this.B33Screen_SizeChanged);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.B33Screen_MouseMove);
            this.ResumeLayout(false);

        }

        #endregion

    }
}
