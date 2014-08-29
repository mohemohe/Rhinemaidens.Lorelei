using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Rhinemaidens.TwitterAPI.REST
{
    class GetImage
    {
        /// <summary>
        /// 画像を取得します
        /// </summary>
        /// <param name="ImageUrl">URL</param>
        /// <param name="Size">取得するサイズ</param>
        /// <param name="Image">出力先Bitmap</param>
        public void GetImage(string ImageUrl, Rhinemaidens.Lorelei.ImageSize Size, out Bitmap Image)
        {
            string url;

            if (Size == Rhinemaidens.Lorelei.ImageSize.Bigger)
            {
                url = ImageUrl.Replace("_normal", "_bigger");
            }
            else if (Size == Rhinemaidens.Lorelei.ImageSize.Mini)
            {
                url = ImageUrl.Replace("_normal", "_mini");
            }
            else if (Size == Rhinemaidens.Lorelei.ImageSize.Original)
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
    }
}
