using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MusicRequestService.Services
{
    public class PublishingService
    {
        private readonly ILogger<PublishingService> _logger;
        private readonly HttpClient _client = new HttpClient();
        private readonly Uri _hostAddress;
        private readonly Dictionary<string, string> _fileRegistry = new Dictionary<string, string>();

        public PublishingService(Uri hostAddress, ILogger<PublishingService> logger)
        {
            _logger = logger;
            _hostAddress = hostAddress;

            //specify to use TLS 1.2 as default connection
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        }

        public string GetFile(string guid)
        {
            string file;

            lock (_fileRegistry)
            {
                if (!_fileRegistry.TryGetValue(guid, out file))
                    return null;
            }

            if (File.Exists(file)) return file;

            lock (_fileRegistry)
            {
                _fileRegistry.Remove(file);
            }

            return file;
        }

        public void Publish(Uri callbackUri, string guid, string file)
        {
            var requestBody = new CallbackRequest
            {
                Uri = $"{_hostAddress}/Request/File/{guid}",
                FileName = Path.GetFileName(file)
            };

            var jsonSerializer = new JsonSerializer();
            var builder = new StringBuilder();
            var textWriter = new StringWriter(builder);
            var writer = new JsonTextWriter(textWriter);

            jsonSerializer.Serialize(writer, requestBody);

            var requestString = builder.ToString();

            try
            {
                _client.PostAsync(callbackUri, new StringContent(requestString, Encoding.UTF8, "application/json"))
                    .Result
                    .EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return;
            }

            if (callbackUri is null) return;

            lock (_fileRegistry)
            {
                _fileRegistry[guid] = file;
            }
        }
    }
}
