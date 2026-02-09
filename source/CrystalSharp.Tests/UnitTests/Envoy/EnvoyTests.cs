using CrystalSharp.Envoy;
using CrystalSharp.Tests.Common;
using CrystalSharp.Tests.Common.Envoy.Notifications;
using CrystalSharp.Tests.Common.Envoy.Requests;
using CrystalSharp.Tests.Common.Envoy.Responses;
using FluentAssertions;
using FluentAssertions.Execution;
using System.Threading.Tasks;

namespace CrystalSharp.Tests.UnitTests.Envoy
{
    [Trait(TestSettings.Category, TestType.Unit)]
    public class EnvoyTests(EnvoyTestFixture fixture) : IClassFixture<EnvoyTestFixture>
    {
        private readonly EnvoyTestFixture _fixture = fixture;

        [Fact]
        public async Task Request_handler_executed()
        {
            // Arrange
            CreateProductRequest request = new() { Name = "Laptop", Price = 300 };
            IEnvoy envoy = _fixture.Envoy;

            // Act
            CreateProductResponse result = await envoy.Send(request, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.ProductName.Should().Be(request.Name);
                result.ProductPrice.Should().Be(request.Price);
            }
        }

        [Fact]
        public async Task Single_notification_published()
        {
            // Arrange
            ProductCreatedNotification notification = new() { Name = "Laptop", Price = 500 };
            string description = $"{notification.Name}: {notification.Price}";
            IEnvoy envoy = _fixture.Envoy;

            // Act
            await envoy.Publish(notification, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            notification.Description.Should().Be(description);
        }

        [Fact]
        public async Task Notification_published_to_multiple_handlers()
        {
            // Arrange
            PostCreatedNotification notification = new() { Title = "New Post" };
            string[] status = ["Verified", "Active"];
            IEnvoy envoy = _fixture.Envoy;

            // Act
            await envoy.Publish(notification, TestContext.Current.CancellationToken).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                notification.Status.Contains(status[0]).Should().BeTrue();
                notification.Status.Contains(status[1]).Should().BeTrue();
            }
        }
    }
}
