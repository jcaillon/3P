using System.ComponentModel;
using YamuiFramework.Controls;
using YamuiFramework.Controls.YamuiList;

namespace _3PA.MainFeatures.Appli.Pages.Options {
    partial class ProfilesPage {
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.yamuiFilteredTypeTreeList1 = new YamuiFramework.Controls.YamuiList.YamuiFilteredTypeTreeList();
            this.yamuiScrollList1 = new YamuiFramework.Controls.YamuiList.YamuiScrollList();
            this.yamuiFilteredTypeList1 = new YamuiFramework.Controls.YamuiList.YamuiFilteredTypeList();
            this.btMinus = new YamuiFramework.Controls.YamuiButton();
            this.btPlus = new YamuiFramework.Controls.YamuiButton();
            this.yamuiButton4 = new YamuiFramework.Controls.YamuiButton();
            this.yamuiButton3 = new YamuiFramework.Controls.YamuiButton();
            this.yamuiButton2 = new YamuiFramework.Controls.YamuiButton();
            this.flFilter1 = new YamuiTextBox();
            this.yamuiButton1 = new YamuiFramework.Controls.YamuiButton();
            this.YamuiFilteredList1 = new YamuiFramework.Controls.YamuiList.YamuiFilteredList();
            this.yamuiLabel3 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiLabel2 = new YamuiFramework.Controls.YamuiLabel();
            this.yamuiLabel1 = new YamuiFramework.Controls.YamuiLabel();
            this.htmlToolTip1 = new YamuiFramework.HtmlRenderer.WinForms.HtmlToolTip();
            this.SuspendLayout();
            // 
            // scrollPanel
            // 
            // 
            // scrollPanel.ContentPanel
            // 
            this.Controls.Add(this.yamuiFilteredTypeTreeList1);
            this.Controls.Add(this.yamuiScrollList1);
            this.Controls.Add(this.yamuiFilteredTypeList1);
            this.Controls.Add(this.btMinus);
            this.Controls.Add(this.btPlus);
            this.Controls.Add(this.yamuiButton4);
            this.Controls.Add(this.yamuiButton3);
            this.Controls.Add(this.yamuiButton2);
            this.Controls.Add(this.flFilter1);
            this.Controls.Add(this.yamuiButton1);
            this.Controls.Add(this.YamuiFilteredList1);
            this.Controls.Add(this.yamuiLabel3);
            this.Controls.Add(this.yamuiLabel2);
            this.Controls.Add(this.yamuiLabel1);
            // 
            // yamuiFilteredTypeTreeList1
            // 
            this.yamuiFilteredTypeTreeList1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.yamuiFilteredTypeTreeList1.EmptyListString = "Too bad!";
            this.yamuiFilteredTypeTreeList1.Location = new System.Drawing.Point(385, 20);
            this.yamuiFilteredTypeTreeList1.Name = "yamuiFilteredTypeTreeList1";
            this.yamuiFilteredTypeTreeList1.ScrollWidth = 10;
            this.yamuiFilteredTypeTreeList1.Size = new System.Drawing.Size(198, 221);
            this.yamuiFilteredTypeTreeList1.TabIndex = 19;
            this.yamuiFilteredTypeTreeList1.UseCustomBackColor = false;
            // 
            // yamuiScrollList1
            // 
            this.yamuiScrollList1.EmptyListString = "Empty list!";
            this.yamuiScrollList1.Location = new System.Drawing.Point(29, 257);
            this.yamuiScrollList1.Name = "yamuiScrollList1";
            this.yamuiScrollList1.RowHeight = 18;
            this.yamuiScrollList1.ScrollWidth = 10;
            this.yamuiScrollList1.Size = new System.Drawing.Size(178, 221);
            this.yamuiScrollList1.TabIndex = 18;
            this.yamuiScrollList1.UseCustomBackColor = false;
            // 
            // yamuiFilteredTypeList1
            // 
            this.yamuiFilteredTypeList1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.yamuiFilteredTypeList1.EmptyListString = "Empty list!";
            this.yamuiFilteredTypeList1.Location = new System.Drawing.Point(385, 257);
            this.yamuiFilteredTypeList1.Name = "yamuiFilteredTypeList1";
            this.yamuiFilteredTypeList1.ScrollWidth = 10;
            this.yamuiFilteredTypeList1.Size = new System.Drawing.Size(198, 221);
            this.yamuiFilteredTypeList1.TabIndex = 17;
            this.yamuiFilteredTypeList1.UseCustomBackColor = false;
            // 
            // btMinus
            // 
            this.btMinus.BackGrndImage = null;
            this.btMinus.GreyScaleBackGrndImage = null;
            this.btMinus.IsFocused = false;
            this.btMinus.IsHovered = false;
            this.btMinus.IsPressed = false;
            this.btMinus.Location = new System.Drawing.Point(625, 257);
            this.btMinus.Name = "btMinus";
            this.btMinus.SetImgSize = new System.Drawing.Size(0, 0);
            this.btMinus.Size = new System.Drawing.Size(26, 23);
            this.btMinus.TabIndex = 16;
            this.btMinus.Text = "-";
            // 
            // btPlus
            // 
            this.btPlus.BackGrndImage = null;
            this.btPlus.GreyScaleBackGrndImage = null;
            this.btPlus.IsFocused = false;
            this.btPlus.IsHovered = false;
            this.btPlus.IsPressed = false;
            this.btPlus.Location = new System.Drawing.Point(593, 257);
            this.btPlus.Name = "btPlus";
            this.btPlus.SetImgSize = new System.Drawing.Size(0, 0);
            this.btPlus.Size = new System.Drawing.Size(26, 23);
            this.btPlus.TabIndex = 15;
            this.btPlus.Text = "+";
            // 
            // yamuiButton4
            // 
            this.yamuiButton4.BackGrndImage = null;
            this.yamuiButton4.GreyScaleBackGrndImage = null;
            this.yamuiButton4.IsFocused = false;
            this.yamuiButton4.IsHovered = false;
            this.yamuiButton4.IsPressed = false;
            this.yamuiButton4.Location = new System.Drawing.Point(272, 160);
            this.yamuiButton4.Name = "yamuiButton4";
            this.yamuiButton4.SetImgSize = new System.Drawing.Size(0, 0);
            this.yamuiButton4.Size = new System.Drawing.Size(75, 23);
            this.yamuiButton4.TabIndex = 13;
            this.yamuiButton4.Text = "4";
            // 
            // yamuiButton3
            // 
            this.yamuiButton3.BackGrndImage = null;
            this.yamuiButton3.GreyScaleBackGrndImage = null;
            this.yamuiButton3.IsFocused = false;
            this.yamuiButton3.IsHovered = false;
            this.yamuiButton3.IsPressed = false;
            this.yamuiButton3.Location = new System.Drawing.Point(191, 160);
            this.yamuiButton3.Name = "yamuiButton3";
            this.yamuiButton3.SetImgSize = new System.Drawing.Size(0, 0);
            this.yamuiButton3.Size = new System.Drawing.Size(75, 23);
            this.yamuiButton3.TabIndex = 12;
            this.yamuiButton3.Text = "3";
            // 
            // yamuiButton2
            // 
            this.yamuiButton2.BackGrndImage = null;
            this.yamuiButton2.GreyScaleBackGrndImage = null;
            this.yamuiButton2.IsFocused = false;
            this.yamuiButton2.IsHovered = false;
            this.yamuiButton2.IsPressed = false;
            this.yamuiButton2.Location = new System.Drawing.Point(110, 160);
            this.yamuiButton2.Name = "yamuiButton2";
            this.yamuiButton2.SetImgSize = new System.Drawing.Size(0, 0);
            this.yamuiButton2.Size = new System.Drawing.Size(75, 23);
            this.yamuiButton2.TabIndex = 11;
            this.yamuiButton2.Text = "2";
            // 
            // flFilter1
            // 
            this.flFilter1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.flFilter1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.flFilter1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.flFilter1.Location = new System.Drawing.Point(213, 231);
            this.flFilter1.Name = "flFilter1";
            this.flFilter1.Size = new System.Drawing.Size(166, 20);
            this.flFilter1.TabIndex = 10;
            this.flFilter1.WaterMark = "fuckkk";
            // 
            // yamuiButton1
            // 
            this.yamuiButton1.BackGrndImage = null;
            this.yamuiButton1.GreyScaleBackGrndImage = null;
            this.yamuiButton1.IsFocused = false;
            this.yamuiButton1.IsHovered = false;
            this.yamuiButton1.IsPressed = false;
            this.yamuiButton1.Location = new System.Drawing.Point(29, 160);
            this.yamuiButton1.Name = "yamuiButton1";
            this.yamuiButton1.SetImgSize = new System.Drawing.Size(0, 0);
            this.yamuiButton1.Size = new System.Drawing.Size(75, 23);
            this.yamuiButton1.TabIndex = 9;
            this.yamuiButton1.Text = "1";
            // 
            // YamuiFilteredList1
            // 
            this.YamuiFilteredList1.EmptyListString = "Empty list!";
            this.YamuiFilteredList1.Location = new System.Drawing.Point(213, 257);
            this.YamuiFilteredList1.Name = "YamuiFilteredList1";
            this.YamuiFilteredList1.ScrollWidth = 10;
            this.YamuiFilteredList1.Size = new System.Drawing.Size(166, 221);
            this.YamuiFilteredList1.TabIndex = 14;
            this.YamuiFilteredList1.UseCustomBackColor = false;
            // 
            // yamuiLabel3
            // 
            this.yamuiLabel3.AutoSize = true;
            this.yamuiLabel3.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel3.Location = new System.Drawing.Point(0, 131);
            this.yamuiLabel3.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel3.Name = "yamuiLabel3";
            this.yamuiLabel3.Size = new System.Drawing.Size(183, 19);
            this.yamuiLabel3.TabIndex = 7;
            this.yamuiLabel3.Text = "SHARED CONFIGURATION";
            // 
            // yamuiLabel2
            // 
            this.yamuiLabel2.AutoSize = true;
            this.yamuiLabel2.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel2.Location = new System.Drawing.Point(0, 59);
            this.yamuiLabel2.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel2.Name = "yamuiLabel2";
            this.yamuiLabel2.Size = new System.Drawing.Size(225, 19);
            this.yamuiLabel2.TabIndex = 6;
            this.yamuiLabel2.Text = "EXTERNALISED CONFIGURATION";
            // 
            // yamuiLabel1
            // 
            this.yamuiLabel1.AutoSize = true;
            this.yamuiLabel1.Function = YamuiFramework.Fonts.FontFunction.Heading;
            this.yamuiLabel1.Location = new System.Drawing.Point(0, 0);
            this.yamuiLabel1.Margin = new System.Windows.Forms.Padding(5, 18, 5, 7);
            this.yamuiLabel1.Name = "yamuiLabel1";
            this.yamuiLabel1.Size = new System.Drawing.Size(133, 19);
            this.yamuiLabel1.TabIndex = 5;
            this.yamuiLabel1.Text = "SET YOUR PROFILE";
            // 
            // htmlToolTip1
            // 
            this.htmlToolTip1.AllowLinksHandling = true;
            this.htmlToolTip1.AutoPopDelay = 90000;
            this.htmlToolTip1.BaseStylesheet = null;
            this.htmlToolTip1.InitialDelay = 300;
            this.htmlToolTip1.MaximumSize = new System.Drawing.Size(0, 0);
            this.htmlToolTip1.OwnerDraw = true;
            this.htmlToolTip1.ReshowDelay = 100;
            // 
            // ProfilesPage
            // 
            this.Name = "ProfilesPage";
            this.Size = new System.Drawing.Size(900, 650);
            this.ResumeLayout(false);

        }

        #endregion
        
        private YamuiLabel yamuiLabel3;
        private YamuiLabel yamuiLabel2;
        private YamuiLabel yamuiLabel1;
        private YamuiFilteredList YamuiFilteredList1;
        private YamuiTextBox flFilter1;
        private YamuiButton yamuiButton1;
        private YamuiFramework.HtmlRenderer.WinForms.HtmlToolTip htmlToolTip1;
        private YamuiButton yamuiButton2;
        private YamuiButton yamuiButton3;
        private YamuiButton yamuiButton4;
        private YamuiButton btMinus;
        private YamuiButton btPlus;
        private YamuiFilteredTypeList yamuiFilteredTypeList1;
        private YamuiScrollList yamuiScrollList1;
        private YamuiFilteredTypeTreeList yamuiFilteredTypeTreeList1;
    }
}
