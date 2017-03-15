using NLog;
using StorageServiceLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace StorageServiceLibrary
{


    [Serializable]
    public class StorageService<T> : MarshalByRefObject, IMasterService<T>, ISlaveService<T> where T : IUnique, ICloneable
    {
        private IIdGenerator idGenerator;
        private IEqualityComparer<T> comparer;
        private ICollection<T> container;
    

   
        public event EventHandler<ChangeMessageEventArgs<T>> changeEvent;


        #region Ctor

        public StorageService(IIdGenerator idGenerator, ICollection<T> container, IEqualityComparer<T> comparer) : base()
        {
            if (idGenerator == null) throw new ArgumentNullException(nameof(idGenerator));
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));

            this.idGenerator = idGenerator;
            this.container = container;
            this.comparer = comparer;
     

            changeEvent += delegate { };
        }

        public StorageService() : this(GetDefaultIdGenerator(), GetDefaultContainer(), GetDefaultComparer()) { }
        public StorageService(IIdGenerator idGenerator) : this(idGenerator, GetDefaultContainer(), GetDefaultComparer()) { }
        public StorageService(ICollection<T> container) : this(GetDefaultIdGenerator(), container, GetDefaultComparer()) { }
        public StorageService(IEqualityComparer<T> comparer) : this(GetDefaultIdGenerator(), GetDefaultContainer(), comparer) { }



        #endregion

        #region Public methods

        public void Add(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            if (container.Contains(item, comparer)) throw new InvalidOperationException(nameof(item));

            while (item.Id == 0 || container.Contains(item, new IdComparer())) item.Id = idGenerator.GetId();

            container.Add(item);

            changeEvent(this, new ChangeMessageEventArgs<T>(item, Actions.Add));

            LogManager.GetCurrentClassLogger().Info("{0} add to storage service", item.ToString());

        }

        public void Remove(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (!container.Remove(item)) throw new InvalidOperationException(nameof(item));
            changeEvent(this, new ChangeMessageEventArgs<T>(item, Actions.Remove));

            LogManager.GetCurrentClassLogger().Info("{0} remove from storage service", item.ToString());
        }


        public IEnumerable<T> Search(Func<T, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            LogManager.GetCurrentClassLogger().Info("Try to search by predicate {0}", predicate.ToString());

            return container.Where(predicate).ToList();
        }

        public void SubscribedOnMasterChange(IMasterService<T> master)
        {
            master.changeEvent += Master_changeEvent;
            LogManager.GetCurrentClassLogger().Info("{0} {1} subscribed to master {2} servise change event" , AppDomain.CurrentDomain.FriendlyName, this.GetType().Name, master.GetType().Name );
        }

        private void Master_changeEvent(object sender, ChangeMessageEventArgs<T> e)
        {
            if (e.Action == Actions.Add) container.Add(e.Argument);
            if (e.Action == Actions.Remove) container.Remove(e.Argument);
        }

        static public ICollection<T> GetDefaultContainer()
        {
            return new List<T>();
        }

        static public IIdGenerator GetDefaultIdGenerator()
        {
            return new IncrementIdGenerator();
        }

        static public IEqualityComparer<T> GetDefaultComparer()
        {
            return new IdComparer();
        }

        public void SaveStateToFile(string path)
        {
            StorageService<T> temp = new StorageService<T>(idGenerator, container, comparer);

            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, temp);

                Debug.WriteLine("Объект сериализован");
            }
        }

        public void RecoverFromFile(string path)
        {
            StorageService<T> temp = null;

            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                temp = (StorageService<T>)formatter.Deserialize(fs);
                Debug.WriteLine("Объект десериализован");
            }

            idGenerator = temp.idGenerator;
            comparer = temp.comparer;
            container = temp.container;
            changeEvent = temp.changeEvent;
        }

        public byte[] Serialize()
        {
            using (var memoryStream = new MemoryStream())
            {
                (new BinaryFormatter()).Serialize(memoryStream, this);
                return memoryStream.ToArray();
            }
        }

        public void Deserialize(byte[] array)
        {
            if (array == null) throw new ArgumentNullException();
            StorageService<T> tempMaster = null;

            using (var memoryStream = new MemoryStream(array))
            {
                tempMaster = (StorageService<T>)(new BinaryFormatter()).Deserialize(memoryStream);
            }

            this.comparer = tempMaster.comparer;
            this.container = tempMaster.container;
            this.idGenerator = tempMaster.idGenerator;
        }

        public override string ToString()
        {
            return String.Format("{1} is executing in AppDomain {0}",
            AppDomain.CurrentDomain.FriendlyName, this.GetType().Name);
        }




        #endregion

        #region Nested clases

        [Serializable]
        private class IncrementIdGenerator : IIdGenerator
        {
            static int counter = 0;

            public int GetId()
            {
                return counter++;
            }
        }

        [Serializable]
        private class IdComparer : IEqualityComparer<T>
        {
            public bool Equals(T x, T y)
            {
                if (x.Id == y.Id) return true;
                return false;
            }

            public int GetHashCode(T obj)
            {
                return obj.Id; //!!!
            }
        }

        #endregion

    }
}
