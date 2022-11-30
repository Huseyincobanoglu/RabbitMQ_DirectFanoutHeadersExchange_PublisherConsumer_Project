using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Rabbit.Consumer
{
    internal class Program
    {
        private static string queuename = "queuename";
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory { HostName = "127.0.0.1", Port = 5672, UserName = "guest", Password = "guest" };

            using (var connection=factory.CreateConnection())
            using (var channel=connection.CreateModel())
            {
                queuename = args.Length > 0 ? args[0] : "queue_name";
                EventingBasicConsumer consumer=new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var messagee = Encoding.UTF8.GetString(body);
                    Console.WriteLine($"Received : {messagee}");
                };
                channel.BasicConsume(queuename, true, consumer);
                Console.WriteLine($"{queuename} Listening \n");
                Console.WriteLine();
            }
        }
    }
}
