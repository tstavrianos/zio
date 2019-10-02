// Copyright (c) Alexandre Mutel. All rights reserved.
// This file is licensed under the BSD-Clause 2 license.
// See the license.txt file in the project root for more information.

using System.IO;
using System.Text;

namespace Zio
{
    /// <summary>
    /// Similar to <see cref="FileInfo"/> but to use with <see cref="IReadOnlyFileSystem"/>, provides properties and instance methods 
    /// for the creation, copying, deletion, moving, and opening of files, and aids in the creation of FileStream objects. 
    /// Note that unlike <see cref="FileInfo"/>, this class doesn't cache any data.
    /// </summary>
    public sealed class ReadOnlyFileEntry : ReadOnlyFileSystemEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyFileEntry"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The file path.</param>
        public ReadOnlyFileEntry(IReadOnlyFileSystem fileSystem, UPath path) : base(fileSystem, path)
        {
        }

        /// <summary>Gets an instance of the parent directory.</summary>
        /// <returns>A <see cref="DirectoryEntry" /> object representing the parent directory of this file.</returns>
        /// <exception cref="DirectoryNotFoundException">
        ///     The specified path is invalid, such as being on an unmapped
        ///     drive.
        /// </exception>
        public ReadOnlyDirectoryEntry Directory => this.Parent;

        /// <summary>Gets or sets a value that determines if the current file is read only.</summary>
        /// <returns>true if the current file is read only; otherwise, false.</returns>
        /// <exception cref="T:System.IO.FileNotFoundException">
        ///     The file described by the current
        ///     <see cref="T:System.IO.FileInfo" /> object could not be found.
        /// </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
        /// <exception cref="T:System.UnauthorizedAccessException">
        ///     This operation is not supported on the current platform.-or- The
        ///     caller does not have the required permission.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        ///     The user does not have write permission, but attempted to set this
        ///     property to false.
        /// </exception>
        public bool IsReadOnly => (this.FileSystem.GetAttributes(this.Path) & FileAttributes.ReadOnly) != 0;

        /// <summary>Gets the size, in bytes, of the current file.</summary>
        /// <returns>The size of the current file in bytes.</returns>
        /// <exception cref="T:System.IO.IOException">
        ///     <see cref="M:System.IO.FileSystemInfo.Refresh" /> cannot update the state of the file or directory.
        /// </exception>
        /// <exception cref="T:System.IO.FileNotFoundException">
        ///     The file does not exist.-or- The Length property is called for a
        ///     directory.
        /// </exception>
        public long Length => this.FileSystem.GetFileLength(this.Path);

        /// <summary>Opens a file in the specified mode with read access.</summary>
        /// <returns>A <see cref="T:System.IO.FileStream" /> object create with open mode, read access and read sharing.</returns>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        /// <exception cref="T:System.IO.FileNotFoundException">The file is not found. </exception>
        /// <exception cref="T:System.UnauthorizedAccessException">
        ///     The path is read-only or is a directory.
        /// </exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">
        ///     The specified path is invalid, such as being on an unmapped
        ///     drive.
        /// </exception>
        /// <exception cref="T:System.IO.IOException">The file is already open. </exception>
        public Stream OpenRead()
        {
            return this.FileSystem.OpenRead(this.Path);
        }

        /// <summary>
        ///     Opens a file, reads all lines of the file with the specified encoding, and then closes the file.
        /// </summary>
        /// <returns>A string containing all lines of the file.</returns>
        /// <remarks>
        ///     This method attempts to automatically detect the encoding of a file based on the presence of byte order marks.
        ///     Encoding formats UTF-8 and UTF-32 (both big-endian and little-endian) can be detected.
        /// </remarks>
        public string ReadAllText()
        {
            return this.FileSystem.ReadAllText(this.Path);
        }

        /// <summary>
        ///     Opens a file, reads all lines of the file with the specified encoding, and then closes the file.
        /// </summary>
        /// <param name="encoding">The encoding to use to decode the text from <see cref="Path" />.</param>
        /// <returns>A string containing all lines of the file.</returns>
        public string ReadAllText(Encoding encoding)
        {
            return this.FileSystem.ReadAllText(this.Path, encoding);
        }

        /// <summary>
        ///     Opens a file, reads all lines of the file with the specified encoding, and then closes the file.
        /// </summary>
        /// <returns>An array of strings containing all lines of the file.</returns>
        public string[] ReadAllLines()
        {
            return this.FileSystem.ReadAllLines(this.Path);
        }

        /// <summary>
        ///     Opens a file, reads all lines of the file with the specified encoding, and then closes the file.
        /// </summary>
        /// <param name="encoding">The encoding to use to decode the text from <paramref name="path" />.</param>
        /// <remarks>
        ///     This method attempts to automatically detect the encoding of a file based on the presence of byte order marks.
        ///     Encoding formats UTF-8 and UTF-32 (both big-endian and little-endian) can be detected.
        /// </remarks>
        /// <returns>An array of strings containing all lines of the file.</returns>
        public string[] ReadAllLines(Encoding encoding)
        {
            return this.FileSystem.ReadAllLines(this.Path, encoding);
        }

        /// <summary>
        ///     Opens a binary file, reads the contents of the file into a byte array, and then closes the file.
        /// </summary>
        /// <returns>A byte array containing the contents of the file.</returns>
        public byte[] ReadAllBytes()
        {
            return this.FileSystem.ReadAllBytes(this.Path);
        }

        /// <inheritdoc />
        public override bool Exists => this.FileSystem.FileExists(this.Path);
    }
}