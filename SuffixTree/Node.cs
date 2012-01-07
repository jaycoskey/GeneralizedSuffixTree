// Jay Coskey, January 2012.  Seattle, WA, USA.

using System.Collections.Generic;

namespace TextAlgorithms
{
    namespace SuffixTree
    {
        public class Node
        {
            #region Lifecycle
            public Node(SuffixTree tree, Node suffixNode)
            {
                this.tree = tree;
                this.id = tree.NodeCount++;
                this.childEdges = new Dictionary<char, Edge>();
                this.suffixNode = suffixNode;
            }
            #endregion // Lifecycle

            #region Public fields / properties / methods
            public void AddChildEdge(char c, Edge edge)
            {
                childEdges.Add(c, edge);
            }

            public IEnumerable<Edge> ChildEdges()
            {
                return childEdges.Values;
            }

            public Edge GetChildEdge(char c)
            {
                Edge childEdge = null;
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

            public Node SuffixNode
            {
                get { return suffixNode; }
                set { suffixNode = value; }
            }

            public SuffixTree Tree { get { return tree; } }
            #endregion // Public fields / properties / methods

            #region Private
            private Dictionary<char, Edge> childEdges;
            private int id;
            private Node suffixNode;
            private SuffixTree tree;
            #endregion // Private
        }
    }
}