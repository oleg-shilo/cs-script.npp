namespace CSScriptNpp.Dialogs
{
    partial class TextVisualizer
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
            this.valueCtrl = new System.Windows.Forms.TextBox();
            this.wordWrap = new System.Windows.Forms.CheckBox();
            this.expressionCtrl = new System.Windows.Forms.Label();
            this.close = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // valueCtrl
            // 
            this.valueCtrl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.valueCtrl.Location = new System.Drawing.Point(2, 27);
            this.valueCtrl.Multiline = true;
            this.valueCtrl.Name = "valueCtrl";
            this.valueCtrl.ReadOnly = true;
            this.valueCtrl.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.valueCtrl.Size = new System.Drawing.Size(602, 308);
            this.valueCtrl.TabIndex = 0;
            // 
            // wordWrap
            // 
            this.wordWrap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.wordWrap.AutoSize = true;
            this.wordWrap.Location = new System.Drawing.Point(11, 347);
            this.wordWrap.Name = "wordWrap";
            this.wordWrap.Size = new System.Drawing.Size(52, 17);
            this.wordWrap.TabIndex = 1;
            this.wordWrap.Text = "Wrap";
            this.wordWrap.UseVisualStyleBackColor = true;
            this.wordWrap.CheckedChanged += new System.EventHandler(this.wordWrap_CheckedChanged);
            // 
            // expressionCtrl
            // 
            this.expressionCtrl.AutoSize = true;
            this.expressionCtrl.Location = new System.Drawing.Point(2, 8);
            this.expressionCtrl.Name = "expressionCtrl";
            this.expressionCtrl.Size = new System.Drawing.Size(61, 13);
            this.expressionCtrl.TabIndex = 2;
            this.expressionCtrl.Text = "Expression:";
            // 
            // close
            // 
            this.close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.close.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.close.Location = new System.Drawing.Point(526, 343);
            this.close.Name = "close";
            this.close.Size = new System.Drawing.Size(75, 23);
            this.close.TabIndex = 3;
            this.close.Text = "Close";
            this.close.UseVisualStyleBackColor = true;
            // 
            // TextVisualizer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.close;
            this.ClientSize = new System.Drawing.Size(608, 372);
            this.Controls.Add(this.close);
            this.Controls.Add(this.expressionCtrl);
            this.Controls.Add(this.wordWrap);
            this.Controls.Add(this.valueCtrl);
            this.MinimizeBox = false;
            this.Name = "TextVisualizer";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Text Visualizer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox valueCtrl;
        private System.Windows.Forms.CheckBox wordWrap;
        private System.Windows.Forms.Label expressionCtrl;
        private System.Windows.Forms.Button close;
    }
}