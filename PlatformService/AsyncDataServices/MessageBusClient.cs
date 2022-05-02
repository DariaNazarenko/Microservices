using System;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PlatformService.Dtos;
using RabbitMQ.Client;

namespace PlatformService.AsyncDataServices
{
    public class MessageBusClient : IMessageBusClient
    {
        private readonly IConfiguration _configuration;
        private readonly IConnection _connection;
        private readonly IModel _chanel;

        public MessageBusClient(IConfiguration configuration)
        {
            _configuration = configuration;
            var factory = new ConnectionFactory() {HostName = _configuration["RabbitMQHost"], Port = int.Parse(_configuration["RabbitMQPort"])};

            try
            {
                _connection = factory.CreateConnection();
                _chanel = _connection.CreateModel();

                _chanel.ExchangeDeclare(exchange: "trigger", type: ExchangeType.Fanout);
                _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

                Console.WriteLine("--> Connected to MessageBus");
            }
            catch(Exception e)
            {
                System.Console.WriteLine($"Could not connect to the message bus: {e.Message}");
            }
        }

        public void PublishNewPlatform(PlatformPublishedhDto platformPublishedDto)
        {
            var message = JsonSerializer.Serialize(platformPublishedDto);

            if(_connection.IsOpen)
            {
                System.Console.WriteLine("RabbitMQ connection open");
                SendMessage(message);
            }
            else
            {
                System.Console.WriteLine("RabbitMQ connection closed");
            }
        }

        private void SendMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);

            _chanel.BasicPublish(exchange: "trigger", routingKey: "", basicProperties: null, body: body);

            System.Console.WriteLine($"Sent message {message}");
        }

        public void Dispose()
        {
            if(_chanel.IsOpen)
            {
                _chanel.Close();
                _connection.Close();
            }
            
            System.Console.WriteLine("Message bus disposed");
        }

        public void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            Console.WriteLine("--> Connection to MessageBus shutdown");
        }
    }
}