using System.Diagnostics;
using System.Globalization;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using NetCord.Rest;

namespace Helpers {
    public static class MessageHelper {
        public static T CreateMessage<T>() where T : IMessageProperties, new()
        {
            return new()
            {
                Content = "",
                Components = [],
            };
        }

        public static async Task<T> GetEventMessage<T>(int week) where T: IMessageProperties, new() {
            try {
                // download the page as html and save as image
                var myEvent = getCurrentEvents(week);

                var message = CreateMessage<T>();

                if (IsEventPosted(myEvent)) {
                    
                    var tableRows = myEvent.QuerySelectorAll("tr");

                    for(int i = 1; i < tableRows.Count(); i++) {
                        var eventImage = myEvent.QuerySelector($"table > tbody > tr:nth-child({i}) > td.image > img").GetAttributeValue("src","");
                        var eventTitle = myEvent.QuerySelector($"table > tbody > tr:nth-child({i}) > td.quest > div > span").InnerText;
                        var eventDescription = myEvent.QuerySelector($"table > tbody > tr:nth-child({i}) > td.quest > p.txt").InnerText;
                        var eventDifficulty = myEvent.QuerySelector($"table > tbody > tr:nth-child({i}) > td.level > span").InnerText;
                        var questInfoFieldNames = myEvent.QuerySelectorAll($"table > tbody > tr:nth-child({i}) > td.overview > ul > li > span.overview_dt");
                        var questInfoFieldValues = myEvent.QuerySelectorAll($"table > tbody > tr:nth-child({i}) > td.overview > ul > li > span.overview_dd");

                        var fileName = $"event{i}week{week}.png";

                        await downloadImage(eventImage, fileName);

                        var embed = new EmbedProperties()
                        {
                            Title = "Monster Hunter Event Quest: " + eventTitle,
                            Description = eventDescription,
                            Url =$"https://info.monsterhunter.com/wilds/event-quest/en-us/schedule?=e{i}", // must be unique for multiple embeds
                            Timestamp = DateTimeOffset.UtcNow,
                            Color = new(0xFFA500),
                            Footer = new()
                            {
                                Text = "Happy Hunting!!"
                            },
                            Image = $"attachment://{fileName}",
                            Fields =
                            [
                                new()
                                {
                                    Name = "Difficulty",
                                    Value = eventDifficulty,
                                    Inline = true,
                                },
                            ],
                        };

                        for(int j = 0; j < questInfoFieldNames.Count(); j++) {
                            var field = new EmbedFieldProperties();
                            field.Name = questInfoFieldNames[j].InnerText.Trim().TrimStart(':');
                            field.Value = questInfoFieldValues[j].InnerText.Trim().TrimStart(':');

                            // if we find the date field, lets format it
                            if (field.Name.Contains("Date")) {
                                var temp = DateTime.ParseExact(field.Value, "MM.dd.yyyy HH:mm", CultureInfo.InvariantCulture);
                                field.Value = temp.ToString("MM/dd/yy h:mm tt");
                            }

                            field.Inline = true;
                            embed.AddFields(field);
                        }

                        var attachment = new AttachmentProperties(fileName, new MemoryStream(File.ReadAllBytes("images/" + fileName)));
                        message.AddAttachments(attachment);
                        message.AddEmbeds(embed);
                    }
                } else {
                    // else, we need to just display "comming soon"
                    var embed = new EmbedProperties()
                    {
                        Title = "Coming Soon",
                        Description = "The event quest has not been posted yet, try again later!",
                        Url =$"https://info.monsterhunter.com/wilds/event-quest/en-us/schedule?=event",
                        Timestamp = DateTimeOffset.UtcNow,
                        Color = new(0xFFA500),
                        Footer = new()
                        {
                            Text = "Happy Hunting!!"
                        },
                        Image = $"attachment://comingsoon.png",
                    };

                    var attachment = new AttachmentProperties("comingsoon.png", new MemoryStream(File.ReadAllBytes("images/comingsoon.png")));
                    message.AddAttachments(attachment);
                    message.AddEmbeds(embed);
                }
                
                // either the embedded quests or a single embed with the comming soon image
                return message;
            }
            catch
            {
                var message = CreateMessage<T>();
                message.Content = "Sorry, request failed! Try again later!";
                return message;
            }
        }

        public static async Task<T> GetChallengeMessage<T>(int week) where T: IMessageProperties, new() {
            try {
                // download the page as html and save as image
                var myChallenge = getCurrentChallenges(week);

                var message = CreateMessage<T>();

                if (IsChallengeComingSoon(myChallenge)) {
                    
                    var tableRows = myChallenge.QuerySelectorAll("tr");

                    for(int i = 1; i < tableRows.Count(); i++) {
                        var challengeImage = myChallenge.QuerySelector($"table > tbody > tr:nth-child({i}) > td.image > img").GetAttributeValue("src","");
                        var challengeTitle = myChallenge.QuerySelector($"table > tbody > tr:nth-child({i}) > td.quest > div > span").InnerText;
                        var challengeDescription = myChallenge.QuerySelector($"table > tbody > tr:nth-child({i}) > td.quest > p.txt").InnerText;
                        var challengeDifficulty = myChallenge.QuerySelector($"table > tbody > tr:nth-child({i}) > td.level > span").InnerText;
                        var questInfoFieldNames = myChallenge.QuerySelectorAll($"table > tbody > tr:nth-child({i}) > td.overview > ul > li > span.overview_dt");
                        var questInfoFieldValues = myChallenge.QuerySelectorAll($"table > tbody > tr:nth-child({i}) > td.overview > ul > li > span.overview_dd");

                        var fileName = $"challenge{i}week{week}.png";

                        await downloadImage(challengeImage, fileName);

                        var embed = new EmbedProperties()
                        {
                            Title = "Monster Hunter Challenge Quest: " + challengeTitle,
                            Description = challengeDescription,
                            Url =$"https://info.monsterhunter.com/wilds/event-quest/en-us/schedule?=c{i}", // must be unique for multiple embeds
                            Timestamp = DateTimeOffset.UtcNow,
                            Color = new(0x32a852),
                            Footer = new()
                            {
                                Text = "Happy Hunting!!"
                            },
                            Image = $"attachment://{fileName}",
                            Fields =
                            [
                                new()
                                {
                                    Name = "Difficulty",
                                    Value = challengeDifficulty,
                                    Inline = true,
                                },
                            ],
                        };

                        for(int j = 0; j < questInfoFieldNames.Count(); j++) {
                            var field = new EmbedFieldProperties();
                            field.Name = questInfoFieldNames[j].InnerText.Trim().TrimStart(':');
                            field.Value = questInfoFieldValues[j].InnerText.Trim().TrimStart(':');

                            // if we find the date field, lets format it
                            if (field.Name.Contains("Date")) {
                                var temp = DateTime.ParseExact(field.Value, "MM.dd.yyyy HH:mm", CultureInfo.InvariantCulture);
                                field.Value = temp.ToString("MM/dd/yy h:mm tt");
                            }

                            field.Inline = true;
                            embed.AddFields(field);
                        }

                        var attachment = new AttachmentProperties(fileName, new MemoryStream(File.ReadAllBytes("images/" + fileName)));
                        message.AddAttachments(attachment);
                        message.AddEmbeds(embed);
                    }
                } else if (myChallenge != null) {
                    // else, we need to just display "comming soon"
                    var embed = new EmbedProperties()
                    {
                        Title = "Coming Soon",
                        Description = "The challenge quest has not been posted yet, try again later!",
                        Url =$"https://info.monsterhunter.com/wilds/event-quest/en-us/schedule?=challenge",
                        Timestamp = DateTimeOffset.UtcNow,
                        Color = new(0x32a852),
                        Footer = new()
                        {
                            Text = "Happy Hunting!!"
                        },
                        Image = $"attachment://comingsoon.png",
                    };

                    var attachment = new AttachmentProperties("comingsoon.png", new MemoryStream(File.ReadAllBytes("images/comingsoon.png")));
                    message.AddAttachments(attachment);
                    message.AddEmbeds(embed);
                }
                
                // either the embedded quests or a single embed with the comming soon image
                return message;
            }
            catch
            {
                var message = CreateMessage<T>();
                message.Content = "Sorry, request failed! Try again later!";
                return message;
            }
        }

        public static HtmlNode? getCurrentEvents(int week = 0) {
            try
            {
                var web = new HtmlWeb();
                var url = "https://info.monsterhunter.com/wilds/event-quest/en-us/schedule?utc=-5";
                var document = web.Load(url);

                var htmlNode = document.DocumentNode.QuerySelector($"#tab{week}").QuerySelector(".table2");

                return htmlNode;
            }
            catch(Exception ex) 
            {   
                Debug.WriteLine(ex, ex.Message);
                Console.WriteLine("Request Failed");
                return null;
            }
        }

        public static HtmlNode? getCurrentChallenges(int week = 0) {
            try
            {
                var web = new HtmlWeb();
                var url = "https://info.monsterhunter.com/wilds/event-quest/en-us/schedule?utc=-5";
                var document = web.Load(url);

                var htmlNode = document.DocumentNode.QuerySelector($"#tab{week}").QuerySelector(".table3");

                return htmlNode;
            }
            catch(Exception ex) 
            {   
                Debug.WriteLine(ex, ex.Message);
                Console.WriteLine("Request Failed");
                return null;
            }
        }

        public static async Task downloadImage(string url, string filename) {
            using (var client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
                    byte[] imageBytes = await client.GetByteArrayAsync(url);
                    await File.WriteAllBytesAsync("images/" + filename, imageBytes);
                    Console.WriteLine($"Image downloaded and saved as: {filename}");
                }
                catch (Exception ex)
                {
                    // Handle any exceptions (e.g., network issues, invalid URL)
                    Console.WriteLine($"Error downloading image: {ex.Message}");
                }
            }
        }

        public static string GetRoleFromId(string roleId)
        {
            return $"<@&{roleId}>";
        }

        // Sometimes the latest info says "Coming Soon" instead of an actual post, so lets look for it
        public static bool IsEventPosted(HtmlNode? myEvent){
            if (myEvent == null) return false;

            var commingSoonText = myEvent.QuerySelector(".coming-quest_inner");

            return commingSoonText == null;
        }

        public static bool IsChallengeComingSoon(HtmlNode? myChallenge){
            if (myChallenge == null) return false;

            var commingSoonText = myChallenge.QuerySelector(".coming-quest_inner");

            return commingSoonText == null;
        }

        public static bool IsChallengePostedAtAll(HtmlNode? myChallenge){
            if (myChallenge == null) return false;

            var commingSoonText = myChallenge.QuerySelector("");

            return commingSoonText == null;
        }
    }
}