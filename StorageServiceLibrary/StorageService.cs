using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageServiceLibrary
{
    public class StorageService<T> : IMasterService<T>, ISlaveService<T> where T : IUnique
    {



    }
}
