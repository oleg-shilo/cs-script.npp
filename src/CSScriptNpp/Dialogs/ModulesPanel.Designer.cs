namespace CSScriptNpp.Dialogs
{
    partial class ModulesPanel
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
            this.modulesList = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // modulesList
            // 
            this.modulesList.Activation = System.Windows.Forms.ItemActivation.TwoClick;
            this.modulesList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader3,
            this.columnHeader2});
            this.modulesList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.modulesList.FullRowSelect = true;
            this.modulesList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.modulesList.HideSelection = false;
            this.modulesList.Location = new System.Drawing.Point(0, 0);
            this.modulesList.MultiSelect = false;
            this.modulesList.Name = "modulesList";
            this.modulesList.OwnerDraw = true;
            this.modulesList.Size = new System.Drawing.Size(284, 261);
            this.modulesList.TabIndex = 1;
            this.modulesList.UseCompatibleStateImageBehavior = false;
            this.modulesList.View = System.Windows.Forms.View.Details;
            this.modulesList.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.stack_DrawColumnHeader);
            this.modulesList.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.stack_DrawSubItem);
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "#";
            this.columnHeader4.Width = 20;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Name";
            this.columnHeader3.Width = 40;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Path";
            this.columnHeader2.Width = 100;
            // 
            // ModulesPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.modulesList);
            this.Name = "ModulesPanel";
            this.Text = "CS-Script Modules";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView modulesList;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
    }
}