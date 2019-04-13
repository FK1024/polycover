using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace polycover.Graphs
{
    [XmlRoot("DirectedGraph")]
    [XmlInclude(typeof(IHNode))]
    [XmlInclude(typeof(IHLink))]
    public class InheritanceGraph : DirectedGraph
    {
        [XmlAttribute]
        public string GraphDirection = "BottomToTop";


        public InheritanceGraph()
        {
            this.Styles = new List<Style>
            {
                new Style(
                    Style.TARGETTYPE_NODE,
                    "not coverable",
                    new List<Condition> { new Condition("IsCoverable", "false") },
                    new List<Setter> { new Setter("Background", "LightGray") }),
                new Style(
                    Style.TARGETTYPE_NODE,
                    "covered",
                    new List<Condition> { new Condition("IsCoverable", "true"), new Condition("IsCovered", "true") },
                    new List<Setter> { new Setter("Stroke", "Green") }),
                new Style(
                    Style.TARGETTYPE_NODE,
                    "not covered",
                    new List<Condition> { new Condition("IsCoverable", "true"), new Condition("IsCovered", "false") },
                    new List<Setter> { new Setter("Stroke", "Red") }),
                new Style(Style.TARGETTYPE_NODE, 3),
                new Style(Type.NAMESPACE),
                new Style(Type.CLASS),
                new Style(Type.METHOD),
                new Style(Type.TYPE),
                new Style(
                    Style.TARGETTYPE_LINK,
                    "derives from",
                    new List<Condition>(),
                    new List<Setter>
                    {
                        new Setter("Stroke", "Gray"),
                        new Setter("StrokeThickness", "3"),
                        new Setter("DrawArrow", "true")
                    })
            };
        }
    }

    public class IHNode : Node
    {
        [XmlAttribute]
        public bool IsCoverable;
        [XmlAttribute]
        public string Group;
        [XmlIgnore]
        public MethodDeclarationSyntax Method;
        [XmlIgnore]
        public ClassDeclarationSyntax ClassDecl;
        [XmlIgnore]
        public int TargetInsertedIfBodyLineNumber;


        private IHNode() { }

        public IHNode(string id, string label, string type)
        {
            this.Id = id;
            this.Label = label;
            this.Type = type;
            this.IsCoverable = true;
            this.Group = "Expanded";
        }

        public IHNode(string id, string label, MethodDeclarationSyntax method)
        {
            this.Id = id;
            this.Label = label;
            this.Type = Graphs.Type.METHOD;
            this.IsCoverable = true;
            this.Group = "Expanded";
            this.Method = method;
        }

        public IHNode(string id, string label, ClassDeclarationSyntax classDecl, bool isCoverable)
        {
            this.Id = id;
            this.Label = label;
            this.Type = Graphs.Type.TYPE;
            this.IsCoverable = isCoverable;
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
