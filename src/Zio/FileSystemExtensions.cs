﻿// Copyright (c) Alexandre Mutel. All rights reserved.
// This file is licensed under the BSD-Clause 2 license. 
// See the license.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Zio.FileSystems;
using static Zio.FileSystemExceptionHelper;

namespace Zio
{
    /// <summary>
    ///     Extension methods for <see cref="IFileSystem" />
    /// </summary>
    public static class FileSystemExtensions
    {
        /// <summary>
        /// Gets or create a <see cref="SubFileSystem"/> from an existing filesystem and the specified sub folder
        /// </summary>
        /// <param name="fs">The filesystem to derive a new sub-filesystem from it</param>
        /// <param name="subFolder">The folder of the sub-filesystem</param>
        /// <returns>A sub-filesystem</returns>
        public static SubFileSystem GetOrCreateSubFileSystem(this IFileSystem fs, UPath subFolder)
        {
            if (!fs.DirectoryExists(subFolder))
            {
                fs.CreateDirectory(subFolder);
            }
            return new SubFileSystem(fs, subFolder);
        }

        /// <summary>
        ///     Copies a file between two filesystems.
        /// </summary>
        /// <param name="fs">The source filesystem</param>
        /// <param name="destFileSystem">The destination filesystem</param>
        /// <param name="srcPath">The source path of the file to copy from the source filesystem</param>
        /// <param name="destPath">The destination path of the file in the destination filesystem</param>
        /// <param name="overwrite"><c>true</c> to overwrite an existing destination file</param>
        public static void CopyFileCross(this IFileSystem fs, IFileSystem destFileSystem, UPath srcPath, UPath destPath, bool overwrite)
        {
            if (destFileSystem == null) throw new ArgumentNullException(nameof(destFileSystem));

            // If this is the same filesystem, use the file system directly to perform the action
            if (fs == destFileSystem)
            {
                fs.CopyFile(srcPath, destPath, overwrite);
                return;
            }

            srcPath.AssertAbsolute(nameof(srcPath));
            if (!fs.FileExists(srcPath))
            {
                throw NewFileNotFoundException(srcPath);
            }

            destPath.AssertAbsolute(nameof(destPath));
            var destDirectory = destPath.GetDirectory();
            if (!destFileSystem.DirectoryExists(destDirectory))
            {
                throw NewDirectoryNotFoundException(destDirectory);
            }

            if (destFileSystem.FileExists(destPath) && !overwrite)
            {
                throw new IOException($"The destination file path `{destPath}` already exist and overwrite is false");
            }

            using (var sourceStream = fs.OpenFile(srcPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var copied = false;
                try
                {
                    using (var destStream = destFileSystem.OpenFile(destPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        sourceStream.CopyTo(destStream);
                    }

                    // NOTE: For some reasons, we can sometimes get an Unauthorized access if we try to set the LastWriteTime after the SetAttributes
                    // So we setup it here.
                    destFileSystem.SetLastWriteTime(destPath, fs.GetLastWriteTime(srcPath));

                    // Preserve attributes and LastWriteTime as a regular File.Copy
                    destFileSystem.SetAttributes(destPath, fs.GetAttributes(srcPath));

                    copied = true;
                }
                finally
                {
                    if (!copied)
                    {
                        try
                        {
                            destFileSystem.DeleteFile(destPath);
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Moves a file between two filesystems.
        /// </summary>
        /// <param name="fs">The source filesystem</param>
        /// <param name="destFileSystem">The destination filesystem</param>
        /// <param name="srcPath">The source path of the file to move from the source filesystem</param>
        /// <param name="destPath">The destination path of the file in the destination filesystem</param>
        public static void MoveFileCross(this IFileSystem fs, IFileSystem destFileSystem, UPath srcPath, UPath destPath)
        {
            if (destFileSystem == null) throw new ArgumentNullException(nameof(destFileSystem));

            // If this is the same filesystem, use the file system directly to perform the action
            if (fs == destFileSystem)
            {
                fs.MoveFile(srcPath, destPath);
                return;
            }

            // Check source
            srcPath.AssertAbsolute(nameof(srcPath));
            if (!fs.FileExists(srcPath))
            {
                throw NewFileNotFoundException(srcPath);
            }

            // Check destination
            destPath.AssertAbsolute(nameof(destPath));
            var destDirectory = destPath.GetDirectory();
            if (!destFileSystem.DirectoryExists(destDirectory))
            {
                throw NewDirectoryNotFoundException(destPath);
            }

            if (destFileSystem.DirectoryExists(destPath))
            {
                throw NewDestinationDirectoryExistException(destPath);
            }

            if (destFileSystem.FileExists(destPath))
            {
                throw NewDestinationFileExistException(destPath);
            }

            using (var sourceStream = fs.OpenFile(srcPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var copied = false;
                try
                {
                    using (var destStream = destFileSystem.OpenFile(destPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        sourceStream.CopyTo(destStream);
                    }

                    // Preserve all attributes and times
                    destFileSystem.SetAttributes(destPath, fs.GetAttributes(srcPath));
                    destFileSystem.SetCreationTime(destPath, fs.GetCreationTime(srcPath));
                    destFileSystem.SetLastAccessTime(destPath, fs.GetLastAccessTime(srcPath));
                    destFileSystem.SetLastWriteTime(destPath, fs.GetLastWriteTime(srcPath));
                    copied = true;
                }
                finally
                {
                    if (!copied)
                    {
                        try
                        {
                            destFileSystem.DeleteFile(destPath);
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
            }

            var deleted = false;
            try
            {
                fs.DeleteFile(srcPath);
                deleted = true;
            }
            finally
            {
                if (!deleted)
                {
                    try
                    {
                        destFileSystem.DeleteFile(destPath);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }

        /// <summary>
        ///     Creates a new file, writes the specified byte array to the file, and then closes the file.
        ///     If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="fs">The filesystem.</param>
        /// <param name="path">The path of the file to open for writing.</param>
        /// <param name="content">The content.</param>
        /// <exception cref="System.ArgumentNullException">content</exception>
        /// <remarks>
        ///     Given a byte array and a file path, this method opens the specified file, writes the
        ///     contents of the byte array to the file, and then closes the file.
        /// </remarks>
        public static void WriteAllBytes(this IFileSystem fs, UPath path, byte[] content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            using (var stream = fs.OpenFile(path, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                stream.Write(content, 0, content.Length);
            }
        }

        

        /// <summary>
        ///     Creates a new file, writes the specified string to the file, and then closes the file.
        ///     If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="fs">The filesystem.</param>
        /// <param name="path">The path of the file to open for writing.</param>
        /// <param name="content">The content.</param>
        /// <exception cref="System.ArgumentNullException">content</exception>
        /// <remarks>
        ///     This method uses UTF-8 encoding without a Byte-Order Mark (BOM), so using the GetPreamble method will return an
        ///     empty byte array.
        ///     If it is necessary to include a UTF-8 identifier, such as a byte order mark, at the beginning of a file,
        ///     use the <see cref="WriteAllText(mgRogue.VFS.IFileSystem,mgRogue.VFS.UPath,string,System.Text.Encoding)" /> method overload with UTF8 encoding.
        /// </remarks>
        public static void WriteAllText(this IFileSystem fs, UPath path, string content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            var stream = fs.OpenFile(path, FileMode.Create, FileAccess.Write, FileShare.Read);
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(content);
                    writer.Flush();
                }
            }
        }

        /// <summary>
        ///     Creates a new file, writes the specified string to the file using the specified encoding, and then
        ///     closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="fs">The filesystem.</param>
        /// <param name="path">The path of the file to open for writing.</param>
        /// <param name="content">The content.</param>
        /// <param name="encoding">The encoding to use to decode the text from <paramref name="path" />. </param>
        /// <exception cref="System.ArgumentNullException">content</exception>
        /// <remarks>
        ///     Given a string and a file path, this method opens the specified file, writes the string to the file using the
        ///     specified encoding, and then closes the file.
        ///     The file handle is guaranteed to be closed by this method, even if exceptions are raised.
        /// </remarks>
        public static void WriteAllText(this IFileSystem fs, UPath path, string content, Encoding encoding)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));
            var stream = fs.OpenFile(path, FileMode.Create, FileAccess.Write, FileShare.Read);
            {
                using (var writer = new StreamWriter(stream, encoding))
                {
                    writer.Write(content);
                    writer.Flush();
                }
            }
        }

        /// <summary>
        ///     Opens a file, appends the specified string to the file, and then closes the file. If the file does not exist,
        ///     this method creates a file, writes the specified string to the file, then closes the file.
        /// </summary>
        /// <param name="fs">The filesystem.</param>
        /// <param name="path">The path of the file to open for appending.</param>
        /// <param name="content">The content to append.</param>
        /// <exception cref="System.ArgumentNullException">content</exception>
        /// <remarks>
        ///     Given a string and a file path, this method opens the specified file, appends the string to the end of the file,
        ///     and then closes the file. The file handle is guaranteed to be closed by this method, even if exceptions are raised.
        ///     The method creates the file if it doesn’t exist, but it doesn't create new directories. Therefore, the value of the
        ///     path parameter must contain existing directories.
        /// </remarks>
        public static void AppendAllText(this IFileSystem fs, UPath path, string content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            var stream = fs.OpenFile(path, FileMode.Append, FileAccess.Write, FileShare.Read);
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(content);
                    writer.Flush();
                }
            }
        }

        /// <summary>
        ///     Appends the specified string to the file, creating the file if it does not already exist.
        /// </summary>
        /// <param name="fs">The filesystem.</param>
        /// <param name="path">The path of the file to open for appending.</param>
        /// <param name="content">The content to append.</param>
        /// <param name="encoding">The encoding to use to encode the text from <paramref name="path" />.</param>
        /// <exception cref="System.ArgumentNullException">content</exception>
        /// <remarks>
        ///     Given a string and a file path, this method opens the specified file, appends the string to the end of the file,
        ///     and then closes the file. The file handle is guaranteed to be closed by this method, even if exceptions are raised.
        ///     The method creates the file if it doesn’t exist, but it doesn't create new directories. Therefore, the value of the
        ///     path parameter must contain existing directories.
        /// </remarks>
        public static void AppendAllText(this IFileSystem fs, UPath path, string content, Encoding encoding)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));
            var stream = fs.OpenFile(path, FileMode.Append, FileAccess.Write, FileShare.Read);
            {
                using (var writer = new StreamWriter(stream, encoding))
                {
                    writer.Write(content);
                    writer.Flush();
                }
            }
        }

        /// <summary>
        /// Creates or overwrites a file in the specified path.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The path and name of the file to create.</param>
        /// <returns>A stream that provides read/write access to the file specified in path.</returns>
        public static Stream CreateFile(this IFileSystem fileSystem, UPath path)
        {
            path.AssertAbsolute();
            return fileSystem.OpenFile(path, FileMode.Create, FileAccess.ReadWrite);
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="FileEntry"/> that match a search pattern in a specified path.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The path of the directory to look for files.</param>
        /// <param name="searchPattern">The search string to match against the names of directories in path. This parameter can contain a combination 
        /// of valid literal path and wildcard (* and ?) characters (see Remarks), but doesn't support regular expressions.</param>
        /// <returns>An enumerable collection of <see cref="FileEntry"/> from the specified path.</returns>
        public static IEnumerable<FileEntry> EnumerateFileEntries(this IFileSystem fileSystem, UPath path, string searchPattern = "*")
        {
            if (searchPattern == null) throw new ArgumentNullException(nameof(searchPattern));
            return EnumerateFileEntries(fileSystem, path, searchPattern, SearchOption.TopDirectoryOnly);
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="FileEntry"/> that match a search pattern in a specified path.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The path of the directory to look for files.</param>
        /// <param name="searchPattern">The search string to match against the names of directories in path. This parameter can contain a combination 
        /// of valid literal path and wildcard (* and ?) characters (see Remarks), but doesn't support regular expressions.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory 
        /// or should include all subdirectories.
        /// The default value is TopDirectoryOnly.</param>
        /// <returns>An enumerable collection of <see cref="FileEntry"/> from the specified path.</returns>
        public static IEnumerable<FileEntry> EnumerateFileEntries(this IFileSystem fileSystem, UPath path, string searchPattern, SearchOption searchOption)
        {
            if (searchPattern == null) throw new ArgumentNullException(nameof(searchPattern));
            foreach (var subPath in fileSystem.EnumerateFiles(path, searchPattern, searchOption))
            {
                yield return new FileEntry(fileSystem, subPath);
            }
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="DirectoryEntry"/> that match a search pattern in a specified path.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The path of the directory to look for directories.</param>
        /// <param name="searchPattern">The search string to match against the names of directories in path. This parameter can contain a combination 
        /// of valid literal path and wildcard (* and ?) characters (see Remarks), but doesn't support regular expressions.</param>
        /// <returns>An enumerable collection of <see cref="DirectoryEntry"/> from the specified path.</returns>
        public static IEnumerable<DirectoryEntry> EnumerateDirectoryEntries(this IFileSystem fileSystem, UPath path, string searchPattern = "*")
        {
            if (searchPattern == null) throw new ArgumentNullException(nameof(searchPattern));
            return EnumerateDirectoryEntries(fileSystem, path, searchPattern, SearchOption.TopDirectoryOnly);
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="DirectoryEntry"/> that match a search pattern in a specified path.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The path of the directory to look for directories.</param>
        /// <param name="searchPattern">The search string to match against the names of directories in path. This parameter can contain a combination 
        /// of valid literal path and wildcard (* and ?) characters (see Remarks), but doesn't support regular expressions.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory 
        /// or should include all subdirectories.
        /// The default value is TopDirectoryOnly.</param>
        /// <returns>An enumerable collection of <see cref="DirectoryEntry"/> from the specified path.</returns>
        public static IEnumerable<DirectoryEntry> EnumerateDirectoryEntries(this IFileSystem fileSystem, UPath path, string searchPattern, SearchOption searchOption)
        {
            if (searchPattern == null) throw new ArgumentNullException(nameof(searchPattern));
            foreach (var subPath in fileSystem.EnumerateDirectories(path, searchPattern, searchOption))
            {
                yield return new DirectoryEntry(fileSystem, subPath);
            }
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="FileSystemEntry"/> that match a search pattern in a specified path.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The path of the directory to look for files and directories.</param>
        /// <param name="searchPattern">The search string to match against the names of directories in path. This parameter can contain a combination 
        /// of valid literal path and wildcard (* and ?) characters (see Remarks), but doesn't support regular expressions.</param>
        /// <returns>An enumerable collection of <see cref="FileSystemEntry"/> that match a search pattern in a specified path.</returns>
        public static IEnumerable<FileSystemEntry> EnumerateFileSystemEntries(this IFileSystem fileSystem, UPath path, string searchPattern = "*")
        {
            if (searchPattern == null) throw new ArgumentNullException(nameof(searchPattern));
            return EnumerateFileSystemEntries(fileSystem, path, searchPattern, SearchOption.TopDirectoryOnly);
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="FileSystemEntry"/> that match a search pattern in a specified path.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The path of the directory to look for files and directories.</param>
        /// <param name="searchPattern">The search string to match against the names of directories in path. This parameter can contain a combination 
        /// of valid literal path and wildcard (* and ?) characters (see Remarks), but doesn't support regular expressions.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory 
        /// or should include all subdirectories.
        /// The default value is TopDirectoryOnly.</param>
        /// <param name="searchTarget">The search target either <see cref="SearchTarget.Both"/> or only <see cref="SearchTarget.Directory"/> or <see cref="SearchTarget.File"/>. Default is <see cref="SearchTarget.Both"/></param>
        /// <returns>An enumerable collection of <see cref="FileSystemEntry"/> that match a search pattern in a specified path.</returns>
        public static IEnumerable<FileSystemEntry> EnumerateFileSystemEntries(this IFileSystem fileSystem, UPath path, string searchPattern, SearchOption searchOption, SearchTarget searchTarget = SearchTarget.Both)
        {
            if (searchPattern == null) throw new ArgumentNullException(nameof(searchPattern));
            foreach (var subPath in fileSystem.EnumeratePaths(path, searchPattern, searchOption, searchTarget))
            {
                yield return fileSystem.DirectoryExists(subPath) ? (FileSystemEntry) new DirectoryEntry(fileSystem, subPath) : new FileEntry(fileSystem, subPath);
            }
        }

        /// <summary>
        /// Gets a <see cref="FileSystemEntry"/> for the specified path. If the file or directory does not exist, throws a <see cref="FileNotFoundException"/>
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The file or directory path.</param>
        /// <returns>A new <see cref="FileSystemEntry"/> from the specified path.</returns>
        public static FileSystemEntry GetFileSystemEntry(this IFileSystem fileSystem, UPath path)
        {
            var fileExists = fileSystem.FileExists(path);
            if (fileExists)
            {
                return new FileEntry(fileSystem, path);
            }
            var directoryExists = fileSystem.DirectoryExists(path);
            if (directoryExists)
            {
                return new DirectoryEntry(fileSystem, path);
            }

            throw NewFileNotFoundException(path);
        }

        /// <summary>
        /// Tries to get a <see cref="FileSystemEntry"/> for the specified path. If the file or directory does not exist, returns null.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The file or directory path.</param>
        /// <returns>A new <see cref="FileSystemEntry"/> from the specified path.</returns>
        public static FileSystemEntry TryGetFileSystemEntry(this IFileSystem fileSystem, UPath path)
        {
            var fileExists = fileSystem.FileExists(path);
            if (fileExists)
            {
                return new FileEntry(fileSystem, path);
            }
            var directoryExists = fileSystem.DirectoryExists(path);
            return directoryExists ? new DirectoryEntry(fileSystem, path) : null;
        }

        /// <summary>
        /// Gets a <see cref="FileEntry"/> for the specified path. If the file does not exist, throws a <see cref="FileNotFoundException"/>
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="filePath">The file path.</param>
        /// <returns>A new <see cref="FileEntry"/> from the specified path.</returns>
        public static FileEntry GetFileEntry(this IFileSystem fileSystem, UPath filePath)
        {
            if (!fileSystem.FileExists(filePath))
            {
                throw NewFileNotFoundException(filePath);
            }
            return new FileEntry(fileSystem, filePath);
        }

        /// <summary>
        /// Gets a <see cref="DirectoryEntry"/> for the specified path. If the file does not exist, throws a <see cref="DirectoryNotFoundException"/>
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="directoryPath">The directory path.</param>
        /// <returns>A new <see cref="DirectoryEntry"/> from the specified path.</returns>
        public static DirectoryEntry GetDirectoryEntry(this IFileSystem fileSystem, UPath directoryPath)
        {
            if (!fileSystem.DirectoryExists(directoryPath))
            {
                throw NewDirectoryNotFoundException(directoryPath);
            }
            return new DirectoryEntry(fileSystem, directoryPath);
        }

        /// <summary>
        /// Tries to watch the specified path. If watching the file system or path is not supported, returns null.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The path to watch for changes.</param>
        /// <returns>An <see cref="IFileSystemWatcher"/> instance or null if not supported.</returns>
        public static IFileSystemWatcher TryWatch(this IFileSystem fileSystem, UPath path)
        {
            return !fileSystem.CanWatch(path) ? null : fileSystem.Watch(path);
        }
    }
}