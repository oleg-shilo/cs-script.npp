namespace Updater
{
    partial class UserInputForm
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
            this.url = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.x64_CheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // okBtn
            // 
            this.okBtn.Location = new System.Drawing.Point(341, 26);
            this.okBtn.Name = "okBtn";
            this.okBtn.Size = new System.Drawing.Size(91, 23);
            this.okBtn.TabIndex = 2;
            this.okBtn.Text = "&Start";
            this.okBtn.UseVisualStyleBackColor = true;
            this.okBtn.Click += new System.EventHandler(this.okBtn_Click);
            // 
            // url
            // 
            this.url.AcceptsReturn = true;
            this.url.Location = new System.Drawing.Point(12, 29);
            this.url.Name = "url";
            this.url.Size = new System.Drawing.Size(312, 20);
            this.url.TabIndex = 0;
            this.url.KeyDown += new System.Windows.Forms.KeyEventHandler(this.url_KeyDown);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(191, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Package (URL, Version or local zip file)";
            // 
            // x64_CheckBox
            // 
            this.x64_CheckBox.AutoSize = true;
            this.x64_CheckBox.Location = new System.Drawing.Point(342, 6);
            this.x64_CheckBox.Name = "x64_CheckBox";
            this.x64_CheckBox.Size = new System.Drawing.Size(43, 17);
            this.x64_CheckBox.TabIndex = 11;
            this.x64_CheckBox.Text = "x64";
            this.x64_CheckBox.UseVisualStyleBackColor = true;
            this.x64_CheckBox.Visible = false;
            // 
            // UserInputForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(444, 61);
            this.Controls.Add(this.x64_CheckBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.url);
            this.Controls.Add(this.okBtn);
            this.KeyPreview = true;
            this.MaximumSize = new System.Drawing.Size(460, 100);
            this.MinimumSize = new System.Drawing.Size(460, 100);
            this.Name = "UserInputForm";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Update - CS-Script.Npp";
            this.TopMost = true;
            this.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.UserInputForm_PreviewKeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button okBtn;
        private System.Windows.Forms.TextBox url;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox x64_CheckBox;
    }
}