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
            this.btGetHelp = new YamuiFramework.Controls.YamuiButtonImage();
            this.lbNbErrors = new YamuiFramework.Controls.YamuiLabel();
            this.lbErrorText = new YamuiFramework.Controls.YamuiLabel();
            this.btClearAllErrors = new YamuiFramework.Controls.YamuiButtonImage();
            this.btNextError = new YamuiFramework.Controls.YamuiButtonImage();
            this.lbStatus = new YamuiFramework.Controls.YamuiLabel();
            this.btPrevError = new YamuiFramework.Controls.YamuiButtonImage();
            this.yamuiLabel1 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiLabel3 = new YamuiFramework.Controls.YamuiLabel();
            this.FileName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.fastOLV = new BrightIdeasSoftware.FastObjectListView();
            this.yamuiLabel4 = new YamuiFramework.Controls.YamuiLabel();
            this.nbitems = new YamuiFramework.Controls.YamuiLabel();
            this.btRefresh = new YamuiFramework.Controls.YamuiButtonImage();
            this.textFilter = new YamuiFramework.Controls.YamuiTextBox();
            this.btErase = new YamuiFramework.Controls.YamuiButtonImage();
            this.btDirectory = new YamuiFramework.Controls.YamuiButtonImage();
            this.lbDirectory = new YamuiFramework.Controls.YamuiLabel();
            this.btGotoDir = new YamuiFramework.Controls.YamuiButtonImage();
            this.yamuiLabel2 = new YamuiFramework.Controls.YamuiLabel();
            this.lblEnv = new YamuiFramework.Controls.YamuiLabel();
            this.btEnvList = new YamuiFramework.Controls.YamuiButtonImage();
            this.btEnvModify = new YamuiFramework.Controls.YamuiButtonImage();
            ((System.ComponentModel.ISupportInitialize)(this.fastOLV)).BeginInit();
            this.SuspendLayout();
            // 
            // toolTipHtml
            // 
            this.toolTipHtml.AllowLinksHandling = true;
            this.toolTipHtml.AutoPopDelay = 90000;
            this.toolTipHtml.BaseStylesheet = null;
            this.toolTipHtml.InitialDelay = 300;
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
            this.lbNbErrors.Text = "0";
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
            this.yamuiLabel1.Location = new System.Drawing.Point(2, 1);
            this.yamuiLabel1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 7);
            this.yamuiLabel1.Name = "yamuiLabel1";
            this.yamuiLabel1.Size = new System.Drawing.Size(90, 19);
            this.yamuiLabel1.TabIndex = 17;
            this.yamuiLabel1.Text = "FILE STATUS";
            // 
            // yamuiLabel3
            // 
            this.yamuiLabel3.AutoSize = true;
            this.yamuiLabel3.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel3.Location = new System.Drawing.Point(4, 93);
            this.yamuiLabel3.Margin = new System.Windows.Forms.Padding(5, 3, 5, 7);
            this.yamuiLabel3.Name = "yamuiLabel3";
            this.yamuiLabel3.Size = new System.Drawing.Size(114, 19);
            this.yamuiLabel3.TabIndex = 27;
            this.yamuiLabel3.Text = "FILES EXPLORER";
            // 
            // FileName
            // 
            this.FileName.AspectName = "DisplayText";
            this.FileName.FillsFreeSpace = true;
            this.FileName.IsEditable = false;
            this.FileName.ShowTextInHeader = false;
            this.FileName.Text = "";
            // 
            // fastOLV
            // 
            this.fastOLV.AllColumns.Add(this.FileName);
            this.fastOLV.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fastOLV.AutoArrange = false;
            this.fastOLV.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.fastOLV.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.FileName});
            this.fastOLV.FullRowSelect = true;
            this.fastOLV.HeaderMaximumHeight = 0;
            this.fastOLV.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.fastOLV.HideSelection = false;
            this.fastOLV.LabelWrap = false;
            this.fastOLV.Location = new System.Drawing.Point(2, 161);
            this.fastOLV.MultiSelect = false;
            this.fastOLV.Name = "fastOLV";
            this.fastOLV.OwnerDraw = true;
            this.fastOLV.RowHeight = 20;
            this.fastOLV.SelectAllOnControlA = false;
            this.fastOLV.ShowGroups = false;
            this.fastOLV.ShowHeaderInAllViews = false;
            this.fastOLV.ShowSortIndicators = false;
            this.fastOLV.Size = new System.Drawing.Size(494, 331);
            this.fastOLV.SortGroupItemsByPrimaryColumn = false;
            this.fastOLV.TabIndex = 28;
            this.fastOLV.UseCellFormatEvents = true;
            this.fastOLV.UseCompatibleStateImageBehavior = false;
            this.fastOLV.UseFiltering = true;
            this.fastOLV.UseHotItem = true;
            this.fastOLV.UseTabAsInput = true;
            this.fastOLV.View = System.Windows.Forms.View.Details;
            this.fastOLV.VirtualMode = true;
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
            this.btRefresh.Location = new System.Drawing.Point(4, 135);
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
            this.textFilter.Location = new System.Drawing.Point(30, 135);
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
            this.btErase.Location = new System.Drawing.Point(473, 135);
            this.btErase.Name = "btErase";
            this.btErase.Size = new System.Drawing.Size(20, 20);
            this.btErase.TabIndex = 33;
            this.btErase.Text = "yamuiImageButton2";
            // 
            // btDirectory
            // 
            this.btDirectory.BackGrndImage = null;
            this.btDirectory.Location = new System.Drawing.Point(4, 115);
            this.btDirectory.Name = "btDirectory";
            this.btDirectory.Size = new System.Drawing.Size(20, 20);
            this.btDirectory.TabIndex = 34;
            this.btDirectory.Text = "yamuiImageButton1";
            // 
            // lbDirectory
            // 
            this.lbDirectory.BackColor = System.Drawing.Color.Transparent;
            this.lbDirectory.Function = YamuiFramework.Fonts.FontFunction.Small;
            this.lbDirectory.Location = new System.Drawing.Point(30, 117);
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
            this.btGotoDir.Location = new System.Drawing.Point(473, 115);
            this.btGotoDir.Name = "btGotoDir";
            this.btGotoDir.Size = new System.Drawing.Size(20, 20);
            this.btGotoDir.TabIndex = 36;
            this.btGotoDir.Text = "yamuiImageButton2";
            // 
            // yamuiLabel2
            // 
            this.yamuiLabel2.AutoSize = true;
            this.yamuiLabel2.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel2.Location = new System.Drawing.Point(4, 47);
            this.yamuiLabel2.Margin = new System.Windows.Forms.Padding(5, 3, 5, 7);
            this.yamuiLabel2.Name = "yamuiLabel2";
            this.yamuiLabel2.Size = new System.Drawing.Size(177, 19);
            this.yamuiLabel2.TabIndex = 37;
            this.yamuiLabel2.Text = "CURRENT ENVIRONMENT";
            // 
            // lblEnv
            // 
            this.lblEnv.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblEnv.Function = YamuiFramework.Fonts.FontFunction.Small;
            this.lblEnv.Location = new System.Drawing.Point(4, 69);
            this.lblEnv.Margin = new System.Windows.Forms.Padding(3);
            this.lblEnv.Name = "lblEnv";
            this.lblEnv.Size = new System.Drawing.Size(443, 20);
            this.lblEnv.TabIndex = 38;
            this.lblEnv.Text = "BOI - A";
            this.lblEnv.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btEnvList
            // 
            this.btEnvList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btEnvList.BackGrndImage = null;
            this.btEnvList.Location = new System.Drawing.Point(473, 69);
            this.btEnvList.Name = "btEnvList";
            this.btEnvList.Size = new System.Drawing.Size(20, 20);
            this.btEnvList.TabIndex = 39;
            this.btEnvList.Text = "yamuiImageButton1";
            // 
            // btEnvModify
            // 
            this.btEnvModify.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btEnvModify.BackGrndImage = null;
            this.btEnvModify.Location = new System.Drawing.Point(453, 69);
            this.btEnvModify.Name = "btEnvModify";
            this.btEnvModify.Size = new System.Drawing.Size(20, 20);
            this.btEnvModify.TabIndex = 40;
            this.btEnvModify.Text = "yamuiImageButton1";
            // 
            // FileExplorerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(497, 525);
            this.Controls.Add(this.btEnvModify);
            this.Controls.Add(this.btEnvList);
            this.Controls.Add(this.lblEnv);
            this.Controls.Add(this.yamuiLabel2);
            this.Controls.Add(this.btGotoDir);
            this.Controls.Add(this.lbDirectory);
            this.Controls.Add(this.btDirectory);
            this.Controls.Add(this.btErase);
            this.Controls.Add(this.textFilter);
            this.Controls.Add(this.btRefresh);
            this.Controls.Add(this.nbitems);
            this.Controls.Add(this.yamuiLabel4);
            this.Controls.Add(this.fastOLV);
            this.Controls.Add(this.yamuiLabel3);
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
            ((System.ComponentModel.ISupportInitialize)(this.fastOLV)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private HtmlToolTip toolTipHtml;
        private YamuiButtonImage btGetHelp;
        private YamuiLabel lbNbErrors;
        private YamuiLabel lbErrorText;
        private YamuiButtonImage btClearAllErrors;
        private YamuiButtonImage btNextError;
        private YamuiLabel lbStatus;
        private YamuiButtonImage btPrevError;
        private YamuiLabel yamuiLabel1;
        private YamuiLabel yamuiLabel3;
        private OLVColumn FileName;
        private FastObjectListView fastOLV;
        private YamuiLabel yamuiLabel4;
        private YamuiLabel nbitems;
        private YamuiButtonImage btRefresh;
        private YamuiTextBox textFilter;
        private YamuiButtonImage btErase;
        private YamuiButtonImage btDirectory;
        private YamuiLabel lbDirectory;
        private YamuiButtonImage btGotoDir;
        private YamuiLabel yamuiLabel2;
        private YamuiLabel lblEnv;
        private YamuiButtonImage btEnvList;
        private YamuiButtonImage btEnvModify;


    }
}