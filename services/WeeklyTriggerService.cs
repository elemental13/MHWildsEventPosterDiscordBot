using Helpers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetCord.Rest;

namespace Services {
    public class WeeklyTriggerService: BackgroundService
    {
        private readonly ILogger<WeeklyTriggerService> _logger;
        private RestClient _client;

        private IHostApplicationLifetime _applicationLifetime;

        private string monsterHunterEventNewsRoleId = "1350311731662164059";

        private ulong monsterHunterChannelId = 1350146109024112721;
        public WeeklyTriggerService(ILogger<WeeklyTriggerService> logger, IHostApplicationLifetime applicationLifetime, RestClient client)
        {
            _logger = logger;
            _applicationLifetime = applicationLifetime;
            _client = client;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("posting new weekly event!");
            // starting with a greeting
            var greetingMessage = MessageHelper.CreateMessage<MessageProperties>();
            greetingMessage.Content =$"{MessageHelper.GetRoleFromId(monsterHunterEventNewsRoleId)} New Event Quests Starting!!";
            await _client.SendMessageAsync(monsterHunterChannelId, greetingMessage, null, stoppingToken);
            // collect new events and send
            // generate the message list
            var messageList = await MessageHelper.GetEventMessageList<MessageProperties>(0);
            // generate the challenge message list
            var challengeList = await MessageHelper.GetChallengeMessageList<MessageProperties>(0);

            try {
                // could be multiple messages with groups of 10 embeds for discord limitations
                foreach(var message in messageList ?? new List<MessageProperties>()) {
                    await _client.SendMessageAsync(monsterHunterChannelId, message, null, stoppingToken);
                }

                // could be multiple messages with groups of 10 embeds for discord limitations
                foreach(var challenge in challengeList ?? new List<MessageProperties>()) {
                    await _client.SendMessageAsync(monsterHunterChannelId, challenge, null, stoppingToken);
                }
            } catch (Exception ex) {
                var errorMessage = MessageHelper.CreateMessage<MessageProperties>();
                errorMessage.Content = "Sorry, something went wrong!!";
                _logger.LogError(ex, ex.Message);
                await _client.SendMessageAsync(monsterHunterChannelId, errorMessage, null, stoppingToken);
            }

            // after we send our message, lets close the application
            _applicationLifetime.StopApplication();
        }
    }
}