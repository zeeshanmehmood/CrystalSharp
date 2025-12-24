using CrystalSharp.Messaging.Distributed;
using CrystalSharp.Messaging.RabbitMq.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CrystalSharp.Messaging.RabbitMq.Extensions
{
    public static class CrystalSharpAdapterRabbitMqExtensions
    {
        extension (ICrystalSharpAdapter crystalSharpAdapter)
        {
            public ICrystalSharpAdapter AddRabbitMq(RabbitMqSettings settings, RabbitMqChannelConfiguration channelConfiguration = null)
            {
                crystalSharpAdapter.ServiceCollection.AddScoped<IRabbitMqConnectionFactory, RabbitMqConnectionFactory>();
                crystalSharpAdapter.ServiceCollection.AddScoped<IMessageBroker>(s => 
                {
                    IRabbitMqConnectionFactory connectionFactory = s.GetRequiredService<IRabbitMqConnectionFactory>();
                    RabbitMqMessageBroker messageBroker = (channelConfiguration is null)
                    ?
                    new RabbitMqMessageBroker(settings, connectionFactory)
                    :
                    new RabbitMqMessageBroker(settings, channelConfiguration, connectionFactory);

                    return messageBroker;
                });

                return crystalSharpAdapter;
            }
        }
    }
}
