using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageServiceLibrary
{
    public interface IStateSaveRecovery
    {
        void SaveStateToFile(string path);
        void RecoverFromFile(string path);
        byte[] Serialize();
        void Deserialize(byte[] array);

    }
}
