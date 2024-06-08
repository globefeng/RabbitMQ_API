using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using System;
using System.Threading;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        [HttpPost]
        public void Post([FromBody] MyMessage myMessage)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: myMessage.QueueName,
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    var body = Encoding.UTF8.GetBytes(myMessage.Message);

                    channel.BasicPublish(exchange: "",
                                         routingKey: myMessage.QueueName,
                                         basicProperties: null,
                                         body: body);
                }
            }
        }

        [HttpGet]
        public string Get(string queueName)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            int messageCount = 0;
            using (var connection = factory.CreateConnection())
            {
                using (var rabbitMqChannel = connection.CreateModel())
                {

                    rabbitMqChannel.QueueDeclare(queue: queueName,
                                                 durable: false,
                                                 exclusive: false,
                                                 autoDelete: false,
                                                 arguments: null);

                    rabbitMqChannel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                    messageCount = Convert.ToInt16(rabbitMqChannel.MessageCount(queueName));
                }
            }

            return String.Format("There are {0} messages in the queue.", messageCount);
        }
    }

    public class MyMessage
    {
        public string QueueName { get; set; }
        public string Message { get; set; }
    }
}
