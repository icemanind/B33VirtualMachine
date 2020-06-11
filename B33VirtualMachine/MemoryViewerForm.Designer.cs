namespace B33VirtualMachine
{
    partial class MemoryViewerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MemoryViewerForm));
            this.label1 = new System.Windows.Forms.Label();
            this.txtScrollToAddress = new System.Windows.Forms.TextBox();
            this.txtMemory = new System.Windows.Forms.RichTextBox();
            this.sbMemory = new System.Windows.Forms.VScrollBar();
            this.btnGo = new System.Windows.Forms.Button();
            this.chkLiveUpdate = new System.Windows.Forms.CheckBox();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 22);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(137, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Scroll To Address (hex):    $";
            // 
            // txtScrollToAddress
            // 
            this.txtScrollToAddress.Location = new System.Drawing.Point(151, 19);
            this.txtScrollToAddress.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtScrollToAddress.Name = "txtScrollToAddress";
            this.txtScrollToAddress.Size = new System.Drawing.Size(47, 20);
            this.txtScrollToAddress.TabIndex = 1;
            this.txtScrollToAddress.TextChanged += new System.EventHandler(this.txtScrollToAddress_TextChanged);
            // 
            // txtMemory
            // 
            this.txtMemory.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMemory.Location = new System.Drawing.Point(19, 44);
            this.txtMemory.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtMemory.Name = "txtMemory";
            this.txtMemory.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.txtMemory.Size = new System.Drawing.Size(784, 298);
            this.txtMemory.TabIndex = 2;
            this.txtMemory.Text = "";
            this.txtMemory.WordWrap = false;
            // 
            // sbMemory
            // 
            this.sbMemory.LargeChange = 16;
            this.sbMemory.Location = new System.Drawing.Point(808, 44);
            this.sbMemory.Maximum = 4096;
            this.sbMemory.Name = "sbMemory";
            this.sbMemory.Size = new System.Drawing.Size(26, 297);
            this.sbMemory.TabIndex = 3;
            this.sbMemory.Scroll += new System.Windows.Forms.ScrollEventHandler(this.sbMemory_Scroll);
            // 
            // btnGo
            // 
            this.btnGo.Location = new System.Drawing.Point(204, 18);
            this.btnGo.Name = "btnGo";
            this.btnGo.Size = new System.Drawing.Size(41, 23);
            this.btnGo.TabIndex = 4;
            this.btnGo.Text = "Go!";
            this.btnGo.UseVisualStyleBackColor = true;
            this.btnGo.Click += new System.EventHandler(this.btnGo_Click);
            // 
            // chkLiveUpdate
            // 
            this.chkLiveUpdate.AutoSize = true;
            this.chkLiveUpdate.Location = new System.Drawing.Point(251, 22);
            this.chkLiveUpdate.Name = "chkLiveUpdate";
            this.chkLiveUpdate.Size = new System.Drawing.Size(114, 17);
            this.chkLiveUpdate.TabIndex = 5;
            this.chkLiveUpdate.Text = "Live Update (slow)";
            this.chkLiveUpdate.UseVisualStyleBackColor = true;
            this.chkLiveUpdate.CheckedChanged += new System.EventHandler(this.chkLiveUpdate_CheckedChanged);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(728, 16);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(75, 23);
            this.btnRefresh.TabIndex = 6;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // MemoryViewerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(841, 369);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.chkLiveUpdate);
            this.Controls.Add(this.btnGo);
            this.Controls.Add(this.sbMemory);
            this.Controls.Add(this.txtMemory);
            this.Controls.Add(this.txtScrollToAddress);
            this.Controls.Add(this.label1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.MaximizeBox = false;
            this.Name = "MemoryViewerForm";
            this.Text = "Memory Viewer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MemoryViewerForm_FormClosing);
            this.Load += new System.EventHandler(this.MemoryViewerForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtScrollToAddress;
        private System.Windows.Forms.RichTextBox txtMemory;
        private System.Windows.Forms.VScrollBar sbMemory;
        private System.Windows.Forms.Button btnGo;
        private System.Windows.Forms.CheckBox chkLiveUpdate;
        private System.Windows.Forms.Button btnRefresh;
    }
}