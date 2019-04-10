using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using polycover.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace polycover
{
    class SyntaxRewriter : CSharpSyntaxRewriter
    {
        private bool isVariantEdge;
        private DirectedGraph graph;
        private StaticCodeAnalysis codeAnalysis;
        private int insertedLines = 0;


        public SyntaxRewriter(StaticCodeAnalysis codeAnalysis, DirectedGraph graph)
        {
            this.isVariantEdge = graph.GetType() == typeof(YoYoGraph);
            this.codeAnalysis = codeAnalysis;
            this.graph = graph;
        }



        // inserts if-statement(s) for each coverage target in each method to be able to detect execution, modifies the graph
        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node) // hopefully this visits the nodes in the correct order
        {
            dynamic coverageTargets;
            int coverageTargetsCount;

            if (isVariantEdge)
            {
                coverageTargets = graph.GetIncomingLinks(codeAnalysis.GetFullMethodName(node));
                coverageTargetsCount = (coverageTargets as List<Link>).Count();
            }
            else
            {
                List<Link> linksToClasses = graph.GetOutgoingLinks(codeAnalysis.GetFullMethodName(node)); // all nodes contained in the method group have a link with the method as source
                coverageTargets = linksToClasses.Select(l => graph.GetNode(l.Target)).ToList();
                coverageTargetsCount = (coverageTargets as List<Node>).Count();
            }

            if (coverageTargetsCount == 0)
            {
                return node;
            }
            else
            {
                int methodBodyStartLine = codeAnalysis.GetMethodBodyStartLine(node) + insertedLines;
                int linesToInsert = 1 + 2 * coverageTargetsCount;
                insertedLines += linesToInsert;

                List<StatementSyntax> ifStatements = CreateIfStatements(node, methodBodyStartLine, coverageTargets);

                // insert the generated if-statements
                var newStatements = node.Body.Statements.InsertRange(0, ifStatements);
                var newBody = node.Body.WithStatements(newStatements);
                var newMethod = node.WithBody(newBody);
                return newMethod;
            }
        }

        // creates an if-statement for each incoming invocation to from which line this method was invoced, modifies the graph
        public List<StatementSyntax> CreateIfStatements(MethodDeclarationSyntax node, int methodBodyStartLine, List<Link> incomingInvocations)
        {
            // first create statement for the assignment of a variable for the caller's line number
            List<StatementSyntax> ifStatements = new List<StatementSyntax>();
            string lineNumberAssignmentStr = "int ln = new System.Diagnostics.StackFrame(1, true).GetFileLineNumber() - 1;" + Environment.NewLine; // Note: "- 1" because GetFileLineNumber is 1 based whereas GetLocation().GetLineSpan().StartLinePosition.Line is zero based!
            ifStatements.Add(SyntaxFactory.ParseStatement(lineNumberAssignmentStr));

            // then create an if-statement for each invocation
            int linkNo = 0;
            foreach (YoYoLink link in incomingInvocations)
            {
                string ifStatementStr = "if (";
                foreach (int line in (graph.GetNode(link.Source) as YoYoNode).Invocation.Lines)
                {
                    ifStatementStr += "ln == " + line + " || ";
                }
                // remove last " || "
                ifStatementStr = ifStatementStr.Substring(0, ifStatementStr.Length - 4);
                ifStatementStr += ")" + Environment.NewLine + "{}" + Environment.NewLine;

                // add the parsed statement
                ifStatements.Add(SyntaxFactory.ParseStatement(ifStatementStr));

                // save the line number where the body of the created statement will be inserted
                link.TargetInsertedIfBodyLineNumber = methodBodyStartLine + 2 + 2 * linkNo;
                linkNo++;
            }

            return ifStatements;
        }

        // creates if-statements to detect on which types each method was called, modifies the graph
        public List<StatementSyntax> CreateIfStatements(MethodDeclarationSyntax node, int methodBodyStartLine, List<Node> classNodes)
        {
            // first create statement for the assignment of a variable for the runtime type
            List<StatementSyntax> ifStatements = new List<StatementSyntax>();
            string typeAssignmentStr = "string type = this.GetType().FullName;" + Environment.NewLine;
            ifStatements.Add(SyntaxFactory.ParseStatement(typeAssignmentStr));

            // then create an if-statement for each class
            int classNo = 0;
            foreach (IHNode classNode in classNodes)
            {
                string ifStatementStr = "if (type == \"" + classNode.Label + "\")" + Environment.NewLine + "{}" + Environment.NewLine;

                // add the parsed statement
                ifStatements.Add(SyntaxFactory.ParseStatement(ifStatementStr));

                // save the line number where the body of the created statement will be inserted
                classNode.TargetInsertedIfBodyLineNumber = methodBodyStartLine + 2 + 2 * classNo;
                classNo++;
            }

            return ifStatements;
        }
    }
}
