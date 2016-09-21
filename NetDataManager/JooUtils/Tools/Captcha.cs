using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Drawing.Drawing2D;
using System.Collections;
using Joo.Utils.Helpers.Visual;
using System.Drawing;

namespace Joo.Utils.Tools
{
    public class Captcha
    {
        // ====================================================================
        // Creates the bitmap image.
        // ====================================================================
        public static BitmapImage GenerateImage(string text, int width, int height, System.Windows.Media.FontFamily wpfFont = null)
        {
            System.Drawing.FontFamily family = null;
            var allFonts = new System.Drawing.Text.InstalledFontCollection().Families;
            if (family != null)
            {
                foreach (var item in allFonts)
                {
                    if (wpfFont.Source == item.Name)
                    {
                        family = item;
                        break;
                    }
                }
            }
            else
            {
                family = allFonts.FirstOrDefault(obj => obj.Name == "Arial");
            }

            if (family == null)
                return null;

            if (width <= 0 || height <= 0)
                return null;

            if (string.IsNullOrEmpty(text))
                return null;

            // Create a new 32-bit bitmap image.
            Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Random random = new Random();

            // Create a graphics object for drawing.
            Graphics g = Graphics.FromImage(bitmap);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, width, height);

            // Fill in the background.
            HatchBrush hatchBrush = new HatchBrush(
              HatchStyle.SmallConfetti,
              Color.LightGray,
              Color.White);
            g.FillRectangle(hatchBrush, rect);

            // Set up the text font.
            SizeF size;
            float fontSize = rect.Height + 1;
            Font font;
            // Adjust the font size until the text fits within the image.
            do
            {
                fontSize--;
                font = new Font(
                  family,
                  fontSize,
                  FontStyle.Bold);

                size = g.MeasureString(text, font);
            } while (size.Width > rect.Width);

            // Set up the text format.
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;

            // Create a path using the text and warp it randomly.
            GraphicsPath path = new GraphicsPath();
            path.AddString(
              text,
              family,
              (int)font.Style,
              font.Size, rect,
              format);
            float v = 4F;
            PointF[] points =
            {
            new PointF(
                random.Next(rect.Width) / v,
                random.Next(rect.Height) / v),
            new PointF(
                rect.Width - random.Next(rect.Width) / v,
                random.Next(rect.Height) / v),
            new PointF(
                random.Next(rect.Width) / v,
                rect.Height - random.Next(rect.Height) / v),
            new PointF(
                rect.Width - random.Next(rect.Width) / v,
                rect.Height - random.Next(rect.Height) / v)
            };
            Matrix matrix = new Matrix();
            
            matrix.Translate(0F, 0F);
            matrix.Rotate(random.Next(-8, 8));
            path.Warp(points, rect, matrix, WarpMode.Perspective, 0F);

            // Draw the text.
            hatchBrush = new HatchBrush(
              HatchStyle.LargeConfetti,
              Color.LightGray,
              Color.DarkGray);
            g.FillPath(hatchBrush, path);

            // Add some random noise.
            int max = Math.Max(rect.Width, rect.Height);
            int min = Math.Min(rect.Width, rect.Height);
            double ratio = max / min;
            for (int i = 0; i < Convert.ToInt32(Math.Round((double)max * Math.Max(1, (4 - ratio)), 0, MidpointRounding.AwayFromZero)); i++)
            {
                int x = random.Next(rect.Width);
                int y = random.Next(rect.Height);
                int w = random.Next(max / 50);
                int h = random.Next(max / 50);
                g.FillEllipse(hatchBrush, x, y, w, h);
            }

            // Clean up.
            font.Dispose();
            hatchBrush.Dispose();
            g.Dispose();

            // Set the image.
            return bitmap.ToBitmapImage(null);
        }

        private static string allowedCharacters = "abcdefghijklmnopqrstuvwxyz0123456789";
        public static string RandomString(int length)
        {
            Random rnd = new Random();
            if (length <= 0)
                return "";

            return new string(Enumerable.Range(0, length)
                   .Select(i => allowedCharacters[rnd.Next(0, allowedCharacters.Length)])
                   .ToArray());
        }
    }
}
