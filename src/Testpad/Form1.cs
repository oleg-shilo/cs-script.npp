//using CSScriptIntellisense;
using CSScriptIntellisense;
using System;
using System.Linq;
using System.Windows.Forms;

namespace Testpad
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            SimpleCodeCompletion.ResetProject();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            listBox1.Items.Clear();

            textBox1.SelectionLength = 0;
            int caretPos = textBox1.SelectionStart;
            string code = textBox1.Text;
            string documentName = "script.cs";

            var data = SimpleCodeCompletion.GetCompletionData(code, caretPos, documentName);

            listBox1.Items.AddRange(data.Select(x => x.DisplayText).ToArray());

            Cursor = Cursors.Default;
        }
    }
}