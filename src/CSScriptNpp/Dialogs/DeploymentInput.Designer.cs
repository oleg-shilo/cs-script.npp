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
            this.windowApp = new System.Windows.Forms.CheckBox();
            this.asDll = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // okBtn
            // 
            this.okBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.okBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okBtn.Location = new System.Drawing.Point(65, 92);
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
            this.cancelBtn.Location = new System.Drawing.Point(161, 92);
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
            this.asScript.Size = new System.Drawing.Size(96, 17);
            this.asScript.TabIndex = 3;
            this.asScript.TabStop = true;
            this.asScript.Text = "Script + engine";
            this.asScript.UseVisualStyleBackColor = true;
            // 
            // windowApp
            // 
            this.windowApp.AutoSize = true;
            this.windowApp.Location = new System.Drawing.Point(138, 12);
            this.windowApp.Name = "windowApp";
            this.windowApp.Size = new System.Drawing.Size(125, 17);
            this.windowApp.TabIndex = 6;
            this.windowApp.Text = "Windows Application";
            this.windowApp.UseVisualStyleBackColor = true;
            this.windowApp.Visible = false;
            // 
            // asDll
            // 
            this.asDll.AutoSize = true;
            this.asDll.Location = new System.Drawing.Point(16, 60);
            this.asDll.Name = "asDll";
            this.asDll.Size = new System.Drawing.Size(65, 17);
            this.asDll.TabIndex = 7;
            this.asDll.Text = "Script dll";
            this.asDll.UseVisualStyleBackColor = true;
            // 
            // DeploymentInput
            // 
            this.AcceptButton = this.okBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.CancelButton = this.cancelBtn;
            this.ClientSize = new System.Drawing.Size(301, 122);
            this.Controls.Add(this.asDll);
            this.Controls.Add(this.windowApp);
            this.Controls.Add(this.asScript);
            this.Controls.Add(this.asExe);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.okBtn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MinimumSize = new System.Drawing.Size(200, 98);
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
        private System.Windows.Forms.CheckBox windowApp;
        private System.Windows.Forms.RadioButton asDll;
    }
}