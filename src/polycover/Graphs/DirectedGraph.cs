using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace polycover.Graphs
{
    public abstract class DirectedGraph
    {
        [XmlAttribute]
        public string Layout = "Sugiyama"; // (= tree layout), neccessary for selcting graph direction
        [XmlAttribute]
        public string Background = "White";
        [XmlArray]
        public List<Node> Nodes = new List<Node>();
        [XmlArray]
        public List<Link> Links = new List<Link>();
        [XmlArray]
        public List<Style> Styles;


        // Node methods

        public Node GetNode(string id)
        {
            return this.Nodes.Where(n => n.Id == id).FirstOrDefault();
        }

        public List<Node> GetNodesOfType(string type)
        {
            return this.Nodes.Where(n => n.Type == type).ToList();
        }

        public void AddNode(Node n)
        {
            if (GetNode(n.Id) == null)
            {
                this.Nodes.Add(n);
            }
            else
            {
                throw new ArgumentException("The graph already has a node with identifier " + n.Id);
            }
        }

        // Link methods

        public void AddLink(Link l)
        {
            this.Links.Add(l);
        }

        public List<Link> GetIncomingLinks(string nodeId)
        {
            return this.Links.Where(l => l.Target == nodeId).ToList();
        }

        public List<Link> GetOutgoingLinks(string nodeId)
        {
            return this.Links.Where(l => l.Source == nodeId).ToList();
        }

        public List<Link> GetLinksFromTypeToType(string sourceType, string targetType)
        {
            return this.Links.Where(l => GetNode(l.Source).Type == sourceType && GetNode(l.Target).Type == targetType).ToList();
        }

        public void Serialize(string dgmlPath)
        {
            XmlSerializer serializer = new XmlSerializer(this.GetType(), "http://schemas.microsoft.com/vs/2009/dgml");
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            XmlWriter xmlWriter = XmlWriter.Create(dgmlPath, settings);
            serializer.Serialize(xmlWriter, this);
        }
    }

    public abstract class Node
    {
        [XmlAttribute]
        public string Id;
        [XmlAttribute]
        public string Label;
        [XmlAttribute]
        public string Type;
        [XmlAttribute]
        public bool IsCovered;
    }

    public abstract class Link
    {
        [XmlAttribute]
        public string Source;
        [XmlAttribute]
        public string Target;
    }

    // enum of node types
    public static class Type
    {
        public const string NAMESPACE = "Namespace";
        public const string CLASS = "Class";
        public const string METHOD = "Method";
        public const string INVOCATION = "Invocation";
        public const string TYPE = "Type";
    }

    public class Style
    {
        [XmlIgnore]
        public const string TARGETTYPE_NODE = "Node";
        [XmlIgnore]
        public const string TARGETTYPE_LINK = "Link";
        [XmlAttribute]
        public string TargetType;
        [XmlAttribute]
        public string GroupLabel;
        [XmlAttribute]
        public string ValueLabel;
        [XmlElement("Condition")]
        public List<Condition> Conditions;
        [XmlElement("Setter")]
        public List<Setter> Setters;


        private Style() { }

        public Style(string targetType, int strokeThickness)
        {
            this.TargetType = targetType;
            this.Setters = new List<Setter> { new Setter("StrokeThickness", strokeThickness.ToString()) };
        }

        public Style(string type)
        {
            string icon = "";
            string background = "";

            switch (type)
            {
                case Type.NAMESPACE:
                    icon = "CodeSchema_Namespace";
                    background = "Darkblue";
                    break;
                case Type.CLASS:
                    icon = "CodeSchema_Class";
                    background = "Blue";
                    break;
                case Type.METHOD:
                    icon = "CodeSchema_Method";
                    background = "LightBlue";
                    break;
                case Type.INVOCATION:
                    icon = "CodeSchema_Event";
                    background = "Beige";
                    break;
                case Type.TYPE:
                    icon = "CodeSchema_Class";
                    background = "Beige";
                    break;
            }

            this.TargetType = TARGETTYPE_NODE;
            this.GroupLabel = type;
            this.ValueLabel = type;
            this.Conditions = new List<Condition> { new Condition("Type", type) };
            this.Setters = new List<Setter> { new Setter("Icon", icon), new Setter("Background", background) };
        }

        public Style(string targetType, string legendLabel, List<Condition> conditions, List<Setter> setters)
        {
            this.TargetType = targetType;
            this.GroupLabel = legendLabel;
            this.ValueLabel = legendLabel;
            this.Conditions = conditions;
            this.Setters = setters;
        }
    }

    public class Condition
    {
        [XmlAttribute]
        public string Expression;


        private Condition() { }

        public Condition(string attribute, string value)
        {
            this.Expression = $"{attribute}='{value}'";
        }
    }

    public class Setter
    {
        [XmlAttribute]
        public string Property;
        [XmlAttribute]
        public string Value;


        private Setter() { }

        public Setter(string property, string value)
        {
            this.Property = property;
            this.Value = value;
        }
    }
}
