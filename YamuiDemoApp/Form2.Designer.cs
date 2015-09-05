namespace YamuiDemoApp {
    partial class Form2 {
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
            this.yamuiTextBox1 = new YamuiFramework.Controls.YamuiTextBox();
            this.SuspendLayout();
            // 
            // yamuiTextBox1
            // 
            this.yamuiTextBox1.Lines = new string[] {
        "yamuiTextBox1"};
            this.yamuiTextBox1.Location = new System.Drawing.Point(60, 41);
            this.yamuiTextBox1.MaxLength = 32767;
            this.yamuiTextBox1.Name = "yamuiTextBox1";
            this.yamuiTextBox1.PasswordChar = '\0';
            this.yamuiTextBox1.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.yamuiTextBox1.SelectedText = "";
            this.yamuiTextBox1.Size = new System.Drawing.Size(128, 23);
            this.yamuiTextBox1.TabIndex = 0;
            this.yamuiTextBox1.Text = "yamuiTextBox1";
            this.yamuiTextBox1.TextChanged += new System.EventHandler(this.yamuiTextBox1_TextChanged);
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(282, 208);
            this.Controls.Add(this.yamuiTextBox1);
            this.Name = "Form2";
            this.Text = "Form2";
            this.ResumeLayout(false);

        }

        #endregion

        private YamuiFramework.Controls.YamuiTextBox yamuiTextBox1;
    }
}