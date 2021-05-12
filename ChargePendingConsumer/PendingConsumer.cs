using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace ChargePendingConsumer
{
    public class PendingConsumer : IPendingConsumer
    {
        private readonly IConnectionFactory _factory;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<PendingConsumer> _logger;
        public PendingConsumer(ILogger<PendingConsumer> logger)
        {
            _logger = logger;
            _factory = new ConnectionFactory()
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "pending.charges", durable: true, exclusive: false, autoDelete: false, arguments: null);
        }
        public void Consume()
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                _logger.LogInformation(message);
            };

            _channel.BasicConsume(queue: "pending.charges", autoAck: true, consumer: consumer);

            Console.ReadLine();
        }
    }
}
