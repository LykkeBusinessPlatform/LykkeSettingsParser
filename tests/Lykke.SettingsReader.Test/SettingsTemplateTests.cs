using System;
using AutoFixture.Xunit2;
using FluentAssertions;
using Lykke.SettingsReader.ConfigurationProvider;
using Lykke.SettingsReader.SettingsTemplate;
using Lykke.SettingsReader.Test.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace Lykke.SettingsReader.Test
{
    public class SettingsTemplateTests
    {
        private readonly WireMockServer _server;
        private string _serverUrl;
        private string _expectedTemplate = @"{""ArrayProperty"": [
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

        public SettingsTemplateTests()
        {
            _server = WireMockServer.Start();
        }

        [Theory]
        [AutoData]
        public void Settings_template_in_json_has_to_be_created_from_IConfiguration(ConfigurationModel testModel)
        {
            var path = "/config";
            _server.Given(Request.Create().WithPath(path).UsingGet())
                .RespondWith(Response.Create().WithBodyAsJson(testModel));

            IConfiguration configuration = new ConfigurationBuilder()
                .AddHttpSourceConfiguration(new Uri($"{_server.Url}{path}"))
                .Build();

            var json = new JsonSettingsTemplateGenerator(configuration).GenerateJsonSettingsTemplate();

            var expectedJson = JToken.Parse(_expectedTemplate);
            var actualJson = JToken.Parse(json);
            actualJson.Should().BeEquivalentTo(expectedJson);
        }
    }
}