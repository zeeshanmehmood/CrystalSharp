using CrystalSharp.Common.Serialization;

namespace CrystalSharp.Tests.Common.Messaging.Data
{
    public class Customer
    {
        public string Name { get; set; }
        public int Rating { get; set; }
        public bool Active { get; set; }

        public static Customer CreateObject(string name, int rating, bool active)
        {
            return new Customer { Name = name, Rating = rating, Active = active };
        }

        public static string CreateJson(string name, int rating, bool active)
        {
            Customer customer = CreateObject(name, rating, active);

            return Serializer.Serialize(customer);
        }
    }
}
