namespace CSScriptIntellisense
{
    partial class MemberInfoPanel
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
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Interval = 5000;
            this.timer1.Tick += new System.EventHandler(this.OnIdelTimer_Tick);
            // 
            // MemberInfoPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.ClientSize = new System.Drawing.Size(355, 33);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.Name = "MemberInfoPanel";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "AutocompleteForm";
            this.Activated += new System.EventHandler(this.MemberInfoPanel_Activated);
            this.Deactivate += new System.EventHandler(this.QuickInfoPanel_Deactivate);
            this.VisibleChanged += new System.EventHandler(this.MemberInfoPanel_VisibleChanged);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.QuickInfoPanel_Paint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MemberInfoPanel_KeyDown);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MemberInfoPanel_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MemberInfoPanel_MouseMove);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer timer1;

    }
}