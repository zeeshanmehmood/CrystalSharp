using CrystalSharp.Common.Extensions;
using CrystalSharp.EntityFrameworkCore.Common.Interceptors;
using CrystalSharp.EntityFrameworkCore.Common.Interceptors.Exceptions;
using CrystalSharp.Infrastructure.ReadModelStoresPersistence;
using CrystalSharp.Oracle.Settings;
using CrystalSharp.Oracle.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrystalSharp.Oracle.Extensions
{
    public static class CrystalSharpAdapterOracleExtensions
    {
        public static ICrystalSharpAdapter AddOracle<TDbContext>(
            this ICrystalSharpAdapter crystalSharpAdapter,
            OracleSettings settings,
            params Type[] interceptors)
            where TDbContext : DbContext
        {
            RegisterDefaultInterceptorsIfRequired(crystalSharpAdapter);

            if (interceptors.HasAny())
            {
                foreach (Type interceptor in interceptors)
                {
                    ValidateDbInterceptor(interceptor);
                    crystalSharpAdapter.ServiceCollection.AddSingleton(interceptor);
                }
            }

            crystalSharpAdapter.ServiceCollection.AddDbContext<TDbContext>((sp, options) =>
            {
                if (settings.LazyLoading)
                {
                    options = options.UseLazyLoadingProxies(settings.LazyLoading);
                }

                List<IInterceptor> interceptorsToRegister = [sp.GetRequiredService<DateTractionInterceptor>(), sp.GetRequiredService<DispatchDomainEventsInterceptor>()];

                if (interceptors.HasAny())
                {
                    foreach (Type interceptor in interceptors)
                    {
                        interceptorsToRegister.Add(sp.GetRequiredService(interceptor) as IInterceptor);
                    }
                }

                options = options.UseOracle(settings.ConnectionString).AddInterceptors(interceptorsToRegister);
            });

            return crystalSharpAdapter;
        }

        public static ICrystalSharpAdapter AddOracleReadModelStore<TDbContext, TKey>(
            this ICrystalSharpAdapter crystalSharpAdapter,
            OracleSettings settings,
            params Type[] interceptors)
            where TDbContext : DbContext
        {
            if (interceptors.HasAny())
            {
                foreach (Type interceptor in interceptors)
                {
                    ValidateDbInterceptor(interceptor);
                    crystalSharpAdapter.ServiceCollection.AddSingleton(interceptor);
                }
            }

            crystalSharpAdapter.ServiceCollection.AddDbContext<TDbContext>((sp, options) =>
            {
                if (settings.LazyLoading)
                {
                    options = options.UseLazyLoadingProxies(settings.LazyLoading);
                }

                List<IInterceptor> interceptorsToRegister = [];

                if (interceptors.HasAny())
                {
                    foreach (Type interceptor in interceptors)
                    {
                        interceptorsToRegister.Add(sp.GetRequiredService(interceptor) as IInterceptor);
                    }
                }

                if (interceptorsToRegister.HasAny())
                {
                    options = options.UseOracle(settings.ConnectionString).AddInterceptors(interceptorsToRegister);
                }
                else
                {
                    options = options.UseOracle(settings.ConnectionString);
                }
            });

            crystalSharpAdapter.ServiceCollection.AddScoped<IReadModelStore<TKey>, OracleReadModelStore<TDbContext, TKey>>();

            return crystalSharpAdapter;
        }

        private static void ValidateDbInterceptor(Type interceptor)
        {
            if (!typeof(IInterceptor).IsAssignableFrom(interceptor))
            {
                string errorMessage = $"{interceptor.Name} must implement the {nameof(IInterceptor).ToDoubleQuotes()} interface of Entity Framework Core.";

                throw new InvalidDbInterceptorException(errorMessage);
            }
        }

        private static void RegisterDefaultInterceptorsIfRequired(ICrystalSharpAdapter crystalSharpAdapter)
        {
            ServiceDescriptor dateTractionInterceptorDescriptor = crystalSharpAdapter.ServiceCollection
                .SingleOrDefault(x => x.ImplementationType == typeof(DateTractionInterceptor));
            ServiceDescriptor dispatchDomainEventsInterceptorDescriptor = crystalSharpAdapter.ServiceCollection
                .SingleOrDefault(x => x.ImplementationType == typeof(DispatchDomainEventsInterceptor));

            if (dateTractionInterceptorDescriptor is null)
            {
                ValidateDbInterceptor(typeof(DateTractionInterceptor));
                crystalSharpAdapter.ServiceCollection.AddSingleton<DateTractionInterceptor>();
            }

            if (dispatchDomainEventsInterceptorDescriptor is null)
            {
                ValidateDbInterceptor(typeof(DispatchDomainEventsInterceptor));
                crystalSharpAdapter.ServiceCollection.AddSingleton<DispatchDomainEventsInterceptor>();
            }
        }
    }
}
