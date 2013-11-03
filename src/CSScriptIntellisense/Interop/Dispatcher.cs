using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CSScriptIntellisense.Interop
{
    public partial class Dispatcher : Form
    {
        static public void Init()
        {
            instance = new Dispatcher();
        }
        static public void Shedule(int interval, Action action)
        {
            instance.SheduleImpl(interval, action);
        }

        static Dispatcher instance;

        Action action;

        public Dispatcher()
        {
            InitializeComponent();
            Top = -400;
        }

        void SheduleImpl(int interval, Action action)
        {
            timer1.Enabled = false;
            timer1.Interval = interval;
            this.action = action;
            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (action != null)
            {
                action();
                action = null;
            }

            timer1.Enabled = false;
        }
    }
}
