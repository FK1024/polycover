using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using polycover.Graphs;

namespace polycover
{
    public class Program
    {
        public static bool isVariantEdge;
        private static string codePath;
        private static string backupDirectory;

        private static void DisplayHelp()
        {
            Console.WriteLine(".NET Framework source code coverage tool" + Environment.NewLine);

            Console.WriteLine("Usage: polycover [options] [arguments]" + Environment.NewLine);

            Console.WriteLine("Options:");
            Console.WriteLine("\t" + "-v|--variant" + "\t" + "the variant of coverage to be calculated: 'edge' or 'type'" + Environment.NewLine);

            Console.WriteLine("Arguments:");
            Console.WriteLine("\t" + "<CODEFILE>" + "\t" + "Path to the source code file");
            Console.WriteLine("\t" + "<SOLUTION>" + "\t" + "Path to the solution");
            Console.WriteLine("\t" + "<TESTPROJECT>" + "\t" + "Path to the test project");
            Console.WriteLine("\t" + "<TESTASSEMBLY>" + "\t" + "Path to the test assembly");
        }

        private static void ParseArguments(string[] args)
        {
            if (args.Length != 6)
            {
                throw new ArgumentException("Wrong number of arguments or options specified.");
            }
            if (args[0] != "-v" && args[0] != "--variant")
            {
                throw new ArgumentException("No variant specified");
            }
            if (args[1] != "edge" && args[1] != "type")
            {
                throw new ArgumentException("Wrong variant specified. It need to be either 'edge' or 'type'.");
            }
            for (int i = 2; i < 6; i++)
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

            isVariantEdge = args[1] == "edge";

            backupDirectory = FindBackupDirectory();

            // input parameters:
            codePath = args[2];
            string solPath = args[3];
            string testProjPath = args[4];
            string testDllPath = args[5];

            string target = "--target dotnet";
            string targetargs = $"--targetargs \"test {testProjPath} --no-build\"";
            string coveragePath = backupDirectory + @"\coverage.json";
            string output = $"--output {coveragePath}";
            string coverletCall = String.Join(" ", new string[] { "coverlet", testDllPath, target, targetargs, output });

            string buildCall = String.Join(" ", new string[] { "dotnet", "build", solPath });


            StaticCodeAnalysis codeAnalysis = new StaticCodeAnalysis(codePath);

            GraphCreator graphCreator = new GraphCreator(codeAnalysis);
            DirectedGraph graph;
            if (isVariantEdge)
            {
                graph = graphCreator.CreateYoYoGraph();
                // if the source code doesn't contain invocations then output the graph, 100% coverage and return
                YoYoGraph yoyoGraph = graph as YoYoGraph;
                if (!yoyoGraph.GetInvocationNodes().Any())
                {
                    Console.WriteLine(Environment.NewLine + "The given source code does not contain any invocations" + Environment.NewLine);
                    // mark every method node as covered
                    foreach (Node methodNode in yoyoGraph.Nodes)
                    {
                        methodNode.IsCovered = true;
                    }
                    OutputResultAndGraph(1, yoyoGraph);
                    return 0;
                }
            }
            else
            {
                graph = graphCreator.CreateInheritanceGraph();
                // if the source code doesn't contain methods then output 100% coverage and return
                InheritanceGraph inheritanceGraph = graph as InheritanceGraph;
                if (!inheritanceGraph.GetMethodNodes().Any())
                {
                    Console.WriteLine(Environment.NewLine + "The given source code does not contain any methods" + Environment.NewLine);
                    Console.WriteLine(Environment.NewLine + "OO Edge Coverage Result: {0:P2}", 1);
                    return 0;
                }
            }

            BackupFile(codePath);

            try
            {
                // manipulate the source code file to be able to gather information needed
                if (isVariantEdge) graphCreator.PreCorrectLineNumbers(graph as YoYoGraph); // YoYo graphs contain line numbers which have to be corrected before code gets inserted
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
        public static void OutputResultAndGraph(double result, DirectedGraph graph)
        {
            string graphPath = $"{codePath.Substring(0, codePath.Length - 3)}_{graph.GetType().Name}.dgml";
            graph.Serialize(graphPath);
            Console.WriteLine($"Generated graph '{graphPath}'");

            Console.WriteLine(Environment.NewLine + "OO Edge Coverage Result: {0:P2}", result);
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
