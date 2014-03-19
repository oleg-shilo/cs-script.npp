using CSScriptNpp.Dialogs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace CSScriptNpp
{
    public partial class DebugPanel : Form
    {
        AutoWatchPanel locals;
        CallStackPanel callstack;

        public DebugPanel()
        {
            InitializeComponent();
            this.stack.Columns[1].Width = 100;
            treeView1.BeforeExpand += treeView1_BeforeExpand;
            tabControl1.SelectedIndex = 3;
            //listView1.OwnerDraw = false;
            //return;

            locals = new AutoWatchPanel();
            callstack = new CallStackPanel();

            tabControl1.AddTab("Auto Watch", locals);
            tabControl1.AddTab("Call Stack", callstack);
        }

        void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            var valueId = e.Node.Tag as string;
            e.Node.Nodes.Clear();
            string data = Debugger.Invoke("locals", valueId);
            if (data != null)
            {
                var nestedNodes = ToNodes(data);
                e.Node.Nodes.AddRange(nestedNodes);
            }
        }

        public void Clear()
        {
            UpdateCallstack("");
            UpdateLocals("");
        }

        public void UpdateCallstack(string data)
        {
            callstack.UpdateCallstack(data);
        }

        TreeNode[] ToNodes(string data)
        {
            if (string.IsNullOrEmpty(data))
                return new TreeNode[0];

            var root = XElement.Parse(data);

            return root.Elements().Select(dbgValue =>
            {
                TreeNode node;

                string id = dbgValue.Attribute("id").Value;
                bool isArray = dbgValue.Attribute("isArray").Value == "true";
                bool isComplex = dbgValue.Attribute("isComplex").Value == "true";
                string type = dbgValue.Attribute("typeName").Value;
                string valName = dbgValue.Attribute("name").Value;

                if (isArray)
                {
                    // It would be nice to display array length here too.
                    // Add a "dummy" sub-node to signify that this node is expandable. We then trap
                    // the BeforeExpand event to add the real children.
                    node = new TreeNode(valName + " (type='" + type + "'):",
                               new TreeNode[1] { new TreeNode("dummy") });
                }
                else if (isComplex)
                {
                    // This will include both instance and static fields
                    // It will also include all base class fields.
                    node = new TreeNode(valName + " (type='" + type + "'):",
                               new TreeNode[1] { new TreeNode("dummy") });
                }
                else
                {
                    // This is a catch-all for primitives.
                    string stValue = dbgValue.Attribute("value").Value;
                    node = new TreeNode(valName + " (type='" + type + "') value=" + stValue);
                }

                node.Tag = id;

                return node;
            }).ToArray();
        }

        public void UpdateLocals(string data)
        {
            Invoke((Action)delegate
            {
                treeView1.Nodes.Clear();
                treeView1.Nodes.AddRange(ToNodes(data));
                locals.SetData(data);
            });
        }

        string FormatXML(string data)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(data);

            using (var stringWriter = new StringWriter())
            using (var xmlWriter = new XmlTextWriter(stringWriter) { Formatting = Formatting.Indented, Indentation = 4 })
            {
                xmlDocument.WriteTo(xmlWriter);
                return stringWriter.ToString();
            }
        }



        private void button1_Click(object sender, EventArgs e)
        {
            label1.Text = Debugger.Invoke(textBox1.Text, null);
        }

        void DebuggerInvokeTest()
        {
            Debugger.BeginInvoke(textBox1.Text, null, result =>
            {
                Invoke((Action)delegate
                {
                    label1.Text = result;
                });
            });
        }
    }
}