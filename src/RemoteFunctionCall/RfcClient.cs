using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RemoteFunctionCall
{
    public class RfcClient
    {
        private readonly RfcConfiguration _configuration;

        private IConnection _connection = null!;
        private IModel _channel = null!;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<RfcResultDto>?> _callbackMapper = new ConcurrentDictionary<string, TaskCompletionSource<RfcResultDto>?>();

        public RfcClient(RfcConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<RfcResultDto> CallAsync(RfcFunctionDto message, CancellationToken cancellationToken = default)
        {
            IBasicProperties props = _channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;
            var tcs = new TaskCompletionSource<RfcResultDto>();
            _callbackMapper.TryAdd(correlationId, tcs);

            _channel.BasicPublish(
                exchange: "",
                routingKey: _configuration.InputQueueName,
                basicProperties: props,
                body: message.Serialize());

            cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out _));
            return tcs.Task;
        }

        public RfcResultDto Call(RfcFunctionDto message, CancellationToken cancellationToken = default)
        {
            var t = CallAsync(message, cancellationToken);
            if (Task.WhenAny(t, Task.Delay(_configuration.Timeout, cancellationToken)).Result != t)
                return new RfcResultDto(false, true, "Timeout");
            return t.Result;
        }

        public void Start()
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
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (_, ea) =>
            {
                if (!_callbackMapper.TryRemove(ea.BasicProperties.CorrelationId, out TaskCompletionSource<RfcResultDto>? tcs))
                    return;
                if (tcs == null)
                    return;
                var body = ea.Body.ToArray();
                var response = new RfcResultDto(body);
                tcs.TrySetResult(response);
            };

            _channel.BasicConsume(
                consumer: consumer,
                queue: _configuration.OutputQueueName,
                autoAck: true);
        }

        public void Stop()
        {
            _channel.Close();
            _connection.Close();
        }
    }
}