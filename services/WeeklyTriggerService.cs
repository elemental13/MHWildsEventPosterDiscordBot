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
            await _client.SendMessageAsync(1350146109024112721, greetingMessage, null, stoppingToken);
            // collect new events and send
            // generate the message
            var message = await MessageHelper.GetEventMessage<MessageProperties>(0);
            // generate the challenge message
            var message2 = await MessageHelper.GetChallengeMessage<MessageProperties>(0);

            // add the challenge messages to the first one to send just one message together
            if (message2?.Embeds?.Count() > 0) {
                message.AddEmbeds(message2.Embeds);
                if (message2.Attachments != null) message.AddAttachments(message2.Attachments);
            }

            await _client.SendMessageAsync(1350146109024112721, message, null, stoppingToken);

            // after we send our message, lets close the application
            _applicationLifetime.StopApplication();
        }
    }
}