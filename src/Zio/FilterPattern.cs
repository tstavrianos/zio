﻿using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Zio
{
    /// <summary>
    /// Filter pattern compiler used for <see cref="Zio.IFileSystemWatcher"/> implementation.
    /// Use the method <see cref="Parse"/> to create a pattern.
    /// </summary>
    public struct FilterPattern
    {
        private static readonly char[] SpecialChars = {'.', '*', '?'};

        private readonly string _exactMatch;
        private readonly Regex _regexMatch;

        public static FilterPattern Parse(string filter)
        {
            return new FilterPattern(filter);
        }
        
        /// <summary>
        /// Tries to match the specified path with this instance.
        /// </summary>
        /// <param name="path">The path to match.</param>
        /// <returns><c>true</c> if the path was matched, <c>false</c> otherwise.</returns>
        public bool Match(UPath path)
        {
            path.AssertNotNull();
            var name = path.GetName();
            // if _execMatch is null and _regexMatch is null, we have a * match
            return this._exactMatch != null ? this._exactMatch == name : this._regexMatch?.IsMatch(name) != false;
        }

        /// <summary>
        /// Tries to match the specified file name with this instance.
        /// </summary>
        /// <param name="fileName">The file name to match.</param>
        /// <returns><c>true</c> if the file name was matched, <c>false</c> otherwise.</returns>
        public bool Match(string fileName)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));
            // if _execMatch is null and _regexMatch is null, we have a * match
            return this._exactMatch != null ? this._exactMatch == fileName : this._regexMatch?.IsMatch(fileName) != false;
        }

        public FilterPattern(string filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            if (filter.IndexOf(UPath.DirectorySeparator) >= 0)
            {
                throw new ArgumentException("Filter cannot contain directory parts.", nameof(filter));
            }

            this._exactMatch = null;
            this._regexMatch = null;

            // Optimized path, most common cases
            if (filter == "" || filter == "*" || filter == "*.*")
            {
                return;
            }

            var appendSpecialCaseForWildcardExt = false;
            var startIndex = 0;
            StringBuilder builder = null;

            try
            {
                int nextIndex;
                while ((nextIndex = filter.IndexOfAny(SpecialChars, startIndex)) >= 0)
                {
                    if (builder == null)
                    {
                        builder = UPath.GetSharedStringBuilder();
                        builder.Append("^");
                    }

                    var lengthToEscape = nextIndex - startIndex;
                    if (lengthToEscape > 0)
                    {
                        var toEscape = Regex.Escape(filter.Substring(startIndex, lengthToEscape));
                        builder.Append(toEscape);
                    }

                    var c = filter[nextIndex];

                    // special case for wildcard file extension to allow blank extensions as well
                    if (c == '.' && nextIndex == filter.Length - 2 && filter[nextIndex + 1] == '*')
                    {
                        appendSpecialCaseForWildcardExt = true;
                        break;
                    }

                    var regexPatternPart =
                        c == '.' ? "\\." : c == '*' ? ".*?" : ".";
                    builder.Append(regexPatternPart);

                    startIndex = nextIndex + 1;
                }
                if (builder == null)
                {
                    this._exactMatch = filter;
                }
                else
                {
                    if (appendSpecialCaseForWildcardExt)
                    {
                        builder.Append("(\\.[^.]*)?");
                    }
                    else
                    {
                        var length = filter.Length - startIndex;
                        if (length > 0)
                        {
                            var toEscape = Regex.Escape(filter.Substring(startIndex, length));
                            builder.Append(toEscape);
                        }
                    }

                    builder.Append("$");

                    var regexPattern = builder.ToString();
                    this._regexMatch = new Regex(regexPattern);
                }
            }
            finally
            {
                if (builder != null)
                {
                    builder.Length = 0;
                }
            }
        }
    }
}
