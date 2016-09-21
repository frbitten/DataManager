using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Joo.Utils.Helpers
{
    /// <summary>
    /// Extensions of basic types
    /// </summary>
    public static class ExtensionOthers
    {
        /// <summary>
        /// Determines whether a value is between a minimum and maximum value.
        /// </summary>
        /// <typeparam name="T">The type of the value parameter.</typeparam>
        /// <param name="value">The value that needs to be checked.</param>
        /// <param name="low">The inclusive lower boundary.</param>
        /// <param name="high">The inclusive upper boundary.</param>
        public static bool IsBetween<T>(this T value, T low, T high) where T : IComparable<T>
        {
            return value.CompareTo(low) >= 0 && value.CompareTo(high) <= 0;
        }

        /// <summary>
        /// Reduces a string size to the desired value
        /// </summary>
        /// <param name="value"> String in question </param>
        /// <param name="maxChars"> Number of characters  </param>
        /// <returns>String Result</returns>
        public static string Truncate(this string value, int maxChars)
        {
            if (value == null)
                return null;

            return value.Length <= maxChars ?
                   value :
                   value.Substring(0, maxChars);
        }

        /// <summary>
        /// Reduces a string size to the (desired value -2) and adds ".." (dots) at the end.
        /// </summary>
        /// <example>
        /// string teste = "teste123";
        /// string result = teste.TruncateWithDots(5);
        /// result is "tes.."
        /// </example>
        /// <param name="value"> String in question </param>
        /// <param name="maxChars"> Number of characters  </param>
        /// <returns>String Result</returns>
        public static string TruncateWithDots(this string value, int maxChars)
        {
            if (value == null)
                return null;

            return value.Length <= maxChars ?
                   value :
                   value.Substring(0, maxChars-2) + "..";
        }

        /// <summary>
        /// Change the characters with accents to normal characters
        /// </summary>
        /// <param name="stIn">String in question</param>
        /// <returns>String Normalized</returns>
        public static string RemoveAccents(this string stIn)
        {
            if (stIn == null)
                return null;

            StringBuilder sb = new StringBuilder();
            string stFormD = stIn.Normalize(NormalizationForm.FormD);

            stFormD.Each(c =>
                {
                    UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(c);
                    if (uc != UnicodeCategory.NonSpacingMark)
                    {
                        sb.Append(c);
                    }
                });

            return (sb.ToString().Normalize(NormalizationForm.FormC));
        }
    }
}
