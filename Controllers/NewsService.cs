﻿using System.ServiceModel.Syndication;
using System.Xml;
using Npgsql;
using Dapper;
using System.Xml.Linq;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

public class NewsService
{
    private readonly string _connectionString;
    private readonly IHttpClientFactory _httpClientFactory;

    public NewsService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _connectionString = configuration.GetConnectionString("PulseDatabase");
        _httpClientFactory = httpClientFactory;
    }
 



    public async Task<List<News>> InternationalGetNewsFromRssFeed(string url, string publisher, string category)
    {
        var feed = await GetFeed(url);
        var newsList = new List<News>();

        foreach (var item in feed.Items)
        {
        
            var publishDate = item.PublishDate.DateTime;

            if (item.PublishDate.Offset != TimeSpan.Zero)
            {
                publishDate = item.PublishDate.UtcDateTime;
            }
            else
            {
                publishDate = DateTime.SpecifyKind(publishDate, DateTimeKind.Utc);
            }

            var news = new News
            {
                Title = item.Title.Text,
                Link = item.Links.FirstOrDefault()?.Uri.ToString(),
                PublishDate = publishDate,
                Publisher = publisher,
                Category = category,
                Image = ExtractImage(item, publisher),
                Slug = ToSlug(item.Title.Text)
            };

            newsList.Add(news);
            await InternationalAddNewsIfNotExists(news);
        }

        return newsList;
    }

    public static string ToSlug(string text)
    {
        
        // Türkçe karakterleri doğru şekilde dönüştür
        var normalized = text
            .ToLowerInvariant()
            .Replace("ı", "i")  // ı -> i
            .Replace("ü", "u")  // ü -> u
            .Replace("ş", "s")  // ş -> s
            .Replace("ğ", "g")  // ğ -> g
            .Replace("ö", "o") // ö->o
            .Replace("I","i")
            .Replace("ç", "c"); // ç -> c


        // ASCII dışı karakterleri temizle
        normalized = Regex.Replace(normalized, @"[^a-z0-9\s-]", ""); // özel karakterleri sil
        normalized = Regex.Replace(normalized, @"\s+", " ").Trim();  // fazla boşlukları düzelt
        normalized = normalized.Substring(0, normalized.Length <= 80 ? normalized.Length : 80).Trim(); // max 80 karakter
        normalized = Regex.Replace(normalized, @"\s", "-"); // boşlukları tire yap

        return normalized;
    }



    public async Task<List<News>> GetNewsFromRssFeed(string url, string publisher, string category)
    {
        var feed = await GetFeed(url);

        if (feed == null)
        {
            Console.WriteLine($"RSS akışı alınamadı: {url}");
            return new List<News>();
        }

        var newsList = new List<News>();

        if (publisher == "Mackolik")
        {
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
                    Link = link,
                    PublishDate = item.PublishDate.DateTime.AddHours(3),
                    Publisher = publisher,
                    Category = category,
                    Image = ExtractImage(item, publisher)
                };

                newsList.Add(news);
                await AddNewsIfNotExists(news);
            }
        }
        else if (publisher == "Anadolu Ajansı")
        {
            using var reader = XmlReader.Create(url);
            var feedDocument = XDocument.Load(reader);

            var posts = feedDocument.Descendants("item").Select(item => new
            {
                Title = item.Element("title")?.Value,
                Link = item.Element("link")?.Value,
                PubDate = item.Element("pubDate") != null
                    ? DateTime.Parse(item.Element("pubDate")?.Value)
                    : (DateTime?)null,
                Image = item.Element("image")?.Value
            }).ToList();

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            foreach (var post in posts)
            {
                var exists = await connection.QueryFirstOrDefaultAsync<bool>(
                    "SELECT 1 FROM news WHERE link = @Link", new { Link = post.Link });
               

                if (!exists)
                {
                    await connection.ExecuteAsync(@"
                    INSERT INTO news (title, image, publishdate, link, publisher, category) 
                    VALUES (@Title, @Image, @PublishDate, @Link, @Publisher, @Category,@Slug)",
                        new
                        {
                            Title = post.Title,
                            Image = post.Image,
                            PublishDate = post.PubDate,
                            Link = post.Link,
                            Publisher = publisher,
                            Category = category,
                            Slug = ToSlug(post.Title)
                        });
                }
            }
        }
        else
        {
            foreach (var item in feed.Items)
            {
                var link = item.Links.FirstOrDefault()?.Uri.ToString();
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
        }

        return newsList;
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
                news.Slug = ToSlug(news.Title);
                await connection.ExecuteAsync(@"
                    INSERT INTO news (title, image, publishdate, link, publisher, category,slug) 
                    VALUES (@Title, @Image, @PublishDate, @Link, @Publisher, @Category,@Slug)",
                    news);
             
            }
        }
    }
    public async Task InternationalAddNewsIfNotExists(News news)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var exists = await connection.QueryFirstOrDefaultAsync<bool>(
                "SELECT 1 FROM newsinternational WHERE link = @Link", new { news.Link });

            if (!exists)
            {
                news.Slug = ToSlug(news.Title);
                await connection.ExecuteAsync(@"
                    INSERT INTO newsinternational (title, image, publishdate, link, publisher, category,slug) 
                    VALUES (@Title, @Image, @PublishDate, @Link, @Publisher, @Category,@Slug)",
                    news);
           
             

            }
        }
    }

    private async Task<SyndicationFeed?> GetFeed(string url)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(100);

            var xmlContent = await client.GetStringAsync(url);
            using (var reader = XmlReader.Create(new StringReader(xmlContent)))
            {
                return SyndicationFeed.Load(reader);
            }
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine($"Zaman aşımı oluştu: {url}");
            return null; 
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hata oluştu: {ex.Message}");
            return null; 
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
                return ExtractSozcuImage(item);
            case "Megabayt":
                return ExtractArkeofiliImage(item);
            case "Arkeofili":
                return ExtractArkeofiliImage(item);
            case "Onedio":
                return ExtractOnedioGundemImage(item);
            case "Onedio Yaşam":
                return ExtractOnedioImage(item);
            case "Mynet":
                return ExtractMynetImage(item);
          
            case "Beyaz Perde":
                return ExtractBeyazPerdeImage(item);
            case "BBC International":
                return ExtractBBCInternationalImage(item);
            case "Yahoo World":
                return ExtractYahooWorldImage(item);
            case "IGN":
                return ExtractIGNImage(item);
            case "Sky News":
                return ExtractSkyNewsImage(item);
            case "BuzzFeed":
                return ExtractBuzzFeedImage(item);
            case "Independent Life":
                return ExtractIndependentLifeImage(item);
            case "Yahoo Life":
                return ExtractYahooLivingImage(item);
            case "Independent Sport":
                return ExtractIndependentSportImage(item);
            case "Global News Sport":
                return ExtractGlobalNewsImage(item);
            case "New Scientist":
                return ExtractNewscientistImage(item);
            case "WebTekno":
                return ExtractWebTeknoImage(item);
            case "Independent Science":
                return ExtractIndependentScienceImage(item);
            case "SciTech":
                return ExtractSciTechImage(item);
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
    private string ExtractOnedioGundemImage(SyndicationItem item)
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
    private string ExtractSciTechImage(SyndicationItem item)
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

    private string ExtractWebTeknoImage(SyndicationItem item)
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
    private string ExtractIndependentSportImage(SyndicationItem item)
    {

        var enclosure = item.ElementExtensions
                            .FirstOrDefault(e => e.OuterName == "content" && e.OuterNamespace == "http://search.yahoo.com/mrss/");


        if (enclosure == null)
        {

            return null;
        }


        var element = enclosure.GetObject<XElement>();


        var url = element?.Attribute("url")?.Value;

        return url;


    }
    private string ExtractIndependentScienceImage(SyndicationItem item)
    {

        var enclosure = item.ElementExtensions
                             .FirstOrDefault(e => e.OuterName == "content" && e.OuterNamespace == "http://search.yahoo.com/mrss/");


        if (enclosure == null)
        {

            return null;
        }


        var element = enclosure.GetObject<XElement>();


        var url = element?.Attribute("url")?.Value;

        return url;


    }
    private string ExtractIndependentLifeImage(SyndicationItem item)
    {

        var enclosure = item.ElementExtensions
                            .FirstOrDefault(e => e.OuterName == "content" && e.OuterNamespace == "http://search.yahoo.com/mrss/");


        if (enclosure == null)
        {

            return null;
        }


        var element = enclosure.GetObject<XElement>();


        var url = element?.Attribute("url")?.Value;

        return url;


    }
    private string ExtractGlobalNewsImage(SyndicationItem item)
    {

        var enclosure = item.ElementExtensions
                             .FirstOrDefault(e => e.OuterName == "content" && e.OuterNamespace == "http://search.yahoo.com/mrss/");


        if (enclosure == null)
        {

            return null;
        }


        var element = enclosure.GetObject<XElement>();


        var url = element?.Attribute("url")?.Value;

        return url;


    }
    private string ExtractYahooWorldImage(SyndicationItem item)
    {

        var enclosure = item.ElementExtensions
                              .FirstOrDefault(e => e.OuterName == "content" && e.OuterNamespace == "http://search.yahoo.com/mrss/");


        if (enclosure == null)
        {

            return null;
        }


        var element = enclosure.GetObject<XElement>();


        var url = element?.Attribute("url")?.Value;

        return url;


    }
    private string ExtractIGNImage(SyndicationItem item)
    {

        var enclosure = item.ElementExtensions
                              .FirstOrDefault(e => e.OuterName == "content" && e.OuterNamespace == "http://search.yahoo.com/mrss/");


        if (enclosure == null)
        {

            return null;
        }


        var element = enclosure.GetObject<XElement>();


        var url = element?.Attribute("url")?.Value;

        return url;


    }
    private string ExtractYahooLivingImage(SyndicationItem item)
    {

        var enclosure = item.ElementExtensions
                           .FirstOrDefault(e => e.OuterName == "content" && e.OuterNamespace == "http://search.yahoo.com/mrss/");


        if (enclosure == null)
        {

            return null;
        }


        var element = enclosure.GetObject<XElement>();


        var url = element?.Attribute("url")?.Value;

        return url;


    }


    private string ExtractBBCInternationalImage(SyndicationItem item)
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
    private string ExtractNewscientistImage(SyndicationItem item)
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
    private string ExtractBuzzFeedImage(SyndicationItem item)
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
    private string ExtractSkyNewsImage(SyndicationItem item)
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
