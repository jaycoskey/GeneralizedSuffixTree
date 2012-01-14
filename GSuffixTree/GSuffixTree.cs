// Jay Coskey, January 2012.  Seattle, WA, USA.

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace TextAlgorithms
{
    namespace SuffixTree
    {
        /// <summary>
        ///     A generalized suffix tree, created by using an extension of Ukkonen's algorithm.
        /// </summary>
        public class GSuffixTree
        {
            #region Lifecycle
            public GSuffixTree(IEnumerable<string> words, bool doConsoleVerbose = false)
            {
                if (words == null || words.Count() == 0)
                {
                    throw new ArgumentException("Error: A new GSuffixTree must be constructed with at least one word");
                }
                this.wordDict = new Dictionary<int, string>();
                this.root = new GstNode(this, null);

                foreach (string word in words.Where(w => w.Length > 0))
                {
                    if (AddWord(word, doConsoleVerbose))
                    {
                        GstUtil.WriteLine(GstVerbosityLevel.Verbose, String.Format(
                            "Added word (\"{0:s}\") to suffix tree", word));
                    } else {
                        GstUtil.WriteLine(GstVerbosityLevel.Quiet, String.Format(
                            "Error: Failed to add word (\"{0:s}\") to suffix tree", word));
                    }
                }
                // GstUtil.Write(GstVerbosityLevel.Normal, this.ToString());
            }
            #endregion // Lifecycle

            #region Public fields
            public static readonly int NoWordNum = -1;
            public static GstVerbosityLevel Verbosity = GstVerbosityLevel.Quiet;
            public readonly static int InfiniteIndex = -1;

            public int EdgeCount = 0;
            public int NodeCount = 0;
            #endregion // Public fields

            #region Public properties / methods
            private bool AddWord(string word, bool doConsoleVerbose = false)
            {
                if (word == null || word.Length == 0) { return false; }

                GstUtil.WriteLine(GstVerbosityLevel.Verbose, new String('-', 40));
                int wordNum = wordCount++;
                wordDict[wordNum] = word;
                GstUtil.WriteLine(GstVerbosityLevel.Verbose, String.Format(
                    "Adding word #{0:d} (\"{1:s}\") to the suffix tree",
                    wordNum, wordDict[wordNum]));
                GstSuffix active = new GstSuffix(this, root, wordNum, 0, GSuffixTree.InfiniteIndex);
                GstUtil.WriteLine(GstVerbosityLevel.Verbose, String.Format(
                    "Created active (longest proper) suffix pointer: {0:s}",
                    active.ToString()));
                int endIndex = 0;
                if (wordNum > 0)
                {
                    skipDuplicateInitialSubstring(ref active, ref endIndex, wordNum);
                    if (endIndex > 0)
                    {
                        GstUtil.WriteLine(GstVerbosityLevel.Verbose, String.Format(
                            "The first {0:d} letter(s) of word #{1:d} are already in the suffix tree",
                            endIndex, wordNum));
                    }
                }
                for (   ; endIndex < wordDict[wordNum].Length; endIndex++)
                {
                    GstUtil.WriteLine(GstVerbosityLevel.Verbose, this.ToString());
                    GstUtil.WriteLine(GstVerbosityLevel.Verbose, String.Format(
                        "Calling extendSuffixes() with endIndex = {0:d} ('{1:c}') and active suffix = {2:s}",
                        endIndex, GetWordChar(wordNum, endIndex), active.ToString()));
                    extendSuffixes(ref active, endIndex, wordNum);
                }
                if (doConsoleVerbose)
                {
                    string logStr = String.Format("Done adding word #{0:d} (\"{1:s}\") to the suffix tree",
                        wordNum, wordDict[wordNum]);
                    GstUtil.WriteLine(GstVerbosityLevel.Verbose, logStr);
                    Console.WriteLine(logStr);
                    Console.WriteLine(this.ToString());
                }
                return true;
            }

            public IEnumerable<DepthTaggedGstEdge> DepthTaggedEdges()
            {
                Stack<DepthTaggedGstEdge> dtEdges = new Stack<DepthTaggedGstEdge>();
                foreach (GstEdge edge in root.ChildEdges()) { dtEdges.Push(new DepthTaggedGstEdge(edge, 1)); }
                while (dtEdges.Count > 0)
                {
                    DepthTaggedGstEdge dtEdge = dtEdges.Pop();
                    foreach (GstEdge childEdge in dtEdge.Edge.ChildNode.ChildEdges())
                    {
                        dtEdges.Push(new DepthTaggedGstEdge(childEdge, dtEdge.Depth + 1));
                    }
                    yield return dtEdge;
                }
                yield break;
            }

            public IEnumerable<GstEdge> Edges()
            {
                Stack<GstEdge> edges = new Stack<GstEdge>();
                foreach (GstEdge edge in root.ChildEdges()) { edges.Push(edge); }
                while (edges.Count > 0)
                {
                    GstEdge edge = edges.Pop();
                    foreach (GstEdge childEdge in edge.ChildNode.ChildEdges())
                    {
                        edges.Push(childEdge);
                    }
                    yield return edge;
                }
                yield break;
            }

            public int FullEdgeCount()
            {
                return Edges().Count();
            }

            public string GetRangeString(int wordNum, int index1, int index2)
            {
                return GetWord(wordNum).Substring(index1, index2 - index1 + 1);
            }

            public string GetWord(int wordNum)
            {
                return wordDict[wordNum];
            }

            public char GetWordChar(int wordNum, int i)
            {
                return GetWord(wordNum)[i];
            }

            public IEnumerable<GstNode> Nodes()
            {
                yield return root;
                Stack<GstEdge> edges = new Stack<GstEdge>();
                foreach (GstEdge edge in root.ChildEdges()) { edges.Push(edge); }
                while (edges.Count > 0)
                {
                    GstEdge edge = edges.Pop();
                    foreach (GstEdge childEdge in edge.ChildNode.ChildEdges())
                    {
                        edges.Push(childEdge);
                    }
                    yield return edge.ChildNode;
                }
                yield break;
            }

            public GstNode Root
            {
                get { return root; }
            }

            public void SetWord(int numWord, string val)
            {
                wordDict[numWord] = val;
            }

            public override string ToString()
            {
                bool doAddIds = true;
                bool doAddTree = true;
                StringBuilder sb = new StringBuilder(toStringHeader());
                if (Edges().Count() == 0)
                {
                    return sb.ToString();
                } 
                sb.AppendLine();
                string edgeTableStr = toStringEdgeTable(-1, doAddIds, doAddTree);
                sb.Append(edgeTableStr);
                return sb.ToString();
            }

            public int WordCount
            {
                get { return wordDict.Count(); }
            }

            #endregion // Public properties / methods

            #region Private methods
            /// <summary>
            ///     Rule #1 (Ukkonen's first group of t_i-transitions): Try to find matching edge for the parent node.
            /// </summary>
            /// <param name="parentNode">This is a member of active.  It is kept separate for clarity.</param>
            private ExtensionResult extendSuffixByRuleOne(
                ref GstSuffix active, ref GstNode parentNode, int endIndex, int wordNum)
            {
                if (active.IsExplicit)
                {
                    GstEdge edge = active.OriginNode.GetChildEdge(GetWordChar(wordNum, endIndex));
                    if (edge != null && edge.IsSet())
                    {
                        return ExtensionResult.Done;
                    }
                }
                else    // active suffix is implicit
                {
                    GstEdge edge = active.OriginNode.GetChildEdge(GetWordChar(wordNum, active.BeginIndex));
                    int span = active.EndIndex - active.BeginIndex;
                    if (edge != null)
                    {
                        int extantWordNum = edge.GetExtantWordNum();
                        if (GetWordChar(extantWordNum, edge.GetBeginIndex(extantWordNum) + span + 1)
                            == GetWordChar(wordNum, endIndex))
                        {
                            return ExtensionResult.Done;
                        }
                        GstUtil.WriteLine(GstVerbosityLevel.Verbose, String.Format(
                            "  Rule #1: About to split edge E{0:d} (\"{1:s}\") at suffix {2:s}",
                            edge.Id, edge.GetText(), active.ToString()));
                        parentNode = edge.Split(active);
                    }
                }
                return ExtensionResult.NotDone;
            }

            /// <summary>
            ///     Rule #2 (Ukkonen's second group of t_i-transitions):
            ///         Create a new edge and add it to the tree at the parent's position.
            //          Part of this is inserting the new edge into the hash table,
            //          and creating a suffix link to the new node from the last one visited.
            /// </summary>
            /// <param name="parentNode">This is a member of active.  It is kept separate for clarity.</param>
            private void extendSuffixByRuleTwo(
                ref GstSuffix active, GstNode parentNode, ref GstNode prevParentNode, int endIndex, int wordNum)
            {
                GstEdge newEdge = new GstEdge(this, parentNode, wordNum, endIndex, GetWord(wordNum).Length - 1);
                newEdge.Add();
                GstUtil.WriteLine(GstVerbosityLevel.Verbose, String.Format(
                    "  Rule #2: New edge E{0:d} (\"{1:s}\") connects N{2:d} (old parent) to N{3:d} (new child)",
                    newEdge.Id,
                    newEdge.GetText(),
                    newEdge.ParentNode.Id,
                    newEdge.ChildNode.Id
                    ));
                setSuffixLink(prevParentNode, parentNode);
                prevParentNode = parentNode;
            }

            private void extendSuffixes(ref GstSuffix active, int beginIndex, int wordNum)
            {
                GstNode parentNode;
                GstNode prevParentNode = null;

                for (   ; ; incrSuffix(ref active, wordNum))
                {
                    parentNode = active.OriginNode;
                    if (extendSuffixByRuleOne(ref active, ref parentNode, beginIndex, wordNum)
                        == ExtensionResult.Done)
                    {
                        break;
                    }
                    extendSuffixByRuleTwo(ref active, parentNode, ref prevParentNode, beginIndex, wordNum);
                }
                setSuffixLink(prevParentNode, parentNode);
                active.EndIndex++;
                active.Canonicalize();
            }

            private void incrSuffix(ref GstSuffix active, int wordNum)
            {
                int origNodeId, begin, end;
                origNodeId = active.OriginNode.Id;
                begin = active.BeginIndex;
                end = active.EndIndex;

                if (active.OriginNode.IsRoot())
                {
                    active.BeginIndex++;
                }
                else
                {
                    active.OriginNode = active.OriginNode.SuffixNode;   // TODO: BUGFIX: SuffixNode can be null
                }
                active.Canonicalize();

                if (origNodeId != active.OriginNode.Id
                    || begin != active.BeginIndex
                    || end != active.EndIndex)
                {
                    GstUtil.WriteLine(GstVerbosityLevel.Verbose, String.Format(
                        "  incrSuffix: Active suffix changed from {0:s} to {1:s}",
                        GstSuffix.ToSuffixString(origNodeId, begin, end),
                        GstSuffix.ToSuffixString(active.OriginNode.Id, active.BeginIndex, active.EndIndex)));
                }
            }

            private void setSuffixLink(GstNode node, GstNode suffixNode)
            {
                if ((node != null) && (node != root))
                {
                    if (node.SuffixNode == null)
                    {
                        GstUtil.WriteLine(GstVerbosityLevel.Verbose, String.Format(
                            "  New suffix link from N{0:d} to N{1:d}",
                            node.Id, suffixNode.Id));
                    }
                    else
                    {
                        if (node.SuffixNode.Id == suffixNode.Id)
                        {
                            GstUtil.WriteLine(GstVerbosityLevel.Verbose, String.Format(
                                "  Suffix link (N{0:d} to N{1:d}) retaining same value",
                                node.Id, node.SuffixNode.Id));
                        }
                        else
                        {
                            GstUtil.WriteLine(GstVerbosityLevel.Verbose, String.Format(
                                "  Suffix link (N{0:d} to N{1:d}) set to new value (N{0:d} to N{2:d})",
                                node.Id, node.SuffixNode.Id, suffixNode.Id));
                        }
                    }
                    node.SuffixNode = suffixNode;
                }
            }

            /// <summary>
            ///     Traverse the suffix tree, following the longest path from the root that matches a prefix of words[wordNum].
            ///     This allows the caller to skip over these duplicate characters, and process only the part of the coming word.
            ///  </summary>
            /// <param name="active">The current active suffix</param>
            /// <param name="endIndex">The number of characters skipped</param>
            /// <param name="wordNum">The index of the current word begin processed</param>
            /// <seealso cref="http://www.cs.uku.fi/~kilpelai/BSA05/lectures/slides08.pdf">
            ///     The first 10 slides of this slideshow by Pekka Kilpeläinen
            ///     have useful tips on creating a generalized suffix tree.
            /// </seealso>
            /// <remarks>
            ///     TODO: Note: The following method is WORK IN PROGRESS, and does not yet work.
            /// </remarks>
            private void skipDuplicateInitialSubstring(ref GstSuffix active, ref int endIndex, int wordNum)
            {
                GstNode curNode = root;
                GstEdge nextEdge = null;
                GstEdge curEdge = null;

                // Traverse matching edges
                while (
                    (endIndex < wordDict[wordNum].Length)
                    && ((nextEdge = curNode.GetChildEdge(GetWordChar(wordNum, endIndex))) != null)
                    )
                {
                    int strLen = nextEdge.GetEndIndex(0) - nextEdge.GetBeginIndex(0) + 1;
                    // edgeStr = String in next edge
                    string edgeStr = wordDict[0].Substring(nextEdge.GetBeginIndex(0), strLen);
                    // wordStr = next segment of upcoming word that corresponds to edgeStr
                    string wordStr = wordDict[wordNum].Substring(0, Math.Min(strLen, wordDict[wordNum].Length));

                    bool foundMismatch = false;
                    int numCharsMatched = 0;
                    // Traverse matching characters within edge
                    for (int i = 0; i < strLen; i++)
                    {
                        if (edgeStr[i] == wordStr[i]) { numCharsMatched++; }
                        else { foundMismatch = true; break; }
                    }
                    endIndex += numCharsMatched;
                    if (foundMismatch)
                    {
                        GstUtil.WriteLine(GstVerbosityLevel.Verbose, String.Format(
                            "  skipDuplicateInitialSubstring: Word #{0:d} does not cover existing edge #{1:d}",
                            wordNum, nextEdge.Id));
                        active.OriginNode = nextEdge.ParentNode;
                        active.EndIndex = active.BeginIndex;
                        break;
                    }
                    else
                    {
                        nextEdge.SetBeginIndex(wordNum, endIndex);
                        nextEdge.SetEndIndex(wordNum, endIndex + strLen - 1);
                        GstUtil.WriteLine(GstVerbosityLevel.Verbose, String.Format(
                            "  skipDuplicateInitialSubstring: Word #{0:d} covers existing edge #{1:d} (\"{2:s}\")",
                            wordNum, nextEdge.Id, nextEdge.ToString(wordNum)));
                        active.OriginNode = nextEdge.ChildNode;
                        active.BeginIndex += numCharsMatched;
                        active.EndIndex = active.BeginIndex;
                    }
                    // Set up next iteration of loop
                    curEdge = nextEdge;
                    curNode = curEdge.ChildNode;
                }
            }

            private string toStringEdgeBanner(int maxWordLength)
            {
                string edgesBanner = "  ParentNode ChildNode  ChldLinkId WordNum    BeginIndex EndIndex String"
                     + new String('.', Math.Max(0, maxWordLength - 6))
                     + "  Tree";
                return edgesBanner;
            }

            private string toStringEdgeTable() {
                return toStringEdgeTable(GSuffixTree.NoWordNum);
            }

            private string toStringEdgeTable(int wordNum,
                bool doAddIds = true,
                bool doAddTree = true)
            {
                StringBuilder sb = new StringBuilder();
                string edgesBanner = toStringEdgeBanner(wordDict.Values.Select(w => w.Length).Max());
                string addIdsSpacer = "  Id  ";
                int[] wordNums = (wordNum == GSuffixTree.NoWordNum)
                    ? wordDict.Keys.ToArray()
                    : new int[] { wordNum };
                if (doAddIds) { sb.Append(addIdsSpacer); }
                sb.AppendLine(edgesBanner);
                foreach (DepthTaggedGstEdge dtEdge in DepthTaggedEdges())
                {
                    GstEdge edge = dtEdge.Edge;
                    string formatStr = "  {0,-11:d}{1,-11:d}{2,-11:s}{3,-11:d}{4,-11:d}{5,-9:d}{6,-"
                        + Math.Max(7, wordDict.Values.Select(w => 1 + w.Length).Max()).ToString()
                        + ":s}";
                    for (int i = 0; i < wordNums.Length; i++)
                    {
                        if (! edge.HasWordNum(wordNums[i])) { continue; }
                        if (doAddIds) { sb.Append(String.Format("  {0,-4:d}", edge.Id)); }
                        sb.Append(String.Format(formatStr,
                            edge.ParentNode.Id, edge.ChildNode.Id,
                            (edge.ChildNode.SuffixNode == null ? "null" : edge.ChildNode.SuffixNode.Id.ToString()),
                            wordNums[i],
                            edge.GetBeginIndex(wordNums[i]), edge.GetEndIndex(wordNums[i]),
                            (new String(' ', edge.GetBeginIndex(wordNums[i]))) +
                                GetRangeString(wordNums[i], edge.GetBeginIndex(wordNums[i]), edge.GetEndIndex(wordNums[i]))
                            ));
                        if (doAddTree)
                        {
                            string depthStr = new String(' ', 2 * dtEdge.Depth - 1) + "*";
                            sb.AppendLine(depthStr);
                        }
                    }
                }
                return sb.ToString();
            }

            private string toStringHeader()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine();
                sb.AppendLine(String.Format("  Currently constructed suffixTree for {0:s}{2:s}{1:s}",
                    (wordDict.Count > 1) ? "[" : String.Empty,
                    (wordDict.Count > 1) ? "]" : String.Empty,
                    String.Join(", ", wordDict.Keys.Select( i => "\"" + GetWord(i) + "\""))
                    ));
                IEnumerable<GstNode> leafNodes = Edges()
                    .Where(e => e.ChildNode == null || e.ChildNode.HasChildEdges() == false)
                    .Select(e => e.ChildNode);
                sb.AppendLine(String.Format("    Count  of  leaf  nodes = {0:d}{1:s}",
                    leafNodes.Count(),
                    leafNodes.Count() == 0
                        ? " (None)"
                        : ": " + String.Join(", ", leafNodes.Select(n => /* "#" + */ n.Id.ToString()))
                    ));
                IEnumerable<GstNode> nodesWithSuffixLinks = Nodes().Where(n => n.SuffixNode != null);
                sb.AppendLine(String.Format("    Count of linking nodes = {0:d}{1:s}",
                    nodesWithSuffixLinks.Count(),
                    nodesWithSuffixLinks.Count() == 0
                        ? " (None)"
                        : ": " + String.Join(", ",
                            nodesWithSuffixLinks.Select(n => String.Format("{0:d} -> {1:d}", n.Id, n.SuffixNode.Id))
                        )
                    ));
                sb.Append(String.Format("  Edge count = {0:d}:", FullEdgeCount()));
                if (Edges().Count() == 0)
                {
                    sb.AppendLine(" None");
                }
                else
                {
                    sb.AppendLine();
                }
                return sb.ToString();
            }
            #endregion // Private methods

            #region Private static fields
            private int wordCount = 0;
            #endregion // Private static fields

            #region Private fields
            private GstNode root;
            private Dictionary<int, string> wordDict;
            #endregion // Private fields
        }
    }
}