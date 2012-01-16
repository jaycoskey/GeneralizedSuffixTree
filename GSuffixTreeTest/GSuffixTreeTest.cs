using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;
using System.Reflection;
using TextAlgorithms.SuffixTree;

using EdgeStringDict = System.Tuple<TextAlgorithms.SuffixTree.GstEdge, System.Collections.Generic.Dictionary<int, string>>;
using StringDict = System.Collections.Generic.Dictionary<int, string>;

namespace TextAlgorithms
{
    namespace SuffixTree
    {
        namespace GSuffixTreeTest
        {
            [AttributeUsage(AttributeTargets.Method)]
            class SuffixTreeTestMethodAttribute : Attribute { }

            public static class SuffixTreeTest
            {
                #region Public static methods
                public static void Main(string[] args)
                {
                    bool isDone = false;
                    while (!isDone)
                    {
                        Console.WriteLine("Note: Each word entered must end with a character "
                            + "that does not appear earlier in the word.");
                        List<string> words = new List<string>();
                        for (int wordNum = 0; ; wordNum++)
                        {
                            Console.Write(String.Format("Enter word #{0:d}{1:s}: ",
                                wordNum,
                                wordNum == 0
                                    ? " (with a unique terminating character) "
                                    : " (enter an empty string to end input)  "
                                ));
                            Console.Out.Flush();
                            string word = Console.ReadLine();
                            if (word == null || word.Length == 0)
                            {
                                break;
                            }
                            else
                            {
                                words.Add(word);
                            }
                        }

                        GSuffixTree.Verbosity = GstVerbosityLevel.Verbose;
                        GSuffixTree tree = null;
                        bool isCreationSuccessful = false;
                        try
                        {
                            tree = new GSuffixTree(words, true);
                            Console.WriteLine("Final suffix tree:");
                            Console.WriteLine(tree.ToString());
                        }
                        catch (Exception ex)
                        {
                            isCreationSuccessful = false;
                            Console.WriteLine();
                            Console.WriteLine(String.Format(
                                "Suffix tree creation: Caught exception: {0:s}", ex.Message));
                            Console.WriteLine("Note: This program is currently a work in progress.");
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
                                Console.WriteLine("Note: This program is currently a work in progress.");

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
                private static void runTests(GSuffixTree tree)
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
                [Description("Ensure that the suffix tree paths from root to leaf nodes are suffixes")]
                private static bool validateSuffixStrings(GSuffixTree tree)
                {
                    List<int> failedLeafNodeIds;
                    bool result = validateSuffixStrings(tree, out failedLeafNodeIds);
                    if (!result)
                    {
                        Console.WriteLine("Error: Some paths had strings that did not match the underlying text.");
                        Console.WriteLine(String.Format("Failed leaf nodes: {0:s}",
                            String.Join(", ", failedLeafNodeIds.Select(x => x.ToString()))
                            ));
                    }
                    return result;
                }

                private static bool validateSuffixStrings(
                    GSuffixTree tree,
                    out List<int> failedLeafNodeIds)
                {
                    var edgeStringDicts = new Stack<EdgeStringDict>();

                    // Step 1: Populate edgeStrings with data from child edges of the root node.
                    //         Track any leaves that are immediate children of the root node.
                    var leafEdgeStringDicts = new List<EdgeStringDict>();
                    foreach (GstEdge edge in tree.Root.ChildEdges()) {
                        var edgeStringDict = new EdgeStringDict(edge, new Dictionary<int, string>());
                        
                        foreach (int wordNum in edge.WordNums()) {
                            edgeStringDict.Item2[wordNum] = edge.GetText();
                            edgeStringDicts.Push(edgeStringDict);
                        }
                        if (!edge.ChildNode.HasChildEdges())
                        {
                            Console.WriteLine(String.Format(
                                "SuffixTreeTest: Found a leaf edge adjacent to the root: E{0:d}",
                                edge.Id));
                            leafEdgeStringDicts.Add(edgeStringDict);
                        }
                    }

                    // Step 2: Walk the tree, adding the remaining edges.  Keep track of leaf edges.
                    //      Also keep a running record of accumulated text for each edge.
                    while (edgeStringDicts.Count > 0)
                    {
                        EdgeStringDict edgeStringDict = edgeStringDicts.Pop();
                        foreach (GstEdge childEdge in edgeStringDict.Item1.ChildNode.ChildEdges())
                        {
                            EdgeStringDict newEdgeStringDict = new EdgeStringDict(childEdge, new Dictionary<int, string>());
                            foreach (int wordNum in childEdge.WordNums()) {
                                newEdgeStringDict.Item2[wordNum] = edgeStringDict.Item2[wordNum] + childEdge.GetText();
                            }
                            edgeStringDicts.Push(newEdgeStringDict);
                            if (!childEdge.ChildNode.HasChildEdges())
                            {
                                Console.WriteLine(String.Format(
                                    "SuffixTreeTest: Found a leaf not adjacent to the root: E{0:s}",
                                    newEdgeStringDict.Item1.Id));
                                leafEdgeStringDicts.Add(newEdgeStringDict);
                            }
                        }
                    }

                    // Step 3: Inspect the leaf edge content (i.e., strings).  Keep track of failed leaf nodes
                    failedLeafNodeIds = new List<int>();
                    foreach (var leafEdgeStringDict in leafEdgeStringDicts)
                    {
                        // Accumulated string should equal the corresponding substring of tree.Text.
                        GstEdge edge = leafEdgeStringDict.Item1;
                        foreach (int wordNum in leafEdgeStringDict.Item2.Keys)
                        {
                            int len = leafEdgeStringDict.Item2[wordNum].Length;
                            string pathStr = leafEdgeStringDict.Item2[wordNum];
                            string textStr = tree.GetRangeString(wordNum,
                                tree.GetWord(wordNum).Length - len, tree.GetWord(wordNum).Length - 1);
                            string formatSpec2 = "{2" /* + "," + tree.GetWord(0).Length.ToString() */ + ":s}";
                            string formatSpec3 = "{3" /* + "," + tree.GetWord(0).Length.ToString() */ + ":s}";
                            string formatStr = "SuffixTreeTest: Leaf edge #{0:d}, word#{1:d}.  "
                                + String.Format("Comparing \"{0:s}\" with \"{1:s}\"", formatSpec2, formatSpec3);
                            Console.WriteLine(formatStr, edge.Id, wordNum, pathStr, textStr);
                            if (pathStr != textStr)
                            {
                                failedLeafNodeIds.Add(leafEdgeStringDict.Item1.ChildNode.Id);
                                break;
                            }
                        }
                    }
                    return (failedLeafNodeIds.Count() == 0);
                }
                #endregion // Private static methods
            }
        }
    }
}