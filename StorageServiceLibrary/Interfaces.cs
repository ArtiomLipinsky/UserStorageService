using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageServiceLibrary
{
    public interface IIdGenerator
    {
        int GetId();
    }

    public interface IUnique
    {
        int Id { get; set; }
    }

    interface IServiceRelocator
    {
        IMasterService<T> Master { get; set; }

        void Synchronize();
        IEnumerable<ISlaveService> GetAllSlaves<ISlaveService>();
        ISlaveService<T> GetNewSlave<T>();
    }

    public interface IMasterService<T> : ISlaveService<T>
    {
        void Add(T item);
        void Remove(T item);
        void Remove(int id); 
    }


    public interface ISlaveService<T>
    {
        T Get(int id);
        IEnumerable<T> Search(Func<T, bool> predicate);

        IEnumerable<T> AsEnumerable();
        IList<T> ToList();
        T[] ToArray();

        int GetHashCode();
    }

}
