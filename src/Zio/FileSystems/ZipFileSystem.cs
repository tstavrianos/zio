﻿#if HAS_ZIPARCHIVE
// Copyright (c) Alexandre Mutel. All rights reserved.
// This file is licensed under the BSD-Clause 2 license. 
// See the license.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Zio.FileSystems
{
    public class ZipFileSystem : FileSystemBase
    {
        private readonly ZipArchive _zipArchive;

        public ZipFileSystem(ZipArchive zipArchive)
        {
            _zipArchive = zipArchive ?? throw new ArgumentNullException(nameof(zipArchive));
        }

        // ----------------------------------------------
        // Directory API
        // ----------------------------------------------

        protected override void CreateDirectoryImpl(PathInfo path)
        {
            _zipArchive.CreateEntry(SafeDirectory(path));
        }

        protected override bool DirectoryExistsImpl(PathInfo path)
        {
            return _zipArchive.GetEntry(SafeDirectory(path)) != null;
        }

        protected override void MoveDirectoryImpl(PathInfo srcPath, PathInfo destPath)
        {
            throw new NotImplementedException();
        }

        protected override void DeleteDirectoryImpl(PathInfo path, bool isRecursive)
        {
            var entry = _zipArchive.GetEntry(SafeDirectory(path));
            entry?.Delete();
        }

        // ----------------------------------------------
        // File API
        // ----------------------------------------------

        protected override void CopyFileImpl(PathInfo srcPath, PathInfo destPath, bool overwrite)
        {
            throw new NotImplementedException();
        }

        protected override void ReplaceFileImpl(PathInfo srcPath, PathInfo destPath, PathInfo destBackupPath, bool ignoreMetadataErrors)
        {
            throw new NotImplementedException();
        }

        protected override long GetFileLengthImpl(PathInfo path)
        {
            var entry = _zipArchive.GetEntry(path.FullName);
            if (entry == null)
            {
                throw new FileNotFoundException($"The file `{path}` was not found in this zip archive");
            }
            return entry.Length;
        }

        protected override bool FileExistsImpl(PathInfo path)
        {
            return _zipArchive.GetEntry(path.FullName) != null;
        }

        protected override void MoveFileImpl(PathInfo srcPath, PathInfo destPath)
        {
            throw new NotImplementedException();
        }

        protected override void DeleteFileImpl(PathInfo path)
        {
            var entry = _zipArchive.GetEntry(path.FullName);
            entry?.Delete();
        }

        private Stream CreateFile(PathInfo path)
        {
            return _zipArchive.CreateEntry(path.FullName).Open();
        }

        protected override Stream OpenFileImpl(PathInfo path, FileMode mode, FileAccess access, FileShare share = FileShare.None)
        {
            var abspath = path.FullName;
            var entry = _zipArchive.GetEntry(abspath);
            switch (mode)
            {
                case FileMode.Create:
                    return entry != null ? entry.Open() : CreateFile(path);

                case FileMode.CreateNew:
                    if (entry == null)
                    {
                        throw new IOException($"The file `{path}` already exists in this zip archive");
                    }
                    return CreateFile(path);

                case FileMode.Open:
                    if (entry == null)
                    {
                        throw new FileNotFoundException($"The file `{path}` was not found in this zip archive");
                    }
                    return entry.Open();

                case FileMode.OpenOrCreate:
                    return entry != null ? entry.Open() : CreateFile(path);

                case FileMode.Append:
                    if (entry == null)
                    {
                        entry = _zipArchive.CreateEntry(abspath);
                    }
                    var appendStream = entry.Open();
                    appendStream.Seek(entry.Length, SeekOrigin.Begin);
                    return appendStream;

                case FileMode.Truncate:
                    if (entry == null)
                    {
                        throw new IOException($"The file `{path}` already exists in this zip archive");
                    }
                    var truncateStream = entry.Open();
                    truncateStream.Write(new byte[0], 0, 0);
                    return truncateStream;
            }
            throw new NotImplementedException();
        }

        // ----------------------------------------------
        // Metadata API
        // ----------------------------------------------

        protected override FileAttributes GetAttributesImpl(PathInfo path)
        {
            throw new NotImplementedException();
        }

        protected override void SetAttributesImpl(PathInfo path, FileAttributes attributes)
        {
            throw new NotImplementedException();
        }

        protected override DateTime GetCreationTimeImpl(PathInfo path)
        {
            throw new NotImplementedException();
        }

        protected override void SetCreationTimeImpl(PathInfo path, DateTime time)
        {
            throw new NotImplementedException();
        }

        protected override DateTime GetLastAccessTimeImpl(PathInfo path)
        {
            throw new NotImplementedException();
        }

        protected override void SetLastAccessTimeImpl(PathInfo path, DateTime time)
        {
            throw new NotImplementedException();
        }

        protected override DateTime GetLastWriteTimeImpl(PathInfo path)
        {
            var entry = _zipArchive.GetEntry(path.FullName);
            if (entry == null)
            {
                throw new FileNotFoundException($"The file `{path}` was not found in this zip archive");
            }
            return entry.LastWriteTime.DateTime;
        }

        protected override void SetLastWriteTimeImpl(PathInfo path, DateTime time)
        {
            var entry = _zipArchive.GetEntry(path.FullName);
            if (entry == null)
            {
                throw new FileNotFoundException($"The file `{path}` was not found in this zip archive");
            }
            entry.LastWriteTime = DateTimeOffset.FromFileTime(time.Ticks);
        }

        // ----------------------------------------------
        // Search API
        // ----------------------------------------------

        protected override IEnumerable<PathInfo> EnumeratePathsImpl(PathInfo path, string searchPattern, SearchOption searchOption, SearchTarget searchTarget)
        {
            var search = SearchPattern.Parse(ref path, ref searchPattern);
            switch (searchTarget)
            {
                case SearchTarget.File:
                    foreach (var entry in _zipArchive.Entries)
                    {
                        var pathInfo = new PathInfo(entry.FullName);
                        if (search.Match(pathInfo))
                        {
                            yield return pathInfo;
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        // ----------------------------------------------
        // Path API
        // ----------------------------------------------

        protected override string ConvertToSystemImpl(PathInfo path)
        {
            return path.FullName;
        }

        protected override PathInfo ConvertFromSystemImpl(string systemPath)
        {
            return new PathInfo(systemPath).AssertAbsolute();
        }

        private static string SafeDirectory(PathInfo path)
        {
            return path.ToRelative().FullName + "/";
        }
    }
}
#endif