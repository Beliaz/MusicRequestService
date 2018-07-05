using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MusicRequestService.Services;

namespace MusicRequestService.Controllers
{
    [Route("/Request")]
    public class RequestController : Controller
    {
        private static readonly Regex UrlRegex =
            new Regex(
                "https?:\\/\\/(www\\.)?[-a-zA-Z0-9@:%._\\+~#=]{2,256}(\\.[a-z]{2,4})?\\b([-a-zA-Z0-9@:%_\\+.~#?&/=]*)");

        private readonly ILogger<RequestController> _logger;
        private readonly HttpClient _client = new HttpClient();
        private readonly DownloadService _downloadService;
        private readonly PublishingService _publishingService;

        public RequestController(ILogger<RequestController> logger, DownloadService downloadService,
            PublishingService publishingService)
        {
            _logger = logger;
            _downloadService = downloadService;
            _publishingService = publishingService;

            _downloadService.DownloadCompleted += DownloadServiceDownloadCompleted;
            _downloadService.DownloadFailed += DownloadServiceDownloadCompleted;
        }

        private void DownloadServiceDownloadCompleted(object sender, DownloadResult e)
        {
            _publishingService.Publish(e.CallbackUri, e.Guid, e.Path);
        }

        private (Uri callbackUri, List<Uri> youtubeUrls) ParseUris(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(text));

            (Uri callbackUri, List<Uri> youtubeUrls) result;

            var url = UrlRegex.Match(text);

            if (!url.Success)
            {
                _logger.LogError($"Text {text} does not contain a callback URI");
                throw new Exception();
            }

            result.callbackUri = new Uri(url.Value);
            url = url.NextMatch();

            result.youtubeUrls = new List<Uri>();
            while (url.Success)
            {
                result.youtubeUrls.Add(new Uri(url.Value));
                url = url.NextMatch();
            }

            return result;
        }

        // POST api/values
        [HttpPost("Youtube")]
        public ActionResult Post([FromBody] string body)
        {
            try
            {
                var (callbackUri, youtubeUrls) = ParseUris(body);

                foreach (var url in youtubeUrls)
                {
                    _downloadService.DownloadAsync(url, callbackUri);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }

            return Ok();
        }

        [HttpPost("Test")]
        public void Test([FromBody] CallbackRequest request)
        {
            _logger.LogInformation(request.Uri);
        }

        [HttpGet("File/{id}")]
        public ActionResult GetFile(string id)
        {
            var filename = _publishingService.GetFile(id);

            if (filename is null || !System.IO.File.Exists(filename))
                return NotFound();


            return File(System.IO.File.OpenRead(filename), "audio/mpeg");
        }

        [HttpGet("Hello")]
        public string Hello()
        {
            return "Hello World";
        }
    }
}