using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;

namespace RemoteFunctionCall.Test
{
    public class UnitTest1
    {
        private RfcServer _server = null!;
        private RfcClient _client = null!;

        [SetUp]
        public void Setup()
        {
            RfcConfiguration configuration = new RfcConfiguration
            {
                Timeout = 5000
            };
            _server = new RfcServer(configuration);
            _client = new RfcClient(configuration);
        }

        [Test]
        public void Test1_NormalCall()
        {
            _server.Start(dto =>
            {
                if (dto.FunctionName == "Sum")
                {
                    int a1, a2;
                    if (dto.Parameters.ContainsKey("A1"))
                    {
                        a1 = int.Parse(dto.Parameters["A1"]);
                        if (dto.Parameters.ContainsKey("A2"))
                        {
                            a2 = int.Parse(dto.Parameters["A2"]);
                            return new RfcResultDto(true, false, "",
                                new KeyValuePair<string, string>("Sum", (a1 + a2).ToString()));
                        }
                    }
                }

                return new RfcResultDto(false, false, "Error");
            });
            _client.Start();

            var response = _client.Call(new RfcFunctionDto("Sum", new KeyValuePair<string, string>("A1", "10"),
                new KeyValuePair<string, string>("A2", "20")));

            Assert.AreEqual(true, response.IsSuccess);
            Assert.AreEqual(false, response.IsTimeout);
            Assert.AreEqual("", response.Message);
            Assert.AreEqual(1, response.Parameters.Count);
            Assert.AreEqual(true, response.Parameters.ContainsKey("Sum"));
            Assert.AreEqual("30", response.Parameters["Sum"]);
        }

        [Test]
        public void Test2_Timeout()
        {
            _server.Start(dto =>
            {
                if (dto.FunctionName == "Sum")
                {
                    int a1, a2;
                    if (dto.Parameters.ContainsKey("A1"))
                    {
                        a1 = int.Parse(dto.Parameters["A1"]);
                        if (dto.Parameters.ContainsKey("A2"))
                        {
                            a2 = int.Parse(dto.Parameters["A2"]);
                            Thread.Sleep(10000);
                            return new RfcResultDto(true, false, "",
                                new KeyValuePair<string, string>("Sum", (a1 + a2).ToString()));
                        }
                    }
                }

                return new RfcResultDto(false, false, "Error");
            });
            _client.Start();

            var response = _client.Call(new RfcFunctionDto("Sum", new KeyValuePair<string, string>("A1", "10"),
                new KeyValuePair<string, string>("A2", "20")));

            Assert.AreEqual(false, response.IsSuccess);
            Assert.AreEqual(true, response.IsTimeout);
            Assert.AreEqual("Timeout", response.Message);
        }
    }
}