using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OOEdgeCoverage
{
    public class Program
    {
        private static string codePath;
        private static string backupDirectory;

        private static void DisplayHelp()
        {
            Console.WriteLine(".NET Framework source code edge coverage tool" + Environment.NewLine);

            Console.WriteLine("Arguments:");
            Console.WriteLine("\t" + "<CODEFILE>" + "\t" + "Path to the source code file");
            Console.WriteLine("\t" + "<SOLUTION>" + "\t" + "Path to the solution");
            Console.WriteLine("\t" + "<TESTPROJECT>" + "\t" + "Path to the test project");
            Console.WriteLine("\t" + "<TESTASSEMBLY>" + "\t" + "Path to the test assembly");
        }

        private static void ParseArguments(string[] args)
        {
            if (args.Length != 4)
            {
                throw new ArgumentException("Wrong number of arguments specified");
            }
            for (int i = 0; i < 4; i++)
            {
                if (!File.Exists(args[i]))
                {
                    throw new ArgumentException(new string[] { "Code", "Solution", "Project", "Assembly" }[i] + $" file does not exist: '{args[i]}'");
                }
            }
        }
        

        static int Main(string[] args)
        {
            if (args.Length == 1 && (args[0] == "-h" || args[0] == "--help" || args[0] == "/?"))
            {
                DisplayHelp();
                return 0;
            }
            else
            {
                try
                {
                    ParseArguments(args);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    DisplayHelp();
                    return 1;
                }
            }

            backupDirectory = FindBackupDirectory();

            // input parameters:
            codePath = args[0];
            string solPath = args[1];
            string testProjPath = args[2];
            string testDllPath = args[3];

            string target = "--target dotnet";
            string targetargs = $"--targetargs \"test {testProjPath} --no-build\"";
            string coveragePath = backupDirectory + @"\coverage.json";
            string output = $"--output {coveragePath}";
            string coverletCall = String.Join(" ", new string[] { "coverlet", testDllPath, target, targetargs, output });

            string buildCall = String.Join(" ", new string[] { "dotnet", "build", solPath });


            StaticCodeAnalysis codeAnalysis = new StaticCodeAnalysis(codePath);

            YoYoGraph graph = CreateYoYoGraph(codeAnalysis);
            // if the source code doesn't contain invocations then output the graph, 100% coverage and return
            if (!graph.GetInvocationNodes().Any())
            {
                Console.WriteLine(Environment.NewLine + "The given source code does not contain any invocations" + Environment.NewLine);
                // mark every method node as covered
                foreach (Node methodNode in graph.Nodes)
                {
                    methodNode.IsCovered = true;
                }
                OutputResultAndGraph(1, graph);
                return 0;
            }

            BackupFile(codePath);

            try
            {
                // manipulate the source code file to be able to gather information needed
                PreCorrectLineNumbers(graph);
                SyntaxRewriter rewriter = new SyntaxRewriter(codeAnalysis, graph);
                SyntaxNode newRoot = rewriter.Visit(codeAnalysis.GetRoot());
                File.WriteAllText(codePath, newRoot.GetText().ToString(), Encoding.Default);
                Console.WriteLine(Environment.NewLine + $"Instrumented file '{codePath}'");

                // re-build the solution
                Process buildProcess = new Process();
                buildProcess.StartInfo.FileName = "cmd.exe";
                buildProcess.StartInfo.Arguments = "/C " + buildCall;
                buildProcess.StartInfo.RedirectStandardOutput = true; // don't write stdout to console
                buildProcess.StartInfo.UseShellExecute = false; // don't create new window
                Console.WriteLine(Environment.NewLine + "Re-building solution ...");
                buildProcess.Start();
                buildProcess.WaitForExit();
                if (buildProcess.ExitCode != 0)
                {
                    Console.WriteLine("build failed");
                    return 1;
                }
                Console.WriteLine("build succeed");

                // start coverlet
                Process coverletProcess = new Process();
                coverletProcess.StartInfo.FileName = "cmd.exe";
                coverletProcess.StartInfo.Arguments = "/C " + coverletCall;
                coverletProcess.StartInfo.RedirectStandardOutput = true; // don't write stdout to console
                coverletProcess.StartInfo.UseShellExecute = false; // don't create new window
                Console.WriteLine(Environment.NewLine + "Starting coverlet ...");
                coverletProcess.Start();
                coverletProcess.WaitForExit();
                if (coverletProcess.ExitCode != 0)
                {
                    Console.WriteLine("coverlet calculation failed");
                    return 1;
                }
                Console.WriteLine("coverlet calculation succeed");

                // read the generated json file & calculate the coverage
                Console.WriteLine(Environment.NewLine + "Calculating coverage result ...");
                Coverage coverage = new Coverage(coveragePath, codePath, graph);
                double result = coverage.Calculate();

                OutputResultAndGraph(result, graph);
            }
            finally
            {
                RestoreFile(codePath);
            }
            
            return 0;
        }


        // creates a file for the graph and writes the percentage result to console
        public static void OutputResultAndGraph(double result, YoYoGraph graph)
        {
            string graphPath = codePath.Substring(0, codePath.Length - 3) + "_YoYoGraph.dgml";
            graph.Serialize(graphPath);
            Console.WriteLine($"Generated YoYo-graph '{graphPath}'");

            Console.WriteLine(Environment.NewLine + "OO Edge Coverage Result: {0:P2}", result);
        }


        // creates and returns the YoYo graph
        public static YoYoGraph CreateYoYoGraph(StaticCodeAnalysis codeAnalysis)
        {
            YoYoGraph graph = new YoYoGraph();
            
            // create a node for each method
            List<MethodDeclarationSyntax> methods = codeAnalysis.GetAllMethods();

            foreach (MethodDeclarationSyntax method in methods)
            {
                string nodeName = codeAnalysis.GetFullMethodName(method);
                graph.AddNode(new Node(nodeName, nodeName, method));
            }

            // create a node for each invocation and add the links
            List<Node> methodNodes = new List<Node>(graph.Nodes);
            foreach (Node methodNode in methodNodes)
            {
                int invocCount = 0;
                List<StaticCodeAnalysis.Invocation> invocations = codeAnalysis.GetInvocations(methodNode.Method);
                foreach (StaticCodeAnalysis.Invocation invocation in invocations)
                {
                    Node invocNode = new Node(methodNode.Id + "_I" + invocCount.ToString(), "invocs", invocation);
                    graph.AddNode(invocNode);
                    invocCount++;
                    graph.AddLink(new Link(methodNode.Id, invocNode.Id));

                    foreach (MethodDeclarationSyntax invocOption in invocation.Methods)
                    {
                        graph.AddLink(new Link(invocNode.Id, graph.GetNode(codeAnalysis.GetFullMethodName(invocOption)).Id));
                    }
                }
            }

            return graph;
        }

        // corrects all saved line numbers of invocations with respect to the code which will be insterted
        public static void PreCorrectLineNumbers(YoYoGraph graph)
        {
            // first create a dictionary ["method body start line", "number of lines to insert"]
            var LinesToInsertAtLine = new Dictionary<int, int>();

            foreach (Node methodNode in graph.GetMethodNodes())
            {
                int methodBodyStartLine = methodNode.Method.Body.OpenBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line;
                int NoIncomingInvocs = graph.GetIncomingLinks(methodNode.Id).Count;
                int NoLinesToInsert = NoIncomingInvocs > 0 ? 1 + 2 * NoIncomingInvocs : 0;
                LinesToInsertAtLine.Add(methodBodyStartLine, NoLinesToInsert);
            }

            // correct all saved line numbers by adding the number of lines getting inserted before
            foreach (Node invocNode in graph.GetInvocationNodes())
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

        // searches for a random folder name in the user's temporary folder which not yet exists to safely backup
        public static string FindBackupDirectory()
        {
            string backupDir;

            do
            {
                backupDir = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            } while (Directory.Exists(backupDir));

            return backupDir;
        }
        
        // creates the previously determined backup folder and copies the given file into it
        public static void BackupFile(string file)
        {
            Directory.CreateDirectory(backupDirectory);
            File.Copy(file, Path.Combine(backupDirectory, Path.GetFileName(file)));
        }
        
        // restores backuped file and deletes the backup folder
        public static void RestoreFile(string file)
        {
            File.Copy(Path.Combine(backupDirectory, Path.GetFileName(file)), file, true);
            Directory.Delete(backupDirectory, true);
        }
    }
}
