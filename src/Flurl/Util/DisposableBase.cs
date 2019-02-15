using System;

namespace Flurl.Util
{
    /// <summary>
    /// Extends IDisposable with a flag indicating wheather object has been disposed
    /// </summary>
    public interface IDisposableObservable : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether this object has been disposed.
        /// </summary>
        bool IsDisposed { get; }
    }

    /// <summary>
    /// Base class that follows IDisposable pattern
    /// </summary>
    public abstract class DisposableBase : IDisposableObservable
    {
        /// <summary>
        /// Finalizes an instance of the <see cref="DisposableBase" /> class.
        /// </summary>
        ~DisposableBase()
        {
            this.Dispose(false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Dispose resources.
        /// </summary>
        /// <param name="disposing">True if called explicitly, false if called through the destructor.</param>
        protected void Dispose(bool disposing)
        {
            if (IsDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.DisposeResources();
            }

            IsDisposed = true;
        }

        /// <summary>
        /// Release resources. base.Dispose() should not be called from this method. 
        /// </summary>
        protected abstract void DisposeResources();
    }
}
