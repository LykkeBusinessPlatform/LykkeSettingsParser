using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Lykke.SettingsReader.Attributes;

namespace Lykke.SettingsReader.Test.Models
{
    public class ConfigurationModel
    {
        [Optional]
        public string StringProperty { get; set; }

        [Optional]
        public DateTime DateTimeProperty { get; set; }

        [Optional]
        public DateTime? NullableDateTimeProperty { get; set; }

        [Optional]
        [JsonConverter(typeof(StringEnumConverter))]
        public ConfigurationModelEnum EnumAsStringProperty { get; set; }

        [Optional]
        public ConfigurationModelEnum EnumAsIntProperty { get; set; }

        [Optional]
        public List<string> ListProperty { get; set; }

        [Optional]
        public string[] ArrayProperty { get; set; }

        [Optional]
        public ConfigurationModelSection1 Section1 { get; set; }
        
        [Optional]
        public List<NestedCollectionSection1> NestedCollectionSection1 { get; set; }
    }
    public class ConfigurationModel2
    {

        [Optional]
        public List<NestedCollectionSection1> NestedCollectionSection1 { get; set; }
    }
    public enum ConfigurationModelEnum
    {
        One,
        Two,
    }

    public class ConfigurationModelSection1
    {
        [Optional]
        public string StringProperty1 { get; set; }
    }

    public class NestedCollectionSection1
    {
        public List<ConfigurationModelSection1> NestedConfigSection { get; set; }
    }
}
