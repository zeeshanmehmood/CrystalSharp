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
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using CrystalSharp.RavenDb.Database;

namespace CrystalSharp.RavenDb.Extensions
{
    public static class CrystalSharpAdapterRavenDbExtensions
    {
        public static ICrystalSharpAdapter AddRavenDb(this ICrystalSharpAdapter crystalSharpAdapter, RavenDbSettings settings)
        {
            crystalSharpAdapter.ServiceCollection.AddSingleton<IDocumentStore>(ConfigureDocumentStore(settings));
            crystalSharpAdapter.ServiceCollection.AddScoped<IDocumentSession>(s => s.GetService<IDocumentStore>().OpenSession());
            crystalSharpAdapter.ServiceCollection.AddScoped<IRavenDbContext, RavenDbContext>();

            return crystalSharpAdapter;
        }

        private static IDocumentStore ConfigureDocumentStore(RavenDbSettings settings)
        {
            IDocumentStore store = null;

            if (settings.UseCertificate)
            {
                store = new DocumentStore
                {
                    Urls = new[] { settings.ConnectionUrl },
                    Database = settings.Database,
                    Certificate = GetCertificate(settings),
                    Conventions = { }
                }.Initialize();
            }
            else
            {
                store = new DocumentStore
                {
                    Urls = new[] { settings.ConnectionUrl },
                    Database = settings.Database,
                    Conventions = { }
                }.Initialize();
            }

            return store;
        }

        private static X509Certificate2 GetCertificate(RavenDbSettings settings)
        {
            X509Certificate2 certificate = null;

            if (settings.LoadCertificateFromString)
            {
                byte[] certificateBytes = Convert.FromBase64String(settings.CertificateString);
                certificate = new X509Certificate2(certificateBytes, settings.CertificatePassword);
            }
            else
            {
                certificate = new X509Certificate2(settings.CertificateFile, settings.CertificatePassword);
            }

            return certificate;
        }
    }
}
