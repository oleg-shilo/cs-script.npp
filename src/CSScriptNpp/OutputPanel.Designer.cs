namespace CSScriptNpp
{
    partial class OutputPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OutputPanel));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.outputType = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.DebugViewBtn = new System.Windows.Forms.ToolStripButton();
            this.debugFilterBtn = new System.Windows.Forms.ToolStripButton();
            this.designTimeTextBox = new System.Windows.Forms.TextBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.outputType,
            this.toolStripSeparator3,
            this.toolStripSeparator1,
            this.toolStripButton1,
            this.toolStripSeparator2,
            this.DebugViewBtn,
            this.debugFilterBtn});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(765, 25);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(107, 22);
            this.toolStripLabel1.Text = "Show output from:";
            // 
            // outputType
            // 
            this.outputType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.outputType.Name = "outputType";
            this.outputType.Size = new System.Drawing.Size(121, 25);
            this.outputType.DropDown += new System.EventHandler(this.outputType_DropDown);
            this.outputType.SelectedIndexChanged += new System.EventHandler(this.outputType_SelectedIndexChanged);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = global::CSScriptNpp.Properties.Resources.clean;
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton1.Text = "Clear";
            this.toolStripButton1.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // DebugViewBtn
            // 
            this.DebugViewBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.DebugViewBtn.Image = ((System.Drawing.Image)(resources.GetObject("DebugViewBtn.Image")));
            this.DebugViewBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.DebugViewBtn.Name = "DebugViewBtn";
            this.DebugViewBtn.Size = new System.Drawing.Size(23, 22);
            this.DebugViewBtn.Text = "DebugView";
            this.DebugViewBtn.Click += new System.EventHandler(this.DebugViewBtn_Click);
            // 
            // debugFilterBtn
            // 
            this.debugFilterBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.debugFilterBtn.Image = ((System.Drawing.Image)(resources.GetObject("debugFilterBtn.Image")));
            this.debugFilterBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.debugFilterBtn.Name = "debugFilterBtn";
            this.debugFilterBtn.Size = new System.Drawing.Size(23, 22);
            this.debugFilterBtn.Text = "debugFilter";
            this.debugFilterBtn.Click += new System.EventHandler(this.debugFilterBtn_Click);
            // 
            // designTimeTextBox
            // 
            this.designTimeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.designTimeTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.designTimeTextBox.Location = new System.Drawing.Point(364, 25);
            this.designTimeTextBox.Multiline = true;
            this.designTimeTextBox.Name = "designTimeTextBox";
            this.designTimeTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.designTimeTextBox.Size = new System.Drawing.Size(401, 243);
            this.designTimeTextBox.TabIndex = 3;
            this.designTimeTextBox.Visible = false;
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(590, 4);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(163, 21);
            this.comboBox1.TabIndex = 4;
            this.comboBox1.Visible = false;
            // 
            // OutputPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(765, 268);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.designTimeTextBox);
            this.Controls.Add(this.toolStrip1);
            this.Name = "OutputPanel";
            this.Text = "CS-Script Output";
            this.Activated += new System.EventHandler(this.OutputPanel_Activated);
            this.Deactivate += new System.EventHandler(this.OutputPanel_Deactivate);
            this.Shown += new System.EventHandler(this.OutputPanel_Shown);
            this.VisibleChanged += new System.EventHandler(this.OutputPanel_VisibleChanged);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripComboBox outputType;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.TextBox designTimeTextBox;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton DebugViewBtn;
        private System.Windows.Forms.ToolStripButton debugFilterBtn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ComboBox comboBox1;
    }
}