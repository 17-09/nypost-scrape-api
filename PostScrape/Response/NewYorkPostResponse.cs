using System;

namespace PostScrape.Response
{
    public class NewYorkPostResponse
    {
        public DateTimeOffset? PublishedTime { get; set; }
        public string Url { get; set; }
        public string Content { get; set; }
    }
}