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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CrystalSharp.Infrastructure.ReadModelStoresPersistence;
using CrystalSharp.Oracle.Database;
using CrystalSharp.Oracle.Stores;

namespace CrystalSharp.Oracle.Extensions
{
    public static class CrystalSharpAdapterOracleExtensions
    {
        public static ICrystalSharpAdapter AddOracle<TDbContext>(this ICrystalSharpAdapter crystalSharpAdapter,
            OracleSettings settings)
            where TDbContext : DbContext
        {
            AddOracleWithOptions<TDbContext>(crystalSharpAdapter, settings);
            crystalSharpAdapter.ServiceCollection.AddScoped<IOracleEntityFrameworkCoreContext, OracleEntityFrameworkCoreContext>();

            return crystalSharpAdapter;
        }

        public static ICrystalSharpAdapter AddOracleReadModelStore<TDbContext, TKey>(this ICrystalSharpAdapter crystalSharpAdapter,
            OracleSettings settings)
            where TDbContext : DbContext
        {
            AddOracleWithOptions<TDbContext>(crystalSharpAdapter, settings);
            crystalSharpAdapter.ServiceCollection.AddScoped<IReadModelStore<TKey>, OracleReadModelStore<TDbContext, TKey>>();

            return crystalSharpAdapter;
        }

        private static void AddOracleWithOptions<TDbContext>(ICrystalSharpAdapter crystalSharpAdapter, OracleSettings settings)
            where TDbContext : DbContext
        {
#if NET8_0
            crystalSharpAdapter.ServiceCollection.AddDbContext<TDbContext>(options =>
            {
                if (settings.LazyLoading)
                {
                    options = options.UseLazyLoadingProxies(settings.LazyLoading);
                }

                options = options.UseOracle(settings.ConnectionString);
            });
#else
            crystalSharpAdapter.ServiceCollection.AddDbContext<TDbContext>(options =>
            {
                if (settings.LazyLoading)
                {
                    options = options.UseLazyLoadingProxies(settings.LazyLoading);
                }

                if (!string.IsNullOrEmpty(settings.Version))
                {
                    options = options.UseOracle(settings.ConnectionString, p =>
                    {
                        p.UseOracleSQLCompatibility(settings.Version);
                    });
                }
                else
                {
                    options = options.UseOracle(settings.ConnectionString);
                }
            });
#endif
        }
    }
}
