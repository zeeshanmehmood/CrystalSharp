// The MIT License (MIT)
//
// Copyright (c) 2024 Zeeshan Mehmood
// https://github.com/zeeshanmehmood/CrystalSharp
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using CrystalSharp.Domain;
using CrystalSharp.Tests.Common.MongoDb.Aggregates.ContactAggregate.Events;

namespace CrystalSharp.Tests.Common.MongoDb.Aggregates.ContactAggregate
{
    public class Contact : AggregateRoot<string>
    {
        public override string Id { get; protected set; } = Guid.NewGuid().ToString("N");
        public PersonInfo PersonInfo { get; private set; }
        public string Email { get; private set; }

        private static void ValidateContact(Contact contact)
        {
            if (contact.PersonInfo == null)
            {
                contact.ThrowDomainException("Person info is required.");
            }

            if (string.IsNullOrEmpty(contact.PersonInfo.FirstName))
            {
                contact.ThrowDomainException("First name is required.");
            }

            if (string.IsNullOrEmpty(contact.PersonInfo.LastName))
            {
                contact.ThrowDomainException("Last name is required.");
            }

            if (string.IsNullOrEmpty(contact.Email))
            {
                contact.ThrowDomainException("Email address is required.");
            }
        }

        public static Contact Create(PersonInfo personInfo, string email)
        {
            Contact contact = new() { PersonInfo = personInfo, Email = email };

            ValidateContact(contact);

            contact.Raise(new ContactCreatedDomainEvent(contact.GlobalUId, contact.PersonInfo, contact.Email));

            return contact;
        }

        public void Change(PersonInfo personInfo, string email)
        {
            PersonInfo = personInfo;
            Email = email;

            ValidateContact(this);

            Raise(new ContactChangedDomainEvent(GlobalUId, PersonInfo, Email));
        }

        public override void Delete()
        {
            base.Delete();
            Raise(new ContactDeletedDomainEvent(GlobalUId, PersonInfo, Email));
        }

        private void Apply(ContactCreatedDomainEvent @event)
        {
            PersonInfo = @event.PersonInfo;
            Email = @event.Email;
        }

        private void Apply(ContactChangedDomainEvent @event)
        {
            PersonInfo = @event.PersonInfo;
            Email = @event.Email;
        }

        private void Apply(ContactDeletedDomainEvent @event)
        {
            PersonInfo = @event.PersonInfo;
            Email = @event.Email;
        }
    }
}
