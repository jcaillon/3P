using System.ComponentModel;
using BrightIdeasSoftware;
using YamuiFramework.Controls;
using YamuiFramework.HtmlRenderer.WinForms;

namespace _3PA.MainFeatures.FileExplorer {
    partial class FileExplorerForm {
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
            this.toolTipHtml = new YamuiFramework.HtmlRenderer.WinForms.HtmlToolTip();
            this.btGetHelp = new YamuiFramework.Controls.YamuiImageButton();
            this.lbNbErrors = new YamuiFramework.Controls.YamuiLabel();
            this.lbErrorText = new YamuiFramework.Controls.YamuiLabel();
            this.btClearAllErrors = new YamuiFramework.Controls.YamuiImageButton();
            this.btNextError = new YamuiFramework.Controls.YamuiImageButton();
            this.lbStatus = new YamuiFramework.Controls.YamuiLabel();
            this.btPrevError = new YamuiFramework.Controls.YamuiImageButton();
            this.yamuiLabel1 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiLabel2 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiLabel3 = new YamuiFramework.Controls.YamuiLabel();
            this.FileName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.ovl = new BrightIdeasSoftware.FastObjectListView();
            this.yamuiLabel4 = new YamuiFramework.Controls.YamuiLabel();
            this.nbitems = new YamuiFramework.Controls.YamuiLabel();
            this.btRefresh = new YamuiFramework.Controls.YamuiImageButton();
            this.textFilter = new YamuiFramework.Controls.YamuiTextBoxAlt();
            this.btErase = new YamuiFramework.Controls.YamuiImageButton();
            this.btDirectory = new YamuiFramework.Controls.YamuiImageButton();
            this.lbDirectory = new YamuiFramework.Controls.YamuiLabel();
            this.btGotoDir = new YamuiFramework.Controls.YamuiImageButton();
            this.yamuiImageButton1 = new YamuiFramework.Controls.YamuiImageButton();
            ((System.ComponentModel.ISupportInitialize)(this.ovl)).BeginInit();
            this.SuspendLayout();
            // 
            // toolTipHtml
            // 
            this.toolTipHtml.AllowLinksHandling = true;
            this.toolTipHtml.AutoPopDelay = 90000;
            this.toolTipHtml.BaseStylesheet = null;
            this.toolTipHtml.InitialDelay = 500;
            this.toolTipHtml.MaximumSize = new System.Drawing.Size(0, 0);
            this.toolTipHtml.OwnerDraw = true;
            this.toolTipHtml.ReshowDelay = 100;
            this.toolTipHtml.ShowAlways = true;
            this.toolTipHtml.TooltipCssClass = "htmltooltip";
            // 
            // btGetHelp
            // 
            this.btGetHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btGetHelp.BackGrndImage = null;
            this.btGetHelp.Location = new System.Drawing.Point(473, 23);
            this.btGetHelp.Name = "btGetHelp";
            this.btGetHelp.Size = new System.Drawing.Size(20, 20);
            this.btGetHelp.TabIndex = 24;
            this.btGetHelp.Text = "yamuiImageButton1";
            // 
            // lbNbErrors
            // 
            this.lbNbErrors.Function = YamuiFramework.Fonts.FontFunction.Small;
            this.lbNbErrors.Location = new System.Drawing.Point(70, 23);
            this.lbNbErrors.Margin = new System.Windows.Forms.Padding(3);
            this.lbNbErrors.Name = "lbNbErrors";
            this.lbNbErrors.Size = new System.Drawing.Size(20, 20);
            this.lbNbErrors.TabIndex = 23;
            this.lbNbErrors.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbErrorText
            // 
            this.lbErrorText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbErrorText.Function = YamuiFramework.Fonts.FontFunction.Small;
            this.lbErrorText.Location = new System.Drawing.Point(93, 23);
            this.lbErrorText.Margin = new System.Windows.Forms.Padding(3);
            this.lbErrorText.Name = "lbErrorText";
            this.lbErrorText.Size = new System.Drawing.Size(374, 20);
            this.lbErrorText.TabIndex = 22;
            this.lbErrorText.Text = "errors";
            this.lbErrorText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btClearAllErrors
            // 
            this.btClearAllErrors.BackGrndImage = null;
            this.btClearAllErrors.Location = new System.Drawing.Point(44, 23);
            this.btClearAllErrors.Name = "btClearAllErrors";
            this.btClearAllErrors.Size = new System.Drawing.Size(20, 20);
            this.btClearAllErrors.TabIndex = 21;
            this.btClearAllErrors.Text = "yamuiImageButton1";
            // 
            // btNextError
            // 
            this.btNextError.BackGrndImage = null;
            this.btNextError.Location = new System.Drawing.Point(24, 23);
            this.btNextError.Name = "btNextError";
            this.btNextError.Size = new System.Drawing.Size(20, 20);
            this.btNextError.TabIndex = 20;
            this.btNextError.Text = "yamuiImageButton1";
            // 
            // lbStatus
            // 
            this.lbStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbStatus.Function = YamuiFramework.Fonts.FontFunction.Small;
            this.lbStatus.Location = new System.Drawing.Point(102, 1);
            this.lbStatus.Margin = new System.Windows.Forms.Padding(3);
            this.lbStatus.Name = "lbStatus";
            this.lbStatus.Size = new System.Drawing.Size(394, 19);
            this.lbStatus.TabIndex = 19;
            this.lbStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btPrevError
            // 
            this.btPrevError.BackGrndImage = null;
            this.btPrevError.Location = new System.Drawing.Point(4, 23);
            this.btPrevError.Name = "btPrevError";
            this.btPrevError.Size = new System.Drawing.Size(20, 20);
            this.btPrevError.TabIndex = 18;
            this.btPrevError.Text = "yamuiImageButton1";
            // 
            // yamuiLabel1
            // 
            this.yamuiLabel1.AutoSize = true;
            this.yamuiLabel1.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel1.Location = new System.Drawing.Point(4, 1);
            this.yamuiLabel1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 7);
            this.yamuiLabel1.Name = "yamuiLabel1";
            this.yamuiLabel1.Size = new System.Drawing.Size(90, 19);
            this.yamuiLabel1.TabIndex = 17;
            this.yamuiLabel1.Text = "FILE STATUS";
            // 
            // yamuiLabel2
            // 
            this.yamuiLabel2.AutoSize = true;
            this.yamuiLabel2.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel2.Location = new System.Drawing.Point(4, 49);
            this.yamuiLabel2.Margin = new System.Windows.Forms.Padding(5, 3, 5, 7);
            this.yamuiLabel2.Name = "yamuiLabel2";
            this.yamuiLabel2.Size = new System.Drawing.Size(70, 19);
            this.yamuiLabel2.TabIndex = 25;
            this.yamuiLabel2.Text = "ACTIONS";
            // 
            // yamuiLabel3
            // 
            this.yamuiLabel3.AutoSize = true;
            this.yamuiLabel3.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel3.Location = new System.Drawing.Point(4, 104);
            this.yamuiLabel3.Margin = new System.Windows.Forms.Padding(5, 3, 5, 7);
            this.yamuiLabel3.Name = "yamuiLabel3";
            this.yamuiLabel3.Size = new System.Drawing.Size(114, 19);
            this.yamuiLabel3.TabIndex = 27;
            this.yamuiLabel3.Text = "FILES EXPLORER";
            // 
            // FileName
            // 
            this.FileName.AspectName = "FileName";
            this.FileName.FillsFreeSpace = true;
            this.FileName.IsEditable = false;
            this.FileName.ShowTextInHeader = false;
            this.FileName.Text = "";
            // 
            // ovl
            // 
            this.ovl.AllColumns.Add(this.FileName);
            this.ovl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ovl.AutoArrange = false;
            this.ovl.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ovl.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.FileName});
            this.ovl.FullRowSelect = true;
            this.ovl.HeaderMaximumHeight = 0;
            this.ovl.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.ovl.HideSelection = false;
            this.ovl.LabelWrap = false;
            this.ovl.Location = new System.Drawing.Point(2, 174);
            this.ovl.MultiSelect = false;
            this.ovl.Name = "ovl";
            this.ovl.OwnerDraw = true;
            this.ovl.RowHeight = 20;
            this.ovl.SelectAllOnControlA = false;
            this.ovl.ShowGroups = false;
            this.ovl.ShowHeaderInAllViews = false;
            this.ovl.ShowSortIndicators = false;
            this.ovl.Size = new System.Drawing.Size(494, 318);
            this.ovl.SortGroupItemsByPrimaryColumn = false;
            this.ovl.TabIndex = 28;
            this.ovl.UseCellFormatEvents = true;
            this.ovl.UseCompatibleStateImageBehavior = false;
            this.ovl.UseFiltering = true;
            this.ovl.UseHotItem = true;
            this.ovl.UseTabAsInput = true;
            this.ovl.View = System.Windows.Forms.View.Details;
            this.ovl.VirtualMode = true;
            // 
            // yamuiLabel4
            // 
            this.yamuiLabel4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.yamuiLabel4.AutoSize = true;
            this.yamuiLabel4.Function = YamuiFramework.Fonts.FontFunction.Small;
            this.yamuiLabel4.Location = new System.Drawing.Point(4, 495);
            this.yamuiLabel4.Margin = new System.Windows.Forms.Padding(0);
            this.yamuiLabel4.Name = "yamuiLabel4";
            this.yamuiLabel4.Size = new System.Drawing.Size(43, 12);
            this.yamuiLabel4.TabIndex = 29;
            this.yamuiLabel4.Text = "Showing";
            this.yamuiLabel4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // nbitems
            // 
            this.nbitems.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nbitems.AutoSize = true;
            this.nbitems.Function = YamuiFramework.Fonts.FontFunction.Small;
            this.nbitems.Location = new System.Drawing.Point(4, 508);
            this.nbitems.Margin = new System.Windows.Forms.Padding(3);
            this.nbitems.Name = "nbitems";
            this.nbitems.Size = new System.Drawing.Size(43, 12);
            this.nbitems.TabIndex = 30;
            this.nbitems.Text = "Showing";
            this.nbitems.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btRefresh
            // 
            this.btRefresh.BackGrndImage = null;
            this.btRefresh.Location = new System.Drawing.Point(4, 148);
            this.btRefresh.Name = "btRefresh";
            this.btRefresh.Size = new System.Drawing.Size(20, 20);
            this.btRefresh.TabIndex = 31;
            this.btRefresh.Text = "yamuiImageButton1";
            // 
            // textFilter
            // 
            this.textFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textFilter.Lines = new string[0];
            this.textFilter.Location = new System.Drawing.Point(30, 148);
            this.textFilter.MaxLength = 32767;
            this.textFilter.Name = "textFilter";
            this.textFilter.PasswordChar = '\0';
            this.textFilter.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.textFilter.SelectedText = "";
            this.textFilter.Size = new System.Drawing.Size(437, 20);
            this.textFilter.TabIndex = 32;
            this.textFilter.WaterMark = "Filter here!";
            // 
            // btErase
            // 
            this.btErase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btErase.BackGrndImage = null;
            this.btErase.Location = new System.Drawing.Point(473, 148);
            this.btErase.Name = "btErase";
            this.btErase.Size = new System.Drawing.Size(20, 20);
            this.btErase.TabIndex = 33;
            this.btErase.Text = "yamuiImageButton2";
            // 
            // btDirectory
            // 
            this.btDirectory.BackGrndImage = null;
            this.btDirectory.Location = new System.Drawing.Point(4, 128);
            this.btDirectory.Name = "btDirectory";
            this.btDirectory.Size = new System.Drawing.Size(20, 20);
            this.btDirectory.TabIndex = 34;
            this.btDirectory.Text = "yamuiImageButton1";
            // 
            // lbDirectory
            // 
            this.lbDirectory.BackColor = System.Drawing.Color.Transparent;
            this.lbDirectory.Function = YamuiFramework.Fonts.FontFunction.Small;
            this.lbDirectory.Location = new System.Drawing.Point(30, 130);
            this.lbDirectory.Margin = new System.Windows.Forms.Padding(3);
            this.lbDirectory.Name = "lbDirectory";
            this.lbDirectory.Size = new System.Drawing.Size(350, 18);
            this.lbDirectory.TabIndex = 35;
            this.lbDirectory.Text = "yamuiLabel5";
            this.lbDirectory.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btGotoDir
            // 
            this.btGotoDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btGotoDir.BackGrndImage = null;
            this.btGotoDir.Location = new System.Drawing.Point(473, 128);
            this.btGotoDir.Name = "btGotoDir";
            this.btGotoDir.Size = new System.Drawing.Size(20, 20);
            this.btGotoDir.TabIndex = 36;
            this.btGotoDir.Text = "yamuiImageButton2";
            // 
            // yamuiImageButton1
            // 
            this.yamuiImageButton1.BackGrndImage = null;
            this.yamuiImageButton1.Location = new System.Drawing.Point(4, 78);
            this.yamuiImageButton1.Name = "yamuiImageButton1";
            this.yamuiImageButton1.Size = new System.Drawing.Size(20, 20);
            this.yamuiImageButton1.TabIndex = 37;
            this.yamuiImageButton1.Text = "yamuiImageButton1";
            // 
            // FileExplorerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(497, 525);
            this.Controls.Add(this.yamuiImageButton1);
            this.Controls.Add(this.btGotoDir);
            this.Controls.Add(this.lbDirectory);
            this.Controls.Add(this.btDirectory);
            this.Controls.Add(this.btErase);
            this.Controls.Add(this.textFilter);
            this.Controls.Add(this.btRefresh);
            this.Controls.Add(this.nbitems);
            this.Controls.Add(this.yamuiLabel4);
            this.Controls.Add(this.ovl);
            this.Controls.Add(this.yamuiLabel3);
            this.Controls.Add(this.yamuiLabel2);
            this.Controls.Add(this.btGetHelp);
            this.Controls.Add(this.lbNbErrors);
            this.Controls.Add(this.lbErrorText);
            this.Controls.Add(this.btClearAllErrors);
            this.Controls.Add(this.btNextError);
            this.Controls.Add(this.lbStatus);
            this.Controls.Add(this.btPrevError);
            this.Controls.Add(this.yamuiLabel1);
            this.Name = "FileExplorerForm";
            this.Text = "FileExplorerForm";
            ((System.ComponentModel.ISupportInitialize)(this.ovl)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private HtmlToolTip toolTipHtml;
        private YamuiImageButton btGetHelp;
        private YamuiLabel lbNbErrors;
        private YamuiLabel lbErrorText;
        private YamuiImageButton btClearAllErrors;
        private YamuiImageButton btNextError;
        private YamuiLabel lbStatus;
        private YamuiImageButton btPrevError;
        private YamuiLabel yamuiLabel1;
        private YamuiLabel yamuiLabel2;
        private YamuiLabel yamuiLabel3;
        private OLVColumn FileName;
        private FastObjectListView ovl;
        private YamuiLabel yamuiLabel4;
        private YamuiLabel nbitems;
        private YamuiImageButton btRefresh;
        private YamuiTextBoxAlt textFilter;
        private YamuiImageButton btErase;
        private YamuiImageButton btDirectory;
        private YamuiLabel lbDirectory;
        private YamuiImageButton btGotoDir;
        private YamuiImageButton yamuiImageButton1;


    }
}