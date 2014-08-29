using Rhinemaidens.OAuth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Rhinemaidens.TwitterAPI.REST
{
    internal class PostTweet
    {
        /// <summary>
        /// ツイートを投稿します
        /// </summary>
        /// <param name="Body">本文</param>
        internal void PostTweetTextOnly(string Body)
        {
            var oah = new OAuthHelper();

            if (Body.Length > 140)
            {
                throw new TooLongTweetBodyException();
            }

            try
            {
                var method = "POST";
                var headerString = oah.BuildHeaderString(HttpUtility.UrlEncode(APIurl.postTweetUrl), method, "", "status=" + Uri.EscapeDataString(Body));
                var sendBytes = Encoding.UTF8.GetBytes("status=" + Uri.EscapeDataString(Body));

                var req = (HttpWebRequest)WebRequest.Create(APIurl.postTweetUrl);
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
        internal void PostTweetWithImage(string Body, string ImageFilePath)
        {
            var oah = new OAuthHelper();

            if (Body.Length > 140)
            {
                throw new TooLongTweetBodyException();
            }

            var method = "POST";
            var headerString = oah.BuildHeaderString(HttpUtility.UrlEncode(APIurl.postTweetWithImageUrl), method, Body, "");

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

                var req = (HttpWebRequest)WebRequest.Create(APIurl.postTweetWithImageUrl);
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
    }
}
