using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace Lykke.SettingsReader
{
    /// <summary>
    /// Interface for IReloadingManager that provides Microsoft.Extensions.Configuration.IConfiguration functionality
    /// </summary>
    /// <typeparam name="T">Type of data to be loaded/reloaded</typeparam>
    public interface IReloadingManagerWithConfiguration<T> : IReloadingManager<T>
    {
        /// <summary>
        /// Property that contains Microsoft.Extensions.Configuration.IConfiguration implementation for IReloadingManager
        /// </summary>
        [NotNull]
        IConfiguration SettingsConfiguration { get; }
    }
}
