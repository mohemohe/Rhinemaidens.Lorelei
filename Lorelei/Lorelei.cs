using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

using OAuth;
using System.Web.Security;

namespace Rhinemaidens
{
    public class Lorelei : ILorelei
    {
        private readonly string requestTokenUrl = "https://api.twitter.com/oauth/request_token";
        private readonly string authUrl = "https://api.twitter.com/oauth/authorize";
        private readonly string accessTokenUrl = "https://api.twitter.com/oauth/access_token";
        private readonly string userStreamUrl = "https://userstream.twitter.com/1.1/user.json";

        private readonly string postTweetUrl = "https://api.twitter.com/1.1/statuses/update.json";
        private readonly string postTweetWithImageUrl = "https://api.twitter.com/1.1/statuses/update_with_media.json";

        public string consumerKey { get; private set; }
        public string consumerSecret { get; private set; }
        public string accessToken { get; private set; }
        public string accessTokenSecret { get; private set; }

        private string requestToken { get; set; }
        private string requestTokenSecret { get; set; }

        private bool isStartedUserStream { get; set; }

        public Queue<TweetInfo> tweetInfoQueue = new Queue<TweetInfo>();

        /// <summary>
        /// ツイートに関する情報を格納します
        /// </summary>
        public class TweetInfo
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

        /// <summary>
        /// ユーザーに関する情報を格納します
        /// </summary>
        class UserInfo
        {
            public string id { get; set; }
            public string screenName { get; set; }
            //TODO: 
        }

        public enum ImageSize
        {
            Normal,
            Mini,
            Bigger,
            Original
        }

        public Lorelei() { }

        public Lorelei(string ConsumerKey, string ConsumerSecret)
        {
            this.consumerKey = ConsumerKey;
            this.consumerSecret = ConsumerSecret;
        }

        public Lorelei(string ConsumerKey, string ConsumerSecret, string AccessToken, string AccessTokenSecret)
        {
            this.consumerKey = ConsumerKey;
            this.consumerSecret = ConsumerSecret;
            this.accessToken = AccessToken;
            this.accessTokenSecret = AccessTokenSecret;
        }

        public void Initialize() { }

        public void Initialize(string ConsumerKey, string ConsumerSecret)
        {
            this.consumerKey = ConsumerKey;
            this.consumerSecret = ConsumerSecret;
        }

        public void Initialize(string ConsumerKey, string ConsumerSecret, string AccessToken, string AccessTokenSecret)
        {
            this.consumerKey = ConsumerKey;
            this.consumerSecret = ConsumerSecret;
            this.accessToken = AccessToken;
            this.accessTokenSecret = AccessTokenSecret;
        }

        /// <summary>
        /// OAuthに必要なヘッダを生成します
        /// </summary>
        /// <param name="EncodedUrl">エンコード済みURL</param>
        /// <param name="Method">GET, POST, etc...</param>
        /// <param name="ExtString1">URLの直後に必要な追加シグネチャ</param>
        /// <param name="ExtString2">oauth_versionの直後に必要な追加シグネチャ</param>
        /// <returns>ヘッダ文字列</returns>
        public string BuildHeaderString(string EncodedUrl, string Method, string ExtString1, string ExtString2)
        {
            var oauth = new OAuthBase();
            var timeStamp = oauth.GenerateTimeStamp();
            var nonce = oauth.GenerateNonce();

            var signatureBase = GenerateSignatureBase(EncodedUrl.Replace("%3a", "%3A").Replace("%2f", "%2F"), Method, timeStamp, nonce, ExtString1, ExtString2);

            var compositeKey = consumerSecret + "&" + accessTokenSecret;

            var signature = oauth.GenerateSignatureUsingHash(signatureBase, new HMACSHA1(Encoding.UTF8.GetBytes(compositeKey)));

            signature = HttpUtility.UrlEncode(signature);

            var HeaderString = "OAuth oauth_consumer_key=\"" + consumerKey + "\", oauth_nonce=\"" + nonce + "\", oauth_signature=\""
                + signature + "\", " + "oauth_signature_method=\"HMAC-SHA1\", oauth_timestamp=\"" + timeStamp + "\", oauth_token=\""
                + accessToken + "\", " + "oauth_version=\"1.0\"";

            return HeaderString;
        }

        private string GenerateSignatureBase(string EncodedUrl, string Method, string TimeStamp, string Nonce, string ExtString1, string ExtString2)
        {
            var signatureBase = "";
            var AND = Uri.EscapeDataString("&").ToString();
            var EQ = Uri.EscapeDataString("=").ToString();

            if (String.IsNullOrEmpty(Method) == true) 
            {
                signatureBase += EncodedUrl + "&";
            }
            else
            {
                signatureBase += Method + "&" + EncodedUrl + "&";
            }

            if (ExtString1 != "")
            {
                signatureBase += Uri.EscapeDataString(ExtString1) + AND;
            }

            signatureBase += "oauth_consumer_key" + EQ + consumerKey + AND + "oauth_nonce" + EQ + Nonce + AND
                + "oauth_signature_method" + EQ + "HMAC-SHA1" + AND + "oauth_timestamp" + EQ + TimeStamp + AND
                + "oauth_token" + EQ + accessToken + AND + "oauth_version" + EQ + "1.0";

            if (ExtString2 != "")
            {
                signatureBase += AND + Uri.EscapeDataString(ExtString2);
            }

            return signatureBase;
        }

        private bool GetRequestToken()
        {
            var oauth = new OAuthBase();
            var timeStamp = oauth.GenerateTimeStamp();
            var nonce = oauth.GenerateNonce();
            var method = "GET";

            string normalizedUrl, normalizedReqParams;

            var signature = oauth.GenerateSignature(new Uri(requestTokenUrl), consumerKey, consumerSecret, null, null
                , method, timeStamp, nonce, OAuthBase.SignatureTypes.HMACSHA1, out normalizedUrl, out normalizedReqParams);

            var tokenUrl = normalizedUrl + "?" + normalizedReqParams + "&oauth_signature=" + signature;

            var wc = new WebClient();
            string res;
            try
            {
                var st = wc.OpenRead(tokenUrl);
                var sr = new StreamReader(st, Encoding.GetEncoding("UTF-8"));
                res = sr.ReadToEnd();
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.Forbidden)
                    {
                        throw new UnauthorizedException();
                    }
                }

                throw new TwitterServerNotWorkingWellException();
            }
            catch
            {
                throw new TwitterServerNotWorkingWellException();
            }

            var re = new Regex("oauth_token=(?<token>.*)&oauth_token_secret=(?<tokenSecret>.*)&oauth_callback_confirmed");
            var m = re.Match(res);

            requestToken = m.Groups["token"].Value;
            requestTokenSecret = m.Groups["tokenSecret"].Value;

            return true;
        }

        /// <summary>
        /// OAuthでこの連携アプリへのアクセス許可を取得するためのURLを生成します
        /// </summary>
        /// <param name="OAuthUrl">認証URL</param>
        public void GetOAuthUrl(out string OAuthUrl)
        {
            if (GetRequestToken() == true)
            {
                OAuthUrl = authUrl + "?oauth_token=" + requestToken + "&oauth_token_secret=" + requestTokenSecret;
            }
            else
            {
                OAuthUrl = null;
            }
        }

        private bool _GetAccessToken(string pin)
        {
            var oauth = new OAuthBase();
            var timeStamp = oauth.GenerateTimeStamp();
            var nonce = oauth.GenerateNonce();
            var method = "GET";

            string normalizedUrl, normalizedReqParams;

            var signature = oauth.GenerateSignature(new Uri(requestTokenUrl), consumerKey, consumerSecret, requestToken, requestTokenSecret
                , method, timeStamp, nonce, OAuthBase.SignatureTypes.HMACSHA1, out normalizedUrl, out normalizedReqParams);

            var tokenUrl = accessTokenUrl + "?" + normalizedReqParams + "&oauth_signature=" + signature + "&oauth_verifier=" + pin;

            var wc = new WebClient();
            string res;
            try
            {
                var st = wc.OpenRead(tokenUrl);
                var sr = new StreamReader(st, Encoding.GetEncoding("UTF-8"));
                res = sr.ReadToEnd();
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.Unauthorized)
                    {
                        throw new UnauthorizedException();
                    }
                }

                throw new TwitterServerNotWorkingWellException();
            }
            catch
            {
                throw new TwitterServerNotWorkingWellException();
            }

            var re = new Regex("oauth_token=(?<token>.*)&oauth_token_secret=(?<tokenSecret>.*)&user_id");
            var m = re.Match(res);

            accessToken = m.Groups["token"].Value;
            accessTokenSecret = m.Groups["tokenSecret"].Value;

            return true;
        }

        /// <summary>
        /// ユーザが入力したPINからアクセストークンを取得します
        /// </summary>
        /// <param name="pin">PIN</param>
        /// <param name="AccessToken">AccessToken</param>
        /// <param name="AccessTokenSecret">AccessTokenSecret</param>
        public void GetAccessToken(string pin, out string AccessToken, out string AccessTokenSecret)
        {
            if (_GetAccessToken(pin) == true)
            {
                AccessToken = accessToken;
                AccessTokenSecret = accessTokenSecret;
            }
            else
            {
                AccessToken = null;
                AccessTokenSecret = null;
            }
        }

        /// <summary>
        /// ツイートを投稿します
        /// </summary>
        /// <param name="Body">本文</param>
        public void PostTweet(string Body)
        {
            if (Body.Length > 140)
            {
                throw new TooLongTweetBodyException();
            }

            try
            {
                var method = "POST";
                var headerString = BuildHeaderString(HttpUtility.UrlEncode(postTweetUrl), method, "", "status=" + Uri.EscapeDataString(Body));
                var sendBytes = Encoding.UTF8.GetBytes("status=" + Uri.EscapeDataString(Body));

                var req = (HttpWebRequest)WebRequest.Create(postTweetUrl);
                req.Method = method;
                req.Headers.Add(HttpRequestHeader.Authorization, headerString);
                req.ContentType = "application/x-www-form-urlencoded";
                req.ContentLength = sendBytes.Length;
                req.ServicePoint.Expect100Continue = false;

                var reqStream = req.GetRequestStream();
                reqStream.Write(sendBytes, 0, sendBytes.Length);
                reqStream.Close();

                var res = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.Unauthorized)
                    {
                        throw new UnauthorizedException();
                    }

                    if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.Forbidden)
                    {
                        throw new DuplicateTweetBodyException();
                    }
                }

                throw new TwitterServerNotWorkingWellException();
            }
            catch
            {
                throw new TwitterServerNotWorkingWellException();
            }
        }

        /// <summary>
        /// 画像付きツイートを投稿します
        /// </summary>
        /// <param name="Body">本文</param>
        /// <param name="ImageFilePath">画像のパス</param>
        public void PostTweetWithImage(string Body, string ImageFilePath)
        {
            if (Body.Length > 140)
            {
                throw new TooLongTweetBodyException();
            }

            var method = "POST";
            var headerString = BuildHeaderString(HttpUtility.UrlEncode(postTweetWithImageUrl), method, Body, "");

            var randStr = DateTime.Now.Ticks.ToString("x");
            var boundary = "--" + randStr;

            var openBytes = Encoding.UTF8.GetBytes(
                boundary + "\r\n" +
                "Content-Type: application/x-www-form-urlencoded\r\n" +
                "Content-Disposition: form-data; name=\"status\"\r\n" +
                "\r\n" +
                Body + "\r\n" +
                boundary + "\r\n" +
                "Content-Type: application/octet-stream\r\n" +
                "Content-Disposition: form-data; name=\"media[]\"; filename=\"" + Path.GetFileName(ImageFilePath) + "\"\r\n" +
                "\r\n"
                );
            var closeBytes = Encoding.UTF8.GetBytes(
                "\r\n" +
                boundary + "--"
                );

            try
            {
                var fs = new FileStream(ImageFilePath, FileMode.Open, FileAccess.Read);

                var req = (HttpWebRequest)WebRequest.Create(postTweetWithImageUrl);
                req.Method = method;
                req.Headers.Add(HttpRequestHeader.Authorization, headerString);
                req.ContentType = "multipart/form-data; boundary=" + randStr;
                req.ContentLength = openBytes.Length + closeBytes.Length + fs.Length;
                req.Host = "api.twitter.com";
                req.KeepAlive = true;
                req.ServicePoint.Expect100Continue = false;

                var reqStream = req.GetRequestStream();
                reqStream.Write(openBytes, 0, openBytes.Length);

                var readData = new byte[0x1000];
                var readSize = 0;
                while (true)
                {
                    readSize = fs.Read(readData, 0, readData.Length);
                    if (readSize == 0)
                    {
                        break;
                    }
                    reqStream.Write(readData, 0, readSize);
                }

                reqStream.Write(closeBytes, 0, closeBytes.Length);
                reqStream.Close();
                fs.Close();

                HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.Unauthorized)
                    {
                        throw new UnauthorizedException();
                    }

                    if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.Forbidden)
                    {
                        throw new DuplicateTweetBodyException();
                    }
                }

                throw new TwitterServerNotWorkingWellException();
            }
            catch
            {
                throw new TwitterServerNotWorkingWellException();
            }
        }


        public void GetUserInfo(long userId)
        {
            //TODO: 
        }

        /// <summary>
        /// 画像を取得します
        /// </summary>
        /// <param name="ImageUrl">URL</param>
        /// <param name="Size">取得するサイズ</param>
        /// <param name="Image">出力先Bitmap</param>
        public void GetImage(string ImageUrl, ImageSize Size, out Bitmap Image)
        {
            string url;

            if (Size == ImageSize.Bigger)
            {
                url = ImageUrl.Replace("_normal", "_bigger");
            }
            else if (Size == ImageSize.Mini)
            {
                url = ImageUrl.Replace("_normal", "_mini");
            }
            else if (Size == ImageSize.Original)
            {
                url = ImageUrl.Replace("_normal", "");
            }
            else
            {
                url = ImageUrl;
            }

            var wc = new WebClient();
            byte[] data;
            try
            {
                data = wc.DownloadData(url);
            }
            catch
            {
                throw new TwitterServerNotWorkingWellException();
            }
            var st = new MemoryStream(data);

            Image = new Bitmap(st);
        }

        /// <summary>
        /// UserStreamに接続します
        /// </summary>
        /// <param name="IsGetAllReplies">フォロー外のリプライも取得する</param>
        public async void ConnectUserStream(bool IsGetAllReplies)
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
        public void DisconnectUserStream()
        {
            isStartedUserStream = false;
        }

        private void GetUserStream(bool IsGetAllReplies)
        {
            while (isStartedUserStream)
            {
                WebResponse res = null;
                int i = 0;

                var method = "GET";
                string headerString;
                string url;
                if (IsGetAllReplies)
                {
                    url = userStreamUrl + "?replies=all";
                    headerString = BuildHeaderString(HttpUtility.UrlEncode(userStreamUrl), method, "", "replies=all");
                }
                else
                {
                    url = userStreamUrl;
                    headerString = BuildHeaderString(HttpUtility.UrlEncode(userStreamUrl), method, "", "");
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

        private void ParseJsonOfUserStream(string Text)
        {
            var jsonRoot = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(Text);
            if (jsonRoot.ContainsKey("user") && jsonRoot.ContainsKey("text"))
            {
                var ti = new TweetInfo();

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

                tweetInfoQueue.Enqueue(ti);
            }
        }

        /// <summary>
        /// 画像をリサイズします
        /// </summary>
        /// <param name="SourceImage">リサイズ元の画像</param>
        /// <param name="Width">幅</param>
        /// <param name="Height">高さ</param>
        /// <param name="ResizedImage">リサイズ後の画像</param>
        public void ResizeImage(int Width, int Height, Bitmap SourceImage, out Bitmap ResizedImage)
        {
            double zoom;

            if ((double)Width / (double)Height <= (double)SourceImage.Width / (double)SourceImage.Height)
            {
                zoom = (double)Width / (double)SourceImage.Width;
            }
            else
            {
                zoom = (double)Height / (double)SourceImage.Height;
            }

            ResizedImage = new Bitmap((int)(SourceImage.Width * zoom), (int)(SourceImage.Height * zoom));
            using (var g = Graphics.FromImage(ResizedImage))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                g.DrawImage(SourceImage, 0, 0, ResizedImage.Width, ResizedImage.Height);
            }
        }

        /// <summary>
        /// リツイート用の画像を生成します
        /// </summary>
        /// <param name="SourceImage">リツイートしたアカウントのアイコン</param>
        /// <param name="SourceImageWidth">幅</param>
        /// <param name="SourceImageHeight">高さ</param>
        /// <param name="SourceOriginImage">リツイートされたアカウントのアイコン</param>
        /// <param name="SourceOriginImageWidth">幅</param>
        /// <param name="SourceOriginImageHeight">高さ</param>
        /// <param name="GeneratedImage"></param>
        public void GenerateRetweeterImage(int Width, int Height, Bitmap SourceOriginImage, int SourceOriginImageWidth, int SourceOriginImageHeight, Bitmap SourceRetweeterImage, int SourceRetweeterImageWidth, int SourceRetweeterImageHeight, out Bitmap GeneratedImage)
        {
            Bitmap tmpSourceOriginImage, tmpSourceRetweeterImage;

            ResizeImage(SourceOriginImageWidth, SourceOriginImageHeight, SourceOriginImage, out tmpSourceOriginImage);
            ResizeImage(SourceRetweeterImageWidth, SourceRetweeterImageHeight, SourceRetweeterImage, out tmpSourceRetweeterImage);
            
            GeneratedImage = new Bitmap(Width, Height);
            using (var g = Graphics.FromImage(GeneratedImage))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                g.DrawImage(tmpSourceOriginImage, 0, 0, tmpSourceOriginImage.Width, tmpSourceOriginImage.Height);
                g.DrawImage(tmpSourceRetweeterImage, GeneratedImage.Width - tmpSourceRetweeterImage.Width, GeneratedImage.Height - tmpSourceRetweeterImage.Height, tmpSourceRetweeterImage.Width, tmpSourceRetweeterImage.Height);
            }
        }
    }
}
