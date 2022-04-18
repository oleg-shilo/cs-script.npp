using System;
using System.Linq;
using System.Windows.Forms;
using Kbg.NppPluginNET.PluginInfrastructure;

namespace CSScriptNpp.Dialogs
{
    public partial class ShortcutBuilder : Form
    {
        public ShortcutBuilder()
        {
            InitializeComponent();
            keysComboBox.Items.AddRange(Enum.GetValues(typeof(Keys)).Cast<object>().ToArray());
            keysComboBox.SelectedItem = Keys.None;
        }

        public new string Name
        {
            get { return nameLabel.Text; }
            set { nameLabel.Text = value; }
        }

        public string Shortcut
        {
            get
            {
                var s = new ShortcutKey(ctrlCheckBox.Checked, altCheckBox.Checked, shiftCheckBox.Checked, (Keys)keysComboBox.SelectedItem);
                return s.ToString();
            }
            set
            {
                var s = new ShortcutKey(value);
                ctrlCheckBox.Checked = s.IsCtrl;
                shiftCheckBox.Checked = s.IsShift;
                altCheckBox.Checked = s.IsAlt;
                keysComboBox.SelectedItem = s.Key;
            }
        }

        void ok_Click(object sender, EventArgs e)
        {
            Close();
            DialogResult = DialogResult.OK;
        }
    }
}