using System;
using System.Collections.Generic;
using System.Threading;

namespace Rhinemaidens
{
    /// <summary>
    /// ツイートに関する情報を格納します
    /// </summary>
    public class TweetInfoPack
    {
        public string id { get; set; }

        public DateTime date { get; set; }

        public string userId { get; set; }

        public string screenName { get; set; }

        public string name { get; set; }

        public string iconUrl { get; set; }

        public string body { get; set; }

        public bool IsRetweet { get; set; }

        public string OriginId { get; set; }

        public DateTime OriginDate { get; set; }

        public string OriginUserId { get; set; }

        public string OriginScreenName { get; set; }

        public string OriginName { get; set; }

        public string OriginIconUrl { get; set; }

        public string OriginBody { get; set; }
    }

    internal static class TweetInfo
    {
        internal static Queue<TweetInfoPack> tweetInfoQueue = new Queue<TweetInfoPack>();

        internal static TweetInfoPack TryDequeueTweetInfoQueue()
        {
            try
            {
                return tweetInfoQueue.Dequeue();
            }
            catch 
            {
                Thread.Sleep(1);
                return null;
            }
        }
    }
}