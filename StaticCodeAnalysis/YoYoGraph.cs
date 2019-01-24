using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace StaticCodeAnalysis
{
    public class YoYoGraph
    {
        public struct Graph
        {
            public Node[] Nodes;
            public Link[] Links;
        }

        public struct Node
        {
            [XmlAttribute]
            public string Id;
            [XmlAttribute]
            public string Label;
            [XmlIgnore]
            public MethodDeclarationSyntax MethDecl;

            public Node(MethodDeclarationSyntax methDecl, string id, string label)
            {
                this.MethDecl = methDecl;
                this.Id = id;
                this.Label = label;
            }
        }

        public struct Link
        {
            [XmlAttribute]
            public string Source;
            [XmlAttribute]
            public string Target;
            [XmlAttribute]
            public string Label;

            public Link(string source, string target, string label)
            {
                this.Source = source;
                this.Target = target;
                this.Label = label;
            }
        }

        public List<Node> Nodes { get; protected set; }
        public List<Link> Links { get; protected set; }

        public YoYoGraph()
        {
            Nodes = new List<Node>();
            Links = new List<Link>();
        }

        public void AddNode(Node n)
        {
            this.Nodes.Add(n);
        }

        public void AddLink(Link l)
        {
            this.Links.Add(l);
        }

        public void Serialize(string xmlpath)
        {
            Graph g = new Graph();
            g.Nodes = this.Nodes.ToArray();
            g.Links = this.Links.ToArray();

            XmlRootAttribute root = new XmlRootAttribute("DirectedGraph");
            root.Namespace = "http://schemas.microsoft.com/vs/2009/dgml";
            XmlSerializer serializer = new XmlSerializer(typeof(Graph), root);
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            XmlWriter xmlWriter = XmlWriter.Create(xmlpath, settings);
            serializer.Serialize(xmlWriter, g);
        }
    }
}
