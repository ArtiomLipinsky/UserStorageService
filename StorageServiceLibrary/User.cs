using System;

namespace StorageServiceLibrary
{

    [Serializable]
    public class User : IUnique
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime DateOfBirth { get; set; }
    }
}
