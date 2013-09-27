using System;
using System.Windows.Forms;

namespace CSScriptNpp
{
    public partial class ScriptNameInput : Form
    {
        public ScriptNameInput()
        {
            InitializeComponent();
            classlessCheckbox.Checked = Config.Instance.ClasslessScriptByDefault;
        }

        private void ScriptNameInput_Load(object sender, EventArgs e)
        {
            nameTextBox.SelectAll();
            nameTextBox.Focus();
        }

        public bool ClasslessScript
        {
            get
            {
                return classlessCheckbox.Checked;
            }
        }
        public string ScriptName
        {
            get
            {
                return nameTextBox.Text;
            }
        }

        private void okBtn_Click(object sender, EventArgs e)
        {
            Config.Instance.ClasslessScriptByDefault = ClasslessScript;
            Config.Instance.Save();
        }
    }
}