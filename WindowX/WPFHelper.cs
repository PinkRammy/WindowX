using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace WindowX
{
    /// <summary>
    /// Contains methods to aid in the development of WPF applications.
    /// </summary>
    public static class WPFHelper
    {

        /// <summary>
        /// Enumerates the child controls of the given type from the specified parent control.
        /// </summary>
        /// <typeparam name="T">The type of child controls to find.</typeparam>
        /// <param name="parent">The parent control.</param>
        /// <returns>An enumeration of child controls of the given type from the parent control.</returns>
        public static IEnumerable<T> GetVisualChildren<T>(DependencyObject parent) where T : Visual
        {
            // Check the given parent
            if (parent == null) yield break;

            // Get the number of children
            var children = VisualTreeHelper.GetChildrenCount(parent);

            // Go through the children
            for (var i = 0; i < children; i++)
            {
                // Get the child control
                var child = (DependencyObject)VisualTreeHelper.GetChild(parent, i);

                // Check if the child control is the type we want
                if (child is T variable) yield return variable;

                // Recursively search for other child controls of the requested type
                foreach (var childOfChild in GetVisualChildren<T>(child))
                    yield return childOfChild;
            }
        }

        /// <summary>
        /// Returns the child control of the given type from the specified parent control.
        /// </summary>
        /// <typeparam name="T">The type of child control to return.</typeparam>
        /// <param name="parent">The parent control.</param>
        /// <param name="name">The name of the child control to find.</param>
        /// <returns>The child control of the given type from the specified parent control.</returns>
        public static T GetVisualChildByName<T>(DependencyObject parent, string name) where T : Visual
        {
            // Check the given parent
            if (parent == null) return null;

            // Check the given name
            if (string.IsNullOrWhiteSpace(name)) return null;

            // Initialize the result
            T result = null;

            // Get the controls
            foreach (var child in GetVisualChildren<T>(parent))
            {
                // Get the child control
                var childElement = child as FrameworkElement;
                if (childElement == null) continue;

                // Check the name of the child control
                if (!childElement.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)) continue;

                // Set the result
                result = child;
            }

            // Return the result
            return result;
        }

    }
}
