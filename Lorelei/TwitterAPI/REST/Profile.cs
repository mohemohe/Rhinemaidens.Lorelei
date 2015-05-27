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
    class Profile
    {
        /// <summary>
        /// 名前を変更します
        /// </summary>
        /// <param name="Name">名前</param>
        internal void UpdateName(string Name)
        {
            var oah = new OAuthHelper();

            try
            {
                var method = "POST";
                var headerString = oah.BuildHeaderString(HttpUtility.UrlEncode(APIurl.updateProfileUrl), method, "name=" + Name, "");
                var sendBytes = Encoding.UTF8.GetBytes("name=" + Name);

                var req = (HttpWebRequest)WebRequest.Create(APIurl.updateProfileUrl);
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
        /// 自己紹介を変更します
        /// </summary>
        /// <param name="Description">自己紹介</param>
        internal void UpdateDescription(string Description)
        {
            var oah = new OAuthHelper();

            try
            {
                var method = "POST";
                var headerString = oah.BuildHeaderString(HttpUtility.UrlEncode(APIurl.updateProfileUrl), method, "description=" + Description, "");
                var sendBytes = Encoding.UTF8.GetBytes("description=" + Description);

                var req = (HttpWebRequest)WebRequest.Create(APIurl.updateProfileUrl);
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
        /// URLを変更します
        /// </summary>
        /// <param name="Url">URL</param>
        internal void UpdateUrl(string Url)
        {
            var oah = new OAuthHelper();

            try
            {
                var method = "POST";
                var headerString = oah.BuildHeaderString(HttpUtility.UrlEncode(APIurl.updateProfileUrl), method, "url=" + Url, "");
                var sendBytes = Encoding.UTF8.GetBytes("");

                var req = (HttpWebRequest)WebRequest.Create(APIurl.updateProfileUrl + "?url=" + HttpUtility.UrlEncode(Url));
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
        /// 現在地を変更します
        /// </summary>
        /// <param name="Location">現在地</param>
        internal void UpdateLocation(string Location)
        {
            var oah = new OAuthHelper();

            try
            {
                var method = "POST";
                var headerString = oah.BuildHeaderString(HttpUtility.UrlEncode(APIurl.updateProfileUrl), method, "location=" + Location, "");
                var sendBytes = Encoding.UTF8.GetBytes("location=" + Location);

                var req = (HttpWebRequest)WebRequest.Create(APIurl.updateProfileUrl);
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
        /// アイコンを変更します
        /// </summary>
        /// <param name="ImageFilePath">画像のパス</param>
        internal void UpdateImage(string ImageFilePath)
        {
            var oah = new OAuthHelper();

            var method = "POST";
            var headerString = oah.BuildHeaderString(HttpUtility.UrlEncode(APIurl.updateProfileImageUrl), method, "", "");

            var randStr = DateTime.Now.Ticks.ToString("x");
            var boundary = "--" + randStr;

            var openBytes = Encoding.UTF8.GetBytes(
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
