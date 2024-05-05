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

using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;
using CrystalSharp.Envoy;
using CrystalSharp.Tests.Common;
using CrystalSharp.Tests.Common.Envoy.Notifications;
using CrystalSharp.Tests.Common.Envoy.Requests;
using CrystalSharp.Tests.Common.Envoy.Responses;

namespace CrystalSharp.Tests.UnitTests.Envoy
{
    [Trait(TestSettings.Category, TestType.Unit)]
    public class EnvoyTests : IClassFixture<EnvoyTestFixture>
    {
        private readonly EnvoyTestFixture _fixture;

        public EnvoyTests(EnvoyTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Request_handler_executed()
        {
            // Arrange
            CreateProductRequest request = new() { Name = "Laptop", Price = 300 };
            IEnvoy envoy = _fixture.Envoy;

            // Act
            CreateProductResponse result = await envoy.Send(request, CancellationToken.None).ConfigureAwait(false);

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
            await envoy.Publish(notification, CancellationToken.None).ConfigureAwait(false);

            // Assert
            notification.Description.Should().Be(description);
        }

        [Fact]
        public async Task Notification_published_to_multiple_handlers()
        {
            // Arrange
            PostCreatedNotification notification = new() { Title = "New Post" };
            string[] status = new[] { "Verified", "Active" };
            IEnvoy envoy = _fixture.Envoy;

            // Act
            await envoy.Publish(notification, CancellationToken.None).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                notification.Status.Contains(status[0]).Should().BeTrue();
                notification.Status.Contains(status[1]).Should().BeTrue();
            }
        }
    }
}
