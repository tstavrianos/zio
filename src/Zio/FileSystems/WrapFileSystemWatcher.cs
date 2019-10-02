using System;

namespace Zio.FileSystems
{
    /// <summary>
    /// Wraps another <see cref="IFileSystemWatcher"/> instance to allow event modification and filtering.
    /// </summary>
    public class WrapFileSystemWatcher : FileSystemWatcher
    {
        private readonly IFileSystemWatcher _watcher;

        public WrapFileSystemWatcher(IFileSystem fileSystem, UPath path, IFileSystemWatcher watcher)
            : base(fileSystem, path)
        {
            if (watcher == null)
            {
                throw new ArgumentNullException(nameof(watcher));
            }

            this._watcher = watcher;

            this.RegisterEvents(this._watcher);
        }

        /// <inheritdoc />
        public override int InternalBufferSize
        {
            get => this._watcher.InternalBufferSize;
            set => this._watcher.InternalBufferSize = value;
        }

        /// <inheritdoc />
        public override NotifyFilters NotifyFilter
        {
            get => this._watcher.NotifyFilter;
            set => this._watcher.NotifyFilter = value;
        }

        /// <inheritdoc />
        public override bool EnableRaisingEvents
        {
            get => this._watcher.EnableRaisingEvents;
            set => this._watcher.EnableRaisingEvents = value;
        }

        /// <inheritdoc />
        public override string Filter
        {
            get => this._watcher.Filter;
            set => this._watcher.Filter = value;
        }

        /// <inheritdoc />
        public override bool IncludeSubdirectories
        {
            get => this._watcher.IncludeSubdirectories;
            set => this._watcher.IncludeSubdirectories = value;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.UnregisterEvents(this._watcher);
                this._watcher.Dispose();
            }
        }
    }
}
