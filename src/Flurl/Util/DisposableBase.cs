using System;

namespace Flurl.Util
{
    public interface IDisposableObservable : IDisposable
    {
        bool IsDisposed { get; }
    }

    /// <summary>
    /// Base class that follows IDisposable pattern described at https://docs.microsoft.com/en-us/dotnet/api/system.idisposable.dispose?redirectedfrom=MSDN&view=netframework-4.7.2#System_IDisposable_Dispose
    /// </summary>
    public abstract class DisposableBase : IDisposableObservable
    {
        ~DisposableBase()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool IsDisposed { get; private set; }

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

        protected abstract void DisposeResources();
    }
}
