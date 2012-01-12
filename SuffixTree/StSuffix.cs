// Jay Coskey, January 2012.  Seattle, WA, USA.

using System;
using System.Collections.Generic;
using System.Text;

using System.Diagnostics;

namespace TextAlgorithms
{
    namespace SuffixTree
    {
        public class StSuffix
        {
            #region Lifecycle
            public StSuffix(
                SuffixTree tree,
                StNode originNode,
                int beginIndex = 0,
                int endIndex = int.MaxValue)
            {
                this.tree = tree;
                this.OriginNode = originNode;
                this.beginIndex = beginIndex;
                this.endIndex = endIndex;
            }
            #endregion // Lifecycle

            #region Public static methods
            public static string ToSuffixString(int nodeId, int begin, int end, string word = "")
            {
                bool isExplicit = (end < begin);
                return String.Format("(N{0:d}, ({1:s}-[{2:d}:{3:d}]{4:s})",
                    nodeId,
                    isExplicit ? "Exp" : "Imp",
                    begin,
                    end == SuffixTree.InfiniteIndex ? "Inf" : end.ToString(),
                    (isExplicit || word == String.Empty) ? String.Empty : "=\"" + word.Substring(begin, end - begin + 1) + "\""
                    );
            }
            #endregion // Public static methods

            #region Public properties / methods
            // In canonical form, the OriginNodeId is the closest node to the end of the tree.
            public int BeginIndex
            {
                get { return beginIndex; }
                set { beginIndex = value; }
            }

            public void Canonicalize()
            {
                if (IsImplicit)
                {
                    bool haveValuesChanged = false;
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("  Canonicalize: Entering");
                    // sb.AppendLine(tree.ToString());

                    int origNodeId, begin, end;
                    origNodeId = this.OriginNode.Id;
                    begin = this.beginIndex;
                    end = this.endIndex;

                    StEdge edge = OriginNode.GetChildEdge(tree.Text[BeginIndex]);
                    while (edge.Span <= Span)
                    {
                        sb.Append(String.Format(
                            "    Canonicalize: Active suffix changed from {0:s}",
                            ToSuffixString(origNodeId, begin, end)));
                        this.beginIndex += edge.Span + 1;
                        this.OriginNode = edge.ChildNode;
                        haveValuesChanged = true;
                        sb.AppendLine(String.Format(" to {0:s}",
                                ToSuffixString(OriginNode.Id, beginIndex, endIndex)));
                        if (Span >= 0)
                        {
                            edge = edge.ChildNode.GetChildEdge(tree.Text[BeginIndex]);
                        }
                    }
                    sb.AppendLine("  Canonicalize: Exiting");
                    if (haveValuesChanged)
                    {
                        StUtil.Write(StVerbosityLevel.Verbose, sb.ToString());
                    }
                }
            }

            public int EndIndex
            {
                get { return endIndex; }
                set { endIndex = value; }
            }

            public bool IsFinite
            {
                get { return endIndex != SuffixTree.InfiniteIndex; }
            }

            public bool IsExplicit {
                get { return (Span < 0); }
            }

            public bool IsImplicit
            {
                get { return (Span >= 0); }
            }

            public StNode OriginNode
            {
                get;
                set;
            }

            public int Span
            {
                get { return this.EndIndex - this.BeginIndex; }
            }

            public override string ToString()
            {
                string result = ToSuffixString(OriginNode.Id, beginIndex, endIndex, tree.Text);
                return result;
            }
            #endregion // Public properties / methods

            #region Private
            private int beginIndex;
            private int endIndex;
            private SuffixTree tree;
            #endregion // Private
        }
    }
}