using System;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Caching.Memory;

namespace PostScrape
{
    public static class BackgroundWorker
    {
        private static readonly MemoryCacheEntryOptions cacheExpirationOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = DateTimeOffset.Now.AddDays(7)
        };

        public static async Task FetchPostsEveryDay(PerformContext context, IJobCancellationToken token)
        {
            var memoryCache = JobActivator.Current.BeginScope(context).Resolve(typeof(IMemoryCache)) as IMemoryCache;
            var archives = await NewYorkScape.GetArchivesInThisDayAsync(DateTimeOffset.UtcNow);

            foreach (var post in archives)
            {
                memoryCache.Set(post.PublishedTime.Value.ToUnixTimeSeconds(), post, cacheExpirationOptions);
            }
        }
    }
}