using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StorageServiceLibrary;
using System.Configuration;
using NLog;
using System.IO;

namespace StorageService
{
    class Program
    {
        static void Main(string[] args)
        {

            int maxSlaveCount = int.Parse(ConfigurationManager.AppSettings["MaxSlaveCount"]);

            string recoveryFilePath = ConfigurationManager.AppSettings["RecoveryFilePath"];

            IMasterService<User> master;

            var userList = new List<User> {

            new User() { FirstName = "Donalda", LastName = "Tramp", Age = 71 },
            new User() { FirstName = "Pik", LastName = "Loas", Age = 71 },
            new User() { FirstName = "Lisa", LastName = "Poret", Age = 71 },
            new User() { FirstName = "Garry", LastName = "Fitaaas", Age = 71 },
            new User() { FirstName = "Genadiy", LastName = "Pohi", Age = 71 },
            new User() { FirstName = "Omg", LastName = "Ram", Age = 133, Id = 133 }

        };

            IServiceReplicator<StorageService<User>, User> replicator =
             new DomainServiceReplicator<StorageService<User>, User>(
                 new StorageService<User>(
                     new List<User>()
             {
                     userList[5]
         }));

            master = replicator.CreateMaster();


            if (File.Exists(recoveryFilePath))
            {
                master.RecoverFromFile(recoveryFilePath);
            }

            else
            {
                foreach (User user in userList)
                {
                    master.Add(user);
                }
            }




            ISlaveService<User> slave1 = replicator.CreateSlave();
            ISlaveService<User> slave2 = replicator.CreateSlave();
            ISlaveService<User> slave3 = replicator.CreateSlave();
            ISlaveService<User> slave4 = replicator.CreateSlave();


            Console.WriteLine("Client domain name: {0}", AppDomain.CurrentDomain.FriendlyName);
            Console.WriteLine(master);
            foreach (ISlaveService<User> slave in replicator.Slaves) { Console.WriteLine(slave); }
            Console.ReadKey();




            Console.WriteLine(master.Search(u => u.Id != 0).FirstOrDefault().ToString());
            foreach (ISlaveService<User> slave in replicator.Slaves) { Console.WriteLine(slave.Search(u => u.Id != 2).FirstOrDefault().ToString()); }
           

            Console.ReadKey();

            var user_to_remove = slave4.Search(u => u.Id != 0).FirstOrDefault();
            master.Remove(user_to_remove);

            Console.WriteLine(master.Search(u => u.Id != 0).FirstOrDefault().ToString());
            foreach (ISlaveService<User> slave in replicator.Slaves) { Console.WriteLine(slave.Search(u => u.Id != 2).FirstOrDefault().ToString()); }

            Console.ReadKey();


            master.SaveStateToFile(recoveryFilePath);



        }
    }
}
