using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.SettingsReader.Exceptions;
using Lykke.SettingsReader.Test.Models;
using Lykke.SettingsReader.Test.Models.CheckAttributes;
using Xunit;

namespace Lykke.SettingsReader.Test
{
    public class SettingsProcessorTest
    {
        private readonly string _jsonTest = @"{""test1"":""testString"",""test2"":2,""test3"":""2017-05-01 22:13:45"", ""subObject"":{""test1"":""testString1"",""test2"":21,""test3"":""2017-05-02 22:13:45""},
""subArray"":[{""test1"":""testString2"",""test2"":22,""test3"":""2017-05-03 22:13:45""},
{""test1"":""testString3"",""test2"":23,""test3"":""2017-05-04 22:13:45""},
{""test1"":""testString4"",""test2"":24,""test3"":""2017-05-05 22:13:45""},
{""test1"":""testString5"",""test2"":25,""test3"":""2017-05-06 22:13:45""}],
  ""subArrayGen"":[{""test1"":""testString6"",""test2"":26,""test3"":""2017-05-07 22:13:45""},
{""test1"":""testString7"",""test2"":27,""test3"":""2017-05-08 22:13:45""},
{""test1"":""testString8"",""test2"":28,""test3"":""2017-05-09 22:13:45""},
{""test1"":""testString9"",""test2"":29,""test3"":""2017-05-10 22:13:45""}],
""TestDouble"" : 0.2, ""SetOnlyProperty"" : 0.3
    }";

        private const string ServiceUrl = "https://api-dev.lykkex.net";


        [Fact]
        public async Task EmptyJson()
        {
            await Assert.ThrowsAsync<JsonStringEmptyException>(async () => await SettingsProcessor.ProcessAsync<TestModel>(string.Empty));
        }

        [Fact]
        public async Task IncorrectJson()
        {
            await Assert.ThrowsAsync<IncorrectJsonFormatException>(async () => await SettingsProcessor.ProcessAsync<TestModel>(_jsonTest.Substring(10)));
        }

        [Fact]
        public async Task FieldMissJson()
        {
            var ex = await Assert.ThrowsAsync<RequiredFieldEmptyException>(async () => await  SettingsProcessor.ProcessAsync<TestModel>(_jsonTest.Replace(@"""test2"":2,", String.Empty)));
            Assert.Equal("Test2", ex.FieldName);
        }

        [Fact]
        public async Task SubFieldMissJson()
        {
            var ex = await Assert.ThrowsAsync<RequiredFieldEmptyException>(async () => await  SettingsProcessor.ProcessAsync<TestModel>(_jsonTest.Replace(@"""test2"":21,", String.Empty)));
            Assert.Equal("SubObject.Test2", ex.FieldName);
        }

        [Fact]
        public async Task SubFieldArrayMissJson()
        {
            var ex = await Assert.ThrowsAsync<RequiredFieldEmptyException>(async () => await  SettingsProcessor.ProcessAsync<TestModel>(_jsonTest.Replace(@"""test2"":24,", String.Empty)));
            Assert.Equal("SubArray.2.Test2", ex.FieldName);
        }

        [Fact]
        public async Task OkJson()
        {
            var model = await SettingsProcessor.ProcessAsync<TestModel>(_jsonTest);
            CheckModel(model);
        }

        [Fact]
        public async Task OkWithOptionalJson()
        {
            var model = await SettingsProcessor.ProcessAsync<TestOptionAttrModel>(_jsonTest);
            CheckModel(model);
            Assert.Null(model.Test4);
            Assert.Null(model.SubObjectOptional);
        }

        private void CheckModel(TestModel model)
        {
            Assert.Equal("testString", model.Test1);
            Assert.Equal(2, model.Test2);
            Assert.Equal(model.Test3, new DateTime(2017, 05, 01, 22, 13, 45));
            Assert.Equal("testString1", model.SubObject.Test1);
            Assert.Equal(21, model.SubObject.Test2);
            Assert.Equal(model.SubObject.Test3, new DateTime(2017, 05, 02, 22, 13, 45));
            Assert.Equal(0.2, model.TestDouble);
            var lst = model.SubArray.ToList();

            Assert.Equal("testString2", lst[0].Test1);
            Assert.Equal(22, lst[0].Test2);
            Assert.Equal(lst[0].Test3, new DateTime(2017, 05, 03, 22, 13, 45));

            Assert.Equal("testString3", lst[1].Test1);
            Assert.Equal(23, lst[1].Test2);
            Assert.Equal(lst[1].Test3, new DateTime(2017, 05, 04, 22, 13, 45));

            Assert.Equal("testString4", lst[2].Test1);
            Assert.Equal(24, lst[2].Test2);
            Assert.Equal(lst[2].Test3, new DateTime(2017, 05, 05, 22, 13, 45));

            Assert.Equal("testString5", lst[3].Test1);
            Assert.Equal(25, lst[3].Test2);
            Assert.Equal(lst[3].Test3, new DateTime(2017, 05, 06, 22, 13, 45));

            lst = model.SubArrayGen.ToList();

            Assert.Equal("testString6", lst[0].Test1);
            Assert.Equal(26, lst[0].Test2);
            Assert.Equal(lst[0].Test3, new DateTime(2017, 05, 07, 22, 13, 45));

            Assert.Equal("testString7", lst[1].Test1);
            Assert.Equal(27, lst[1].Test2);
            Assert.Equal(lst[1].Test3, new DateTime(2017, 05, 08, 22, 13, 45));

            Assert.Equal("testString8", lst[2].Test1);
            Assert.Equal(28, lst[2].Test2);
            Assert.Equal(lst[2].Test3, new DateTime(2017, 05, 09, 22, 13, 45));

            Assert.Equal("testString9", lst[3].Test1);
            Assert.Equal(29, lst[3].Test2);
            Assert.Equal(lst[3].Test3, new DateTime(2017, 05, 10, 22, 13, 45));
        }

        [Fact]
        public async Task Test_NegativeNumbersAsStrings_IsOk()
        {
            string json = "{'Int': '-1234', " +
                          "'Double': '-10.4', " +
                          "'Float': '-10.4', " +
                          "'Decimal': '-10.2'}";

            var model = await SettingsProcessor.ProcessAsync<TestConvert>(json);
            Assert.Equal(-1234, model.Int);
            Assert.Equal(-10.4, Math.Round(model.Double, 2));
            Assert.Equal(-10.4, Math.Round(model.Float, 2));
            Assert.Equal((decimal)-10.2, model.Decimal);
        }

        [Fact]
        public async Task Test_NegativeNumbers_IsOk()
        {
            string json = "{'Int': -1234, " +
                          "'Double': -10.4, " +
                          "'Float': -10.4, " +
                          "'Decimal': -10.2}";

            var model = await SettingsProcessor.ProcessAsync<TestConvert>(json);
            Assert.Equal(-1234, model.Int);
            Assert.Equal(-10.4, Math.Round(model.Double, 2));
            Assert.Equal(-10.4, Math.Round(model.Float, 2));
            Assert.Equal((decimal)-10.2, model.Decimal);
        }

        [Fact(Skip = "Depends on external service")]
        public async Task HttpCheckAttribute_IsOk()
        {
            var settings = await SettingsProcessor.ProcessAsync<TestHttpCheckModel>(
                    $"{{'Service': {{'ServiceUrl': '{ServiceUrl}'}}, 'Url': '{ServiceUrl}', 'Port':5672, 'Num': 1234}}");

            string message = await SettingsProcessor.CheckDependenciesAsync(settings);
            
            Assert.Null(message);
        }

        [Fact(Skip = "Depends on external service")]
        public async Task HttpCheckAttribute_IsArrayOk()
        {
            var settings = await SettingsProcessor.ProcessAsync<TestHttpCheckArrayModel>(
                    $"{{'Services': [{{'ServiceUrl': '{ServiceUrl}'}}], 'List': [{{'ServiceUrl': '{ServiceUrl}'}}], " +
                    $"'IList': [{{'ServiceUrl': '{ServiceUrl}'}}], 'RoList': [{{'ServiceUrl': '{ServiceUrl}'}}], " +
                    $"'RoCollection': [{{'ServiceUrl': '{ServiceUrl}'}}], 'Enumerable': [{{'ServiceUrl': '{ServiceUrl}'}}]}}");

            string message = await SettingsProcessor.CheckDependenciesAsync(settings);
            
            Assert.Null(message);
        }

        [Fact(Skip = "Depends on external service")]
        public async Task HttpCheckAttribute_IsListOk()
        {
            var settings = await SettingsProcessor.ProcessAsync<TestHttpCheckListModel>(
                    $"{{'Services': ['{ServiceUrl}'],'List': ['{ServiceUrl}'],'IList': ['{ServiceUrl}']," +
                    $"'RoList': ['{ServiceUrl}'],'RoCollection': ['{ServiceUrl}'],'Enumerable': ['{ServiceUrl}']}}");

            string message = await SettingsProcessor.CheckDependenciesAsync(settings);
            
            Assert.Null(message);
        }

        [Fact]
        public async Task HttpCheckAttribute_IsDictionaryOk()
        {
            var settings = await SettingsProcessor.ProcessAsync<TestHttpCheckDictioinaryModel>(
                    $"{{'Services': {{'first': {{'ServiceUrl': '{ServiceUrl}'}} }}," +
                    $"'IDict': {{'first': {{'ServiceUrl': '{ServiceUrl}'}} }}," +
                    $"'RoDict': {{'first': {{'ServiceUrl': '{ServiceUrl}'}} }} }}");
            
            string message = await SettingsProcessor.CheckDependenciesAsync(settings);
            
            Assert.Null(message);
        }

        [Fact]
        public async Task HttpCheckAttribute_IsInvalidUrl()
        {
            var settings = await SettingsProcessor.ProcessAsync<TestHttpCheckModel>(
                    $"{{'Service': {{'ServiceUrl': 'not_url_at_all'}}, 'Url': '{ServiceUrl}/', 'Port':5672, 'Num': 1234}}");

            string message = await SettingsProcessor.CheckDependenciesAsync(settings);
            
            Assert.Contains("Failed", message);
        }
        
        [Fact]
        public async Task HttpCheckAttribute_IsEmptyField()
        {
            var settings = await SettingsProcessor.ProcessAsync<TestHttpCheckModel>(
                $"{{'Service': {{'ServiceUrl': '{ServiceUrl}'}}, 'Url': '', 'Port':5672, 'Num': 1234}}");

            string message = await SettingsProcessor.CheckDependenciesAsync(settings);
            
            Assert.Contains("Empty setting value", message);
        }

        [Fact]
        public async Task TcpCheckAttribute_IsArrayOk()
        {
            var checkList = new List<(string, string)>
            {
                ("Endpoints", "127.0.0.1:5672"),
                ("List", "127.0.0.1:5672"),
                ("IList", "127.0.0.1:5672"),
                ("RoList", "127.0.0.1:5672"),
                ("RoCollection", "127.0.0.1:5672"),
                ("Enumerable", "127.0.0.1:5672")
            };
            
            foreach (var pair in checkList)
            {
                var settings = await SettingsProcessor.ProcessAsync<TestTcpCheckArrayModel>($"{{'{pair.Item1}': [{{'HostPort': '{pair.Item2}'}}] }}");
                
                string message = await SettingsProcessor.CheckDependenciesAsync(settings);
                
                Assert.Contains("Failed", message);
            }
        }

        [Fact]
        public async Task TcpCheckAttribute_IsListOk()
        {
            var checkList = new List<(string, string)>
            {
                ("Hosts", "127.0.0.1:5672"),
                ("List", "127.0.0.1:5672"),
                ("IList", "127.0.0.1:5672"),
                ("RoList", "127.0.0.1:5672"),
                ("RoCollection", "127.0.0.1:5672"),
                ("Enumerable", "127.0.0.1:5672")
            };
            foreach (var pair in checkList)
            {
                var settings = await SettingsProcessor.ProcessAsync<TestTcpCheckListModel>($"{{'{pair.Item1}': ['{pair.Item2}'] }}");
                
                string message = await SettingsProcessor.CheckDependenciesAsync(settings);
                
                Assert.Contains("Failed", message);
            }
        }

        [Fact]
        public async Task TcpCheckAttribute_IsDictionaryOk()
        {
            var settings = await SettingsProcessor.ProcessAsync<TestTcpCheckDictionaryModel>("{'Endpoints': {'first': {'HostPort': '127.0.0.1:5672'}}," +
                                                                       "'IDict': {'first': {'HostPort': '127.0.0.1:5672'}}," +
                                                                                             
                                                                       "'RoDict': {'first': {'HostPort': '127.0.0.1:5672'}}}");
            
            string message = await SettingsProcessor.CheckDependenciesAsync(settings);
            
            Assert.Null(message);
        }

        [Fact]
        public async Task TcpCheckAttribute_IsInvalidPort()
        {
            var settings = await SettingsProcessor.ProcessAsync<TestTcpCheckModel>(
                "{'HostInfo': {'HostPort': '127.0.0.1:zzz'}, 'Host': '127.0.0.1', 'Port': 5672, 'Server': '127.0.0.1'}");

            string message = await SettingsProcessor.CheckDependenciesAsync(settings);
            
            Assert.NotNull(message);
            Assert.Contains("Invalid port", message);
        }

        [Fact]
        public async Task TcpCheckAttribute_IsInvalidPortValue()
        {
            const string host = "127.0.0.1";
            const int port = 5672;

            var settings = await SettingsProcessor.ProcessAsync<TestTcpCheckModel>($"{{'HostInfo': {{'HostPort': '{host}:{port}'}} }}");

            string message = await SettingsProcessor.CheckDependenciesAsync(settings);
            
            Assert.Contains("Failed", message);
            
            settings = await SettingsProcessor.ProcessAsync<TestTcpCheckModel>($"{{'Host': '{host}', 'Port': 'not a port'}}");
            
            message = await SettingsProcessor.CheckDependenciesAsync(settings);

            Assert.NotNull(message);
            Assert.Equal($"Check of the 'Host' field value [{host}] is failed: Invalid port value in property 'Port'", message);

            settings = await SettingsProcessor.ProcessAsync<TestTcpCheckModel>($"{{'Host': '{host}', 'Port': '{port}'}}");

            message = await SettingsProcessor.CheckDependenciesAsync(settings);
            
            Assert.Contains("Failed", message);

            settings = await SettingsProcessor.ProcessAsync<TestTcpCheckModel>($"{{'Server': '{host}'}}");

            message = await SettingsProcessor.CheckDependenciesAsync(settings);
            
            Assert.Contains("Failed", message);
        }

        [Fact]
        public async Task TcpCheckAttribute_WrongPortProperty()
        {
            var settings = await SettingsProcessor.ProcessAsync<WrongTestTcpCheckModel>("{'Host': '127.0.0.1', 'Port': '5672'}");
            
            string message = await SettingsProcessor.CheckDependenciesAsync(settings);

            Assert.NotNull(message);
            Assert.Equal("Check of the 'Host' field value [127.0.0.1] is failed: Property 'ServicePort' not found", message);
        }

        /*
        // Works only under VPN
        [Fact]
        public void AmqpCheckAttribute_IsOk()
        {
            string json = "{'ConnStr': 'amqp://guest:guest@localhost:5672', 'Rabbit': {'ConnString': 'amqp://lykke.user:123qwe123qwe123@rabbit-registration.lykke-service.svc.cluster.local:5672'}}";
            var exception = Record.Exception(() =>
            {
                SettingsProcessor.ProcessAsync<TestAmqpCheckModel>(json);
            });

            Assert.Null(exception);
        }
        */

        [Fact]
        public async Task AmqpCheckAttribute_IsArrayOk()
        {
            var checkList = new List<(string, string)>
            {
                ("Rabbits", "amqp://guest:guest@localhost:5672"),
                ("List", "amqp://guest:guest@localhost:5672"),
                ("IList", "amqp://guest:guest@localhost:5672"),
                ("RoList", "amqp://guest:guest@localhost:5672"),
                ("RoCollection", "amqp://guest:guest@localhost:5672"),
                ("Enumerable", "amqp://guest:guest@localhost:5672")
            };
            
            foreach (var pair in checkList)
            {
                var settings = await SettingsProcessor.ProcessAsync<TestAmqpCheckArrayModel>($"{{'{pair.Item1}': [{{'ConnString': '{pair.Item2}'}}] }}");
                string message = await SettingsProcessor.CheckDependenciesAsync(settings);
                Assert.Contains("Failed", message);
            }
        }

        [Fact]
        public async Task AmqpCheckAttribute_IsListOk()
        {
            var checkList = new List<(string, string)>
            {
                ("Rabbits", "amqp://guest:guest@localhost:5672"),
                ("List", "amqp://guest:guest@localhost:5672"),
                ("IList", "amqp://guest:guest@localhost:5672"),
                ("RoList", "amqp://guest:guest@localhost:5672"),
                ("RoCollection", "amqp://guest:guest@localhost:5672"),
                ("Enumerable", "amqp://guest:guest@localhost:5672")
            };
            foreach (var pair in checkList)
            {
                var settings = await SettingsProcessor.ProcessAsync<TestAmqpCheckListModel>($"{{'{pair.Item1}': ['{pair.Item2}'] }}");
                string message = await SettingsProcessor.CheckDependenciesAsync(settings);
                Assert.Contains("Failed", message);
            }
        }

        [Fact]
        public async Task AmqpCheckAttribute_IsDictionaryOk()
        {
            var settings = await SettingsProcessor.ProcessAsync<TestAmqpCheckDictionaryModel>("{'Rabbits': {'first': {'ConnString': 'amqp://guest:guest@localhost:5672'}}," +
                                                                        "'IDict': {'first': {'ConnString': 'amqp://guest:guest@localhost:5672'}}," +
            
                                                                                              "'RoDict': {'first': {'ConnString': 'amqp://guest:guest@localhost:5672'}}}");
            
            string message = await SettingsProcessor.CheckDependenciesAsync(settings);
            
            Assert.Null(message);
        }

        [Fact]
        public async Task AmqpCheckAttribute_IsInvalidPort()
        {
            var settings = await SettingsProcessor.ProcessAsync<TestAmqpCheckModel>(
                "{'ConnStr': 'amqp://guest:guest@localhost:5672', 'Rabbit': {'ConnString': 'amqp://lykke.user:123qwe123qwe123@rabbit-registration.lykke-service.svc.cluster.local:zzz'}}");
            
            string message = await SettingsProcessor.CheckDependenciesAsync(settings);
            
            Assert.Contains("Failed", message);
        }

        [Fact]
        public async Task AmqpCheckAttribute_IsInvalidConnectionString()
        {
            var settings = await SettingsProcessor.ProcessAsync<TestAmqpCheckModel>(
                "{'ConnStr': 'amqp://guest:guest@localhost:5672', 'Rabbit': {'ConnString': 'rabbit-registration.lykke-service.svc.cluster.local:5672'}}");

            string message = await SettingsProcessor.CheckDependenciesAsync(settings);
            
            Assert.Contains("Failed", message);
        }
    }
}
