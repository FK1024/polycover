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
            public Category[] Categories;
        }

        public struct Node
        {
            [XmlIgnore]
            public MethodDeclarationSyntax MethDecl;
            [XmlAttribute]
            public string Id;
            [XmlAttribute]
            public string Label;
            [XmlAttribute]
            public string Category;
            
            public Node(MethodDeclarationSyntax methDecl, string id, string label, string category)
            {
                this.MethDecl = methDecl;
                this.Id = id;
                this.Label = label;
                this.Category = category;
            }
        }

        public struct Link
        {
            [XmlAttribute]
            public string Source;
            [XmlAttribute]
            public string Target;

            public Link(string source, string target)
            {
                this.Source = source;
                this.Target = target;
            }
        }

        public struct Category
        {
            [XmlAttribute]
            public string Id;
            [XmlAttribute]
            public string Background;

            public Category(string id, string background)
            {
                this.Id = id;
                this.Background = background;
            }
        }

        public List<Node> Nodes { get; protected set; }
        public List<Link> Links { get; protected set; }
        public List<Category> Categories { get; protected set; }

        public YoYoGraph()
        {
            Nodes = new List<Node>();
            Links = new List<Link>();
            Categories = new List<Category>();
        }

        public void AddNode(Node n)
        {
            this.Nodes.Add(n);
        }

        public void AddLink(Link l)
        {
            this.Links.Add(l);
        }

        public void AddCategory(Category c)
        {
            this.Categories.Add(c);
        }

        public void Serialize(string xmlpath)
        {
            Graph g = new Graph();
            g.Nodes = this.Nodes.ToArray();
            g.Links = this.Links.ToArray();
            g.Categories = this.Categories.ToArray();

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
