namespace RemoteFunctionCall
{
    public class RfcConfiguration
    {
        public string RabbitMqHostName { get; set; } = "localhost";
        public string RabbitMqUserName { get; set; } = "guest";
        public string RabbitMqPassword { get; set; } = "guest";
        public string InputQueueName { get; set; } = "RfcQueueName.Input";
        public string OutputQueueName { get; set; } = "RfcQueueName.Output";
        public int Timeout { get; set; } = 30000;
    }
}