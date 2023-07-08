using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitListener.Infrastructure;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitListener.Application
{
    public class RabbitListenerService : BackgroundService
    {
        private readonly ILogger<RabbitListenerService> _logger;
        private readonly RabbitMQConnection _rabbitMQConnection;

        public RabbitListenerService(ILogger<RabbitListenerService> logger, RabbitMQConnection rabbitMQConnection)
        {
            _logger = logger;
            _rabbitMQConnection = rabbitMQConnection;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var channel = _rabbitMQConnection.CreateChannel();

            channel.QueueDeclare(queue: _rabbitMQConnection.QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (sender, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = System.Text.Encoding.UTF8.GetString(body);

                _logger.LogInformation("Received message: {Message}", message);

                if (Uri.TryCreate(message, UriKind.Absolute, out var uri))
                {
                    var statusCode = await GetStatusCodeAsync(uri);
                    LogStatusCode(message, statusCode);
                }

                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            channel.BasicConsume(queue: _rabbitMQConnection.QueueName, autoAck: false, consumer: consumer);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task<HttpStatusCode> GetStatusCodeAsync(Uri uri)
        {
            try
            {
                var request = WebRequest.CreateHttp(uri);
                request.Method = "HEAD";

                using (var response = await request.GetResponseAsync() as HttpWebResponse)
                {
                    return response.StatusCode;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending HEAD request to {Uri}", uri);
                return HttpStatusCode.BadRequest;
            }
        }

        private void LogStatusCode(string url, HttpStatusCode statusCode)
        {
            var logObject = new
            {
                ServiceName = "RabbitListener",
                Url = url,
                StatusCode = (int)statusCode
            };

            _logger.LogInformation("Status code logged: {LogObject}", logObject);
        }
    }
}
