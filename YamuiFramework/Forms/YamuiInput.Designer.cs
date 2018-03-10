using System.ComponentModel;
using System.Windows.Forms;
using YamuiFramework.Controls;
using YamuiFramework.HtmlRenderer.WinForms;

namespace YamuiFramework.Forms {
    partial class YamuiInput {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            this.components = new System.ComponentModel.Container();
            this.contentPanel = new YamuiFramework.Controls.YamuiScrollPanel();
            this.contentLabel = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this._errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.titleLabel = new YamuiFramework.HtmlRenderer.WinForms.HtmlLabel();
            this.contentPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // contentPanel
            // 
            this.contentPanel.Controls.Add(this.contentLabel);
            this.contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.contentPanel.Location = new System.Drawing.Point(5, 50);
            this.contentPanel.Name = "contentPanel";
            this.contentPanel.NoBackgroundImage = true;
            this.contentPanel.Size = new System.Drawing.Size(290, 225);
            this.contentPanel.TabIndex = 4;
            // 
            // contentLabel
            // 
            this.contentLabel.AutoSize = false;
            this.contentLabel.AutoSizeHeightOnly = true;
            this.contentLabel.BackColor = System.Drawing.Color.Transparent;
            this.contentLabel.BaseStylesheet = null;
            this.contentLabel.Location = new System.Drawing.Point(0, 0);
            this.contentLabel.Name = "contentLabel";
            this.contentLabel.Size = new System.Drawing.Size(245, 15);
            this.contentLabel.TabIndex = 4;
            this.contentLabel.TabStop = false;
            this.contentLabel.Text = "contentLabel";
            // 
            // _errorProvider
            // 
            this._errorProvider.ContainerControl = this;
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = false;
            this.titleLabel.AutoSizeHeightOnly = true;
            this.titleLabel.BackColor = System.Drawing.Color.Transparent;
            this.titleLabel.BaseStylesheet = null;
            this.titleLabel.CausesValidation = false;
            this.titleLabel.Enabled = false;
            this.titleLabel.IsContextMenuEnabled = false;
            this.titleLabel.IsSelectionEnabled = false;
            this.titleLabel.Location = new System.Drawing.Point(5, 5);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(243, 15);
            this.titleLabel.TabIndex = 6;
            this.titleLabel.TabStop = false;
            this.titleLabel.Text = "titleLabel";
            // 
            // YamuiInput
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(300, 325);
            this.Controls.Add(this.titleLabel);
            this.Controls.Add(this.contentPanel);
            this.MinimizeBox = false;
            this.Name = "YamuiInput";
            this.Padding = new System.Windows.Forms.Padding(5, 50, 5, 50);
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "YamuiFormMessageBox";
            this.contentPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private YamuiScrollPanel contentPanel;
        private HtmlLabel contentLabel;
        private ErrorProvider _errorProvider;
        private HtmlLabel titleLabel;



    }
}