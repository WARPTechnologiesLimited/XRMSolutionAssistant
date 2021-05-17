// -----------------------------------------------------------------------
// <copyright file="StringNumericCompare.cs" company="WARP Technologies Limited">
// Released by WARP for use by the CRM development community.
// </copyright>
// -----------------------------------------------------------------------

namespace WARP.XrmSolutionAssistant.Core.Comparers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    /// <summary>
    /// Comparer that compares strings which contains integers.
    /// </summary>
    public class StringNumericCompare : IComparer<string>
    {
        /// <summary>
        /// Method to determine if a object string is a number.
        /// </summary>
        /// <param name="value">Object to test.</param>
        /// <returns>True if numeric.</returns>
        public static bool IsNumeric(object value)
        {
            return int.TryParse(value.ToString(), out var i);
        }

        /// <inheritdoc />
        public int Compare(string s1, string s2)
        {
            const int S1GreaterThanS2 = 1;
            const int S2GreaterThanS1 = -1;

            if (IsNumeric(s1) && IsNumeric(s2))
            {
                if (Convert.ToInt32(s1) > Convert.ToInt32(s2))
                {
                    return S1GreaterThanS2;
                }

                if (Convert.ToInt32(s1) < Convert.ToInt32(s2))
                {
                    return S2GreaterThanS1;
                }

                if (Convert.ToInt32(s1) == Convert.ToInt32(s2))
                {
                    return 0;
                }
            }

            if (IsNumeric(s1) && !IsNumeric(s2))
            {
                return S2GreaterThanS1;
            }

            if (!IsNumeric(s1) && IsNumeric(s2))
            {
                return S1GreaterThanS2;
            }

            return string.Compare(s1, s2, true, CultureInfo.InvariantCulture);
        }
    }
}