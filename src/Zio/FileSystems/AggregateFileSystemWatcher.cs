using System;
using System.Collections.Generic;

namespace Zio.FileSystems
{
    /// <summary>
    /// Aggregates events from multiple <see cref="FileSystemWatcher"/> into one.
    /// </summary>
    public class AggregateFileSystemWatcher : FileSystemWatcher
    {
        private readonly List<IFileSystemWatcher> _children;
        private int _internalBufferSize;
        private NotifyFilters _notifyFilter;
        private bool _enableRaisingEvents;
        private bool _includeSubdirectories;
        private string _filter;

        public AggregateFileSystemWatcher(IFileSystem fileSystem, UPath path)
            : base(fileSystem, path)
        {
            this._children = new List<IFileSystemWatcher>();
            this._internalBufferSize = 0;
            this._notifyFilter = NotifyFilters.Default;
            this._enableRaisingEvents = false;
            this._includeSubdirectories = false;
            this._filter = "*.*";
        }

        /// <summary>
        /// Adds an <see cref="IFileSystemWatcher"/> instance to aggregate events from.
        /// </summary>
        /// <param name="watcher">The <see cref="IFileSystemWatcher"/> instance to add.</param>
        public void Add(IFileSystemWatcher watcher)
        {
            if (watcher == null)
            {
                throw new ArgumentNullException(nameof(watcher));
            }

            lock (this._children)
            {
                if (this._children.Contains(watcher))
                {
                    throw new ArgumentException("The filesystem watcher is already added", nameof(watcher));
                }

                watcher.InternalBufferSize = this.InternalBufferSize;
                watcher.NotifyFilter = this.NotifyFilter;
                watcher.EnableRaisingEvents = this.EnableRaisingEvents;
                watcher.IncludeSubdirectories = this.IncludeSubdirectories;
                watcher.Filter = this.Filter;

                this.RegisterEvents(watcher);
                this._children.Add(watcher);
            }
        }

        /// <summary>
        /// Removes <see cref="IFileSystemWatcher"/> instances from this instance.
        /// </summary>
        /// <param name="fileSystem">The <see cref="IFileSystem"/> to stop aggregating events from.</param>
        public void RemoveFrom(IFileSystem fileSystem)
        {
            if (fileSystem == null)
            {
                throw new ArgumentNullException(nameof(fileSystem));
            }

            lock (this._children)
            {
                for (var i = this._children.Count - 1; i >= 0; i--)
                {
                    var watcher = this._children[i];
                    if (watcher.FileSystem != fileSystem)
                    {
                        continue;
                    }

                    this.UnregisterEvents(watcher);
                    this._children.RemoveAt(i);
                    watcher.Dispose();
                }
            }
        }

        /// <summary>
        /// Removes all <see cref="IFileSystemWatcher"/> instances from this instance.
        /// </summary>
        /// <param name="excludeFileSystem">Exclude this filesystem from removal.</param>
        public void Clear(IFileSystem excludeFileSystem = null)
        {
            lock (this._children)
            {
                for (var i = this._children.Count - 1; i >= 0; i--)
                {
                    var watcher = this._children[i];
                    if (watcher.FileSystem == excludeFileSystem)
                    {
                        continue;
                    }

                    this.UnregisterEvents(watcher);
                    this._children.RemoveAt(i);
                    watcher.Dispose();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Clear();
            }
        }

        /// <inheritdoc />
        public override int InternalBufferSize
        {
            get => this._internalBufferSize;
            set
            {
                if (value == this._internalBufferSize)
                {
                    return;
                }

                lock (this._children)
                {
                    foreach (var watcher in this._children)
                    {
                        watcher.InternalBufferSize = value;
                    }
                }

                this._internalBufferSize = value;
            }
        }

        /// <inheritdoc />
        public override NotifyFilters NotifyFilter
        {
            get => this._notifyFilter;
            set
            {
                if (value == this._notifyFilter)
                {
                    return;
                }

                lock (this._children)
                {
                    foreach (var watcher in this._children)
                    {
                        watcher.NotifyFilter = value;
                    }
                }

                this._notifyFilter = value;
            }
        }

        /// <inheritdoc />
        public override bool EnableRaisingEvents
        {
            get => this._enableRaisingEvents;
            set
            {
                if (value == this._enableRaisingEvents)
                {
                    return;
                }

                lock (this._children)
                {
                    foreach (var watcher in this._children)
                    {
                        watcher.EnableRaisingEvents = value;
                    }
                }

                this._enableRaisingEvents = value;
            }
        }

        /// <inheritdoc />
        public override bool IncludeSubdirectories
        {
            get => this._includeSubdirectories;
            set
            {
                if (value == this._includeSubdirectories)
                {
                    return;
                }

                lock (this._children)
                {
                    foreach (var watcher in this._children)
                    {
                        watcher.IncludeSubdirectories = value;
                    }
                }

                this._includeSubdirectories = value;
            }
        }

        /// <inheritdoc />
        public override string Filter
        {
            get => this._filter;
            set
            {
                if (value == this._filter)
                {
                    return;
                }

                lock (this._children)
                {
                    foreach (var watcher in this._children)
                    {
                        watcher.Filter = value;
                    }
                }

                this._filter = value;
            }
        }
    }
}
