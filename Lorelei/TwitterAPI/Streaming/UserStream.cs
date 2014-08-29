using Rhinemaidens.OAuth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace Rhinemaidens.TwitterAPI.Streaming
{
    internal static class UserStream
    {
        internal static bool isStartedUserStream { get; set; }

        /// <summary>
        /// UserStreamに接続します
        /// </summary>
        /// <param name="IsGetAllReplies">フォロー外のリプライも取得する</param>
        internal static async void ConnectUserStream(bool IsGetAllReplies)
        {
            isStartedUserStream = true;
            try
            {
                await Task.Run(() => GetUserStream(IsGetAllReplies));
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// UserStreamを切断します
        /// </summary>
        internal static void DisconnectUserStream()
        {
            isStartedUserStream = false;
        }

        private static void GetUserStream(bool IsGetAllReplies)
        {
            var oah = new OAuthHelper();

            while (isStartedUserStream)
            {
                WebResponse res = null;
                int i = 0;

                var method = "GET";
                string headerString;
                string url;
                if (IsGetAllReplies)
                {
                    url = APIurl.userStreamUrl + "?replies=all";
                    headerString = oah.BuildHeaderString(HttpUtility.UrlEncode(APIurl.userStreamUrl), method, "", "replies=all");
                }
                else
                {
                    url = APIurl.userStreamUrl;
                    headerString = oah.BuildHeaderString(HttpUtility.UrlEncode(APIurl.userStreamUrl), method, "", "");
                }

                do
                {
                    var req = (HttpWebRequest)HttpWebRequest.Create(url);
                    req.Method = method;
                    req.Headers.Add(HttpRequestHeader.Authorization, headerString);
                    req.Timeout = Timeout.Infinite;
                    req.ServicePoint.Expect100Continue = false;

                    try
                    {
                        res = req.GetResponse();
                        i = 0;
                    }
                    catch (WebException e)
                    {
                        if ((int)((HttpWebResponse)e.Response).StatusCode == 420)
                        {
                            var sleepTime = 5 * 1000 * Math.Pow(2, i);

                            if (sleepTime > 300 * 100 * 1000)
                            {
                                break;

                            }

                            Thread.Sleep((int)sleepTime);
                            i++;
                        }
                        else
                        {
                            var sleepTime = 5 * 1000 * Math.Pow(2, i);

                            if (sleepTime > 320 * 1000)
                            {
                                break;

                            }

                            Thread.Sleep((int)sleepTime);
                            i++;
                        }
                    }
                    catch
                    {
                        var sleepTime = 250 * Math.Pow(2, i);

                        if (sleepTime > 16 * 1000)
                        {
                            break;

                        }

                        Thread.Sleep((int)sleepTime);
                        i++;
                    }

                } while (res == null);
                var sr = new StreamReader(res.GetResponseStream());

                while (isStartedUserStream == true)
                {
                    try
                    {
                        string Text = sr.ReadLine();
                        if (Text != null && Text.Length > 0)
                        {
                            ParseJsonOfUserStream(Text);
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }
                    catch
                    {
                        break;
                    }
                }
                try
                {
                    res.Close();
                }
                catch { }
            }

            if (isStartedUserStream)
            {
                throw new DeadOrDisconnectedUserStreamException();
            }

        }

        private static void ParseJsonOfUserStream(string Text)
        {
            var jsonRoot = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(Text);
            if (jsonRoot.ContainsKey("user") && jsonRoot.ContainsKey("text"))
            {
                var ti = new TweetInfoPack();

                object tweetUserObj;
                object tweetIdObj;
                object tweetUserIdObj;
                object tweetNameObj;
                object tweetScreenNameObj;
                object tweetIconUrlObj;
                object tweetBodyObj;

                var tweetUserSB = new StringBuilder();
                jsonRoot.TryGetValue("user", out tweetUserObj);
                var jrs = new JavaScriptSerializer();
                jrs.Serialize(tweetUserObj, tweetUserSB);
                var JsonUser = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(tweetUserSB.ToString(0, tweetUserSB.Length));

                jsonRoot.TryGetValue("id", out tweetIdObj);
                JsonUser.TryGetValue("id", out tweetUserIdObj);
                JsonUser.TryGetValue("name", out tweetNameObj);
                JsonUser.TryGetValue("screen_name", out tweetScreenNameObj);
                JsonUser.TryGetValue("profile_image_url_https", out tweetIconUrlObj);
                jsonRoot.TryGetValue("text", out tweetBodyObj);

                ti.id = tweetIdObj.ToString();
                ti.userId = tweetUserIdObj.ToString();
                ti.screenName = tweetScreenNameObj.ToString();
                ti.name = tweetNameObj.ToString();
                ti.iconUrl = tweetIconUrlObj.ToString();
                ti.body = tweetBodyObj.ToString();

                object tweetRetweetedStatus;

                jsonRoot.TryGetValue("retweeted_status", out tweetRetweetedStatus);
                if (tweetRetweetedStatus != null)
                {
                    object retweetOriginUserObj;
                    object retweetOriginIdObj;
                    object retweetOriginUserIdObj;
                    object retweetOriginNameObj;
                    object retweetOriginScreenNameObj;
                    object retweetOriginIconUrlObj;
                    object retweetOriginBodyObj;

                    var tweetRetweetedStatusSB = new StringBuilder();
                    jrs.Serialize(tweetRetweetedStatus, tweetRetweetedStatusSB);
                    var JsonRetweet = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(tweetRetweetedStatusSB.ToString(0, tweetRetweetedStatusSB.Length));
                    JsonRetweet.TryGetValue("user", out retweetOriginUserObj);
                    var tweetRetweetedOriginUserSB = new StringBuilder();
                    jrs.Serialize(retweetOriginUserObj, tweetRetweetedOriginUserSB);
                    var JsonRetweetOriginUser = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(tweetRetweetedOriginUserSB.ToString(0, tweetRetweetedOriginUserSB.Length));

                    JsonRetweet.TryGetValue("id", out retweetOriginIdObj);
                    JsonRetweetOriginUser.TryGetValue("id", out retweetOriginUserIdObj);
                    JsonRetweetOriginUser.TryGetValue("name", out retweetOriginNameObj);
                    JsonRetweetOriginUser.TryGetValue("screen_name", out retweetOriginScreenNameObj);
                    JsonRetweetOriginUser.TryGetValue("profile_image_url_https", out retweetOriginIconUrlObj);
                    JsonRetweet.TryGetValue("text", out retweetOriginBodyObj);

                    ti.IsRetweet = true;
                    ti.OriginId = retweetOriginIdObj.ToString();
                    ti.OriginUserId = retweetOriginUserIdObj.ToString();
                    ti.OriginName = retweetOriginNameObj.ToString();
                    ti.OriginScreenName = retweetOriginScreenNameObj.ToString();
                    ti.OriginIconUrl = retweetOriginIconUrlObj.ToString();
                    ti.OriginBody = retweetOriginBodyObj.ToString();
                }
                else
                {
                    ti.IsRetweet = false;
                }

                TweetInfo.tweetInfoQueue.Enqueue(ti);
            }
        }
    }
}
