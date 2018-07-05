using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Ai.LUIS;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts;
using Microsoft.Bot.Builder.Prompts.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using ChoicePrompt = Microsoft.Bot.Builder.Dialogs.ChoicePrompt;
using TextPrompt = Microsoft.Bot.Builder.Dialogs.TextPrompt;

namespace BotService
{
    public enum Intents
    {
        PlaySong,
        GetCurrentSong,
        None
    }

    public enum Dialogs
    {
        Introduction,
        PlayUrl,
        Query,
        AskTitleAndArtist,
        AskArtist,
        GetCurrentSong,

        SongTitlePrompt,
        SongArtistPrompt,
        ProviderPrompt
    }

    [Serializable]
    public class Nyx : IBot
    {
        public Nyx()
        {
            _dialogs.Add(Dialogs.Introduction.ToString(), new WaterfallStep[] {IntroductionDialog});

            _dialogs.Add(Dialogs.SongTitlePrompt.ToString(), new TextPrompt(TitleValidator));
            _dialogs.Add(Dialogs.SongArtistPrompt.ToString(), new TextPrompt(ArtistValidator));
            _dialogs.Add(Dialogs.ProviderPrompt.ToString(), new ChoicePrompt(Culture.English, ProviderValidator));

            _dialogs.Add(Dialogs.AskTitleAndArtist.ToString(),
                new WaterfallStep[] {AskSongTitle, AskSongArtist, QuerySoundPoolDialog, QueryProviderDialog});

            _dialogs.Add(Dialogs.AskArtist.ToString(),
                new WaterfallStep[] {AskSongArtist, QuerySoundPoolDialog, QueryProviderDialog});

            _dialogs.Add(Dialogs.Query.ToString(), new WaterfallStep[] {QuerySoundPoolDialog});
            _dialogs.Add(Dialogs.PlayUrl.ToString(), new WaterfallStep[] {PlayUrlDialog});
            _dialogs.Add(Dialogs.GetCurrentSong.ToString(), new WaterfallStep[] {GetCurrentSongDialog});
        }

        public async Task OnTurn(ITurnContext context)
        {
            try
            {
                const double luisIntentThreshold = 0.7;

                if (context.Activity.Type == ActivityTypes.Message)
                {
                    var state = context.GetConversationState<BaseState>();
                    var dialogContext = _dialogs.CreateContext(context, state);

                    if (!context.Responded)
                    {
                        await dialogContext.Continue();

                        if (!context.Responded)
                        {
                            var (intent, luisResult) = ProcessLuis(context,
                                luisIntentThreshold);

                            Dialogs dialog;
                            switch (intent)
                            {
                                case Intents.PlaySong:
                                    dialog = BeginPlaySong(luisResult);
                                    break;
                                case Intents.GetCurrentSong:
                                    dialog = BeginGetCurrentSong(luisResult);
                                    break;
                                case Intents.None:
                                    dialog = BeginNone(luisResult);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            await dialogContext.Begin(dialog.ToString());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task ProviderValidator(ITurnContext context, ChoiceResult result)
        {
            const double choiceThreshold = 0.8;
            if (result.Value.Score < choiceThreshold)
            {
                result.Status = PromptStatus.NotRecognized;
                await context.SendActivity("I'm not quite sure which choice you made");
                return;
            }

            if (result.Value.Index < 0 || result.Value.Index >= _providers.Length)
            {
                result.Status = PromptStatus.NotRecognized;
                await context.SendActivity("Your choice is invalid");
            }
        }

        private static async Task TitleValidator(ITurnContext context, TextResult result)
        {
            if (string.IsNullOrWhiteSpace(result.Value))
            {
                result.Status = PromptStatus.NotRecognized;
                await context.SendActivity("The song title should be at least 1 characters long.");
            }
        }

        private static async Task ArtistValidator(ITurnContext context, TextResult result)
        {
            if (string.IsNullOrWhiteSpace(result.Value))
            {
                result.Status = PromptStatus.NotRecognized;
                await context.SendActivity("The artist name should be at least 1 characters long.");
            }
        }

        private Task IntroductionDialog(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            return dialogContext.Context.SendActivity(
                "Hi! My name is Nyx and I'm DJ Morning's digital assistant. How can I help you?");
        }

        private async Task AskSongTitle(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            await dialogContext.Context.SendActivity("Sure!");

            var playSongState = new PlaySongState(dialogContext.ActiveDialog.State);

            if (!string.IsNullOrEmpty(playSongState.Title))
                await dialogContext.Continue();
            else
                await dialogContext.Prompt(Dialogs.SongTitlePrompt.ToString(), "What's the name of the song?");
        }

        private async Task AskSongArtist(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            var playSongState = new PlaySongState(dialogContext.ActiveDialog.State);

            if (args is TextResult textResult)
            {
                playSongState.Title = textResult.Value;
                await dialogContext.Context.SendActivity($"Got: {playSongState.Title}");
            }

            if (!string.IsNullOrEmpty(playSongState.Artist))
                await dialogContext.Continue();
            else
                await dialogContext.Prompt(Dialogs.SongArtistPrompt.ToString(), "Ok, and who is this song by?");
        }

        private async Task QuerySoundPoolDialog(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            var playSongState = new PlaySongState(dialogContext.ActiveDialog.State);

            if (args is TextResult textResult)
            {
                playSongState.Artist = textResult.Value;
                await dialogContext.Context.SendActivity($"Got: {playSongState.Artist}");
            }

            if (!string.IsNullOrEmpty(playSongState.Provider))
            {
                await dialogContext.Continue();
            }
            else
            {
                await dialogContext.Context.SendActivity(
                    "Ok great! I will look if I can find the song in my library...");

                await dialogContext.Context.SendActivity(
                    "Sorry but I do not have this song yet. Where shall I search for it?");

                var choices = ChoiceFactory.ToChoices(_providers.ToList());

                await dialogContext.Prompt(Dialogs.ProviderPrompt.ToString(),
                    "Which music service shall I use?", new ChoicePromptOptions {Choices = choices});
            }
        }

        private async Task QueryProviderDialog(DialogContext dialogContext, IDictionary<string, object> args,
            SkipStepFunction next)
        {
            var playSongState = new PlaySongState(dialogContext.ActiveDialog.State);

            if (args.First().Value is FoundChoice choice)
            {
                playSongState.Provider = choice.Value;
                await dialogContext.Context.SendActivity($"Got: {playSongState.Provider}");
            }

            await dialogContext.Context.SendActivity("Just a sec...");

            await dialogContext.Context.SendActivity("Alright, I found the following:");

            await dialogContext.Context.SendActivity(
                $"[{playSongState.Provider}]: {playSongState.Artist} - {playSongState.Title}");

            await dialogContext.End();
        }

        private Task PlayUrlDialog(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            return dialogContext.Context.SendActivity("Thanks for the URL! I look if I can make use of it");
        }

        private Task GetCurrentSongDialog(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            return dialogContext.Context.SendActivity("Sorry but there is no music playing at the moment :/");
        }

        private static (Intents, RecognizerResult) ProcessLuis(ITurnContext context, double luisIntentThreshold)
        {
            var luisResult = context.Services.Get<RecognizerResult>(
                LuisRecognizerMiddleware.LuisRecognizerResultKey);

            var (intent, score) = luisResult.GetTopScoringIntent();

            if (score < luisIntentThreshold) return (Intents.None, luisResult);

            if (!Enum.TryParse(intent, true, out Intents intentResult))
                intentResult = Intents.None;

            return (intentResult, luisResult);
        }

        private Dialogs BeginNone(RecognizerResult luisResult)
        {
            return Dialogs.Introduction;
        }

        private Dialogs BeginGetCurrentSong(RecognizerResult luisResult)
        {
            return Dialogs.GetCurrentSong;
        }

        private Dialogs BeginPlaySong(RecognizerResult luisResult)
        {
            return luisResult.Entities.TryGetValue("Entertainment.Title", out var entity)
                ? Dialogs.AskArtist
                : Dialogs.AskTitleAndArtist;
        }

        private readonly DialogSet _dialogs = new DialogSet();

        private readonly LuisModel _luisModel = new LuisModel(
            "de0b2f73-861f-42b9-82d2-61f4196b20eb",
            "049fb6cc089b4ad291556f1cbc1b70b4",
            new Uri("https://westeurope.api.cognitive.microsoft.com/luis/v2.0/apps/"));
        //Environment.GetEnvironmentVariable("LuisAppId", EnvironmentVariableTarget.Process),
        //Environment.GetEnvironmentVariable("LuisSubscriptionKey", EnvironmentVariableTarget.Process),
        //new Uri(Environment.GetEnvironmentVariable("LuisDomain", EnvironmentVariableTarget.Process) ??
        //        throw new Exception()));

        private readonly string[] _providers =
        {
            "YouTube"
        };
    }
}