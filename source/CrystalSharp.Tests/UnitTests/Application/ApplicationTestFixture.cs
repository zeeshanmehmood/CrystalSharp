using CrystalSharp.Application.Execution;
using CrystalSharp.Tests.Common;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CrystalSharp.Tests.UnitTests.Application
{
    public class ApplicationTestFixture : UnitTestBase, IDisposable
    {
        public ICommandExecutor CommandExecutor { get; private set; }
        public IQueryExecutor QueryExecutor { get; private set; }
        public INotificationPublisher NotificationPublisher { get; private set; }

        public ApplicationTestFixture()
        {
            IResolver resolver = ConfigureCrystalSharpAdapter(new ServiceCollection());
            CommandExecutor = resolver.Resolve<ICommandExecutor>();
            QueryExecutor = resolver.Resolve<IQueryExecutor>();
            NotificationPublisher = resolver.Resolve<INotificationPublisher>();
        }

        public void Dispose()
        {
            //
        }
    }
}
