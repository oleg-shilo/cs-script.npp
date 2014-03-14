namespace CSScriptNpp
{
    partial class DebugPanel
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.stack = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabControl1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(545, 290);
            this.tabControl1.TabIndex = 2;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.stack);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(537, 264);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Call Stack";
            this.tabPage2.UseVisualStyleBackColor = true;
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
            this.stack.Location = new System.Drawing.Point(3, 3);
            this.stack.MultiSelect = false;
            this.stack.Name = "stack";
            this.stack.OwnerDraw = true;
            this.stack.Size = new System.Drawing.Size(531, 258);
            this.stack.TabIndex = 0;
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
            this.columnHeader2.Width = 25;
            // 
            // tabPage1
            // 
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(537, 264);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Locals";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // DebugPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(545, 290);
            this.Controls.Add(this.tabControl1);
            this.Name = "DebugPanel";
            this.Text = "DebugPanel";
            this.tabControl1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ListView stack;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;

    }
}