using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Utils
{
    public class CmPixelLengthConverter
    {
        private static double inch = 2.54;
        private static double dpi = 96.0;

        public static double CmToPixel(double cm)
        {
            if (cm > 0)
            {
                double inches = cm / inch;
                return inches * dpi;
            }
            else
            {
                return 0.0;
            }
        }

        public static double PixelToCm(double pixel)
        {
            if (pixel > 0.0)
            {
                double inches = pixel / dpi;
                return inches * inch;
            }
            else
            return 0;
        }

        public static GridLength CmToLenght(double cm)
        {
            if (cm > 0)
            {
                double inches = cm / inch;
                return new GridLength(inches * dpi);

            }
            else
                return new GridLength();
        }

        public static double LenghtToCm(GridLength length)
        {
            if (length.Value > 0.0)
            {
                double inches = (length.Value / dpi);
                return inches * inch;
            }
            else
                return 0;
        }

    }
}
