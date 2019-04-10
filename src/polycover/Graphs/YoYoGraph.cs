using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace polycover.Graphs
{
    [XmlRoot("DirectedGraph")]
    [XmlInclude(typeof(YoYoNode))]
    [XmlInclude(typeof(YoYoLink))]
    public class YoYoGraph : DirectedGraph
    {
        [XmlAttribute]
        public string GraphDirection = "LeftToRight";
        [XmlArray]
        public readonly List<Category> Categories;

        public YoYoGraph()
        {
            this.Categories = new List<Category>
            {
                new Category("method", "Lightgray"),
                new Category("invocation", "Lightblue")
            };
            this.Styles = new List<Style>
            {
                new Style("Node", "IsCovered", "true", new List<Condition> { new Condition("IsCovered='true'") }, new List<Setter> { new Setter("Stroke", "Green") }),
                new Style("Node", "IsCovered", "false", new List<Condition> { new Condition("IsCovered='false'") }, new List<Setter> { new Setter("Stroke", "Red") }),
                new Style("Link", "IsCovered", "true", new List<Condition> { new Condition("IsCovered='true'") }, new List<Setter> { new Setter("Stroke", "Green") }),
                new Style("Link", "IsCovered", "false", new List<Condition> { new Condition("IsCovered='false'") }, new List<Setter> { new Setter("Stroke", "Red") })
            };
        }


        // Node methods

        public List<Node> GetMethodNodes()
        {
            return this.Nodes.Where(n => (n as YoYoNode).Method != null).ToList();
        }

        public List<Node> GetInvocationNodes()
        {
            return this.Nodes.Where(n => (n as YoYoNode).Method == null).ToList();
        }

        // Link methods

        public List<Link> GetLinksFromInvoc2Method()
        {
            return this.Links.Where(l => GetMethodNodes().Select(mn => mn.Id).Contains(l.Target)).ToList();
        }
    }

    public class YoYoNode : Node
    {
        [XmlAttribute]
        public string Category;
        [XmlIgnore]
        public MethodDeclarationSyntax Method;
        [XmlIgnore]
        public StaticCodeAnalysis.Invocation Invocation;

        private YoYoNode() { }

        public YoYoNode(string id, string label, MethodDeclarationSyntax method)
        {
            this.Id = id;
            this.Label = label;
            this.Category = "method";
            this.Method = method;
        }

        public YoYoNode(string id, string label, StaticCodeAnalysis.Invocation invocation)
        {
            this.Id = id;
            this.Label = label;
            this.Category = "invocation";
            this.Invocation = invocation;
        }
    }

    public class YoYoLink : Link
    {
        [XmlIgnore]
        public int TargetInsertedIfBodyLineNumber;
        [XmlAttribute]
        public bool IsCovered;

        private YoYoLink() { }

        public YoYoLink(string source, string target)
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
