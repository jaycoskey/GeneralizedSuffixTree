using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;
using System.Reflection;
using TextAlgorithms.SuffixTree;

namespace TextAlgorithms
{
    namespace SuffixTree
    {
        namespace SuffixTreeTest
        {
            [AttributeUsage(AttributeTargets.Method)]
            class SuffixTreeTestMethodAttribute : Attribute { }

            public static class SuffixTreeTest
            {
                #region Public static methods
                /// <summary>
                ///     Run tests tagged with a particular attribute, and invoke them using reflection.
                ///     Currently, only tests from this class are invoked. 
                /// </summary>
                public static void Main(string[] args)
                {
                    bool isDone = false;
                    while (!isDone)
                    {
                        Console.Write("Enter input string with unique final character: ");
                        Console.Out.Flush();
                        string word = Console.ReadLine();

                        SuffixTree.Verbosity = StVerbosityLevel.Verbose;
                        SuffixTree tree = null;
                        bool isCreationSuccessful = true;
                        try
                        {
                            tree = new SuffixTree(word);
                            Console.WriteLine("Final suffix tree:");
                            Console.WriteLine(tree.ToString());
                        }
                        catch (Exception ex)
                        {
                            isCreationSuccessful = false;
                            Console.WriteLine();
                            Console.WriteLine(String.Format(
                                "Suffix tree creation: Caught exception: {0;s}", ex.Message));
                        }
                        Console.WriteLine();
                        if (isCreationSuccessful)
                        {
                            Console.Write("Press 'Enter' to proceed with validation: ");
                            Console.Out.Flush();
                            Console.ReadLine();
                            try
                            {
                                runTests(tree);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine();
                                Console.WriteLine(String.Format(
                                    "Suffix tree testing: Caught exception: {0;s}", ex.Message));
                            }
                        }
                        Console.Write("Continue (y or n)? ");
                        Console.Out.Flush();
                        string continueStr = Console.ReadLine();
                        if (continueStr == null || continueStr.Length > 0 && continueStr.ToLower()[0] != 'y')
                        {
                            isDone = true;
                        }
                    }

                }
                #endregion Public static methods

                #region Private static methods
                private static void runTests(SuffixTree tree)
                {
                    var testMethods = // MethodBase.GetCurrentMethod().DeclaringType
                        (typeof(SuffixTreeTest)).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                        .Where(m => m.GetCustomAttributes(typeof(SuffixTreeTestMethodAttribute), false).Length > 0)
                        .OrderBy(m => m.Name);
                    int numTests = 0;
                    int numFailedTests = 0;
                    foreach (MethodInfo mi in testMethods)
                    {
                        DescriptionAttribute descriptor = (DescriptionAttribute)
                            mi.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault();
                        bool didPassTest = (bool)mi.Invoke(null, new Object[1] { tree });
                        numTests++;
                        if (!didPassTest)
                        {
                            numFailedTests++;
                            Console.WriteLine("Failed test: {0:s}", descriptor.Description);
                        }
                    }
                    Console.WriteLine(String.Format("Passed {0:d} of {1:d} tests",
                        numTests - numFailedTests, numTests));
                }

                [SuffixTreeTestMethodAttribute]
                [Description("Ensure that the suffix tree paths from root to leaf nodes are indeed suffixes.")]
                private static bool validateSuffixStrings(SuffixTree tree)
                {
                    List<int> failedLeafNodeIds;
                    bool result = validateSuffixStrings(tree, out failedLeafNodeIds);
                    if (!result)
                    {
                        Console.WriteLine("Error: Some paths had strings that did not match the underlying text.");
                        Console.WriteLine(String.Format("Failed leaf nodes: {0:s}",
                            failedLeafNodeIds.Count == 0
                                ? "None"
                                : String.Join(", ", failedLeafNodeIds.Select(x => x.ToString()))
                            ));
                    }
                    return result;
                }

                private static bool validateSuffixStrings(
                    SuffixTree tree,
                    out List<int> failedLeafNodeIds)
                {
                    Stack<Tuple<StEdge, string>> edgeStrings = new Stack<Tuple<StEdge, string>>();

                    // Step 1: Populate edgeStrings with data from child edges of the root node.
                    //         Track any leaves that are immedage children of the root node.
                    List<Tuple<StEdge, string>> leafEdgeStrings = new List<Tuple<StEdge, string>>();
                    foreach (StEdge edge in tree.Root.ChildEdges()) {
                        Tuple<StEdge, string> edgeString = new Tuple<StEdge, string>(edge, tree.EdgeSubstring(edge));
                        edgeStrings.Push(edgeString);
                        if (!edge.ChildNode.HasChildEdges())
                        {
                            Console.WriteLine(String.Format("SuffixTreeTest: Found a leaf: {0:s}", edgeString.Item2));
                            leafEdgeStrings.Add(edgeString);
                        }
                    }

                    // Step 2: Walk the tree, adding the remaining edges.  Keep track of leaf edges.
                    while (edgeStrings.Count > 0)
                    {
                        Tuple<StEdge, string> edgeString = edgeStrings.Pop();
                        foreach (StEdge childEdge in edgeString.Item1.ChildNode.ChildEdges())
                        {
                            Tuple<StEdge, string> newEdgeString = new Tuple<StEdge, string>(
                                childEdge, edgeString.Item2 + tree.EdgeSubstring(childEdge));
                            edgeStrings.Push(newEdgeString);
                            if (!childEdge.ChildNode.HasChildEdges())
                            {
                                Console.WriteLine(String.Format("SuffixTreeTest: Found a leaf: {0:s}", newEdgeString.Item2));
                                leafEdgeStrings.Add(newEdgeString);
                            }
                        }
                    }

                    // Step 3: Inspect the leaf edge data.  Keep track of failed leaf nodes
                    failedLeafNodeIds = new List<int>();
                    foreach (var leafEdgeString in leafEdgeStrings)
                    {
                        // Accumulated string should equal the corresponding substring of tree.Text.
                        int len = leafEdgeString.Item2.Length;
                        string pathStr = leafEdgeString.Item2;
                        string textStr = tree.RangeString(tree.Text.Length - len, tree.Text.Length - 1);
                        string formatSpec = "{0," + tree.Text.Length.ToString() + ":s}";
                        string formatStr = String.Format(
                            "SuffixTreeTest: About to compare \"{0:s}\" with \"{1:s}\"",
                            formatSpec, formatSpec);                            
                        Console.WriteLine(formatStr, pathStr, textStr);
                        if (pathStr != textStr)
                        {
                            failedLeafNodeIds.Add(leafEdgeString.Item1.ChildNode.Id);
                        }
                    }
                    return (failedLeafNodeIds.Count() == 0);
                }
                #endregion // Private static methods
            }
        }
    }
}