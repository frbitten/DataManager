using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing;
using System.Windows.Media;

namespace Utils.Helpers.Visual
{
    public static class ManipulatingImage
    {

        /// <summary>
        /// Change DPI image To 96
        /// </summary>
        /// <param name="bitmapImage"></param>
        /// <param name="encoder">Default enconder is JPEG</param>
        /// <returns></returns>
        public static BitmapImage ConvertBitmapTo96DPI(BitmapImage bitmapImage, BitmapEncoder encoder)
        {
            int width = bitmapImage.PixelWidth;
            int height = bitmapImage.PixelHeight;

            int stride = width * 4; // 4 bytes per pixel
            byte[] pixelData = new byte[stride * height];
            bitmapImage.CopyPixels(pixelData, stride, 0);

            BitmapSource bitmapSource = BitmapSource.Create(width, height, 96, 96, PixelFormats.Pbgra32, null, pixelData, stride);

            // Convert to BitmapImage
            if (encoder == null)
                encoder = new JpegBitmapEncoder();

            MemoryStream memoryStream = new MemoryStream();
            BitmapImage bImg = new BitmapImage();

            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            encoder.Save(memoryStream);

            bImg.BeginInit();
            bImg.CacheOption = BitmapCacheOption.None;
            bImg.StreamSource = new MemoryStream(memoryStream.ToArray());
            bImg.EndInit();

            memoryStream.Close();

            return bImg;
        }

        /// <summary>
        /// Convert System.Media.Image.Imaging.BitmapImage (WPF) to System.Drawing.Bitmap (WinForms and Others)
        /// </summary>
        /// <param name="bitmapImage"></param>
        /// <param name="encoder">Default enconder is JPEG</param>
        /// <returns></returns>        
        public static Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage, BitmapEncoder encoder)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                if (encoder == null)
                    encoder = new JpegBitmapEncoder();

                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                encoder.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        public static BitmapImage ToBitmapImage(this Bitmap bitmap, System.Drawing.Imaging.ImageFormat encoder)
        {
            MemoryStream outStream = new MemoryStream();
            encoder = null;

            if (encoder == null)
                encoder = System.Drawing.Imaging.ImageFormat.Jpeg;

            bitmap.Save(outStream, encoder);

            MemoryStream other = new MemoryStream(outStream.GetBuffer());
            outStream.Dispose();

            var image = new BitmapImage();
            image.CacheOption = BitmapCacheOption.None;
            image.BeginInit();
            image.StreamSource = other;
            image.EndInit();

            return image;
        }

        public static bool ContainsTransparent(System.Drawing.Bitmap image)
        {
            for (int y = 0; y < image.Height; ++y)
            {
                for (int x = 0; x < image.Width; ++x)
                {
                    if (image.GetPixel(x, y).A != 255)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
