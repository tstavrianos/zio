// Copyright (c) Alexandre Mutel. All rights reserved.
// This file is licensed under the BSD-Clause 2 license.
// See the license.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Zio.FileSystems
{
    public sealed class ZipFileSystem : IReadOnlyFileSystem
    {
        private readonly ZipArchive archive;
        private readonly bool ownArchive;

        public ZipFileSystem(ZipArchive archive, bool ownArchive = true)
        {
            this.archive = archive;
            this.ownArchive = ownArchive;
        }

        private static string ToEntryPath(UPath path) => path.FullName.TrimStart('/');

        private ZipArchiveEntry ToEntry(UPath path) => this.archive.GetEntry(ToEntryPath(path));

        private static UPath ToPath(ZipArchiveEntry entry) => new UPath('/' + entry.FullName);

        private IEnumerable<ZipArchiveEntry> GetZipEntries() => this.archive.Entries;

        public void Dispose()
        {
            if (this.ownArchive) this.archive.Dispose();
        }

        public bool DirectoryExists(UPath path) => this.GetZipEntries().Select(ToPath).Any(entryPath => entryPath.IsInDirectory(path, false));

        public long GetFileLength(UPath path) => this.ToEntry(path).Length;

        public bool FileExists(UPath path) => this.ToEntry(path) != null;

        public Stream OpenRead(UPath path) => this.ToEntry(path).Open();

        public FileAttributes GetAttributes(UPath path) => (FileAttributes) this.ToEntry(path).ExternalAttributes;

        public DateTime GetCreationTime(UPath path) => this.ToEntry(path).LastWriteTime.LocalDateTime;

        public DateTime GetLastAccessTime(UPath path) => this.ToEntry(path).LastWriteTime.LocalDateTime;

        public DateTime GetLastWriteTime(UPath path) => this.ToEntry(path).LastWriteTime.LocalDateTime;

        public IEnumerable<UPath> EnumeratePaths(UPath path, string searchPattern, SearchOption searchOption,
            SearchTarget searchTarget)
        {
            var search = SearchPattern.Parse(ref path, ref searchPattern);

            var hashset = new HashSet<UPath>();
            // ReSharper disable once LoopCanBePartlyConvertedToQuery
            foreach (var entry in this.archive.Entries)
            {
                var p = new UPath('/' + entry.FullName);
                if (searchTarget == SearchTarget.Both || searchTarget == SearchTarget.File)
                {
                    if (p.IsInDirectory(path, searchOption == SearchOption.AllDirectories) && search.Match(p))
                    {
                        hashset.Add(p);
                    }
                }

                if (searchTarget != SearchTarget.Both && searchTarget != SearchTarget.Directory) continue;
                p = p.GetDirectory();
                if (p.IsInDirectory(path, searchOption == SearchOption.AllDirectories) && search.Match(p))
                {
                    hashset.Add(p);
                }
            }

            return hashset;
        }

        public string ConvertPathToInternal(UPath path) => '/' + path.FullName;

        public UPath ConvertPathFromInternal(string systemPath) => new UPath('/' + systemPath);
    }
}