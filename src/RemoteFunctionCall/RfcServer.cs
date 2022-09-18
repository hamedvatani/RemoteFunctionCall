using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RemoteFunctionCall
{
    public class RfcServer
    {
        private readonly RfcConfiguration _configuration;
        private IConnection _connection = null!;
        private IModel _channel = null!;
        private EventingBasicConsumer _consumer = null!;

        public RfcServer(RfcConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Start(Func<RfcFunctionDto, RfcResultDto> function)
        {
            var factory = new ConnectionFactory()
            {
                HostName = _configuration.RabbitMqHostName,
                UserName = _configuration.RabbitMqUserName,
                Password = _configuration.RabbitMqPassword
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(_configuration.InputQueueName, true, false, false);
            _channel.QueueDeclare(_configuration.OutputQueueName, true, false, false);
            _consumer = new EventingBasicConsumer(_channel);

            _consumer.Received += (_, ea) =>
            {
                var body = ea.Body.ToArray();
                var props = ea.BasicProperties;
                var replyProps = _channel.CreateBasicProperties();
                replyProps.CorrelationId = props.CorrelationId;

                RfcResultDto response = new RfcResultDto(false, false, "");
                try
                {
                    var message = new RfcFunctionDto(body);
                    response = function.Invoke(message);
                }
                catch (Exception e)
                {
                    response = new RfcResultDto(false, false, e.Message);
                }
                finally
                {
                    _channel.BasicPublish(exchange: "", routingKey: _configuration.OutputQueueName,
                        basicProperties: replyProps, body: response.Serialize());
                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
            };

            _channel.BasicConsume(
                consumer: _consumer,
                queue: _configuration.InputQueueName,
                autoAck: false);
        }

        public void Stop()
        {
            _channel.Close();
            _connection.Close();
        }
    }
}