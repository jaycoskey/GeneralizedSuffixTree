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
            public Edge Edge;
            public int Depth;
            public DepthTaggedEdge(Edge e, int d)
            {
                Edge = e;
                Depth = d;
            }
        }

        public class Edge
        {
            #region Lifecycle
            public Edge(
                SuffixTree tree,
                Node parentNode,
                int indexOfFirstChar,
                int indexOfLastChar)
            {
                this.id = Edge.nextId++;
                this.tree = tree;
                this.ParentNode = parentNode;
                this.ChildNode = new Node(tree, null);
                this.BeginIndex = indexOfFirstChar;
                this.EndIndex = indexOfLastChar;
            }
            #endregion // Lifecycle

            #region Public properties / methods
            public void Add()
            {
                Util.WriteLine(VerbosityLevel.Verbose, String.Format(
                    "    Adding edge to (node, char) = ({0:d}, '{1:c}').  New edge id=#{2:d}",
                    ParentNode.Id, tree.Text[BeginIndex], this.Id));
                ParentNode.AddChildEdge(tree.Text[BeginIndex], this);
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

            public void Remove()
            {
                this.ParentNode.RemoveChildEdge(tree.Text[BeginIndex]);
            }

            public int Span { get { return (this.EndIndex - this.BeginIndex); } }

            public Node Split(Suffix s)
            {
                Util.WriteLine(VerbosityLevel.Normal, String.Format(
                    "  Splitting edge={0:s}", this.ToString(tree.Text)));
                Util.WriteLine(VerbosityLevel.Normal, String.Format(
                    "  ... at {0:s}", s.ToString(tree.Text)));
                Remove();
                Edge newEdge = new Edge(tree, s.OriginNode, BeginIndex, BeginIndex + s.Span);
                newEdge.Add();
                Util.WriteLine(VerbosityLevel.Normal, String.Format(
                    "  ... into new edge={0:s}",
                    newEdge.ToString(tree.Text)));
                newEdge.ChildNode.SuffixNode = s.OriginNode;
                BeginIndex += s.Span + 1;
                ParentNode = newEdge.ChildNode;
                Add();
                Util.WriteLine(VerbosityLevel.Normal, String.Format(
                    "  ... and modified edge={0:s}",
                    this.ToString(tree.Text)));
                return newEdge.ChildNode;
            }

            public string ToString(string text, int maxLen = int.MaxValue)
            {
                string result = String.Format(
                    "Edge#{0:d}: (Begin, End) nodes = ({1:d}, {2:d}); "
                        + "T[{3:d}:{4:d}]"
                        + "=\"{5:s}\"",
                    this.Id,
                    this.ParentNode.Id, this.ChildNode.Id,
                    this.BeginIndex, this.EndIndex,
                    text.Substring(this.BeginIndex, Math.Min(maxLen, this.Span + 1)));
                return result;
            }
            #endregion // Public properties / methods

            #region Public fields
            public int BeginIndex;
            public int EndIndex;
            public Node ChildNode;
            public Node ParentNode;
            #endregion // Public fields

            #region Private
            private int id;     // Pedagogical aide: Not needed for creation of the tree. 
            private static int nextId = 0;
            private SuffixTree tree;
            #endregion // Private
        }
    }
}