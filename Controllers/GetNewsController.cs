﻿using Microsoft.AspNetCore.Mvc;
using System.ServiceModel.Syndication;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Extensions.Hosting;
using System.IO;
using ImageMagick;


namespace pulse.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GetNewsController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        public GetNewsController(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(100);
        }
      
        [HttpGet("convert-to-webp")]
        public async Task<IActionResult> ConvertToWebP([FromQuery] string imageUrl, [FromQuery] int quality = 80)
        {
            try
            {
                var response = await _httpClient.GetAsync(imageUrl);
                response.EnsureSuccessStatusCode();

                var imageData = await response.Content.ReadAsByteArrayAsync();

                using var image = new MagickImage(imageData);
                image.Format = MagickFormat.WebP;
                if (quality < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(quality), "Quality değeri negatif olamaz.");
                }

                image.Quality = (uint)quality;


                var webpData = image.ToByteArray();

                return File(webpData, "image/webp");
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        [HttpPost("anadoluajansi")]
        public async Task<IActionResult> PostanadoluajansiRssFeed([FromBody] NewsDetails newsDetails)
        {
        
            var url = newsDetails.url;
            try
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

                return Ok(posts);

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }

        [HttpPost("sozcu")]
        public async Task<IActionResult> PostSozcuRssFeed([FromBody] NewsDetails newsDetails)
        {
            var url = newsDetails.url;
            try
            {
                using var reader = XmlReader.Create(url);
                var feed = SyndicationFeed.Load(reader);

                var posts = feed.Items.Select(post =>
                {
                    var mediaContent = post.ElementExtensions
                        .FirstOrDefault(e => e.OuterName == "content" && e.OuterNamespace == "http://search.yahoo.com/mrss/");

                    string imageUrl = null;
                    if (mediaContent != null)
                    {
                        var element = mediaContent.GetObject<XElement>();
                        imageUrl = element.Attribute("url")?.Value;
                    }

                    return new
                    {
                        Title = post.Title.Text,
                        Link = post.Links.FirstOrDefault()?.Uri.ToString(),
                        PubDate = post.PublishDate.DateTime,
                        Image = imageUrl
                    };
                }).ToList();

                return Ok(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("skynews")]
        public async Task<IActionResult> PostSkyNewsRssFeed([FromBody] NewsDetails newsDetails)
        {
            var url = newsDetails.url;
            try
            {
                using var reader = XmlReader.Create(url);
                var feed = SyndicationFeed.Load(reader);

                var posts = feed.Items.Select(post =>
                {
                    string imageUrl = null;

                    var enclosure = post.ElementExtensions
                        .FirstOrDefault(e => e.OuterName == "enclosure");
                    if (enclosure != null)
                    {
                        var element = enclosure.GetObject<XElement>();
                        imageUrl = element.Attribute("url")?.Value;
                    }

                    if (imageUrl == null)
                    {
                        var thumbnail = post.ElementExtensions
                            .FirstOrDefault(e => e.OuterName == "thumbnail");
                        if (thumbnail != null)
                        {
                            var element = thumbnail.GetObject<XElement>();
                            imageUrl = element.Attribute("url")?.Value;
                        }
                    }

                    return new
                    {
                        Title = post.Title.Text,
                        Link = post.Links.FirstOrDefault()?.Uri.ToString(),
                        PubDate = post.PublishDate.DateTime,
                        Image = imageUrl
                    };
                }).ToList();

                return Ok(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("cumhuriyet")]
        public async Task<IActionResult> PostCumhuriyetRssFeed([FromBody] NewsDetails newsDetails)
        {
            var url = newsDetails.url;
            try
            {
                using var reader = XmlReader.Create(url);
                var feed = SyndicationFeed.Load(reader);

                var posts = feed.Items.Select(post =>
                {
                    string imageUrl = null;

                    var enclosure = post.ElementExtensions
                        .FirstOrDefault(e => e.OuterName == "enclosure");
                    if (enclosure != null)
                    {
                        var element = enclosure.GetObject<XElement>();
                        imageUrl = element.Attribute("url")?.Value;
                    }

                    if (imageUrl == null)
                    {
                        var thumbnail = post.ElementExtensions
                            .FirstOrDefault(e => e.OuterName == "thumbnail");
                        if (thumbnail != null)
                        {
                            var element = thumbnail.GetObject<XElement>();
                            imageUrl = element.Attribute("url")?.Value;
                        }
                    }

                    return new
                    {
                        Title = post.Title.Text,
                        Link = post.Links.FirstOrDefault()?.Uri.ToString(),
                        PubDate = post.PublishDate.DateTime,
                        Image = imageUrl
                    };
                }).ToList();

                return Ok(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("trt-spor")]
        public async Task<IActionResult> PostTrtSporRssFeed([FromBody] NewsDetails newsDetails)
        {
            var url = newsDetails.url;
            try
            {
                using var reader = XmlReader.Create(url);
                var feed = SyndicationFeed.Load(reader);

                var posts = feed.Items.Select(post =>
                {
                    var mediaContent = post.ElementExtensions
                        .FirstOrDefault(e => e.OuterName == "content" && e.OuterNamespace == "http://search.yahoo.com/mrss/");
                   
                    string imageUrl = null;
                    if (mediaContent != null)
                    {
                        var element = mediaContent.GetObject<XElement>();
                        imageUrl = element.Attribute("url")?.Value;
                    }

                    
                        var link = post.Links.FirstOrDefault(l => l.RelationshipType == "alternate")?.Uri.ToString();
                    

                    return new
                    {
                        Title = post.Title.Text,
                        Link = link,
                        PubDate = post.PublishDate.DateTime,
                        Image = imageUrl
                    };
                }).ToList();

                return Ok(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }




        [HttpPost("BBC")]
        public async Task<IActionResult> PostBBCRssFeed([FromBody] NewsDetails newsDetails)
        {
            var url = newsDetails.url;
            try
            {
                using var reader = XmlReader.Create(url);
                var feed = SyndicationFeed.Load(reader);

                var posts = feed.Items.Select(post =>
                {
                    string imageUrl = null;

                    var enclosure = post.ElementExtensions
                        .FirstOrDefault(e => e.OuterName == "enclosure");
                    if (enclosure != null)
                    {
                        var element = enclosure.GetObject<XElement>();
                        imageUrl = element.Attribute("url")?.Value;
                    }

                    if (imageUrl == null)
                    {
                        var thumbnail = post.ElementExtensions
                            .FirstOrDefault(e => e.OuterName == "thumbnail");
                        if (thumbnail != null)
                        {
                            var element = thumbnail.GetObject<XElement>();
                            imageUrl = element.Attribute("url")?.Value;
                        }
                    }

                    return new
                    {
                        Title = post.Title.Text,
                        Link = post.Links.FirstOrDefault()?.Uri.ToString(),
                        PubDate = post.PublishDate.DateTime,
                        Image = imageUrl
                    };
                }).ToList();

                return Ok(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }
        [HttpPost("YahooWorldRss")]
        public async Task<IActionResult> PostYahooWorldRssFeed([FromBody] NewsDetails newsDetails)
        {
            var url = newsDetails.url;
            try
            {
                using var reader = XmlReader.Create(url);
                var feed = SyndicationFeed.Load(reader);

                var posts = feed.Items.Select(post =>
                {
                    string imageUrl = null;

                    var enclosure = post.ElementExtensions
                         .FirstOrDefault(e => e.OuterName == "content" && e.OuterNamespace == "http://search.yahoo.com/mrss/");
                    if (enclosure != null)
                    {
                        var element = enclosure.GetObject<XElement>();
                        imageUrl = element.Attribute("url")?.Value;
                    }


                    return new
                    {
                        Title = post.Title.Text,
                        Link = post.Links.FirstOrDefault()?.Uri.ToString(),
                        PubDate = post.PublishDate.DateTime,
                        Image = imageUrl
                    };
                }).ToList();

                return Ok(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("globalnewssport")]
        public async Task<IActionResult> PostglobalnewssportRssFeed([FromBody] NewsDetails newsDetails)
        {
            var url = newsDetails.url;
            try
            {
                using var reader = XmlReader.Create(url);
                var feed = SyndicationFeed.Load(reader);

                var posts = feed.Items.Select(post =>
                {
                    string imageUrl = null;

                    var enclosure = post.ElementExtensions
                         .FirstOrDefault(e => e.OuterName == "content" && e.OuterNamespace == "http://search.yahoo.com/mrss/");
                    if (enclosure != null)
                    {
                        var element = enclosure.GetObject<XElement>();
                        imageUrl = element.Attribute("url")?.Value;
                    }


                    return new
                    {
                        Title = post.Title.Text,
                        Link = post.Links.FirstOrDefault()?.Uri.ToString(),
                        PubDate = post.PublishDate.DateTime,
                        Image = imageUrl
                    };
                }).ToList();

                return Ok(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("independentsportnews")]
        public async Task<IActionResult> PostIndependentSportNewsRssFeed([FromBody] NewsDetails newsDetails)
        {
            var url = newsDetails.url;
            try
            {
                using var reader = XmlReader.Create(url);
                var feed = SyndicationFeed.Load(reader);

                var posts = feed.Items.Select(post =>
                {
                    string imageUrl = null;

                    var enclosure = post.ElementExtensions
                         .FirstOrDefault(e => e.OuterName == "content" && e.OuterNamespace == "http://search.yahoo.com/mrss/");
                    if (enclosure != null)
                    {
                        var element = enclosure.GetObject<XElement>();
                        imageUrl = element.Attribute("url")?.Value;
                    }


                    return new
                    {
                        Title = post.Title.Text,
                        Link = post.Links.FirstOrDefault()?.Uri.ToString(),
                        PubDate = post.PublishDate.DateTime,
                        Image = imageUrl
                    };
                }).ToList();

                return Ok(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("independentsciencenews")]
        public async Task<IActionResult> PostIndependentScienceNewsRssFeed([FromBody] NewsDetails newsDetails)
        {
            var url = newsDetails.url;
            try
            {
                using var reader = XmlReader.Create(url);
                var feed = SyndicationFeed.Load(reader);

                var posts = feed.Items.Select(post =>
                {
                    string imageUrl = null;

                    var enclosure = post.ElementExtensions
                         .FirstOrDefault(e => e.OuterName == "content" && e.OuterNamespace == "http://search.yahoo.com/mrss/");
                    if (enclosure != null)
                    {
                        var element = enclosure.GetObject<XElement>();
                        imageUrl = element.Attribute("url")?.Value;
                    }


                    return new
                    {
                        Title = post.Title.Text,
                        Link = post.Links.FirstOrDefault()?.Uri.ToString(),
                        PubDate = post.PublishDate.DateTime,
                        Image = imageUrl
                    };
                }).ToList();

                return Ok(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("independentlifenews")]
        public async Task<IActionResult> PostIndependentLifeNewsRssFeed([FromBody] NewsDetails newsDetails)
        {
            var url = newsDetails.url;
            try
            {
                using var reader = XmlReader.Create(url);
                var feed = SyndicationFeed.Load(reader);

                var posts = feed.Items.Select(post =>
                {
                    string imageUrl = null;

                    var enclosure = post.ElementExtensions
                         .FirstOrDefault(e => e.OuterName == "content" && e.OuterNamespace == "http://search.yahoo.com/mrss/");
                    if (enclosure != null)
                    {
                        var element = enclosure.GetObject<XElement>();
                        imageUrl = element.Attribute("url")?.Value;
                    }


                    return new
                    {
                        Title = post.Title.Text,
                        Link = post.Links.FirstOrDefault()?.Uri.ToString(),
                        PubDate = post.PublishDate.DateTime,
                        Image = imageUrl
                    };
                }).ToList();

                return Ok(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("YahooLivingRss")]
        public async Task<IActionResult> PostYahooRssFeed([FromBody] NewsDetails newsDetails)
        {
            var url = newsDetails.url;
            try
            {
                using var reader = XmlReader.Create(url);
                var feed = SyndicationFeed.Load(reader);

                var posts = feed.Items.Select(post =>
                {
                    string imageUrl = null;

                    var enclosure = post.ElementExtensions
                         .FirstOrDefault(e => e.OuterName == "content" && e.OuterNamespace == "http://search.yahoo.com/mrss/");
                    if (enclosure != null)
                    {
                        var element = enclosure.GetObject<XElement>();
                        imageUrl = element.Attribute("url")?.Value;
                    }


                    return new
                    {
                        Title = post.Title.Text,
                        Link = post.Links.FirstOrDefault()?.Uri.ToString(),
                        PubDate = post.PublishDate.DateTime,
                        Image = imageUrl
                    };
                }).ToList();

                return Ok(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
   
        [HttpPost("BBCInternational")]
        public async Task<IActionResult> PostBBCInternationalRssFeed([FromBody] NewsDetails newsDetails)
        {
            var url = newsDetails.url;
            try
            {
                using var reader = XmlReader.Create(url);
                var feed = SyndicationFeed.Load(reader);

                var posts = feed.Items.Select(post =>
                {
                    string imageUrl = null;

                    var enclosure = post.ElementExtensions
                        .FirstOrDefault(e => e.OuterName == "enclosure");
                    if (enclosure != null)
                    {
                        var element = enclosure.GetObject<XElement>();
                        imageUrl = element.Attribute("url")?.Value;
                    }

                    if (imageUrl == null)
                    {
                        var thumbnail = post.ElementExtensions
                            .FirstOrDefault(e => e.OuterName == "thumbnail");
                        if (thumbnail != null)
                        {
                            var element = thumbnail.GetObject<XElement>();
                            imageUrl = element.Attribute("url")?.Value;
                        }
                    }

                    return new
                    {
                        Title = post.Title.Text,
                        Link = post.Links.FirstOrDefault()?.Uri.ToString(),
                        PubDate = post.PublishDate.DateTime,
                        Image = imageUrl
                    };
                }).ToList();

                return Ok(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }


        [HttpPost("beyaz-perde")]
        public async Task<IActionResult> PostBeyazPerdeRssFeed([FromBody] NewsDetails newsDetails)
        {
            var url = newsDetails.url;
            try
            {
                using var reader = XmlReader.Create(url);
                var feed = SyndicationFeed.Load(reader);

                var posts = feed.Items.Select(post =>
                {
                    string imageUrl = null;

                    var enclosure = post.ElementExtensions
                        .FirstOrDefault(e => e.OuterName == "enclosure");
                    if (enclosure != null)
                    {
                        var element = enclosure.GetObject<XElement>();
                        imageUrl = element.Attribute("url")?.Value;
                    }

                    if (imageUrl == null)
                    {
                        var thumbnail = post.ElementExtensions
                            .FirstOrDefault(e => e.OuterName == "thumbnail");
                        if (thumbnail != null)
                        {
                            var element = thumbnail.GetObject<XElement>();
                            imageUrl = element.Attribute("url")?.Value;
                        }
                    }

                    return new
                    {
                        Title = post.Title.Text,
                        Link = post.Links.FirstOrDefault()?.Uri.ToString(),
                        PubDate = post.PublishDate.DateTime,
                        Image = imageUrl
                    };
                }).ToList();

                return Ok(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("onediotest")]
        public async Task<IActionResult> PostOnedioTestRssFeed([FromBody] NewsDetails newsDetails)
        {
            var url = newsDetails.url;
            try
            {
                using var reader = XmlReader.Create(url);
                var feed = SyndicationFeed.Load(reader);

                var posts = feed.Items.Select(post =>
                {
                    string imageUrl = null;

                    var enclosure = post.ElementExtensions
                         .FirstOrDefault(e => e.OuterName == "content" && e.OuterNamespace == "http://search.yahoo.com/mrss/");
                    if (enclosure != null)
                    {
                        var element = enclosure.GetObject<XElement>();
                        imageUrl = element.Attribute("url")?.Value;
                    }


                    return new
                    {
                        Title = post.Title.Text,
                        Link = post.Links.FirstOrDefault()?.Uri.ToString(),
                        PubDate = post.PublishDate.DateTime,
                        Image = imageUrl
                    };
                }).ToList();

                return Ok(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("onediogundem")]
        public async Task<IActionResult> PostOnedioGundemRssFeed([FromBody] NewsDetails newsDetails)
        {
            var url = newsDetails.url;
            try
            {
                using var reader = XmlReader.Create(url);
                var feed = SyndicationFeed.Load(reader);

                var posts = feed.Items.Select(post =>
                {
                    string imageUrl = null;

                    var enclosure = post.ElementExtensions
                         .FirstOrDefault(e => e.OuterName == "content" && e.OuterNamespace == "http://search.yahoo.com/mrss/");
                    if (enclosure != null)
                    {
                        var element = enclosure.GetObject<XElement>();
                        imageUrl = element.Attribute("url")?.Value;
                    }


                    return new
                    {
                        Title = post.Title.Text,
                        Link = post.Links.FirstOrDefault()?.Uri.ToString(),
                        PubDate = post.PublishDate.DateTime,
                        Image = imageUrl
                    };
                }).ToList();

                return Ok(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("onediogaming")]
        public async Task<IActionResult> PostOnedioGamingRssFeed([FromBody] NewsDetails newsDetails)
        {
            var url = newsDetails.url;
            try
            {
                using var reader = XmlReader.Create(url);
                var feed = SyndicationFeed.Load(reader);

                var posts = feed.Items.Select(post =>
                {
                    string imageUrl = null;

                    var enclosure = post.ElementExtensions
                         .FirstOrDefault(e => e.OuterName == "content" && e.OuterNamespace == "http://search.yahoo.com/mrss/");
                    if (enclosure != null)
                    {
                        var element = enclosure.GetObject<XElement>();
                        imageUrl = element.Attribute("url")?.Value;
                    }


                    return new
                    {
                        Title = post.Title.Text,
                        Link = post.Links.FirstOrDefault()?.Uri.ToString(),
                        PubDate = post.PublishDate.DateTime,
                        Image = imageUrl
                    };
                }).ToList();

                return Ok(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("onedio")]
        public async Task<IActionResult> PostOnedioRssFeed([FromBody] NewsDetails newsDetails)
        {
            var url = newsDetails.url;
            try
            {
                using var reader = XmlReader.Create(url);
                var feed = SyndicationFeed.Load(reader);

                var posts = feed.Items.Select(post =>
                {
                    string imageUrl = null;

                   var enclosure= post.ElementExtensions
                        .FirstOrDefault(e => e.OuterName == "content" && e.OuterNamespace == "http://search.yahoo.com/mrss/");
                    if (enclosure != null)
                    {
                        var element = enclosure.GetObject<XElement>();
                        imageUrl = element.Attribute("url")?.Value;
                    }


                    return new
                    {
                        Title = post.Title.Text,
                        Link = post.Links.FirstOrDefault()?.Uri.ToString(),
                        PubDate = post.PublishDate.DateTime,
                        Image = imageUrl
                    };
                }).ToList();

                return Ok(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpPost("newscientist")]
        public async Task<IActionResult> PostNewScientistFeed([FromBody] NewsDetails newsDetails)
        {
            var url = newsDetails.url;
            try
            {
                using var reader = XmlReader.Create(url);
                var feed = SyndicationFeed.Load(reader);

                var posts = feed.Items.Select(post =>
                {
                    string imageUrl = null;

                    var enclosure = post.ElementExtensions
                        .FirstOrDefault(e => e.OuterName == "enclosure");
                    if (enclosure != null)
                    {
                        var element = enclosure.GetObject<XElement>();
                        imageUrl = element.Attribute("url")?.Value;
                    }

                    if (imageUrl == null)
                    {
                        var thumbnail = post.ElementExtensions
                            .FirstOrDefault(e => e.OuterName == "thumbnail");
                        if (thumbnail != null)
                        {
                            var element = thumbnail.GetObject<XElement>();
                            imageUrl = element.Attribute("url")?.Value;
                        }
                    }

                    return new
                    {
                        Title = post.Title.Text,
                        Link = post.Links.FirstOrDefault()?.Uri.ToString(),
                        PubDate = post.PublishDate.DateTime,
                        Image = imageUrl
                    };
                }).ToList();

                return Ok(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("SciTech")]
        public async Task<IActionResult> PostSciTechRssFeed([FromBody] NewsDetails newsDetails)
        {
            var url = newsDetails.url;
            try
            {
                using var reader = XmlReader.Create(url);
                var feed = SyndicationFeed.Load(reader);

                var posts = feed.Items.Select(post =>
                {

                    string imageUrl = null;

                    string description = post.Summary?.Text;

                    if (!string.IsNullOrEmpty(description))
                    {

                        var startIndex = description.IndexOf("src=\"") + 5;
                        var endIndex = description.IndexOf("\"", startIndex);

                        if (startIndex > 4 && endIndex > startIndex)
                        {
                            imageUrl = description.Substring(startIndex, endIndex - startIndex);
                        }
                    }

                    return new
                    {
                        Title = post.Title.Text,
                        Link = post.Links.FirstOrDefault()?.Uri.ToString(),
                        PubDate = post.PublishDate.DateTime,
                        Image = imageUrl
                    };
                }).ToList();

                return Ok(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
            [HttpPost("arkeofili")]
public async Task<IActionResult> PostArkeofiliRssFeed([FromBody] NewsDetails newsDetails)
{
    var url = newsDetails.url;
    try
    {
        using var reader = XmlReader.Create(url);
        var feed = SyndicationFeed.Load(reader);

        var posts = feed.Items.Select(post =>
        {

            string imageUrl = null;

            string description = post.Summary?.Text;

            if (!string.IsNullOrEmpty(description))
            {
            
                var startIndex = description.IndexOf("src=\"") + 5; 
                var endIndex = description.IndexOf("\"", startIndex); 

                if (startIndex > 4 && endIndex > startIndex) 
                {
                    imageUrl = description.Substring(startIndex, endIndex - startIndex);
                }
            }

            return new
            {
                Title = post.Title.Text,
                Link = post.Links.FirstOrDefault()?.Uri.ToString(),
                PubDate = post.PublishDate.DateTime,
                Image = imageUrl
            };
        }).ToList();

        return Ok(posts);
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Internal server error: {ex.Message}");
    }
}


        [HttpPost("mynet")]
        public async Task<IActionResult> PostMynetRssFeed([FromBody] NewsDetails newsDetails)
        {
            var url = newsDetails.url;
            try
            {
                using var reader = XmlReader.Create(url);
                var feed = SyndicationFeed.Load(reader);

                var posts = feed.Items.Select(post =>
                {
                    string imageUrl = null;

                    var imageElement = post.ElementExtensions
                        .FirstOrDefault(e => e.OuterName == "img300x300");
                    if (imageElement != null)
                    {
                        imageUrl = imageElement.GetObject<XElement>().Value;
                    }

                    return new
                    {
                        Title = post.Title.Text,
                        Link = post.Links.FirstOrDefault()?.Uri.ToString(),
                        PubDate = post.PublishDate.DateTime,
                        Image = imageUrl
                    };
                }).ToList();

                return Ok(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("mackolik")]
        public async Task<IActionResult> PostMackolikFeed([FromBody] NewsDetails newsDetails)
        {
            var url = newsDetails.url;
            try
            {
                using var reader = XmlReader.Create(url);
                var feed = SyndicationFeed.Load(reader);

                var posts = feed.Items.Select(post =>
                {
                    string imageUrl = null;

                    var imageElement = post.ElementExtensions
                        .FirstOrDefault(e => e.OuterName == "img");
                    if (imageElement != null)
                    {
                        imageUrl = imageElement.GetObject<XElement>().Value;
                    }
                    var link = post.Links.FirstOrDefault()?.Uri.ToString();
                    if (!string.IsNullOrEmpty(link) && link.StartsWith("//"))
                    {
                        link = "https://" + link.Substring(2); // "//" yerine "https://" ekle
                    }

                    if (!string.IsNullOrEmpty(imageUrl) && imageUrl.StartsWith("//"))
                    {
                        imageUrl = "https://" + imageUrl.Substring(2); // "//" yerine "https://" ekle
                    }
                  

                    return new
                    {
                        Title = post.Title.Text,
                        Link = link,
                        PubDate = post.PublishDate.DateTime,
                        Image = imageUrl
                    };
                }).ToList();

                return Ok(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("megabayt")]
        public async Task<IActionResult> PostMegabaytRssFeed([FromBody] NewsDetails newsDetails)
        {
            var url = newsDetails.url;
            try
            {
                using var reader = XmlReader.Create(url);
                var feed = SyndicationFeed.Load(reader);

                var posts = feed.Items.Select(post =>
                {
                    string imageUrl = null;
                
                    var uri = post.Links[2].Uri.ToString();

                    imageUrl = uri;
                

                    return new
                    {
                        Title = post.Title.Text,
                        Link = post.Links.FirstOrDefault()?.Uri.ToString(),
                        PubDate = post.PublishDate.DateTime,
                        Image = imageUrl
                    };
                }).ToList();

                return Ok(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
     
        public class NewsDetails
        {
            public string url { get; set; }
        }
    }
}