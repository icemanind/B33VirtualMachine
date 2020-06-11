namespace B33VirtualMachine
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
            this.components = new System.ComponentModel.Container();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openB33BinaryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.virtualMachineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pauseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.updateRegisterStatusInRealTimeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showDebugFlowInRealTimeSlowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.virtualMachineSpeedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.realTimeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ms1SecondToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ms34SecondToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ms12SecondToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ms14SecondToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ms18SecondToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ms116SecondToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ms132SecondToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ms164SecondToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ofdOpenBinary = new System.Windows.Forms.OpenFileDialog();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsbOpen = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbPlay = new System.Windows.Forms.ToolStripButton();
            this.tsbPause = new System.Windows.Forms.ToolStripButton();
            this.tsbStop = new System.Windows.Forms.ToolStripButton();
            this.tmrPulse = new System.Windows.Forms.Timer(this.components);
            this.txtSourceCode = new System.Windows.Forms.RichTextBox();
            this.b33Screen2 = new B33Cpu.Hardware.B33Screen();
            this.b33Screen1 = new B33Cpu.Hardware.B33Screen();
            this.showMemoryViewerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lblCpuInfo = new B33VirtualMachine.RegisterLabel();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.virtualMachineToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(9, 3, 0, 3);
            this.menuStrip1.Size = new System.Drawing.Size(1407, 35);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openB33BinaryToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(50, 29);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // openB33BinaryToolStripMenuItem
            // 
            this.openB33BinaryToolStripMenuItem.Name = "openB33BinaryToolStripMenuItem";
            this.openB33BinaryToolStripMenuItem.Size = new System.Drawing.Size(241, 30);
            this.openB33BinaryToolStripMenuItem.Text = "&Open B33 Binary...";
            this.openB33BinaryToolStripMenuItem.Click += new System.EventHandler(this.OpenB33BinaryToolStripMenuItemClick);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(238, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(241, 30);
            this.exitToolStripMenuItem.Text = "E&xit";
            // 
            // virtualMachineToolStripMenuItem
            // 
            this.virtualMachineToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startToolStripMenuItem,
            this.pauseToolStripMenuItem,
            this.stopToolStripMenuItem,
            this.toolStripSeparator3,
            this.updateRegisterStatusInRealTimeToolStripMenuItem,
            this.showDebugFlowInRealTimeSlowToolStripMenuItem,
            this.showMemoryViewerToolStripMenuItem,
            this.toolStripSeparator4,
            this.virtualMachineSpeedToolStripMenuItem});
            this.virtualMachineToolStripMenuItem.Name = "virtualMachineToolStripMenuItem";
            this.virtualMachineToolStripMenuItem.Size = new System.Drawing.Size(145, 29);
            this.virtualMachineToolStripMenuItem.Text = "&Virtual Machine";
            // 
            // startToolStripMenuItem
            // 
            this.startToolStripMenuItem.Name = "startToolStripMenuItem";
            this.startToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.startToolStripMenuItem.Size = new System.Drawing.Size(404, 30);
            this.startToolStripMenuItem.Text = "&Start";
            this.startToolStripMenuItem.Click += new System.EventHandler(this.StartToolStripMenuItemClick);
            // 
            // pauseToolStripMenuItem
            // 
            this.pauseToolStripMenuItem.Name = "pauseToolStripMenuItem";
            this.pauseToolStripMenuItem.Size = new System.Drawing.Size(404, 30);
            this.pauseToolStripMenuItem.Text = "&Pause";
            this.pauseToolStripMenuItem.Click += new System.EventHandler(this.PauseToolStripMenuItemClick);
            // 
            // stopToolStripMenuItem
            // 
            this.stopToolStripMenuItem.Name = "stopToolStripMenuItem";
            this.stopToolStripMenuItem.Size = new System.Drawing.Size(404, 30);
            this.stopToolStripMenuItem.Text = "S&top";
            this.stopToolStripMenuItem.Click += new System.EventHandler(this.StopToolStripMenuItemClick);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(401, 6);
            // 
            // updateRegisterStatusInRealTimeToolStripMenuItem
            // 
            this.updateRegisterStatusInRealTimeToolStripMenuItem.Checked = true;
            this.updateRegisterStatusInRealTimeToolStripMenuItem.CheckOnClick = true;
            this.updateRegisterStatusInRealTimeToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.updateRegisterStatusInRealTimeToolStripMenuItem.Name = "updateRegisterStatusInRealTimeToolStripMenuItem";
            this.updateRegisterStatusInRealTimeToolStripMenuItem.Size = new System.Drawing.Size(404, 30);
            this.updateRegisterStatusInRealTimeToolStripMenuItem.Text = "&Update Register Status in Real Time";
            // 
            // showDebugFlowInRealTimeSlowToolStripMenuItem
            // 
            this.showDebugFlowInRealTimeSlowToolStripMenuItem.CheckOnClick = true;
            this.showDebugFlowInRealTimeSlowToolStripMenuItem.Name = "showDebugFlowInRealTimeSlowToolStripMenuItem";
            this.showDebugFlowInRealTimeSlowToolStripMenuItem.Size = new System.Drawing.Size(404, 30);
            this.showDebugFlowInRealTimeSlowToolStripMenuItem.Text = "Show &Debug Flow in Real Time (Slow!!)";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(401, 6);
            // 
            // virtualMachineSpeedToolStripMenuItem
            // 
            this.virtualMachineSpeedToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.realTimeToolStripMenuItem,
            this.ms1SecondToolStripMenuItem,
            this.ms34SecondToolStripMenuItem,
            this.ms12SecondToolStripMenuItem,
            this.ms14SecondToolStripMenuItem,
            this.ms18SecondToolStripMenuItem,
            this.ms116SecondToolStripMenuItem,
            this.ms132SecondToolStripMenuItem,
            this.ms164SecondToolStripMenuItem});
            this.virtualMachineSpeedToolStripMenuItem.Name = "virtualMachineSpeedToolStripMenuItem";
            this.virtualMachineSpeedToolStripMenuItem.Size = new System.Drawing.Size(404, 30);
            this.virtualMachineSpeedToolStripMenuItem.Text = "Virtual Machine Sp&eed";
            // 
            // realTimeToolStripMenuItem
            // 
            this.realTimeToolStripMenuItem.Checked = true;
            this.realTimeToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.realTimeToolStripMenuItem.Name = "realTimeToolStripMenuItem";
            this.realTimeToolStripMenuItem.Size = new System.Drawing.Size(257, 30);
            this.realTimeToolStripMenuItem.Text = "&Real Time";
            this.realTimeToolStripMenuItem.Click += new System.EventHandler(this.realTimeToolStripMenuItem_Click);
            // 
            // ms1SecondToolStripMenuItem
            // 
            this.ms1SecondToolStripMenuItem.Name = "ms1SecondToolStripMenuItem";
            this.ms1SecondToolStripMenuItem.Size = new System.Drawing.Size(257, 30);
            this.ms1SecondToolStripMenuItem.Text = "1000ms (1 Second)";
            this.ms1SecondToolStripMenuItem.Click += new System.EventHandler(this.ms1SecondToolStripMenuItem_Click);
            // 
            // ms34SecondToolStripMenuItem
            // 
            this.ms34SecondToolStripMenuItem.Name = "ms34SecondToolStripMenuItem";
            this.ms34SecondToolStripMenuItem.Size = new System.Drawing.Size(257, 30);
            this.ms34SecondToolStripMenuItem.Text = "750ms (3/4 Second)";
            this.ms34SecondToolStripMenuItem.Click += new System.EventHandler(this.ms34SecondToolStripMenuItem_Click);
            // 
            // ms12SecondToolStripMenuItem
            // 
            this.ms12SecondToolStripMenuItem.Name = "ms12SecondToolStripMenuItem";
            this.ms12SecondToolStripMenuItem.Size = new System.Drawing.Size(257, 30);
            this.ms12SecondToolStripMenuItem.Text = "500ms (1/2 Second)";
            this.ms12SecondToolStripMenuItem.Click += new System.EventHandler(this.ms12SecondToolStripMenuItem_Click);
            // 
            // ms14SecondToolStripMenuItem
            // 
            this.ms14SecondToolStripMenuItem.Name = "ms14SecondToolStripMenuItem";
            this.ms14SecondToolStripMenuItem.Size = new System.Drawing.Size(257, 30);
            this.ms14SecondToolStripMenuItem.Text = "250ms (1/4 Second)";
            this.ms14SecondToolStripMenuItem.Click += new System.EventHandler(this.ms14SecondToolStripMenuItem_Click);
            // 
            // ms18SecondToolStripMenuItem
            // 
            this.ms18SecondToolStripMenuItem.Name = "ms18SecondToolStripMenuItem";
            this.ms18SecondToolStripMenuItem.Size = new System.Drawing.Size(257, 30);
            this.ms18SecondToolStripMenuItem.Text = "125ms (1/8 Second)";
            this.ms18SecondToolStripMenuItem.Click += new System.EventHandler(this.ms18SecondToolStripMenuItem_Click);
            // 
            // ms116SecondToolStripMenuItem
            // 
            this.ms116SecondToolStripMenuItem.Name = "ms116SecondToolStripMenuItem";
            this.ms116SecondToolStripMenuItem.Size = new System.Drawing.Size(257, 30);
            this.ms116SecondToolStripMenuItem.Text = "62ms (1/16 Second)";
            this.ms116SecondToolStripMenuItem.Click += new System.EventHandler(this.ms116SecondToolStripMenuItem_Click);
            // 
            // ms132SecondToolStripMenuItem
            // 
            this.ms132SecondToolStripMenuItem.Name = "ms132SecondToolStripMenuItem";
            this.ms132SecondToolStripMenuItem.Size = new System.Drawing.Size(257, 30);
            this.ms132SecondToolStripMenuItem.Text = "31ms (1/32 Second)";
            this.ms132SecondToolStripMenuItem.Click += new System.EventHandler(this.ms132SecondToolStripMenuItem_Click);
            // 
            // ms164SecondToolStripMenuItem
            // 
            this.ms164SecondToolStripMenuItem.Name = "ms164SecondToolStripMenuItem";
            this.ms164SecondToolStripMenuItem.Size = new System.Drawing.Size(257, 30);
            this.ms164SecondToolStripMenuItem.Text = "16ms (1/64 Second)";
            this.ms164SecondToolStripMenuItem.Click += new System.EventHandler(this.ms164SecondToolStripMenuItem_Click);
            // 
            // ofdOpenBinary
            // 
            this.ofdOpenBinary.DefaultExt = "B33";
            this.ofdOpenBinary.Filter = "B33 Files|*.B33";
            this.ofdOpenBinary.Title = "Open B33 Binary File";
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbOpen,
            this.toolStripSeparator2,
            this.tsbPlay,
            this.tsbPause,
            this.tsbStop});
            this.toolStrip1.Location = new System.Drawing.Point(0, 35);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStrip1.Size = new System.Drawing.Size(1407, 31);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // tsbOpen
            // 
            this.tsbOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbOpen.Image = global::B33VirtualMachine.Properties.Resources.openHS;
            this.tsbOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbOpen.Name = "tsbOpen";
            this.tsbOpen.Size = new System.Drawing.Size(28, 28);
            this.tsbOpen.Text = "toolStripButton1";
            this.tsbOpen.ToolTipText = "Open B33 Binary";
            this.tsbOpen.Click += new System.EventHandler(this.TsbOpenClick);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 31);
            // 
            // tsbPlay
            // 
            this.tsbPlay.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbPlay.Image = global::B33VirtualMachine.Properties.Resources.PlayHS;
            this.tsbPlay.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbPlay.Name = "tsbPlay";
            this.tsbPlay.Size = new System.Drawing.Size(28, 28);
            this.tsbPlay.Text = "toolStripButton2";
            this.tsbPlay.ToolTipText = "Start Virtual Machine";
            this.tsbPlay.Click += new System.EventHandler(this.tsbPlay_Click);
            // 
            // tsbPause
            // 
            this.tsbPause.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbPause.Image = global::B33VirtualMachine.Properties.Resources.PauseHS;
            this.tsbPause.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbPause.Name = "tsbPause";
            this.tsbPause.Size = new System.Drawing.Size(28, 28);
            this.tsbPause.Text = "toolStripButton3";
            this.tsbPause.ToolTipText = "Pause Virtual Machine";
            this.tsbPause.Click += new System.EventHandler(this.TsbPauseClick);
            // 
            // tsbStop
            // 
            this.tsbStop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbStop.Image = global::B33VirtualMachine.Properties.Resources.StopHS;
            this.tsbStop.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbStop.Name = "tsbStop";
            this.tsbStop.Size = new System.Drawing.Size(28, 28);
            this.tsbStop.Text = "toolStripButton4";
            this.tsbStop.ToolTipText = "Stop Virtual Machine";
            this.tsbStop.Click += new System.EventHandler(this.TsbStopClick);
            // 
            // txtSourceCode
            // 
            this.txtSourceCode.BackColor = System.Drawing.Color.White;
            this.txtSourceCode.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtSourceCode.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSourceCode.Location = new System.Drawing.Point(972, 143);
            this.txtSourceCode.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtSourceCode.Name = "txtSourceCode";
            this.txtSourceCode.ReadOnly = true;
            this.txtSourceCode.Size = new System.Drawing.Size(435, 431);
            this.txtSourceCode.TabIndex = 4;
            this.txtSourceCode.Text = "";
            this.txtSourceCode.WordWrap = false;
            // 
            // b33Screen2
            // 
            this.b33Screen2.BackColor = System.Drawing.Color.Black;
            this.b33Screen2.Location = new System.Drawing.Point(6, 618);
            this.b33Screen2.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.b33Screen2.MemoryLocation = ((ushort)(49152));
            this.b33Screen2.Name = "b33Screen2";
            this.b33Screen2.Size = new System.Drawing.Size(644, 304);
            this.b33Screen2.TabIndex = 5;
            // 
            // b33Screen1
            // 
            this.b33Screen1.BackColor = System.Drawing.Color.Black;
            this.b33Screen1.Location = new System.Drawing.Point(6, 143);
            this.b33Screen1.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.b33Screen1.MemoryLocation = ((ushort)(57344));
            this.b33Screen1.Name = "b33Screen1";
            this.b33Screen1.Size = new System.Drawing.Size(644, 304);
            this.b33Screen1.TabIndex = 3;
            // 
            // showMemoryViewerToolStripMenuItem
            // 
            this.showMemoryViewerToolStripMenuItem.Name = "showMemoryViewerToolStripMenuItem";
            this.showMemoryViewerToolStripMenuItem.Size = new System.Drawing.Size(404, 30);
            this.showMemoryViewerToolStripMenuItem.Text = "Show Memory Viewer";
            this.showMemoryViewerToolStripMenuItem.Click += new System.EventHandler(this.showMemoryViewerToolStripMenuItem_Click);
            // 
            // lblCpuInfo
            // 
            this.lblCpuInfo.BackColor = System.Drawing.Color.LightYellow;
            this.lblCpuInfo.Location = new System.Drawing.Point(10, 87);
            this.lblCpuInfo.Name = "lblCpuInfo";
            this.lblCpuInfo.Size = new System.Drawing.Size(1407, 33);
            this.lblCpuInfo.TabIndex = 6;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1407, 1091);
            this.Controls.Add(this.lblCpuInfo);
            this.Controls.Add(this.b33Screen2);
            this.Controls.Add(this.txtSourceCode);
            this.Controls.Add(this.b33Screen1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "B33 Virtual Machine";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainFormFormClosing);
            this.Load += new System.EventHandler(this.MainFormLoad);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openB33BinaryToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog ofdOpenBinary;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsbOpen;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton tsbPlay;
        private System.Windows.Forms.ToolStripButton tsbPause;
        private System.Windows.Forms.ToolStripButton tsbStop;
        private B33Cpu.Hardware.B33Screen b33Screen1;
        private System.Windows.Forms.ToolStripMenuItem virtualMachineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pauseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stopToolStripMenuItem;
        private System.Windows.Forms.Timer tmrPulse;
        private System.Windows.Forms.RichTextBox txtSourceCode;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem updateRegisterStatusInRealTimeToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem virtualMachineSpeedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem realTimeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ms1SecondToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ms34SecondToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ms12SecondToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ms14SecondToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ms18SecondToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ms116SecondToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showDebugFlowInRealTimeSlowToolStripMenuItem;
        private B33Cpu.Hardware.B33Screen b33Screen2;
        private System.Windows.Forms.ToolStripMenuItem ms132SecondToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ms164SecondToolStripMenuItem;
        private RegisterLabel lblCpuInfo;
        private System.Windows.Forms.ToolStripMenuItem showMemoryViewerToolStripMenuItem;
    }
}

