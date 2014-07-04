using System;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace CSScriptNpp.Dialogs
{
    public partial class TextVisualizer : Form
    {
        public void InitAsCollection(string dbgId)
        {
            try
            {
                string data = Debugger.Invoke("locals", dbgId);

                var root = XElement.Parse(data);

                var xItems = root.Elements();

                //name="[n]"
                string[] items = xItems.Where(x => x.Attribute("name").Value.StartsWith("["))
                                       .Select(x =>
                                               {
                                                   var rawDisplay = x.Attribute("rawDisplayValue");
                                                   if (rawDisplay != null && rawDisplay.Value != "")
                                                       return rawDisplay.Value;
                                                   else
                                                       return x.Attribute("name").Value + ": " + x.Attribute("value").Value;
                                               })
                                       .ToArray();

                valueCtrl.Text += "\r\n-----------------\r\n" + string.Join("\r\n", items);
            }
            catch { }
        }

        public TextVisualizer(string expression, string value)
        {
            InitializeComponent();
            expressionCtrl.Text = "Expression: " + expression;
            valueCtrl.Text = value;
            wordWrap.Checked = Config.Instance.WordWrapInVisualizer;
        }

        private void wordWrap_CheckedChanged(object sender, EventArgs e)
        {
            valueCtrl.WordWrap = wordWrap.Checked;
            Config.Instance.WordWrapInVisualizer = wordWrap.Checked;
            Config.Instance.Save();
        }
    }
}