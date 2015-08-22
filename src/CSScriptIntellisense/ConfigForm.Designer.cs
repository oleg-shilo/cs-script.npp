namespace CSScriptIntellisense
{
    partial class ConfigForm
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
            this.useArrow = new System.Windows.Forms.CheckBox();
            this.intercept = new System.Windows.Forms.CheckBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.useMethodBrackets = new System.Windows.Forms.CheckBox();
            this.ignoreDocExceptions = new System.Windows.Forms.CheckBox();
            this.formatAsYouType = new System.Windows.Forms.CheckBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.ContentPanel = new System.Windows.Forms.Panel();
            this.roslynFormatter = new System.Windows.Forms.CheckBox();
            this.ContentPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // useArrow
            // 
            this.useArrow.AutoSize = true;
            this.useArrow.Location = new System.Drawing.Point(0, 9);
            this.useArrow.Name = "useArrow";
            this.useArrow.Size = new System.Drawing.Size(205, 17);
            this.useArrow.TabIndex = 0;
            this.useArrow.Text = "Use Right Arrow to accept suggestion";
            this.useArrow.UseVisualStyleBackColor = true;
            // 
            // intercept
            // 
            this.intercept.AutoSize = true;
            this.intercept.Location = new System.Drawing.Point(155, 21);
            this.intercept.Name = "intercept";
            this.intercept.Size = new System.Drawing.Size(155, 17);
            this.intercept.TabIndex = 0;
            this.intercept.Text = "Use Visual Studio shortcuts";
            this.intercept.UseVisualStyleBackColor = true;
            this.intercept.Visible = false;
            // 
            // useMethodBrackets
            // 
            this.useMethodBrackets.AutoSize = true;
            this.useMethodBrackets.Location = new System.Drawing.Point(0, 32);
            this.useMethodBrackets.Name = "useMethodBrackets";
            this.useMethodBrackets.Size = new System.Drawing.Size(191, 17);
            this.useMethodBrackets.TabIndex = 2;
            this.useMethodBrackets.Text = "End methods with an open bracket";
            this.useMethodBrackets.UseVisualStyleBackColor = true;
            // 
            // ignoreDocExceptions
            // 
            this.ignoreDocExceptions.AutoSize = true;
            this.ignoreDocExceptions.Location = new System.Drawing.Point(145, 44);
            this.ignoreDocExceptions.Name = "ignoreDocExceptions";
            this.ignoreDocExceptions.Size = new System.Drawing.Size(278, 17);
            this.ignoreDocExceptions.TabIndex = 2;
            this.ignoreDocExceptions.Text = "Skip Exceptions section from the XML documentation";
            this.ignoreDocExceptions.UseVisualStyleBackColor = true;
            this.ignoreDocExceptions.Visible = false;
            // 
            // formatAsYouType
            // 
            this.formatAsYouType.AutoSize = true;
            this.formatAsYouType.Location = new System.Drawing.Point(0, 55);
            this.formatAsYouType.Name = "formatAsYouType";
            this.formatAsYouType.Size = new System.Drawing.Size(142, 17);
            this.formatAsYouType.TabIndex = 2;
            this.formatAsYouType.Text = "Format code as you type";
            this.formatAsYouType.UseVisualStyleBackColor = true;
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(12, 106);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(120, 13);
            this.linkLabel1.TabIndex = 4;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Edit settings file  instead";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // ContentPanel
            // 
            this.ContentPanel.Controls.Add(this.useMethodBrackets);
            this.ContentPanel.Controls.Add(this.useArrow);
            this.ContentPanel.Controls.Add(this.roslynFormatter);
            this.ContentPanel.Controls.Add(this.ignoreDocExceptions);
            this.ContentPanel.Controls.Add(this.intercept);
            this.ContentPanel.Controls.Add(this.formatAsYouType);
            this.ContentPanel.Location = new System.Drawing.Point(13, 3);
            this.ContentPanel.Name = "ContentPanel";
            this.ContentPanel.Size = new System.Drawing.Size(290, 100);
            this.ContentPanel.TabIndex = 5;
            // 
            // roslynFormatter
            // 
            this.roslynFormatter.AutoSize = true;
            this.roslynFormatter.Location = new System.Drawing.Point(0, 78);
            this.roslynFormatter.Name = "roslynFormatter";
            this.roslynFormatter.Size = new System.Drawing.Size(127, 17);
            this.roslynFormatter.TabIndex = 2;
            this.roslynFormatter.Text = "Use Roslyn Formatter";
            this.roslynFormatter.UseVisualStyleBackColor = true;
            // 
            // ConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(315, 135);
            this.Controls.Add(this.ContentPanel);
            this.Controls.Add(this.linkLabel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.KeyPreview = true;
            this.Name = "ConfigForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "C# Intellisense Settings";
            this.Load += new System.EventHandler(this.ConfigForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ConfigForm_KeyDown);
            this.ContentPanel.ResumeLayout(false);
            this.ContentPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox useArrow;
        private System.Windows.Forms.CheckBox intercept;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.CheckBox useMethodBrackets;
        private System.Windows.Forms.CheckBox ignoreDocExceptions;
        private System.Windows.Forms.CheckBox formatAsYouType;
        private System.Windows.Forms.LinkLabel linkLabel1;
        public System.Windows.Forms.Panel ContentPanel;
        private System.Windows.Forms.CheckBox roslynFormatter;
    }
}