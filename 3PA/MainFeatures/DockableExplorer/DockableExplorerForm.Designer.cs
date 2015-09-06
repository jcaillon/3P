namespace _3PA.MainFeatures.DockableExplorer {
    partial class DockableExplorerForm {
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
            this.components = new System.ComponentModel.Container();
            this.yamuiTextBox1 = new YamuiFramework.Controls.YamuiTextBox();
            this.ovlTree = new BrightIdeasSoftware.TreeListView();
            this.DisplayText = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            ((System.ComponentModel.ISupportInitialize)(this.ovlTree)).BeginInit();
            this.SuspendLayout();
            // 
            // yamuiTextBox1
            // 
            this.yamuiTextBox1.Lines = new string[] {
        "yamuiTextBox1"};
            this.yamuiTextBox1.Location = new System.Drawing.Point(12, 12);
            this.yamuiTextBox1.MaxLength = 32767;
            this.yamuiTextBox1.Name = "yamuiTextBox1";
            this.yamuiTextBox1.PasswordChar = '\0';
            this.yamuiTextBox1.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.yamuiTextBox1.SelectedText = "";
            this.yamuiTextBox1.Size = new System.Drawing.Size(267, 23);
            this.yamuiTextBox1.TabIndex = 0;
            this.yamuiTextBox1.Text = "yamuiTextBox1";
            // 
            // ovlTree
            // 
            this.ovlTree.AllColumns.Add(this.DisplayText);
            this.ovlTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ovlTree.AutoArrange = false;
            this.ovlTree.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ovlTree.CausesValidation = false;
            this.ovlTree.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.DisplayText});
            this.ovlTree.FullRowSelect = true;
            this.ovlTree.HeaderMaximumHeight = 0;
            this.ovlTree.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.ovlTree.Location = new System.Drawing.Point(12, 41);
            this.ovlTree.MultiSelect = false;
            this.ovlTree.Name = "ovlTree";
            this.ovlTree.OwnerDraw = true;
            this.ovlTree.RowHeight = 20;
            this.ovlTree.ShowGroups = false;
            this.ovlTree.Size = new System.Drawing.Size(267, 489);
            this.ovlTree.TabIndex = 1;
            this.ovlTree.UseCompatibleStateImageBehavior = false;
            this.ovlTree.UseFiltering = true;
            this.ovlTree.UseHotItem = true;
            this.ovlTree.View = System.Windows.Forms.View.Details;
            this.ovlTree.VirtualMode = true;
            // 
            // DisplayText
            // 
            this.DisplayText.AspectName = "DisplayText";
            this.DisplayText.FillsFreeSpace = true;
            // 
            // DockableExplorerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(291, 583);
            this.Controls.Add(this.ovlTree);
            this.Controls.Add(this.yamuiTextBox1);
            this.Name = "DockableExplorerForm";
            this.Text = "DockableExplorerForm";
            ((System.ComponentModel.ISupportInitialize)(this.ovlTree)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private YamuiFramework.Controls.YamuiTextBox yamuiTextBox1;
        private BrightIdeasSoftware.TreeListView ovlTree;
        private BrightIdeasSoftware.OLVColumn DisplayText;
    }
}