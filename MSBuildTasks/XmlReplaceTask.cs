using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Xml;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace RandREng.MsBuildTasks
{
    public class XmlReplaceTask : Task
    {
        private string attribute = null;
        private bool createAttribute = false;
        private string path = null;
        private string replace = null;
        private string xPath = null;
        private string xmlns = null;

        public string Attribute
        {
            get { return this.attribute; }
            set { this.attribute = value; }
        }

        public bool CreateAttribute
        {
            get { return this.createAttribute; }
            set { this.createAttribute = value; }
        }

        [Required]
        public string Path
        {
            get { return path; }
            set { path = value; }
        }

        [Required]
        public string Replace
        {
            get { return replace; }
            set { replace = value; }
        }

        [Required]
        public string XPath
        {
            get { return this.xPath; }
            set { this.xPath = value; }
        }

        public string XmlNs
        {
            get { return this.xmlns; }
            set { this.xmlns = value; }
        }

        public override bool Execute()
        {
            try
            {
                bool changed = false;
                XmlDocument doc = new XmlDocument();
                doc.Load( this.Path );

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
//                nsmgr.AddNamespace("default", doc.DocumentElement.NamespaceURI);
                if (!string.IsNullOrEmpty(this.XmlNs))
                {
                    string[] ns = this.XmlNs.Split(new char[] { ',' });
                    if ((ns.Length == 0) || ((ns.Length % 2) == 1))
                    {
                        Log.LogError("Invalid XmlNs specified");
                        return false;
                    }
                    for (int index = 0; index < ns.Length - 1; index += 2)
                    {
                        nsmgr.AddNamespace(ns[index], ns[index + 1]);
                    }
                }

                XmlNodeList nodeList = doc.SelectNodes(this.XPath, nsmgr);
				if (nodeList.Count != 0)
				{
					foreach (XmlNode node in nodeList)
					{
						XmlElement element = node as XmlElement;

						if (element != null)
						{
							if (string.IsNullOrEmpty(attribute))
							{
								element.InnerText = this.Replace;
								changed = true;
							}
							else
							{
								if (element.HasAttribute(this.Attribute) || this.CreateAttribute)
								{
									element.SetAttribute(this.Attribute, this.Replace);
									changed = true;
								}
								else
								{
									Log.LogError("Attribute does not exist");
								}
							}
						}
					}
				}
				else
				{
					Log.LogError("XPath not found");
				}
                if ( changed )
                {
                    File.SetAttributes( this.Path, FileAttributes.Normal );
                    doc.Save( this.Path );
                }
                return true;
            }
            catch ( Exception ex )
            {
                Log.LogError( ex.Message );
            }
            return false;
        }
    }
}
