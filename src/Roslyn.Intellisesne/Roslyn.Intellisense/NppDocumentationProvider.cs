using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.CSharp;

namespace RoslynIntellisense
{
    public class NppDocumentationProvider : DocumentationProvider
    {
        string module;

        public NppDocumentationProvider(string module)
        {
            this.module = module;
        }

        static public NppDocumentationProvider NewFor(string module)
        {
            return new NppDocumentationProvider(module);
        }

        public override bool Equals(object obj)
        {
            return module.Equals(obj);
        }

        public override int GetHashCode()
        {
            return module.GetHashCode();
        }

        internal XDocument GetXmlDocumentationFile(string dllPath)
        {
            if (string.IsNullOrEmpty(dllPath))
                return null;

            if (Path.GetDirectoryName(dllPath).EndsWith("CSScriptNpp", StringComparison.OrdinalIgnoreCase))
                return null;

            var xmlFileName = Path.GetFileNameWithoutExtension(dllPath) + ".xml";
            var localPath = Path.Combine(Path.GetDirectoryName(dllPath), xmlFileName);

            if (File.Exists(localPath))
                return XDocument.Load(localPath);

            //if it's a .NET framework assembly it's in one of following folders

            var netPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\" + xmlFileName);
            if (File.Exists(netPath))
            {
                if (!gacDocs.ContainsKey(netPath))
                {
                    return gacDocs[netPath] = XDocument.Load(netPath);
                }
                return gacDocs[netPath];
            }

            return null;
        }

        public string GetDocumentationFor(string documentationMemberID)
        {
            return this.GetDocumentationForSymbol(documentationMemberID, null);
        }

        static Dictionary<string, XDocument> gacDocs = new Dictionary<string, XDocument>();

        protected override string GetDocumentationForSymbol(string documentationMemberID, CultureInfo preferredCulture, CancellationToken cancellationToken = default(CancellationToken))
        {
            string xml = "";
            try
            {
                XDocument xmlFile = GetXmlDocumentationFile(module);
                if (xmlFile != null)
                {
                    xml = xmlFile.Root.Element("members")
                                      .Elements("member")
                                      .Where(x => x.Attribute("name").Value == documentationMemberID)
                                      .FirstOrDefault()?
                                      .ToString();
                }
            }
            catch { } //doc failures are not critical

            return xml;
        }
    }

}