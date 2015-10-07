using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using _3PA.Lib;

namespace _3PA.Interop
{
    public partial class Dispatcher : Form
    {
        static public void Init()
        {
            _instance = new Dispatcher();
        }

        static public void Shedule(int interval, Action action)
        {
            _instance.SheduleImpl(interval, action);
        }

        static Dispatcher _instance;

        Action _action;

        public Dispatcher()
        {
            InitializeComponent();
            Top = -400;
        }

        void SheduleImpl(int interval, Action action)
        {
            timer1.Enabled = false;
            timer1.Interval = interval;
            _action = action;
            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            if (_action != null)
            {
                _action();
                _action = null;
            }
        }

        static public void Do(Action action)
        {
            _instance.SafeInvoke(d => action());
        }
    }

}