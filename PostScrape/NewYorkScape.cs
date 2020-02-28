using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using PostScrape.Extensions;
using PostScrape.Response;

namespace PostScrape
{
    public static class NewYorkScape
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        private static readonly DateTimeFormatInfo _formatInfo = new DateTimeFormatInfo
            {AMDesignator = "am", PMDesignator = "pm"};

        private const string _host = "https://nypost.com";
        private const string _zoneId = "America/New_York";

        public static async Task<List<NewYorkPostResponse>> GetArchivesInThisDayAsync(DateTimeOffset dateTimeOffset)
        {
            var day = dateTimeOffset.ToString("yyyy/MM/dd");

            var archiveContent =
                await (await _httpClient.GetAsync($"{_host}/{day}"))
                    .Content
                    .ReadAsStringAsync();

            var document = new HtmlDocument();
            document.LoadHtml(archiveContent);

            var xPathToPostLink =
                $"//div[@class='entry-meta']/ancestor::li/div[@class='entry-thumbnail']/a";
            var hrefs = document.DocumentNode.SelectNodes(xPathToPostLink)
                .Select(n => n.Attributes["href"].Value)
                .ToArray();

            var results = new List<NewYorkPostResponse>();
            foreach (var href in hrefs)
            {
                var content = await GetContentAsync(href);
                results.Add(content);
            }

            return results;
        }

        public static async Task<NewYorkPostResponse> GetContentAsync(DateTimeOffset utcPublishedDateTime)
        {
            var url = await GetUrlAsync(utcPublishedDateTime);
            return await GetContentAsync(url);
        }

        private static async Task<string> GetUrlAsync(DateTimeOffset utcPublishedDateTime)
        {
            var newYorkDate = utcPublishedDateTime.ToZone(_zoneId);

            var dateOfPost = newYorkDate.ToString("yyyy/MM/dd");
            var dateTimeOfPost = newYorkDate.ToString("MMMM d, yyyy | h:mmtt", _formatInfo);

            var archiveContent =
                await (await _httpClient.GetAsync($"{_host}/{dateOfPost}"))
                    .Content
                    .ReadAsStringAsync();

            var document = new HtmlDocument();
            document.LoadHtml(archiveContent);

            var xPathToPostLink =
                $"//div[@class='entry-meta']/p[contains(text(),'{dateTimeOfPost}')]/ancestor::li/div[@class='entry-thumbnail']/a";
            var href = document.DocumentNode.SelectNodes(xPathToPostLink)
                .First()
                .Attributes["href"]
                .Value;

            return href;
        }

        private static async Task<NewYorkPostResponse> GetContentAsync(string url)
        {
            var content = await (await _httpClient.GetAsync(url))
                .Content
                .ReadAsStringAsync();

            var document = new HtmlDocument();
            document.LoadHtml(content);

            var xPathToMeta =
                $"//meta[@property='article:published_time']";

            var publishedTimeAsString = document.DocumentNode.SelectNodes(xPathToMeta)
                .First()
                .Attributes["content"]
                .Value;

            var publishedTime = DateTimeOffset.Parse(publishedTimeAsString);

            var xPathToTitle = "//div[@class='article-header']/h1";
            var title = document.DocumentNode.SelectNodes(xPathToTitle)
                .First()
                .InnerText
                .Trim();

            return new NewYorkPostResponse
            {
                Content = title,
                Url = url,
                PublishedTime = publishedTime
            };
        }
    }
}