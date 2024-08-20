using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;

namespace Lykke.SettingsReader.ReloadingManager.Configuration
{
    /// <summary>
    /// Microsoft.Extensions.Configuration.IConfigurationSection implementation for logging configuration section
    /// </summary>
    internal class SettingsConfigurationSection<T> : IConfigurationSection
    {
        private readonly IReloadingManager<T> _manager;
        private readonly JToken _token;
        private IEnumerable<JToken> _children;

        /// <summary>
        /// Gets the key this section occupies in its parent.
        /// </summary>
        [NotNull]
        public string Key { get; private set; }

        /// <summary>
        /// Gets the full path to this section within the Microsoft.Extensions.Configuration.IConfiguration.
        /// </summary>
        [NotNull]
        public string Path { get; private set; }

        /// <summary>
        /// Gets or sets the section value.
        /// </summary>
        [AllowNull]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets a configuration value.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <returns>The configuration value.</returns>
        [AllowNull]
        public string this[[NotNull] string key]
        {
            get
            {
                if (key == null)
                    throw new ArgumentNullException();
                var item = _token[key];
                if (item == null)
                    return null;
                if (item.Type == JTokenType.Array || item.Type == JTokenType.Object)
                    return item.ToString(Formatting.None);
                return item.ToString();
            }
            set => throw new NotImplementedException();
        }

        internal SettingsConfigurationSection(
            IReloadingManager<T> manager,
            string key,
            string path,
            JToken token)
        {
            _manager = manager;
            Key = key;
            Path = path;
            _token = token;
            if (token == null)
            {
                Value = null;
                _children = new JToken[0];
            }
            else if (token.Type == JTokenType.Array)
            {
                Value = token.ToString(Formatting.None);
                _children = new JToken[0];
            }
            else
            {
                Value = token.Type == JTokenType.Object ? token.ToString(Formatting.None) : token.ToString();
                _children = token.Children();
            }
        }

        /// <summary>
        /// Gets the immediate descendant configuration sub-sections.
        /// </summary>
        /// <returns>The configuration sub-sections.</returns>
        public IEnumerable<IConfigurationSection> GetChildren()
        {
            return _children
                .Select(p => new SettingsConfigurationSection<T>(
                    _manager,
                    GetKeyFromPath(p.Path),
                    p.Path,
                    p.Type == JTokenType.Property ? p.First : p))
                .ToList();
        }

        /// <summary>
        /// Returns a Microsoft.Extensions.Primitives.IChangeToken that can be used to observe when this configuration is reloaded.
        /// </summary>
        /// <returns>A Microsoft.Extensions.Primitives.IChangeToken.</returns>
        public IChangeToken GetReloadToken()
        {
            return new SettingsChangeToken<T>(_manager);
        }

        /// <summary>
        /// Gets a configuration sub-section with the specified key.
        /// </summary>
        /// <param name="key">The key of the configuration section.</param>
        /// <returns>The Microsoft.Extensions.Configuration.IConfigurationSection.</returns>
        /// <remarks>This method will never return null. If no matching sub-section is found with the specified key,
        /// an empty Microsoft.Extensions.Configuration.IConfigurationSection will be returned.</remarks>
        public IConfigurationSection GetSection([NotNull] string key)
        {
            if (key == null)
                throw new ArgumentNullException();
            JToken childToken = null;
            try
            {
                childToken = _token.HasValues ? _token.Value<JToken>(key) : null;
            }
            catch
            {
            }
            return new SettingsConfigurationSection<T>(
                _manager,
                key,
                GetPath(key, childToken, _token),
                childToken);
        }

        private string GetPath(string key, JToken token, JToken parentToken)
        {
            string path = key;
            if (token != null)
            {
                if (_token.Type != JTokenType.Property)
                    token = token.Parent;
                if (!string.IsNullOrEmpty(token.Path))
                    path = token.Path.Replace('.', ':');
            }
            else if (parentToken != null)
            {
                path = string.IsNullOrEmpty(parentToken.Path) ? key : $"{parentToken.Path.Replace('.', ':')}:{key}";
            }
            return path;
        }

        private string GetKeyFromPath(string path)
        {
            string key = path;
            int ind = path.LastIndexOf('.');
            if (ind != -1)
                key = path.Substring(ind + 1);
            return key;
        }
    }
}
