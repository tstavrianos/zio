using System;

namespace Zio.FileSystems
{
    public class FileSystemWatcher : IFileSystemWatcher
    {
        private string _filter;
        private FilterPattern _filterPattern;

        /// <inheritdoc />
        public event EventHandler<FileChangedEventArgs> Changed;

        /// <inheritdoc />
        public event EventHandler<FileChangedEventArgs> Created;

        /// <inheritdoc />
        public event EventHandler<FileChangedEventArgs> Deleted;

        /// <inheritdoc />
        public event EventHandler<FileSystemErrorEventArgs> Error;

        /// <inheritdoc />
        public event EventHandler<FileRenamedEventArgs> Renamed;

        /// <inheritdoc />
        public IFileSystem FileSystem { get; }

        /// <inheritdoc />
        public UPath Path { get; }

        /// <inheritdoc />
        public virtual int InternalBufferSize
        {
            get => 0;
            set { }
        }

        /// <inheritdoc />
        public virtual NotifyFilters NotifyFilter { get; set; } = NotifyFilters.Default;

        /// <inheritdoc />
        public virtual bool EnableRaisingEvents { get; set; }

        /// <inheritdoc />
        public virtual string Filter
        {
            get => this._filter;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = "*";
                }

                if (value == this._filter)
                {
                    return;
                }

                this._filterPattern = FilterPattern.Parse(value);
                this._filter = value;
            }
        }

        /// <inheritdoc />
        public virtual bool IncludeSubdirectories { get; set; }

        public FileSystemWatcher(IFileSystem fileSystem, UPath path)
        {
            if (fileSystem == null)
            {
                throw new ArgumentNullException(nameof(fileSystem));
            }
            path.AssertAbsolute();

            this.FileSystem = fileSystem;
            this.Path = path;
            this._filter = "*.*";
        }

        ~FileSystemWatcher()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// Raises the <see cref="Changed"/> event. 
        /// </summary>
        /// <param name="args">Arguments for the event.</param>
        public void RaiseChanged(FileChangedEventArgs args)
        {
            if (!this.ShouldRaiseEvent(args))
            {
                return;
            }

            this.Changed?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="Created"/> event. 
        /// </summary>
        /// <param name="args">Arguments for the event.</param>
        public void RaiseCreated(FileChangedEventArgs args)
        {
            if (!this.ShouldRaiseEvent(args))
            {
                return;
            }

            this.Created?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="Deleted"/> event. 
        /// </summary>
        /// <param name="args">Arguments for the event.</param>
        public void RaiseDeleted(FileChangedEventArgs args)
        {
            if (!this.ShouldRaiseEvent(args))
            {
                return;
            }

            this.Deleted?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="Error"/> event. 
        /// </summary>
        /// <param name="args">Arguments for the event.</param>
        public void RaiseError(FileSystemErrorEventArgs args)
        {
            if (!this.EnableRaisingEvents)
            {
                return;
            }

            this.Error?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="Renamed"/> event. 
        /// </summary>
        /// <param name="args">Arguments for the event.</param>
        public void RaiseRenamed(FileRenamedEventArgs args)
        {
            if (!this.ShouldRaiseEvent(args))
            {
                return;
            }

            this.Renamed?.Invoke(this, args);
        }

        private bool ShouldRaiseEvent(FileChangedEventArgs args)
        {
            return this.EnableRaisingEvents && this._filterPattern.Match(args.Name) && this.ShouldRaiseEventImpl(args);
        }

        /// <summary>
        /// Checks if the event should be raised for the given arguments. Default implementation
        /// checks if the <see cref="FileChangedEventArgs.FullPath"/> is contained in <see cref="Path"/>.
        /// </summary>
        /// <param name="args">Arguments for the event.</param>
        /// <returns>True if the event should be raised, false to ignore it.</returns>
        protected virtual bool ShouldRaiseEventImpl(FileChangedEventArgs args)
        {
            return args.FullPath.IsInDirectory(this.Path, this.IncludeSubdirectories);
        }

        /// <summary>
        /// Listens to events from another <see cref="IFileSystemWatcher"/> instance to forward them
        /// into this instance.
        /// </summary>
        /// <param name="watcher">Other instance to listen to.</param>
        protected void RegisterEvents(IFileSystemWatcher watcher)
        {
            if (watcher == null)
            {
                throw new ArgumentNullException(nameof(watcher));
            }

            watcher.Changed += this.OnChanged;
            watcher.Created += this.OnCreated;
            watcher.Deleted += this.OnDeleted;
            watcher.Error += this.OnError;
            watcher.Renamed += this.OnRenamed;
        }

        /// <summary>
        /// Stops listening to events from another <see cref="IFileSystemWatcher"/>.
        /// </summary>
        /// <param name="watcher">Instance to remove event handlers from.</param>
        protected void UnregisterEvents(IFileSystemWatcher watcher)
        {
            if (watcher == null)
            {
                throw new ArgumentNullException(nameof(watcher));
            }

            watcher.Changed -= this.OnChanged;
            watcher.Created -= this.OnCreated;
            watcher.Deleted -= this.OnDeleted;
            watcher.Error -= this.OnError;
            watcher.Renamed -= this.OnRenamed;
        }

        /// <summary>
        /// Attempts to convert paths from an existing event in another <see cref="IFileSystem"/> into
        /// this <see cref="FileSystem"/>. If this returns <c>null</c> the event will be discarded.
        /// </summary>
        /// <param name="pathFromEvent">Path from the other filesystem.</param>
        /// <returns>Path in this filesystem, or null if it cannot be converted.</returns>
        protected virtual UPath? TryConvertPath(UPath pathFromEvent)
        {
            return pathFromEvent;
        }

        private void OnChanged(object sender, FileChangedEventArgs args)
        {
            var newPath = this.TryConvertPath(args.FullPath);
            if (!newPath.HasValue)
            {
                return;
            }

            var newArgs = new FileChangedEventArgs(this.FileSystem, args.ChangeType, newPath.Value);
            this.RaiseChanged(newArgs);
        }

        private void OnCreated(object sender, FileChangedEventArgs args)
        {
            var newPath = this.TryConvertPath(args.FullPath);
            if (!newPath.HasValue)
            {
                return;
            }

            var newArgs = new FileChangedEventArgs(this.FileSystem, args.ChangeType, newPath.Value);
            this.RaiseCreated(newArgs);
        }

        private void OnDeleted(object sender, FileChangedEventArgs args)
        {
            var newPath = this.TryConvertPath(args.FullPath);
            if (!newPath.HasValue)
            {
                return;
            }

            var newArgs = new FileChangedEventArgs(this.FileSystem, args.ChangeType, newPath.Value);
            this.RaiseDeleted(newArgs);
        }

        private void OnError(object sender, FileSystemErrorEventArgs args)
        {
            this.RaiseError(args);
        }

        private void OnRenamed(object sender, FileRenamedEventArgs args)
        {
            var newPath = this.TryConvertPath(args.FullPath);
            if (!newPath.HasValue)
            {
                return;
            }

            var newOldPath = this.TryConvertPath(args.OldFullPath);
            if (!newOldPath.HasValue)
            {
                return;
            }
            
            var newArgs = new FileRenamedEventArgs(this.FileSystem, args.ChangeType, newPath.Value, newOldPath.Value);
            this.RaiseRenamed(newArgs);
        }
    }
}
