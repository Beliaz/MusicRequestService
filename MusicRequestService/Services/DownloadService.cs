using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MusicRequestService.Services
{
    public class DownloadResult : EventArgs
    {
        public string Guid { get; set; }

        public string Path { get; set; }

        public Uri CallbackUri { get; set; }
    }

    public class DownloadService
    {
        private readonly ILogger _logger;
        private static readonly string ScriptPath = Path.Combine(".", "Download-MP3.ps1");
        private const string ScriptExecutor = "pwsh";
        private const string Extension = ".mp3";

        public DownloadService(ILogger<DownloadService> logger)
        {
            _logger = logger;
        }

        public event EventHandler<DownloadResult> DownloadCompleted;
        public event EventHandler<DownloadResult> DownloadFailed;

        public Task DownloadAsync(Uri url, Uri callbackUri)
        {
            var guid = Guid.NewGuid().ToString();
            var tmpPath = Path.Combine(Path.GetTempPath(), guid);

            return Task.Run(() =>
            {
                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();

                outputBuilder.Append("standard output:\n\n");
                errorBuilder.Append("error output:\n\n");

                void DownloadProcessFailed()
                {
                    _logger.LogError($"Downloading failed\n{outputBuilder}\n\n{errorBuilder}");
                    DownloadFailed?.Invoke(this, new DownloadResult());
                }

                var downloadProcess = new Process
                {
                    StartInfo = new ProcessStartInfo(ScriptExecutor, $"{ScriptPath} {url} {tmpPath}")
                    {
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false
                    }
                };

                downloadProcess.ErrorDataReceived += (sender, args) => errorBuilder.AppendLine(args.Data);
                downloadProcess.OutputDataReceived += (sender, args) => outputBuilder.AppendLine(args.Data);
                
                try
                {
                    downloadProcess.Start();
                }
                catch (Exception e)
                {
                    _logger.LogError($"Error while starting download process\n{e}");
                    DownloadFailed?.Invoke(this, new DownloadResult());
                    return;
                }
                
                downloadProcess.BeginOutputReadLine();
                downloadProcess.BeginErrorReadLine();
                downloadProcess.WaitForExit();

                if (downloadProcess.ExitCode != 0 || 
                    !Directory.Exists(tmpPath))
                {
                    DownloadProcessFailed();
                    return;
                }

                var files = Directory.EnumerateFiles(tmpPath)?.ToList();

                if (files is null || !files.Any(f => f.EndsWith(Extension)))
                {
                    DownloadProcessFailed();
                    return;
                }

                var file = files.First(f => f.EndsWith(Extension));
                
                _logger.LogInformation($"Downloading {Path.GetFileName(file)} from {url} succeeded");

                DownloadCompleted?.Invoke(this, new DownloadResult
                {
                    Guid = guid,
                    Path = file,
                    CallbackUri = callbackUri
                });
            });
        }
    }
}
