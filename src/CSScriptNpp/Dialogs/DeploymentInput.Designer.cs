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
            this.asDll = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // okBtn
            // 
            this.okBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.okBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okBtn.Location = new System.Drawing.Point(87, 203);
            this.okBtn.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.okBtn.Name = "okBtn";
            this.okBtn.Size = new System.Drawing.Size(100, 28);
            this.okBtn.TabIndex = 1;
            this.okBtn.Text = "&Prepare";
            this.okBtn.UseVisualStyleBackColor = true;
            this.okBtn.Click += new System.EventHandler(this.okBtn_Click);
            // 
            // cancelBtn
            // 
            this.cancelBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelBtn.Location = new System.Drawing.Point(215, 203);
            this.cancelBtn.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(100, 28);
            this.cancelBtn.TabIndex = 1;
            this.cancelBtn.Text = "&Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            // 
            // asExe
            // 
            this.asExe.AutoSize = true;
            this.asExe.Location = new System.Drawing.Point(21, 46);
            this.asExe.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.asExe.Name = "asExe";
            this.asExe.Size = new System.Drawing.Size(190, 21);
            this.asExe.TabIndex = 3;
            this.asExe.Text = "Self-sufficient executable ";
            this.asExe.UseVisualStyleBackColor = true;
            this.asExe.CheckedChanged += new System.EventHandler(this.asExe_CheckedChanged);
            // 
            // asScript
            // 
            this.asScript.AutoSize = true;
            this.asScript.Checked = true;
            this.asScript.Location = new System.Drawing.Point(21, 15);
            this.asScript.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.asScript.Name = "asScript";
            this.asScript.Size = new System.Drawing.Size(162, 21);
            this.asScript.TabIndex = 3;
            this.asScript.Text = "Script + script engine";
            this.asScript.UseVisualStyleBackColor = true;
            // 
            // versionsList
            // 
            this.versionsList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.versionsList.FormattingEnabled = true;
            this.versionsList.Location = new System.Drawing.Point(21, 162);
            this.versionsList.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.versionsList.Name = "versionsList";
            this.versionsList.Size = new System.Drawing.Size(360, 24);
            this.versionsList.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 136);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(155, 17);
            this.label1.TabIndex = 5;
            this.label1.Text = "Target runtime version:";
            // 
            // windowApp
            // 
            this.windowApp.AutoSize = true;
            this.windowApp.Location = new System.Drawing.Point(48, 74);
            this.windowApp.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.windowApp.Name = "windowApp";
            this.windowApp.Size = new System.Drawing.Size(159, 21);
            this.windowApp.TabIndex = 6;
            this.windowApp.Text = "Windows Application";
            this.windowApp.UseVisualStyleBackColor = true;
            // 
            // asDll
            // 
            this.asDll.AutoSize = true;
            this.asDll.Location = new System.Drawing.Point(21, 103);
            this.asDll.Margin = new System.Windows.Forms.Padding(4);
            this.asDll.Name = "asDll";
            this.asDll.Size = new System.Drawing.Size(83, 21);
            this.asDll.TabIndex = 7;
            this.asDll.Text = "Script dll";
            this.asDll.UseVisualStyleBackColor = true;
            // 
            // DeploymentInput
            // 
            this.AcceptButton = this.okBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.CancelButton = this.cancelBtn;
            this.ClientSize = new System.Drawing.Size(401, 240);
            this.Controls.Add(this.asDll);
            this.Controls.Add(this.windowApp);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.versionsList);
            this.Controls.Add(this.asScript);
            this.Controls.Add(this.asExe);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.okBtn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MinimumSize = new System.Drawing.Size(261, 112);
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
        private System.Windows.Forms.RadioButton asDll;
    }
}