// Copyright (c) Alexandre Mutel. All rights reserved.
// This file is licensed under the BSD-Clause 2 license. 
// See the license.txt file in the project root for more information.
using System;
using System.Collections.Generic;
using System.IO;

namespace Zio.FileSystems
{
    /// <summary>
    /// Provides an abstract base <see cref="IFileSystem"/> for composing a filesystem with another FileSystem. 
    /// This implementation delegates by default its implementation to the filesystem passed to the constructor.
    /// </summary>
    public abstract class ComposeFileSystem : FileSystem
    {
        protected bool Owned { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComposeFileSystem"/> class.
        /// </summary>
        /// <param name="fileSystem">The delegated file system (can be null).</param>
        /// <param name="owned">True if <paramref name="fileSystem"/> should be disposed when this instance is disposed.</param>
        protected ComposeFileSystem(IFileSystem fileSystem, bool owned = true)
        {
            this.NextFileSystem = fileSystem;
            this.Owned = owned;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.Owned)
            {
                this.NextFileSystem?.Dispose();
            }
        }

        /// <summary>
        /// Gets the next delegated file system (may be null).
        /// </summary>
        protected IFileSystem NextFileSystem { get; }

        /// <summary>
        /// Gets the next delegated file system or throws an error if it is null.
        /// </summary>
        protected IFileSystem NextFileSystemSafe
        {
            get
            {
                if (this.NextFileSystem == null)
                {
                    throw new InvalidOperationException("The delegate filesystem for this instance is null");
                }
                return this.NextFileSystem;
            }
        }

        // ----------------------------------------------
        // Directory API
        // ----------------------------------------------

        /// <inheritdoc />
        protected override void CreateDirectoryImpl(UPath path)
        {
            this.NextFileSystemSafe.CreateDirectory(this.ConvertPathToDelegate(path));
        }

        /// <inheritdoc />
        protected override bool DirectoryExistsImpl(UPath path)
        {
            return this.NextFileSystemSafe.DirectoryExists(this.ConvertPathToDelegate(path));
        }

        /// <inheritdoc />
        protected override void MoveDirectoryImpl(UPath srcPath, UPath destPath)
        {
            this.NextFileSystemSafe.MoveDirectory(this.ConvertPathToDelegate(srcPath), this.ConvertPathToDelegate(destPath));
        }

        /// <inheritdoc />
        protected override void DeleteDirectoryImpl(UPath path, bool isRecursive)
        {
            this.NextFileSystemSafe.DeleteDirectory(this.ConvertPathToDelegate(path), isRecursive);
        }

        // ----------------------------------------------
        // File API
        // ----------------------------------------------

        /// <inheritdoc />
        protected override void CopyFileImpl(UPath srcPath, UPath destPath, bool overwrite)
        {
            this.NextFileSystemSafe.CopyFile(this.ConvertPathToDelegate(srcPath), this.ConvertPathToDelegate(destPath), overwrite);
        }

        /// <inheritdoc />
        protected override void ReplaceFileImpl(UPath srcPath, UPath destPath, UPath destBackupPath,
            bool ignoreMetadataErrors)
        {
            this.NextFileSystemSafe.ReplaceFile(this.ConvertPathToDelegate(srcPath), this.ConvertPathToDelegate(destPath), destBackupPath.IsNull ? destBackupPath : this.ConvertPathToDelegate(destBackupPath), ignoreMetadataErrors);
        }

        /// <inheritdoc />
        protected override long GetFileLengthImpl(UPath path)
        {
            return this.NextFileSystemSafe.GetFileLength(this.ConvertPathToDelegate(path));
        }

        /// <inheritdoc />
        protected override bool FileExistsImpl(UPath path)
        {
            return this.NextFileSystemSafe.FileExists(this.ConvertPathToDelegate(path));
        }

        /// <inheritdoc />
        protected override void MoveFileImpl(UPath srcPath, UPath destPath)
        {
            this.NextFileSystemSafe.MoveFile(this.ConvertPathToDelegate(srcPath), this.ConvertPathToDelegate(destPath));
        }

        /// <inheritdoc />
        protected override void DeleteFileImpl(UPath path)
        {
            this.NextFileSystemSafe.DeleteFile(this.ConvertPathToDelegate(path));
        }

        /// <inheritdoc />
        protected override Stream OpenFileImpl(UPath path, FileMode mode, FileAccess access, FileShare share = FileShare.None)
        {
            return this.NextFileSystemSafe.OpenFile(this.ConvertPathToDelegate(path), mode, access, share);
        }

        // ----------------------------------------------
        // Metadata API
        // ----------------------------------------------

        /// <inheritdoc />
        protected override FileAttributes GetAttributesImpl(UPath path)
        {
            return this.NextFileSystemSafe.GetAttributes(this.ConvertPathToDelegate(path));
        }

        /// <inheritdoc />
        protected override void SetAttributesImpl(UPath path, FileAttributes attributes)
        {
            this.NextFileSystemSafe.SetAttributes(this.ConvertPathToDelegate(path), attributes);
        }

        /// <inheritdoc />
        protected override DateTime GetCreationTimeImpl(UPath path)
        {
            return this.NextFileSystemSafe.GetCreationTime(this.ConvertPathToDelegate(path));
        }

        /// <inheritdoc />
        protected override void SetCreationTimeImpl(UPath path, DateTime time)
        {
            this.NextFileSystemSafe.SetCreationTime(this.ConvertPathToDelegate(path), time);
        }

        /// <inheritdoc />
        protected override DateTime GetLastAccessTimeImpl(UPath path)
        {
            return this.NextFileSystemSafe.GetLastAccessTime(this.ConvertPathToDelegate(path));
        }

        /// <inheritdoc />
        protected override void SetLastAccessTimeImpl(UPath path, DateTime time)
        {
            this.NextFileSystemSafe.SetLastAccessTime(this.ConvertPathToDelegate(path), time);
        }

        /// <inheritdoc />
        protected override DateTime GetLastWriteTimeImpl(UPath path)
        {
            return this.NextFileSystemSafe.GetLastWriteTime(this.ConvertPathToDelegate(path));
        }

        /// <inheritdoc />
        protected override void SetLastWriteTimeImpl(UPath path, DateTime time)
        {
            this.NextFileSystemSafe.SetLastWriteTime(this.ConvertPathToDelegate(path), time);
        }

        // ----------------------------------------------
        // Search API
        // ----------------------------------------------

        /// <inheritdoc />
        protected override IEnumerable<UPath> EnumeratePathsImpl(UPath path, string searchPattern, SearchOption searchOption, SearchTarget searchTarget)
        {
            foreach (var subPath in this.NextFileSystemSafe.EnumeratePaths(this.ConvertPathToDelegate(path), searchPattern, searchOption, searchTarget))
            {
                yield return this.ConvertPathFromDelegate(subPath);
            }
        }

        // ----------------------------------------------
        // Watch API
        // ----------------------------------------------
        
        /// <inheritdoc />
        protected override bool CanWatchImpl(UPath path)
        {
            return this.NextFileSystemSafe.CanWatch(this.ConvertPathToDelegate(path));
        }

        /// <inheritdoc />
        protected override IFileSystemWatcher WatchImpl(UPath path)
        {
            return this.NextFileSystemSafe.Watch(this.ConvertPathToDelegate(path));
        }

        // ----------------------------------------------
        // Path API
        // ----------------------------------------------

        /// <inheritdoc />
        protected override string ConvertPathToInternalImpl(UPath path)
        {
            return this.NextFileSystemSafe.ConvertPathToInternal(this.ConvertPathToDelegate(path));
        }

        /// <inheritdoc />
        protected override UPath ConvertPathFromInternalImpl(string innerPath)
        {
            return this.ConvertPathFromDelegate(this.NextFileSystemSafe.ConvertPathFromInternal(innerPath));
        }

        /// <summary>
        /// Converts the specified path to the path supported by the underlying <see cref="NextFileSystem"/>
        /// </summary>
        /// <param name="path">The path exposed by this filesystem</param>
        /// <returns>A new path translated to the delegate path</returns>
        protected abstract UPath ConvertPathToDelegate(UPath path);

        /// <summary>
        /// Converts the specified delegate path to the path exposed by this filesystem.
        /// </summary>
        /// <param name="path">The path used by the underlying <see cref="NextFileSystem"/></param>
        /// <returns>A new path translated to this filesystem</returns>
        protected abstract UPath ConvertPathFromDelegate(UPath path);
    }
}