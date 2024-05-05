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
using CrystalSharp.Infrastructure.ReadModels;

namespace CrystalSharp.Tests.Common.Elasticsearch.ReadModels
{
    public class ProductReadModel : ReadModel<Guid>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public bool InStock { get; set; }
        public DateTime? AvailableFrom { get; set; }

        public static ProductReadModel Create(string name, string description, decimal price, bool inStock = true, DateTime? availableFrom = null)
        {
            Guid id = Guid.NewGuid();

            return new ProductReadModel
            {
                Id = id,
                GlobalUId = id,
                Name = name,
                Description = description,
                Price = price,
                InStock = inStock,
                AvailableFrom = availableFrom,
                CreatedOn = DateTime.Now
            };
        }

        public void Change(string name, string description, decimal price, bool inStock = true, DateTime? availableFrom = null)
        {
            Name = name;
            Description = description;
            Price = price;
            InStock = inStock;
            AvailableFrom = availableFrom;
            ModifiedOn = DateTime.Now;
        }
    }
}
