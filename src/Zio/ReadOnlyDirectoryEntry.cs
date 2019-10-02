// Copyright (c) Alexandre Mutel. All rights reserved.
// This file is licensed under the BSD-Clause 2 license.
// See the license.txt file in the project root for more information.

using System.Collections.Generic;
using System.IO;

namespace Zio
{
    /// <summary>
    /// Exposes instance methods for creating, moving, and enumerating through directories and subdirectories. 
    /// </summary>
    public sealed class ReadOnlyDirectoryEntry : ReadOnlyFileSystemEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyDirectoryEntry"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The directory path.</param>
        public ReadOnlyDirectoryEntry(IReadOnlyFileSystem fileSystem, UPath path) : base(fileSystem, path)
        {
        }

        /// <summary>Returns an enumerable collection of directory information that matches a specified search pattern and search subdirectory option. </summary>
        /// <param name="searchPattern">The search string to match against the names of directories in path. This parameter can contain a combination 
        /// of valid literal path and wildcard (* and ?) characters (see Remarks), but doesn't support regular expressions.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory 
        /// or should include all subdirectories.
        /// The default value is TopDirectoryOnly.</param>
        /// <returns>An enumerable collection of directories.</returns>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The path encapsulated in the <see cref="T:System.IO.DirectoryInfo" /> object is invalid (for example, it is on an unmapped drive). </exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        public IEnumerable<ReadOnlyDirectoryEntry> EnumerateDirectories(string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return this.FileSystem.EnumerateDirectoryEntries(this.Path, searchPattern, searchOption);
        }

        /// <summary>Returns an enumerable collection of file information that matches a specified search pattern and search subdirectory option.</summary>
        /// <param name="searchPattern">The search string to match against the names of directories in path. This parameter can contain a combination 
        /// of valid literal path and wildcard (* and ?) characters (see Remarks), but doesn't support regular expressions.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory 
        /// or should include all subdirectories.
        /// The default value is TopDirectoryOnly.</param>
        /// <returns>An enumerable collection of files.</returns>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The path encapsulated in the <see cref="T:System.IO.DirectoryInfo" /> object is invalid (for example, it is on an unmapped drive). </exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        public IEnumerable<ReadOnlyFileEntry> EnumerateFiles(string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return this.FileSystem.EnumerateFileEntries(this.Path, searchPattern, searchOption);
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="ReadOnlyFileSystemEntry"/> that match a search pattern in a specified path.
        /// </summary>
        /// <param name="searchPattern">The search string to match against the names of directories in path. This parameter can contain a combination 
        /// of valid literal path and wildcard (* and ?) characters (see Remarks), but doesn't support regular expressions.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory 
        /// or should include all subdirectories.
        /// The default value is TopDirectoryOnly.</param>
        /// <param name="searchTarget">The search target either <see cref="SearchTarget.Both"/> or only <see cref="SearchTarget.Directory"/> or <see cref="SearchTarget.File"/>.</param>
        /// <returns>An enumerable collection of <see cref="ReadOnlyFileSystemEntry"/> that match a search pattern in a specified path.</returns>
        public IEnumerable<ReadOnlyFileSystemEntry> EnumerateEntries(string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly, SearchTarget searchTarget = SearchTarget.Both)
        {
            return this.FileSystem.EnumerateFileSystemEntries(this.Path, searchPattern, searchOption, searchTarget);
        }

        /// <inheritdoc />
        public override bool Exists => this.FileSystem.DirectoryExists(this.Path);
    }
}