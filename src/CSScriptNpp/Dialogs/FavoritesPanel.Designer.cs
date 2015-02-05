namespace CSScriptNpp.Dialogs
{
    partial class FavoritesPanel
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
            this.scriptsList = new System.Windows.Forms.ListBox();
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshLabel = new System.Windows.Forms.LinkLabel();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.contextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // scriptsList
            // 
            this.scriptsList.ContextMenuStrip = this.contextMenu;
            this.scriptsList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scriptsList.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.scriptsList.Location = new System.Drawing.Point(0, 0);
            this.scriptsList.Name = "scriptsList";
            this.scriptsList.Size = new System.Drawing.Size(284, 261);
            this.scriptsList.TabIndex = 0;
            this.toolTip1.SetToolTip(this.scriptsList, "<placeholder>");
            this.scriptsList.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.scriptsList_DrawItem);
            this.scriptsList.KeyDown += new System.Windows.Forms.KeyEventHandler(this.scriptsList_KeyDown);
            this.scriptsList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.scriptsList_MouseDoubleClick);
            this.scriptsList.MouseMove += new System.Windows.Forms.MouseEventHandler(this.scriptsList_MouseMove);
            // 
            // contextMenu
            // 
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.removeToolStripMenuItem});
            this.contextMenu.Name = "contextMenu";
            this.contextMenu.Size = new System.Drawing.Size(118, 48);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.removeToolStripMenuItem.Text = "Remove";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.removeToolStripMenuItem_Click);
            // 
            // refreshLabel
            // 
            this.refreshLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.refreshLabel.AutoSize = true;
            this.refreshLabel.BackColor = System.Drawing.Color.Transparent;
            this.refreshLabel.Location = new System.Drawing.Point(238, 2);
            this.refreshLabel.Name = "refreshLabel";
            this.refreshLabel.Size = new System.Drawing.Size(44, 13);
            this.refreshLabel.TabIndex = 2;
            this.refreshLabel.TabStop = true;
            this.refreshLabel.Text = "Refresh";
            this.refreshLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.refreshLabel_LinkClicked);
            // 
            // FavoritesPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.refreshLabel);
            this.Controls.Add(this.scriptsList);
            this.Name = "FavoritesPanel";
            this.Text = "FavoritesPanel";
            this.contextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox scriptsList;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.LinkLabel refreshLabel;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}