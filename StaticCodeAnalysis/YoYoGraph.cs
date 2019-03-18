using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace StaticCodeAnalysis
{
    [XmlRoot("DirectedGraph", Namespace = "http://schemas.microsoft.com/vs/2009/dgml")]
    public class YoYoGraph
    {
        [XmlAttribute]
        public string Layout = "Sugiyama"; // (= tree layout), neccessary for selcting graph direction
        [XmlAttribute]
        public string GraphDirection = "LeftToRight";
        [XmlAttribute]
        public string Background = "White";
        [XmlArray]
        public List<Node> Nodes;
        [XmlArray]
        public List<Link> Links;
        [XmlArray]
        public readonly List<Category> Categories;
        [XmlArray]
        public readonly List<Style> Styles;

        public YoYoGraph()
        {
            this.Nodes = new List<Node>();
            this.Links = new List<Link>();
            this.Categories = new List<Category>
            {
                new Category("method", "Lightgray"),
                new Category("invocation", "Lightblue")
            };
            this.Styles = new List<Style>
            {
                new Style("Node", "IsCovered", "true", new Condition("IsCovered='true'"), new Setter("Stroke", "Green")),
                new Style("Node", "IsCovered", "false", new Condition("IsCovered='false'"), new Setter("Stroke", "Red")),
                new Style("Link", "IsCovered", "true", new Condition("IsCovered='true'"), new Setter("Stroke", "Green")),
                new Style("Link", "IsCovered", "false", new Condition("IsCovered='false'"), new Setter("Stroke", "Red"))
            };
        }


        // Node methods

        public Node GetNode(string id)
        {
            return this.Nodes.Where(n => n.Id == id).FirstOrDefault();
        }

        public List<Node> GetMethodNodes()
        {
            return this.Nodes.Where(n => n.Method != null).ToList();
        }

        public List<Node> GetInvocationNodes()
        {
            return this.Nodes.Where(n => n.Method == null).ToList();
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

        public List<Link> GetLinksFromInvoc2Method()
        {
            return this.Links.Where(l => GetMethodNodes().Select(mn => mn.Id).Contains(l.Target)).ToList();
        }

        public List<Link> GetIncomingLinks(string nodeId)
        {
            return this.Links.Where(l => l.Target == nodeId).ToList();
        }

        public List<Link> GetOutgoingLinks(string nodeId)
        {
            return this.Links.Where(l => l.Source == nodeId).ToList();
        }

        public void Serialize(string xmlpath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(YoYoGraph));
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            XmlWriter xmlWriter = XmlWriter.Create(xmlpath, settings);
            serializer.Serialize(xmlWriter, this);
        }
    }

    public class Node
    {
        [XmlAttribute]
        public string Id;
        [XmlAttribute]
        public string Label;
        [XmlAttribute]
        public string Category;
        [XmlAttribute]
        public bool IsCovered;
        [XmlIgnore]
        public MethodDeclarationSyntax Method;
        [XmlIgnore]
        public StaticCodeAnalysis.Invocation Invocation;

        private Node() { }

        public Node(string id, string label, MethodDeclarationSyntax method)
        {
            this.Id = id;
            this.Label = label;
            this.Category = "method";
            this.Method = method;
        }

        public Node(string id, string label, StaticCodeAnalysis.Invocation invocation)
        {
            this.Id = id;
            this.Label = label;
            this.Category = "invocation";
            this.Invocation = invocation;
        }
    }

    public class Link
    {
        [XmlAttribute]
        public string Source;
        [XmlAttribute]
        public string Target;
        [XmlIgnore]
        public int TargetInsertedIfBodyLineNumber;
        [XmlAttribute]
        public bool IsCovered;

        private Link() { }

        public Link(string source, string target)
        {
            this.Source = source;
            this.Target = target;
        }
    }

    public class Category
    {
        [XmlAttribute]
        public string Id;
        [XmlAttribute]
        public string Background;

        private Category() { }

        public Category(string id, string background)
        {
            this.Id = id;
            this.Background = background;
        }
    }

    public class Style
    {
        [XmlAttribute]
        public string TargetType;
        [XmlAttribute]
        public string GroupLabel;
        [XmlAttribute]
        public string ValueLabel;
        [XmlElement]
        public Condition Condition;
        [XmlElement]
        public Setter Setter;

        private Style() { }

        public Style(string targetType, string groupLabel, string valueLabel, Condition condition, Setter setter)
        {
            this.TargetType = targetType;
            this.GroupLabel = groupLabel;
            this.ValueLabel = valueLabel;
            this.Condition = condition;
            this.Setter = setter;
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
