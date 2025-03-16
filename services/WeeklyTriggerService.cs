using Helpers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetCord.Gateway;
using NetCord.Rest;

namespace Services {
    public class WeeklyTriggerService: BackgroundService
    {
        private readonly ILogger<WeeklyTriggerService> _logger;
        private RestClient _client;

        private string monsterHunterEventNewsRoleId = "1350311731662164059";
        public WeeklyTriggerService(ILogger<WeeklyTriggerService> logger, RestClient client)
        {
            _logger = logger;
            _client = client;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("WeeklyTriggerService running at: {time}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                var dateTime = DateTimeOffset.UtcNow;
                // if on Tuesday after 7pm
                if (dateTime.DayOfWeek == DayOfWeek.Tuesday && dateTime.TimeOfDay > new TimeSpan(19, 0, 0))
                {
                    _logger.LogInformation("posting new weekly event!");
                    // starting with a greeting
                    var greetingMessage = MessageHelper.CreateMessage<MessageProperties>();
                    greetingMessage.Content =$"{MessageHelper.GetRoleFromId(monsterHunterEventNewsRoleId)} New Event Quests Starting!!";
                    await _client.SendMessageAsync(1350146109024112721, greetingMessage, null, stoppingToken);
                    // collect new events and send
                    var message = await MessageHelper.GetEventMessage<MessageProperties>(0);
                    await _client.SendMessageAsync(1350146109024112721, message, null, stoppingToken);
                }

                await WaitUntilNextTuesday(stoppingToken);
            }
        }

        private async Task WaitUntilNextTuesday(CancellationToken stoppingToken)
        {
            var today = DateTime.Today;
            var daysUntilTuesday = ((int) DayOfWeek.Tuesday - (int) today.DayOfWeek + 7) % 7;
            var nextTuesday = today.AddDays(daysUntilTuesday);
            var ts = new TimeSpan(19,1,0);
            nextTuesday = nextTuesday.Date + ts;

            var timeOffset = (nextTuesday - today).TotalMilliseconds;

            await Task.Delay(Convert.ToInt32(timeOffset), stoppingToken); // wait until tomorrow to do anything
        }
    }
}