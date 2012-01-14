// Jay Coskey, January 2012.  Seattle, WA, USA.

using System;
using System.Collections.Generic;
using System.Text;

namespace TextAlgorithms
{
    namespace SuffixTree
    {
        public class GstSuffix
        {
            #region Lifecycle
            public GstSuffix(
                GSuffixTree tree,
                GstNode originNode,
                int wordNum,
                int beginIndex,
                int endIndex)
            {
                this.tree = tree;
                this.OriginNode = originNode;
                this.WordNum = wordNum;
                this.beginIndex = beginIndex;
                this.endIndex = endIndex;
            }
            #endregion // Lifecycle

            #region Public static methods
            public static string ToSuffixString(int nodeId, int begin, int end, string word = "")
            {
                bool isExplicit = (end < begin);
                return String.Format("(N{0:d}, {1:s}-[{2:d}:{3:s}]{4:s})",
                    nodeId,
                    isExplicit ? "Exp" : "Imp",
                    begin,
                    end == GSuffixTree.InfiniteIndex ? "Inf" : end.ToString(),
                    (isExplicit || word == String.Empty) ? String.Empty : " = \"" + word.Substring(begin, end - begin + 1) + "\""
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

            /// <remarks>
            ///     Constraint: Implicit suffixes must have BeginIndex < words[wordNum].Length
            /// </remarks>
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

                    GstEdge edge = OriginNode.GetChildEdge(tree.GetWordChar(WordNum, BeginIndex));
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
                            edge = edge.ChildNode.GetChildEdge(tree.GetWordChar(0, BeginIndex));
                        }
                    }
                    sb.AppendLine("  Canonicalize: Exiting");
                    if (haveValuesChanged)
                    {
                        GstUtil.Write(GstVerbosityLevel.Verbose, sb.ToString());
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
                get { return endIndex != GSuffixTree.InfiniteIndex; }
            }

            public bool IsExplicit {
                get { return (Span < 0); }
            }

            public bool IsImplicit
            {
                get { return (Span >= 0); }
            }

            public GstNode OriginNode
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
                string result = ToSuffixString(OriginNode.Id, beginIndex, endIndex, tree.GetWord(WordNum));
                return result;
            }

            public int WordNum
            {
                get;
                set;
            }
            #endregion // Public properties / methods

            #region Private fields
            private int beginIndex;
            private int endIndex;
            private GSuffixTree tree;
            #endregion // Private fields
        }
    }
}