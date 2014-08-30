using Rhinemaidens.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Rhinemaidens.TwitterAPI.REST
{
    internal class Favorite
    {
        internal void AddFavorite(string TweetId)
        {
            var oah = new OAuthHelper();

            try
            {
                var method = "POST";
                var headerString = oah.BuildHeaderString(HttpUtility.UrlEncode(APIurl.createFavoriteUrl), method, "id=" + TweetId, "");
                var sendBytes = Encoding.UTF8.GetBytes("id=" + TweetId);

                var req = (HttpWebRequest)WebRequest.Create(APIurl.createFavoriteUrl);
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

        internal void DeleteFavorite(string TweetId)
        {
            var oah = new OAuthHelper();

            try
            {
                var method = "POST";
                var headerString = oah.BuildHeaderString(HttpUtility.UrlEncode(APIurl.destroyFavoriteUrl), method, "id=" + TweetId, "");
                var sendBytes = Encoding.UTF8.GetBytes("id=" + TweetId);

                var req = (HttpWebRequest)WebRequest.Create(APIurl.destroyFavoriteUrl);
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
    }
}
