using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MusicRequestService.Controllers
{
    [DataContract]
    public class CallbackRequest
    {
        [DataMember]
        public string Uri { get; set; }

        [DataMember]
        public string FileName { get; set; }
    }

    [Route("/Request")]
    public class RequestController : Controller
    {
        private readonly ILogger _logger;
        private readonly HttpClient _client = new HttpClient();

        public RequestController(ILogger<RequestController> logger)
        {
            _logger = logger;

            //specify to use TLS 1.2 as default connection
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

        }

        // POST api/values
        [HttpPost("Youtube")]
        public void Post([FromBody] string body)
        {
            if (body is null)
            {
                _logger.LogWarning("Request body is null");
                return;
            }

            var url = Regex.Match(body, "https?:\\/\\/(www\\.)?[-a-zA-Z0-9@:%._\\+~#=]{2,256}(\\.[a-z]{2,4})?\\b([-a-zA-Z0-9@:%_\\+.~#?&/=]*)");

            if (!url.Success)
            {
                _logger.LogError($"Text {body} does not contain the host address");
                return;
            }

            var hostAddress = url.Value;
            url = url.NextMatch();

            if (!url.Success)
            {
                _logger.LogError($"Text {body} does not contain a callback URI");
                return;
            }

            var callBackUri = new Uri(url.Value);
            url = url.NextMatch();

            _client.BaseAddress =
                new Uri(callBackUri.GetComponents(UriComponents.SchemeAndServer, UriFormat.SafeUnescaped));

            while (url.Success)
            {
                Process downloadProcess;

                var guid = Guid.NewGuid().ToString();
                var tmpPath = Path.Combine(Path.GetTempPath(), guid);

                try
                {
                    downloadProcess = Process.Start("pwsh", $"./Download-MP3.ps1 {url.Value} {tmpPath}");
                }
                catch (Exception e)
                {
                    _logger.LogError($"Error while starting download process\n{e}");
                    return;
                }

                if (downloadProcess is null)
                {
                    _logger.LogWarning("Process handle is null");
                    return;
                }

                downloadProcess.WaitForExit();

                url = url.NextMatch();

                if (downloadProcess.ExitCode != 0)
                {
                    _logger.LogWarning($"Download process failed with exit code {downloadProcess.ExitCode}");
                    continue;
                }

                _logger.LogInformation($"Sucessfully downloaded {url.Value}");

                if (!Directory.Exists(tmpPath))
                {
                    _logger.LogWarning("Download process failed - no file produced");
                    continue;
                }

                var files = Directory.EnumerateFiles(tmpPath)?.ToList();
                if (files is null || !files.Any())
                {
                    _logger.LogWarning("Download process failed - no file produced");
                    continue;
                }
                
                var requestBody = new CallbackRequest
                {
                    Uri = $"{hostAddress}/Request/File/{guid}",
                    FileName = Path.GetFileName(files.First())
                };

                var jsonSerializer = new JsonSerializer();
                var builder = new StringBuilder();
                var textWriter = new StringWriter(builder);
                var writer = new JsonTextWriter(textWriter);

                jsonSerializer.Serialize(writer, requestBody);

                var requestString = builder.ToString();
                
                _client.PostAsync(callBackUri.PathAndQuery.Replace("%2F", "/"),
                    new StringContent(requestString, Encoding.UTF8, "application/json"))
                    .Result
                    .EnsureSuccessStatusCode();

            }
        }

        [HttpPost("Test")]
        public void Test([FromBody] CallbackRequest request)
        {
            _logger.LogInformation(request.Uri);
        }

        [HttpGet("File/{id}")]
        public ActionResult GetFile(string id)
        {
            var path = Path.Combine(Path.GetTempPath(), id);

            if (!Directory.Exists(path) || !Directory.EnumerateFiles(path).Any())
                return NotFound();

            var filename = Directory
                .EnumerateFiles(path)
                .First(s => Path.GetExtension(s).EndsWith("mp3"));

            return File(System.IO.File.OpenRead(filename), "audio/mpeg");
        }

        [HttpGet("Hello")]
        public string Hello()
        {
            return "Hello World";
        }
    }
}
