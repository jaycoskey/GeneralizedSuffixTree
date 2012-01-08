// Jay Coskey, January 2012.  Seattle, WA, USA.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;
using System.Reflection;

namespace TextAlgorithms
{
    namespace SuffixTree
    {
        /// <summary>
        ///     Formerly passed to Util.Write() and Util.WriteLine().  Currently unused.
        /// </summary>
        [Flags]
        public enum TraceFlags
        {
            None = 0,
            Edge = 1,
            Node = 2,
            Suffix = 4,
            Tree = 8,
            Verbose = 16
        }

        public class Util
        {
            #region Public static properties / methods
            public static void Entering()
            {
                Console.WriteLine(Indentation + String.Format("Entering {0:s}....", Util.GetCurrentMethodName(2)));
                TraceDepth++;
            }

            public static void Exiting()
            {
                TraceDepth--;
                Console.WriteLine(Indentation + String.Format("Exiting {0:s}....", Util.GetCurrentMethodName(2)));
            }

            public static string GetCurrentMethodName(int stackDepth = 1)
            {
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(stackDepth);
                MethodBase mb = sf.GetMethod();
                return mb.Name;
            }

            public static string Indentation
            {
                get { return new String(' ', 2 * TraceDepth); }
            }

            public static void Write(VerbosityLevel v, string message)
            {
                if (SuffixTree.Verbosity >= v)
                {
                    Debug.Write(Indentation + message);
                }
            }

            public static void WriteLine(VerbosityLevel v, string message)
            {
                Util.Write(v, message + Environment.NewLine);
            }
            #endregion Public static properties / methods

            #region Public fields
            public static int TraceDepth = 0;
            #endregion // Public fields
        }

        public enum VerbosityLevel
        {
            Quiet, Normal, Verbose
        }
    }
}