using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StaticCodeAnalysis
{
    public class Program
    {
        static void Main(string[] args)
        {
            string codePath = @"..\..\..\ExampleCode\ExampleCode6.cs";
            StaticCodeAnalysis testAnalysis = new StaticCodeAnalysis(codePath);
            
            YoYoGraph testGraph = new YoYoGraph();
            
            // create a node for each method
            List<MethodDeclarationSyntax> methods = testAnalysis.GetAllMethods();

            foreach (MethodDeclarationSyntax method in methods)
            {
                string nodeName = testAnalysis.GetFullMethodName(method);
                testGraph.AddNode(new Node(nodeName, nodeName, method));
            }

            // create a node for each invocation and add the links
            List<Node> methodNodes = new List<Node>(testGraph.Nodes);
            foreach (Node methodNode in methodNodes)
            {
                int invocCount = 0;
                List<StaticCodeAnalysis.Invocation> invocations = testAnalysis.GetInvocations(methodNode.Method);
                foreach (StaticCodeAnalysis.Invocation invocation in invocations)
                {
                    Node invocNode = new Node(methodNode.Id + "_I" + invocCount.ToString(), "invocs", invocation);
                    testGraph.AddNode(invocNode);
                    invocCount++;
                    testGraph.AddLink(new Link(methodNode.Id, invocNode.Id));

                    foreach (MethodDeclarationSyntax invocOption in invocation.Methods)
                    {
                        testGraph.AddLink(new Link(invocNode.Id, testGraph.GetNode(testAnalysis.GetFullMethodName(invocOption)).Id));
                    }
                }
            }

            string DGMLPath = codePath.Substring(0, codePath.Length - 3) + "YoYoGraph.dgml";
            testGraph.Serialize(DGMLPath);
        }
    }
}
