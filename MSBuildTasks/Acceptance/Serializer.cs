using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace RandREng.MsBuildTasks.Acceptance
{
    public abstract class Serializer
    {
        public static T Deserialize<T>(string xml) where T : class
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            StringReader reader = new StringReader(xml);

            return serializer.Deserialize(reader) as T;
        }

        public static string Serialize(object data)
        {
            XmlSerializer serializer = new XmlSerializer(data.GetType());

            StringWriter buffer = new StringWriter();

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            XmlWriter writer = XmlWriter.Create(buffer, settings);
            serializer.Serialize(writer, data, ns);
            writer.Flush();

            return buffer.ToString();
        }
    }
}
