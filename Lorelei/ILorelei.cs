using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rhinemaidens
{
    interface ILorelei
    {
        /// <summary>
        /// OAuthに必要なヘッダを生成します
        /// </summary>
        /// <param name="EncodedUrl">エンコード済みURL</param>
        /// <param name="Method">GET, POST, etc...</param>
        /// <param name="ExtString1">URLの直後に必要な追加シグネチャ</param>
        /// <param name="ExtString2">oauth_versionの直後に必要な追加シグネチャ</param>
        /// <returns>ヘッダ文字列</returns>
        string BuildHeaderString(string EncodedUrl, string Method, string ExtString1, string ExtString2);

        /// <summary>
        /// OAuthでこの連携アプリへのアクセス許可を取得するためのURLを生成します
        /// </summary>
        /// <param name="OAuthUrl">認証URL</param>
        void GetOAuthUrl(out string OAuthUrl);

        /// <summary>
        /// ユーザが入力したPINからアクセストークンを取得します
        /// </summary>
        /// <param name="pin">PIN</param>
        /// <param name="AccessToken">AccessToken</param>
        /// <param name="AccessTokenSecret">AccessTokenSecret</param>
        void GetAccessToken(string pin, out string AccessToken, out string AccessTokenSecret);

        /// <summary>
        /// ツイートを投稿します
        /// </summary>
        /// <param name="Body">本文</param>
        void PostTweet(string Body);

        /// <summary>
        /// 画像付きツイートを投稿します
        /// </summary>
        /// <param name="Body">本文</param>
        /// <param name="ImageFilePath">画像のパス</param>
        void PostTweetWithImage(string Body, string ImageFilePath);

        /// <summary>
        /// 画像を取得します
        /// </summary>
        /// <param name="ImageUrl">URL</param>
        /// <param name="Size">取得するサイズ</param>
        /// <param name="Image">出力先Bitmap</param>
        void GetImage(string ImageUrl, Rhinemaidens.Lorelei.ImageSize Size, out Bitmap Image);

        /// <summary>
        /// UserStreamに接続します
        /// </summary>
        /// <param name="IsGetAllReplies">フォロー外のリプライも取得する</param>
        void ConnectUserStream(bool IsGetAllReplies);

        /// <summary>
        /// UserStreamを切断します
        /// </summary>
        void DisconnectUserStream();

        /// <summary>
        /// 画像をリサイズします
        /// </summary>
        /// <param name="SourceImage">リサイズ元の画像</param>
        /// <param name="Width">幅</param>
        /// <param name="Height">高さ</param>
        /// <param name="ResizedImage">リサイズ後の画像</param>
        void ResizeImage(int Width, int Height, Bitmap SourceImage, out Bitmap ResizedImage);

        /// <summary>
        /// リツイート用の画像を生成します
        /// </summary>
        /// <param name="SourceImage">リツイートしたアカウントのアイコン</param>
        /// <param name="SourceImageWidth">幅</param>
        /// <param name="SourceImageHeight">高さ</param>
        /// <param name="SourceOriginImage">リツイートされたアカウントのアイコン</param>
        /// <param name="SourceOriginImageWidth">幅</param>
        /// <param name="SourceOriginImageHeight">高さ</param>
        /// <param name="OffsetX">リツイートされたアカウントのアイコンの横位置</param>
        /// <param name="OffsetY">リツイートされたアカウントのアイコンの縦位置</param>
        /// <param name="GeneratedImage"></param>
        void GenerateRetweeterImage(int Width, int Height, Bitmap SourceOriginImage, int SourceOriginImageWidth, int SourceOriginImageHeight, Bitmap SourceRetweeterImage, int SourceRetweeterImageWidth, int SourceRetweeterImageHeight, out Bitmap GeneratedImage);
        
    }
}
