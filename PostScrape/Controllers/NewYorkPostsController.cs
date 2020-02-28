using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PostScrape.Response;

namespace PostScrape.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewYorkPostsController : ControllerBase
    {
        private static readonly MemoryCacheEntryOptions _memoryCacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = DateTime.UtcNow.AddDays(5)
        };

        private readonly ILogger<NewYorkPostsController> _logger;
        private readonly IMemoryCache _memoryCache;

        public NewYorkPostsController(ILogger<NewYorkPostsController> logger, IMemoryCache memoryCache)
        {
            _logger = logger;
            _memoryCache = memoryCache;
        }

        [HttpGet]
        [Route("{utcPublishedDateTime}")]
        public async Task<IActionResult> GetPostContentAsync([FromRoute] DateTimeOffset utcPublishedDateTime,
            [FromQuery] bool usingCache = true)
        {
            try
            {
                NewYorkPostResponse response;

                if (usingCache)
                {
                    var key = utcPublishedDateTime.ToUnixTimeSeconds();
                    response = await
                        _memoryCache.GetOrCreateAsync(key,
                            _ => NewYorkScape.GetContentAsync(utcPublishedDateTime));
                }
                else
                {
                    response = await NewYorkScape.GetContentAsync(utcPublishedDateTime);
                }

                return Ok(response);
            }
            catch (Exception)
            {
                return Ok(
                    $"Can not find any post content with this published time : {utcPublishedDateTime.ToString()}");
            }
        }
    }
}