// Copyright (c) Alexandre Mutel. All rights reserved.
// This file is licensed under the BSD-Clause 2 license.
// See the license.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;

namespace Zio
{
    /// <inheritdoc />
    /// <summary>
    /// Interface of a read only FileSystem.
    /// </summary>
    public interface IReadOnlyFileSystem : IDisposable
    {
        // ----------------------------------------------
        // Directory API
        // ----------------------------------------------

        /// <summary>
        /// Determines whether the given path refers to an existing directory on disk.
        /// </summary>
        /// <param name="path">The path to test.</param>
        /// <returns><c>true</c> if the given path refers to an existing directory on disk, <c>false</c> otherwise.</returns>
        bool DirectoryExists(UPath path);

        // ----------------------------------------------
        // File API
        // ----------------------------------------------

        /// <summary>
        /// Gets the size, in bytes, of a file.
        /// </summary>
        /// <param name="path">The path of a file.</param>
        /// <returns>The size, in bytes, of the file</returns>
        long GetFileLength(UPath path);

        /// <summary>
        /// Determines whether the specified file exists.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns><c>true</c> if the caller has the required permissions and path contains the name of an existing file; 
        /// otherwise, <c>false</c>. This method also returns false if path is null, an invalid path, or a zero-length string. 
        /// If the caller does not have sufficient permissions to read the specified file, 
        /// no exception is thrown and the method returns false regardless of the existence of path.</returns>
        bool FileExists(UPath path);

        /// <summary>
        /// Opens a file <see cref="Stream"/> on the specified path, having the specified mode with read access.
        /// </summary>
        /// <param name="path">The path to the file to open.</param>
        /// <returns>A file <see cref="Stream"/> on the specified path, having the specified mode with read access.</returns>
        Stream OpenRead(UPath path);

        // ----------------------------------------------
        // Metadata API
        // ----------------------------------------------

        /// <summary>
        /// Gets the <see cref="FileAttributes"/> of the file or directory on the path.
        /// </summary>
        /// <param name="path">The path to the file or directory.</param>
        /// <returns>The <see cref="FileAttributes"/> of the file or directory on the path.</returns>
        FileAttributes GetAttributes(UPath path);

        /// <summary>
        /// Returns the creation date and time of the specified file or directory.
        /// </summary>
        /// <param name="path">The path to a file or directory for which to obtain creation date and time information.</param>
        /// <returns>A <see cref="DateTime"/> structure set to the creation date and time for the specified file or directory. This value is expressed in local time.</returns>
        DateTime GetCreationTime(UPath path);

        /// <summary>
        /// Returns the last access date and time of the specified file or directory.
        /// </summary>
        /// <param name="path">The path to a file or directory for which to obtain creation date and time information.</param>
        /// <returns>A <see cref="DateTime"/> structure set to the last access date and time for the specified file or directory. This value is expressed in local time.</returns>
        DateTime GetLastAccessTime(UPath path);

        /// <summary>
        /// Returns the last write date and time of the specified file or directory.
        /// </summary>
        /// <param name="path">The path to a file or directory for which to obtain creation date and time information.</param>
        /// <returns>A <see cref="DateTime"/> structure set to the last write date and time for the specified file or directory. This value is expressed in local time.</returns>
        DateTime GetLastWriteTime(UPath path);

        // ----------------------------------------------
        // Search API
        // ----------------------------------------------

        /// <summary>
        /// Returns an enumerable collection of file names and/or directory names that match a search pattern in a specified path, and optionally searches subdirectories.
        /// </summary>
        /// <param name="path">The path to the directory to search.</param>
        /// <param name="searchPattern">The search string to match against file-system entries in path. This parameter can contain a combination of valid literal path and wildcard (* and ?) characters (see Remarks), but doesn't support regular expressions.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or should include all subdirectories.</param>
        /// <param name="searchTarget">The search target either <see cref="SearchTarget.Both"/> or only <see cref="SearchTarget.Directory"/> or <see cref="SearchTarget.File"/>.</param>
        /// <returns>An enumerable collection of file-system paths in the directory specified by path and that match the specified search pattern, option and target.</returns>
        IEnumerable<UPath> EnumeratePaths(UPath path, string searchPattern, SearchOption searchOption, SearchTarget searchTarget);

        // ----------------------------------------------
        // Path API
        // ----------------------------------------------

        /// <summary>
        /// Converts the specified path to the underlying path used by this <see cref="IFileSystem"/>. In case of a <see cref="PhysicalFileSystem"/>, it 
        /// would represent the actual path on the disk.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The converted system path according to the specified path.</returns>
        string ConvertPathToInternal(UPath path);

        /// <summary>
        /// Converts the specified system path to a <see cref="IFileSystem"/> path.
        /// </summary>
        /// <param name="systemPath">The system path.</param>
        /// <returns>The converted path according to the system path.</returns>
        UPath ConvertPathFromInternal(string systemPath);
    }
}