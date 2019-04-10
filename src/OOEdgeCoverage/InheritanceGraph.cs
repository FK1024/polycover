using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace OOEdgeCoverage
{
    [XmlRoot("DirectedGraph")]
    [XmlInclude(typeof(IHNode))]
    [XmlInclude(typeof(IHLink))]
    public class InheritanceGraph : DirectedGraph
    {
        [XmlAttribute]
        public string GraphDirection = "TopToBottom";

        public InheritanceGraph()
        {
            this.Styles = new List<Style>
            {
                new Style("Node", "IsCovered", "true", new Condition("IsCovered='true'"), new Setter("Stroke", "Green")),
                new Style("Node", "IsCovered", "false", new Condition("IsCovered='false'"), new Setter("Stroke", "Red"))
            };
        }


        // Node methods

        public List<Node> GetMethodNodes()
        {
            return this.Nodes.Where(n => (n as IHNode).Method != null).ToList();
        }

        public List<Node> GetClassNodes()
        {
            return this.Nodes.Where(n => (n as IHNode).ClassDecl != null).ToList();
        }
    }

    public class IHNode : Node
    {
        [XmlAttribute]
        public string Group;
        [XmlIgnore]
        public MethodDeclarationSyntax Method;
        [XmlIgnore]
        public ClassDeclarationSyntax ClassDecl;
        [XmlIgnore]
        public int TargetInsertedIfBodyLineNumber;

        private IHNode() { }

        public IHNode(string id, string label, MethodDeclarationSyntax method)
        {
            this.Id = id;
            this.Label = label;
            this.Group = "Expanded";
            this.Method = method;
        }

        public IHNode(string id, string label, ClassDeclarationSyntax classDecl)
        {
            this.Id = id;
            this.Label = label;
            this.ClassDecl = classDecl;
        }
    }

    public class IHLink : Link
    {
        [XmlAttribute]
        public string Category;

        private IHLink() { }

        public IHLink(string source, string target, bool isContained)
        {
            this.Source = source;
            this.Target = target;
            if (isContained) this.Category = "Contains";
        }
    }
}
