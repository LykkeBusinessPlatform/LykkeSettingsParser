using Microsoft.WindowsAzure.Storage;

namespace Lykke.SettingsReader.Checkers
{
    // todo: the checker has to be removed
    internal class AzureBlobChecker : ISettingsFieldChecker
    {
        public CheckFieldResult CheckField(object model, string propertyName, string value)
        {
            string url = string.Empty;
            
            try
            {
                var account = CloudStorageAccount.Parse(value);
                var client = account.CreateCloudBlobClient();
                url = client.BaseUri.ToString();
                client.ListContainersSegmentedAsync(null).GetAwaiter().GetResult();
                return CheckFieldResult.Ok(propertyName, url);
            }
            catch
            {
                return CheckFieldResult.Failed(propertyName, url);
            }
        }
    }
}
