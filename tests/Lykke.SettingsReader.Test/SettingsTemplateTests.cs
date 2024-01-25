using System;
using System.Linq;
using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using Lykke.SettingsReader.ConfigurationProvider;
using Lykke.SettingsReader.SettingsTemplate;
using Lykke.SettingsReader.Test.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        public SettingsTemplateTests()
        {
            var server = WireMockServer.Start();
            
            const string path = "/config";
            var testModel = new Fixture().Create<ConfigurationModel>();
            server.Given(Request.Create().WithPath(path).UsingGet())
                .RespondWith(Response.Create().WithBodyAsJson(testModel));

            _configuration = new ConfigurationBuilder()
                .AddHttpSourceConfiguration(new Uri($"{server.Url}{path}"))
                .Build();
        }

       [Fact]
        public void Settings_template_in_json_has_to_be_created_from_IConfiguration()
        {
            var json = new DefaultJsonTemplateGenerator(_configuration).GenerateJsonSettingsTemplate();
            var expectedJson = JToken.Parse(_expectedTemplate);
            var actualJson = JToken.Parse(json);
            actualJson.Should().BeEquivalentTo(expectedJson);
        }
        
        [Fact]
        public void Settings_template_ioc_registration_should_register_IJsonSettingsTemplateGenerator()
        {
            var sp = new ServiceCollection()
                .AddSingleton(_configuration)
                .AddSettingsTemplateGenerator()
                .BuildServiceProvider();

            var generator = sp.GetService<IJsonSettingsTemplateGenerator>();
            generator.Should().NotBeNull();
        }
    }
}