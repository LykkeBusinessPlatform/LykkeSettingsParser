using System;
using System.IO;
using System.Linq;
using AutoFixture.Xunit2;
using FluentAssertions;
using Lykke.SettingsReader.ConfigurationProvider;
using Lykke.SettingsReader.Test.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Lykke.SettingsReader.Test
{
    public class HttpConfigurationProviderTests
    {
        private readonly WireMockServer _server;
        private  IConfigurationRoot _configuration;
        private string _serverUrl;

        public HttpConfigurationProviderTests()
        {
            _server = WireMockServer.Start();
        }

        [Theory]
        [AutoData]
        public void Configuration_should_be_taken_from_http_endpoint_and_bind_correctly(ConfigurationModel testModel)
        {
            var path  = "/config";
            _server.Given(Request.Create().WithPath(path).UsingGet()).RespondWith(Response.Create().WithBodyAsJson(testModel));
            
            _configuration = new ConfigurationBuilder()
                .AddHttpConfiguration(new Uri($"{_server.Url}{path}"))
                .Build();
            
            var bindedModel = new ConfigurationModel();
            _configuration.Bind(bindedModel);

            bindedModel.Should().BeEquivalentTo(testModel);
        }
    }
}