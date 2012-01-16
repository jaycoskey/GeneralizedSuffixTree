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
        public class DepthTaggedGstEdge
        {
            public GstEdge Edge;
            public int Depth;
            public DepthTaggedGstEdge(GstEdge e, int d)
            {
                Edge = e;
                Depth = d;
            }
        }

        public class GstEdge
        {
            #region Lifecycle
            public GstEdge(
                GSuffixTree tree,
                GstNode parentNode)
            {
                this.id = tree.EdgeCount++;
                this.tree = tree;
                this.ParentNode = parentNode;
                this.ChildNode = new GstNode(tree, null);
                this.beginIndexes = new Dictionary<int, int>();
                this.endIndexes = new Dictionary<int, int>();
            }

            public GstEdge(
                GSuffixTree tree,
                GstNode parentNode,
                int wordNum,
                int beginIndex,
                int endIndex)
                : this(tree, parentNode)
            {
                this.beginIndexes[wordNum] = beginIndex;
                this.endIndexes[wordNum] = endIndex;
            }
            #endregion // Lifecycle

            #region Public fields
            public GstNode ChildNode;
            public GstNode ParentNode;
            #endregion // Public fields

            #region Public properties / methods
            public void Add()
            {
                char firstChar = GetFirstChar();
                ParentNode.AddChildEdge(firstChar, this);
            }

            public int GetBeginIndex(int wordNum)
            {
                return beginIndexes[wordNum];
            }

            public int GetEndIndex(int wordNum)
            {
                return endIndexes[wordNum];
            }

            public int GetExtantWordNum() {
                return beginIndexes.Keys.First();
            }

            public char GetFirstChar()
            {
                int extantWordNum = GetExtantWordNum();
                char firstChar = GetWordChar(extantWordNum, GetBeginIndex(extantWordNum));
                return firstChar;
            }

            public string GetText(int wordNum = -1)
            {
                if (wordNum == -1) { wordNum = GetExtantWordNum(); }
                int beginIndex = GetBeginIndex(wordNum);
                int endIndex = RealEndIndex(wordNum);
                string result = tree.GetWord(wordNum).Substring(beginIndex, endIndex - beginIndex + 1);
                return result;
            }

            public char GetWordChar(int wordNum, int i)
            {
                return tree.GetWordChar(wordNum, i);
            }

            public bool HasWordNum(int wordNum)
            {
                return beginIndexes.ContainsKey(wordNum);
            }

            public int Id
            {
                get { return id; }
            }

            public int IncBeginIndex(int wordNum, int delta)
            {
                return beginIndexes[wordNum] += delta;
            }

            public bool IsSet()
            {
                Debug.Assert(ParentNode != null, "ParentNode is unset.");
                bool result = ParentNode != null;
                return result;
            }

            public void MoveFromTo(GstNode oldParentNode, char oldFirstChar, GstNode newParentNode, char newFirstChar)
            {
                GstEdge self = oldParentNode.GetChildEdge(oldFirstChar);
                if (self != this) {
                    throw new ArgumentException("Error: MoveTo called with incorrect parent node and/or first char arguments");
                }
                oldParentNode.RemoveChildEdge(oldFirstChar);
                newParentNode.AddChildEdge(newFirstChar, this);
            }

            public int RealEndIndex(int wordNum)
            {
                int endIndex = GetEndIndex(wordNum);
                if (endIndex == GSuffixTree.InfiniteIndex)
                {
                    endIndex = tree.GetWord(wordNum).Length - 1;
                }
                return endIndex;
            }

            public void Remove()
            {
                char firstChar = GetFirstChar();
                this.ParentNode.RemoveChildEdge(firstChar);
            }

            public void SetBeginIndex(int wordNum, int val)
            {
                beginIndexes[wordNum] = val;
            }

            public void SetEndIndex(int wordNum, int val)
            {
                endIndexes[wordNum] = val;
            }

            public int Span(int wordNum = -1) {
                if (wordNum == -1) { wordNum = GetExtantWordNum(); }
                return (this.GetEndIndex(wordNum) - this.GetBeginIndex(wordNum));
            }

            public GstNode Split(GstSuffix s)
            {
                // Create new edge
                int wordNum = s.WordNum;
                GstEdge newEdge;
                if (this.HasWordNum(s.WordNum))
                {
                     newEdge = new GstEdge(tree, s.OriginNode, wordNum, GetBeginIndex(wordNum), GetBeginIndex(wordNum) + s.Span);
                } else
                {
                    newEdge = new GstEdge(tree, s.OriginNode, wordNum, s.BeginIndex, s.EndIndex);
                }
                foreach (int n in beginIndexes.Keys) { newEdge.SetBeginIndex(n, beginIndexes[n]); }
                foreach (int n in endIndexes.Keys) { newEdge.SetEndIndex(n, beginIndexes[n] + s.Span); }
                newEdge.ChildNode.SuffixNode = s.OriginNode;

                char oldFirstChar = GetFirstChar();
                // Modify old edge
                int [] wordNums = beginIndexes.Keys.ToArray();
                foreach (int n in wordNums) { IncBeginIndex(n, s.Span + 1); }

                // Perform switch
                MoveFromTo(ParentNode, oldFirstChar, newEdge.ChildNode, GetFirstChar());
                ParentNode = newEdge.ChildNode;
                newEdge.Add();
                GstUtil.WriteLine(GstVerbosityLevel.Normal, String.Format(
                    "  Split E{0:d} into E{1:d} + E{0:d} = \"{2:s}\" + \"{3:s}\"",
                    Id, newEdge.Id,
                    newEdge.GetText(),
                    this.GetText()
                    ));
                
                return newEdge.ChildNode;
            }

            public string ToString(int wordNum)
            {
                StringBuilder sb = new StringBuilder(String.Format("E{0:d} = (N{1:d}, N{2:d})", Id, ParentNode.Id, ChildNode.Id));
                if (wordNum >= 0) {
                    sb.Append(String.Format(" = T[{0:d}:{1:d}]",
                        GetBeginIndex(wordNum), RealEndIndex(wordNum)));
                    if (Span() >= 0)
                    {
                        sb.Append(" = \"" + tree.GetWord(wordNum).Substring(GetBeginIndex(wordNum), Span() + 1) + "\"");
                    }
                }
                return sb.ToString();
            }

            public IEnumerable<int> WordNums()
            {
                return beginIndexes.Keys;
            }
            #endregion // Public properties / methods

            #region Private
            private Dictionary<int, int> beginIndexes;
            private Dictionary<int, int> endIndexes;
            private int id;     // Pedagogical aide: Not needed for creation of the tree.
            private GSuffixTree tree;
            #endregion // Private
        }
    }
}