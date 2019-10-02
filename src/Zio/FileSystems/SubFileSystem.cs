﻿// Copyright (c) Alexandre Mutel. All rights reserved.
// This file is licensed under the BSD-Clause 2 license. 
// See the license.txt file in the project root for more information.
using System;
using System.IO;
using static Zio.FileSystemExceptionHelper;

namespace Zio.FileSystems
{
    /// <summary>
    /// Provides a secure view on a sub folder of another delegate <see cref="IFileSystem"/>
    /// </summary>
    public class SubFileSystem : ComposeFileSystem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubFileSystem"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system to create a view from.</param>
        /// <param name="subPath">The sub path view to create filesystem.</param>
        /// <param name="owned">True if <paramref name="fileSystem"/> should be disposed when this instance is disposed.</param>
        /// <exception cref="DirectoryNotFoundException">If the directory subPath does not exist in the delegate FileSystem</exception>
        public SubFileSystem(IFileSystem fileSystem, UPath subPath, bool owned = true) : base(fileSystem, owned)
        {
            this.SubPath = subPath.AssertAbsolute(nameof(subPath));
            if (!fileSystem.DirectoryExists(this.SubPath))
            {
                throw NewDirectoryNotFoundException(this.SubPath);
            }
        }

        /// <summary>
        /// Gets the sub path relative to the delegate <see cref="ComposeFileSystem.NextFileSystem"/>
        /// </summary>
        public UPath SubPath { get; }

        /// <inheritdoc />
        protected override IFileSystemWatcher WatchImpl(UPath path)
        {
            var delegateWatcher = base.WatchImpl(path);
            return new Watcher(this, path, delegateWatcher);
        }

        private class Watcher : WrapFileSystemWatcher
        {
            private readonly SubFileSystem _fileSystem;

            public Watcher(SubFileSystem fileSystem, UPath path, IFileSystemWatcher watcher)
                : base(fileSystem, path, watcher)
            {
                this._fileSystem = fileSystem;
            }

            protected override UPath? TryConvertPath(UPath pathFromEvent)
            {
                if (!pathFromEvent.IsInDirectory(this._fileSystem.SubPath, true))
                {
                    return null;
                }

                return this._fileSystem.ConvertPathFromDelegate(pathFromEvent);
            }
        }

        /// <inheritdoc />
        protected override UPath ConvertPathToDelegate(UPath path)
        {
            var safePath = path.ToRelative();
            return this.SubPath / safePath;
        }

        /// <inheritdoc />
        protected override UPath ConvertPathFromDelegate(UPath path)
        {
            var fullPath = path.FullName;
            if (!fullPath.StartsWith(this.SubPath.FullName) || (fullPath.Length > this.SubPath.FullName.Length && fullPath[this.SubPath.FullName.Length] != UPath.DirectorySeparator))
            {
                // More a safe guard, as it should never happen, but if a delegate filesystem doesn't respect its root path
                // we are throwing an exception here
                throw new InvalidOperationException($"The path `{path}` returned by the delegate filesystem is not rooted to the subpath `{this.SubPath}`");
            }

            var subPath = fullPath.Substring(this.SubPath.FullName.Length);
            return subPath == string.Empty ? UPath.Root : new UPath(subPath, true);
        }
    }
}