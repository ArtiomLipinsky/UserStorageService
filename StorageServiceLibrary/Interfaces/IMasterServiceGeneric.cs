

using System;

namespace StorageServiceLibrary
{
    public interface IMasterService<T> : ISlaveService<T>, IStateSaveRecovery
    {
        event EventHandler<ChangeMessageEventArgs<T>> changeEvent;
        void Add(T item);
        void Remove(T item);
    }
}
