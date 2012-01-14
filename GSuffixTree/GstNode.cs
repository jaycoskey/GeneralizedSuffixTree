// Jay Coskey, January 2012.  Seattle, WA, USA.

using System.Collections.Generic;

using System.Diagnostics;

namespace TextAlgorithms
{
    namespace SuffixTree
    {
        public class GstNode
        {
            #region Lifecycle
            public GstNode(GSuffixTree tree, GstNode suffixNode)
            {
                this.tree = tree;
                this.id = tree.NodeCount++;
                this.childEdges = new Dictionary<char, GstEdge>();
                this.suffixNode = suffixNode;
            }
            #endregion // Lifecycle

            #region Public properties / methods
            public void AddChildEdge(char c, GstEdge edge)
            {
                childEdges.Add(c, edge);
            }

            public IEnumerable<char> ChildChars()
            {
                return childEdges.Keys;
            }

            public IEnumerable<GstEdge> ChildEdges()
            {
                return childEdges.Values;
            }

            public GstEdge GetChildEdge(char c)
            {
                GstEdge childEdge = null;
                childEdges.TryGetValue(c, out childEdge);
                return childEdge;
            }

            public bool HasChildEdges()
            {
                return childEdges.Count > 0;
            }

            public int Id { get { return id; } }

            public bool IsRoot()
            {
                return this == tree.Root;
            }

            public void RemoveChildEdge(char c) {
                childEdges.Remove(c);
            }

            public GstNode SuffixNode
            {
                get { return suffixNode; }
                set { suffixNode = value; }
            }

            public GSuffixTree Tree {
                get { return tree; }
            }
            #endregion // Public properties / methods

            #region Private
            private Dictionary<char, GstEdge> childEdges;
            private int id;
            private GstNode suffixNode;
            private GSuffixTree tree;
            #endregion // Private
        }
    }
}