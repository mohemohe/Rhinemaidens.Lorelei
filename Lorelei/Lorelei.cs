using Rhinemaidens.Helper;
using Rhinemaidens.OAuth;
using Rhinemaidens.TwitterAPI.REST;
using Rhinemaidens.TwitterAPI.Streaming;
using System.Drawing;

namespace Rhinemaidens
{
    public class Lorelei : ILorelei
    {
        #region Property

        public string consumerKey
        {
            get
            {
                return OAuthData.consumerKey;
            }

            internal set
            {
                OAuthData.consumerKey = value;
            }
        }

        public string consumerSecret
        {
            get
            {
                return OAuthData.consumerSecret;
            }

            internal set
            {
                OAuthData.consumerSecret = value;
            }
        }

        public string accessToken
        {
            get
            {
                return OAuthData.accessToken;
            }

            internal set
            {
                OAuthData.accessToken = value;
            }
        }

        public string accessTokenSecret
        {
            get
            {
                return OAuthData.accessTokenSecret;
            }

            internal set
            {
                OAuthData.accessTokenSecret = value;
            }
        }

        public string requestToken
        {
            get
            {
                return OAuthData.requestToken;
            }

            internal set
            {
                OAuthData.requestToken = value;
            }
        }

        public string requestTokenSecret
        {
            get
            {
                return OAuthData.requestTokenSecret;
            }

            internal set
            {
                OAuthData.requestTokenSecret = value;
            }
        }

        #endregion Property

        public enum ImageSize
        {
            Normal,
            Mini,
            Bigger,
            Original
        }

        public Lorelei()
        {
            Initialize();
        }

        public Lorelei(string ConsumerKey, string ConsumerSecret)
        {
            Initialize(ConsumerKey, ConsumerSecret);
        }

        public Lorelei(string ConsumerKey, string ConsumerSecret, string AccessToken, string AccessTokenSecret)
        {
            Initialize(ConsumerKey, ConsumerSecret, AccessToken, AccessTokenSecret);
        }

        /// <summary>
        /// Loreleiを初期化します
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Loreleiを初期化します
        /// </summary>
        /// <param name="ConsumerKey">Consumer key (API key)</param>
        /// <param name="ConsumerSecret">Consumer secret (API secret)</param>
        public void Initialize(string ConsumerKey, string ConsumerSecret)
        {
            this.consumerKey = ConsumerKey;
            this.consumerSecret = ConsumerSecret;
        }

        /// <summary>
        /// Loreleiを初期化します
        /// </summary>
        /// <param name="ConsumerKey">Consumer key (API key)</param>
        /// <param name="ConsumerSecret">Consumer secret (API secret)</param>
        /// <param name="AccessToken">Access token</param>
        /// <param name="AccessTokenSecret">Access token secret</param>
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
            var oah = new OAuthHelper();
            return oah.BuildHeaderString(EncodedUrl, Method, ExtString1, ExtString2);
        }

        /// <summary>
        /// OAuthでこの連携アプリへのアクセス許可を取得するためのURLを生成します
        /// </summary>
        /// <param name="OAuthUrl">認証URL</param>
        public void GetOAuthUrl(out string OAuthUrl)
        {
            var oah = new OAuthHelper();
            oah.GetOAuthUrl(out OAuthUrl);
        }

        /// <summary>
        /// ユーザが入力したPINからアクセストークンを取得します
        /// </summary>
        /// <param name="pin">PIN</param>
        /// <param name="AccessToken">AccessToken</param>
        /// <param name="AccessTokenSecret">AccessTokenSecret</param>
        public void GetAccessToken(string pin, out string AccessToken, out string AccessTokenSecret)
        {
            var oah = new OAuthHelper();
            oah.GetAccessToken(pin, out AccessToken, out AccessTokenSecret);
        }

        /// <summary>
        /// ツイートを投稿します
        /// </summary>
        /// <param name="Body">本文</param>
        public void PostTweet(string Body)
        {
            var pt = new PostTweet();

            try
            {
                pt.PostTweetTextOnly(Body);
            }
            catch { throw; }
        }

        /// <summary>
        /// ツイートを返信します
        /// </summary>
        /// <param name="Body">本文</param>
        public void PostTweet(string Body, string In_reply_to_status_id)
        {
            var pt = new PostTweet();

            try
            {
                pt.PostTweetTextOnly(Body, In_reply_to_status_id);
            }
            catch { throw; }
        }

        /// <summary>
        /// 画像付きツイートを投稿します
        /// </summary>
        /// <param name="Body">本文</param>
        /// <param name="ImageFilePath">画像のパス</param>
        public void PostTweetWithImage(string Body, string ImageFilePath)
        {
            var pt = new PostTweet();

            try
            {
                pt.PostTweetWithImage(Body, ImageFilePath);
            }
            catch { throw; }
        }

        /// <summary>
        /// ふぁぼります
        /// </summary>
        /// <param name="TweetId">ふぁぼるツイートのID</param>
        public void AddFavorite(string TweetId)
        {
            var f = new Favorite();

            try
            {
                f.AddFavorite(TweetId);
            }
            catch { throw; }
        }

        /// <summary>
        /// あんふぁぼします
        /// </summary>
        /// <param name="TweetId">あんふぁぼするツイートのID</param>
        public void DeleteFavorite(string TweetId)
        {
            var f = new Favorite();

            try
            {
                f.DeleteFavorite(TweetId);
            }
            catch { throw; }
        }

        /// <summary>
        /// 公式RTします
        /// </summary>
        /// <param name="TweetId">公式RTするツイートのID</param>
        public void PostRetweet(string TweetId)
        {
            var r = new Retweet();

            try
            {
                r.PostRetweet(TweetId);
            }
            catch { throw; }
        }

        public void GetUserInfo(string userId)
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
            var gi = new GetImage();

            try
            {
                gi.DownloadImage(ImageUrl, Size, out Image);
            }
            catch { throw; }
        }

        /// <summary>
        /// UserStreamに接続します
        /// </summary>
        /// <param name="IsGetAllReplies">フォロー外のリプライも取得する</param>
        public void ConnectUserStream(bool IsGetAllReplies)
        {
            try
            {
                UserStream.ConnectUserStream(IsGetAllReplies);
            }
            catch { throw; }
        }

        /// <summary>
        /// UserStreamを切断します
        /// </summary>
        public void DisconnectUserStream()
        {
            UserStream.DisconnectUserStream();
        }

        /// <summary>
        ///  取得したツイートをキューから取り出します
        /// </summary>
        /// <returns>ツイートに関する情報のパック</returns>
        public TweetInfoPack TryDequeueTweetInfoQueue()
        {
            return TweetInfo.TryDequeueTweetInfoQueue();
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
            var i = new Rhinemaidens.Helper.EditImage();
            i.ResizeImage(Width, Height, SourceImage, out ResizedImage);
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
        /// <param name="GeneratedImage">生成した画像</param>
        public void GenerateRetweeterImage(int Width, int Height, Bitmap SourceOriginImage, int SourceOriginImageWidth, int SourceOriginImageHeight, Bitmap SourceRetweeterImage, int SourceRetweeterImageWidth, int SourceRetweeterImageHeight, out Bitmap GeneratedImage)
        {
            var i = new Rhinemaidens.Helper.EditImage();
            i.GenerateRetweeterImage(Width, Height, SourceOriginImage, SourceOriginImageWidth, SourceOriginImageHeight, SourceRetweeterImage, SourceRetweeterImageWidth, SourceRetweeterImageHeight, out GeneratedImage);
        }
    }
}