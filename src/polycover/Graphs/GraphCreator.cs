using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace polycover.Graphs
{
    class GraphCreator
    {
        private StaticCodeAnalysis codeAnalysis;

        
        public GraphCreator(StaticCodeAnalysis codeAnalysis)
        {
            this.codeAnalysis = codeAnalysis;
        }


        // creates and returns the YoYo graph
        public  YoYoGraph CreateYoYoGraph()
        {
            YoYoGraph graph = new YoYoGraph();

            // create a node for each method
            List<MethodDeclarationSyntax> methods = codeAnalysis.GetAllMethods();

            foreach (MethodDeclarationSyntax method in methods)
            {
                graph.AddNode(new YoYoNode(codeAnalysis.GetMethodId(method), codeAnalysis.GetFullMethodName(method), method));
            }

            // create a node for each invocation and add the links
            List<Node> methodNodes = new List<Node>(graph.Nodes);
            foreach (YoYoNode methodNode in methodNodes)
            {
                int invocCount = 0;
                List<StaticCodeAnalysis.Invocation> invocations = codeAnalysis.GetInvocations(methodNode.Method);
                foreach (StaticCodeAnalysis.Invocation invocation in invocations)
                {
                    YoYoNode invocNode = new YoYoNode(methodNode.Id + "_I" + invocCount.ToString(), $"invocs (L {String.Join(", ", invocation.Lines.Select(l => l + 1))})", invocation);
                    graph.AddNode(invocNode);
                    invocCount++;
                    graph.AddLink(new YoYoLink(methodNode.Id, invocNode.Id));

                    foreach (MethodDeclarationSyntax invocOption in invocation.Methods)
                    {
                        graph.AddLink(new YoYoLink(invocNode.Id, graph.GetNode(codeAnalysis.GetMethodId(invocOption)).Id));
                    }
                }
            }

            return graph;
        }

        // creates and returns the inheritance graph
        public  InheritanceGraph CreateInheritanceGraph()
        {
            InheritanceGraph graph = new InheritanceGraph();
            string nodeId;

            foreach (NamespaceDeclarationSyntax ns in codeAnalysis.GetAllNamespaces())
            {
                // create a group for each namespace
                nodeId = codeAnalysis.GetNamespaceId(ns);
                IHNode nsNode = new IHNode(nodeId, nodeId, Type.NAMESPACE);
                graph.AddNode(nsNode);

                foreach (ClassDeclarationSyntax cl in codeAnalysis.GetAllClasses(ns))
                {
                    // create a group for each class and link it to the namespace group
                    nodeId = codeAnalysis.GetClassId(cl);
                    IHNode clNode = new IHNode(nodeId, cl.Identifier.Text, Type.CLASS);
                    graph.AddNode(clNode);
                    graph.AddLink(new IHLink(nsNode.Id, clNode.Id, true));

                    foreach (MethodDeclarationSyntax method in codeAnalysis.GetAllMethods(cl))
                    {
                        // create a group for each method and link it to the class group
                        nodeId = codeAnalysis.GetMethodId(method);
                        IHNode methodNode = new IHNode(nodeId, method.Identifier.Text, method);
                        graph.AddNode(methodNode);
                        graph.AddLink(new IHLink(clNode.Id, methodNode.Id, true));

                        // add the inheritance tree to the method group
                        StaticCodeAnalysis.InheritanceTree tree = codeAnalysis.GetInheritanceTree(method);
                        graph = Tree2Graph(graph, tree.rootClass, methodNode, null);
                    }
                }
            }

            return graph;
        }

        // helper functions:
        // =================

        // helper to convert the recoursive inheritance tree to a flattened graph
        private InheritanceGraph Tree2Graph(InheritanceGraph graph, InheritanceNode tree, IHNode groupNode, IHNode baseTypeNode)
        {
            ClassDeclarationSyntax classDecl = tree.GetBaseClass();
            string nodeName = codeAnalysis.GetClassId(classDecl);
            // add the type node
            IHNode typeNode = new IHNode(groupNode.Id + ":" + nodeName, nodeName, classDecl, !codeAnalysis.IsClassAbstract(classDecl));
            graph.AddNode(typeNode);
            // link the type node to the method group
            graph.AddLink(new IHLink(groupNode.Id, typeNode.Id, true));
            // link the type node to it's base type if it has one
            if (baseTypeNode != null) graph.AddLink(new IHLink(typeNode.Id, baseTypeNode.Id, false));

            foreach (var subClass in tree.GetSubClasses())
            {
                graph = Tree2Graph(graph, subClass, groupNode, typeNode);
            }

            return graph;
        }

        // corrects all saved line numbers of invocations in a given YoYo graph with respect to the code which will be insterted
        public void PreCorrectLineNumbers(YoYoGraph graph)
        {
            // first create a dictionary ["method body start line", "number of lines to insert"]
            var LinesToInsertAtLine = new Dictionary<int, int>();

            foreach (YoYoNode methodNode in graph.GetNodesOfType(Type.METHOD))
            {
                int methodBodyStartLine = codeAnalysis.GetMethodBodyStartLine(methodNode.Method);
                int NoIncomingInvocs = graph.GetIncomingLinks(methodNode.Id).Count;
                int NoLinesToInsert = NoIncomingInvocs > 0 ? 1 + 2 * NoIncomingInvocs : 0;
                LinesToInsertAtLine.Add(methodBodyStartLine, NoLinesToInsert);
            }

            // correct all saved line numbers by adding the number of lines getting inserted before
            foreach (YoYoNode invocNode in graph.GetNodesOfType(Type.INVOCATION))
            {
                // check if invocation has line numbers which need to be corrected
                if (invocNode.Invocation.Lines.Any(l => l >= LinesToInsertAtLine.Where(elem => elem.Value > 0).Select(elem => elem.Key).Min()))
                {
                    List<int> correctedLines = new List<int>();
                    foreach (int invocLine in invocNode.Invocation.Lines)
                    {
                        int linesToInsert = (from methodLine in LinesToInsertAtLine
                                             where methodLine.Key <= invocLine
                                             select methodLine.Value).Sum();
                        correctedLines.Add(invocLine + linesToInsert);
                    }
                    invocNode.Invocation = new StaticCodeAnalysis.Invocation(correctedLines, invocNode.Invocation.Methods);
                }
            }
        }
    }
}
