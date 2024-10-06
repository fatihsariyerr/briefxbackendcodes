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
                await newsService.GetNewsFromRssFeed("https://www.megabayt.com/rss", "Megabayt", "bilim");
                await newsService.GetNewsFromRssFeed("https://arkeofili.com/feed/", "Arkeofili", "bilim");
                await newsService.GetNewsFromRssFeed("https://onedio.com/Publisher/publisher-yasam.rss", "Onedio", "yasam");
                await newsService.GetNewsFromRssFeed("https://www.mynet.com/haber/rss/kategori/yasam", "Mynet", "yasam");
                await newsService.GetNewsFromRssFeed("https://www.beyazperde.com/rss/haberler.xml", "Beyaz Perde", "yasam");
                await newsService.GetNewsFromRssFeed("https://feeds.bbci.co.uk/turkce/rss.xml", "BBC", "gundem");
                await newsService.GetNewsFromRssFeed("https://arsiv.mackolik.com/Rss", "Mackolik", "spor");
                await newsService.GetNewsFromRssFeed("https://www.trthaber.com/spor_articles.rss", "Trt Spor", "spor");
                await newsService.GetNewsFromRssFeed("https://onedio.com/Publisher/publisher-gaming.rss", "Onedio Gaming", "gaming");
                await newsService.GetNewsFromRssFeed("https://onedio.com/Publisher/publisher-test.rss", "Onedio Testler", "test");





            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
