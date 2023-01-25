using Microsoft.WindowsAzure.Storage;

namespace Lykke.SettingsReader.Checkers
{
    // todo: the checker has to be removed
    internal class AzureQueueChecker : ISettingsFieldChecker
    {
        public CheckFieldResult CheckField(object model, string propertyName, string value)
        {
            string url = string.Empty;
            
            try
            {
                var account = CloudStorageAccount.Parse(value);
                var client = account.CreateCloudQueueClient();
                url = client.BaseUri.ToString();
                client.ListQueuesSegmentedAsync(null).GetAwaiter().GetResult();
                return CheckFieldResult.Ok(propertyName, url);
            }
            catch
            {
                return CheckFieldResult.Failed(propertyName, url);
            }
        }
    }
}
