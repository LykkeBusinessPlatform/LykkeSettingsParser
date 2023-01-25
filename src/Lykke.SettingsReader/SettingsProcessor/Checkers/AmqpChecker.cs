using System;
using RabbitMQ.Client;

namespace Lykke.SettingsReader.Checkers
{
    // todo: the checker has to be moved to RabbitMq package
    internal class AmqpChecker : ISettingsFieldChecker
    {
        public CheckFieldResult CheckField(object model, string propertyName, string value)
        {
            ConnectionFactory factory;
            try
            {
                factory = new ConnectionFactory {Uri = new Uri(value, UriKind.Absolute)};
            }
            catch
            {
                return CheckFieldResult.Failed(propertyName, value);
            }
            
            var schema = factory.Ssl.Enabled ? "amqps" : "amqp";
            var url = $"{schema}://{factory.HostName}:{factory.Port}";

            try
            {
                using (var connection = factory.CreateConnection())
                {
                    bool checkResult = connection.IsOpen;
                    return checkResult
                        ? CheckFieldResult.Ok(propertyName, url)
                        : CheckFieldResult.Failed(propertyName, url);
                }
            }
            catch
            {
                return CheckFieldResult.Failed(propertyName, url);
            }
        }
    }
}
