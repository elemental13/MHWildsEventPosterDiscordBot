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
                var myEvents = getCurrentEvents(week);
                
                var message = CreateMessage<T>();
                var tableRows = myEvents.QuerySelectorAll("tr");

                for(int i = 1; i < tableRows.Count(); i++) {
                    var eventImage = myEvents.QuerySelector($"table > tbody > tr:nth-child({i}) > td.image > img").GetAttributeValue("src","");
                    var eventTitle = myEvents.QuerySelector($"table > tbody > tr:nth-child({i}) > td.quest > div > span").InnerText;
                    var eventDescription = myEvents.QuerySelector($"table > tbody > tr:nth-child({i}) > td.quest > p.txt").InnerText;
                    var eventDifficulty = myEvents.QuerySelector($"table > tbody > tr:nth-child({i}) > td.level > span").InnerText;
                    var questInfoFieldNames = myEvents.QuerySelectorAll($"table > tbody > tr:nth-child({i}) > td.overview > ul > li > span.overview_dt");
                    var questInfoFieldValues = myEvents.QuerySelectorAll($"table > tbody > tr:nth-child({i}) > td.overview > ul > li > span.overview_dd");

                    var fileName = $"event{i}week{week}.png";

                    await downloadImage(eventImage, fileName);

                    var embed = new EmbedProperties()
                    {
                        Title = "Monster Hunter Event Quest: " + eventTitle,
                        Description = eventDescription,
                        Url =$"https://info.monsterhunter.com/wilds/event-quest/en-us/schedule?={i}", // must be unique for multiple embeds
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

                var htmlNode = document.DocumentNode.QuerySelector($"#tab{week}");

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
    }
}