using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Rabbit.Publisher.Models;
using RabbitMQ.Client;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Rabbit.Publisher.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private IConnection connection;
        private string Url = "amqp://guest:guest@localhost:5672";
        private IModel channel => CreateChannel();
        private const string EXCHANGE_NAME = "exchange_name";
        private const string QUEUE_NAME = "queue_name";
        private const string FANOUT_ROUTİNG_KEY = "fanout_routing_key";
        private const string EXCHANGE_FANOUT_NAME = "fanout_exchange_name";

        public HomeController()
        {

        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get()
        {
            //DirectExchange();
            HeaderExchange();
            connection.Close();
            return Ok();
        }

        [HttpGet("getfanout")]
        public IActionResult GetFanout(string queue_name)
        {
            FanoutExchange(queue_name);
            connection.Close();
            return Ok();
        }
        private void HeaderExchange()
        {
            byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new Users { Id = 1, Name = "Hüseyin", Password = "1234" }));
            channel.ExchangeDeclare(EXCHANGE_NAME, ExchangeType.Headers);
            channel.QueueDeclare(QUEUE_NAME, false, false, false);
            channel.QueueBind(QUEUE_NAME, EXCHANGE_NAME, string.Empty, new Dictionary<string, object>{
                {"x-match","all" },
                {"op","convert" },
                {"format","png" },
            });
            var props = channel.CreateBasicProperties();
            props.Headers= new Dictionary < string, object>{
                { "op","convert" },
                { "format","png" },
            };
            channel.BasicPublish(EXCHANGE_NAME, string.Empty, props, data);
        }
        private void FanoutExchange(string queue_name)
        {
            byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new Users { Id = 1, Name = "Hüseyin", Password = "1234" }));
            channel.ExchangeDeclare(EXCHANGE_FANOUT_NAME, ExchangeType.Fanout);
            channel.QueueDeclare(queue_name, false, false, false, null);
            channel.QueueBind(queue_name, EXCHANGE_FANOUT_NAME, FANOUT_ROUTİNG_KEY);
            channel.BasicPublish(EXCHANGE_NAME, FANOUT_ROUTİNG_KEY, null, data);
        }

        private void DirectExchange()
        {
            byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new Users { Id = 1, Name = "Hüseyin", Password = "1234" }));
            channel.ExchangeDeclare(EXCHANGE_NAME, ExchangeType.Direct);
            channel.QueueDeclare(QUEUE_NAME, false, false, false, null);
            channel.QueueBind(QUEUE_NAME, EXCHANGE_NAME, QUEUE_NAME);
            channel.BasicPublish(EXCHANGE_NAME, QUEUE_NAME, null, data);
        }

        private IModel CreateChannel()
        {
            if (connection == null)
            {
                connection = GetConnection();
                return connection.CreateModel();
            }
            else
            {
                return connection.CreateModel();
            }
        }

        private IConnection GetConnection()
        {

            ConnectionFactory factory = new ConnectionFactory()
            {
                Uri = new Uri(Url, UriKind.RelativeOrAbsolute)
            };

            return factory.CreateConnection();
        }
    }
}
