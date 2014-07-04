namespace CSScriptNpp.Dialogs
{
    partial class UpdateOptionsPanel
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
            this.msiDeployment = new System.Windows.Forms.RadioButton();
            this.customDeployment = new System.Windows.Forms.RadioButton();
            this.optionsGroup = new System.Windows.Forms.GroupBox();
            this.manualDeployment = new System.Windows.Forms.RadioButton();
            this.okBtn = new System.Windows.Forms.Button();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.versionLbl = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.progressLbl = new System.Windows.Forms.Label();
            this.releaseNotes = new System.Windows.Forms.LinkLabel();
            this.showOptions = new System.Windows.Forms.CheckBox();
            this.optionsGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // msiDeployment
            // 
            this.msiDeployment.AutoSize = true;
            this.msiDeployment.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.msiDeployment.Location = new System.Drawing.Point(6, 68);
            this.msiDeployment.Name = "msiDeployment";
            this.msiDeployment.Size = new System.Drawing.Size(248, 43);
            this.msiDeployment.TabIndex = 0;
            this.msiDeployment.Tag = "msi";
            this.msiDeployment.Text = "Download &MSI.\r\nNot suitable for Notepad++ portable installation.\r\nThis option ma" +
    "y require system reboot.";
            this.msiDeployment.UseVisualStyleBackColor = true;
            // 
            // customDeployment
            // 
            this.customDeployment.AutoSize = true;
            this.customDeployment.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.customDeployment.Checked = true;
            this.customDeployment.Location = new System.Drawing.Point(6, 19);
            this.customDeployment.Name = "customDeployment";
            this.customDeployment.Size = new System.Drawing.Size(256, 43);
            this.customDeployment.TabIndex = 1;
            this.customDeployment.TabStop = true;
            this.customDeployment.Tag = "custom";
            this.customDeployment.Text = "Download and &replace binaries (Recommended).\r\nSuitable for any Notepad++ install" +
    "ation.\r\nIt is an equivalent of Notepad++ Plugin Manager.\r\n";
            this.customDeployment.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.customDeployment.UseVisualStyleBackColor = true;
            // 
            // optionsGroup
            // 
            this.optionsGroup.Controls.Add(this.customDeployment);
            this.optionsGroup.Controls.Add(this.manualDeployment);
            this.optionsGroup.Controls.Add(this.msiDeployment);
            this.optionsGroup.Location = new System.Drawing.Point(12, 61);
            this.optionsGroup.Name = "optionsGroup";
            this.optionsGroup.Size = new System.Drawing.Size(425, 164);
            this.optionsGroup.TabIndex = 2;
            this.optionsGroup.TabStop = false;
            // 
            // manualDeployment
            // 
            this.manualDeployment.AutoSize = true;
            this.manualDeployment.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.manualDeployment.Location = new System.Drawing.Point(6, 117);
            this.manualDeployment.Name = "manualDeployment";
            this.manualDeployment.Size = new System.Drawing.Size(287, 30);
            this.manualDeployment.TabIndex = 0;
            this.manualDeployment.Tag = "download";
            this.manualDeployment.Text = "&Download binaries.\r\nThis option requires you to deploy the binaries manually.";
            this.manualDeployment.UseVisualStyleBackColor = true;
            // 
            // okBtn
            // 
            this.okBtn.Location = new System.Drawing.Point(336, 6);
            this.okBtn.Name = "okBtn";
            this.okBtn.Size = new System.Drawing.Size(101, 23);
            this.okBtn.TabIndex = 3;
            this.okBtn.Text = "&Proceed";
            this.okBtn.UseVisualStyleBackColor = true;
            this.okBtn.Click += new System.EventHandler(this.okBtn_Click);
            // 
            // cancelBtn
            // 
            this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelBtn.Location = new System.Drawing.Point(336, 35);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(101, 23);
            this.cancelBtn.TabIndex = 3;
            this.cancelBtn.Text = "&Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Available version: ";
            // 
            // versionLbl
            // 
            this.versionLbl.AutoSize = true;
            this.versionLbl.Location = new System.Drawing.Point(101, 9);
            this.versionLbl.Name = "versionLbl";
            this.versionLbl.Size = new System.Drawing.Size(53, 13);
            this.versionLbl.TabIndex = 4;
            this.versionLbl.Text = "<version>";
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(12, 25);
            this.progressBar.MarqueeAnimationSpeed = 50;
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(316, 10);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar.TabIndex = 5;
            // 
            // progressLbl
            // 
            this.progressLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.progressLbl.Location = new System.Drawing.Point(228, 9);
            this.progressLbl.Name = "progressLbl";
            this.progressLbl.Size = new System.Drawing.Size(97, 13);
            this.progressLbl.TabIndex = 4;
            this.progressLbl.Text = "Downloading";
            this.progressLbl.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.progressLbl.UseMnemonic = false;
            this.progressLbl.Visible = false;
            // 
            // releaseNotes
            // 
            this.releaseNotes.AutoSize = true;
            this.releaseNotes.Location = new System.Drawing.Point(227, 45);
            this.releaseNotes.Name = "releaseNotes";
            this.releaseNotes.Size = new System.Drawing.Size(103, 13);
            this.releaseNotes.TabIndex = 6;
            this.releaseNotes.TabStop = true;
            this.releaseNotes.Text = "View Release Notes";
            this.releaseNotes.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.releaseNotes_LinkClicked);
            // 
            // showOptions
            // 
            this.showOptions.AutoSize = true;
            this.showOptions.Location = new System.Drawing.Point(12, 41);
            this.showOptions.Name = "showOptions";
            this.showOptions.Size = new System.Drawing.Size(138, 17);
            this.showOptions.TabIndex = 7;
            this.showOptions.Text = "Show Updating Options";
            this.showOptions.UseVisualStyleBackColor = true;
            this.showOptions.CheckedChanged += new System.EventHandler(this.showOptions_CheckedChanged);
            // 
            // UpdateOptionsPanel
            // 
            this.AcceptButton = this.okBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelBtn;
            this.ClientSize = new System.Drawing.Size(442, 238);
            this.Controls.Add(this.showOptions);
            this.Controls.Add(this.releaseNotes);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.progressLbl);
            this.Controls.Add(this.versionLbl);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.okBtn);
            this.Controls.Add(this.optionsGroup);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "UpdateOptionsPanel";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "CS-Script Update";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.UpdateOptionsPanel_FormClosed);
            this.optionsGroup.ResumeLayout(false);
            this.optionsGroup.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton msiDeployment;
        private System.Windows.Forms.RadioButton customDeployment;
        private System.Windows.Forms.GroupBox optionsGroup;
        private System.Windows.Forms.RadioButton manualDeployment;
        private System.Windows.Forms.Button okBtn;
        private System.Windows.Forms.Button cancelBtn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label versionLbl;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label progressLbl;
        private System.Windows.Forms.LinkLabel releaseNotes;
        private System.Windows.Forms.CheckBox showOptions;
    }
}