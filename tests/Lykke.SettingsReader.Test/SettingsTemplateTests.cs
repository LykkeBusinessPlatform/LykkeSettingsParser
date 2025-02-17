using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using Lykke.SettingsReader.ConfigurationProvider;
using Lykke.SettingsReader.Extensions;
using Lykke.SettingsReader.SettingsTemplate;
using Lykke.SettingsReader.Test.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace Lykke.SettingsReader.Test
{
    public class SettingsTemplateTests
    {
        private readonly IConfiguration _configuration;
        private readonly WireMockServer _server;
        private readonly IServiceProvider _sp;
        private readonly string _expectedTemplate = @"{""ArrayProperty"": [
                                                       ""Guid""
                                                     ],
                                                     ""DateTimeProperty"": ""DateTime"",
                                                     ""EnumAsIntProperty"": ""Integer"",
                                                     ""EnumAsStringProperty"": ""String"",
                                                     ""ListProperty"": [
                                                       ""Guid""
                                                     ],
                                                     ""NestedCollectionSection1"": [
                                                       {
                                                         ""NestedConfigSection"": [
                                                           {
                                                             ""StringProperty1"": ""String""
                                                           }
                                                         ]
                                                       }
                                                     ],
                                                     ""NullableDateTimeProperty"": ""DateTime"",
                                                     ""Section1"": {
                                                       ""StringProperty1"": ""String""
                                                     },
                                                     ""StringProperty"": ""String""
                                                   }";

        private readonly string[] _testVariables = new[]
        {
            "ASPNETCORE_URLS",
            "DOTNET_RUNNING_IN_CONTAINER",
            "DOTNET_SYSTEM_GLOBALIZATION_INVARIANT",
            "DOTNET_VERSION",
            "SYSTEM_GLOBALIZATION_INVARIANT",
            "HOME",
            "HOSTNAME",
            "PATH",
            "applicationName",
            "AXLE_PORT",
            "AXLE_PORT_5012_TCP",
            "AXLE_PORT_5012_TCP_ADDR",
            "AXLE_PORT_5012_TCP_PORT",
            "AXLE_PORT_5012_TCP_PROTO",
            "AXLE_SERVICE_HOST",
            "AXLE_SERVICE_PORT_HTTP",
            "BIGBROTHER_SNAPSHOT_WEBAPI_SERVICE_PORT_HTTP",
            "BIGBROTHER_SNAPSHOT_WEBAPI_SERVICE_HOST",
            "BIGBROTHER_SNAPSHOT_WEBAPI_PORT_5025_TCP_ADDR",
            "URLS",
            "VERSION",
            "KUBERNETES_SERVICE_PORT_HTTPS",
            "MDM_EXT_SERVICE_PORT_MDM_EXT",
            "MT_ASSET_SERVICE_SERVICE_PORT_SIDECAR",
            "MT_ASSET_SERVICE_SERVICE_PORT_SIDECAR_HTTP",
            "POSTGRES_SERVICE_PORT_5432",
            "RABBIT_MT_SERVICE_PORT_15672" ,
            "RABBIT_MT_SERVICE_PORT_5672",
            "TRADING_IDEAS_SERVICE_PORT_TRADING_IDEAS_MAIN_SERVICE",
            "TRADING_IDEAS_SERVICE_PORT_TRADING_IDEAS_MOCK",
            "contentRoot",
            "SettingsUrl"
        };

        public SettingsTemplateTests()
        {
            _server = WireMockServer.Start();
            var testModel = new Fixture().Create<ConfigurationModel>();

            const string path = "/config";
            _server.Given(Request.Create().WithPath(path).UsingGet())
                .RespondWith(Response.Create().WithBodyAsJson(testModel));

            _configuration = new ConfigurationBuilder()
                .AddHttpSourceConfiguration(new Uri($"{_server.Url}{path}"))
                .Build();

            _sp = new ServiceCollection()
                .AddSingleton(_configuration)
                .AddLogging()
                .AddSettingsTemplateGenerator()
                .BuildServiceProvider();
        }

        [Fact]
        public void Settings_template_in_json_has_to_be_created_from_IConfiguration()
        {
            var json = new DefaultJsonTemplateGenerator(_sp.GetService<ILogger<SettingsTemplateConverter>>(), _configuration, new TemplateFilters()).GenerateJsonSettingsTemplate();
            var expectedJson = JToken.Parse(_expectedTemplate);
            var actualJson = JToken.Parse(json);
            actualJson.Should().BeEquivalentTo(expectedJson);
        }

       [Fact]
        public void Settings_template_has_to_be_filtered_out_of_variables()
        {
            var variables = _testVariables.ToDictionary(s => s, s => "");

            const string path = "/envconfig";
            _server.Given(Request.Create().WithPath(path).UsingGet())
                .RespondWith(Response.Create().WithBodyAsJson(variables));

            var configuration = new ConfigurationBuilder()
                .AddHttpSourceConfiguration(new Uri($"{_server.Url}{path}"))
                .Build();

            var allTestVariablesAdded = _testVariables.All(variable => configuration.GetChildren().Select(section => section.Key).Contains(variable));
            allTestVariablesAdded.Should().BeTrue();

            var json = new DefaultJsonTemplateGenerator(_sp.GetService<ILogger<SettingsTemplateConverter>>(), configuration, new TemplateFilters()).GenerateJsonSettingsTemplate();
            var actualJson = JToken.Parse(json);

            actualJson.Should().BeEmpty();
        }

        [Fact]
        public void Settings_template_ioc_registration_should_register_IJsonSettingsTemplateGenerator()
        {
            var generator = _sp.GetService<IJsonSettingsTemplateGenerator>();
            generator.Should().NotBeNull();
        }

        [Fact]
        public void Settings_template_has_variables_from_json_config()
        {
            Environment.SetEnvironmentVariable("SettingsUrl", "settings.json");

            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .TryAddConfigurationSource(out var source)
                .Build();

            var existingSection = configuration.GetSection("TestModel");
            var nonExistingSection = configuration.GetSection("TestModel1");
            var json = new DefaultJsonTemplateGenerator(_sp.GetService<ILogger<SettingsTemplateConverter>>(), configuration, new TemplateFilters()).GenerateJsonSettingsTemplate();
            var actualJson = JToken.Parse(json);
            var settingsTemplateModel = actualJson["TestModel"];
            var nonExistingSettingsTemplateModel = actualJson["TestModel1"];

            source.Should().Be(ConfigurationSource.Json, "because settings.json file was added to IConfigurationRoot based on SettingsUrl env variable");
            existingSection.Exists().Should().BeTrue("because IConfigurationRoot should contain settings from settings.json file");
            nonExistingSection.Exists().Should().BeFalse("because settings.json file doesn't contain TestModel1");
            settingsTemplateModel.Should().NotBeNull("because SettingsTemplateGenerator was able to read settings from settings.json file");
            nonExistingSettingsTemplateModel.Should().BeNull("because settings.json file doesn't contain TestModel1");
        }

        [Fact]
        public void Settings_template_has_variables_from_json_config_provided_directly()
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .TryAddConfigurationSource("settings.json")
                .Build();

            var existingSection = configuration.GetSection("TestModel");
            var nonExistingSection = configuration.GetSection("TestModel1");
            var json = new DefaultJsonTemplateGenerator(_sp.GetService<ILogger<SettingsTemplateConverter>>(), configuration, new TemplateFilters()).GenerateJsonSettingsTemplate();
            var actualJson = JToken.Parse(json);
            var settingsTemplateModel = actualJson["TestModel"];
            var nonExistingSettingsTemplateModel = actualJson["TestModel1"];

            existingSection.Exists().Should().BeTrue("because IConfigurationRoot should contain settings from settings.json file");
            nonExistingSection.Exists().Should().BeFalse("because settings.json file doesn't contain TestModel1");
            settingsTemplateModel.Should().NotBeNull("because SettingsTemplateGenerator was able to read settings from settings.json file");
            nonExistingSettingsTemplateModel.Should().BeNull("because settings.json file doesn't contain TestModel1");
        }

        [Fact]
        public void Settings_template_has_variables_from_http_config()
        {
            const string path = "/config";
            Environment.SetEnvironmentVariable("SettingsUrl", $"{_server.Url}{path}");

            var testModel = new Fixture().Create<TestConfig>();

            _server.Given(Request.Create().WithPath(path).UsingGet())
                .RespondWith(Response.Create().WithBodyAsJson(testModel));

            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .TryAddConfigurationSource(out var source)
                .Build();

            var existingSection = configuration.GetSection("TestModel");
            var nonExistingSection = configuration.GetSection("TestModel1");
            var json = new DefaultJsonTemplateGenerator(_sp.GetService<ILogger<SettingsTemplateConverter>>(), configuration, new TemplateFilters()).GenerateJsonSettingsTemplate();
            var actualJson = JToken.Parse(json);
            var settingsTemplateModel = actualJson["TestModel"];
            var nonExistingSettingsTemplateModel = actualJson["TestModel1"];

            source.Should().Be(ConfigurationSource.Http, "because HttpConfigurationSource was used based on SettingsUrl env variable");
            existingSection.Exists().Should().BeTrue("because IConfigurationRoot should contain settings from /config endpoint");
            nonExistingSection.Exists().Should().BeFalse("because /config endpoint doesn't contain TestModel1");
            settingsTemplateModel.Should().NotBeNull("because SettingsTemplateGenerator was able to read settings from /config endpoint");
            nonExistingSettingsTemplateModel.Should().BeNull("because /config endpoint doesn't contain TestModel1");
        }

        [Fact]
        public void TryAddConfigurationSource_throws_exception_when_SettingsUrl_is_empty()
        {
            Environment.SetEnvironmentVariable("SettingsUrl", " ");

            var configuration = () => new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .TryAddConfigurationSource()
                .Build();

            configuration.Should().Throw<ArgumentException>().WithMessage(
                "configurationUrlOrPath is not specified and environment variable 'SettingsUrl' is not defined",
                "because SettingsUrl env variable was empty (or whitespace only)");
        }

        [Fact]
        public void TryAddConfigurationSource_throws_exception_when_SettingsUrl_is_not_specified_and_not_provided_directly()
        {
            var configuration = () => new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .TryAddConfigurationSource()
                .Build();

            configuration.Should().Throw<ArgumentException>().WithMessage(
                "configurationUrlOrPath is not specified and environment variable 'SettingsUrl' is not defined",
                "because SettingsUrl env variable was not provided and url was not specified directly in TryAddConfigurationSource");
        }
    }
}