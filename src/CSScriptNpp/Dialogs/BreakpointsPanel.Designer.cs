namespace CSScriptNpp.Dialogs
{
    partial class BreakpointsPanel
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
            this.breakPoints = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.removeAll = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // breakPoints
            // 
            this.breakPoints.Activation = System.Windows.Forms.ItemActivation.TwoClick;
            this.breakPoints.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.breakPoints.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.breakPoints.FullRowSelect = true;
            this.breakPoints.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.breakPoints.HideSelection = false;
            this.breakPoints.Location = new System.Drawing.Point(0, 23);
            this.breakPoints.MultiSelect = false;
            this.breakPoints.Name = "stack";
            this.breakPoints.ShowItemToolTips = true;
            this.breakPoints.Size = new System.Drawing.Size(284, 238);
            this.breakPoints.TabIndex = 1;
            this.breakPoints.UseCompatibleStateImageBehavior = false;
            this.breakPoints.View = System.Windows.Forms.View.Details;
            this.breakPoints.ItemMouseHover += new System.Windows.Forms.ListViewItemMouseHoverEventHandler(this.stack_ItemMouseHover);
            this.breakPoints.DoubleClick += new System.EventHandler(this.stack_DoubleClick);
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
            // columnHeader3
            // 
            this.columnHeader3.Text = "Line";
            // 
            // removeAll
            // 
            this.removeAll.AutoSize = true;
            this.removeAll.Location = new System.Drawing.Point(0, 4);
            this.removeAll.Name = "removeAll";
            this.removeAll.Size = new System.Drawing.Size(61, 13);
            this.removeAll.TabIndex = 2;
            this.removeAll.TabStop = true;
            this.removeAll.Text = "Remove All";
            this.removeAll.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.removeAll_LinkClicked);
            // 
            // BreakpointsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.removeAll);
            this.Controls.Add(this.breakPoints);
            this.Name = "BreakpointsPanel";
            this.Text = "BreakpointsPanel";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView breakPoints;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.LinkLabel removeAll;
    }
}