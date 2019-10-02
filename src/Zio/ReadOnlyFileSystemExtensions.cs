// Copyright (c) Alexandre Mutel. All rights reserved.
// This file is licensed under the BSD-Clause 2 license.
// See the license.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Zio
{
    /// <summary>
    ///     Extension methods for <see cref="IReadOnlyFileSystem" />
    /// </summary>
    public static class ReadOnlyFileSystemExtensions
    {
        /// <summary>
        ///     Opens a binary file, reads the contents of the file into a byte array, and then closes the file.
        /// </summary>
        /// <param name="fs">The filesystem.</param>
        /// <param name="path">The path of the file to open for reading.</param>
        /// <returns>A byte array containing the contents of the file.</returns>
        public static byte[] ReadAllBytes(this IReadOnlyFileSystem fs, UPath path)
        {
            var memstream = new MemoryStream();
            using (var stream = fs.OpenRead(path))
            {
                stream.CopyTo(memstream);
            }
            return memstream.ToArray();
        }

        /// <summary>
        ///     Opens a file, reads all lines of the file with the specified encoding, and then closes the file.
        /// </summary>
        /// <param name="fs">The filesystem.</param>
        /// <param name="path">The path of the file to open for reading.</param>
        /// <returns>A string containing all lines of the file.</returns>
        /// <remarks>
        ///     This method attempts to automatically detect the encoding of a file based on the presence of byte order marks.
        ///     Encoding formats UTF-8 and UTF-32 (both big-endian and little-endian) can be detected.
        /// </remarks>
        public static string ReadAllText(this IReadOnlyFileSystem fs, UPath path)
        {
            var stream = fs.OpenRead(path);
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        ///     Opens a file, reads all lines of the file with the specified encoding, and then closes the file.
        /// </summary>
        /// <param name="fs">The filesystem.</param>
        /// <param name="path">The path of the file to open for reading.</param>
        /// <param name="encoding">The encoding to use to decode the text from <paramref name="path" />.</param>
        /// <returns>A string containing all lines of the file.</returns>
        public static string ReadAllText(this IReadOnlyFileSystem fs, UPath path, Encoding encoding)
        {
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));
            var stream = fs.OpenRead(path);
            {
                using (var reader = new StreamReader(stream, encoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }
        
        /// <summary>
        ///     Opens a file, reads all lines of the file with the specified encoding, and then closes the file.
        /// </summary>
        /// <param name="fs">The filesystem.</param>
        /// <param name="path">The path of the file to open for reading.</param>
        /// <returns>An array of strings containing all lines of the file.</returns>
        public static string[] ReadAllLines(this IReadOnlyFileSystem fs, UPath path)
        {
            var stream = fs.OpenRead(path);
            {
                using (var reader = new StreamReader(stream))
                {
                    var lines = new List<string>();
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        lines.Add(line);
                    }
                    return lines.ToArray();
                }
            }
        }

        /// <summary>
        ///     Opens a file, reads all lines of the file with the specified encoding, and then closes the file.
        /// </summary>
        /// <param name="fs">The filesystem.</param>
        /// <param name="path">The path of the file to open for reading.</param>
        /// <param name="encoding">The encoding to use to decode the text from <paramref name="path" />.</param>
        /// <remarks>
        ///     This method attempts to automatically detect the encoding of a file based on the presence of byte order marks.
        ///     Encoding formats UTF-8 and UTF-32 (both big-endian and little-endian) can be detected.
        /// </remarks>
        /// <returns>An array of strings containing all lines of the file.</returns>
        public static string[] ReadAllLines(this IReadOnlyFileSystem fs, UPath path, Encoding encoding)
        {
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));
            var stream = fs.OpenRead(path);
            {
                using (var reader = new StreamReader(stream, encoding))
                {
                    var lines = new List<string>();
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        lines.Add(line);
                    }
                    return lines.ToArray();
                }
            }
        }
        
        /// <summary>
        /// Returns an enumerable collection of directory names that match a search pattern in a specified path.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The path of the directory to look for directories.</param>
        /// <param name="searchPattern">The search string to match against the names of directories in path. This parameter can contain a combination 
        /// of valid literal path and wildcard (* and ?) characters (see Remarks), but doesn't support regular expressions.</param>
        /// <returns>An enumerable collection of the full names (including paths) for the directories in the directory specified by path.</returns>
        public static IEnumerable<UPath> EnumerateDirectories(this IReadOnlyFileSystem fileSystem, UPath path, string searchPattern = "*")
        {
            if (searchPattern == null) throw new ArgumentNullException(nameof(searchPattern));
            return EnumerateDirectories(fileSystem, path, searchPattern, SearchOption.TopDirectoryOnly);
        }

        /// <summary>
        /// Returns an enumerable collection of directory names that match a search pattern in a specified path, and optionally searches subdirectories.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The path of the directory to look for directories.</param>
        /// <param name="searchPattern">The search string to match against the names of directories in path. This parameter can contain a combination 
        /// of valid literal path and wildcard (* and ?) characters (see Remarks), but doesn't support regular expressions.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory 
        /// or should include all subdirectories.
        /// The default value is TopDirectoryOnly.</param>
        /// <returns>An enumerable collection of the full names (including paths) for the directories in the directory specified by path.</returns>
        public static IEnumerable<UPath> EnumerateDirectories(this IReadOnlyFileSystem fileSystem, UPath path, string searchPattern, SearchOption searchOption)
        {
            if (searchPattern == null) throw new ArgumentNullException(nameof(searchPattern));
            foreach (var subPath in fileSystem.EnumeratePaths(path, searchPattern, searchOption, SearchTarget.Directory))
                yield return subPath;
        }

        /// <summary>
        /// Returns an enumerable collection of file names that match a search pattern in a specified path.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The path of the directory to look for files.</param>
        /// <param name="searchPattern">The search string to match against the names of directories in path. This parameter can contain a combination 
        /// of valid literal path and wildcard (* and ?) characters (see Remarks), but doesn't support regular expressions.</param>
        /// <returns>An enumerable collection of the full names (including paths) for the files in the directory specified by path.</returns>
        public static IEnumerable<UPath> EnumerateFiles(this IReadOnlyFileSystem fileSystem, UPath path, string searchPattern = "*")
        {
            if (searchPattern == null) throw new ArgumentNullException(nameof(searchPattern));
            return EnumerateFiles(fileSystem, path, searchPattern, SearchOption.TopDirectoryOnly);
        }

        /// <summary>
        /// Returns an enumerable collection of file names in a specified path.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The path of the directory to look for files.</param>
        /// <param name="searchPattern">The search string to match against the names of directories in path. This parameter can contain a combination 
        /// of valid literal path and wildcard (* and ?) characters (see Remarks), but doesn't support regular expressions.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory 
        /// or should include all subdirectories.
        /// The default value is TopDirectoryOnly.</param>
        /// <returns>An enumerable collection of the full names (including paths) for the files in the directory specified by path.</returns>
        public static IEnumerable<UPath> EnumerateFiles(this IReadOnlyFileSystem fileSystem, UPath path, string searchPattern, SearchOption searchOption)
        {
            if (searchPattern == null) throw new ArgumentNullException(nameof(searchPattern));
            foreach (var subPath in fileSystem.EnumeratePaths(path, searchPattern, searchOption, SearchTarget.File))
                yield return subPath;
        }

        /// <summary>
        /// Returns an enumerable collection of file or directory names in a specified path.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The path of the directory to look for files or directories.</param>
        /// <param name="searchPattern">The search string to match against the names of directories in path. This parameter can contain a combination 
        /// of valid literal path and wildcard (* and ?) characters (see Remarks), but doesn't support regular expressions.</param>
        /// <returns>An enumerable collection of the full names (including paths) for the files and directories in the directory specified by path.</returns>
        public static IEnumerable<UPath> EnumeratePaths(this IReadOnlyFileSystem fileSystem, UPath path, string searchPattern = "*")
        {
            if (searchPattern == null) throw new ArgumentNullException(nameof(searchPattern));
            return EnumeratePaths(fileSystem, path, searchPattern, SearchOption.TopDirectoryOnly);
        }

        /// <summary>
        /// Returns an enumerable collection of file or directory names in a specified path.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The path of the directory to look for files or directories.</param>
        /// <param name="searchPattern">The search string to match against the names of directories in path. This parameter can contain a combination 
        /// of valid literal path and wildcard (* and ?) characters (see Remarks), but doesn't support regular expressions.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory 
        /// or should include all subdirectories.
        /// The default value is TopDirectoryOnly.</param>
        /// <returns>An enumerable collection of the full names (including paths) for the files and directories in the directory specified by path.</returns>
        public static IEnumerable<UPath> EnumeratePaths(this IReadOnlyFileSystem fileSystem, UPath path, string searchPattern, SearchOption searchOption)
        {
            if (searchPattern == null) throw new ArgumentNullException(nameof(searchPattern));
            return fileSystem.EnumeratePaths(path, searchPattern, searchOption, SearchTarget.Both);
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="ReadOnlyFileEntry"/> that match a search pattern in a specified path.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The path of the directory to look for files.</param>
        /// <param name="searchPattern">The search string to match against the names of directories in path. This parameter can contain a combination 
        /// of valid literal path and wildcard (* and ?) characters (see Remarks), but doesn't support regular expressions.</param>
        /// <returns>An enumerable collection of <see cref="ReadOnlyFileEntry"/> from the specified path.</returns>
        public static IEnumerable<ReadOnlyFileEntry> EnumerateFileEntries(this IReadOnlyFileSystem fileSystem, UPath path, string searchPattern = "*")
        {
            if (searchPattern == null) throw new ArgumentNullException(nameof(searchPattern));
            return EnumerateFileEntries(fileSystem, path, searchPattern, SearchOption.TopDirectoryOnly);
        }
        
        /// <summary>
        /// Returns an enumerable collection of <see cref="ReadOnlyFileEntry"/> that match a search pattern in a specified path.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The path of the directory to look for files.</param>
        /// <param name="searchPattern">The search string to match against the names of directories in path. This parameter can contain a combination 
        /// of valid literal path and wildcard (* and ?) characters (see Remarks), but doesn't support regular expressions.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory 
        /// or should include all subdirectories.
        /// The default value is TopDirectoryOnly.</param>
        /// <returns>An enumerable collection of <see cref="ReadOnlyFileEntry"/> from the specified path.</returns>
        public static IEnumerable<ReadOnlyFileEntry> EnumerateFileEntries(this IReadOnlyFileSystem fileSystem, UPath path, string searchPattern, SearchOption searchOption)
        {
            if (searchPattern == null) throw new ArgumentNullException(nameof(searchPattern));
            foreach (var subPath in EnumerateFiles(fileSystem, path, searchPattern, searchOption))
            {
                yield return new ReadOnlyFileEntry(fileSystem, subPath);
            }
        }
        
        /// <summary>
        /// Returns an enumerable collection of <see cref="ReadOnlyDirectoryEntry"/> that match a search pattern in a specified path.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The path of the directory to look for directories.</param>
        /// <param name="searchPattern">The search string to match against the names of directories in path. This parameter can contain a combination 
        /// of valid literal path and wildcard (* and ?) characters (see Remarks), but doesn't support regular expressions.</param>
        /// <returns>An enumerable collection of <see cref="ReadOnlyDirectoryEntry"/> from the specified path.</returns>
        public static IEnumerable<ReadOnlyDirectoryEntry> EnumerateDirectoryEntries(this IReadOnlyFileSystem fileSystem, UPath path, string searchPattern = "*")
        {
            if (searchPattern == null) throw new ArgumentNullException(nameof(searchPattern));
            return EnumerateDirectoryEntries(fileSystem, path, searchPattern, SearchOption.TopDirectoryOnly);
        }
        
        /// <summary>
        /// Returns an enumerable collection of <see cref="ReadOnlyDirectoryEntry"/> that match a search pattern in a specified path.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The path of the directory to look for directories.</param>
        /// <param name="searchPattern">The search string to match against the names of directories in path. This parameter can contain a combination 
        /// of valid literal path and wildcard (* and ?) characters (see Remarks), but doesn't support regular expressions.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory 
        /// or should include all subdirectories.
        /// The default value is TopDirectoryOnly.</param>
        /// <returns>An enumerable collection of <see cref="ReadOnlyDirectoryEntry"/> from the specified path.</returns>
        public static IEnumerable<ReadOnlyDirectoryEntry> EnumerateDirectoryEntries(this IReadOnlyFileSystem fileSystem, UPath path, string searchPattern, SearchOption searchOption)
        {
            if (searchPattern == null) throw new ArgumentNullException(nameof(searchPattern));
            foreach (var subPath in EnumerateDirectories(fileSystem, path, searchPattern, searchOption))
            {
                yield return new ReadOnlyDirectoryEntry(fileSystem, subPath);
            }
        }
        
        /// <summary>
        /// Returns an enumerable collection of <see cref="ReadOnlyFileSystemEntry"/> that match a search pattern in a specified path.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The path of the directory to look for files and directories.</param>
        /// <param name="searchPattern">The search string to match against the names of directories in path. This parameter can contain a combination 
        /// of valid literal path and wildcard (* and ?) characters (see Remarks), but doesn't support regular expressions.</param>
        /// <returns>An enumerable collection of <see cref="ReadOnlyFileSystemEntry"/> that match a search pattern in a specified path.</returns>
        public static IEnumerable<ReadOnlyFileSystemEntry> EnumerateFileSystemEntries(this IReadOnlyFileSystem fileSystem, UPath path, string searchPattern = "*")
        {
            if (searchPattern == null) throw new ArgumentNullException(nameof(searchPattern));
            return EnumerateFileSystemEntries(fileSystem, path, searchPattern, SearchOption.TopDirectoryOnly);
        }
        
        /// <summary>
        /// Returns an enumerable collection of <see cref="ReadOnlyFileSystemEntry"/> that match a search pattern in a specified path.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The path of the directory to look for files and directories.</param>
        /// <param name="searchPattern">The search string to match against the names of directories in path. This parameter can contain a combination 
        /// of valid literal path and wildcard (* and ?) characters (see Remarks), but doesn't support regular expressions.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory 
        /// or should include all subdirectories.
        /// The default value is TopDirectoryOnly.</param>
        /// <param name="searchTarget">The search target either <see cref="SearchTarget.Both"/> or only <see cref="SearchTarget.Directory"/> or <see cref="SearchTarget.File"/>. Default is <see cref="SearchTarget.Both"/></param>
        /// <returns>An enumerable collection of <see cref="ReadOnlyFileSystemEntry"/> that match a search pattern in a specified path.</returns>
        public static IEnumerable<ReadOnlyFileSystemEntry> EnumerateFileSystemEntries(this IReadOnlyFileSystem fileSystem, UPath path, string searchPattern, SearchOption searchOption, SearchTarget searchTarget = SearchTarget.Both)
        {
            if (searchPattern == null) throw new ArgumentNullException(nameof(searchPattern));
            foreach (var subPath in fileSystem.EnumeratePaths(path, searchPattern, searchOption, searchTarget))
            {
                yield return fileSystem.DirectoryExists(subPath) ? (ReadOnlyFileSystemEntry) new ReadOnlyDirectoryEntry(fileSystem, subPath) : new ReadOnlyFileEntry(fileSystem, subPath);
            }
        }
        
        /// <summary>
        /// Gets a <see cref="ReadOnlyFileSystemEntry"/> for the specified path. If the file or directory does not exist, throws a <see cref="FileNotFoundException"/>
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The file or directory path.</param>
        /// <returns>A new <see cref="ReadOnlyFileSystemEntry"/> from the specified path.</returns>
        public static ReadOnlyFileSystemEntry GetFileSystemEntry(this IReadOnlyFileSystem fileSystem, UPath path)
        {
            var fileExists = fileSystem.FileExists(path);
            if (fileExists)
            {
                return new ReadOnlyFileEntry(fileSystem, path);
            }
            var directoryExists = fileSystem.DirectoryExists(path);
            if (directoryExists)
            {
                return new ReadOnlyDirectoryEntry(fileSystem, path);
            }

            throw FileSystemExceptionHelper.NewFileNotFoundException(path);
        }
        
        /// <summary>
        /// Tries to get a <see cref="ReadOnlyFileSystemEntry"/> for the specified path. If the file or directory does not exist, returns null.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The file or directory path.</param>
        /// <returns>A new <see cref="ReadOnlyFileSystemEntry"/> from the specified path.</returns>
        public static ReadOnlyFileSystemEntry TryGetFileSystemEntry(this IReadOnlyFileSystem fileSystem, UPath path)
        {
            var fileExists = fileSystem.FileExists(path);
            if (fileExists)
            {
                return new ReadOnlyFileEntry(fileSystem, path);
            }
            var directoryExists = fileSystem.DirectoryExists(path);
            return directoryExists ? new ReadOnlyDirectoryEntry(fileSystem, path) : null;
        }

        /// <summary>
        /// Gets a <see cref="ReadOnlyFileEntry"/> for the specified path. If the file does not exist, throws a <see cref="FileNotFoundException"/>
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="filePath">The file path.</param>
        /// <returns>A new <see cref="ReadOnlyFileEntry"/> from the specified path.</returns>
        public static ReadOnlyFileEntry GetFileEntry(this IReadOnlyFileSystem fileSystem, UPath filePath)
        {
            if (!fileSystem.FileExists(filePath))
            {
                throw FileSystemExceptionHelper.NewFileNotFoundException(filePath);
            }
            return new ReadOnlyFileEntry(fileSystem, filePath);
        }
        
        /// <summary>
        /// Gets a <see cref="ReadOnlyDirectoryEntry"/> for the specified path. If the file does not exist, throws a <see cref="DirectoryNotFoundException"/>
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="directoryPath">The directory path.</param>
        /// <returns>A new <see cref="ReadOnlyDirectoryEntry"/> from the specified path.</returns>
        public static ReadOnlyDirectoryEntry GetDirectoryEntry(this IReadOnlyFileSystem fileSystem, UPath directoryPath)
        {
            if (!fileSystem.DirectoryExists(directoryPath))
            {
                throw FileSystemExceptionHelper.NewDirectoryNotFoundException(directoryPath);
            }
            return new ReadOnlyDirectoryEntry(fileSystem, directoryPath);
        }
    }
}