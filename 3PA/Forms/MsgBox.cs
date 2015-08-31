using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace _3PA.Forms
{

/*
 DialogResult result = MsgBox.Show("Are you sure you want to exit?", "Exit", MsgBox.Buttons.YesNo, MsgBox.Icon.Info, MsgBox.AnimateStyle.SlideDown);

        if (result == DialogResult.Yes)
        {
            MessageBox.Show("Exiting now");
        }
 */

    class MsgBox : Form
    {
        private const int CS_DROPSHADOW = 0x00020000;
        private static MsgBox _msgBox;
        private Panel _plHeader = new Panel();
        private Panel _plFooter = new Panel();
        private Panel _plIcon = new Panel();
        private PictureBox _picIcon = new PictureBox();
        private FlowLayoutPanel _flpButtons = new FlowLayoutPanel();
        private Label _lblTitle;
        private Label _lblMessage;
        private List<Button> _buttonCollection = new List<Button>();
        private static DialogResult _buttonResult;
        private static Timer _timer;
        private static Point lastMousePos;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool MessageBeep(uint type);

        private MsgBox()
        {
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.FromArgb(45, 45, 48);
            StartPosition = FormStartPosition.CenterScreen;
            Padding = new Padding(3);
            Width = 400;

            _lblTitle = new Label();
            _lblTitle.ForeColor = Color.White;
            _lblTitle.Font = new Font("Segoe UI", 18);
            _lblTitle.Dock = DockStyle.Top;
            _lblTitle.Height = 50;

            _lblMessage = new Label();
            _lblMessage.ForeColor = Color.White;
            _lblMessage.Font = new Font("Segoe UI", 10);
            _lblMessage.Dock = DockStyle.Fill;

            _flpButtons.FlowDirection = FlowDirection.RightToLeft;
            _flpButtons.Dock = DockStyle.Fill;

            _plHeader.Dock = DockStyle.Fill;
            _plHeader.Padding = new Padding(20);
            _plHeader.Controls.Add(_lblMessage);
            _plHeader.Controls.Add(_lblTitle);

            _plFooter.Dock = DockStyle.Bottom;
            _plFooter.Padding = new Padding(20);
            _plFooter.BackColor = Color.FromArgb(37, 37, 38);
            _plFooter.Height = 80;
            _plFooter.Controls.Add(_flpButtons);

            _picIcon.Width = 32;
            _picIcon.Height = 32;
            _picIcon.Location = new Point(30, 50);

            _plIcon.Dock = DockStyle.Left;
            _plIcon.Padding = new Padding(20);
            _plIcon.Width = 70;
            _plIcon.Controls.Add(_picIcon);

            List<Control> controlCollection = new List<Control>();
            controlCollection.Add(this);
            controlCollection.Add(_lblTitle);
            controlCollection.Add(_lblMessage);
            controlCollection.Add(_flpButtons);
            controlCollection.Add(_plHeader);
            controlCollection.Add(_plFooter);
            controlCollection.Add(_plIcon);
            controlCollection.Add(_picIcon);

            foreach (Control control in controlCollection)
            {
                control.MouseDown += MsgBox_MouseDown;
                control.MouseMove += MsgBox_MouseMove;
            }

            Controls.Add(_plHeader);
            Controls.Add(_plIcon);
            Controls.Add(_plFooter);
        }

        private static void MsgBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                lastMousePos = new Point(e.X, e.Y);
            }
        }


        private static void MsgBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _msgBox.Left += e.X - lastMousePos.X;
                _msgBox.Top += e.Y - lastMousePos.Y;
            }
        }

        public static DialogResult Show(string message)
        {
            _msgBox = new MsgBox();
            _msgBox._lblMessage.Text = message;

            InitButtons(Buttons.OK);

            _msgBox.ShowDialog();
            MessageBeep(0);
            return _buttonResult;
        }

        public static DialogResult Show(string message, string title)
        {
            _msgBox = new MsgBox();
            _msgBox._lblMessage.Text = message;
            _msgBox._lblTitle.Text = title;
            _msgBox.Size = MessageSize(message);

            InitButtons(Buttons.OK);

            _msgBox.ShowDialog();
            MessageBeep(0);
            return _buttonResult;
        }

        public static DialogResult Show(string message, string title, Buttons buttons)
        {
            _msgBox = new MsgBox();
            _msgBox._lblMessage.Text = message;
            _msgBox._lblTitle.Text = title;
            _msgBox._plIcon.Hide();

            InitButtons(buttons);

            _msgBox.Size = MessageSize(message);
            _msgBox.ShowDialog();
            MessageBeep(0);
            return _buttonResult;
        }

        public static DialogResult Show(string message, string title, Buttons buttons, Icon icon)
        {
            _msgBox = new MsgBox();
            _msgBox._lblMessage.Text = message;
            _msgBox._lblTitle.Text = title;

            InitButtons(buttons);
            InitIcon(icon);

            _msgBox.Size = MessageSize(message);
            _msgBox.ShowDialog();
            MessageBeep(0);
            return _buttonResult;
        }

        public static DialogResult Show(string message, string title, Buttons buttons, Icon icon, AnimateStyle style)
        {
            _msgBox = new MsgBox();
            _msgBox._lblMessage.Text = message;
            _msgBox._lblTitle.Text = title;
            _msgBox.Height = 0;

            InitButtons(buttons);
            InitIcon(icon);

            _timer = new Timer();
            Size formSize = MessageSize(message);

            switch (style)
            {
                case AnimateStyle.SlideDown:
                    _msgBox.Size = new Size(formSize.Width, 0);
                    _timer.Interval = 1;
                    _timer.Tag = new AnimateMsgBox(formSize, style);
                    break;

                case AnimateStyle.FadeIn:
                    _msgBox.Size = formSize;
                    _msgBox.Opacity = 0;
                    _timer.Interval = 20;
                    _timer.Tag = new AnimateMsgBox(formSize, style);
                    break;

                case AnimateStyle.ZoomIn:
                    _msgBox.Size = new Size(formSize.Width + 100, formSize.Height + 100);
                    _timer.Tag = new AnimateMsgBox(formSize, style);
                    _timer.Interval = 1;
                    break;
            }

            _timer.Tick += timer_Tick;
            _timer.Start();

            _msgBox.ShowDialog();
            MessageBeep(0);
            return _buttonResult;
        }

        static void timer_Tick(object sender, EventArgs e)
        {
            Timer timer = (Timer)sender;
            AnimateMsgBox animate = (AnimateMsgBox)timer.Tag;

            switch(animate.Style){
                case AnimateStyle.SlideDown:
                    if (_msgBox.Height < animate.FormSize.Height)
                    {
                        _msgBox.Height += 17;
                        _msgBox.Invalidate();
                    }
                    else
                    {
                        _timer.Stop();
                        _timer.Dispose();
                    }
                    break;

                case AnimateStyle.FadeIn:
                    if (_msgBox.Opacity < 1)
                    {
                        _msgBox.Opacity += 0.1;
                        _msgBox.Invalidate();
                    }
                    else
                    {
                        _timer.Stop();
                        _timer.Dispose();
                    }
                    break;

                case AnimateStyle.ZoomIn:
                    if (_msgBox.Width > animate.FormSize.Width )
                    {
                        _msgBox.Width -= 17;
                        _msgBox.Invalidate();
                    }
                    if (_msgBox.Height > animate.FormSize.Height)
                    {
                        _msgBox.Height -= 17;
                        _msgBox.Invalidate();
                    }
                    break;
            }
        }

        private static void InitButtons(Buttons buttons)
        {
            switch (buttons)
            {
                case Buttons.AbortRetryIgnore:
                    _msgBox.InitAbortRetryIgnoreButtons();
                    break;

                case Buttons.OK:
                    _msgBox.InitOKButton();
                    break;

                case Buttons.OKCancel:
                    _msgBox.InitOKCancelButtons();
                    break;

                case Buttons.RetryCancel:
                    _msgBox.InitRetryCancelButtons();
                    break;

                case Buttons.YesNo:
                    _msgBox.InitYesNoButtons();
                    break;

                case Buttons.YesNoCancel:
                    _msgBox.InitYesNoCancelButtons();
                    break;
            }

            foreach (Button btn in _msgBox._buttonCollection)
            {
                btn.ForeColor = Color.FromArgb(170, 170, 170);
                btn.Font = new Font("Segoe UI", 8);
                btn.Padding = new Padding(3);
                btn.FlatStyle = FlatStyle.Flat;
                btn.Height = 30;
                btn.FlatAppearance.BorderColor = Color.FromArgb(99, 99, 98);

                _msgBox._flpButtons.Controls.Add(btn);
            }
        }

        private static void InitIcon(Icon icon)
        {
            switch (icon)
            {
                case Icon.Application:
                    _msgBox._picIcon.Image = SystemIcons.Application.ToBitmap();
                    break;

                case Icon.Exclamation:
                    _msgBox._picIcon.Image = SystemIcons.Exclamation.ToBitmap();
                    break;

                case Icon.Error:
                    _msgBox._picIcon.Image = SystemIcons.Error.ToBitmap();
                    break;

                case Icon.Info:
                    _msgBox._picIcon.Image = SystemIcons.Information.ToBitmap();
                    break;

                case Icon.Question:
                    _msgBox._picIcon.Image = SystemIcons.Question.ToBitmap();
                    break;

                case Icon.Shield:
                    _msgBox._picIcon.Image = SystemIcons.Shield.ToBitmap();
                    break;

                case Icon.Warning:
                    _msgBox._picIcon.Image = SystemIcons.Warning.ToBitmap();
                    break;
            }
        }

        private void InitAbortRetryIgnoreButtons()
        {
            Button btnAbort = new Button();
            btnAbort.Text = "Abort";
            btnAbort.Click += ButtonClick;

            Button btnRetry = new Button();
            btnRetry.Text = "Retry";
            btnRetry.Click += ButtonClick;

            Button btnIgnore = new Button();
            btnIgnore.Text = "Ignore";
            btnIgnore.Click += ButtonClick;

            _buttonCollection.Add(btnAbort);
            _buttonCollection.Add(btnRetry);
            _buttonCollection.Add(btnIgnore);
        }

        private void InitOKButton()
        {
            Button btnOK = new Button();
            btnOK.Text = "OK";
            btnOK.Click += ButtonClick;

            _buttonCollection.Add(btnOK);
        }

        private void InitOKCancelButtons()
        {
            Button btnOK = new Button();
            btnOK.Text = "OK";
            btnOK.Click += ButtonClick;

            Button btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.Click += ButtonClick;


            _buttonCollection.Add(btnOK);
            _buttonCollection.Add(btnCancel);
        }

        private void InitRetryCancelButtons()
        {
            Button btnRetry = new Button();
            btnRetry.Text = "OK";
            btnRetry.Click += ButtonClick;

            Button btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.Click += ButtonClick;


            _buttonCollection.Add(btnRetry);
            _buttonCollection.Add(btnCancel);
        }

        private void InitYesNoButtons()
        {
            Button btnYes = new Button();
            btnYes.Text = "Yes";
            btnYes.Click += ButtonClick;

            Button btnNo = new Button();
            btnNo.Text = "No";
            btnNo.Click += ButtonClick;


            _buttonCollection.Add(btnYes);
            _buttonCollection.Add(btnNo);
        }

        private void InitYesNoCancelButtons()
        {
            Button btnYes = new Button();
            btnYes.Text = "Abort";
            btnYes.Click += ButtonClick;

            Button btnNo = new Button();
            btnNo.Text = "Retry";
            btnNo.Click += ButtonClick;

            Button btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.Click += ButtonClick;

            _buttonCollection.Add(btnYes);
            _buttonCollection.Add(btnNo);
            _buttonCollection.Add(btnCancel);
        }

        private static void ButtonClick(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            switch (btn.Text)
            {
                case "Abort":
                    _buttonResult = DialogResult.Abort;
                    break;

                case "Retry":
                    _buttonResult = DialogResult.Retry;
                    break;

                case "Ignore":
                    _buttonResult = DialogResult.Ignore;
                    break;

                case "OK":
                    _buttonResult = DialogResult.OK;
                    break;

                case "Cancel":
                    _buttonResult = DialogResult.Cancel;
                    break;

                case "Yes":
                    _buttonResult = DialogResult.Yes;
                    break;

                case "No":
                    _buttonResult = DialogResult.No;
                    break;
            }

            _msgBox.Dispose();
        }

        private static Size MessageSize(string message)
        {
            Graphics g = _msgBox.CreateGraphics();
            int width=350;
            int height = 230;

            SizeF size = g.MeasureString(message, new Font("Segoe UI", 10));

            if (message.Length < 150)
            {
                if ((int)size.Width > 350)
                {
                    width = (int)size.Width;
                }
            }
            else
            {
                string[] groups = (from Match m in Regex.Matches(message, ".{1,180}") select m.Value).ToArray();
                int lines = groups.Length+1;
                width = 700;
                height += (int)(size.Height+10) * lines;
            }
            return new Size(width, height);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            Rectangle rect = new Rectangle(new Point(0, 0), new Size(Width - 1, Height - 1));
            Pen pen = new Pen(Color.FromArgb(0, 151, 251));

            g.DrawRectangle(pen, rect);
        }

        public enum Buttons
        {
            AbortRetryIgnore=1,
            OK=2,
            OKCancel=3,
            RetryCancel=4,
            YesNo=5,
            YesNoCancel=6
        }

        public enum Icon 
        {
            Application = 1,
            Exclamation = 2,
            Error = 3,
            Warning = 4,
            Info = 5,
            Question = 6,
            Shield = 7,
            Search = 8
        }

        public enum AnimateStyle
        {
            SlideDown=1,
            FadeIn= 2, 
            ZoomIn =3
        }

    }

    class AnimateMsgBox
    {
        public Size FormSize;
        public MsgBox.AnimateStyle Style;

        public AnimateMsgBox(Size formSize, MsgBox.AnimateStyle style)
        {
            FormSize = formSize;
            Style = style;
        }
    }
}
