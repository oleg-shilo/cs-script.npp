using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Updater
{
    public partial class WaitPrompt : Form
    {
        static WaitPrompt prompt;

        public new static void Show()
        {
            if (prompt == null) //not very accurate but practical enough
                ThreadPool.QueueUserWorkItem(x =>
                {
                    try
                    {
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                    }
                    catch { }
                    prompt = new WaitPrompt();
                    prompt.VisibleChanged += (sender, e) =>
                                                 prompt.Activate();
                    prompt.ShowDialog();
                });
        }

        public static void OnProgress(long step, long total)
        {
            try
            {
                int percentage = (int)((double)step / (double)total * 100.0);
                prompt.UpdateStep(percentage);
            }
            catch { }
        }

        public new static void Hide()
        {
            try
            {
                if (prompt != null)
                    prompt.Invoke((Action)prompt.Close);
            }
            catch { }
        }

        public WaitPrompt()
        {
            InitializeComponent();
        }

        void UpdateStep(int percentage)
        {
            this.Invoke((Action)delegate
            {
                progressBar1.Value = percentage;
                Debug.WriteLine("{0}%", percentage);
            });
        }

        private void WaitPrompt_Load(object sender, EventArgs e)
        {
        }
    }
}