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

namespace CrystalSharp.Domain
{
    public abstract class Entity<TKey> : IEntity<TKey>
    {
        public virtual TKey Id { get; protected set; }
        public Guid GlobalUId { get; protected set; } = Guid.NewGuid();
        public EntityStatus EntityStatus { get; protected set; } = EntityStatus.Active;
        public DateTime CreatedOn { get; protected set; }
        public DateTime? ModifiedOn { get; protected set; }

        public void SetSecondaryId(Guid globalUId)
        {
            GlobalUId = globalUId;
        }

        public void SetCreatedOn(DateTime dateTime)
        {
            CreatedOn = (CreatedOn == default) ? dateTime : CreatedOn;
        }

        public void SetModifiedOn(DateTime dateTime)
        {
            ModifiedOn = dateTime;
        }

        public virtual void Activate()
        {
            EntityStatus = EntityStatus.Active;
        }

        public virtual void Delete()
        {
            EntityStatus = EntityStatus.Deleted;
        }

        public bool Active()
        {
            return EntityStatus == EntityStatus.Active;
        }
    }
}
