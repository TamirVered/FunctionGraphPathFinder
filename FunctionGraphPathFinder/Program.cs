using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FunctionGraphPathFinder
{
    /// <summary>
    /// Contains the main program execution flow of FunctionGraphPathFinder.
    /// </summary>
    class Program
    {
        /// <summary>
        /// A backup of the <see cref="ConsoleColor"/> with which the application was started.
        /// </summary>
        static ConsoleColor startColor = Console.ForegroundColor;

        /// <summary>
        /// Indicates whether this execution of FunctionGraphPathFinder has found a relevant path.
        /// </summary>
        static bool foundAny = false;

        /// <summary>
        /// Wraps <see cref="Console.Write(string)"/> with specific color printing.
        /// </summary>
        /// <param name="text">A text string to be printed to the console.</param>
        /// <param name="color">The foreground color of the text to be printed.</param>
        private static void WriteColored(string text, ConsoleColor color)
        {
            ConsoleColor backup = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = backup;
        }

        /// <summary>
        /// Wraps <see cref="Console.WriteLine(string)"/> with specific color printing.
        /// </summary>
        /// <param name="text">A text string to be printed to the console.</param>
        /// <param name="color">The foreground color of the text to be printed.</param>
        private static void WriteLineColored(string text, ConsoleColor color)
        {
            ConsoleColor backup = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = backup;
        }

        /// <summary>
        /// Prints usage help string to the console.
        /// </summary>
        private static void PrintHelp()
        {
            WriteLineColored("This utility receives a global call-graph exported as JSON from cutter and finds all paths between two given functions in it.", ConsoleColor.Yellow);
            WriteColored("Usage: ", ConsoleColor.Yellow);
            Console.WriteLine("FunctionGraphPathFinder.exe [path] [source function] [target function]");
            WriteColored("Example: ", ConsoleColor.Yellow);
            Console.WriteLine("FunctionGraphPathFinder.exe \"C:\\re\\call-graph.json\" fcn.00114818 fcn.00115e4d");
            WriteLineColored("Output, when there are three relevant paths:" + Environment.NewLine, ConsoleColor.Yellow);
            WriteColored("fnc.00114818", ConsoleColor.Red);
            Console.Write(" -> ");
            WriteColored("fnc.00124ee6", ConsoleColor.Green);
            Console.Write(" -> ");
            WriteLineColored("fnc.00115e4d" + Environment.NewLine, ConsoleColor.Red);

            WriteColored("fnc.00114818", ConsoleColor.Red);
            Console.Write(" -> ");
            WriteColored("FunctionWithCutomName", ConsoleColor.Green);
            Console.Write(" -> ");
            WriteColored("fnc.00124ee6", ConsoleColor.Green);
            Console.Write(" -> ");
            WriteLineColored("fnc.00115e4d" + Environment.NewLine, ConsoleColor.Red);

            WriteColored("fnc.00114818", ConsoleColor.Red);
            Console.Write(" -> ");
            WriteColored("FunctionWithCutomName", ConsoleColor.Green);
            Console.Write(" -> ");
            WriteLineColored("fnc.00115e4d" + Environment.NewLine, ConsoleColor.Red);

            WriteLineColored("Note: Cutter tends to omit special characters (such as '?') from the exported graph, but only in some contexts. Make sure to preprocess the json accordingly", ConsoleColor.Blue);
        }

        /// <summary>
        /// Prints all paths from <see cref="startFunction"/> to <see cref="targetFunction"/> in a given <see cref="graph"/>.
        /// </summary>
        /// <param name="graph">Specifies the graph in which the search will occur.</param>
        /// <param name="startFunction">Specifies the function from which the graph search should find paths.</param>
        /// <param name="targetFunction">Specifies the function to which the graph search should find paths.</param>
        /// <typeparam name="T">Specifies the type of the nodes in the graph.</typeparam>
        public static void PrintAllPaths<T>(Dictionary<T, T[]> graph, T startFunction, T targetFunction)
        {
            HashSet<T> isVisited = new HashSet<T>();
            List<T> pathList = new List<T>();

            pathList.Add(startFunction);

            InnerPrintAllPaths(graph, startFunction, targetFunction, isVisited, pathList);
        }

        /// <summary>
        /// Recursively prints all paths from <see cref="startFunction"/> to <see cref="targetFunction"/> in a given <see cref="graph"/>.
        /// </summary>
        /// <param name="graph">Specifies the graph in which the search will occur.</param>
        /// <param name="currentFunction">Specifies the function from which the graph search should find paths.</param>
        /// <param name="targetFunction">Specifies the function to which the graph search should find paths.</param>
        /// <param name="activeVisitedNodes">A collection of nodes that were already visited by the search.</param>
        /// <param name="localPathList">Specifies the path currently being searched.</param>
        /// <typeparam name="T">Specifies the type of the nodes in the graph.</typeparam>
        private static void InnerPrintAllPaths<T>(Dictionary<T, T[]> graph, T currentFunction, T targetFunction,
                                       HashSet<T> activeVisitedNodes,
                                       List<T> localPathList)
        {
            if (currentFunction.Equals(targetFunction))
            {
                if (foundAny)
                {
                    Console.WriteLine();
                }

                foundAny = true;
                int count = localPathList.Count;
                for (int i = 0; i < localPathList.Count; i++)
                {
                    if (i == 0)
                    {
                        WriteColored(localPathList[i].ToString(), ConsoleColor.Red);
                        continue;
                    }
                    Console.Write(" -> ");
                    if (i == localPathList.Count - 1)
                    {
                        WriteColored(localPathList[i].ToString(), ConsoleColor.Red);
                    }
                    else
                    {
                        WriteColored(localPathList[i].ToString(), ConsoleColor.Green);
                    }
                }

                Console.WriteLine();
                return;
            }

            if (!graph.ContainsKey(currentFunction))
            {
                return;
            }

            activeVisitedNodes.Add(currentFunction);

            foreach (T node in graph[currentFunction])
            {
                if (!activeVisitedNodes.Contains(node))
                {
                    localPathList.Add(node);
                    InnerPrintAllPaths(graph, node, targetFunction, activeVisitedNodes, localPathList);
                    localPathList.Remove(node);
                }
            }

            activeVisitedNodes.Remove(currentFunction);
        }

        static void Main(string[] args)
        {
            Console.CancelKeyPress += closeEvent;

            if (args.Length != 3)
            {
                PrintHelp();
                return;
            }

            Dictionary<string, string[]> graph;
            string currentError = "Invalid path. For help use - h.";
            try
            {
                string filePath = Path.GetFullPath(args[0]);
                currentError = "Error reading file.";
                string fileContent = File.ReadAllText(filePath);
                currentError = "Invalid JSON file.";
                var deserializedData = JsonSerializer.Deserialize<FunctionGraphNodeDataModel[]>(fileContent);
                graph = deserializedData.ToDictionary(x => x.Name, x => x.Imports);
            }
            catch (Exception exception)
            {
                WriteLineColored(currentError, ConsoleColor.Red);
                Console.WriteLine("Exception details:");
                Console.WriteLine(exception.Message);
                return;
            }
            
            if (!graph.ContainsKey(args[1]))
            {
                WriteLineColored($"JSON file does not contain function: \"{args[1]}\"", ConsoleColor.Red);
                return;
            }

            if (!graph.ContainsKey(args[2]))
            {
                if (!graph.Any(x => x.Value.Contains(args[2])))
                {
                    WriteLineColored($"JSON file does not contain target function: \"{args[2]}\"", ConsoleColor.Red);
                    return;
                }
            }

            PrintAllPaths(graph, args[1], args[2]);

            if (!foundAny)
            {
                WriteLineColored($"Could not find path between \"{args[1]}\" and \"{args[2]}\"", ConsoleColor.Red);
            }
        }

        /// <summary>
        /// Backs up the console color on SIGINT (Ctrl + C).
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">A <see cref="ConsoleCancelEventArgs"/> object that contains the event data.</param>
        protected static void closeEvent(object sender, ConsoleCancelEventArgs args)
        {
            Console.ForegroundColor = startColor;
        }
    }
}
