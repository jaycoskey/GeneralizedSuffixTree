// Jay Coskey, January 2011.  Seattle, WA, USA.

using System;
using System.Collections.Generic;

namespace TextAlgorithms
{
    namespace SuffixTree
    {
        public class Suffix
        {
            #region Lifecycle
            public Suffix(
                SuffixTree tree,
                Node originNode,
                int beginIndex = 0,
                int endIndex = int.MaxValue)
            {
                this.tree = tree;
                this.OriginNode = originNode;
                this.beginIndex = beginIndex;
                this.endIndex = endIndex;
            }
            #endregion // Lifecycle

            #region Public fields / properties / methods
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
                    Util.WriteLine(VerbosityLevel.Verbose, String.Format(
                        "    Canonicalize: Suffix is implicit... [{0:s}]",
                        this.ToString(tree.Text)
                        ));
                    Edge edge = OriginNode.GetChildEdge(tree.Text[BeginIndex]);
                    Util.WriteLine(VerbosityLevel.Verbose, String.Format(
                        "      Canonicalize(): Original edge = {0:s}",
                        edge.ToString(tree.Text)));
                    while (edge.Span <= Span)
                    {
                        this.beginIndex += edge.Span + 1;
                        this.OriginNode = edge.ChildNode;
                        if (Span >= 0)
                        {
                            edge = edge.ChildNode.GetChildEdge(tree.Text[this.BeginIndex]);
                            Util.WriteLine(VerbosityLevel.Verbose, String.Format(
                                "      Canonicalize(): Updated edge = {0:s}",
                                edge.ToString(tree.Text)));
                        }
                    }
                    Util.WriteLine(VerbosityLevel.Verbose, String.Format(
                        "    Canonicalize: Final suffix = {0:s}",
                        this.ToString(tree.Text)));
                }
                else
                {
                    Util.WriteLine(VerbosityLevel.Verbose, String.Format(
                        "    Suffix is explicit: Nothing to do. [{0:s}]",
                        this.ToString(tree.Text)
                        ));
                }
            }

            public int EndIndex
            {
                get { return endIndex; }
                set { endIndex = value; }
            }

            public bool IsFinite
            {
                get { return endIndex > -1; }
            }

            public bool IsExplicit {
                get { return (Span < 0); }
            }

            public bool IsImplicit
            {
                get { return (Span >= 0); }
            }

            public Node OriginNode
            {
                get;
                set;
            }

            public int Span
            {
                get { return this.EndIndex - this.BeginIndex; }
            }

            public string ToString(string text, int maxLength = int.MaxValue)
            {
                string result = String.Format(
                    "Suffix is {0:s}, OriginNode#={1:d}, T[{2:d}:{3:s}]=\"{4:s}\"",
                    IsExplicit ? "explicit" : "implicit",
                    OriginNode.Id,
                    BeginIndex,
                    IsFinite ? this.EndIndex.ToString() : "Inf",
                    (IsFinite && BeginIndex > EndIndex)
                        ? String.Empty
                        : text.Substring(BeginIndex, Math.Min(maxLength,
                            IsFinite ? (EndIndex - BeginIndex + 1) : (tree.Text.Length - BeginIndex)
                            ))
                    );
                return result;
            }
            #endregion // Public fields / properties / methods

            #region Private
            private int beginIndex;
            private int endIndex;
            private SuffixTree tree;
            #endregion // Private
        }
    }
}