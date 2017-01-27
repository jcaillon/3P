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
            this.btDirectory = new YamuiFramework.Controls.YamuiButtonImage();
            this.lbDirectory = new YamuiFramework.Controls.YamuiLabel();
            this.btGotoDir = new YamuiFramework.Controls.YamuiButtonImage();
            this.yamuiLabel2 = new YamuiFramework.Controls.YamuiLabel();
            this.lblEnv = new YamuiFramework.Controls.YamuiLabel();
            this.btEnvList = new YamuiFramework.Controls.YamuiButtonImage();
            this.btEnvModify = new YamuiFramework.Controls.YamuiButtonImage();
            this.btStopExecution = new YamuiFramework.Controls.YamuiButtonImage();
            this.btBringProcessToFront = new YamuiFramework.Controls.YamuiButtonImage();
            this.filterbox = new YamuiFramework.Controls.YamuiList.YamuiFilterBox();
            this.yamuiList = new YamuiFramework.Controls.YamuiList.YamuiFilteredTypeTreeList();
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
            // 
            // btGetHelp
            // 
            this.btGetHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btGetHelp.BackGrndImage = null;
            this.btGetHelp.GreyScaleBackGrndImage = null;
            this.btGetHelp.IsFocused = false;
            this.btGetHelp.IsHovered = false;
            this.btGetHelp.IsPressed = false;
            this.btGetHelp.Location = new System.Drawing.Point(339, 23);
            this.btGetHelp.Name = "btGetHelp";
            this.btGetHelp.SetImgSize = new System.Drawing.Size(0, 0);
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
            this.lbErrorText.Size = new System.Drawing.Size(240, 20);
            this.lbErrorText.TabIndex = 22;
            this.lbErrorText.Text = "errors";
            this.lbErrorText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btClearAllErrors
            // 
            this.btClearAllErrors.BackGrndImage = null;
            this.btClearAllErrors.GreyScaleBackGrndImage = null;
            this.btClearAllErrors.IsFocused = false;
            this.btClearAllErrors.IsHovered = false;
            this.btClearAllErrors.IsPressed = false;
            this.btClearAllErrors.Location = new System.Drawing.Point(44, 23);
            this.btClearAllErrors.Name = "btClearAllErrors";
            this.btClearAllErrors.SetImgSize = new System.Drawing.Size(0, 0);
            this.btClearAllErrors.Size = new System.Drawing.Size(20, 20);
            this.btClearAllErrors.TabIndex = 21;
            this.btClearAllErrors.Text = "yamuiImageButton1";
            // 
            // btNextError
            // 
            this.btNextError.BackGrndImage = null;
            this.btNextError.GreyScaleBackGrndImage = null;
            this.btNextError.IsFocused = false;
            this.btNextError.IsHovered = false;
            this.btNextError.IsPressed = false;
            this.btNextError.Location = new System.Drawing.Point(24, 23);
            this.btNextError.Name = "btNextError";
            this.btNextError.SetImgSize = new System.Drawing.Size(0, 0);
            this.btNextError.Size = new System.Drawing.Size(20, 20);
            this.btNextError.TabIndex = 20;
            this.btNextError.Text = "yamuiImageButton1";
            // 
            // lbStatus
            // 
            this.lbStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbStatus.Function = YamuiFramework.Fonts.FontFunction.Small;
            this.lbStatus.Location = new System.Drawing.Point(112, 1);
            this.lbStatus.Margin = new System.Windows.Forms.Padding(3);
            this.lbStatus.Name = "lbStatus";
            this.lbStatus.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
            this.lbStatus.Size = new System.Drawing.Size(250, 20);
            this.lbStatus.TabIndex = 19;
            this.lbStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btPrevError
            // 
            this.btPrevError.BackGrndImage = null;
            this.btPrevError.GreyScaleBackGrndImage = null;
            this.btPrevError.IsFocused = false;
            this.btPrevError.IsHovered = false;
            this.btPrevError.IsPressed = false;
            this.btPrevError.Location = new System.Drawing.Point(4, 23);
            this.btPrevError.Name = "btPrevError";
            this.btPrevError.SetImgSize = new System.Drawing.Size(0, 0);
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
            // btDirectory
            // 
            this.btDirectory.BackGrndImage = null;
            this.btDirectory.GreyScaleBackGrndImage = null;
            this.btDirectory.IsFocused = false;
            this.btDirectory.IsHovered = false;
            this.btDirectory.IsPressed = false;
            this.btDirectory.Location = new System.Drawing.Point(4, 115);
            this.btDirectory.Name = "btDirectory";
            this.btDirectory.SetImgSize = new System.Drawing.Size(0, 0);
            this.btDirectory.Size = new System.Drawing.Size(20, 20);
            this.btDirectory.TabIndex = 34;
            this.btDirectory.Text = "yamuiImageButton1";
            // 
            // lbDirectory
            // 
            this.lbDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbDirectory.BackColor = System.Drawing.Color.Transparent;
            this.lbDirectory.Function = YamuiFramework.Fonts.FontFunction.Small;
            this.lbDirectory.Location = new System.Drawing.Point(30, 117);
            this.lbDirectory.Margin = new System.Windows.Forms.Padding(3);
            this.lbDirectory.Name = "lbDirectory";
            this.lbDirectory.Size = new System.Drawing.Size(303, 18);
            this.lbDirectory.TabIndex = 35;
            this.lbDirectory.Text = "yamuiLabel5";
            this.lbDirectory.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btGotoDir
            // 
            this.btGotoDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btGotoDir.BackGrndImage = null;
            this.btGotoDir.GreyScaleBackGrndImage = null;
            this.btGotoDir.IsFocused = false;
            this.btGotoDir.IsHovered = false;
            this.btGotoDir.IsPressed = false;
            this.btGotoDir.Location = new System.Drawing.Point(339, 115);
            this.btGotoDir.Name = "btGotoDir";
            this.btGotoDir.SetImgSize = new System.Drawing.Size(0, 0);
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
            this.lblEnv.Size = new System.Drawing.Size(309, 20);
            this.lblEnv.TabIndex = 38;
            this.lblEnv.Text = "BOI - A";
            this.lblEnv.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btEnvList
            // 
            this.btEnvList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btEnvList.BackGrndImage = null;
            this.btEnvList.GreyScaleBackGrndImage = null;
            this.btEnvList.IsFocused = false;
            this.btEnvList.IsHovered = false;
            this.btEnvList.IsPressed = false;
            this.btEnvList.Location = new System.Drawing.Point(339, 69);
            this.btEnvList.Name = "btEnvList";
            this.btEnvList.SetImgSize = new System.Drawing.Size(0, 0);
            this.btEnvList.Size = new System.Drawing.Size(20, 20);
            this.btEnvList.TabIndex = 39;
            this.btEnvList.Text = "yamuiImageButton1";
            // 
            // btEnvModify
            // 
            this.btEnvModify.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btEnvModify.BackGrndImage = null;
            this.btEnvModify.GreyScaleBackGrndImage = null;
            this.btEnvModify.IsFocused = false;
            this.btEnvModify.IsHovered = false;
            this.btEnvModify.IsPressed = false;
            this.btEnvModify.Location = new System.Drawing.Point(319, 69);
            this.btEnvModify.Name = "btEnvModify";
            this.btEnvModify.SetImgSize = new System.Drawing.Size(0, 0);
            this.btEnvModify.Size = new System.Drawing.Size(20, 20);
            this.btEnvModify.TabIndex = 40;
            this.btEnvModify.Text = "yamuiImageButton1";
            // 
            // btStopExecution
            // 
            this.btStopExecution.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btStopExecution.BackGrndImage = null;
            this.btStopExecution.GreyScaleBackGrndImage = null;
            this.btStopExecution.IsFocused = false;
            this.btStopExecution.IsHovered = false;
            this.btStopExecution.IsPressed = false;
            this.btStopExecution.Location = new System.Drawing.Point(339, 1);
            this.btStopExecution.Name = "btStopExecution";
            this.btStopExecution.SetImgSize = new System.Drawing.Size(0, 0);
            this.btStopExecution.Size = new System.Drawing.Size(20, 20);
            this.btStopExecution.TabIndex = 41;
            this.btStopExecution.Text = "yamuiImageButton1";
            // 
            // btBringProcessToFront
            // 
            this.btBringProcessToFront.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btBringProcessToFront.BackGrndImage = null;
            this.btBringProcessToFront.GreyScaleBackGrndImage = null;
            this.btBringProcessToFront.IsFocused = false;
            this.btBringProcessToFront.IsHovered = false;
            this.btBringProcessToFront.IsPressed = false;
            this.btBringProcessToFront.Location = new System.Drawing.Point(319, 1);
            this.btBringProcessToFront.Name = "btBringProcessToFront";
            this.btBringProcessToFront.SetImgSize = new System.Drawing.Size(0, 0);
            this.btBringProcessToFront.Size = new System.Drawing.Size(20, 20);
            this.btBringProcessToFront.TabIndex = 42;
            this.btBringProcessToFront.Text = "yamuiImageButton1";
            // 
            // filterbox
            // 
            this.filterbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.filterbox.Location = new System.Drawing.Point(4, 141);
            this.filterbox.Name = "filterbox";
            this.filterbox.Size = new System.Drawing.Size(355, 20);
            this.filterbox.TabIndex = 43;
            // 
            // yamuiList
            // 
            this.yamuiList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.yamuiList.EmptyListString = "Empty list!";
            this.yamuiList.Location = new System.Drawing.Point(0, 167);
            this.yamuiList.Name = "yamuiList";
            this.yamuiList.ScrollWidth = 10;
            this.yamuiList.Size = new System.Drawing.Size(362, 405);
            this.yamuiList.SortingClass = null;
            this.yamuiList.TabIndex = 44;
            this.yamuiList.UseCustomBackColor = false;
            // 
            // FileExplorerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(363, 572);
            this.Controls.Add(this.yamuiList);
            this.Controls.Add(this.filterbox);
            this.Controls.Add(this.btBringProcessToFront);
            this.Controls.Add(this.btStopExecution);
            this.Controls.Add(this.btEnvModify);
            this.Controls.Add(this.btEnvList);
            this.Controls.Add(this.lblEnv);
            this.Controls.Add(this.yamuiLabel2);
            this.Controls.Add(this.btGotoDir);
            this.Controls.Add(this.lbDirectory);
            this.Controls.Add(this.btDirectory);
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
        private YamuiButtonImage btDirectory;
        private YamuiLabel lbDirectory;
        private YamuiButtonImage btGotoDir;
        private YamuiLabel yamuiLabel2;
        private YamuiLabel lblEnv;
        private YamuiButtonImage btEnvList;
        private YamuiButtonImage btEnvModify;
        private YamuiButtonImage btStopExecution;
        private YamuiButtonImage btBringProcessToFront;
        private YamuiFramework.Controls.YamuiList.YamuiFilterBox filterbox;
        private YamuiFramework.Controls.YamuiList.YamuiFilteredTypeTreeList yamuiList;
    }
}