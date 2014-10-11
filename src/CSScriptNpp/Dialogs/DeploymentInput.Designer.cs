namespace CSScriptNpp
{
    partial class DeploymentInput
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
            this.okBtn = new System.Windows.Forms.Button();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.asExe = new System.Windows.Forms.RadioButton();
            this.asScript = new System.Windows.Forms.RadioButton();
            this.versionsList = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.windowApp = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // okBtn
            // 
            this.okBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.okBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okBtn.Location = new System.Drawing.Point(65, 142);
            this.okBtn.Name = "okBtn";
            this.okBtn.Size = new System.Drawing.Size(75, 23);
            this.okBtn.TabIndex = 1;
            this.okBtn.Text = "&Prepare";
            this.okBtn.UseVisualStyleBackColor = true;
            this.okBtn.Click += new System.EventHandler(this.okBtn_Click);
            // 
            // cancelBtn
            // 
            this.cancelBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelBtn.Location = new System.Drawing.Point(161, 142);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(75, 23);
            this.cancelBtn.TabIndex = 1;
            this.cancelBtn.Text = "&Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            // 
            // asExe
            // 
            this.asExe.AutoSize = true;
            this.asExe.Location = new System.Drawing.Point(16, 37);
            this.asExe.Name = "asExe";
            this.asExe.Size = new System.Drawing.Size(146, 17);
            this.asExe.TabIndex = 3;
            this.asExe.Text = "Self-sufficient executable ";
            this.asExe.UseVisualStyleBackColor = true;
            this.asExe.CheckedChanged += new System.EventHandler(this.asExe_CheckedChanged);
            // 
            // asScript
            // 
            this.asScript.AutoSize = true;
            this.asScript.Checked = true;
            this.asScript.Location = new System.Drawing.Point(16, 12);
            this.asScript.Name = "asScript";
            this.asScript.Size = new System.Drawing.Size(124, 17);
            this.asScript.TabIndex = 3;
            this.asScript.TabStop = true;
            this.asScript.Text = "Script + script engine";
            this.asScript.UseVisualStyleBackColor = true;
            // 
            // versionsList
            // 
            this.versionsList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.versionsList.FormattingEnabled = true;
            this.versionsList.Location = new System.Drawing.Point(16, 112);
            this.versionsList.Name = "versionsList";
            this.versionsList.Size = new System.Drawing.Size(271, 21);
            this.versionsList.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 91);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(115, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Target runtime version:";
            // 
            // windowApp
            // 
            this.windowApp.AutoSize = true;
            this.windowApp.Location = new System.Drawing.Point(36, 60);
            this.windowApp.Name = "windowApp";
            this.windowApp.Size = new System.Drawing.Size(120, 17);
            this.windowApp.TabIndex = 6;
            this.windowApp.Text = "Window Application";
            this.windowApp.UseVisualStyleBackColor = true;
            // 
            // DeploymentInput
            // 
            this.AcceptButton = this.okBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.CancelButton = this.cancelBtn;
            this.ClientSize = new System.Drawing.Size(301, 172);
            this.Controls.Add(this.windowApp);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.versionsList);
            this.Controls.Add(this.asScript);
            this.Controls.Add(this.asExe);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.okBtn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MinimumSize = new System.Drawing.Size(200, 100);
            this.Name = "DeploymentInput";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Script distribution package";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button okBtn;
        private System.Windows.Forms.Button cancelBtn;
        private System.Windows.Forms.RadioButton asExe;
        private System.Windows.Forms.RadioButton asScript;
        private System.Windows.Forms.ComboBox versionsList;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox windowApp;
    }
}