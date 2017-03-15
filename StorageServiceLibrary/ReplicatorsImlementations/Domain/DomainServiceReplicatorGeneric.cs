using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace StorageServiceLibrary
{
    [Serializable]
    public class DomainServiceReplicator<TService, U> : IServiceReplicator<TService, U>, IDisposable where TService : IMasterService<U>
    {

        private AppDomain masterDomain;
        private List<AppDomain> slaveDomainList;

        private IMasterService<U> master;
        private List<ISlaveService<U>> slaveList;

        private int slaveCount;
        private int maxSlaveCount;

        private byte[] binSerializeData;

        public IEnumerable<ISlaveService<U>> Slaves
        {
            get
            {
                return slaveList.AsEnumerable();
            }
        }

        public DomainServiceReplicator(TService masterInstance, int maxSlaveCount = 5)
        {
            if (typeof(TService) == null) throw new ArgumentNullException(nameof(TService));
            if (masterInstance == null) throw new ArgumentNullException(nameof(masterInstance));
            if (maxSlaveCount < 1) throw new ArgumentOutOfRangeException(nameof(maxSlaveCount));

            slaveCount = 0;

            //masterInstance.SaveStateToFile("masterInst.dat");
            binSerializeData = masterInstance.Serialize();

            this.maxSlaveCount = maxSlaveCount;
            slaveList = new List<ISlaveService<U>>();
            slaveDomainList = new List<AppDomain>();
            masterDomain = null;
        }

        public IMasterService<U> CreateMaster()
        {
            if (master != null) return master;

            AppDomainSetup appDomainSetup = new AppDomainSetup
            {
                ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
                PrivateBinPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MasterAppDomain"),
            };

            AppDomain domain = AppDomain.CreateDomain
                ("MasterAppDomain", null, appDomainSetup);

            masterDomain = domain;

            var masterTemp = (IMasterService<U>)domain.CreateInstanceAndUnwrap
                ("StorageServiceLibrary", typeof(TService).FullName);


            masterTemp.Deserialize(binSerializeData);

            this.master = masterTemp;

            return masterTemp;
        }

        public ISlaveService<U> CreateSlave()
        {
            if (slaveCount++ > maxSlaveCount) throw new IndexOutOfRangeException(nameof(slaveCount));
            if (masterDomain == null) throw new FieldAccessException(nameof(masterDomain));

            AppDomainSetup appDomainSetup = new AppDomainSetup
            {
                ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
                PrivateBinPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SlaveDomain_" + slaveCount)
            };

            AppDomain domain = AppDomain.CreateDomain
                ("SlaveDomain_" + slaveCount, null, appDomainSetup);

            slaveDomainList.Add(domain);

            var temp = (IMasterService<U>)domain.CreateInstanceAndUnwrap
                ("StorageServiceLibrary", typeof(TService).FullName);

            master.SaveStateToFile("master.dat");
            temp.RecoverFromFile("master.dat");
            var slave = temp as ISlaveService<U>;


            slave.SubscribedOnMasterChange(master);
            slaveList.Add(slave);

            return slave;
        }

        //public void SaveStateToFile(string path)
        //{

        //    using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
        //    {
        //        BinaryFormatter formatter = new BinaryFormatter();
        //        formatter.Serialize(fs, this);

        //        Debug.WriteLine("Объект сериализован");
        //    }
        //}

        //public void RecoverFromFile(string path)
        //{
        //    DomainServiceReplicator<TService, U> temp = null;

        //    using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
        //    {
        //        BinaryFormatter formatter = new BinaryFormatter();
        //        temp = (DomainServiceReplicator<TService, U>)formatter.Deserialize(fs);
        //        Debug.WriteLine("Объект десериализован");
        //    }

        //    masterDomain = temp.masterDomain;
        //    slaveDomainList = temp.slaveDomainList;
        //    master = temp.master;
        //    slaveList = temp.slaveList;
        //    slaveCount = temp.slaveCount;
        //    maxSlaveCount = temp.maxSlaveCount;
        //}

        public override string ToString()
        {
            Debug.WriteLine("Object is executing in AppDomain \"{0}\"",
            AppDomain.CurrentDomain.FriendlyName);
            return base.ToString();

        }

        public void Dispose()
        {

            AppDomain.Unload(masterDomain);

            foreach (AppDomain domain in slaveDomainList)
            {
                AppDomain.Unload(domain);
            }

            masterDomain = null;
            slaveDomainList = new List<AppDomain>();

        }





    }
}
