using Newtonsoft.Json.Linq;
using polycover.Graphs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

            // get targets and their parents
            dynamic targets;
            List<Node> parents;
            if (isVariantEdge)
            {
                YoYoGraph yoyoGraph = graph as YoYoGraph;
                targets = yoyoGraph.GetLinksFromInvoc2Method();
                parents = yoyoGraph.GetMethodNodes();
            }
            else
            {
                InheritanceGraph inheritanceGraph = graph as InheritanceGraph;
                targets = inheritanceGraph.GetClassNodes();
                parents = inheritanceGraph.GetMethodNodes();
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
                List<Node> invocNodes = (graph as YoYoGraph).GetInvocationNodes();
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

            // mark completely covered parents of targets which are all method nodes:
            foreach (Node parent in parents)
            {
                List<Link> methodOutgoingLinks = graph.GetOutgoingLinks(parent.Id);
                // if all outgoing links of a method node are covered then the method node is covered too
                bool isCovered;
                if (isVariantEdge)
                {
                    isCovered = methodOutgoingLinks.All(l => (l as YoYoLink).IsCovered);
                }
                else
                {
                    List<Node> methodsClasses = methodOutgoingLinks.Select(l => graph.GetNode(l.Target)).ToList();
                    isCovered = methodsClasses.All(l => l.IsCovered);
                }

                if (isCovered)
                {
                    parent.IsCovered = true;
                }
                else
                {
                    parent.IsCovered = false;
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
