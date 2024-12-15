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

                await newsService.GetNewsFromRssFeed("https://www.sozcu.com.tr/feeds-rss-category-gundem", "Sözcü", "gundem");
                await newsService.GetNewsFromRssFeed("https://www.cumhuriyet.com.tr/rss", "Cumhuriyet", "gundem");
                await newsService.GetNewsFromRssFeed("https://onedio.com/Publisher/publisher-gundem.rss", "Onedio", "gundem");
                await newsService.GetNewsFromRssFeed("https://www.megabayt.com/rss", "Megabayt", "bilim");
                await newsService.GetNewsFromRssFeed("https://arkeofili.com/feed/", "Arkeofili", "bilim");
                await newsService.GetNewsFromRssFeed("https://onedio.com/Publisher/publisher-yasam.rss", "Onedio Yaşam", "yasam");
                await newsService.GetNewsFromRssFeed("https://www.aa.com.tr/tr/rss/default?cat=guncel", "Anadolu Ajansı", "gundem");
                await newsService.GetNewsFromRssFeed("https://www.mynet.com/haber/rss/kategori/yasam", "Mynet", "yasam");
                await newsService.GetNewsFromRssFeed("https://www.beyazperde.com/rss/haberler.xml", "Beyaz Perde", "yasam");
                await newsService.GetNewsFromRssFeed("https://feeds.bbci.co.uk/turkce/rss.xml", "BBC", "gundem");
                await newsService.GetNewsFromRssFeed("https://arsiv.mackolik.com/Rss", "Mackolik", "spor");
                await newsService.GetNewsFromRssFeed("https://www.trthaber.com/spor_articles.rss", "Trt Spor", "spor");
                await newsService.GetNewsFromRssFeed("https://onedio.com/Publisher/publisher-gaming.rss", "Onedio Gaming", "gaming");
                await newsService.GetNewsFromRssFeed("https://onedio.com/Publisher/publisher-test.rss", "Onedio Testler", "test");
                await newsService.InternationalGetNewsFromRssFeed("https://feeds.skynews.com/feeds/rss/home.xml/", "Sky News", "news");
                await newsService.InternationalGetNewsFromRssFeed("https://www.yahoo.com/news/rss", "Yahoo World", "news");
                await newsService.InternationalGetNewsFromRssFeed("https://feeds.bbci.co.uk/news/world/rss.xml", "BBC International", "news");
                await newsService.InternationalGetNewsFromRssFeed("https://www.independent.co.uk/life-style/rss", "Independent Life", "life");
                await newsService.InternationalGetNewsFromRssFeed("https://www.yahoo.com/lifestyle/rss/", "Yahoo Life", "life");
                await newsService.InternationalGetNewsFromRssFeed("https://www.independent.co.uk/sport/rss", "Independent Sport", "sport");
                await newsService.InternationalGetNewsFromRssFeed("https://globalnews.ca/sports/feed/", "Global News Sport", "sport");
                await newsService.InternationalGetNewsFromRssFeed("https://www.newscientist.com/feed/home/", "New Scientist", "tech");
                await newsService.InternationalGetNewsFromRssFeed("https://www.independent.co.uk/news/science/rss", "Independent Science", "tech");
                await newsService.InternationalGetNewsFromRssFeed("https://scitechdaily.com/feed/", "SciTech", "tech");
                await newsService.InternationalGetNewsFromRssFeed("https://www.buzzfeed.com/quizzes.xml", "BuzzFeed", "test");
                await newsService.InternationalGetNewsFromRssFeed("http://feeds.feedburner.com/ign/games-all", "IGN", "gaming");



            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
