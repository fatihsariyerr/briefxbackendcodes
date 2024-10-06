using System.ServiceModel.Syndication;
using System.Xml;
using Npgsql;
using Dapper;
using System.Xml.Linq;
using Microsoft.Extensions.Hosting;

public class NewsService
{
    private readonly string _connectionString;
    private readonly IHttpClientFactory _httpClientFactory;

    public NewsService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _connectionString = configuration.GetConnectionString("PulseDatabase");
        _httpClientFactory = httpClientFactory;
    }

    public async Task<List<News>> GetNewsFromRssFeed(string url, string publisher, string category)
    {
        if (publisher=="Mackolik")
        {
            var feed = await GetFeed(url);
            var newsList = new List<News>();

            foreach (var item in feed.Items)
            {
                var link = item.Links.FirstOrDefault()?.Uri.ToString();
                if (!string.IsNullOrEmpty(link) && link.StartsWith("//"))
                {
                    link = "https://" + link.Substring(2);
                }
                var news = new News
                {
                    Title = item.Title.Text,
                    Link = link.ToString(),
                    PublishDate = item.PublishDate.DateTime,
                    Publisher = publisher,
                    Category = category,
                    Image = ExtractImage(item, publisher)
                };

                newsList.Add(news);
                await AddNewsIfNotExists(news);
            }
            return newsList;
        }
        else if (publisher=="Trt Spor")
        {
            var feed = await GetFeed(url);
            var newsList = new List<News>();

            foreach (var item in feed.Items)
            {
                var link = item.Links.FirstOrDefault(l => l.RelationshipType == "alternate")?.Uri.ToString();
                var news = new News
                {
                    Title = item.Title.Text,
                    Link = link,
                    PublishDate = item.PublishDate.DateTime,
                    Publisher = publisher,
                    Category = category,
                    Image = ExtractImage(item, publisher)
                };

                newsList.Add(news);
                await AddNewsIfNotExists(news);
            }
            return newsList;
        }
        else
        {

        var feed = await GetFeed(url);
        var newsList = new List<News>();

        foreach (var item in feed.Items)
        {
            var news = new News
            {
                Title = item.Title.Text,
                Link = item.Links.FirstOrDefault()?.Uri.ToString(),
                PublishDate = item.PublishDate.DateTime,
                Publisher = publisher,
                Category = category,
                Image = ExtractImage(item, publisher)
            };

            newsList.Add(news);
            await AddNewsIfNotExists(news);
        }
        
      
        return newsList;
        }
    }

    public async Task AddNewsIfNotExists(News news)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var exists = await connection.QueryFirstOrDefaultAsync<bool>(
                "SELECT 1 FROM news WHERE link = @Link", new { news.Link });

            if (!exists)
            {
                await connection.ExecuteAsync(@"
                    INSERT INTO news (title, image, publishdate, link, publisher, category) 
                    VALUES (@Title, @Image, @PublishDate, @Link, @Publisher, @Category)",
                    news);
            }
        }
    }

    private async Task<SyndicationFeed> GetFeed(string url)
    {
        var client = _httpClientFactory.CreateClient();
        var xmlContent = await client.GetStringAsync(url);
        using (var reader = XmlReader.Create(new StringReader(xmlContent)))
        {
            return SyndicationFeed.Load(reader);
        }
    }

    private string ExtractImage(SyndicationItem item, string publisher)
    {
        switch (publisher)
        {
            case "Sözcü":
                return ExtractSozcuImage(item);
            case "Onedio Gaming":
                return ExtractOnedioGamingImage(item);
            case "Onedio Testler":
                return ExtractOnedioTestlerImage(item);
            case "Trt Spor":
                return ExtractTrtSporImage(item);
            case "Mackolik":
                return ExtractMackolikImage(item);
            case "BBC":
                return ExtractBBCImage(item);
            case "Cumhuriyet":
                return ExtractCumhuriyetImage(item);
            case "Megabayt":
                return ExtractMegabaytImage(item);
            case "Arkeofili":
                return ExtractArkeofiliImage(item);
            case "Onedio":
                return ExtractOnedioImage(item);
            case "Mynet":
                return ExtractMynetImage(item);
            case "Beyaz Perde":
                return ExtractBeyazPerdeImage(item);
          
            default:
                return null;
        }
    }

    private string ExtractTrtSporImage(SyndicationItem item)
    {
        var mediaContent = item.ElementExtensions
            .FirstOrDefault(e => e.OuterName == "content" && e.OuterNamespace == "http://search.yahoo.com/mrss/");
        return mediaContent?.GetObject<XElement>().Attribute("url")?.Value;
    }
    private string ExtractSozcuImage(SyndicationItem item)
    {
        var mediaContent = item.ElementExtensions
            .FirstOrDefault(e => e.OuterName == "content" && e.OuterNamespace == "http://search.yahoo.com/mrss/");
        return mediaContent?.GetObject<XElement>().Attribute("url")?.Value;
    }
    private string ExtractMynetImage(SyndicationItem item)
    {
        var imageElement = item.ElementExtensions
                        .FirstOrDefault(e => e.OuterName == "img300x300");

        return imageElement?.GetObject<XElement>().Value;


    }
    private string ExtractMackolikImage(SyndicationItem item)
    {
        string imageUrl = null;

        var imageElement = item.ElementExtensions
            .FirstOrDefault(e => e.OuterName == "img");
        if (imageElement != null)
        {
            imageUrl = imageElement.GetObject<XElement>().Value;
        }
        

        if (!string.IsNullOrEmpty(imageUrl) && imageUrl.StartsWith("//"))
        {
            imageUrl = "https://" + imageUrl.Substring(2); 
        }

        return imageUrl.ToString(); ;

    }
    private string ExtractArkeofiliImage(SyndicationItem item)
    {
        string imageUrl = null;

        string description = item.Summary?.Text;

        if (!string.IsNullOrEmpty(description))
        {

            var startIndex = description.IndexOf("src=\"") + 5;
            var endIndex = description.IndexOf("\"", startIndex);

            if (startIndex > 4 && endIndex > startIndex)
            {
                imageUrl = description.Substring(startIndex, endIndex - startIndex);
            }
        }
        return imageUrl;

    }

    private string ExtractMegabaytImage(SyndicationItem item)
    {

        var uri = item.Links[2].Uri.ToString();

        return uri;

    }
    private string ExtractOnedioGamingImage(SyndicationItem item)
    {

        var enclosure = item.ElementExtensions
                         .FirstOrDefault(e => e.OuterName == "content" && e.OuterNamespace == "http://search.yahoo.com/mrss/");
        var element = enclosure.GetObject<XElement>();
        var url = element.Attribute("url")?.Value;
        return url;


    }
    private string ExtractOnedioTestlerImage(SyndicationItem item)
    {

        var enclosure = item.ElementExtensions
                         .FirstOrDefault(e => e.OuterName == "content" && e.OuterNamespace == "http://search.yahoo.com/mrss/");
        var element = enclosure.GetObject<XElement>();
        var url = element.Attribute("url")?.Value;
        return url;


    }
    private string ExtractOnedioImage(SyndicationItem item)
    {
       
        var enclosure = item.ElementExtensions
                         .FirstOrDefault(e => e.OuterName == "content" && e.OuterNamespace == "http://search.yahoo.com/mrss/");
        var element = enclosure.GetObject<XElement>();
        var url = element.Attribute("url")?.Value;
        return url;


    }

    private string ExtractBeyazPerdeImage(SyndicationItem item)
    {

        var enclosure = item.ElementExtensions
                        .FirstOrDefault(e => e.OuterName == "enclosure");
        if (enclosure != null)
        {
            return enclosure?.GetObject<XElement>().Attribute("url")?.Value;
        }
        else
        {
            var thumbnail = item.ElementExtensions
                          .FirstOrDefault(e => e.OuterName == "thumbnail");

            return thumbnail?.GetObject<XElement>().Attribute("url")?.Value;

        }

    }
    private string ExtractBBCImage(SyndicationItem item)
    {

        var enclosure = item.ElementExtensions
                        .FirstOrDefault(e => e.OuterName == "enclosure");
        if (enclosure != null)
        {
            return enclosure?.GetObject<XElement>().Attribute("url")?.Value;
        }
        else
        {
            var thumbnail = item.ElementExtensions
                          .FirstOrDefault(e => e.OuterName == "thumbnail");

            return thumbnail?.GetObject<XElement>().Attribute("url")?.Value;

        }

    }
    private string ExtractCumhuriyetImage(SyndicationItem item)
    {
     
        var enclosure = item.ElementExtensions
                        .FirstOrDefault(e => e.OuterName == "enclosure");
        if (enclosure != null)
        {
            return enclosure?.GetObject<XElement>().Attribute("url")?.Value;
        }
        else
        {
            var thumbnail = item.ElementExtensions
                          .FirstOrDefault(e => e.OuterName == "thumbnail");
          
                return thumbnail?.GetObject<XElement>().Attribute("url")?.Value;
          
        }

    }
}
