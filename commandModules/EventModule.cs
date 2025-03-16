using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using Helpers;

namespace CommandModules {
    public class EventModule : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SlashCommand("checkevents", "Check the current event quests going on this week!")]    
        public async Task GetEventsEmbedAsync() {
            await checkAndSendEventByWeek(0);
        }

        [SlashCommand("checknextweekevent", "Check the next weeks event quests coming soon!")]    
        public async Task CheckNextWeekEventAsync() {
            await checkAndSendEventByWeek(1);
        }

        [SlashCommand("checkweekafternextevent", "Check the week AFTER next (2 weeks from now) event quests coming soon!")]    
        public async Task CheckWeekAfterNextEventAsync() {
            await checkAndSendEventByWeek(2);
        }

        public async Task checkAndSendEventByWeek(int week) {
            try {
                // tells discord to wait, discord gives the user a "bot is thinking" message and I have roughly 15 minutes to respond
                await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());

                // generate the message
                var message = await MessageHelper.GetEventMessage<InteractionMessageProperties>(week);

                await Context.Interaction.SendFollowupMessageAsync(message);
            }
            catch
            {
                var message = MessageHelper.CreateMessage<InteractionMessageProperties>();
                message.Content = "Sorry, request failed! Try again later!";
                await Context.Interaction.SendFollowupMessageAsync(message);
            }
        }
    }
}