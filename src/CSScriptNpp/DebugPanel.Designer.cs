namespace CSScriptNpp
{
    partial class DebugPanel
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
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.goBtn = new System.Windows.Forms.ToolStripButton();
            this.breakBtn = new System.Windows.Forms.ToolStripButton();
            this.stopBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.stepOverBtn = new System.Windows.Forms.ToolStripButton();
            this.stepIntoBtn = new System.Windows.Forms.ToolStripButton();
            this.stepOutBtn = new System.Windows.Forms.ToolStripButton();
            this.setNextBtn = new System.Windows.Forms.ToolStripButton();
            this.runToCursorBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toggleBpBtn = new System.Windows.Forms.ToolStripButton();
            this.quickWatchBtn = new System.Windows.Forms.ToolStripButton();
            this.appTypeCombo = new System.Windows.Forms.ToolStripComboBox();
            this.breakOnExceptionBtn = new System.Windows.Forms.ToolStripButton();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.AutoSize = false;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.goBtn,
            this.breakBtn,
            this.stopBtn,
            this.toolStripSeparator1,
            this.stepOverBtn,
            this.stepIntoBtn,
            this.stepOutBtn,
            this.setNextBtn,
            this.runToCursorBtn,
            this.toolStripSeparator2,
            this.toggleBpBtn,
            this.quickWatchBtn,
            this.appTypeCombo,
            this.breakOnExceptionBtn});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(545, 25);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // goBtn
            // 
            this.goBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.goBtn.Image = global::CSScriptNpp.Resources.Resources.dbg_go;
            this.goBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.goBtn.Name = "goBtn";
            this.goBtn.Size = new System.Drawing.Size(23, 22);
            this.goBtn.Text = "Start Debugging";
            this.goBtn.ToolTipText = "Start Debugging";
            this.goBtn.Click += new System.EventHandler(this.goBtn_Click);
            // 
            // breakBtn
            // 
            this.breakBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.breakBtn.Image = global::CSScriptNpp.Resources.Resources.dbg_break;
            this.breakBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.breakBtn.Name = "breakBtn";
            this.breakBtn.Size = new System.Drawing.Size(23, 22);
            this.breakBtn.Text = "Break All";
            this.breakBtn.Click += new System.EventHandler(this.breakBtn_Click);
            // 
            // stopBtn
            // 
            this.stopBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.stopBtn.Image = global::CSScriptNpp.Resources.Resources.dbg_stop;
            this.stopBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.stopBtn.Name = "stopBtn";
            this.stopBtn.Size = new System.Drawing.Size(23, 22);
            this.stopBtn.Text = "Stop Debugging";
            this.stopBtn.ToolTipText = "Stop Debugging";
            this.stopBtn.Click += new System.EventHandler(this.stopBtn_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // stepOverBtn
            // 
            this.stepOverBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.stepOverBtn.Image = global::CSScriptNpp.Resources.Resources.dbg_stepover;
            this.stepOverBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.stepOverBtn.Name = "stepOverBtn";
            this.stepOverBtn.Size = new System.Drawing.Size(23, 22);
            this.stepOverBtn.Text = "toolStripButton4";
            this.stepOverBtn.ToolTipText = "Step Over";
            this.stepOverBtn.Click += new System.EventHandler(this.stepOverBtn_Click);
            // 
            // stepIntoBtn
            // 
            this.stepIntoBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.stepIntoBtn.Image = global::CSScriptNpp.Resources.Resources.dbg_stepin;
            this.stepIntoBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.stepIntoBtn.Name = "stepIntoBtn";
            this.stepIntoBtn.Size = new System.Drawing.Size(23, 22);
            this.stepIntoBtn.Text = "Step Into";
            this.stepIntoBtn.ToolTipText = "Step Into";
            this.stepIntoBtn.Click += new System.EventHandler(this.stepIntoBtn_Click);
            // 
            // stepOutBtn
            // 
            this.stepOutBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.stepOutBtn.Image = global::CSScriptNpp.Resources.Resources.dbg_stepout;
            this.stepOutBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.stepOutBtn.Name = "stepOutBtn";
            this.stepOutBtn.Size = new System.Drawing.Size(23, 22);
            this.stepOutBtn.Text = "Step Out";
            this.stepOutBtn.Click += new System.EventHandler(this.stepOutBtn_Click);
            // 
            // setNextBtn
            // 
            this.setNextBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.setNextBtn.Image = global::CSScriptNpp.Resources.Resources.dbg_setnext;
            this.setNextBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.setNextBtn.Name = "setNextBtn";
            this.setNextBtn.Size = new System.Drawing.Size(23, 22);
            this.setNextBtn.Text = "Set Next Statement";
            this.setNextBtn.ToolTipText = "Set Next Statement";
            this.setNextBtn.Click += new System.EventHandler(this.setNextBtn_Click);
            // 
            // runToCursorBtn
            // 
            this.runToCursorBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.runToCursorBtn.Image = global::CSScriptNpp.Resources.Resources.dbg_runtocusrsor;
            this.runToCursorBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.runToCursorBtn.Name = "runToCursorBtn";
            this.runToCursorBtn.Size = new System.Drawing.Size(23, 22);
            this.runToCursorBtn.Text = "Run to cursor";
            this.runToCursorBtn.ToolTipText = "Run to cursor";
            this.runToCursorBtn.Click += new System.EventHandler(this.runToCursorBtn_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toggleBpBtn
            // 
            this.toggleBpBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toggleBpBtn.Image = global::CSScriptNpp.Resources.Resources.dbg_togglebp;
            this.toggleBpBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toggleBpBtn.Name = "toggleBpBtn";
            this.toggleBpBtn.Size = new System.Drawing.Size(23, 22);
            this.toggleBpBtn.Text = "Toggle Breakpoint";
            this.toggleBpBtn.Click += new System.EventHandler(this.toggleBpBtn_Click);
            // 
            // quickWatchBtn
            // 
            this.quickWatchBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.quickWatchBtn.Image = global::CSScriptNpp.Resources.Resources.dbg_qwatch;
            this.quickWatchBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.quickWatchBtn.Name = "quickWatchBtn";
            this.quickWatchBtn.Size = new System.Drawing.Size(23, 22);
            this.quickWatchBtn.Text = "QuickWatch";
            this.quickWatchBtn.Click += new System.EventHandler(this.quickWatch_Click);
            // 
            // appTypeCombo
            // 
            this.appTypeCombo.AutoSize = false;
            this.appTypeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.appTypeCombo.DropDownWidth = 80;
            this.appTypeCombo.Items.AddRange(new object[] {
            "Console",
            "Windows"});
            this.appTypeCombo.Name = "appTypeCombo";
            this.appTypeCombo.Size = new System.Drawing.Size(121, 23);
            this.appTypeCombo.ToolTipText = "Application Type";
            this.appTypeCombo.SelectedIndexChanged += new System.EventHandler(this.appTypeCombo_SelectedIndexChanged);
            // 
            // breakOnExceptionBtn
            // 
            this.breakOnExceptionBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.breakOnExceptionBtn.Image = global::CSScriptNpp.Resources.Resources.dbg_remove_stoponexc;
            this.breakOnExceptionBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.breakOnExceptionBtn.Name = "breakOnExceptionBtn";
            this.breakOnExceptionBtn.Size = new System.Drawing.Size(23, 22);
            this.breakOnExceptionBtn.Text = "Toggle Break On Exceptions";
            this.breakOnExceptionBtn.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Location = new System.Drawing.Point(0, 28);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(545, 262);
            this.tabControl1.TabIndex = 2;
            // 
            // DebugPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(545, 290);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.tabControl1);
            this.Name = "DebugPanel";
            this.Text = "CS-Script Debug View";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.ToolStripButton goBtn;
        private System.Windows.Forms.ToolStripButton breakBtn;
        private System.Windows.Forms.ToolStripButton stopBtn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton stepOverBtn;
        private System.Windows.Forms.ToolStripButton stepIntoBtn;
        private System.Windows.Forms.ToolStripButton stepOutBtn;
        private System.Windows.Forms.ToolStripButton setNextBtn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toggleBpBtn;
        private System.Windows.Forms.ToolStripComboBox appTypeCombo;
        private System.Windows.Forms.ToolStripButton runToCursorBtn;
        private System.Windows.Forms.ToolStripButton quickWatchBtn;
        private System.Windows.Forms.ToolStripButton breakOnExceptionBtn;

    }
}