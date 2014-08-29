using OAuth;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Rhinemaidens.OAuth
{
    internal class OAuthHelper
    {
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

            var compositeKey = OAuthData.consumerSecret + "&" + OAuthData.accessTokenSecret;

            var signature = oauth.GenerateSignatureUsingHash(signatureBase, new HMACSHA1(Encoding.UTF8.GetBytes(compositeKey)));

            signature = HttpUtility.UrlEncode(signature);

            var HeaderString = "OAuth oauth_consumer_key=\"" + OAuthData.consumerKey + "\", oauth_nonce=\"" + nonce + "\", oauth_signature=\""
                + signature + "\", " + "oauth_signature_method=\"HMAC-SHA1\", oauth_timestamp=\"" + timeStamp + "\", oauth_token=\""
                + OAuthData.accessToken + "\", " + "oauth_version=\"1.0\"";

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

            signatureBase += "oauth_consumer_key" + EQ + OAuthData.consumerKey + AND + "oauth_nonce" + EQ + Nonce + AND
                + "oauth_signature_method" + EQ + "HMAC-SHA1" + AND + "oauth_timestamp" + EQ + TimeStamp + AND
                + "oauth_token" + EQ + OAuthData.accessToken + AND + "oauth_version" + EQ + "1.0";

            if (ExtString2 != "")
            {
                signatureBase += AND + Uri.EscapeDataString(ExtString2);
            }

            return signatureBase;
        }

        internal bool GetRequestToken()
        {
            var oauth = new OAuthBase();
            var timeStamp = oauth.GenerateTimeStamp();
            var nonce = oauth.GenerateNonce();
            var method = "GET";

            string normalizedUrl, normalizedReqParams;

            var signature = oauth.GenerateSignature(new Uri(APIurl.requestTokenUrl), OAuthData.consumerKey, OAuthData.consumerSecret, null, null
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

            OAuthData.requestToken = m.Groups["token"].Value;
            OAuthData.requestTokenSecret = m.Groups["tokenSecret"].Value;

            return true;
        }

        /// <summary>
        /// OAuthでこの連携アプリへのアクセス許可を取得するためのURLを生成します
        /// </summary>
        /// <param name="OAuthUrl">認証URL</param>
        internal void GetOAuthUrl(out string OAuthUrl)
        {
            if (GetRequestToken() == true)
            {
                OAuthUrl = APIurl.authUrl + "?oauth_token=" + OAuthData.requestToken + "&oauth_token_secret=" + OAuthData.requestTokenSecret;
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

            var signature = oauth.GenerateSignature(new Uri(APIurl.requestTokenUrl), OAuthData.consumerKey, OAuthData.consumerSecret, OAuthData.requestToken, OAuthData.requestTokenSecret
                , method, timeStamp, nonce, OAuthBase.SignatureTypes.HMACSHA1, out normalizedUrl, out normalizedReqParams);

            var tokenUrl = APIurl.accessTokenUrl + "?" + normalizedReqParams + "&oauth_signature=" + signature + "&oauth_verifier=" + pin;

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

            OAuthData.accessToken = m.Groups["token"].Value;
            OAuthData.accessTokenSecret = m.Groups["tokenSecret"].Value;

            return true;
        }

        /// <summary>
        /// ユーザが入力したPINからアクセストークンを取得します
        /// </summary>
        /// <param name="pin">PIN</param>
        /// <param name="AccessToken">AccessToken</param>
        /// <param name="AccessTokenSecret">AccessTokenSecret</param>
        internal void GetAccessToken(string pin, out string AccessToken, out string AccessTokenSecret)
        {
            if (_GetAccessToken(pin) == true)
            {
                AccessToken = OAuthData.accessToken;
                AccessTokenSecret = OAuthData.accessTokenSecret;
            }
            else
            {
                AccessToken = null;
                AccessTokenSecret = null;
            }
        }
    }
}