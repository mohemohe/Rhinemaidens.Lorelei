using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rhinemaidens
{
    public static class APIurl
    {
        public static readonly string requestTokenUrl = "https://api.twitter.com/oauth/request_token";
        public static readonly string authUrl = "https://api.twitter.com/oauth/authorize";
        public static readonly string accessTokenUrl = "https://api.twitter.com/oauth/access_token";
        public static readonly string userStreamUrl = "https://userstream.twitter.com/1.1/user.json";

        public static readonly string postTweetUrl = "https://api.twitter.com/1.1/statuses/update.json";
        public static readonly string postTweetWithImageUrl = "https://api.twitter.com/1.1/statuses/update_with_media.json";
    }
}
