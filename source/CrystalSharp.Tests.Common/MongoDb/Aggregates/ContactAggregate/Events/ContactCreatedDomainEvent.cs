using CrystalSharp.Domain.Infrastructure;
using Newtonsoft.Json;
using System;

namespace CrystalSharp.Tests.Common.MongoDb.Aggregates.ContactAggregate.Events
{
    public class ContactCreatedDomainEvent : DomainEvent
    {
        public PersonDetails PersonDetails { get; set; }
        public string Email { get; set; }

        public ContactCreatedDomainEvent(Guid streamId, PersonDetails personDetails, string email)
        {
            StreamId = streamId;
            PersonDetails = personDetails;
            Email = email;
        }

        [JsonConstructor]
        public ContactCreatedDomainEvent(
            Guid streamId,
            PersonDetails personDetails,
            string email,
            int entityStatus,
            DateTime createdAt,
            DateTime? modifiedOn,
            long version)
        {
            StreamId = streamId;
            PersonDetails = personDetails;
            Email = email;
            EntityStatus = entityStatus;
            CreatedAt = createdAt;
            ModifiedOn = modifiedOn;
            Version = version;
        }
    }
}
