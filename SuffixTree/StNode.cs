// Jay Coskey, January 2012.  Seattle, WA, USA.

using System.Collections.Generic;

namespace TextAlgorithms
{
    namespace SuffixTree
    {
        public class StNode
        {
            #region Lifecycle
            public StNode(SuffixTree tree, StNode suffixNode)
            {
                this.tree = tree;
                this.id = tree.NodeCount++;
                this.childEdges = new Dictionary<char, StEdge>();
                this.suffixNode = suffixNode;
            }
            #endregion // Lifecycle

            #region Public fields / properties / methods
            public void AddChildEdge(char c, StEdge edge)
            {
                childEdges.Add(c, edge);
            }

            public IEnumerable<StEdge> ChildEdges()
            {
                return childEdges.Values;
            }

            public StEdge GetChildEdge(char c)
            {
                StEdge childEdge = null;
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

            public StNode SuffixNode
            {
                get { return suffixNode; }
                set { suffixNode = value; }
            }

            public SuffixTree Tree { get { return tree; } }
            #endregion // Public fields / properties / methods

            #region Private
            private Dictionary<char, StEdge> childEdges;
            private int id;
            private StNode suffixNode;
            private SuffixTree tree;
            #endregion // Private
        }
    }
}