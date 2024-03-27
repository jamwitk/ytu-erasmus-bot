using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using HtmlAgilityPack;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;

namespace BOT_TelegramAPI;

public class News
{
    public string Title { get; set; }
    public string Content { get; set; }
    public string Date { get; set; }
    public string Link { get; set; }
}
public static class Program
{
    static readonly CancellationTokenSource CancellationTokenSource = new();
    private const string Token = "";
    static readonly TelegramBotClient BotClient = new(Token);
    private static string _username = "@ytuduyurubot";
    
    //database 
    private static readonly List<News> MathAnnouncements = [];
    private static readonly List<News> ErasmusAnnouncements = [];
    private static readonly List<long> MathSubscribers = [];
    private static readonly List<long> ErasmusSubscribers = [];
    public static void Main(string[] args)
    {
        //DEBUG AREA
        
        //SendHttpRequestErasmusAsync(CancellationTokenSource).Wait();
        
        
        //DEBUG AREA
        
        
        StartBot();
        CheckForSubscribersFiles();

        
        
         
        Console.ReadLine();
    }

    private static void CheckForSubscribersFiles()
    {
        if (!File.Exists("math-subscribers.txt"))
        {
            File.Create("math-subscribers.txt");
        }
        else
        {
            var lines = File.ReadAllLines("math-subscribers.txt");
            foreach (var line in lines)
            {
                MathSubscribers.Add(long.Parse(line));
            }
        }
        
        if (!File.Exists("erasmus-subscribers.txt"))
        {
            File.Create("erasmus-subscribers.txt");
        }
        else
        {
            var lines = File.ReadAllLines("erasmus-subscribers.txt");
            foreach (var line in lines)
            {
                ErasmusSubscribers.Add(long.Parse(line));
            }
        }
    }
    private static IEnumerable<BotCommand> MatStartCommand()
    {
        yield return new BotCommand
        {
            Command = "matduyurubaslat",
            Description = "Matematik Bölümünün son duyurularını bildirir."
        };
        yield return new BotCommand
        {
            Command = "matduyurukapat",
            Description = "Artık Matematik Bölümünün son duyurularını bildirmez."
        };
        yield return new BotCommand
        {
            Command = "startErasmus",
            Description = "Erasmus duyurularını bildirir."
        };  
        
    }
    private static void StartBot()
    {
        ReceiverOptions receiverOptions = new ()
        {
            AllowedUpdates = Array.Empty<UpdateType>() 
        };
        
        BotClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions,CancellationTokenSource.Token); 
        var me = BotClient.GetMeAsync(cancellationToken: CancellationTokenSource.Token).Result;
        
        BotClient.SetMyCommandsAsync(MatStartCommand());
        
        Console.WriteLine($"Start listening for @{me.Username}"); Console.ReadLine();

        CancellationTokenSource.Cancel();
    }
    
    private static readonly HttpClient Client = new();
    
    private static async void LoopRequestMat()
    {
        Console.WriteLine("LoopRequestMat");
        Console.WriteLine(MathSubscribers.Count);
        while (true)
        {
            try
            {
                if (MathSubscribers.Count > 0)
                {
                    Console.WriteLine("Requesting... Mat Duyuru");
                    SendHttpRequestMatAsync(CancellationTokenSource).Wait(); 
                }
                await Task.Delay(300 * 1000, CancellationTokenSource.Token);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
    private static void LoopRequestErasmus()
    {
        Console.WriteLine("LoopRequestErasmus");
        Console.WriteLine(ErasmusSubscribers.Count);
        while (true)
        {
            try
            {
                if (ErasmusSubscribers.Count > 0)
                {
                    Console.WriteLine("Requesting... Erasmus Duyuru");
                    SendHttpRequestErasmusAsync(CancellationTokenSource).Wait(); 
                }
                Task.Delay(300 * 1000, CancellationTokenSource.Token).Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
    private static async Task SendHttpRequestMatAsync(CancellationTokenSource cancellationTokenSource)
    {
        try
        {
            
            string url = "https://mat.yildiz.edu.tr/duyurular/1/1/";
            using var request = await Client.GetAsync(url, cancellationTokenSource.Token);
            request.EnsureSuccessStatusCode();
            var responseContent = await request.Content.ReadAsStringAsync();
            
            //var responseContent = await File.ReadAllTextAsync("duyuru.html",cancellationTokenSource.Token);
            
            var list = EncryptMathFromHtml(responseContent);
            Console.WriteLine("List count: "+list.Count+" Announcements count: "+MathAnnouncements.Count);
            switch (list.Count)
            {
                case > 0 when MathAnnouncements.Count > 0:
                {
                    foreach (var news in list.Where(news => MathAnnouncements.All(x => x.Title != news.Title)))
                    {
                        MathAnnouncements.Add(news);
                        //var message = $"*{news.Title}*\n{news.Date}\n[Link]({news.Link})";
                        var message = "Yeni Duyuru | Toplanin toplanin günün gazetesi çıktı.";
                        message = EscapeMarkdown(message);
                        foreach (var subscriber in MathSubscribers)
                        {
                            InlineKeyboardMarkup inlineKeyboard = new(new[]
                            {
                                InlineKeyboardButton.WithUrl(
                                    text: "Duyuru Linki",
                                    url: news.Link)
                            });
                            await BotClient.SendTextMessageAsync(subscriber, message, null,ParseMode.MarkdownV2, 
                                replyMarkup: inlineKeyboard,
                                cancellationToken: cancellationTokenSource.Token);
                        }
                        Console.WriteLine("Yeni bir duyuru var! "+message);
                    }
                    break;
                }
                case > 0 when MathAnnouncements.Count == 0:
                {
                    MathAnnouncements.AddRange(list);
                    break;
                }
                case > 0 when MathAnnouncements.Count > 0:
                    break;
                default:
                    Console.WriteLine("No new announcement");
                    break;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

    }
    private static async Task SendHttpRequestErasmusAsync(CancellationTokenSource cancellationTokenSource)
    {
        try
        {
            string url = "http://www.erasmus.yildiz.edu.tr/haberler";
            using var request = await Client.GetAsync(url, cancellationTokenSource.Token);
            request.EnsureSuccessStatusCode();
            var responseContent = await request.Content.ReadAsStringAsync();
            //var responseContent = await File.ReadAllTextAsync("erasmus.html",cancellationTokenSource.Token);
            var list = EncryptErasmusFromHtml(responseContent);
            Console.WriteLine("List count: "+list.Count+" Announcements count: "+ErasmusAnnouncements.Count);
            switch (list.Count)
            {
                case > 0 when ErasmusAnnouncements.Count > 0:
                {
                    foreach (var news in list.Where(news => ErasmusAnnouncements.All(x => x.Title != news.Title)))
                    {
                        ErasmusAnnouncements.Add(news);
                        //var message = $"*{news.Title}*\n{news.Date}\n[Link]({news.Link})";
                        var message = "Yeni bir Erasmus duyurusu var!";
                        message = EscapeMarkdown(message);
                        foreach (var subscriber in ErasmusSubscribers)
                        {
                            InlineKeyboardMarkup inlineKeyboard = new(new[]
                            {
                                InlineKeyboardButton.WithUrl(
                                    text: "Duyuru Linki",
                                    url: news.Link)
                            });
                            await BotClient.SendTextMessageAsync(subscriber, message, null,ParseMode.MarkdownV2, 
                                replyMarkup: inlineKeyboard,
                                cancellationToken: cancellationTokenSource.Token);
                        }
                        Console.WriteLine("Yeni bir Erasmus duyurusu var! "+message);
                    }
                    break;
                }
                case > 0 when ErasmusAnnouncements.Count == 0:
                {
                    ErasmusAnnouncements.AddRange(list);
                    break;
                }
                case > 0 when ErasmusAnnouncements.Count > 0:
                    break;
                default:
                    Console.WriteLine("No new announcement");
                    break;
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    private static string EscapeMarkdown(string text) {
        //return text.Replace(/[_*[\]()~`>#\+\-=|{}.!]/g, "\\$&"); 
        return text.Replace("_", "\\_").Replace("*", "\\*")
            .Replace("[", "\\[")
            .Replace("]", "\\]")
            .Replace("(", "\\(")
            .Replace(")", "\\)")
            .Replace("~", "\\~")
            .Replace("`", "\\`")
            .Replace(">", "\\>")
            .Replace("#", "\\#")
            .Replace("+", "\\+")
            .Replace("-", "\\-")
            .Replace("=", "\\=")
            .Replace("|", "\\|")
            .Replace("{", "\\{")
            .Replace("}", "\\}")
            .Replace(".", "\\.")
            .Replace("!", "\\!");
    }

    private static List<News> EncryptErasmusFromHtml(string text)
    {
        var list = new List<News>();
        var html = new HtmlDocument();
        html.LoadHtml(text);
        var root = html.DocumentNode;
        var nodes = root.SelectSingleNode("//div[@class='col-md-9 col-sm-9 col-xs-12 page-content']");
        foreach (var node in nodes.ChildNodes)
        {
            if (node.Attributes["class"] == null || node.Attributes["class"].Value != "news-item") continue;
            News news = new News();
            foreach (var childNode in node.ChildNodes)
            {
                if(string.IsNullOrWhiteSpace(childNode.InnerText)) continue;
                
                
                news.Title = childNode.InnerText.Trim();
                
                if (childNode.Attributes["href"] != null)
                {
                    news.Link = childNode.Attributes["href"].Value;
                }
            }
            list.Add(news);
        }

        return list;
    }
    private static List<News> EncryptMathFromHtml(string text)
    {
        var list = new List<News>();
        var html = new HtmlDocument();
        html.LoadHtml(text);
        var root = html.DocumentNode;
        var nodes = root.SelectSingleNode("//div[@id='lisans']");
        foreach (var node in nodes.ChildNodes)
        {
            if (node.Attributes["class"] == null || node.Attributes["class"].Value != "one_announcement") continue;
            News news = new News();
            foreach (var childNode in node.ChildNodes)
            {
                if(string.IsNullOrWhiteSpace(childNode.InnerText)) continue;
                if (childNode.Attributes["class"] != null &&
                    childNode.Attributes["class"].Value == "one_announcement_date")
                {
                        news.Date = childNode.InnerText.Trim();
                        news.Date = Regex.Replace(news.Date, @"\s{2,}", " ");
                }
                else if (childNode.Attributes["id"] != null &&
                         childNode.Attributes["id"].Value == "all_news_title")
                {
                    news.Title = childNode.InnerText.Trim();
                    news.Link = childNode.SelectSingleNode("//div[@id='all_news_title']//a").Attributes["href"].Value;
                }
            }
            list.Add(news);
        }
        return list;
    }

    
    private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.Message)
        {
            var message = update.Message;
            if(message!.Type == MessageType.Text && message.Text!.Contains("matduyurubaslat"))
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Size Matematik Bölümünün son duyurularını bildirmeye başladım.", cancellationToken: cancellationToken);
                if (MathSubscribers.All(x => x != message.Chat.Id))
                {
                    MathSubscribers.Add(message.Chat.Id);
                    await File.WriteAllLinesAsync("math-subscribers.txt", MathSubscribers.Select(x => x.ToString()), cancellationToken);
                    LoopRequestMat();
                }
                Console.WriteLine("New subscriber for math: "+message.Chat.Id);
            }
            else if(message!.Type == MessageType.Text && message.Text!.Contains("matduyurukapat"))
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Size Matematik Bölümünün son duyurularını artık bildirmeyeceğim.", cancellationToken: cancellationToken);
                if (MathSubscribers.Contains(message.Chat.Id))
                {
                    MathSubscribers.Remove(message.Chat.Id);
                    await File.WriteAllLinesAsync("math-subscribers.txt", MathSubscribers.Select(x => x.ToString()), cancellationToken);
                }
            }
            else if (message!.Type == MessageType.Text && message.Text!.Contains("starterasmus"))
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Size Erasmus duyurularını bildirmeye başladım.", cancellationToken: cancellationToken);
                if (ErasmusSubscribers.All(x => x != message.Chat.Id))
                {
                    ErasmusSubscribers.Add(message.Chat.Id);
                    await File.WriteAllLinesAsync("erasmus-subscribers.txt", ErasmusSubscribers.Select(x => x.ToString()), cancellationToken);
                    LoopRequestErasmus();
                }

                Console.WriteLine("New subscriber for erasmus: "+message.Chat.Id);
            }
            else if (message!.Type == MessageType.Text && message.Text!.Contains("stoperasmus"))
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Size Erasmus duyurularını artık bildirmeyeceğim.", cancellationToken: cancellationToken);
                if (ErasmusSubscribers.Contains(message.Chat.Id))
                {
                    ErasmusSubscribers.Remove(message.Chat.Id);
                    await File.WriteAllLinesAsync("erasmus-subscribers.txt", ErasmusSubscribers.Select(x => x.ToString()), cancellationToken);
                }
            }
            
        }
    }

    

    private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }
    
}