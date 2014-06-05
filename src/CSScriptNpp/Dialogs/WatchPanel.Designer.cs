namespace CSScriptNpp.Dialogs
{
    partial class WatchPanel
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
            this.contentPanel = new System.Windows.Forms.Panel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.addAtCaretBtn = new System.Windows.Forms.ToolStripButton();
            this.addExpressionBtn = new System.Windows.Forms.ToolStripButton();
            this.deleteExpressionBtn = new System.Windows.Forms.ToolStripButton();
            this.deleteAllExpressionsBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // contentPanel
            // 
            this.contentPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.contentPanel.Location = new System.Drawing.Point(1, 25);
            this.contentPanel.Name = "contentPanel";
            this.contentPanel.Size = new System.Drawing.Size(280, 234);
            this.contentPanel.TabIndex = 1;
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addAtCaretBtn,
            this.addExpressionBtn,
            this.deleteExpressionBtn,
            this.deleteAllExpressionsBtn});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(284, 25);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // addAtCaretBtn
            // 
            this.addAtCaretBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.addAtCaretBtn.Image = global::CSScriptNpp.Resources.Resources.dbg_addwatch_at_caret;
            this.addAtCaretBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.addAtCaretBtn.Name = "addAtCaretBtn";
            this.addAtCaretBtn.Size = new System.Drawing.Size(23, 22);
            this.addAtCaretBtn.Text = "toolStripButton2";
            this.addAtCaretBtn.ToolTipText = "Add Expression from the caret position";
            this.addAtCaretBtn.Click += new System.EventHandler(this.addAtCaretBtn_Click);
            // 
            // addExpressionBtn
            // 
            this.addExpressionBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.addExpressionBtn.Image = global::CSScriptNpp.Resources.Resources.dbg_addwatch;
            this.addExpressionBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.addExpressionBtn.Name = "addExpressionBtn";
            this.addExpressionBtn.Size = new System.Drawing.Size(23, 22);
            this.addExpressionBtn.ToolTipText = "Add Expression";
            this.addExpressionBtn.Click += new System.EventHandler(this.addExpressionBtn_Click);
            // 
            // deleteExpressionBtn
            // 
            this.deleteExpressionBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.deleteExpressionBtn.Image = global::CSScriptNpp.Resources.Resources.dbg_removewatch;
            this.deleteExpressionBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.deleteExpressionBtn.Name = "deleteExpressionBtn";
            this.deleteExpressionBtn.Size = new System.Drawing.Size(23, 22);
            this.deleteExpressionBtn.Text = "toolStripButton1";
            this.deleteExpressionBtn.ToolTipText = "Delete selected Expression(s)";
            this.deleteExpressionBtn.Click += new System.EventHandler(this.deleteExpressionBtn_Click);
            // 
            // deleteAllExpressionsBtn
            // 
            this.deleteAllExpressionsBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.deleteAllExpressionsBtn.Image = global::CSScriptNpp.Resources.Resources.dbg_removeallwatch;
            this.deleteAllExpressionsBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.deleteAllExpressionsBtn.Name = "deleteAllExpressionsBtn";
            this.deleteAllExpressionsBtn.Size = new System.Drawing.Size(23, 22);
            this.deleteAllExpressionsBtn.Text = "toolStripButton1";
            this.deleteAllExpressionsBtn.ToolTipText = "Delete All Expressions";
            this.deleteAllExpressionsBtn.Click += new System.EventHandler(this.deleteAllExpressionsBtn_Click);
            // 
            // WatchPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.contentPanel);
            this.Name = "WatchPanel";
            this.Text = "CS-Script Watch";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel contentPanel;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton addExpressionBtn;
        private System.Windows.Forms.ToolStripButton deleteExpressionBtn;
        private System.Windows.Forms.ToolStripButton deleteAllExpressionsBtn;
        private System.Windows.Forms.ToolStripButton addAtCaretBtn;
    }
}