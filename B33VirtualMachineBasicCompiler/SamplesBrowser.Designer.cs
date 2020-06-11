namespace B33VirtualMachineBasicCompiler
{
    partial class SamplesBrowser
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
            this.gvSamples = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.gvSamples)).BeginInit();
            this.SuspendLayout();
            // 
            // gvSamples
            // 
            this.gvSamples.AllowUserToAddRows = false;
            this.gvSamples.AllowUserToDeleteRows = false;
            this.gvSamples.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gvSamples.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.gvSamples.Location = new System.Drawing.Point(13, 43);
            this.gvSamples.MultiSelect = false;
            this.gvSamples.Name = "gvSamples";
            this.gvSamples.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gvSamples.ShowCellErrors = false;
            this.gvSamples.ShowCellToolTips = false;
            this.gvSamples.ShowRowErrors = false;
            this.gvSamples.Size = new System.Drawing.Size(410, 179);
            this.gvSamples.TabIndex = 6;
            this.gvSamples.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.gvSamples_CellDoubleClick);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(13, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(410, 32);
            this.label1.TabIndex = 5;
            this.label1.Text = "Load a Sample by simply double clicking it";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // SamplesBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(437, 231);
            this.Controls.Add(this.gvSamples);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SamplesBrowser";
            this.ShowInTaskbar = false;
            this.Text = "Samples Browser";
            this.Load += new System.EventHandler(this.SamplesBrowser_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gvSamples)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView gvSamples;
        private System.Windows.Forms.Label label1;
    }
}