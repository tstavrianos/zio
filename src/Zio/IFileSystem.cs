// Copyright (c) Alexandre Mutel. All rights reserved.
// This file is licensed under the BSD-Clause 2 license. 
// See the license.txt file in the project root for more information.

using System;
using System.IO;

namespace Zio
{
    /// <summary>
    /// Interface of a FileSystem.
    /// </summary>
    public interface IFileSystem : IReadOnlyFileSystem
    {
        // ----------------------------------------------
        // Directory API
        // ----------------------------------------------

        /// <summary>
        /// Creates all directories and subdirectories in the specified path unless they already exist.
        /// </summary>
        /// <param name="path">The directory to create.</param>
        void CreateDirectory(UPath path);

        /// <summary>
        /// Moves a directory and its contents to a new location.
        /// </summary>
        /// <param name="srcPath">The path of the directory to move.</param>
        /// <param name="destPath">The path to the new location for <paramref name="srcPath"/></param>
        void MoveDirectory(UPath srcPath, UPath destPath);

        /// <summary>
        /// Deletes the specified directory and, if indicated, any subdirectories and files in the directory. 
        /// </summary>
        /// <param name="path">The path of the directory to remove.</param>
        /// <param name="isRecursive"><c>true</c> to remove directories, subdirectories, and files in path; otherwise, <c>false</c>.</param>
        void DeleteDirectory(UPath path, bool isRecursive);

        // ----------------------------------------------
        // File API
        // ----------------------------------------------

        /// <summary>
        /// Copies an existing file to a new file. Overwriting a file of the same name is allowed.
        /// </summary>
        /// <param name="srcPath">The path of the file to copy.</param>
        /// <param name="destPath">The path of the destination file. This cannot be a directory.</param>
        /// <param name="overwrite"><c>true</c> if the destination file can be overwritten; otherwise, <c>false</c>.</param>
        void CopyFile(UPath srcPath, UPath destPath, bool overwrite);

        /// <summary>
        /// Replaces the contents of a specified file with the contents of another file, deleting the original file, and creating a backup of the replaced file and optionally ignores merge errors.
        /// </summary>
        /// <param name="srcPath">The path of a file that replaces the file specified by <paramref name="destPath"/>.</param>
        /// <param name="destPath">The path of the file being replaced.</param>
        /// <param name="destBackupPath">The path of the backup file (maybe null, in that case, it doesn't create any backup)</param>
        /// <param name="ignoreMetadataErrors"><c>true</c> to ignore merge errors (such as attributes and access control lists (ACLs)) from the replaced file to the replacement file; otherwise, <c>false</c>.</param>
        void ReplaceFile(UPath srcPath, UPath destPath, UPath destBackupPath, bool ignoreMetadataErrors);

        /// <summary>
        /// Moves a specified file to a new location, providing the option to specify a new file name.
        /// </summary>
        /// <param name="srcPath">The path of the file to move.</param>
        /// <param name="destPath">The new path and name for the file.</param>
        void MoveFile(UPath srcPath, UPath destPath);

        /// <summary>
        /// Deletes the specified file. 
        /// </summary>
        /// <param name="path">The path of the file to be deleted.</param>
        void DeleteFile(UPath path);

        /// <summary>
        /// Opens a file <see cref="Stream"/> on the specified path, having the specified mode with read, write, or read/write access and the specified sharing option.
        /// </summary>
        /// <param name="path">The path to the file to open.</param>
        /// <param name="mode">A <see cref="FileMode"/> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.</param>
        /// <param name="access">A <see cref="FileAccess"/> value that specifies the operations that can be performed on the file.</param>
        /// <param name="share">A <see cref="FileShare"/> value specifying the type of access other threads have to the file.</param>
        /// <returns>A file <see cref="Stream"/> on the specified path, having the specified mode with read, write, or read/write access and the specified sharing option.</returns>
        Stream OpenFile(UPath path, FileMode mode, FileAccess access, FileShare share = FileShare.None);

        // ----------------------------------------------
        // Metadata API
        // ----------------------------------------------

        /// <summary>
        /// Sets the specified <see cref="FileAttributes"/> of the file or directory on the specified path.
        /// </summary>
        /// <param name="path">The path to the file or directory.</param>
        /// <param name="attributes">A bitwise combination of the enumeration values.</param>
        void SetAttributes(UPath path, FileAttributes attributes);

        /// <summary>
        /// Sets the date and time the file was created.
        /// </summary>
        /// <param name="path">The path to a file or directory for which to set the creation date and time.</param>
        /// <param name="time">A <see cref="DateTime"/> containing the value to set for the creation date and time of path. This value is expressed in local time.</param>
        void SetCreationTime(UPath path, DateTime time);

        /// <summary>
        /// Sets the date and time the file was last accessed.
        /// </summary>
        /// <param name="path">The path to a file or directory for which to set the last access date and time.</param>
        /// <param name="time">A <see cref="DateTime"/> containing the value to set for the last access date and time of path. This value is expressed in local time.</param>
        void SetLastAccessTime(UPath path, DateTime time);

        /// <summary>
        /// Sets the date and time that the specified file was last written to.
        /// </summary>
        /// <param name="path">The path to a file or directory for which to set the last write date and time.</param>
        /// <param name="time">A <see cref="DateTime"/> containing the value to set for the last write date and time of path. This value is expressed in local time.</param>
        void SetLastWriteTime(UPath path, DateTime time);

        // ----------------------------------------------
        // Watch API
        // ----------------------------------------------

        /// <summary>
        /// Checks if the file system and <paramref name="path"/> can be watched with <see cref="Watch"/>.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns>True if the the path can be watched on this file system.</returns>
        bool CanWatch(UPath path);

        /// <summary>
        /// Returns an <see cref="IFileSystemWatcher"/> instance that can be used to watch for changes to files and directories in the given path. The instance must be
        /// configured before events are raised.
        /// </summary>
        /// <param name="path">The path to watch for changes.</param>
        /// <returns>An <see cref="IFileSystemWatcher"/> instance that watches the given path.</returns>
        IFileSystemWatcher Watch(UPath path);

        // ----------------------------------------------
        // General API
        // ----------------------------------------------
        
        /// <summary>
        /// Return this filesystem as a read-only variant.
        /// </summary>
        /// <remarks>All implementations should return the 'this' object.</remarks>
        /// <returns>this</returns>
        IReadOnlyFileSystem AsReadOnly();
    }
}