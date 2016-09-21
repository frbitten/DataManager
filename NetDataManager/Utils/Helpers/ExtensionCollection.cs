using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Linq.Expressions;

namespace Utils.Helpers
{
    /// <summary>
    /// Extensions of collections
    /// </summary>
    public static class ExtensionCollection
    {
        /// <summary>
        /// Executes the given action against the given ICollection instance.
        /// </summary>
        /// <typeparam name="T">The type of the ICollection parameter.</typeparam>
        /// <param name="items">The collection the action is performed against.</param>
        /// <param name="action">The action that is performed on each item.</param>
        public static void Each<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (T item in items)
            {
                action(item);
            }
        }

        /// <summary>
        /// Determines whether a parameter is in a given list of parameters.
        /// E.g.. 11.In(1,2,3) will return false.
        /// </summary>
        /// <typeparam name="T">The type of the source parameter.</typeparam>
        /// <param name="source">The item that needs to be checked.</param>
        /// <param name="list">The list that will be checked for the given source.</param>
        public static bool In<T>(this T source, params T[] list)
        {
            if (null == source) throw new ArgumentNullException("source");
            return list.Contains(source);
        }


        public static void AddMany<T>(this List<T> source, params T[] list)
        {
            foreach (var item in list)
            {
                source.Add(item);
            }


        }

        public static IEnumerable<T> Remove<T>(this T[] source, Func<T, bool> lambda)
        {
            return source.Where(obj => !lambda(obj));
        }

        public static T[] RemoveSingleOrDefault<T>(this T[] source, Func<T, bool> lambda)
        {
            for (int i = 0; i < source.Length; i++)
            {
                if (lambda(source[i]))
                {
                    T[] dest = new T[source.Length - 1];
                    if (i > 0)
                        Array.Copy(source, 0, dest, 0, i);

                    if (i < source.Length - 1)
                        Array.Copy(source, i + 1, dest, i, source.Length - i - 1);

                    return dest;
                }
            }
            return source;
        }
        /// <summary>
        /// Determines whether the specified collection has any elements in the sequence.
        /// This method also checks for a null collection.
        /// </summary>
        /// <param name="items">The ICollection of items to check.</param>
        public static bool HasElements(this ICollection items)
        {
            return items != null && items.Count > 0;
        }

        /// <summary>
        /// Retorna uma parte do array passado de acordo com os index e tamanho
        /// </summary>
        /// <typeparam name="T"> tipo do array</typeparam>
        /// <param name="data"> array principal</param>
        /// <param name="index"> indice inicial para a copia</param>
        /// <param name="length">quantidade dos dados copiados</param>
        /// <returns></returns>
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> target, T element)
        {
            foreach (T e in target) 
                yield return e;
            yield return element;
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> target, params IEnumerable<T>[] list)
        {
            foreach (var item in target)
            {
                yield return item;
            }

            foreach (var item in list)
            {
                if (item == null)
                    continue;

                foreach (var e in item)
                {
                    yield return e;
                }
            }
        }
        /// <summary>
        /// Returns all items in the first collection except the ones in the second collection that match the lambda condition (same types)
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        /// <param name="listA">The first list</param>
        /// <param name="listB">The second list</param>
        /// <param name="lambda">The filter expression</param>
        /// <returns>The filtered list</returns>
        public static IEnumerable<T> Except<T>(this IEnumerable<T> listA, IEnumerable<T> listB, Func<T, T, bool> lambda)
        {
            return listA.Except(listB, new LambdaComparer<T>(lambda));
        }

        /// <summary>
        /// Returns all items in the first collection that intersect the ones in the second collection that match the lambda condition (same types)
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        /// <param name="listA">The first list</param>
        /// <param name="listB">The second list</param>
        /// <param name="lambda">The filter expression</param>
        /// <returns>The filtered list</returns>
        public static IEnumerable<T> Intersect<T>(this IEnumerable<T> listA, IEnumerable<T> listB, Func<T, T, bool> lambda)
        {
            return listA.Intersect(listB, new LambdaComparer<T>(lambda));
        }

        /// <summary>
        /// Returns all items in the first collection that match the lambda condition (suport different types)
        /// </summary>
        /// <typeparam name="T1">The type</typeparam>
        /// <typeparam name="T2">The second type</typeparam>
        /// <param name="listA">The first list</param>
        /// <param name="listB">The second list</param>
        /// <param name="comparer">The filter expression</param>
        /// <returns>The filtered list</returns>
        public static IEnumerable<T1> MultiCompare<T1, T2>(this IEnumerable<T1> listA, IEnumerable<T2> listB, Func<T1, T2, bool> comparer)
        {
            return listB.SelectMany(t1 => listA.Where(t2 => comparer(t2, t1)));
        }

    }

    public class LambdaComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _lambdaComparer;
        private readonly Func<T, int> _lambdaHash;

        public LambdaComparer(Func<T, T, bool> lambdaComparer)
            : this(lambdaComparer, o => 0)
        {
        }

        public LambdaComparer(Func<T, T, bool> lambdaComparer, Func<T, int> lambdaHash)
        {
            if (lambdaComparer == null)
                throw new ArgumentNullException("lambdaComparer");
            if (lambdaHash == null)
                throw new ArgumentNullException("lambdaHash");

            _lambdaComparer = lambdaComparer;
            _lambdaHash = lambdaHash;
        }

        public bool Equals(T x, T y)
        {
            return _lambdaComparer(x, y);
        }

        public int GetHashCode(T obj)
        {
            return _lambdaHash(obj);
        }
    }

    public class LambdaIComparer<T, TKey> : IComparer<T>, System.Collections.Generic.IEqualityComparer<T>
    {
        private readonly Expression<Func<T, TKey>> _KeyExpr;
        private readonly Func<T, TKey> _CompiledFunc;
        private readonly TKey item;

        // Constructor
        public LambdaIComparer(Expression<Func<T, TKey>> getKey)
        {
            _KeyExpr = getKey;
            _CompiledFunc = _KeyExpr.Compile();
       
        }

        public int Compare(T obj1, T obj2)
        {
            return Comparer<TKey>.Default.Compare(_CompiledFunc(obj1), _CompiledFunc(obj2));
        }

        public bool Equals(T obj1, T obj2)
        {
            return EqualityComparer<TKey>.Default.Equals(_CompiledFunc(obj1), _CompiledFunc(obj2));
        }

        public int GetHashCode(T obj)
        {
            return EqualityComparer<TKey>.Default.GetHashCode(_CompiledFunc(obj));
        }
    }

    #region [ Garbage ]
    ///// <summary>
    ///// Returns all items in the second collection that match the lambda condition (suport different types)
    ///// </summary>
    ///// <typeparam name="T1">The type</typeparam>
    ///// <typeparam name="T2">The second type</typeparam>
    ///// <param name="listA">The first list</param>
    ///// <param name="listB">The second list</param>
    ///// <param name="comparer">The filter expression</param>
    ///// <returns>The filtered list</returns>
    //public static IEnumerable<T2> MultiCompareT1<T1, T2>(this IEnumerable<T1> listA, IEnumerable<T2> listB, Func<T1, T2, bool> comparer)
    //{
    //    return listA.SelectMany(t1 => listB.Where(t2 => comparer(t1, t2)));
    //}
    #endregion

}
