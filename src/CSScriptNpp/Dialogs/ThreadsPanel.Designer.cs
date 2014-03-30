namespace CSScriptNpp.Dialogs
{
    partial class ThreadsPanel
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
            this.threadsList = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // threadsList
            // 
            this.threadsList.Activation = System.Windows.Forms.ItemActivation.TwoClick;
            this.threadsList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader4,
            this.columnHeader3,
            this.columnHeader2});
            this.threadsList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.threadsList.FullRowSelect = true;
            this.threadsList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.threadsList.HideSelection = false;
            this.threadsList.Location = new System.Drawing.Point(0, 0);
            this.threadsList.MultiSelect = false;
            this.threadsList.Name = "threadsList";
            this.threadsList.OwnerDraw = true;
            this.threadsList.Size = new System.Drawing.Size(284, 261);
            this.threadsList.TabIndex = 1;
            this.threadsList.UseCompatibleStateImageBehavior = false;
            this.threadsList.View = System.Windows.Forms.View.Details;
            this.threadsList.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.stack_DrawColumnHeader);
            this.threadsList.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.stack_DrawSubItem);
            this.threadsList.DoubleClick += new System.EventHandler(this.stack_DoubleClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "";
            this.columnHeader1.Width = 20;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "ID";
            this.columnHeader3.Width = 40;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "#";
            this.columnHeader4.Width = 20;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Name";
            this.columnHeader2.Width = 100;
            // 
            // ThreadsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.threadsList);
            this.Name = "ThreadsPanel";
            this.Text = "CallStackPanel";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView threadsList;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
    }
}