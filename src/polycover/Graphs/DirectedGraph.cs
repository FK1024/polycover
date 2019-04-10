using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        public bool IsCovered;
        
    }

    public abstract class Link
    {
        [XmlAttribute]
        public string Source;
        [XmlAttribute]
        public string Target;
    }

    public class Style
    {
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

        public Style(string targetType, string groupLabel, string valueLabel, List<Condition> conditions, List<Setter> setters)
        {
            this.TargetType = targetType;
            this.GroupLabel = groupLabel;
            this.ValueLabel = valueLabel;
            this.Conditions = conditions;
            this.Setters = setters;
        }
    }

    public class Condition
    {
        [XmlAttribute]
        public string Expression;

        private Condition() { }

        public Condition(string expression)
        {
            this.Expression = expression;
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
