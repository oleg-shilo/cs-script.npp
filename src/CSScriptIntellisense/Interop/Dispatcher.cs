using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace CSScriptIntellisense.Interop
{
    public partial class Dispatcher : Form
    {
        static public void Init()
        {
            instance = new Dispatcher();
        }

        static public void Schedule(int interval, Action action)
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
            timer1.Enabled = false;
            if (action != null)
            {
                action();
                action = null;
            }
        }
    }

    public partial class NppUI
    {
        static Dictionary<int, List<Action>> handlers = new Dictionary<int, List<Action>>();

        static public void Marshal(Action action)
        {
            Marshal(0, action);
        }

        static public void Marshal(int interval, Action action)
        {
            if (handlers.Any()) return;

            int key = Environment.TickCount + interval;

            if (!handlers.ContainsKey(key))
                handlers.Add(key, new List<Action>());

            handlers[key].Add(action);
        }

        static public void OnNppTick()
        {
            var signaledHandlers = handlers.Where(x => x.Key < Environment.TickCount).ToArray();
            foreach (KeyValuePair<int, List<Action>> item in signaledHandlers)
            {
                foreach (Action action in item.Value)
                    try
                    {
                        action();
                    }
                    catch { }
                handlers.Remove(item.Key);
            }
        }
    }
}