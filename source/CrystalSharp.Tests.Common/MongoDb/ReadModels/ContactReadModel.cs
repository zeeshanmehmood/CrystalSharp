using CrystalSharp.Common.Extensions;
using CrystalSharp.Infrastructure.ReadModels;
using System;

namespace CrystalSharp.Tests.Common.MongoDb.ReadModels
{
    public class ContactReadModel : ReadModel<string>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        public static ContactReadModel Create(string firstName, string lastName, string email)
        {
            Guid id = Guid.Create();

            return new ContactReadModel
            {
                Id = Guid.Create().ToString(),
                GlobalUId = id,
                FirstName = firstName,
                LastName = lastName,
                Email = email
            };
        }

        public void Change(string firstName, string lastName, string email)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
        }
    }
}
