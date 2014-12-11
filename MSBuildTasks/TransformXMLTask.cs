using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Build.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Text.RegularExpressions;

namespace RandREng.MsBuildTasks
{
    public class TransformXMLTask : Task
    {
        ITaskItem[] _inputFiles = null;

        [Required]
        public ITaskItem[] Include
        {
            get { return _inputFiles; }
            set { _inputFiles = value; }
        }

        string _outputDir;
        
        [Required]
        public string OutputDir
        {
            get { return _outputDir; }
            set { _outputDir = value; }
        }

        
        string _outputExt=string.Empty;
        [Required]
        public string OutputExtension
        {
            get { return _outputExt; }
            set { _outputExt = value; }
                
        }

        string _xslTransform = string.Empty;
        [Required]
        public string XSLTransform
        {
            get { return _xslTransform; }
            set { _xslTransform = value; }
        }

        public override bool Execute()
        {
            XsltArgumentList argList = new XsltArgumentList();
            argList.AddExtensionObject("urn:Helper", this);
            
            foreach (ITaskItem item in _inputFiles)
            {
                if (item.ItemSpec.Length > 0)
                {
                    XslCompiledTransform trans = new XslCompiledTransform();
                    trans.Load(this.XSLTransform);
                    XmlReader rdr = XmlReader.Create(item.ItemSpec);
                    argList.AddParam("results", "", Path.GetFileName(item.ItemSpec));
                    string outFile=Path.Combine(OutputDir, Path.GetFileNameWithoutExtension(item.ItemSpec)+"." + OutputExtension);
                    XmlWriter writer = XmlWriter.Create(outFile);
                    trans.Transform(rdr, argList,writer);
                    argList.RemoveParam("results", "");
                }
            }

            return true;
        }

        public string DateTimeToString(string timestring)
        {
            DateTime dt;

            if (!DateTime.TryParse(timestring, out dt))
            {
                Int64 i64 = 0;
                if (Int64.TryParse(timestring, out i64))
                {
                    dt = DateTime.FromBinary(i64);
                }
            }


            return dt.ToLocalTime().ToString();
        }

        public string TimeSpan(string timestring1, string timestring2)
        {
            DateTime dt1;
            if (!DateTime.TryParse(timestring1, out dt1))
            {
                Int64 i1;
                if (Int64.TryParse(timestring1, out i1))
                {
                    dt1 = DateTime.FromBinary(i1);
                }
            }
            DateTime dt2;
            if (!DateTime.TryParse(timestring2, out dt2))
            {
                Int64 i1;
                if (Int64.TryParse(timestring2, out i1))
                {
                    dt2 = DateTime.FromBinary(i1);
                }
            }

            TimeSpan ts = dt2 - dt1;
            return ts.ToString();
        }

        public XPathNodeIterator GetClassInformation(string classInfo)
        {
            Regex regex = new Regex(@"[,=]",RegexOptions.IgnorePatternWhitespace);
            string[] tokens = regex.Split(classInfo);

            string strFormat= "<root><className>{0}</className><assemblyName>{1}</assemblyName></root>";

            string className = tokens[0].Trim();
            string assembly = tokens[1].Trim();

            string strXml = string.Format(strFormat, className, assembly);
            XPathDocument xdoc = new XPathDocument(new StringReader(strXml));
            XPathNodeIterator itr = xdoc.CreateNavigator().Select("/root");
            return itr;
            
        }

    }
}
