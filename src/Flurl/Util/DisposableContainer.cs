using System;
using System.Collections.Generic;

namespace Flurl.Util
{
    /// <summary>
    /// A container that holds a collection of disposable objects and dispose all of them of container's own disposal
    /// </summary>
    public class DisposableContainer : DisposableBase
    {
        private readonly IList<IDisposable> _itemsToDispose = new List<IDisposable>();

        /// <summary>
        /// Register a disposable object to the container so that it will be disposed on container's disposal
        /// </summary>
        /// <param name="object"></param>
        public void Register(IDisposable @object)
        {
            _itemsToDispose.Add(@object);
        }

        /// <inheritdoc/>
        protected override void DisposeResources()
        {
            foreach (var item in _itemsToDispose)
            {
                using (item) { }
            }
        }
    }
}
