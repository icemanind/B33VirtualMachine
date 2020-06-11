namespace B33Cpu.Hardware
{
    partial class B33GfxScreen
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
            // B33GfxScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.DoubleBuffered = true;
            this.Name = "B33GfxScreen";
            this.Size = new System.Drawing.Size(255, 200);
            this.SizeChanged += new System.EventHandler(this.B33GfxScreen_SizeChanged);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.B33GfxScreen_Paint);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
