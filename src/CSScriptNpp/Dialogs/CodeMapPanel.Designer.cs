namespace CSScriptNpp
{
    partial class CodeMapPanel
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
            this.mapTxt = new System.Windows.Forms.TextBox();
            this.refreshLabel = new System.Windows.Forms.LinkLabel();
            this.error = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // mapTxt
            // 
            this.mapTxt.BackColor = System.Drawing.Color.White;
            this.mapTxt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mapTxt.Location = new System.Drawing.Point(0, 0);
            this.mapTxt.Multiline = true;
            this.mapTxt.Name = "mapTxt";
            this.mapTxt.ReadOnly = true;
            this.mapTxt.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.mapTxt.Size = new System.Drawing.Size(284, 261);
            this.mapTxt.TabIndex = 0;
            this.mapTxt.WordWrap = false;
            this.mapTxt.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.mapTxt_MouseDoubleClick);
            // 
            // refreshLabel
            // 
            this.refreshLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.refreshLabel.AutoSize = true;
            this.refreshLabel.Location = new System.Drawing.Point(218, 3);
            this.refreshLabel.Name = "refreshLabel";
            this.refreshLabel.Size = new System.Drawing.Size(44, 13);
            this.refreshLabel.TabIndex = 1;
            this.refreshLabel.TabStop = true;
            this.refreshLabel.Text = "Refresh";
            this.refreshLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.refreshLabel_LinkClicked);
            // 
            // error
            // 
            this.error.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.error.BackColor = System.Drawing.Color.White;
            this.error.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.error.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.error.ForeColor = System.Drawing.Color.Red;
            this.error.Location = new System.Drawing.Point(12, 12);
            this.error.Multiline = true;
            this.error.Name = "error";
            this.error.ReadOnly = true;
            this.error.Size = new System.Drawing.Size(195, 64);
            this.error.TabIndex = 3;
            // 
            // CodeMapPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.error);
            this.Controls.Add(this.refreshLabel);
            this.Controls.Add(this.mapTxt);
            this.Name = "CodeMapPanel";
            this.Text = "CS-Script Code Map";
            this.VisibleChanged += new System.EventHandler(this.CodeMapPanel_VisibleChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox mapTxt;
        private System.Windows.Forms.LinkLabel refreshLabel;
        private System.Windows.Forms.TextBox error;
    }
}