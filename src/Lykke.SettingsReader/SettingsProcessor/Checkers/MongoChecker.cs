using System;
using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters;

namespace Lykke.SettingsReader.Checkers
{
    internal class MongoChecker : ISettingsFieldChecker
    {
        public CheckFieldResult CheckField(object model, string propertyName, string value)
        {
            try
            {
                var mongoUrl = MongoUrl.Create(value);

                var client = new MongoClient(mongoUrl);
                
                client.ListDatabases();
                
                return client.Cluster.Description.State == ClusterState.Connected
                    ? CheckFieldResult.Ok(propertyName, value)
                    : CheckFieldResult.Failed(propertyName, value);
            }
            catch (MongoConfigurationException)
            {
                return CheckFieldResult.Failed(propertyName, value);
            }
            catch (TimeoutException)
            {
                return CheckFieldResult.Failed(propertyName, value);
            }
        }
    }
}
