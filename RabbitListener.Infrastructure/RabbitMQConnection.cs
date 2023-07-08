using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace RabbitListener.Infrastructure
{
    public class RabbitMQConnection
    {
        public string Hostname { get; }
        public int Port { get; }
        public string Username { get; }
        public string Password { get; }
        public string VirtualHost { get; }
        public string QueueName { get; }

        public RabbitMQConnection(IConfiguration configuration)
        {
            Hostname = configuration["RabbitMQ:Hostname"];
            Port = int.Parse(configuration["RabbitMQ:Port"]);
            Username = configuration["RabbitMQ:Username"];
            Password = configuration["RabbitMQ:Password"];
            VirtualHost = configuration["RabbitMQ:VirtualHost"];
            QueueName = configuration["RabbitMQ:QueueName"];
        }

        public IModel CreateChannel()
        {
            var factory = new ConnectionFactory()
            {
                HostName = Hostname,
                Port = Port,
                UserName = Username,
                Password = Password,
                VirtualHost = VirtualHost
            };

            var connection = factory.CreateConnection();
            return connection.CreateModel();
        }
    }
}
