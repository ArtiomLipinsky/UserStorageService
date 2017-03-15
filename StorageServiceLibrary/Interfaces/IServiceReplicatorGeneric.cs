

using StorageServiceLibrary;
using System;
using System.Collections.Generic;

namespace StorageServiceLibrary
{
    public interface IServiceReplicator<TService, U> where TService : IMasterService<U>
    {
        IEnumerable<ISlaveService<U>> Slaves { get; }
        IMasterService<U> CreateMaster();
        ISlaveService<U> CreateSlave();

    }
}
