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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolbarContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.organizeButtonsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newBtn = new System.Windows.Forms.ToolStripButton();
            this.historyBtn = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.validateBtn = new System.Windows.Forms.ToolStripButton();
            this.stopBtn = new System.Windows.Forms.ToolStripButton();
            this.runBtn = new System.Windows.Forms.ToolStripButton();
            this.debugBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.loadBtn = new System.Windows.Forms.ToolStripButton();
            this.reloadBtn = new System.Windows.Forms.ToolStripButton();
            this.synchBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.outputBtn = new System.Windows.Forms.ToolStripButton();
            this.openInVsBtn = new System.Windows.Forms.ToolStripButton();
            this.aboutBtn = new System.Windows.Forms.ToolStripButton();
            this.helpBtn = new System.Windows.Forms.ToolStripButton();
            this.openScriptsFolderBtn = new System.Windows.Forms.ToolStripButton();
            this.configBtn = new System.Windows.Forms.ToolStripButton();
            this.deployBtn = new System.Windows.Forms.ToolStripButton();
            this.shortcutsBtn = new System.Windows.Forms.ToolStripButton();
            this.restartNppBtn = new System.Windows.Forms.ToolStripButton();
            this.favoritesBtn = new System.Windows.Forms.ToolStripButton();
            this.solutionContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.unloadScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openCommandPromptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.itemContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openContainingFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.whatsNewTxt = new System.Windows.Forms.TextBox();
            this.whatsNewPanel = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.toolStrip1.SuspendLayout();
            this.toolbarContextMenuStrip.SuspendLayout();
            this.solutionContextMenu.SuspendLayout();
            this.itemContextMenu.SuspendLayout();
            this.whatsNewPanel.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(932, -1);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(32, 21);
            this.pictureBox1.TabIndex = 11;
            this.pictureBox1.TabStop = false;
            this.toolTip1.SetToolTip(this.pictureBox1, "Close \"What\'s new\" panel");
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // treeView1
            // 
            this.treeView1.BackColor = System.Drawing.Color.White;
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.HideSelection = false;
            this.treeView1.ImageIndex = 0;
            this.treeView1.ImageList = this.imageList1;
            this.treeView1.Location = new System.Drawing.Point(3, 3);
            this.treeView1.Name = "treeView1";
            this.treeView1.SelectedImageIndex = 0;
            this.treeView1.ShowNodeToolTips = true;
            this.treeView1.ShowRootLines = false;
            this.treeView1.Size = new System.Drawing.Size(975, 423);
            this.treeView1.TabIndex = 3;
            this.treeView1.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterCollapse);
            this.treeView1.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterExpand);
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            this.treeView1.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView1_NodeMouseClick);
            this.treeView1.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView1_NodeMouseDoubleClick);
            this.treeView1.SizeChanged += new System.EventHandler(this.treeView1_SizeChanged);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "folder_opened.png");
            this.imageList1.Images.SetKeyName(1, "css_logo_16x16_tb.png");
            this.imageList1.Images.SetKeyName(2, "references.png");
            this.imageList1.Images.SetKeyName(3, "includes.png");
            this.imageList1.Images.SetKeyName(4, "css_logo_vb_16x16_tb.png");
            // 
            // toolStrip1
            // 
            this.toolStrip1.ContextMenuStrip = this.toolbarContextMenuStrip;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newBtn,
            this.historyBtn,
            this.validateBtn,
            this.stopBtn,
            this.runBtn,
            this.debugBtn,
            this.toolStripSeparator2,
            this.loadBtn,
            this.reloadBtn,
            this.synchBtn,
            this.toolStripSeparator1,
            this.outputBtn,
            this.openInVsBtn,
            this.aboutBtn,
            this.helpBtn,
            this.openScriptsFolderBtn,
            this.configBtn,
            this.deployBtn,
            this.shortcutsBtn,
            this.restartNppBtn,
            this.favoritesBtn});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(989, 31);
            this.toolStrip1.TabIndex = 4;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolbarContextMenuStrip
            // 
            this.toolbarContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.organizeButtonsToolStripMenuItem});
            this.toolbarContextMenuStrip.Name = "toolbarContextMenuStrip";
            this.toolbarContextMenuStrip.Size = new System.Drawing.Size(166, 26);
            // 
            // organizeButtonsToolStripMenuItem
            // 
            this.organizeButtonsToolStripMenuItem.Name = "organizeButtonsToolStripMenuItem";
            this.organizeButtonsToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            this.organizeButtonsToolStripMenuItem.Text = "Organize buttons";
            this.organizeButtonsToolStripMenuItem.Click += new System.EventHandler(this.organizeButtonsToolStripMenuItem_Click);
            // 
            // newBtn
            // 
            this.newBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.newBtn.Image = global::CSScriptNpp.Resources.Resources.add;
            this.newBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.newBtn.Name = "newBtn";
            this.newBtn.Size = new System.Drawing.Size(23, 28);
            this.newBtn.Text = "New";
            this.newBtn.ToolTipText = "Create new script";
            this.newBtn.Click += new System.EventHandler(this.newBtn_Click);
            // 
            // historyBtn
            // 
            this.historyBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.historyBtn.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator3});
            this.historyBtn.Image = global::CSScriptNpp.Resources.Resources.history;
            this.historyBtn.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.historyBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.historyBtn.Name = "historyBtn";
            this.historyBtn.Size = new System.Drawing.Size(37, 28);
            this.historyBtn.Text = "Recent";
            this.historyBtn.ToolTipText = "Recent Scripts";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(57, 6);
            // 
            // validateBtn
            // 
            this.validateBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.validateBtn.Image = global::CSScriptNpp.Resources.Resources.check;
            this.validateBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.validateBtn.Name = "validateBtn";
            this.validateBtn.Size = new System.Drawing.Size(23, 28);
            this.validateBtn.Text = "Validate";
            this.validateBtn.ToolTipText = "Build (validate) current script";
            this.validateBtn.Click += new System.EventHandler(this.validateBtn_Click);
            // 
            // stopBtn
            // 
            this.stopBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.stopBtn.Image = global::CSScriptNpp.Resources.Resources.stop;
            this.stopBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.stopBtn.Name = "stopBtn";
            this.stopBtn.Size = new System.Drawing.Size(23, 28);
            this.stopBtn.Text = "Stop";
            this.stopBtn.ToolTipText = "Stop running script";
            this.stopBtn.Click += new System.EventHandler(this.stopBtn_Click);
            // 
            // runBtn
            // 
            this.runBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.runBtn.Image = global::CSScriptNpp.Resources.Resources.run;
            this.runBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.runBtn.Name = "runBtn";
            this.runBtn.Size = new System.Drawing.Size(23, 28);
            this.runBtn.Text = "Run";
            this.runBtn.ToolTipText = "Run current script";
            this.runBtn.Click += new System.EventHandler(this.runBtn_Click);
            // 
            // debugBtn
            // 
            this.debugBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.debugBtn.Image = global::CSScriptNpp.Resources.Resources.debug;
            this.debugBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.debugBtn.Name = "debugBtn";
            this.debugBtn.Size = new System.Drawing.Size(23, 28);
            this.debugBtn.Text = "Debug";
            this.debugBtn.ToolTipText = "Debug script with the system default debugger";
            this.debugBtn.Click += new System.EventHandler(this.debugBtn_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 31);
            // 
            // loadBtn
            // 
            this.loadBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.loadBtn.Image = global::CSScriptNpp.Resources.Resources.load;
            this.loadBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.loadBtn.Name = "loadBtn";
            this.loadBtn.Size = new System.Drawing.Size(23, 28);
            this.loadBtn.Text = "load";
            this.loadBtn.ToolTipText = "Load script from current document";
            this.loadBtn.Click += new System.EventHandler(this.loadBtn_Click);
            // 
            // reloadBtn
            // 
            this.reloadBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.reloadBtn.Image = global::CSScriptNpp.Resources.Resources.reload;
            this.reloadBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.reloadBtn.Name = "reloadBtn";
            this.reloadBtn.Size = new System.Drawing.Size(23, 28);
            this.reloadBtn.Text = "reload";
            this.reloadBtn.ToolTipText = "Reload current script";
            this.reloadBtn.Click += new System.EventHandler(this.reloadBtn_Click);
            // 
            // synchBtn
            // 
            this.synchBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.synchBtn.Image = global::CSScriptNpp.Resources.Resources.synch;
            this.synchBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.synchBtn.Name = "synchBtn";
            this.synchBtn.Size = new System.Drawing.Size(23, 28);
            this.synchBtn.Text = "synch";
            this.synchBtn.ToolTipText = "Synch with current document";
            this.synchBtn.Click += new System.EventHandler(this.synchBtn_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 31);
            // 
            // outputBtn
            // 
            this.outputBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.outputBtn.Image = global::CSScriptNpp.Resources.Resources.outputpanel;
            this.outputBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.outputBtn.Name = "outputBtn";
            this.outputBtn.Size = new System.Drawing.Size(23, 28);
            this.outputBtn.Text = "Show/Hide secondary panels";
            this.outputBtn.Click += new System.EventHandler(this.outputBtn_Click);
            // 
            // openInVsBtn
            // 
            this.openInVsBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.openInVsBtn.Image = global::CSScriptNpp.Resources.Resources.vs;
            this.openInVsBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openInVsBtn.Name = "openInVsBtn";
            this.openInVsBtn.Size = new System.Drawing.Size(23, 28);
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
            this.aboutBtn.Size = new System.Drawing.Size(23, 28);
            this.aboutBtn.Text = "aboutBtn";
            this.aboutBtn.ToolTipText = "About CS-Script plugin";
            this.aboutBtn.Click += new System.EventHandler(this.aboutBtn_Click);
            // 
            // helpBtn
            // 
            this.helpBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.helpBtn.Image = global::CSScriptNpp.Resources.Resources.Help;
            this.helpBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.helpBtn.Name = "helpBtn";
            this.helpBtn.Size = new System.Drawing.Size(23, 28);
            this.helpBtn.Text = "help";
            this.helpBtn.ToolTipText = "Show Help";
            this.helpBtn.Visible = false;
            this.helpBtn.Click += new System.EventHandler(this.hlpBtn_Click);
            // 
            // openScriptsFolderBtn
            // 
            this.openScriptsFolderBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.openScriptsFolderBtn.Image = ((System.Drawing.Image)(resources.GetObject("openScriptsFolderBtn.Image")));
            this.openScriptsFolderBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openScriptsFolderBtn.Name = "openScriptsFolderBtn";
            this.openScriptsFolderBtn.Size = new System.Drawing.Size(23, 28);
            this.openScriptsFolderBtn.Text = "scriptsFolder";
            this.openScriptsFolderBtn.ToolTipText = "Open Scripts Default Folder";
            this.openScriptsFolderBtn.Click += new System.EventHandler(this.openScriptsFolderBtn_Click);
            // 
            // configBtn
            // 
            this.configBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.configBtn.Image = ((System.Drawing.Image)(resources.GetObject("configBtn.Image")));
            this.configBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.configBtn.Name = "configBtn";
            this.configBtn.Size = new System.Drawing.Size(23, 28);
            this.configBtn.Text = "configBtn";
            this.configBtn.ToolTipText = "Show Config Dialog";
            this.configBtn.Click += new System.EventHandler(this.configBtn_Click);
            // 
            // deployBtn
            // 
            this.deployBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.deployBtn.Image = global::CSScriptNpp.Resources.Resources.deploy;
            this.deployBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.deployBtn.Name = "deployBtn";
            this.deployBtn.Size = new System.Drawing.Size(23, 28);
            this.deployBtn.Text = "deployBtn";
            this.deployBtn.ToolTipText = "Prepare script for distribution";
            this.deployBtn.Click += new System.EventHandler(this.deployBtn_Click);
            // 
            // shortcutsBtn
            // 
            this.shortcutsBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.shortcutsBtn.Image = ((System.Drawing.Image)(resources.GetObject("shortcutsBtn.Image")));
            this.shortcutsBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.shortcutsBtn.Name = "shortcutsBtn";
            this.shortcutsBtn.Size = new System.Drawing.Size(23, 28);
            this.shortcutsBtn.Text = "shortcutsBtn";
            this.shortcutsBtn.ToolTipText = "Show Plugin Shortcuts";
            this.shortcutsBtn.Click += new System.EventHandler(this.shortcutsBtn_Click);
            // 
            // restartNppBtn
            // 
            this.restartNppBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.restartNppBtn.Image = global::CSScriptNpp.Resources.Resources.restart_npp;
            this.restartNppBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.restartNppBtn.Name = "restartNppBtn";
            this.restartNppBtn.Size = new System.Drawing.Size(23, 28);
            this.restartNppBtn.Text = "retartNppBtn";
            this.restartNppBtn.ToolTipText = "Restart Notepad++ (Elevated)";
            this.restartNppBtn.Click += new System.EventHandler(this.restartNppBtn_Click);
            // 
            // favoritesBtn
            // 
            this.favoritesBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.favoritesBtn.Image = global::CSScriptNpp.Resources.Resources.favorites;
            this.favoritesBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.favoritesBtn.Name = "favoritesBtn";
            this.favoritesBtn.Size = new System.Drawing.Size(23, 28);
            this.favoritesBtn.Text = "Add to Favorites";
            this.favoritesBtn.Click += new System.EventHandler(this.favoritesBtn_Click);
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
            // whatsNewTxt
            // 
            this.whatsNewTxt.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.whatsNewTxt.BackColor = System.Drawing.Color.White;
            this.whatsNewTxt.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.whatsNewTxt.ForeColor = System.Drawing.Color.DarkBlue;
            this.whatsNewTxt.Location = new System.Drawing.Point(6, 24);
            this.whatsNewTxt.Multiline = true;
            this.whatsNewTxt.Name = "whatsNewTxt";
            this.whatsNewTxt.ReadOnly = true;
            this.whatsNewTxt.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.whatsNewTxt.Size = new System.Drawing.Size(954, 426);
            this.whatsNewTxt.TabIndex = 5;
            // 
            // whatsNewPanel
            // 
            this.whatsNewPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.whatsNewPanel.BackColor = System.Drawing.Color.White;
            this.whatsNewPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.whatsNewPanel.Controls.Add(this.pictureBox1);
            this.whatsNewPanel.Controls.Add(this.label1);
            this.whatsNewPanel.Controls.Add(this.whatsNewTxt);
            this.whatsNewPanel.Location = new System.Drawing.Point(12, 23);
            this.whatsNewPanel.Name = "whatsNewPanel";
            this.whatsNewPanel.Size = new System.Drawing.Size(965, 455);
            this.whatsNewPanel.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Red;
            this.label1.Location = new System.Drawing.Point(0, 2);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(99, 17);
            this.label1.TabIndex = 10;
            this.label1.Text = "What\'s new?";
            // 
            // tabControl1
            // 
            this.tabControl1.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Location = new System.Drawing.Point(0, 34);
            this.tabControl1.Multiline = true;
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(989, 455);
            this.tabControl1.TabIndex = 11;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.White;
            this.tabPage1.Controls.Add(this.treeView1);
            this.tabPage1.Location = new System.Drawing.Point(4, 4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(981, 429);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Script Project";
            // 
            // ProjectPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(989, 490);
            this.Controls.Add(this.whatsNewPanel);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.tabControl1);
            this.KeyPreview = true;
            this.Name = "ProjectPanel";
            this.Text = "CS-Script Scrips Manager";
            this.Deactivate += new System.EventHandler(this.ProjectPanel_Deactivate);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.toolbarContextMenuStrip.ResumeLayout(false);
            this.solutionContextMenu.ResumeLayout(false);
            this.itemContextMenu.ResumeLayout(false);
            this.whatsNewPanel.ResumeLayout(false);
            this.whatsNewPanel.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
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
        private System.Windows.Forms.ToolStripButton openScriptsFolderBtn;
        private System.Windows.Forms.ToolStripButton configBtn;
        private System.Windows.Forms.ToolStripButton deployBtn;
        private System.Windows.Forms.ToolStripButton shortcutsBtn;
        private System.Windows.Forms.ToolStripDropDownButton historyBtn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.TextBox whatsNewTxt;
        private System.Windows.Forms.Panel whatsNewPanel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.ContextMenuStrip toolbarContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem organizeButtonsToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton restartNppBtn;
        private System.Windows.Forms.ToolStripButton favoritesBtn;
    }
}