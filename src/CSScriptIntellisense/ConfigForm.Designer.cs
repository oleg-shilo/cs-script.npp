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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigForm));
            this.useArrow = new System.Windows.Forms.CheckBox();
            this.intercept = new System.Windows.Forms.CheckBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.autoInsertSingle = new System.Windows.Forms.CheckBox();
            this.useMethodBrackets = new System.Windows.Forms.CheckBox();
            this.ignoreDocExceptions = new System.Windows.Forms.CheckBox();
            this.formatAsYouType = new System.Windows.Forms.CheckBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.ContentPanel = new System.Windows.Forms.Panel();
            this.vbSupport = new System.Windows.Forms.CheckBox();
            this.formatOnSave = new System.Windows.Forms.CheckBox();
            this.F12OnCtrlClick = new System.Windows.Forms.CheckBox();
            this.useContextMenu = new System.Windows.Forms.CheckBox();
            this.roslynIntellisense = new System.Windows.Forms.CheckBox();
            this.roslynFormatter = new System.Windows.Forms.CheckBox();
            this.ContentPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // useArrow
            // 
            this.useArrow.AutoSize = true;
            this.useArrow.Location = new System.Drawing.Point(0, 9);
            this.useArrow.Name = "useArrow";
            this.useArrow.Size = new System.Drawing.Size(248, 17);
            this.useArrow.TabIndex = 0;
            this.useArrow.Text = "Use Right Arrow to accept selected suggestion";
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
            // autoInsertSingle
            // 
            this.autoInsertSingle.AutoSize = true;
            this.autoInsertSingle.Location = new System.Drawing.Point(0, 32);
            this.autoInsertSingle.Name = "autoInsertSingle";
            this.autoInsertSingle.Size = new System.Drawing.Size(155, 17);
            this.autoInsertSingle.TabIndex = 2;
            this.autoInsertSingle.Text = "Auto insert single suggetion";
            this.toolTip1.SetToolTip(this.autoInsertSingle, "Auto inster suggested autocompletion item \r\nif it is the only item in the suggest" +
        "ion list.");
            this.autoInsertSingle.UseVisualStyleBackColor = true;
            // 
            // useMethodBrackets
            // 
            this.useMethodBrackets.AutoSize = true;
            this.useMethodBrackets.Location = new System.Drawing.Point(0, 56);
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
            this.formatAsYouType.Location = new System.Drawing.Point(0, 79);
            this.formatAsYouType.Name = "formatAsYouType";
            this.formatAsYouType.Size = new System.Drawing.Size(142, 17);
            this.formatAsYouType.TabIndex = 2;
            this.formatAsYouType.Text = "Format code as you type";
            this.formatAsYouType.UseVisualStyleBackColor = true;
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(10, 215);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(120, 13);
            this.linkLabel1.TabIndex = 4;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Edit settings file  instead";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // ContentPanel
            // 
            this.ContentPanel.Controls.Add(this.vbSupport);
            this.ContentPanel.Controls.Add(this.formatOnSave);
            this.ContentPanel.Controls.Add(this.useMethodBrackets);
            this.ContentPanel.Controls.Add(this.useArrow);
            this.ContentPanel.Controls.Add(this.autoInsertSingle);
            this.ContentPanel.Controls.Add(this.F12OnCtrlClick);
            this.ContentPanel.Controls.Add(this.useContextMenu);
            this.ContentPanel.Controls.Add(this.roslynIntellisense);
            this.ContentPanel.Controls.Add(this.roslynFormatter);
            this.ContentPanel.Controls.Add(this.ignoreDocExceptions);
            this.ContentPanel.Controls.Add(this.intercept);
            this.ContentPanel.Controls.Add(this.formatAsYouType);
            this.ContentPanel.Location = new System.Drawing.Point(13, 3);
            this.ContentPanel.Name = "ContentPanel";
            this.ContentPanel.Size = new System.Drawing.Size(290, 209);
            this.ContentPanel.TabIndex = 5;
            // 
            // vbSupport
            // 
            this.vbSupport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.vbSupport.AutoSize = true;
            this.vbSupport.Location = new System.Drawing.Point(0, 192);
            this.vbSupport.Name = "vbSupport";
            this.vbSupport.Size = new System.Drawing.Size(120, 17);
            this.vbSupport.TabIndex = 8;
            this.vbSupport.Text = "Support for VB.NET";
            this.toolTip1.SetToolTip(this.vbSupport, resources.GetString("vbSupport.ToolTip"));
            this.vbSupport.UseVisualStyleBackColor = true;
            // 
            // formatOnSave
            // 
            this.formatOnSave.AutoSize = true;
            this.formatOnSave.Location = new System.Drawing.Point(0, 102);
            this.formatOnSave.Name = "formatOnSave";
            this.formatOnSave.Size = new System.Drawing.Size(99, 17);
            this.formatOnSave.TabIndex = 3;
            this.formatOnSave.Text = "Format on save";
            this.formatOnSave.UseVisualStyleBackColor = true;
            // 
            // F12OnCtrlClick
            // 
            this.F12OnCtrlClick.AutoSize = true;
            this.F12OnCtrlClick.Location = new System.Drawing.Point(0, 147);
            this.F12OnCtrlClick.Name = "F12OnCtrlClick";
            this.F12OnCtrlClick.Size = new System.Drawing.Size(227, 17);
            this.F12OnCtrlClick.TabIndex = 2;
            this.F12OnCtrlClick.Text = "\"Go To Definition\" on mouse Ctrl+LeftClick";
            this.F12OnCtrlClick.UseVisualStyleBackColor = true;
            // 
            // useContextMenu
            // 
            this.useContextMenu.AutoSize = true;
            this.useContextMenu.Location = new System.Drawing.Point(0, 124);
            this.useContextMenu.Name = "useContextMenu";
            this.useContextMenu.Size = new System.Drawing.Size(183, 17);
            this.useContextMenu.TabIndex = 2;
            this.useContextMenu.Text = "Use Context Menu for commands";
            this.useContextMenu.UseVisualStyleBackColor = true;
            // 
            // roslynIntellisense
            // 
            this.roslynIntellisense.AutoSize = true;
            this.roslynIntellisense.Location = new System.Drawing.Point(0, 170);
            this.roslynIntellisense.Name = "roslynIntellisense";
            this.roslynIntellisense.Size = new System.Drawing.Size(225, 17);
            this.roslynIntellisense.TabIndex = 2;
            this.roslynIntellisense.Text = "C# 6 support (Roslyn) - Requires .NET 4.6";
            this.roslynIntellisense.UseVisualStyleBackColor = true;
            // 
            // roslynFormatter
            // 
            this.roslynFormatter.AutoSize = true;
            this.roslynFormatter.Location = new System.Drawing.Point(163, 79);
            this.roslynFormatter.Name = "roslynFormatter";
            this.roslynFormatter.Size = new System.Drawing.Size(127, 17);
            this.roslynFormatter.TabIndex = 2;
            this.roslynFormatter.Text = "Use Roslyn Formatter";
            this.roslynFormatter.UseVisualStyleBackColor = true;
            this.roslynFormatter.Visible = false;
            // 
            // ConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(315, 239);
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
        private System.Windows.Forms.CheckBox roslynIntellisense;
        private System.Windows.Forms.CheckBox autoInsertSingle;
        private System.Windows.Forms.CheckBox useContextMenu;
        private System.Windows.Forms.CheckBox F12OnCtrlClick;
        private System.Windows.Forms.CheckBox formatOnSave;
        private System.Windows.Forms.CheckBox vbSupport;
    }
}