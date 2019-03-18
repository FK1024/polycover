using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StaticCodeAnalysis
{
    class Coverage
    {
        private YoYoGraph graph;
        private JToken fileCov;


        public Coverage(string jsonPath, string codePath, YoYoGraph graph)
        {
            this.graph = graph;
            JToken coverage = JObject.Parse(File.ReadAllText(jsonPath));
            JToken projectCov = coverage.Values().First();
            this.fileCov = projectCov[codePath];
        }


        // calculates coverage results by comparing information from the json file with information from the graph, modifies the graph
        public double Calculate()
        {
            JToken lines = GetLines();
            int linksCovered = 0;

            // mark the covered links from an invocation node to a method node
            List<Link> linksFromInvoc2Method = graph.GetLinksFromInvoc2Method();
            foreach (Link link in linksFromInvoc2Method)
            {
                if (lines[(link.TargetInsertedIfBodyLineNumber + 1).ToString()].ToObject<int>() > 0)
                {
                    link.IsCovered = true;
                    linksCovered++;
                }
                else
                {
                    link.IsCovered = false;
                }
            }

            // mark the completely covered invocation nodes and their incoming links
            List<Node> invocNodes = graph.GetInvocationNodes();
            foreach (Node invocNode in invocNodes)
            {
                List<Link> invocOutgoingLinks = graph.GetOutgoingLinks(invocNode.Id);
                Link invocIncomingLink = graph.GetIncomingLinks(invocNode.Id).First();
                // if all outgoing links of an invocation node are covered then the invocation node and it's incoming link are covered too
                if (invocOutgoingLinks.All(l => l.IsCovered))
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

            // mark the completely covered method nodes
            List<Node> methodNodes = graph.GetMethodNodes();
            foreach (Node methodNode in methodNodes)
            {
                List<Link> methodOutgoingLinks = graph.GetOutgoingLinks(methodNode.Id);
                // if all outgoing links of a method node are covered then the method node is covered too
                if (methodOutgoingLinks.All(l => l.IsCovered))
                {
                    methodNode.IsCovered = true;
                }
                else
                {
                    methodNode.IsCovered = false;
                }
            }

            double result = (double)linksCovered / linksFromInvoc2Method.Count;
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
