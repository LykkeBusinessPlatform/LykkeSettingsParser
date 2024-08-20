using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Primitives;

namespace Lykke.SettingsReader.ReloadingManager.Configuration
{
    /// <summary>
    /// Provides Microsoft.Extensions.Primitives.IChangeToken functionality for LoggingConfiguration
    /// </summary>
    internal class SettingsChangeToken<T> : IChangeToken
    {
        /// <summary>
        /// Handles ChangeCallback unregistration for SettingsChangeToken
        /// </summary>
        internal class CallbackUnregisterHandler : IDisposable
        {
            private readonly Action _unregisterCallback;

            internal CallbackUnregisterHandler(Action unregisterCallback)
            {
                _unregisterCallback = unregisterCallback;
            }

            /// <summary>
            /// Actually unregisters change callback from SettingsChangeToken
            /// </summary>
            public void Dispose() => _unregisterCallback();
        }

        private readonly List<(Action<object>, object)> _callbacks = new List<(Action<object>, object)>();
        private readonly IReloadingManager<T> _manager;

        private DateTime _lastChangeCheck;

        /// <summary>
        /// Gets a value that indicates if a change has occured.
        /// </summary>
        public bool HasChanged
        {
            get
            {
                bool wasReloaded = _manager.WasReloadedFrom(_lastChangeCheck);
                _lastChangeCheck = DateTime.UtcNow;
                if (wasReloaded)
                    foreach (var callbackPair in _callbacks)
                    {
                        try
                        {
                            callbackPair.Item1(callbackPair.Item2);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                return wasReloaded;
            }
        }

        /// <summary>
        /// Indicates if this token will pro-actively raise callbacks. Callbacks are still guaranteed to fire, eventually.
        /// </summary>
        public bool ActiveChangeCallbacks => false;

        /// <summary>
        /// C-tor
        /// </summary>
        internal SettingsChangeToken(IReloadingManager<T> manager)
        {
            _manager = manager;
            _lastChangeCheck = DateTime.UtcNow;
        }

        /// <summary>
        /// Registers for a callback that will be invoked when the entry has changed.
        /// Microsoft.Extensions.Primitives.IChangeToken.HasChanged MUST be set before the callback is invoked.
        /// </summary>
        /// <param name="callback">The System.Action`1 to invoke.</param>
        /// <param name="state">State to be passed into the callback.</param>
        /// <returns>An System.IDisposable that is used to unregister the callback.</returns>
        public IDisposable RegisterChangeCallback([NotNull] Action<object> callback, [AllowNull] object state)
        {
            var callBackPair = (callback, state);
            _callbacks.Add(callBackPair);

            return new CallbackUnregisterHandler(() => _callbacks.Remove(callBackPair));
        }
    }
}
