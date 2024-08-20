using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lykke.SettingsReader
{
    /// <summary>
    /// Base class for IReloadingManager functionality
    /// </summary>
    /// <typeparam name="TSettings">Type of data to be loaded/reloaded</typeparam>
    public abstract class ReloadingManagerBase<TSettings> : IReloadingManager<TSettings>
    {
        private readonly ReaderWriterLockSlim _sync = new ReaderWriterLockSlim();

        private Task<TSettings> _currentTask;
        private DateTime? _lastReloadTime;

        /// <summary>
        /// Current task for data loading
        /// </summary>
        protected Task<TSettings> CurrentTask => Load(reload: false);

        /// <summary>
        /// Loads data asynchronously
        /// </summary>
        protected abstract Task<TSettings> Load();

        /// <summary>
        /// Finish flag for initial data load
        /// </summary>
        public bool HasLoaded => CurrentTask.Status == TaskStatus.RanToCompletion;

        /// <summary>
        /// Current data value
        /// </summary>
        public TSettings CurrentValue => CurrentTask.GetAwaiter().GetResult();

        /// <summary>
        /// Forces data to be reloaded
        /// </summary>
        public Task<TSettings> Reload() => Load(reload: true);

        /// <summary>
        /// Check if data was reloaded during time from provided moment
        /// </summary>
        public bool WasReloadedFrom(DateTime dateTime)
        {
            if (!_lastReloadTime.HasValue)
                return false;

            return dateTime <= _lastReloadTime.Value;
        }

        private Task<TSettings> Load(bool reload)
        {
            bool CheckCurrentTask() => _currentTask != null && !(_currentTask.IsCompleted && reload);

            try
            {
                _sync.EnterReadLock();

                if (CheckCurrentTask())
                {
                    return _currentTask;
                }
            }
            finally
            {
                _sync.ExitReadLock();
            }

            try
            {
                _sync.EnterWriteLock();

                // double check

                if (CheckCurrentTask())
                {
                    return _currentTask;
                }

                if (_currentTask != null && _currentTask.IsCompleted)
                    _lastReloadTime = DateTime.UtcNow;

                return _currentTask = Load();
            }
            finally
            {
                _sync.ExitWriteLock();
            }
        }
    }
}