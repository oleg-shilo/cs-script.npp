namespace CSScriptNpp
{
    partial class ProjectPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProjectPanel));
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.newBtn = new System.Windows.Forms.ToolStripButton();
            this.validateBtn = new System.Windows.Forms.ToolStripButton();
            this.stopBtn = new System.Windows.Forms.ToolStripButton();
            this.runBtn = new System.Windows.Forms.ToolStripButton();
            this.debugBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.reloadBtn = new System.Windows.Forms.ToolStripButton();
            this.loadBtn = new System.Windows.Forms.ToolStripButton();
            this.synchBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.outputBtn = new System.Windows.Forms.ToolStripButton();
            this.openInVsBtn = new System.Windows.Forms.ToolStripButton();
            this.aboutBtn = new System.Windows.Forms.ToolStripButton();
            this.helpBtn = new System.Windows.Forms.ToolStripButton();
            this.solutionContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.unloadScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openCommandPromptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.itemContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openContainingFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1.SuspendLayout();
            this.solutionContextMenu.SuspendLayout();
            this.itemContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeView1
            // 
            this.treeView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeView1.HideSelection = false;
            this.treeView1.ImageIndex = 0;
            this.treeView1.ImageList = this.imageList1;
            this.treeView1.Location = new System.Drawing.Point(0, 28);
            this.treeView1.Name = "treeView1";
            this.treeView1.SelectedImageIndex = 0;
            this.treeView1.ShowNodeToolTips = true;
            this.treeView1.ShowRootLines = false;
            this.treeView1.Size = new System.Drawing.Size(396, 213);
            this.treeView1.TabIndex = 3;
            this.treeView1.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterCollapse);
            this.treeView1.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterExpand);
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            this.treeView1.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView1_NodeMouseClick);
            this.treeView1.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView1_NodeMouseDoubleClick);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "folder_opened.png");
            this.imageList1.Images.SetKeyName(1, "css_logo_16x16_tb.png");
            this.imageList1.Images.SetKeyName(2, "references.png");
            this.imageList1.Images.SetKeyName(3, "includes.png");
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newBtn,
            this.validateBtn,
            this.stopBtn,
            this.runBtn,
            this.debugBtn,
            this.toolStripSeparator2,
            this.reloadBtn,
            this.loadBtn,
            this.synchBtn,
            this.toolStripSeparator1,
            this.outputBtn,
            this.openInVsBtn,
            this.aboutBtn,
            this.helpBtn});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(396, 25);
            this.toolStrip1.TabIndex = 4;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // newBtn
            // 
            this.newBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.newBtn.Image = global::CSScriptNpp.Resources.Resources.add;
            this.newBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.newBtn.Name = "newBtn";
            this.newBtn.Size = new System.Drawing.Size(23, 22);
            this.newBtn.Text = "new";
            this.newBtn.ToolTipText = "Create new script";
            this.newBtn.Click += new System.EventHandler(this.newBtn_Click);
            // 
            // validateBtn
            // 
            this.validateBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.validateBtn.Image = global::CSScriptNpp.Resources.Resources.check;
            this.validateBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.validateBtn.Name = "validateBtn";
            this.validateBtn.Size = new System.Drawing.Size(23, 22);
            this.validateBtn.Text = "validate";
            this.validateBtn.ToolTipText = "Build (validate) current script\r\nShortcut: Ctrl+Shift+B";
            this.validateBtn.Click += new System.EventHandler(this.validateBtn_Click);
            // 
            // stopBtn
            // 
            this.stopBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.stopBtn.Image = global::CSScriptNpp.Resources.Resources.stop;
            this.stopBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.stopBtn.Name = "stopBtn";
            this.stopBtn.Size = new System.Drawing.Size(23, 22);
            this.stopBtn.Text = "stop";
            this.stopBtn.ToolTipText = "Stop running script";
            this.stopBtn.Visible = false;
            this.stopBtn.Click += new System.EventHandler(this.stopBtn_Click);
            // 
            // runBtn
            // 
            this.runBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.runBtn.Image = global::CSScriptNpp.Resources.Resources.run;
            this.runBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.runBtn.Name = "runBtn";
            this.runBtn.Size = new System.Drawing.Size(23, 22);
            this.runBtn.Text = "run";
            this.runBtn.ToolTipText = "Run current script\r\nShortcut: F5";
            this.runBtn.Click += new System.EventHandler(this.runBtn_Click);
            // 
            // debugBtn
            // 
            this.debugBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.debugBtn.Image = global::CSScriptNpp.Resources.Resources.debug;
            this.debugBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.debugBtn.Name = "debugBtn";
            this.debugBtn.Size = new System.Drawing.Size(23, 22);
            this.debugBtn.Text = "Debug";
            this.debugBtn.ToolTipText = "Debug script with the syestem default debugger";
            this.debugBtn.Click += new System.EventHandler(this.debugBtn_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // reloadBtn
            // 
            this.reloadBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.reloadBtn.Image = global::CSScriptNpp.Resources.Resources.reload;
            this.reloadBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.reloadBtn.Name = "reloadBtn";
            this.reloadBtn.Size = new System.Drawing.Size(23, 22);
            this.reloadBtn.Text = "reload";
            this.reloadBtn.ToolTipText = "Reload current script";
            this.reloadBtn.Click += new System.EventHandler(this.reloadBtn_Click);
            // 
            // loadBtn
            // 
            this.loadBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.loadBtn.Image = global::CSScriptNpp.Resources.Resources.load;
            this.loadBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.loadBtn.Name = "loadBtn";
            this.loadBtn.Size = new System.Drawing.Size(23, 22);
            this.loadBtn.Text = "load";
            this.loadBtn.ToolTipText = "Load script from current document\r\nShortcut: Ctrl+F7";
            this.loadBtn.Click += new System.EventHandler(this.loadBtn_Click);
            // 
            // synchBtn
            // 
            this.synchBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.synchBtn.Image = global::CSScriptNpp.Resources.Resources.synch;
            this.synchBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.synchBtn.Name = "synchBtn";
            this.synchBtn.Size = new System.Drawing.Size(23, 22);
            this.synchBtn.Text = "synch";
            this.synchBtn.ToolTipText = "Synch with current document";
            this.synchBtn.Click += new System.EventHandler(this.synchBtn_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // outputBtn
            // 
            this.outputBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.outputBtn.Image = global::CSScriptNpp.Resources.Resources.outputpanel;
            this.outputBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.outputBtn.Name = "outputBtn";
            this.outputBtn.Size = new System.Drawing.Size(23, 22);
            this.outputBtn.Text = "Toggle output panel visibility";
            this.outputBtn.Click += new System.EventHandler(this.outputBtn_Click);
            // 
            // openInVsBtn
            // 
            this.openInVsBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.openInVsBtn.Image = global::CSScriptNpp.Resources.Resources.vs;
            this.openInVsBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openInVsBtn.Name = "openInVsBtn";
            this.openInVsBtn.Size = new System.Drawing.Size(23, 22);
            this.openInVsBtn.Text = "openInVsBtn";
            this.openInVsBtn.ToolTipText = "Open current script in Visual Studio";
            this.openInVsBtn.Click += new System.EventHandler(this.openInVsBtn_Click);
            // 
            // aboutBtn
            // 
            this.aboutBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.aboutBtn.Image = global::CSScriptNpp.Resources.Resources.about;
            this.aboutBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.aboutBtn.Name = "aboutBtn";
            this.aboutBtn.Size = new System.Drawing.Size(23, 22);
            this.aboutBtn.Text = "aboutBtn";
            this.aboutBtn.ToolTipText = "Abut CS-Script plugin";
            this.aboutBtn.Click += new System.EventHandler(this.aboutBtn_Click);
            // 
            // helpBtn
            // 
            this.helpBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.helpBtn.Image = global::CSScriptNpp.Resources.Resources.Help;
            this.helpBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.helpBtn.Name = "helpBtn";
            this.helpBtn.Size = new System.Drawing.Size(23, 22);
            this.helpBtn.Text = "help";
            this.helpBtn.ToolTipText = "Show Help";
            this.helpBtn.Visible = false;
            this.helpBtn.Click += new System.EventHandler(this.hlpBtn_Click);
            // 
            // solutionContextMenu
            // 
            this.solutionContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.unloadScriptToolStripMenuItem,
            this.openCommandPromptToolStripMenuItem});
            this.solutionContextMenu.Name = "solutionContextMenu";
            this.solutionContextMenu.Size = new System.Drawing.Size(207, 48);
            // 
            // unloadScriptToolStripMenuItem
            // 
            this.unloadScriptToolStripMenuItem.Name = "unloadScriptToolStripMenuItem";
            this.unloadScriptToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.unloadScriptToolStripMenuItem.Text = "&Unload Script";
            this.unloadScriptToolStripMenuItem.Click += new System.EventHandler(this.unloadScriptToolStripMenuItem_Click);
            // 
            // openCommandPromptToolStripMenuItem
            // 
            this.openCommandPromptToolStripMenuItem.Name = "openCommandPromptToolStripMenuItem";
            this.openCommandPromptToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.openCommandPromptToolStripMenuItem.Text = "Open &Command Prompt";
            this.openCommandPromptToolStripMenuItem.Click += new System.EventHandler(this.openCommandPromptToolStripMenuItem_Click);
            // 
            // itemContextMenu
            // 
            this.itemContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openContainingFolderToolStripMenuItem});
            this.itemContextMenu.Name = "itemContextMenu";
            this.itemContextMenu.Size = new System.Drawing.Size(202, 26);
            // 
            // openContainingFolderToolStripMenuItem
            // 
            this.openContainingFolderToolStripMenuItem.Name = "openContainingFolderToolStripMenuItem";
            this.openContainingFolderToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.openContainingFolderToolStripMenuItem.Text = "Open Containing &Folder";
            this.openContainingFolderToolStripMenuItem.Click += new System.EventHandler(this.openContainingFolderToolStripMenuItem_Click);
            // 
            // ProjectPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(396, 242);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.treeView1);
            this.KeyPreview = true;
            this.Name = "ProjectPanel";
            this.Text = "ManageScripts";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ProjectPanel_KeyDown);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.solutionContextMenu.ResumeLayout(false);
            this.itemContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton newBtn;
        private System.Windows.Forms.ToolStripButton reloadBtn;
        private System.Windows.Forms.ToolStripButton validateBtn;
        private System.Windows.Forms.ToolStripButton runBtn;
        private System.Windows.Forms.ToolStripButton loadBtn;
        private System.Windows.Forms.ToolStripButton outputBtn;
        private System.Windows.Forms.ToolStripButton synchBtn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton aboutBtn;
        private System.Windows.Forms.ToolStripButton openInVsBtn;
        private System.Windows.Forms.ToolStripButton helpBtn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton stopBtn;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ContextMenuStrip solutionContextMenu;
        private System.Windows.Forms.ToolStripMenuItem unloadScriptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openCommandPromptToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip itemContextMenu;
        private System.Windows.Forms.ToolStripMenuItem openContainingFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton debugBtn;
    }
}