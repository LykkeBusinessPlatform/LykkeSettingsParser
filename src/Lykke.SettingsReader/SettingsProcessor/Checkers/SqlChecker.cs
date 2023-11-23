using System;
using System.Data.SqlClient;

namespace Lykke.SettingsReader.Checkers
{
    // todo: the checker has to be moved to SQL package
    internal class SqlChecker : ISettingsFieldChecker
    {
        public CheckFieldResult CheckField(object model, string propertyName, string value)
        {
            var url = string.Empty;
            
            try
            {
                var sb = new SqlConnectionStringBuilder(value);
                url = sb.DataSource;
                
                using (var connection = new SqlConnection(sb.ConnectionString))
                {
                    connection.Open();
                }
                return CheckFieldResult.Ok(propertyName, url);
            }
            catch (Exception ex)
            {
                return CheckFieldResult.Failed(propertyName, url, ex.Message);
            }
        }
    }
}
