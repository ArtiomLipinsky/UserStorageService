using System;

namespace StorageServiceLibrary
{

    [Serializable]
    public class User : IUnique, ICloneable, IEquatable<User>
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int Age { get; set; }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public User Clone()
        {
            return new User()
            {
                Id = Id,
                FirstName = FirstName,
                LastName = LastName,
                Age = Age
            };
        }

        public override string ToString()
        {
            return $"{FirstName} {LastName} {Age} years";
        }

        public bool Equals(User other)
        {
            return Equals(this, other);
        }

        public static bool Equals(User user1, User user2)
        {
            if (user1 == null || user2 == null) return false;
            if (user1.Id == user2.Id) return true;
            return false;

        }
    }
}
