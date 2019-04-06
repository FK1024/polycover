using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OOEdgeCoverage
{
    class SyntaxRewriter : CSharpSyntaxRewriter
    {
        private YoYoGraph graph;
        private StaticCodeAnalysis codeAnalysis;
        private int insertedLines = 0;


        public SyntaxRewriter(StaticCodeAnalysis codeAnalysis, YoYoGraph graph)
        {
            this.codeAnalysis = codeAnalysis;
            this.graph = graph;
        }


        // inserts an if-statement for each incoming invocation for each method to detect from which line each method was invoced, modifies the YoYo graph
        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node) // hopefully this visits the nodes in the correct order
        {
            List<Link> incomingInvocations = graph.GetIncomingLinks(codeAnalysis.GetFullMethodName(node));
            int NoIncomingInvocs = incomingInvocations.Count();
            if (NoIncomingInvocs == 0)
            {
                return node;
            }
            else
            {
                int methodBodyStartLine = codeAnalysis.GetMethodBodyStartLine(node) + insertedLines;
                int linesToInsert = 1 + 2 * NoIncomingInvocs;
                insertedLines += linesToInsert;
                
                // first create statement for the assignment of a variable for the caller's line number
                List<StatementSyntax> ifStatements = new List<StatementSyntax>();
                string lineNumberAssignmentStr = "int ln = new System.Diagnostics.StackFrame(1, true).GetFileLineNumber() - 1;" + Environment.NewLine; // Note: "- 1" because GetFileLineNumber is 1 based whereas GetLocation().GetLineSpan().StartLinePosition.Line is zero based!
                ifStatements.Add(SyntaxFactory.ParseStatement(lineNumberAssignmentStr));

                // then create an if-statement for each invocation
                int linkNo = 0;
                foreach (Link link in incomingInvocations)
                {
                    string ifStatementStr = "if (";
                    foreach (int line in graph.GetNode(link.Source).Invocation.Lines)
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

                // insert the generated if-statements
                var newStatements = node.Body.Statements.InsertRange(0, ifStatements);
                var newBody = node.Body.WithStatements(newStatements);
                var newMethod = node.WithBody(newBody);
                return newMethod;
            }
        }
    }
}
