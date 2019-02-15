using System;
using System.Collections.Generic;

namespace Flurl.Util
{
    public class DisposableContainer : DisposableBase
    {
        private readonly IList<IDisposable> _itemsToDispose = new List<IDisposable>();

        public void Register(IDisposable @object)
        {
            _itemsToDispose.Add(@object);
        }

        protected override void DisposeResources()
        {
            foreach (var item in _itemsToDispose)
            {
                using (item) { }
            }
        }
    }
}
