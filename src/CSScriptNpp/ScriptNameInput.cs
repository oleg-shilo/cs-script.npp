using System;
using System.Windows.Forms;

namespace CSScriptNpp
{
    public partial class ScriptNameInput : Form
    {
        public ScriptNameInput()
        {
            InitializeComponent();
        }

        private void ScriptNameInput_Load(object sender, EventArgs e)
        {
            nameTextBox.SelectAll();
            nameTextBox.Focus();
        }

        public string ScriptName
        {
            get
            {
                return nameTextBox.Text;
            }
        }
    }
}