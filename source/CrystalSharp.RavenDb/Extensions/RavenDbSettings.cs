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

namespace CrystalSharp.RavenDb.Extensions
{
    public class RavenDbSettings
    {
        public string ConnectionUrl { get; }
        public string Database { get; }
        public bool UseCertificate { get; }

        /// <summary>
        /// If loading certificate from some other store such as environment variable or key/value pair string then this value must be true and CertificateString should have the value.
        /// </summary>
        public bool LoadCertificateFromString { get; }

        /// <summary>
        /// Use this property if LoadCertificateFromString property is true.
        /// </summary>
        public string CertificateString { get; }

        public string CertificateFile { get; }
        public string CertificatePassword { get; }

        public RavenDbSettings(string connectionUrl,
            string database,
            bool useCertificate = false,
            bool loadCertificateFromString = false,
            string certificateString = "",
            string certificateFile = "",
            string certificatePassword = "")
        {
            ConnectionUrl = connectionUrl;
            Database = database;
            UseCertificate = useCertificate;
            LoadCertificateFromString = loadCertificateFromString;
            CertificateString = certificateString;
            CertificateFile = certificateFile;
            CertificatePassword = certificatePassword;
        }
    }
}
