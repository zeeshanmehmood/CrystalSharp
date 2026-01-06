using CrystalSharp.Common.Extensions;
using CrystalSharp.Domain;
using CrystalSharp.Tests.Common.MongoDb.Aggregates.ContactAggregate.Events;
using System;

namespace CrystalSharp.Tests.Common.MongoDb.Aggregates.ContactAggregate
{
    public class Contact : AggregateRoot<string>
    {
        public override string Id { get; protected set; } = Guid.Create().ToString("N");
        public PersonDetails PersonDetails { get; private set; }
        public string Email { get; private set; }

        private static void ValidateContact(Contact contact)
        {
            if (contact.PersonDetails is null)
            {
                contact.ThrowDomainException("Person details are required.");
            }

            if (string.IsNullOrEmpty(contact.PersonDetails.FirstName))
            {
                contact.ThrowDomainException("First name is required.");
            }

            if (string.IsNullOrEmpty(contact.PersonDetails.LastName))
            {
                contact.ThrowDomainException("Last name is required.");
            }

            if (string.IsNullOrEmpty(contact.Email))
            {
                contact.ThrowDomainException("Email address is required.");
            }
        }

        public static Contact Create(PersonDetails personDetails, string email)
        {
            Contact contact = new() { PersonDetails = personDetails, Email = email };

            ValidateContact(contact);

            contact.Raise(new ContactCreatedDomainEvent(contact.GlobalUId, contact.PersonDetails, contact.Email));

            return contact;
        }

        public void Change(PersonDetails personDetails, string email)
        {
            PersonDetails = personDetails;
            Email = email;

            ValidateContact(this);

            Raise(new ContactChangedDomainEvent(GlobalUId, PersonDetails, Email));
        }

        public override void Delete()
        {
            base.Delete();
            Raise(new ContactDeletedDomainEvent(GlobalUId, PersonDetails, Email));
        }

        private void Apply(ContactCreatedDomainEvent @event)
        {
            PersonDetails = @event.PersonDetails;
            Email = @event.Email;
        }

        private void Apply(ContactChangedDomainEvent @event)
        {
            PersonDetails = @event.PersonDetails;
            Email = @event.Email;
        }

        private void Apply(ContactDeletedDomainEvent @event)
        {
            PersonDetails = @event.PersonDetails;
            Email = @event.Email;
        }
    }
}
