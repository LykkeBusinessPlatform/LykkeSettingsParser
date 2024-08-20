using System;
using System.Threading.Tasks;

namespace Lykke.SettingsReader
{
    /// <summary>
    /// Provides reloading functionality for any kind of data.
    /// </summary>
    /// <typeparam name="T">Type of data to be loaded/reloaded</typeparam>
    public interface IReloadingManager<T>
    {
        /// <summary>
        /// Finish flag for initial data load
        /// </summary>
        bool HasLoaded { get; }

        /// <summary>
        /// Current data value
        /// </summary>
        T CurrentValue { get; }

        /// <summary>
        /// Forces data to be reloaded
        /// </summary>
        Task<T> Reload();

        /// <summary>
        /// Check if data was reloaded during time from provided moment
        /// </summary>
        bool WasReloadedFrom(DateTime dateTime);
    }
}