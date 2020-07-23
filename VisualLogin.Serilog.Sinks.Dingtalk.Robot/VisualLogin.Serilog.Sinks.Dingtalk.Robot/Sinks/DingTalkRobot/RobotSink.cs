using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.DingTalkRobot
{
    public class RobotSink : IBatchedLogEventSink, IDisposable
    {
        readonly string BaseUrl = "https://oapi.dingtalk.com/robot/send?access_token=";
        readonly RobotConnectionInfo _connectionInfo;
        readonly ITextFormatter _textFormatter;
        private readonly HttpClient _client;
        public RobotSink(RobotConnectionInfo connectionInfo, ITextFormatter textFormatter)
        {
            _connectionInfo = connectionInfo ?? throw new ArgumentNullException(nameof(connectionInfo));
            _textFormatter = textFormatter;
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            _client = new HttpClient(httpClientHandler);
        }

        public async Task EmitBatchAsync(IEnumerable<LogEvent> events)
        {
            var timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var secretEnc = Encoding.UTF8.GetBytes(_connectionInfo.Secret);
            var stringToSign = $"{timestamp}\n{_connectionInfo.Secret}";
            var stringToSignEnc = Encoding.UTF8.GetBytes(stringToSign);
            var hmac256 = new HMACSHA256(secretEnc);
            var hashMessage = hmac256.ComputeHash(stringToSignEnc);
            var sign = Convert.ToBase64String(hashMessage);
            var payload = new StringWriter();
            foreach (var logEvent in events)
            {
                _textFormatter.Format(logEvent, payload);
            }
            RobotMessageText robotMessage=new RobotMessageText();
            robotMessage.text.content = payload.ToString();
            var str = new StringContent(JsonConvert.SerializeObject(robotMessage), Encoding.UTF8,
                "application/json");
            await _client.PostAsync($"{BaseUrl}{_connectionInfo.Token}&timestamp={timestamp}&sign={sign}", str);
        }

        public Task OnEmptyBatchAsync()
        {
            return Task.FromResult(false);
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}