//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
#region Using directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

#endregion

using System.IO;

namespace gui
{
    partial class LaunchProcess : Form
    {
        static string lastProcessName = "\"" + Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\C# Scripts\debugging2.exe""";
        static string lastProcessArgs = "";
        //static string lastProcessName = "\"" + Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Visual Studio 2013\Projects\DbAgentTest\bin\Debug\DbAgentTest.exe""";
        //static string lastProcessArgs = @"/dbg """ + Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\C# Scripts\debugging2.cs""";

        public LaunchProcess()
        {
            InitializeComponent();

            this.textBoxProcessName.Text = lastProcessName;

            this.textBoxArgs.Text = lastProcessArgs;
            this.textBoxWorkingDir.Text = System.IO.Directory.GetCurrentDirectory();
        }

        #region Properties
        // Properties for caller to get stuff.

        // Process working directory.
        string m_WorkingDir;
        public string WorkingDir
        {
            get { return m_WorkingDir; }
        }

        // Arguments to pass to process.
        string m_Arguments;
        public string Arguments
        {
            get { return m_Arguments; }
        }

        // Full path to process name
        // This will be null if the cancelled.
        string m_ProcessName;
        public string ProcessName
        {
            get { return m_ProcessName; }
        }
        #endregion

        private void buttonLaunch_Click(object sender, EventArgs e)
        {
            // Need to cache results because once we close the form,we'll lose all
            // the text boxes.
            m_WorkingDir = this.textBoxWorkingDir.Text;
            m_Arguments = this.textBoxArgs.Text;
            m_ProcessName = this.textBoxProcessName.Text;
            lastProcessName = this.textBoxProcessName.Text;
            lastProcessArgs = this.textBoxArgs.Text;
            if (!m_ProcessName.StartsWith("\""))
                m_ProcessName = "\"" + m_ProcessName + "\"";


            this.Close();
        }

        private void buttonOpenProcess_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog f = new OpenFileDialog())
            {
                f.DefaultExt = "exe";
                f.CheckFileExists = true;
                f.CheckPathExists = true;
                f.ValidateNames = true;
                f.InitialDirectory = this.textBoxWorkingDir.Text;
                f.Multiselect = false;
                f.Title = "Select executable to start debugging.";

                DialogResult x = f.ShowDialog();
                if (x != DialogResult.OK)
                {
                    return;
                }

                this.textBoxProcessName.Text = f.FileName;
                this.textBoxWorkingDir.Text = System.IO.Directory.GetCurrentDirectory();
            }
        }
    }
}