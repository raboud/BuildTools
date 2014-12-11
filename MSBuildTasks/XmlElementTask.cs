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
    public abstract class XmlElementTask : Task
    {
        private string _path = null;
        private string _xPath = null;
        private string _name = null;
        private string _value = null;
        private xmlOperation _xmlOperation = xmlOperation.Unspecified;

        private string _attributeName1 = null;
        private string _attributeValue1 = null;

        private string _attributeName2 = null;
        private string _attributeValue2 = null;

        private string _attributeName3 = null;
        private string _attributeValue3 = null;

        private string _attributeName4 = null;
        private string _attributeValue4 = null;

        private string _attributeName5 = null;
        private string _attributeValue5 = null;

        internal enum xmlOperation
        {
            Unspecified,
            InsertBefore,
            InsertAfter,
            PrependChild,
            AppendChild
        }

        internal xmlOperation XmlOperation
        {
            get { return this._xmlOperation; }
            set { this._xmlOperation = value; }
        }

        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }

        public string XPath
        {
            get { return this._xPath; }
            set { this._xPath = value; }
        }

        public string Name
        {
            get { return this._name; }
            set { this._name = value; }
        }

        public string Value
        {
            get { return this._value; }
            set { this._value = value; }
        }

        public string AttributeName1
        {
            get { return this._attributeName1; }
            set { this._attributeName1 = value; }
        }

        public string AttributeName2
        {
            get { return this._attributeName2; }
            set { this._attributeName2 = value; }
        }

        public string AttributeName3
        {
            get { return this._attributeName3; }
            set { this._attributeName3 = value; }
        }

        public string AttributeName4
        {
            get { return this._attributeName4; }
            set { this._attributeName4 = value; }
        }

        public string AttributeName5
        {
            get { return this._attributeName5; }
            set { this._attributeName5 = value; }
        }

        public string AttributeValue1
        {
            get { return this._attributeValue1; }
            set { this._attributeValue1 = value; }
        }

        public string AttributeValue2
        {
            get { return this._attributeValue2; }
            set { this._attributeValue2 = value; }
        }

        public string AttributeValue3
        {
            get { return this._attributeValue3; }
            set { this._attributeValue3 = value; }
        }

        public string AttributeValue4
        {
            get { return this._attributeValue4; }
            set { this._attributeValue4 = value; }
        }

        public string AttributeValue5
        {
            get { return this._attributeValue5; }
            set { this._attributeValue5 = value; }
        }

        public override bool Execute()
        {
            try
            {
                if (this.XmlOperation == xmlOperation.Unspecified)
                {
                    Log.LogError("XmlElement 'XmlOperation' was unspecified.");
                    return false;
                }

                if (String.IsNullOrEmpty(this.Name))
                {
                    Log.LogError("XmlElement task failed because Name was not specified.");
                    return false;
                }

                XmlDocument doc = new XmlDocument();
                doc.Load(this.Path);

                XmlElement element = doc.SelectSingleNode(this.XPath) as XmlElement;

                if (element != null)
                {
                    // Create a new element
                    XmlElement newElement = doc.CreateElement(this.Name);
                    if (!String.IsNullOrEmpty(this.Value))
                    {
                        newElement.InnerText = this.Value;
                    }

                    string attrName = this.AttributeName1;
                    string attrValue = this.AttributeValue1;
                    if (!String.IsNullOrEmpty(attrName))
                    {
                        XmlAttribute attr = doc.CreateAttribute(attrName);
                        if (!String.IsNullOrEmpty(attrValue))
                        {
                            attr.Value = attrValue;
                        }
                        newElement.Attributes.Append(attr);
                    }

                    attrName = this.AttributeName2;
                    attrValue = this.AttributeValue2;
                    if (!String.IsNullOrEmpty(attrName))
                    {
                        XmlAttribute attr = doc.CreateAttribute(attrName);
                        if (!String.IsNullOrEmpty(attrValue))
                        {
                            attr.Value = attrValue;
                        }
                        newElement.Attributes.Append(attr);
                    }

                    attrName = this.AttributeName3;
                    attrValue = this.AttributeValue3;
                    if (!String.IsNullOrEmpty(attrName))
                    {
                        XmlAttribute attr = doc.CreateAttribute(attrName);
                        if (!String.IsNullOrEmpty(attrValue))
                        {
                            attr.Value = attrValue;
                        }
                        newElement.Attributes.Append(attr);
                    }

                    attrName = this.AttributeName4;
                    attrValue = this.AttributeValue4;
                    if (!String.IsNullOrEmpty(attrName))
                    {
                        XmlAttribute attr = doc.CreateAttribute(attrName);
                        if (!String.IsNullOrEmpty(attrValue))
                        {
                            attr.Value = attrValue;
                        }
                        newElement.Attributes.Append(attr);
                    }

                    attrName = this.AttributeName5;
                    attrValue = this.AttributeValue5;
                    if (!String.IsNullOrEmpty(attrName))
                    {
                        XmlAttribute attr = doc.CreateAttribute(attrName);
                        if (!String.IsNullOrEmpty(attrValue))
                        {
                            attr.Value = attrValue;
                        }
                        newElement.Attributes.Append(attr);
                    }

                    switch (this.XmlOperation)
                    {
                        case xmlOperation.InsertBefore:
                            element.ParentNode.InsertBefore(newElement, element);
                            break;
                        case xmlOperation.InsertAfter:
                            element.ParentNode.InsertAfter(newElement, element);
                            break;
                        case xmlOperation.PrependChild:
                            element.PrependChild(newElement);
                            break;
                        case xmlOperation.AppendChild:
                            element.AppendChild(newElement);
                            break;
                        default:
                            Log.LogError("XmlElement 'XmlOperation' was invalid.");
                            return false;
                    }
                }
                else
                {
                    Log.LogError("XPath not found");
                    return false;
                }

                File.SetAttributes(this.Path, FileAttributes.Normal);
                doc.Save(this.Path);

                return true;
            }
            catch (Exception ex)
            {
                Log.LogError(ex.Message);
            }
            return false;
        }
    }
}
