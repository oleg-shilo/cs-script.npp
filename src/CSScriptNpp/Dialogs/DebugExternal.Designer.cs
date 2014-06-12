namespace CSScriptNpp
{
    partial class DebugExternal
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
            this.label1 = new System.Windows.Forms.Label();
            this.appPath = new System.Windows.Forms.TextBox();
            this.startBtn = new System.Windows.Forms.Button();
            this.attacheBtn = new System.Windows.Forms.Button();
            this.refreshBtn = new System.Windows.Forms.Button();
            this.processList = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.managedOnly = new System.Windows.Forms.CheckBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Executable path:";
            // 
            // appPath
            // 
            this.appPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.appPath.Location = new System.Drawing.Point(12, 25);
            this.appPath.Name = "appPath";
            this.appPath.Size = new System.Drawing.Size(664, 20);
            this.appPath.TabIndex = 3;
            // 
            // startBtn
            // 
            this.startBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.startBtn.Location = new System.Drawing.Point(683, 23);
            this.startBtn.Name = "startBtn";
            this.startBtn.Size = new System.Drawing.Size(75, 23);
            this.startBtn.TabIndex = 4;
            this.startBtn.Text = "Start";
            this.startBtn.UseVisualStyleBackColor = true;
            this.startBtn.Click += new System.EventHandler(this.button1_Click);
            // 
            // attacheBtn
            // 
            this.attacheBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.attacheBtn.Location = new System.Drawing.Point(683, 63);
            this.attacheBtn.Name = "attacheBtn";
            this.attacheBtn.Size = new System.Drawing.Size(75, 23);
            this.attacheBtn.TabIndex = 1;
            this.attacheBtn.Text = "Attach";
            this.attacheBtn.UseVisualStyleBackColor = true;
            this.attacheBtn.Click += new System.EventHandler(this.attacheBtn_Click);
            // 
            // refreshBtn
            // 
            this.refreshBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.refreshBtn.Location = new System.Drawing.Point(683, 92);
            this.refreshBtn.Name = "refreshBtn";
            this.refreshBtn.Size = new System.Drawing.Size(75, 23);
            this.refreshBtn.TabIndex = 2;
            this.refreshBtn.Text = "Refresh";
            this.refreshBtn.UseVisualStyleBackColor = true;
            this.refreshBtn.Click += new System.EventHandler(this.refreshBtn_Click);
            // 
            // processList
            // 
            this.processList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.processList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader3});
            this.processList.FullRowSelect = true;
            this.processList.HideSelection = false;
            this.processList.Location = new System.Drawing.Point(12, 63);
            this.processList.MultiSelect = false;
            this.processList.Name = "processList";
            this.processList.Size = new System.Drawing.Size(664, 204);
            this.processList.TabIndex = 0;
            this.processList.UseCompatibleStateImageBehavior = false;
            this.processList.View = System.Windows.Forms.View.Details;
            this.processList.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.processList_ColumnClick);
            this.processList.DoubleClick += new System.EventHandler(this.processList_DoubleClick);
            this.processList.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.processList_PreviewKeyDown);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Process";
            this.columnHeader1.Width = 148;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "ID";
            this.columnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader2.Width = 51;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "CPU";
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Runtime";
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Title";
            this.columnHeader3.Width = 313;
            // 
            // managedOnly
            // 
            this.managedOnly.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.managedOnly.AutoSize = true;
            this.managedOnly.Location = new System.Drawing.Point(12, 274);
            this.managedOnly.Name = "managedOnly";
            this.managedOnly.Size = new System.Drawing.Size(147, 17);
            this.managedOnly.TabIndex = 9;
            this.managedOnly.Text = "Managed Processes Only";
            this.managedOnly.UseVisualStyleBackColor = true;
            this.managedOnly.CheckedChanged += new System.EventHandler(this.managedOnly_CheckedChanged);
            // 
            // timer1
            // 
            this.timer1.Interval = 200;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // DebugExternal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(767, 293);
            this.Controls.Add(this.managedOnly);
            this.Controls.Add(this.processList);
            this.Controls.Add(this.refreshBtn);
            this.Controls.Add(this.attacheBtn);
            this.Controls.Add(this.startBtn);
            this.Controls.Add(this.appPath);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MinimumSize = new System.Drawing.Size(200, 100);
            this.Name = "DebugExternal";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Debug External Process";
            this.Load += new System.EventHandler(this.DebugExternal_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox appPath;
        private System.Windows.Forms.Button startBtn;
        private System.Windows.Forms.Button attacheBtn;
        private System.Windows.Forms.Button refreshBtn;
        private System.Windows.Forms.ListView processList;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.CheckBox managedOnly;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.Timer timer1;
    }
}