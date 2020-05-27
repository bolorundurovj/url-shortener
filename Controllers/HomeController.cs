using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.Linq;
using MongoDB.Driver;
using  shortid;
using url_shortener.Models;

namespace url_shortener.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMongoDatabase mongoDatabase;
        private const string ServiceUrl = "Request.Url.Host";
        
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            
            var connectionString = "mongodb://admin:admin1234@ds048537.mlab.com:48537/url-shortener?retryWrites=false";
            var mongoClient = new MongoClient(connectionString);
            mongoDatabase = mongoClient.GetDatabase("url-shortener");
        }
        
        [HttpGet]
        public async Task<IActionResult> Index(string u)
        {
            // get shortened url collection
            var shortenedUrlCollection = mongoDatabase.GetCollection<ShortenedUrl>("shortened-urls");
            // first check if we have the short code
            var shortenedUrl = await shortenedUrlCollection
                .AsQueryable()
                .FirstOrDefaultAsync(x => x.ShortCode == u);

            // if the short code does not exist, send back to home page
            if (shortenedUrl == null)
            {
                return View();
            }

            return Redirect(shortenedUrl.OriginalUrl);
        }

        public IActionResult Privacy()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> ShortenUrl(string longUrl)
        {
            // get shortened url collection
            var shortenedUrlCollection = mongoDatabase.GetCollection<ShortenedUrl>("shortened-urls");
            // first check if we have the url stored
            var shortenedUrl = await shortenedUrlCollection
                .AsQueryable()
                .FirstOrDefaultAsync(x => x.OriginalUrl == longUrl);

            // if the long url has not been shortened
            if (shortenedUrl == null)
            {
                var shortCode = ShortId.Generate(length: 8);
                shortenedUrl = new ShortenedUrl
                {
                    CreatedAt = DateTime.UtcNow,
                    OriginalUrl = longUrl,
                    ShortCode = shortCode,
                    ShortUrl = $"{ServiceUrl}/{shortCode}"
                };
                // add to database
                await shortenedUrlCollection.InsertOneAsync(shortenedUrl);
            }

            return View(shortenedUrl);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
