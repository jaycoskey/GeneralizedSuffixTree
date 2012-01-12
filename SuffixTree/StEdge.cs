// Jay Coskey, January 2012.  Seattle, WA, USA.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;

namespace TextAlgorithms
{
    namespace SuffixTree
    {
        public class DepthTaggedEdge
        {
            public StEdge Edge;
            public int Depth;
            public DepthTaggedEdge(StEdge e, int d)
            {
                Edge = e;
                Depth = d;
            }
        }

        public class StEdge
        {
            #region Lifecycle
            public StEdge(
                SuffixTree tree,
                StNode parentNode,
                int indexOfFirstChar,
                int indexOfLastChar)
            {
                this.id = StEdge.nextId++;
                this.tree = tree;
                this.ParentNode = parentNode;
                this.ChildNode = new StNode(tree, null);
                this.BeginIndex = indexOfFirstChar;
                this.EndIndex = indexOfLastChar;
            }
            #endregion // Lifecycle

            #region Public fields
            public int BeginIndex;
            public int EndIndex;
            public StNode ChildNode;
            public StNode ParentNode;
            #endregion // Public fields

            #region Public properties / methods
            public void Add()
            {
                ParentNode.AddChildEdge(tree.Text[BeginIndex], this);
            }

            public string GetText()
            {
                int endIndex = RealEndIndex();
                string result = tree.Text.Substring(BeginIndex, endIndex - BeginIndex + 1);
                return result;
            }

            public int Id
            {
                get { return id; }
            }

            public bool IsSet()
            {
                Debug.Assert(ParentNode != null, "ParentNode is unset.");
                bool result = ParentNode != null;
                return result;
            }

            public int RealEndIndex()
            {
                int result = (EndIndex == SuffixTree.InfiniteIndex)
                    ? tree.Text.Length - 1
                    : EndIndex;
                return result;
            }

            public void Remove()
            {
                this.ParentNode.RemoveChildEdge(tree.Text[BeginIndex]);
            }

            public int Span { get { return (this.EndIndex - this.BeginIndex); } }

            public StNode Split(StSuffix s)
            {
                Remove();
                StEdge newEdge = new StEdge(tree, s.OriginNode, BeginIndex, BeginIndex + s.Span);
                newEdge.Add();
                newEdge.ChildNode.SuffixNode = s.OriginNode;
                BeginIndex += s.Span + 1;
                ParentNode = newEdge.ChildNode;
                Add();
                StUtil.WriteLine(StVerbosityLevel.Normal, String.Format(
                    "  Split E{0:d} into E{1:d} + E{0:d} = \"{2:s}\" + \"{3:s}\"",
                    Id, newEdge.Id,
                    newEdge.GetText(),
                    this.GetText()
                    ));
                return newEdge.ChildNode;
            }

            public string ToString(string text = "")
            {
                string result = String.Format(
                    "Edge{0:d} = (N{1:d}, N{2:d}) = "
                        + "T[{3:d}:{4:s}]"
                        + "{5:s}",
                    this.Id,
                    this.ParentNode.Id,
                    this.ChildNode.Id,
                    this.BeginIndex,
                    (this.EndIndex == SuffixTree.InfiniteIndex) ? "Inf" : this.EndIndex.ToString(),
                    (text == null || text == String.Empty)
                    ? String.Empty
                    : text.Substring(this.BeginIndex, this.Span + 1));
                return result;
            }
            #endregion // Public properties / methods

            #region Private fields
            private int id;     // Pedagogical aide: Not needed for creation of the tree. 
            private static int nextId = 0;
            private SuffixTree tree;
            #endregion // Private fields
        }
    }
}