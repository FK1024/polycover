using Newtonsoft.Json.Linq;
using polycover.Graphs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Type = polycover.Graphs.Type;

namespace polycover
{
    class Coverage
    {
        private bool isVariantEdge;
        private DirectedGraph graph;
        private JToken fileCov;


        public Coverage(string jsonPath, string codePath, DirectedGraph graph)
        {
            this.isVariantEdge = graph.GetType() == typeof(YoYoGraph);
            this.graph = graph;
            JToken coverage = JObject.Parse(File.ReadAllText(jsonPath));
            foreach (JToken projectCov in coverage.Values())
            {
                if (projectCov[codePath] != null)
                {
                    this.fileCov = projectCov[codePath];
                    break;
                }
            }
        }


        // calculates coverage results by comparing information from the json file with information from the graph, modifies the graph
        public double Calculate()
        {
            JToken lines = GetLines();
            int targetsCovered = 0;

            // get targets
            dynamic targets;
            if (isVariantEdge)
            {
                targets = graph.GetLinksFromTypeToType(Type.INVOCATION, Type.METHOD);
            }
            else
            {
                targets = graph.GetNodesOfType(Type.TYPE).Where(n => (n as IHNode).IsCoverable).ToList();
            }

            // mark covered targets:
            //  * targets of a YoYo graph are links from an invocation node to a method node
            //  * targets of an inheritance graph are classes inside a method group
            foreach (dynamic target in targets)
            {
                if (lines[(target.TargetInsertedIfBodyLineNumber + 1).ToString()].ToObject<int>() > 0) // Note: "+ 1" because coverlet is 1 based
                {
                    target.IsCovered = true;
                    targetsCovered++;
                }
                else
                {
                    target.IsCovered = false;
                }
            }

            // mark completely covered invocation nodes and their incoming links if graph is a YoYo graph
            if (isVariantEdge)
            {
                List<Node> invocNodes = graph.GetNodesOfType(Type.INVOCATION);
                foreach (Node invocNode in invocNodes)
                {
                    List<Link> invocOutgoingLinks = graph.GetOutgoingLinks(invocNode.Id);
                    YoYoLink invocIncomingLink = graph.GetIncomingLinks(invocNode.Id).First() as YoYoLink;
                    // if all outgoing links of an invocation node are covered then the invocation node and it's incoming link are covered too
                    if (invocOutgoingLinks.All(l => (l as YoYoLink).IsCovered))
                    {
                        invocNode.IsCovered = true;
                        invocIncomingLink.IsCovered = true;
                    }
                    else
                    {
                        invocNode.IsCovered = false;
                        invocIncomingLink.IsCovered = false;
                    }
                }
            }

            // mark completely covered method nodes:
            foreach (Node methodNode in graph.GetNodesOfType(Type.METHOD))
            {
                List<Link> methodOutgoingLinks = graph.GetOutgoingLinks(methodNode.Id);
                // if all outgoing links of a method node are covered then the method node is covered too
                bool isCovered;
                if (isVariantEdge)
                {
                    isCovered = methodOutgoingLinks.All(l => (l as YoYoLink).IsCovered);
                }
                else
                {
                    List<Node> methodsTypes = methodOutgoingLinks.Select(l => graph.GetNode(l.Target)).ToList();
                    isCovered = methodsTypes.All(t => t.IsCovered);
                }

                if (isCovered)
                {
                    methodNode.IsCovered = true;
                }
                else
                {
                    methodNode.IsCovered = false;
                }
            }

            // mark completely covered class nodes (inheritance graph only):
            foreach (Node classNode in graph.GetNodesOfType(Type.CLASS))
            {
                List<Link> classOutgoingLinks = graph.GetOutgoingLinks(classNode.Id);
                List<Node> classesMethods = classOutgoingLinks.Select(l => graph.GetNode(l.Target)).ToList();
                // if all outgoing links of a class node are covered then the class node is covered too
                if (classesMethods.All(m => m.IsCovered))
                {
                    classNode.IsCovered = true;
                }
                else
                {
                    classNode.IsCovered = false;
                }
            }

            // mark completely covered namespace nodes (inheritance graph only):
            foreach (Node namespaceNode in graph.GetNodesOfType(Type.NAMESPACE))
            {
                List<Link> namespaceOutgoingLinks = graph.GetOutgoingLinks(namespaceNode.Id);
                List<Node> namespacesClasses = namespaceOutgoingLinks.Select(l => graph.GetNode(l.Target)).ToList();
                // if all outgoing links of a namespace node are covered then the namespace node is covered too
                if (namespacesClasses.All(m => m.IsCovered))
                {
                    namespaceNode.IsCovered = true;
                }
                else
                {
                    namespaceNode.IsCovered = false;
                }
            }

            double result = (double)targetsCovered / targets.Count;
            return result;
        }

        // appends all "lines" tokens in current file
        private JToken GetLines()
        {
            var lines = new JObject();

            foreach (JToken classCov in this.fileCov.Values())
            {
                foreach (JToken methodCov in classCov.Values())
                {
                    lines.Merge(methodCov["Lines"] as JObject);
                }
            }

            return lines;
        }
    }
}
