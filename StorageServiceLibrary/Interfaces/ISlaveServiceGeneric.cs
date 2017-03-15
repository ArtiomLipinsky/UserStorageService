using System;
using System.Collections.Generic;


namespace StorageServiceLibrary
{
    public interface ISlaveService<T>
    {
        IEnumerable<T> Search(Func<T, bool> predicate);

        void SubscribedOnMasterChange(IMasterService<T> master);

    }
}
