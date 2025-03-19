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
            // Get the current local time
            DateTime localTime = DateTime.Now;

            // Find the target timezone (e.g., Central Standard Time)
            TimeZoneInfo targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");

            // Convert the local time to the central timezone
            DateTime todayDateTime = TimeZoneInfo.ConvertTime(localTime, targetTimeZone);

            _logger.LogInformation("WeeklyTriggerService running at: {time}", todayDateTime);

            while (!stoppingToken.IsCancellationRequested)
            {
                // if on Tuesday after 7pm and before 9pm
                if (todayDateTime.DayOfWeek == DayOfWeek.Tuesday && todayDateTime.TimeOfDay > new TimeSpan(19, 0, 0) && todayDateTime.TimeOfDay < new TimeSpan(21,0,0))
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

                await WaitUntilNextTuesday(todayDateTime, stoppingToken);
            }
        }

        private async Task WaitUntilNextTuesday(DateTime currentTime, CancellationToken stoppingToken)
        {
            var daysUntilTuesday = ((int) DayOfWeek.Tuesday - (int) currentTime.DayOfWeek + 7) % 7;
            if (daysUntilTuesday == 0) daysUntilTuesday = 7; // if today IS tuesday, the next one is 7
            var nextTuesday = currentTime.AddDays(daysUntilTuesday);
            var ts = new TimeSpan(19,1,0);
            nextTuesday = nextTuesday.Date + ts;

            var timeOffset = (nextTuesday - currentTime).TotalMilliseconds;

            var printOffset = nextTuesday - currentTime;

            _logger.LogInformation($"Waiting now until next Tuesday: {printOffset.Days} days, {printOffset.Hours} hours, {printOffset.Minutes} minutes");

            await Task.Delay(Convert.ToInt32(timeOffset), stoppingToken); // wait until next tuesday to do anything
        }
    }
}