// Copyright (c) Alexandre Mutel. All rights reserved.
// This file is licensed under the BSD-Clause 2 license. 
// See the license.txt file in the project root for more information.
using System;
using System.IO;

namespace Zio
{
    /// <summary>
    /// Similar to <see cref="FileSystemInfo"/> but to use with <see cref="IFileSystem"/>, provides the base class 
    /// for both <see cref="FileEntry"/> and <see cref="DirectoryEntry"/> objects.
    /// </summary>
    public abstract class FileSystemEntry : IEquatable<FileSystemEntry>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemEntry"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The path to the file or directory.</param>
        /// <exception cref="System.ArgumentNullException">fileSystem</exception>
        protected FileSystemEntry(IFileSystem fileSystem, UPath path)
        {
            this.FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            path.AssertAbsolute();
            this.Path = path;
        }

        /// <summary>
        /// Gets the path of this entry.
        /// </summary>
        public UPath Path { get; }

        /// <summary>
        /// Gets the file system used by this entry.
        /// </summary>
        public IFileSystem FileSystem { get; }

        /// <summary>
        /// Gets the full path of the directory or file.
        /// </summary>
        public string FullName => this.Path.FullName;

        /// <summary>
        /// Gets the name of the file or directory (with its extension).
        /// </summary>
        public string Name => this.Path.GetName();

        /// <summary>
        /// Gets the name of the file or directory without its extension.
        /// </summary>
        public string NameWithoutExtension => this.Path.GetNameWithoutExtension();

        /// <summary>
        /// Gets the extension with a leading dot.
        /// </summary>
        public string ExtensionWithDot => this.Path.GetExtensionWithDot();

        /// <summary>
        /// Gets or sets the attributes for the current file or directory
        /// </summary>
        public FileAttributes Attributes
        {
            get => this.FileSystem.GetAttributes(this.Path);

            set => this.FileSystem.SetAttributes(this.Path, value);
        }

        /// <summary>
        /// Gets a value indicating whether this file or directory exists.
        /// </summary>
        /// <value><c>true</c> if this file or directory exists; otherwise, <c>false</c>.</value>
        public abstract bool Exists { get; }

        /// <summary>
        /// Gets or sets the creation time of the current file or directory.
        /// </summary>
        public DateTime CreationTime
        {
            get => this.FileSystem.GetCreationTime(this.Path);
            set => this.FileSystem.SetCreationTime(this.Path, value);
        }

        /// <summary>
        /// Gets or sets the last access time of the current file or directory.
        /// </summary>
        public DateTime LastAccessTime
        {
            get => this.FileSystem.GetLastAccessTime(this.Path);
            set => this.FileSystem.SetLastAccessTime(this.Path, value);
        }

        /// <summary>
        /// Gets or sets the last write time of the current file or directory.
        /// </summary>
        public DateTime LastWriteTime
        {
            get => this.FileSystem.GetLastWriteTime(this.Path);
            set => this.FileSystem.SetLastWriteTime(this.Path, value);
        }

        /// <summary>Gets an instance of the parent directory.</summary>
        /// <returns>A <see cref="DirectoryEntry" /> object representing the parent directory of this file.</returns>
        /// <exception cref="DirectoryNotFoundException">
        ///     The specified path is invalid, such as being on an unmapped
        ///     drive.
        /// </exception>
        public DirectoryEntry Parent => this.Path == UPath.Root ? null : new DirectoryEntry(this.FileSystem, this.Path / "..");

        /// <summary>
        /// Deletes a file or directory.
        /// </summary>
        public abstract void Delete();

        /// <summary>
        /// Returns the <see cref="FullName"/> of this instance.
        /// </summary>
        /// <returns>The <see cref="FullName"/> of this instance.</returns>
        public override string ToString()
        {
            return this.Path.FullName;
        }

        /// <inheritdoc />
        public bool Equals(FileSystemEntry other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return this.Path.Equals(other.Path) && this.FileSystem.Equals(other.FileSystem);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((FileSystemEntry) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (this.Path.GetHashCode() * 397) ^ this.FileSystem.GetHashCode();
            }
        }

        public static bool operator ==(FileSystemEntry left, FileSystemEntry right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(FileSystemEntry left, FileSystemEntry right)
        {
            return !Equals(left, right);
        }
    }
}