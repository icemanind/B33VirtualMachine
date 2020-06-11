namespace B33VirtualMachineBasicCompiler
{
    partial class LabelResolves
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
            this.txtLabelResolves = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // txtLabelResolves
            // 
            this.txtLabelResolves.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLabelResolves.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtLabelResolves.Location = new System.Drawing.Point(0, 0);
            this.txtLabelResolves.Multiline = true;
            this.txtLabelResolves.Name = "txtLabelResolves";
            this.txtLabelResolves.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLabelResolves.Size = new System.Drawing.Size(435, 301);
            this.txtLabelResolves.TabIndex = 0;
            // 
            // LabelResolves
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(435, 301);
            this.Controls.Add(this.txtLabelResolves);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LabelResolves";
            this.ShowInTaskbar = false;
            this.Text = "Label Resolves";
            this.Load += new System.EventHandler(this.LabelResolves_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtLabelResolves;
    }
}