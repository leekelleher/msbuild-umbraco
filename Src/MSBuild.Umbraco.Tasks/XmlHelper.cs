using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace MSBuild.Umbraco.Tasks
{
    public class XmlHelper
    {
        internal static void UpdateNode(ref XmlDocument doc, string xpath, string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            var node = doc.SelectSingleNode(xpath);
            if (node != null)
            {
                node.InnerText = value;
            }
        }

        internal static void UpdateCDataNode(ref XmlDocument doc, string xpath, string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            var node = doc.SelectSingleNode(xpath);
            if (node != null)
            {
                var cDataSection = doc.CreateCDataSection(value);
                node.InnerXml = cDataSection.OuterXml;
            }
        }

        internal static void UpdateAttribute(ref XmlDocument doc, string xpath, string attribute, string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            var node = doc.SelectSingleNode(xpath);
            if (node != null)
            {
                var attr = node.Attributes[attribute];
                if (attr != null)
                {
                    attr.Value = value;
                }
            }
        }

        internal static XmlNode CreateNode(ref XmlDocument doc, string name, string value)
        {
            var node = doc.CreateNode(XmlNodeType.Element, name, "");
            node.AppendChild(doc.CreateTextNode(value));
            return node;
        }
    }
}
