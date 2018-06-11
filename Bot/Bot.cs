using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bot.Dialogs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Bot
{
    public static class Bot
    {
        private static readonly BotFrameworkAdapter BotAdapter;
        private static readonly Nyx Nyx = new Nyx();

        static Bot()
        {
            var appId = Environment.GetEnvironmentVariable(@"MS_APP_ID");
            var pwd = Environment.GetEnvironmentVariable(@"MS_APP_PASSWORD");

            BotAdapter = new BotFrameworkAdapter(appId, pwd)
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));
        }

        [FunctionName("Nyx")]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequest req, TraceWriter log)
        {
            var requestBody = new StreamReader(req.Body).ReadToEnd();

            log.Verbose($@"Bot got: {requestBody}");

            var activity = JsonConvert.DeserializeObject<Activity>(requestBody);

            try
            {
                await BotAdapter.ProcessActivity(req.Headers[@"Authorization"].FirstOrDefault(), activity, Nyx.OnTurn);

                return new OkResult();
            }
            catch (Exception ex)
            {
                return new ObjectResult(ex) {StatusCode = StatusCodes.Status500InternalServerError};
            }
        }
    }
}