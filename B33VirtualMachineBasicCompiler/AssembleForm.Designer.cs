namespace B33VirtualMachineBasicCompiler
{
    partial class AssembleForm
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
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.label1 = new System.Windows.Forms.Label();
            this.txtOrigin = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtOutputFilename = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.btnAssembleSource = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.chkShowLabelResolve = new System.Windows.Forms.CheckBox();
            this.chkExposeDebugInfo = new System.Windows.Forms.CheckBox();
            this.chkDualMonitors = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "B33";
            this.saveFileDialog1.Filter = "B33 Files|*.B33";
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(16, 11);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(159, 29);
            this.label1.TabIndex = 1;
            this.label1.Text = "Origin (in Hex):";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtOrigin
            // 
            this.txtOrigin.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtOrigin.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtOrigin.Location = new System.Drawing.Point(180, 11);
            this.txtOrigin.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtOrigin.Name = "txtOrigin";
            this.txtOrigin.Size = new System.Drawing.Size(86, 28);
            this.txtOrigin.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(10, 46);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(165, 29);
            this.label2.TabIndex = 3;
            this.label2.Text = "Output Filename:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtOutputFilename
            // 
            this.txtOutputFilename.Location = new System.Drawing.Point(180, 46);
            this.txtOutputFilename.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtOutputFilename.Name = "txtOutputFilename";
            this.txtOutputFilename.Size = new System.Drawing.Size(268, 26);
            this.txtOutputFilename.TabIndex = 4;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(454, 45);
            this.btnBrowse.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(42, 35);
            this.btnBrowse.TabIndex = 5;
            this.btnBrowse.Text = "...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.BtnBrowseClick);
            // 
            // btnAssembleSource
            // 
            this.btnAssembleSource.Location = new System.Drawing.Point(18, 188);
            this.btnAssembleSource.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnAssembleSource.Name = "btnAssembleSource";
            this.btnAssembleSource.Size = new System.Drawing.Size(186, 35);
            this.btnAssembleSource.TabIndex = 6;
            this.btnAssembleSource.Text = "Compile Program!";
            this.btnAssembleSource.UseVisualStyleBackColor = true;
            this.btnAssembleSource.Click += new System.EventHandler(this.BtnAssembleSourceClick);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(310, 188);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(186, 35);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.BtnCancelClick);
            // 
            // chkShowLabelResolve
            // 
            this.chkShowLabelResolve.AutoSize = true;
            this.chkShowLabelResolve.Checked = true;
            this.chkShowLabelResolve.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowLabelResolve.Location = new System.Drawing.Point(75, 86);
            this.chkShowLabelResolve.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkShowLabelResolve.Name = "chkShowLabelResolve";
            this.chkShowLabelResolve.Size = new System.Drawing.Size(371, 24);
            this.chkShowLabelResolve.TabIndex = 8;
            this.chkShowLabelResolve.Text = "Show Label Resolve Addresses After Compiling";
            this.chkShowLabelResolve.UseVisualStyleBackColor = true;
            // 
            // chkExposeDebugInfo
            // 
            this.chkExposeDebugInfo.AutoSize = true;
            this.chkExposeDebugInfo.Location = new System.Drawing.Point(75, 117);
            this.chkExposeDebugInfo.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkExposeDebugInfo.Name = "chkExposeDebugInfo";
            this.chkExposeDebugInfo.Size = new System.Drawing.Size(225, 24);
            this.chkExposeDebugInfo.TabIndex = 9;
            this.chkExposeDebugInfo.Text = "Expose Debug Information";
            this.chkExposeDebugInfo.UseVisualStyleBackColor = true;
            // 
            // chkDualMonitors
            // 
            this.chkDualMonitors.AutoSize = true;
            this.chkDualMonitors.Location = new System.Drawing.Point(75, 149);
            this.chkDualMonitors.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkDualMonitors.Name = "chkDualMonitors";
            this.chkDualMonitors.Size = new System.Drawing.Size(265, 24);
            this.chkDualMonitors.TabIndex = 10;
            this.chkDualMonitors.Text = "Program Requires Dual Monitors";
            this.chkDualMonitors.UseVisualStyleBackColor = true;
            // 
            // AssembleForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(520, 238);
            this.Controls.Add(this.chkDualMonitors);
            this.Controls.Add(this.chkExposeDebugInfo);
            this.Controls.Add(this.chkShowLabelResolve);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnAssembleSource);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.txtOutputFilename);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtOrigin);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AssembleForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Compile BASIC Program";
            this.Load += new System.EventHandler(this.AssembleFormLoad);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtOrigin;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtOutputFilename;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnAssembleSource;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox chkShowLabelResolve;
        private System.Windows.Forms.CheckBox chkExposeDebugInfo;
        private System.Windows.Forms.CheckBox chkDualMonitors;
    }
}