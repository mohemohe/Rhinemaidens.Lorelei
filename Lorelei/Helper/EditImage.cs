using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rhinemaidens.Helper
{
    public class EditImage
    {
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
