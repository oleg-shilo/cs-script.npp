namespace CSScriptNpp.Dialogs
{
    partial class CallStackPanel
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
            this.stack = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // stack
            // 
            this.stack.Activation = System.Windows.Forms.ItemActivation.TwoClick;
            this.stack.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.stack.Dock = System.Windows.Forms.DockStyle.Fill;
            this.stack.FullRowSelect = true;
            this.stack.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.stack.HideSelection = false;
            this.stack.Location = new System.Drawing.Point(0, 0);
            this.stack.MultiSelect = false;
            this.stack.Name = "stack";
            this.stack.OwnerDraw = true;
            this.stack.Size = new System.Drawing.Size(284, 261);
            this.stack.TabIndex = 1;
            this.stack.UseCompatibleStateImageBehavior = false;
            this.stack.View = System.Windows.Forms.View.Details;
            this.stack.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.stack_DrawColumnHeader);
            this.stack.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.stack_DrawSubItem);
            this.stack.DoubleClick += new System.EventHandler(this.stack_DoubleClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "";
            this.columnHeader1.Width = 20;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Name";
            this.columnHeader2.Width = 100;
            // 
            // CallStackPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.stack);
            this.Name = "CallStackPanel";
            this.Text = "CS-Script Call Stack";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView stack;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
    }
}