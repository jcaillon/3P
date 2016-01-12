using System;
using YamuiFramework.Helper;
using _3PA.Interop;

namespace _3PA.MainFeatures.NppInterfaceForm {
    partial class NppDockableDialog {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {

            _masterForm.VisibleChanged -= Cover_OnVisibleChanged;
            _masterForm.Closed -= MasterFormOnClosed;

            _masterForm.ClientSizeChanged -= RefreshPosAndLoc;
            _masterForm.LocationChanged -= RefreshPosAndLoc;
            _masterForm.LostFocus -= RefreshPosAndLoc;
            _masterForm.GotFocus -= RefreshPosAndLoc;

            if (!Owner.IsDisposed && Environment.OSVersion.Version.Major >= 6) {
                int value = 0;
                WinApi.DwmSetWindowAttribute(Owner.Handle, WinApi.DwmwaTransitionsForcedisabled, ref value, 4);
            }

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
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Text = "NppDockableDialog";
        }

        #endregion
    }
}