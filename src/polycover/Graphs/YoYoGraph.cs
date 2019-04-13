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


        public YoYoGraph()
        {
            this.Styles = new List<Style>
            {
                new Style(
                    Style.TARGETTYPE_NODE,
                    "covered",
                    new List<Condition> { new Condition("IsCovered", "true") },
                    new List<Setter> { new Setter("Stroke", "Green") }),
                new Style(
                    Style.TARGETTYPE_NODE,
                    "not covered",
                    new List<Condition> { new Condition("IsCovered", "false") },
                    new List<Setter> { new Setter("Stroke", "Red") }),
                new Style(Style.TARGETTYPE_NODE, 3),
                new Style(Type.METHOD),
                new Style(Type.INVOCATION),
                new Style(
                    Style.TARGETTYPE_LINK,
                    "covered",
                    new List<Condition> { new Condition("IsCovered", "true") },
                    new List<Setter> { new Setter("Stroke", "Green") }),
                new Style(Style.TARGETTYPE_LINK, 3),
                new Style(
                    Style.TARGETTYPE_LINK,
                    "not covered",
                    new List<Condition> { new Condition("IsCovered", "false") },
                    new List<Setter> { new Setter("Stroke", "Red") })
            };
        }
    }

    public class YoYoNode : Node
    {
        [XmlIgnore]
        public MethodDeclarationSyntax Method;
        [XmlIgnore]
        public StaticCodeAnalysis.Invocation Invocation;


        private YoYoNode() { }

        public YoYoNode(string id, string label, MethodDeclarationSyntax method)
        {
            this.Id = id;
            this.Label = label;
            this.Type = Graphs.Type.METHOD;
            this.Method = method;
        }

        public YoYoNode(string id, string label, StaticCodeAnalysis.Invocation invocation)
        {
            this.Id = id;
            this.Label = label;
            this.Type = Graphs.Type.INVOCATION;
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
}
