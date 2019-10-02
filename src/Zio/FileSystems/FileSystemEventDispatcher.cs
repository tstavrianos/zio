using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Zio.FileSystems
{
    /// <summary>
    /// Stores <see cref="FileSystemWatcher"/> instances to dispatch events to. Events are
    /// called on a separate thread.
    /// </summary>
    /// <typeparam name="T">The <see cref="FileSystemWatcher"/> type to store.</typeparam>
    public class FileSystemEventDispatcher<T> : IDisposable
        where T : FileSystemWatcher
    {
        private readonly Thread _dispatchThread;
        private readonly BlockingCollection<Action> _dispatchQueue;
        private readonly CancellationTokenSource _dispatchCts;
        private readonly List<T> _watchers;

        public FileSystemEventDispatcher(IFileSystem fileSystem)
        {
            this.FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            this._dispatchThread = new Thread(this.DispatchWorker)
            {
                Name = "FileSystem Event Dispatch",
                IsBackground = true
            };

            this._dispatchQueue = new BlockingCollection<Action>(16);
            this._dispatchCts = new CancellationTokenSource();
            this._watchers = new List<T>();

            this._dispatchThread.Start();
        }

        public IFileSystem FileSystem { get; }

        ~FileSystemEventDispatcher()
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
            this._dispatchCts?.Cancel();
            this._dispatchThread?.Join();

            if (!disposing)
            {
                return;
            }

            this._dispatchQueue.CompleteAdding();

            lock (this._watchers)
            {
                foreach (var watcher in this._watchers)
                {
                    watcher.Dispose();
                }

                this._watchers.Clear();
            }

            this._dispatchQueue.Dispose();
        }

        /// <summary>
        /// Adds a <see cref="FileSystemWatcher"/> instance to dispatch events to.
        /// </summary>
        /// <param name="watcher">Instance to add.</param>
        public void Add(T watcher)
        {
            lock (this._watchers)
            {
                this._watchers.Add(watcher);
            }
        }

        /// <summary>
        /// Removes a <see cref="FileSystemWatcher"/> instance to stop dispatching events.
        /// </summary>
        /// <param name="watcher">Instance to remove.</param>
        public void Remove(T watcher)
        {
            lock (this._watchers)
            {
                this._watchers.Remove(watcher);
            }
        }

        /// <summary>
        /// Raise the <see cref="IFileSystemWatcher.Changed"/> event on watchers.
        /// </summary>
        /// <param name="path">Absolute path to the changed file or directory.</param>
        public void RaiseChange(UPath path)
        {
            var args = new FileChangedEventArgs(this.FileSystem, WatcherChangeTypes.Changed, path);
            this.Dispatch(args, (w, a) => w.RaiseChanged(a));
        }

        /// <summary>
        /// Raise the <see cref="IFileSystemWatcher.Created"/> event on watchers.
        /// </summary>
        /// <param name="path">Absolute path to the new file or directory.</param>
        public void RaiseCreated(UPath path)
        {
            var args = new FileChangedEventArgs(this.FileSystem, WatcherChangeTypes.Created, path);
            this.Dispatch(args, (w, a) => w.RaiseCreated(a));
        }
        
        /// <summary>
        /// Raise the <see cref="IFileSystemWatcher.Deleted"/> event on watchers.
        /// </summary>
        /// <param name="path">Absolute path to the changed file or directory.</param>
        public void RaiseDeleted(UPath path)
        {
            var args = new FileChangedEventArgs(this.FileSystem, WatcherChangeTypes.Deleted, path);
            this.Dispatch(args, (w, a) => w.RaiseDeleted(a));
        }

        /// <summary>
        /// Raise the <see cref="IFileSystemWatcher.Renamed"/> event on watchers.
        /// </summary>
        /// <param name="newPath">Absolute path to the new file or directory.</param>
        /// <param name="oldPath">Absolute path to the old file or directory.</param>
        public void RaiseRenamed(UPath newPath, UPath oldPath)
        {
            var args = new FileRenamedEventArgs(this.FileSystem, WatcherChangeTypes.Renamed, newPath, oldPath);
            this.Dispatch(args, (w, a) => w.RaiseRenamed(a));
        }

        /// <summary>
        /// Raise the <see cref="IFileSystemWatcher.Error"/> event on watchers.
        /// </summary>
        /// <param name="exception">Exception that occurred.</param>
        public void RaiseError(Exception exception)
        {
            var args = new FileSystemErrorEventArgs(exception);
            this.Dispatch(args, (w, a) => w.RaiseError(a), false);
        }

        private void Dispatch<TArgs>(TArgs eventArgs, Action<T, TArgs> handler, bool captureError = true)
            where TArgs : EventArgs
        {
            List<T> watchersSnapshot;
            lock (this._watchers)
            {
                if (this._watchers.Count == 0)
                {
                    return;
                }

                watchersSnapshot = this._watchers.ToList(); // TODO: reduce allocations
            }

            // The events should be called on a separate thread because the filesystem code
            // could be holding locks that must be released.
            this._dispatchQueue.Add(() =>
            {
                foreach (var watcher in watchersSnapshot)
                {
                    try
                    {
                        handler(watcher, eventArgs);
                    }
                    catch (Exception e) when (captureError)
                    {
                        this.RaiseError(e);
                    }
                }
            });
        }

        // Worker runs on dedicated thread to call events
        private void DispatchWorker()
        {
            var ct = this._dispatchCts.Token;

            try
            {
                foreach (var action in this._dispatchQueue.GetConsumingEnumerable(ct))
                {
                    action();
                }
            }
            catch (OperationCanceledException) { }
            catch (ObjectDisposedException) { }
        }
    }
}
