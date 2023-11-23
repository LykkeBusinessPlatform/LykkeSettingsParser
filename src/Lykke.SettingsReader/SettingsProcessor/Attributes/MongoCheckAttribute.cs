using Lykke.SettingsReader.Checkers;

namespace Lykke.SettingsReader.Attributes
{
    /// <summary>
    /// Attribute for MongoDb connection string check
    /// </summary>
    public class MongoCheckAttribute : BaseCheckAttribute
    {
        /// <inheritdoc />
        public override ISettingsFieldChecker GetChecker()
        {
            return new MongoChecker();
        }
    }
}
