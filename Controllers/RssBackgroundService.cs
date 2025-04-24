using static System.Net.WebRequestMethods;

public class RssBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services;

    public RssBackgroundService(IServiceProvider services)
    {
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _services.CreateScope())
            {
                var newsService = scope.ServiceProvider.GetRequiredService<NewsService>();

                var rssFeeds = new List<(string Url, string Publisher, string Category)>
                {
                    ("https://www.sozcu.com.tr/feeds-rss-category-gundem", "Sözcü", "gundem"),
                    ("https://www.cumhuriyet.com.tr/rss", "Cumhuriyet", "gundem"),
                    ("https://onedio.com/Publisher/publisher-gundem.rss", "Onedio", "gundem"),
                    ("https://www.megabayt.com/rss", "Megabayt", "bilim"),
                     ("https://www.megabayt.com/rss/oyun", "Megabayt", "gaming"),
                    ("https://arkeofili.com/feed/", "Arkeofili", "bilim"),
                     ("https://www.webtekno.com/rss.xml", "WebTekno", "bilim"),
                    ("https://onedio.com/Publisher/publisher-yasam.rss", "Onedio Yaşam", "yasam"),
                    ("https://www.aa.com.tr/tr/rss/default?cat=guncel", "Anadolu Ajansı", "gundem"),
                    ("https://www.mynet.com/haber/rss/kategori/yasam", "Mynet", "yasam"),
                    ("https://www.beyazperde.com/rss/haberler.xml", "Beyaz Perde", "yasam"),
                    ("https://feeds.bbci.co.uk/turkce/rss.xml", "BBC", "gundem"),
                    ("https://arsiv.mackolik.com/Rss", "Mackolik", "spor"),
                    ("https://www.trthaber.com/spor_articles.rss", "Trt Spor", "spor"),
                    ("https://onedio.com/Publisher/publisher-gaming.rss", "Onedio Gaming", "gaming"),
                    ("https://onedio.com/Publisher/publisher-test.rss", "Onedio Testler", "test"),
                    ("https://feeds.skynews.com/feeds/rss/home.xml/", "Sky News International", "news"),
                    ("https://www.yahoo.com/news/rss", "Yahoo World International", "news"),
                    ("https://feeds.bbci.co.uk/news/world/rss.xml", "BBC International", "news"),
                    ("https://www.independent.co.uk/life-style/rss", "Independent Life International", "life"),
                    ("https://www.yahoo.com/lifestyle/rss/", "Yahoo Life International", "life"),
                    ("https://www.independent.co.uk/sport/rss", "Independent Sport International", "sport"),
                    ("https://globalnews.ca/sports/feed/", "Global News Sport International", "sport"),
                    ("https://www.newscientist.com/feed/home/", "New Scientist International", "tech"),
                    ("https://www.independent.co.uk/news/science/rss", "Independent Science International", "tech"),
                    ("https://scitechdaily.com/feed/", "SciTech International", "tech"),
                    ("https://www.buzzfeed.com/quizzes.xml", "BuzzFeed International", "test"),
                    ("http://feeds.feedburner.com/ign/games-all", "IGN International", "gaming")
                };

                foreach (var (url, publisher, category) in rssFeeds)
                {
                    try
                    {
                        if (publisher.Contains("International"))
                        {
                            await newsService.InternationalGetNewsFromRssFeed(url, publisher.Replace(" International",""), category);
                        }
                        else
                        {
                            await newsService.GetNewsFromRssFeed(url, publisher, category);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Hata oluştu: {ex.Message} (Publisher: {publisher}, Url: {url})");
                    
                    }
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
        }
    }
}
