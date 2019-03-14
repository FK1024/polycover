using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        [XmlArray]
        public List<Node> Nodes;
        [XmlArray]
        public List<Link> Links;
        [XmlArray]
        public readonly List<Category> Categories;

        public YoYoGraph()
        {
            this.Nodes = new List<Node>();
            this.Links = new List<Link>();
            this.Categories = new List<Category>
            {
                new Category("method", ColorTranslator.ToHtml(Color.FromArgb(Color.LightBlue.ToArgb()))),
                new Category("invocation", ColorTranslator.ToHtml(Color.FromArgb(Color.LightCoral.ToArgb())))
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

        public List<Link> GetIncomingLinks() // incoming links are links from an invocation node to a method node
        {
            return this.Links.Where(l => GetMethodNodes().Select(mn => mn.Id).Contains(l.Target)).ToList();
        }

        public List<Link> GetIncomingLinks(string methodNodeId)
        {
            return this.Links.Where(l => l.Target == methodNodeId).ToList();
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
}
