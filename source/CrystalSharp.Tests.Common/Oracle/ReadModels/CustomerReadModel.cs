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
using System.ComponentModel.DataAnnotations.Schema;
using CrystalSharp.Infrastructure.ReadModels;

namespace CrystalSharp.Tests.Common.Oracle.ReadModels
{
    [Table("CUSTOMER", Schema = "SYSTEM")]
    public class CustomerReadModel : ReadModel<int>
    {
        public string Name { get; set; }
        public string Code { get; set; }

        public static CustomerReadModel Create(Guid globalUId, string name, string code)
        {
            return new CustomerReadModel
            {
                GlobalUId = globalUId,
                Name = name,
                Code = code
            };
        }

        public static CustomerReadModel Create(string name, string code)
        {
            return Create(Guid.NewGuid(), name, code);
        }

        public void Change(string name, string code)
        {
            Name = name;
            Code = code;
        }
    }
}
