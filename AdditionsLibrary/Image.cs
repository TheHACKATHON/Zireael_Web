using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdditionsLibrary
{
    public static class ImageWork
    {
        public static readonly int MinimumSize = 128;
        public static void ImageForDB(Stream stream, out byte[] imageB, out byte[] bigImageB)
        {
            imageB = null;
            bigImageB = null;
            
            using (var image = Image.FromStream(stream))
            {
                var width = image.Width;
                var height = image.Height;

                if(width < MinimumSize || height < MinimumSize)
                {
                    throw new Exception("Изображение слишком маленькое");
                }

                int newHeight;
                int newWidth;
                if (width > height)
                {
                    newWidth = height;
                    newHeight = height;
                }
                else
                {
                    newWidth = width;
                    newHeight = width;
                }

                var newImage = Crop(stream, new Rectangle(width / 2 - newWidth / 2, height / 2 - newHeight / 2, newWidth, newHeight));

                using (var bigStream = new MemoryStream())
                {
                    var bigImage = ResizeImage(newImage, 1024);
                    bigImage.Save(bigStream, ImageFormat.Png);
                    bigImageB = bigStream.ToArray();
                }

                using (var smallStream = new MemoryStream())
                {
                    var smallImage = ResizeImage(newImage, 128);
                    smallImage.Save(smallStream, ImageFormat.Png);
                    imageB = smallStream.ToArray();
                }
                newImage.Dispose();
            }
        }

        private static Image Crop(Stream stream, Rectangle selection)
        {
            using (var bmp = new Bitmap(Image.FromStream(stream)))
            {
                return bmp.Clone(selection, bmp.PixelFormat);
            }
        }

        private static Image ResizeImage(Image sourceImage, int newSize)
        {
            var res = new Bitmap(newSize, newSize);
            using (var gr = Graphics.FromImage(res))
                gr.DrawImage(sourceImage, 0, 0, res.Width, res.Height);

            return res;
        }
    }
}
