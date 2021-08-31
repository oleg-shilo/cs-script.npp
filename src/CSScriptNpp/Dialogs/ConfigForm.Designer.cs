namespace CSScriptNpp
{
    partial class ConfigForm
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
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.useCustomLauncherCmd = new System.Windows.Forms.TextBox();
            this.useCustomLauncher = new System.Windows.Forms.CheckBox();
            this.customSyntaxerExe = new System.Windows.Forms.TextBox();
            this.customSyntaxer = new System.Windows.Forms.CheckBox();
            this.checkUpdates = new System.Windows.Forms.CheckBox();
            this.useCS6 = new System.Windows.Forms.CheckBox();
            this.contentControl = new System.Windows.Forms.TabControl();
            this.generalPage = new System.Windows.Forms.TabPage();
            this.restorePanels = new System.Windows.Forms.CheckBox();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.enableNetCore = new System.Windows.Forms.LinkLabel();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.syntaxerPort = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.scriptsDir = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.customEngineLocation = new System.Windows.Forms.TextBox();
            this.installedEngineLocation = new System.Windows.Forms.TextBox();
            this.customEngine = new System.Windows.Forms.RadioButton();
            this.installedEngine = new System.Windows.Forms.RadioButton();
            this.embeddedEngine = new System.Windows.Forms.RadioButton();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.cssyntaxerInstallCmd = new System.Windows.Forms.TextBox();
            this.cssInstallCmd = new System.Windows.Forms.TextBox();
            this.linkLabel4 = new System.Windows.Forms.LinkLabel();
            this.linkLabel3 = new System.Windows.Forms.LinkLabel();
            this.deploySyntaxer = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.deployCSScript = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.customUpdateUrl = new System.Windows.Forms.TextBox();
            this.update = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.contentControl.SuspendLayout();
            this.generalPage.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // useCustomLauncherCmd
            // 
            this.useCustomLauncherCmd.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.useCustomLauncherCmd.Location = new System.Drawing.Point(166, 194);
            this.useCustomLauncherCmd.Name = "useCustomLauncherCmd";
            this.useCustomLauncherCmd.Size = new System.Drawing.Size(241, 20);
            this.useCustomLauncherCmd.TabIndex = 9;
            this.toolTip1.SetToolTip(this.useCustomLauncherCmd, "Custom launcher path with the arguments.\r\n(e.g. \r\n    \'\"my launcher.exe\" -run \"%1" +
        "\" -debug\'\r\n    \'my_launcher.exe -run\'\r\n    \'%CSSCRIPT_DIR%/cscs.exe\'\r\n)");
            this.useCustomLauncherCmd.Visible = false;
            // 
            // useCustomLauncher
            // 
            this.useCustomLauncher.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.useCustomLauncher.AutoSize = true;
            this.useCustomLauncher.Location = new System.Drawing.Point(13, 188);
            this.useCustomLauncher.Name = "useCustomLauncher";
            this.useCustomLauncher.Size = new System.Drawing.Size(126, 17);
            this.useCustomLauncher.TabIndex = 10;
            this.useCustomLauncher.Text = "Custom (F5) launcher";
            this.toolTip1.SetToolTip(this.useCustomLauncher, "Custom launcher path with the arguments.\r\n(e.g. \r\n    \"my launcher.exe\" -run \"%1\"" +
        " -debug\r\n    \"my launcher.exe\" -run\r\n    \"%CSSCRIPT_DIR%/cscs.exe\"\r\n    css.exe\r" +
        "\n)");
            this.useCustomLauncher.UseVisualStyleBackColor = true;
            this.useCustomLauncher.Visible = false;
            // 
            // customSyntaxerExe
            // 
            this.customSyntaxerExe.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.customSyntaxerExe.Location = new System.Drawing.Point(6, 41);
            this.customSyntaxerExe.Name = "customSyntaxerExe";
            this.customSyntaxerExe.Size = new System.Drawing.Size(392, 20);
            this.customSyntaxerExe.TabIndex = 4;
            this.toolTip1.SetToolTip(this.customSyntaxerExe, "Path to the custom syntaxer\r\n");
            // 
            // customSyntaxer
            // 
            this.customSyntaxer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.customSyntaxer.Location = new System.Drawing.Point(6, 20);
            this.customSyntaxer.Name = "customSyntaxer";
            this.customSyntaxer.Size = new System.Drawing.Size(147, 21);
            this.customSyntaxer.TabIndex = 5;
            this.customSyntaxer.Text = "Custom Location";
            this.toolTip1.SetToolTip(this.customSyntaxer, "Custom launcher path with the arguments.\r\n(e.g. \r\n    \"my launcher.exe\" -run \"%1\"" +
        " -debug\r\n    \"my launcher.exe\" -run\r\n    \"%CSSCRIPT_DIR%/cscs.exe\"\r\n    css.exe\r" +
        "\n)");
            this.customSyntaxer.UseVisualStyleBackColor = true;
            this.customSyntaxer.CheckedChanged += new System.EventHandler(this.customSyntaxer_CheckedChanged);
            // 
            // checkUpdates
            // 
            this.checkUpdates.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkUpdates.AutoSize = true;
            this.checkUpdates.Location = new System.Drawing.Point(13, 143);
            this.checkUpdates.Name = "checkUpdates";
            this.checkUpdates.Size = new System.Drawing.Size(160, 17);
            this.checkUpdates.TabIndex = 7;
            this.checkUpdates.Text = "Check for updates at startup";
            this.checkUpdates.UseVisualStyleBackColor = true;
            // 
            // useCS6
            // 
            this.useCS6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.useCS6.AutoSize = true;
            this.useCS6.Location = new System.Drawing.Point(209, -44);
            this.useCS6.Name = "useCS6";
            this.useCS6.Size = new System.Drawing.Size(222, 17);
            this.useCS6.TabIndex = 7;
            this.useCS6.Text = "Script execution - handle C# 6.0  (Roslyn)";
            this.useCS6.UseVisualStyleBackColor = true;
            this.useCS6.Visible = false;
            // 
            // contentControl
            // 
            this.contentControl.Controls.Add(this.generalPage);
            this.contentControl.Controls.Add(this.tabPage2);
            this.contentControl.Controls.Add(this.tabPage1);
            this.contentControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.contentControl.Location = new System.Drawing.Point(0, 0);
            this.contentControl.Name = "contentControl";
            this.contentControl.SelectedIndex = 0;
            this.contentControl.Size = new System.Drawing.Size(429, 299);
            this.contentControl.TabIndex = 8;
            // 
            // generalPage
            // 
            this.generalPage.Controls.Add(this.useCustomLauncherCmd);
            this.generalPage.Controls.Add(this.useCustomLauncher);
            this.generalPage.Controls.Add(this.restorePanels);
            this.generalPage.Controls.Add(this.checkUpdates);
            this.generalPage.Controls.Add(this.useCS6);
            this.generalPage.Controls.Add(this.linkLabel2);
            this.generalPage.Controls.Add(this.enableNetCore);
            this.generalPage.Controls.Add(this.linkLabel1);
            this.generalPage.Location = new System.Drawing.Point(4, 22);
            this.generalPage.Name = "generalPage";
            this.generalPage.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.generalPage.Size = new System.Drawing.Size(421, 273);
            this.generalPage.TabIndex = 0;
            this.generalPage.Text = "General";
            this.generalPage.UseVisualStyleBackColor = true;
            // 
            // restorePanels
            // 
            this.restorePanels.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.restorePanels.AutoSize = true;
            this.restorePanels.Location = new System.Drawing.Point(13, 165);
            this.restorePanels.Name = "restorePanels";
            this.restorePanels.Size = new System.Drawing.Size(147, 17);
            this.restorePanels.TabIndex = 8;
            this.restorePanels.Text = "Restore panels on startup";
            this.restorePanels.UseVisualStyleBackColor = true;
            // 
            // linkLabel2
            // 
            this.linkLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.Location = new System.Drawing.Point(10, 255);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(167, 13);
            this.linkLabel2.TabIndex = 5;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "Edit Visual Studio project template";
            this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // enableNetCore
            // 
            this.enableNetCore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.enableNetCore.AutoSize = true;
            this.enableNetCore.Location = new System.Drawing.Point(10, 208);
            this.enableNetCore.Name = "enableNetCore";
            this.enableNetCore.Size = new System.Drawing.Size(156, 13);
            this.enableNetCore.TabIndex = 5;
            this.enableNetCore.TabStop = true;
            this.enableNetCore.Text = "Enable .NET 5/Core integration";
            this.enableNetCore.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.enableNetCore_LinkClicked);
            // 
            // linkLabel1
            // 
            this.linkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(10, 231);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(120, 13);
            this.linkLabel1.TabIndex = 5;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Edit settings file  instead";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.groupBox3);
            this.tabPage2.Controls.Add(this.label1);
            this.tabPage2.Controls.Add(this.scriptsDir);
            this.tabPage2.Controls.Add(this.groupBox1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.tabPage2.Size = new System.Drawing.Size(421, 273);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "CS-Script";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.customSyntaxer);
            this.groupBox3.Controls.Add(this.syntaxerPort);
            this.groupBox3.Controls.Add(this.customSyntaxerExe);
            this.groupBox3.Location = new System.Drawing.Point(9, 185);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(404, 70);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Syntaxer";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(301, 17);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Port:";
            // 
            // syntaxerPort
            // 
            this.syntaxerPort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.syntaxerPort.Location = new System.Drawing.Point(336, 14);
            this.syntaxerPort.Name = "syntaxerPort";
            this.syntaxerPort.Size = new System.Drawing.Size(62, 20);
            this.syntaxerPort.TabIndex = 6;
            this.syntaxerPort.Text = "18001";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Scripts Directory:";
            // 
            // scriptsDir
            // 
            this.scriptsDir.Location = new System.Drawing.Point(104, 9);
            this.scriptsDir.Name = "scriptsDir";
            this.scriptsDir.Size = new System.Drawing.Size(305, 20);
            this.scriptsDir.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.customEngineLocation);
            this.groupBox1.Controls.Add(this.installedEngineLocation);
            this.groupBox1.Controls.Add(this.customEngine);
            this.groupBox1.Controls.Add(this.installedEngine);
            this.groupBox1.Controls.Add(this.embeddedEngine);
            this.groupBox1.Location = new System.Drawing.Point(9, 35);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(404, 144);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Engine Location";
            // 
            // customEngineLocation
            // 
            this.customEngineLocation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.customEngineLocation.Location = new System.Drawing.Point(43, 110);
            this.customEngineLocation.Name = "customEngineLocation";
            this.customEngineLocation.Size = new System.Drawing.Size(349, 20);
            this.customEngineLocation.TabIndex = 1;
            // 
            // installedEngineLocation
            // 
            this.installedEngineLocation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.installedEngineLocation.Location = new System.Drawing.Point(43, 64);
            this.installedEngineLocation.Name = "installedEngineLocation";
            this.installedEngineLocation.ReadOnly = true;
            this.installedEngineLocation.Size = new System.Drawing.Size(349, 20);
            this.installedEngineLocation.TabIndex = 1;
            // 
            // customEngine
            // 
            this.customEngine.AutoSize = true;
            this.customEngine.Location = new System.Drawing.Point(22, 87);
            this.customEngine.Name = "customEngine";
            this.customEngine.Size = new System.Drawing.Size(104, 17);
            this.customEngine.TabIndex = 0;
            this.customEngine.Text = "Custom Location";
            this.customEngine.UseVisualStyleBackColor = true;
            this.customEngine.CheckedChanged += new System.EventHandler(this.engine_CheckedChanged);
            // 
            // installedEngine
            // 
            this.installedEngine.AutoSize = true;
            this.installedEngine.Location = new System.Drawing.Point(22, 41);
            this.installedEngine.Name = "installedEngine";
            this.installedEngine.Size = new System.Drawing.Size(226, 17);
            this.installedEngine.TabIndex = 0;
            this.installedEngine.Text = "Installed CS-Script (%CSSCRIPT_ROOT%)";
            this.installedEngine.UseVisualStyleBackColor = true;
            this.installedEngine.CheckedChanged += new System.EventHandler(this.engine_CheckedChanged);
            // 
            // embeddedEngine
            // 
            this.embeddedEngine.AutoSize = true;
            this.embeddedEngine.Location = new System.Drawing.Point(22, 17);
            this.embeddedEngine.Name = "embeddedEngine";
            this.embeddedEngine.Size = new System.Drawing.Size(76, 17);
            this.embeddedEngine.TabIndex = 0;
            this.embeddedEngine.Text = "Embedded";
            this.embeddedEngine.UseVisualStyleBackColor = true;
            this.embeddedEngine.CheckedChanged += new System.EventHandler(this.engine_CheckedChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.groupBox4);
            this.tabPage1.Controls.Add(this.groupBox2);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.tabPage1.Size = new System.Drawing.Size(421, 273);
            this.tabPage1.TabIndex = 2;
            this.tabPage1.Text = "Update";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.cssyntaxerInstallCmd);
            this.groupBox4.Controls.Add(this.cssInstallCmd);
            this.groupBox4.Controls.Add(this.linkLabel4);
            this.groupBox4.Controls.Add(this.linkLabel3);
            this.groupBox4.Controls.Add(this.deploySyntaxer);
            this.groupBox4.Controls.Add(this.label5);
            this.groupBox4.Controls.Add(this.deployCSScript);
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Location = new System.Drawing.Point(8, 127);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(399, 141);
            this.groupBox4.TabIndex = 5;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Services Update";
            // 
            // cssyntaxerInstallCmd
            // 
            this.cssyntaxerInstallCmd.Location = new System.Drawing.Point(15, 111);
            this.cssyntaxerInstallCmd.Name = "cssyntaxerInstallCmd";
            this.cssyntaxerInstallCmd.ReadOnly = true;
            this.cssyntaxerInstallCmd.Size = new System.Drawing.Size(274, 20);
            this.cssyntaxerInstallCmd.TabIndex = 8;
            this.cssyntaxerInstallCmd.Text = "choco install cs-syntaxer --y";
            // 
            // cssInstallCmd
            // 
            this.cssInstallCmd.Location = new System.Drawing.Point(15, 49);
            this.cssInstallCmd.Name = "cssInstallCmd";
            this.cssInstallCmd.ReadOnly = true;
            this.cssInstallCmd.Size = new System.Drawing.Size(274, 20);
            this.cssInstallCmd.TabIndex = 7;
            this.cssInstallCmd.Text = "choco install cs-script --y";
            // 
            // linkLabel4
            // 
            this.linkLabel4.AutoSize = true;
            this.linkLabel4.Location = new System.Drawing.Point(293, 91);
            this.linkLabel4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.linkLabel4.Name = "linkLabel4";
            this.linkLabel4.Size = new System.Drawing.Size(59, 13);
            this.linkLabel4.TabIndex = 6;
            this.linkLabel4.TabStop = true;
            this.linkLabel4.Text = "Read more";
            this.linkLabel4.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel4_LinkClicked);
            // 
            // linkLabel3
            // 
            this.linkLabel3.AutoSize = true;
            this.linkLabel3.Location = new System.Drawing.Point(293, 26);
            this.linkLabel3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.linkLabel3.Name = "linkLabel3";
            this.linkLabel3.Size = new System.Drawing.Size(59, 13);
            this.linkLabel3.TabIndex = 6;
            this.linkLabel3.TabStop = true;
            this.linkLabel3.Text = "Read more";
            this.linkLabel3.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel3_LinkClicked);
            // 
            // deploySyntaxer
            // 
            this.deploySyntaxer.Location = new System.Drawing.Point(295, 109);
            this.deploySyntaxer.Name = "deploySyntaxer";
            this.deploySyntaxer.Size = new System.Drawing.Size(98, 23);
            this.deploySyntaxer.TabIndex = 5;
            this.deploySyntaxer.Text = "Deploy/Update";
            this.deploySyntaxer.UseVisualStyleBackColor = true;
            this.deploySyntaxer.Click += new System.EventHandler(this.deploySyntaxer_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 91);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(144, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Latest Syntaxer (Intellisense):";
            // 
            // deployCSScript
            // 
            this.deployCSScript.Location = new System.Drawing.Point(295, 47);
            this.deployCSScript.Name = "deployCSScript";
            this.deployCSScript.Size = new System.Drawing.Size(98, 23);
            this.deployCSScript.TabIndex = 3;
            this.deployCSScript.Text = "Deploy/Update";
            this.deployCSScript.UseVisualStyleBackColor = true;
            this.deployCSScript.Click += new System.EventHandler(this.deployCSScript_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 26);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(155, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Latest CS-Script (script engine):";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.customUpdateUrl);
            this.groupBox2.Controls.Add(this.update);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Location = new System.Drawing.Point(8, 21);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(399, 80);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Plugin Update";
            // 
            // customUpdateUrl
            // 
            this.customUpdateUrl.Location = new System.Drawing.Point(15, 44);
            this.customUpdateUrl.Name = "customUpdateUrl";
            this.customUpdateUrl.Size = new System.Drawing.Size(297, 20);
            this.customUpdateUrl.TabIndex = 4;
            // 
            // update
            // 
            this.update.Location = new System.Drawing.Point(318, 43);
            this.update.Name = "update";
            this.update.Size = new System.Drawing.Size(75, 23);
            this.update.TabIndex = 3;
            this.update.Text = "Update";
            this.update.UseVisualStyleBackColor = true;
            this.update.Click += new System.EventHandler(this.update_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(116, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Custom update source:";
            // 
            // ConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(429, 299);
            this.Controls.Add(this.contentControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.KeyPreview = true;
            this.Name = "ConfigForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "CS-Script Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ConfigForm_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ConfigForm_KeyDown);
            this.contentControl.ResumeLayout(false);
            this.generalPage.ResumeLayout(false);
            this.generalPage.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabPage1.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.CheckBox checkUpdates;
        private System.Windows.Forms.CheckBox useCS6;
        private System.Windows.Forms.TabControl contentControl;
        private System.Windows.Forms.TabPage generalPage;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton customEngine;
        private System.Windows.Forms.RadioButton installedEngine;
        private System.Windows.Forms.RadioButton embeddedEngine;
        private System.Windows.Forms.TextBox customEngineLocation;
        private System.Windows.Forms.TextBox installedEngineLocation;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.CheckBox restorePanels;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox scriptsDir;
        private System.Windows.Forms.LinkLabel linkLabel2;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button update;
        private System.Windows.Forms.TextBox customUpdateUrl;
        private System.Windows.Forms.TextBox useCustomLauncherCmd;
        private System.Windows.Forms.CheckBox useCustomLauncher;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox customSyntaxer;
        private System.Windows.Forms.TextBox syntaxerPort;
        private System.Windows.Forms.TextBox customSyntaxerExe;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button deployCSScript;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.LinkLabel linkLabel4;
        private System.Windows.Forms.LinkLabel linkLabel3;
        private System.Windows.Forms.Button deploySyntaxer;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox cssyntaxerInstallCmd;
        private System.Windows.Forms.TextBox cssInstallCmd;
        private System.Windows.Forms.LinkLabel enableNetCore;
    }
}