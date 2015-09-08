namespace _3PA.MainFeatures.ToolTip {
    partial class InfoToolTipForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.labelContent = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.SuspendLayout();
            // 
            // labelContent
            // 
            this.labelContent.AutoSize = false;
            this.labelContent.AutoSizeHeightOnly = true;
            this.labelContent.BackColor = System.Drawing.Color.Transparent;
            this.labelContent.BaseStylesheet = null;
            this.labelContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelContent.Location = new System.Drawing.Point(1, 1);
            this.labelContent.Name = "labelContent";
            this.labelContent.Size = new System.Drawing.Size(306, 147);
            this.labelContent.TabIndex = 0;
            // 
            // ToolTipForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(308, 149);
            this.Controls.Add(this.labelContent);
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "ToolTipForm";
            this.Padding = new System.Windows.Forms.Padding(1);
            this.ResumeLayout(false);

        }

        #endregion

        private YamuiFramework.HtmlRenderer.WinForms.HtmlLabel labelContent;

    }
}