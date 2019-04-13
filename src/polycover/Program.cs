using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using polycover.Graphs;
using Type = polycover.Graphs.Type;

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
                    throw new FileNotFoundException(new string[] { "Code", "Solution", "Project", "Assembly" }[i - 2] + $" file does not exist: '{args[i]}'");
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
            string coverletCommand = String.Join(" ", new string[] { "coverlet", testDllPath, target, targetargs, output });

            string buildCommand = String.Join(" ", new string[] { "dotnet", "build", solPath });


            StaticCodeAnalysis codeAnalysis = new StaticCodeAnalysis(codePath);

            GraphCreator graphCreator = new GraphCreator(codeAnalysis);
            DirectedGraph graph;
            string coverageTargets;
            bool hasCoverageTargets;
            if (isVariantEdge)
            {
                graph = graphCreator.CreateYoYoGraph();
                // if the source code doesn't contain invocations then output 100% coverage and return
                coverageTargets = "invocations";
                hasCoverageTargets = graph.GetNodesOfType(Type.INVOCATION).Any();
            }
            else
            {
                graph = graphCreator.CreateInheritanceGraph();

                coverageTargets = "methods";
                hasCoverageTargets = graph.GetNodesOfType(Type.METHOD).Any();
            }

            // if the source code doesn't contain coverage targets then output 100% coverage and return
            if (!hasCoverageTargets)
            {
                Console.WriteLine(Environment.NewLine + $"The given source code does not contain any {coverageTargets}" + Environment.NewLine);
                OutputResult(1);
                return 0;
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
                RunCMDCommand(buildCommand, "Re-building solution ...", "build succeed", "build failed");

                // start coverlet
                RunCMDCommand(coverletCommand, "Starting coverlet ...", "coverlet calculation succeed", "coverlet calculation failed");

                // read the generated json file & calculate the coverage
                Console.WriteLine(Environment.NewLine + "Calculating coverage result ...");
                Coverage coverage = new Coverage(coveragePath, codePath, graph);
                double result = coverage.Calculate();

                OutputResultAndGraph(result, graph);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 1;
            }
            finally
            {
                RestoreFile(codePath);
            }

            return 0;
        }

        // helper functions:
        // =================

        // creates a file for the graph and writes the percentage result to console
        public static void OutputResultAndGraph(double result, DirectedGraph graph)
        {
            string graphPath = $"{codePath.Substring(0, codePath.Length - 3)}_{graph.GetType().Name}.dgml";
            graph.Serialize(graphPath);
            Console.WriteLine($"Generated graph '{graphPath}'");
            OutputResult(result);
        }

        // writes the percentage result to console
        public static void OutputResult(double result)
        {
            Console.WriteLine(Environment.NewLine + "Coverage result: {0:P2}", result);
        }

        // creates and starts a new process to execute a given command on windows shell
        public static void RunCMDCommand(string command, string startMessage, string successMessage, string failMessage)
        {
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = "/C " + command;
            process.StartInfo.RedirectStandardOutput = true; // don't write stdout to console
            process.StartInfo.UseShellExecute = false; // don't create new window
            Console.WriteLine(Environment.NewLine + startMessage);
            process.Start();
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                throw new Exception(failMessage);
            }
            Console.WriteLine(successMessage);
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
