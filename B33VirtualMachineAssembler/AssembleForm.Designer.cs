namespace B33VirtualMachineAssembler
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
            this.label1.Location = new System.Drawing.Point(11, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(106, 19);
            this.label1.TabIndex = 1;
            this.label1.Text = "Origin (in Hex):";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtOrigin
            // 
            this.txtOrigin.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtOrigin.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtOrigin.Location = new System.Drawing.Point(120, 7);
            this.txtOrigin.Name = "txtOrigin";
            this.txtOrigin.Size = new System.Drawing.Size(59, 21);
            this.txtOrigin.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(7, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(110, 19);
            this.label2.TabIndex = 3;
            this.label2.Text = "Output Filename:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtOutputFilename
            // 
            this.txtOutputFilename.Location = new System.Drawing.Point(120, 30);
            this.txtOutputFilename.Name = "txtOutputFilename";
            this.txtOutputFilename.Size = new System.Drawing.Size(180, 20);
            this.txtOutputFilename.TabIndex = 4;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(303, 29);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(28, 23);
            this.btnBrowse.TabIndex = 5;
            this.btnBrowse.Text = "...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.BtnBrowseClick);
            // 
            // btnAssembleSource
            // 
            this.btnAssembleSource.Location = new System.Drawing.Point(12, 122);
            this.btnAssembleSource.Name = "btnAssembleSource";
            this.btnAssembleSource.Size = new System.Drawing.Size(124, 23);
            this.btnAssembleSource.TabIndex = 6;
            this.btnAssembleSource.Text = "Assemble Source!";
            this.btnAssembleSource.UseVisualStyleBackColor = true;
            this.btnAssembleSource.Click += new System.EventHandler(this.BtnAssembleSourceClick);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(207, 122);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(124, 23);
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
            this.chkShowLabelResolve.Location = new System.Drawing.Point(50, 56);
            this.chkShowLabelResolve.Name = "chkShowLabelResolve";
            this.chkShowLabelResolve.Size = new System.Drawing.Size(248, 17);
            this.chkShowLabelResolve.TabIndex = 8;
            this.chkShowLabelResolve.Text = "Show Label Resolve Addresses After Assembly";
            this.chkShowLabelResolve.UseVisualStyleBackColor = true;
            // 
            // chkExposeDebugInfo
            // 
            this.chkExposeDebugInfo.AutoSize = true;
            this.chkExposeDebugInfo.Location = new System.Drawing.Point(50, 76);
            this.chkExposeDebugInfo.Name = "chkExposeDebugInfo";
            this.chkExposeDebugInfo.Size = new System.Drawing.Size(151, 17);
            this.chkExposeDebugInfo.TabIndex = 9;
            this.chkExposeDebugInfo.Text = "Expose Debug Information";
            this.chkExposeDebugInfo.UseVisualStyleBackColor = true;
            // 
            // chkDualMonitors
            // 
            this.chkDualMonitors.AutoSize = true;
            this.chkDualMonitors.Location = new System.Drawing.Point(50, 97);
            this.chkDualMonitors.Name = "chkDualMonitors";
            this.chkDualMonitors.Size = new System.Drawing.Size(178, 17);
            this.chkDualMonitors.TabIndex = 10;
            this.chkDualMonitors.Text = "Program Requires Dual Monitors";
            this.chkDualMonitors.UseVisualStyleBackColor = true;
            // 
            // AssembleForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(347, 155);
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
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AssembleForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Assemble";
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