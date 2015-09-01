namespace YamuiDemoApp {
    partial class AutoCompletionForm {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AutoCompletionForm));
            this.fastOLV = new BrightIdeasSoftware.FastObjectListView();
            this.Keyword = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.typeImageList = new System.Windows.Forms.ImageList(this.components);
            this.hotItemStyle = new BrightIdeasSoftware.HotItemStyle();
            this.yamuiButton1 = new YamuiFramework.Controls.YamuiButton();
            this.yamuiTextBox1 = new YamuiFramework.Controls.YamuiTextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.fastOLV)).BeginInit();
            this.SuspendLayout();
            // 
            // fastOLV
            // 
            this.fastOLV.Activation = System.Windows.Forms.ItemActivation.TwoClick;
            this.fastOLV.AllColumns.Add(this.Keyword);
            this.fastOLV.AlternateRowBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.fastOLV.AutoArrange = false;
            this.fastOLV.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.fastOLV.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.fastOLV.CausesValidation = false;
            this.fastOLV.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Keyword});
            this.fastOLV.CopySelectionOnControlC = false;
            this.fastOLV.CopySelectionOnControlCUsesDragSource = false;
            this.fastOLV.Cursor = System.Windows.Forms.Cursors.Default;
            this.fastOLV.Dock = System.Windows.Forms.DockStyle.Top;
            this.fastOLV.EmptyListMsg = "Empty!";
            this.fastOLV.ForeColor = System.Drawing.Color.Black;
            this.fastOLV.FullRowSelect = true;
            this.fastOLV.HasCollapsibleGroups = false;
            this.fastOLV.HeaderMaximumHeight = 0;
            this.fastOLV.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.fastOLV.HideSelection = false;
            this.fastOLV.HighlightBackgroundColor = System.Drawing.Color.Maroon;
            this.fastOLV.HighlightForegroundColor = System.Drawing.Color.White;
            this.fastOLV.Location = new System.Drawing.Point(0, 0);
            this.fastOLV.MultiSelect = false;
            this.fastOLV.Name = "fastOLV";
            this.fastOLV.OwnerDraw = true;
            this.fastOLV.PersistentCheckBoxes = false;
            this.fastOLV.SelectAllOnControlA = false;
            this.fastOLV.SelectColumnsOnRightClick = false;
            this.fastOLV.SelectColumnsOnRightClickBehaviour = BrightIdeasSoftware.ObjectListView.ColumnSelectBehaviour.None;
            this.fastOLV.ShowFilterMenuOnRightClick = false;
            this.fastOLV.ShowGroups = false;
            this.fastOLV.ShowHeaderInAllViews = false;
            this.fastOLV.ShowSortIndicators = false;
            this.fastOLV.Size = new System.Drawing.Size(325, 452);
            this.fastOLV.SmallImageList = this.typeImageList;
            this.fastOLV.TabIndex = 0;
            this.fastOLV.TriggerCellOverEventsWhenOverHeader = false;
            this.fastOLV.UpdateSpaceFillingColumnsWhenDraggingColumnDivider = false;
            this.fastOLV.UseAlternatingBackColors = true;
            this.fastOLV.UseCompatibleStateImageBehavior = false;
            this.fastOLV.UseFiltering = true;
            this.fastOLV.View = System.Windows.Forms.View.Details;
            this.fastOLV.VirtualMode = true;
            // 
            // Keyword
            // 
            this.Keyword.AspectName = "DisplayText";
            this.Keyword.AutoCompleteEditor = false;
            this.Keyword.AutoCompleteEditorMode = System.Windows.Forms.AutoCompleteMode.None;
            this.Keyword.CellVerticalAlignment = System.Drawing.StringAlignment.Center;
            this.Keyword.FillsFreeSpace = true;
            this.Keyword.Groupable = false;
            this.Keyword.HeaderCheckBoxUpdatesRowCheckBoxes = false;
            this.Keyword.Hideable = false;
            this.Keyword.IsEditable = false;
            this.Keyword.ShowTextInHeader = false;
            this.Keyword.Text = "Keywords";
            this.Keyword.Width = 100;
            // 
            // typeImageList
            // 
            this.typeImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("typeImageList.ImageStream")));
            this.typeImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.typeImageList.Images.SetKeyName(0, "autocompletion_all.png");
            this.typeImageList.Images.SetKeyName(1, "autocompletion_buffer.png");
            this.typeImageList.Images.SetKeyName(2, "autocompletion_cancel.png");
            this.typeImageList.Images.SetKeyName(3, "autocompletion_empty.png");
            this.typeImageList.Images.SetKeyName(4, "autocompletion_field.png");
            this.typeImageList.Images.SetKeyName(5, "autocompletion_field_pk.png");
            this.typeImageList.Images.SetKeyName(6, "autocompletion_keyword.png");
            this.typeImageList.Images.SetKeyName(7, "autocompletion_snippets.png");
            this.typeImageList.Images.SetKeyName(8, "autocompletion_snippets2.png");
            this.typeImageList.Images.SetKeyName(9, "autocompletion_snippets3.png");
            this.typeImageList.Images.SetKeyName(10, "autocompletion_table.png");
            this.typeImageList.Images.SetKeyName(11, "autocompletion_temptable.png");
            this.typeImageList.Images.SetKeyName(12, "autocompletion_variable.png");
            // 
            // hotItemStyle
            // 
            this.hotItemStyle.BackColor = System.Drawing.Color.Olive;
            this.hotItemStyle.ForeColor = System.Drawing.Color.Black;
            // 
            // yamuiButton1
            // 
            this.yamuiButton1.Location = new System.Drawing.Point(28, 514);
            this.yamuiButton1.Name = "yamuiButton1";
            this.yamuiButton1.Size = new System.Drawing.Size(87, 24);
            this.yamuiButton1.TabIndex = 1;
            this.yamuiButton1.Text = "yamuiButton1";
            this.yamuiButton1.ButtonPressed += new System.EventHandler<YamuiFramework.Controls.ButtonPressedEventArgs>(this.yamuiButton1_ButtonPressed);
            // 
            // yamuiTextBox1
            // 
            this.yamuiTextBox1.Lines = new string[0];
            this.yamuiTextBox1.Location = new System.Drawing.Point(161, 514);
            this.yamuiTextBox1.MaxLength = 32767;
            this.yamuiTextBox1.Name = "yamuiTextBox1";
            this.yamuiTextBox1.PasswordChar = '\0';
            this.yamuiTextBox1.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.yamuiTextBox1.SelectedText = "";
            this.yamuiTextBox1.Size = new System.Drawing.Size(118, 23);
            this.yamuiTextBox1.TabIndex = 2;
            this.yamuiTextBox1.WaterMark = "filter!";
            this.yamuiTextBox1.TextChanged += new System.EventHandler(this.yamuiTextBox1_TextChanged);
            this.yamuiTextBox1.Click += new System.EventHandler(this.yamuiTextBox1_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(88, 588);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 20);
            this.textBox1.TabIndex = 3;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // AutoCompletionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(325, 658);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.yamuiTextBox1);
            this.Controls.Add(this.yamuiButton1);
            this.Controls.Add(this.fastOLV);
            this.Name = "AutoCompletionForm";
            this.Text = "AutoCompletionForm";
            ((System.ComponentModel.ISupportInitialize)(this.fastOLV)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private BrightIdeasSoftware.FastObjectListView fastOLV;
        private BrightIdeasSoftware.OLVColumn Keyword;
        private System.Windows.Forms.ImageList typeImageList;
        private BrightIdeasSoftware.HotItemStyle hotItemStyle;
        private YamuiFramework.Controls.YamuiButton yamuiButton1;
        private YamuiFramework.Controls.YamuiTextBox yamuiTextBox1;
        private System.Windows.Forms.TextBox textBox1;
    }
}