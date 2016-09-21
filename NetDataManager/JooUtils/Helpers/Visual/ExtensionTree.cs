using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Joo.Utils.Helpers.Visual
{
    /// <summary>
    /// Extensions of visual trees
    /// </summary>
    public static class VisualControls
    {

        /// <summary>
        /// Find visual children in tree inside the element, method uses VisualTreeHelper.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="depObj"></param>
        /// <returns></returns>
        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = (DependencyObject)VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        /// <summary>
        /// FirstOrDefault visual child in tree inside the element, method uses VisualTreeHelper.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="depObj"></param>
        /// <returns></returns>
        public static T FindVisualChild<T>(this DependencyObject obj) where T : DependencyObject
        {
            if (obj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(obj, i);

                    if (child != null && child is T)
                        return (T)child;

                    else
                    {
                        T childOfChild = FindVisualChild<T>(child);

                        if (childOfChild != null)
                            return childOfChild;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Find visual parent in tree above the element, method uses VisualTreeHelper.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="StopAt"></param>
        /// <returns></returns>
        public static T FindParent<T>(this DependencyObject item, Type StopAt) where T : DependencyObject
        {
            if (item is T)
            {
                return item as T;
            }
            else
            {
                DependencyObject _parent = VisualTreeHelper.GetParent(item);
                if (_parent == null)
                {
                    return default(T);
                }
                else
                {
                    Type _type = _parent.GetType();
                    if (StopAt != null)
                    {
                        if ((_type.IsSubclassOf(StopAt) == true) || (_type == StopAt))
                        {
                            return null;
                        }
                    }

                    if ((_type.IsSubclassOf(typeof(T)) == true) || (_type == typeof(T)))
                    {
                        return _parent as T;
                    }
                    else
                    {
                        return FindParent<T>(_parent, StopAt);
                    }
                }
            }
        }
    }
}
