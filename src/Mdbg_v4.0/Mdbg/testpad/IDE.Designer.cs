namespace testpad
{
    partial class IDE
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IDE));
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.output = new System.Windows.Forms.TextBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.pause = new System.Windows.Forms.ToolStripButton();
            this.go = new System.Windows.Forms.ToolStripButton();
            this.stepover = new System.Windows.Forms.ToolStripButton();
            this.stepin = new System.Windows.Forms.ToolStripButton();
            this.stepout = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.execute = new System.Windows.Forms.Button();
            this.lineNumber = new System.Windows.Forms.NumericUpDown();
            this.source = new System.Windows.Forms.ComboBox();
            this.insertBreakPoint = new System.Windows.Forms.Button();
            this.start = new System.Windows.Forms.Button();
            this.appName = new System.Windows.Forms.ComboBox();
            this.appArgs = new System.Windows.Forms.ComboBox();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lineNumber)).BeginInit();
            this.SuspendLayout();
            // 
            // comboBox1
            // 
            this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "mo nc on",
            "run \"E:\\Galos\\Projects\\MDbg\\Version_4\\MDbg Sample\\bin\\Debug\\test\\ConsoleApplicati" +
                "on12.exe\"",
            "go",
            "next",
            "out",
            "step",
            "suspend"});
            this.comboBox1.Location = new System.Drawing.Point(12, 55);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(728, 21);
            this.comboBox1.TabIndex = 1;
            this.comboBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
            // 
            // output
            // 
            this.output.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.output.Location = new System.Drawing.Point(12, 112);
            this.output.Multiline = true;
            this.output.Name = "output";
            this.output.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.output.Size = new System.Drawing.Size(847, 109);
            this.output.TabIndex = 2;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pause,
            this.go,
            this.stepover,
            this.stepin,
            this.stepout,
            this.toolStripButton2,
            this.toolStripButton1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(871, 25);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // pause
            // 
            this.pause.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.pause.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.pause.Name = "pause";
            this.pause.Size = new System.Drawing.Size(40, 22);
            this.pause.Text = "break";
            this.pause.Click += new System.EventHandler(this.pause_Click);
            // 
            // go
            // 
            this.go.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.go.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.go.Name = "go";
            this.go.Size = new System.Drawing.Size(25, 22);
            this.go.Text = "go";
            this.go.Click += new System.EventHandler(this.go_Click);
            // 
            // stepover
            // 
            this.stepover.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.stepover.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.stepover.Name = "stepover";
            this.stepover.Size = new System.Drawing.Size(59, 22);
            this.stepover.Text = "step over";
            this.stepover.Click += new System.EventHandler(this.stepover_Click);
            // 
            // stepin
            // 
            this.stepin.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.stepin.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.stepin.Name = "stepin";
            this.stepin.Size = new System.Drawing.Size(46, 22);
            this.stepin.Text = "step in";
            this.stepin.Click += new System.EventHandler(this.stepin_Click);
            // 
            // stepout
            // 
            this.stepout.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.stepout.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.stepout.Name = "stepout";
            this.stepout.Size = new System.Drawing.Size(54, 22);
            this.stepout.Text = "step out";
            this.stepout.Click += new System.EventHandler(this.stepout_Click);
            // 
            // toolStripButton2
            // 
            this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton2.Image")));
            this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Size = new System.Drawing.Size(29, 22);
            this.toolStripButton2.Text = "exit";
            this.toolStripButton2.Click += new System.EventHandler(this.exit_Click);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(30, 22);
            this.toolStripButton1.Text = "test";
            this.toolStripButton1.Click += new System.EventHandler(this.test_Click);
            // 
            // execute
            // 
            this.execute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.execute.Location = new System.Drawing.Point(746, 53);
            this.execute.Name = "execute";
            this.execute.Size = new System.Drawing.Size(113, 23);
            this.execute.TabIndex = 4;
            this.execute.Text = "Execute";
            this.execute.UseVisualStyleBackColor = true;
            this.execute.Click += new System.EventHandler(this.execute_Click);
            // 
            // lineNumber
            // 
            this.lineNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lineNumber.Location = new System.Drawing.Point(661, 83);
            this.lineNumber.Name = "lineNumber";
            this.lineNumber.Size = new System.Drawing.Size(79, 20);
            this.lineNumber.TabIndex = 7;
            this.lineNumber.Value = new decimal(new int[] {
            13,
            0,
            0,
            0});
            // 
            // source
            // 
            this.source.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.source.FormattingEnabled = true;
            this.source.Items.AddRange(new object[] {
            "E:\\Galos\\Projects\\MDbg\\Version_4\\MDbg Sample\\bin\\Debug\\test\\Script.cs",
            "c:\\Users\\osh\\Documents\\Visual Studio 2012\\Projects\\ConsoleApplication12\\ConsoleAp" +
                "plication12\\Program.cs"});
            this.source.Location = new System.Drawing.Point(12, 82);
            this.source.Name = "source";
            this.source.Size = new System.Drawing.Size(643, 21);
            this.source.TabIndex = 6;
            // 
            // insertBreakPoint
            // 
            this.insertBreakPoint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.insertBreakPoint.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.insertBreakPoint.Location = new System.Drawing.Point(746, 79);
            this.insertBreakPoint.Name = "insertBreakPoint";
            this.insertBreakPoint.Size = new System.Drawing.Size(113, 25);
            this.insertBreakPoint.TabIndex = 5;
            this.insertBreakPoint.Text = "Insert break point";
            this.insertBreakPoint.UseVisualStyleBackColor = true;
            this.insertBreakPoint.Click += new System.EventHandler(this.insertBreakPoint_Click);
            // 
            // start
            // 
            this.start.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.start.Location = new System.Drawing.Point(746, 26);
            this.start.Name = "start";
            this.start.Size = new System.Drawing.Size(113, 23);
            this.start.TabIndex = 8;
            this.start.Text = "Start";
            this.start.UseVisualStyleBackColor = true;
            this.start.Click += new System.EventHandler(this.start_Click);
            // 
            // appName
            // 
            this.appName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.appName.FormattingEnabled = true;
            this.appName.Items.AddRange(new object[] {
            "E:\\Galos\\Projects\\MDbg\\Version_4\\MDbg Sample\\bin\\Debug\\test\\css_dbg.exe",
            "E:\\Galos\\Projects\\MDbg\\Version_4\\MDbg Sample\\bin\\Debug\\test\\ConsoleApplication12." +
                "exe",
            "C:\\Program Files (x86)\\Notepad++\\plugins\\CSScriptNpp\\css_dbg.exe"});
            this.appName.Location = new System.Drawing.Point(12, 28);
            this.appName.Name = "appName";
            this.appName.Size = new System.Drawing.Size(533, 21);
            this.appName.TabIndex = 1;
            this.appName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
            // 
            // appArgs
            // 
            this.appArgs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.appArgs.FormattingEnabled = true;
            this.appArgs.Items.AddRange(new object[] {
            "/dbg \"E:\\Galos\\Projects\\MDbg\\Version_4\\MDbg Sample\\bin\\Debug\\test\\Script.cs\"",
            "csws.exe /dbg /l \"E:\\Galos\\Projects\\MDbg\\Version_4\\MDbg Sample\\bin\\Debug\\test\\Scr" +
                "ipt.cs\""});
            this.appArgs.Location = new System.Drawing.Point(551, 28);
            this.appArgs.Name = "appArgs";
            this.appArgs.Size = new System.Drawing.Size(189, 21);
            this.appArgs.TabIndex = 1;
            this.appArgs.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
            // 
            // IDE
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(871, 233);
            this.Controls.Add(this.start);
            this.Controls.Add(this.lineNumber);
            this.Controls.Add(this.source);
            this.Controls.Add(this.insertBreakPoint);
            this.Controls.Add(this.execute);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.appArgs);
            this.Controls.Add(this.appName);
            this.Controls.Add(this.output);
            this.Controls.Add(this.comboBox1);
            this.Name = "IDE";
            this.Text = "IDE";
            this.Load += new System.EventHandler(this.IDE_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lineNumber)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.TextBox output;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton pause;
        private System.Windows.Forms.ToolStripButton go;
        private System.Windows.Forms.ToolStripButton stepover;
        private System.Windows.Forms.ToolStripButton stepin;
        private System.Windows.Forms.ToolStripButton stepout;
        private System.Windows.Forms.Button execute;
        private System.Windows.Forms.NumericUpDown lineNumber;
        private System.Windows.Forms.ComboBox source;
        private System.Windows.Forms.Button insertBreakPoint;
        private System.Windows.Forms.Button start;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.ComboBox appName;
        private System.Windows.Forms.ComboBox appArgs;
    }
}