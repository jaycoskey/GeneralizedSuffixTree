// Jay Coskey, January 2012.  Seattle, WA, USA.

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

using System.Diagnostics;

namespace TextAlgorithms
{
    namespace SuffixTree
    {
        public class SuffixTree
        {
            #region Lifecycle
            public SuffixTree(string text)
            {
                this.text = text;
                this.root = new StNode(this, null);

                StUtil.WriteLine(StVerbosityLevel.Verbose, "Creating the active (longest proper) suffix pointer");
                StSuffix active = new StSuffix(this, root, 0, InfiniteIndex);
                for (int endIndex = 0; endIndex < text.Length; endIndex++)
                {
                    StUtil.WriteLine(StVerbosityLevel.Verbose, this.ToString());
                    StUtil.WriteLine(StVerbosityLevel.Verbose, String.Format(
                        "Calling extendSuffixes() with endIndex = {0:d} ('{1:c}') and active suffix = {2:s}",
                        endIndex, text[endIndex], active.ToString()));
                    extendSuffixes(ref active, endIndex);
                }
                StUtil.Write(StVerbosityLevel.Normal, this.ToString());
            }
            #endregion // Lifecycle

            #region Public fields
            public static StVerbosityLevel Verbosity = StVerbosityLevel.Quiet;

            public readonly static int InfiniteIndex = -1;
            public int NodeCount = 0;
            #endregion // Public fields

            #region Public properties / methods
            public IEnumerable<DepthTaggedEdge> DepthTaggedEdges()
            {
                Stack<DepthTaggedEdge> dtEdges = new Stack<DepthTaggedEdge>();
                foreach (StEdge edge in root.ChildEdges()) { dtEdges.Push(new DepthTaggedEdge(edge, 1)); }
                while (dtEdges.Count > 0)
                {
                    DepthTaggedEdge dtEdge = dtEdges.Pop();
                    foreach (StEdge childEdge in dtEdge.Edge.ChildNode.ChildEdges())
                    {
                        dtEdges.Push(new DepthTaggedEdge(childEdge, dtEdge.Depth + 1));
                    }
                    yield return dtEdge;
                }
                yield break;
            }

            public IEnumerable<StEdge> Edges()
            {
                Stack<StEdge> edges = new Stack<StEdge>();
                foreach (StEdge edge in root.ChildEdges()) { edges.Push(edge); }
                while (edges.Count > 0)
                {
                    StEdge edge = edges.Pop();
                    foreach (StEdge childEdge in edge.ChildNode.ChildEdges())
                    {
                        edges.Push(childEdge);
                    }
                    yield return edge;
                }
                yield break;
            }

            public string EdgeSubstring(StEdge e)
            {
                int index1 = e.BeginIndex;
                int index2 = e.EndIndex;
                return RangeString(index1, index2);
            }

            public int FullEdgeCount()
            {
                return Edges().Count();
            }

            public IEnumerable<StNode> Nodes()
            {
                yield return root;
                Stack<StEdge> edges = new Stack<StEdge>();
                foreach (StEdge edge in root.ChildEdges()) { edges.Push(edge); }
                while (edges.Count > 0)
                {
                    StEdge edge = edges.Pop();
                    foreach (StEdge childEdge in edge.ChildNode.ChildEdges())
                    {
                        edges.Push(childEdge);
                    }
                    yield return edge.ChildNode;
                }
                yield break;
            }

            public string RangeString(int index1, int index2)
            {
                return Text.Substring(index1, index2 - index1 + 1);
            }

            public StNode Root
            {
                get { return root; }
            }

            public string Text
            {
                get { return text; }
            }

            public override string ToString()
            {
                bool doAddIds = true;
                bool doAddTree = true;
                StringBuilder sb = new StringBuilder(this.getToStringHeader());
                if (Edges().Count() == 0)
                {
                    return sb.ToString();
                } 
                sb.AppendLine();
                string edgeTableStr = getToStringEdgeTable(doAddIds, doAddTree);
                sb.Append(edgeTableStr);
                return sb.ToString();
            }
            #endregion // Public properties / methods

            #region Private methods
            // Rule 1: Try to find matching edge for the parent node.
            private ExtensionResult extendSuffixByRuleOne(
                ref StSuffix active, ref StNode parentNode, int endIndex)
            {
                if (active.IsExplicit)
                {
                    StEdge edge = active.OriginNode.GetChildEdge(text[endIndex]);
                    if (edge != null && edge.IsSet())
                    {
                        return ExtensionResult.Done;
                    }
                }
                else    // active suffix is implicit
                {
                    StEdge edge = active.OriginNode.GetChildEdge(text[active.BeginIndex]);
                    int span = active.EndIndex - active.BeginIndex;
                    if (text[edge.BeginIndex + span + 1] == text[endIndex])
                    {
                        return ExtensionResult.Done;
                    }
                    StUtil.WriteLine(StVerbosityLevel.Verbose, String.Format(
                        "  Rule #1: About to split edge E{0:d} (\"{1:s}\") at suffix {2:s}",
                        edge.Id, edge.GetText(), active.ToString()));
                        parentNode = edge.Split(active);
                }
                return ExtensionResult.NotDone;
            }

            // Rule 2: Create a new edge and add it to the tree at the parent's position.
            //     Part of this is inserting the new edge into the hash table,
            //     and creating a suffix link to the new node from the last one visited.
            private void extendSuffixByRuleTwo(
                ref StSuffix active, StNode parentNode, ref StNode prevParentNode,  int endIndex)
            {
                StEdge newEdge = new StEdge(this, parentNode, endIndex, this.text.Length - 1);
                newEdge.Add();
                StUtil.WriteLine(StVerbosityLevel.Verbose, String.Format(
                    "  Rule #2: New edge E{0:d} (\"{1:s}\") connects N{2:d} (old parent) to N{3:d} (new child)",
                    newEdge.Id,
                    newEdge.GetText(),
                    newEdge.ParentNode.Id,
                    newEdge.ChildNode.Id
                    ));
                setSuffixLink(prevParentNode, parentNode);
                prevParentNode = parentNode;
            }

            private void extendSuffixes(ref StSuffix active, int beginIndex)
            {
                StNode parentNode;
                StNode prevParentNode = null;

                for (   ; ; incrSuffix(ref active))
                {
                    parentNode = active.OriginNode;
                    if (extendSuffixByRuleOne(ref active, ref parentNode,  beginIndex) == ExtensionResult.Done)
                    {
                        break;
                    }
                    extendSuffixByRuleTwo(ref active, parentNode, ref prevParentNode, beginIndex);
                }
                setSuffixLink(prevParentNode, parentNode);
                active.EndIndex++;
                active.Canonicalize();
            }

            private string getToStringEdgeBanner()
            {
                string edgesBanner = "  ParentNode ChildNode  ChldLinkId BeginIndex EndIndex String"
                     + new String('.', Math.Max(0, text.Length - 6))
                     + "  Tree";
                return edgesBanner;
            }

            private string getToStringEdgeTable(bool doAddIds, bool doAddTree)
            {
                StringBuilder sb = new StringBuilder();
                string edgesBanner = getToStringEdgeBanner();
                string addIdsSpacer = "  Id  ";
                if (doAddIds) { sb.Append(addIdsSpacer); }
                sb.AppendLine(edgesBanner);
                foreach (DepthTaggedEdge dtEdge in DepthTaggedEdges())
                {
                    StEdge edge = dtEdge.Edge;
                    string formatStr = "  {0,-11:d}{1,-11:d}{2,-11:s}{3,-11:d}{4,-9:d}{5,-"
                        + text.Length.ToString()
                        + ":s}";
                    if (doAddIds) { sb.Append(String.Format("  {0,-4:d}", edge.Id)); }
                    sb.Append(  // edge.ToString(text)
                        String.Format(formatStr,
                        edge.ParentNode.Id, edge.ChildNode.Id,
                        (edge.ChildNode.SuffixNode == null ? "null" : edge.ChildNode.SuffixNode.Id.ToString()),
                        edge.BeginIndex, edge.EndIndex,
                        (new String(' ', edge.BeginIndex)) +
                            RangeString(edge.BeginIndex, edge.EndIndex)));
                    if (doAddTree)
                    {
                        string depthStr = new String(' ', 2 * dtEdge.Depth) + "*";
                        sb.AppendLine(depthStr);
                    }
                }
                return sb.ToString();
            }

            private string getToStringHeader()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine();
                sb.AppendLine(String.Format("  Currently constructed SuffixTree for \"{0:s}\" (length={1:d})",
                        text, text.Length));
                IEnumerable<StNode> leafNodes = Edges()
                    .Where(e => e.ChildNode == null || e.ChildNode.HasChildEdges() == false)
                    .Select(e => e.ChildNode);
                sb.AppendLine(String.Format("  Count  of  leaf  nodes = {0:d}{1:s}",
                    leafNodes.Count(),
                    leafNodes.Count() == 0
                        ? " (None)"
                        : ": " + String.Join(", ", leafNodes.Select(n => /* "#" + */ n.Id.ToString()))
                    ));
                IEnumerable<StNode> nodesWithSuffixLinks = Nodes().Where(n => n.SuffixNode != null);
                sb.AppendLine(String.Format("  Count of linking nodes = {0:d}{1:s}",
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

            private void incrSuffix(ref StSuffix active)
            {
                int origNodeId, begin, end;
                origNodeId = active.OriginNode.Id;
                begin = active.BeginIndex;
                end = active.EndIndex;

                if (active.OriginNode.IsRoot()) { active.BeginIndex++; }
                else { active.OriginNode = active.OriginNode.SuffixNode; }
                active.Canonicalize();

                if (origNodeId != active.OriginNode.Id
                    || begin != active.BeginIndex
                    || end != active.EndIndex)
                {
                    StUtil.WriteLine(StVerbosityLevel.Verbose, String.Format(
                        "  incrSuffix: Active suffix changed from {0:s} to {1:s}",
                        StSuffix.ToSuffixString(origNodeId, begin, end),
                        StSuffix.ToSuffixString(active.OriginNode.Id, active.BeginIndex, active.EndIndex)));
                }
            }

            private void setSuffixLink(StNode node, StNode suffixNode)
            {
                if ((node != null) && (node != root))
                {
                    if (node.SuffixNode == null)
                    {
                        StUtil.WriteLine(StVerbosityLevel.Verbose, String.Format(
                            "  New suffix link from N{0:d} to N{1:d}",
                            node.Id, suffixNode.Id));
                    } else {
                        if (node.SuffixNode.Id == suffixNode.Id) {
                            StUtil.WriteLine(StVerbosityLevel.Verbose, String.Format(
                                "  Suffix link (N{0:d} to N{1:d}) retaining same value",
                                node.Id, node.SuffixNode.Id));
                        } else {
                            StUtil.WriteLine(StVerbosityLevel.Verbose, String.Format(
                                "  Suffix link (N{0:d} to N{1:d}) set to new value (N{0:d} to N{2:d})",
                                node.Id, node.SuffixNode.Id, suffixNode.Id));
                        }
                    }
                    node.SuffixNode = suffixNode;
                }
            }
            #endregion // Private methods

            #region Private fields
            private string text;
            private StNode root;
            #endregion // Private fields
        }
    }
}