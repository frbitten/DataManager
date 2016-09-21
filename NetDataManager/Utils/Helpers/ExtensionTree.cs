using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils.Helpers
{
    /// <summary>
    /// Extensions of trees
    /// </summary>
    public static class ExtensionTree
    {
        /// <summary>
        /// In depth-first traversal, the algorithm will dig continue to dig down a nodes children until it reaches a leaf node (a node without children), before considering the next child of the current parent node.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="head"></param>
        /// <param name="childrenFunc"></param>
        /// <returns></returns>
        public static IEnumerable<T> AsDepthFirstEnumerable<T>(this T head, Func<T, IEnumerable<T>> childrenFunc) where T : class
        {
            yield return head;
            foreach (var node in childrenFunc(head))
            {
                foreach (var child in AsDepthFirstEnumerable(node, childrenFunc))
                {
                    yield return child;
                }
            }
        } 

        /// <summary>
        /// In breadth-first traversal, the algorithm will return all nodes at a particular depth first before considering the children at the next level. I.e. First return all the nodes from level 1, then all nodes from level 2, etc.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="head"></param>
        /// <param name="childrenFunc"></param>
        /// <returns></returns>
        public static IEnumerable<T> AsBreadthFirstEnumerable<T>(this T head, Func<T, IEnumerable<T>> childrenFunc) where T : class
        {
            yield return head;
            var last = head;
            foreach (var node in AsBreadthFirstEnumerable(head, childrenFunc))
            {
                foreach (var child in childrenFunc(node))
                {
                    yield return child;
                    last = child;
                }
                if (last.Equals(node)) yield break;
            }
        } 

    }
}
