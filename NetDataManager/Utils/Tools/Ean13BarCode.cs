using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Windows;

namespace Utils
{
    public class Ean13BarCode
    {
        #region [ Fields ]
        private double widthBarCode;
        private double heightBarCode;
        private double lineBarCode;
        private string totalCode;
        #endregion

        #region [ Codification Fields ]
        // Left Hand Digits.
        private string[] _aOddLeft = { "0001101", "0011001", "0010011", "0111101", 
										  "0100011", "0110001", "0101111", "0111011", 
										  "0110111", "0001011" };

        private string[] _aEvenLeft = { "0100111", "0110011", "0011011", "0100001", 
										   "0011101", "0111001", "0000101", "0010001", 
										   "0001001", "0010111" };

        // Right Hand Digits.
        private string[] _aRight = { "1110010", "1100110", "1101100", "1000010", 
										"1011100", "1001110", "1010000", "1000100", 
										"1001000", "1110100" };


        private string _sTail = "101";
        private string _sSeparator = "01010";
        #endregion

        #region [ Properties ]
        public string TotalCode
        {
            get
            {
                return this.totalCode;
            }
            set
            {
                this.totalCode = string.Empty;
                Int64 numberCode = 0;

                if (!Int64.TryParse(value, out numberCode))
                {
                    throw new Exception("The code only accepts numbers");
                }

                if (value.Length != 12)
                {
                    throw new Exception("The code must have twelve digits.");
                }

                int checkSumDigit = CalculateChecksumDigit(value);
                this.totalCode = (value + checkSumDigit.ToString());
            }
        }
        #endregion

        #region [ Constructors ]
        public Ean13BarCode()
        {
        }

        public Ean13BarCode(long code)
        {
            TotalCode = code.ToString();
        }

        public Ean13BarCode(string code)
        {
            this.TotalCode = code;
        }
        #endregion

        #region [ Public Methods ]
        public ImageSource EncoderFixed(double totalWidth, double totalHeight)
        {

            if (totalWidth >= 76) //Mínimo de tamanho recomendado.
            {
                this.widthBarCode = totalWidth;

                double line = totalWidth / 95; //Total de Traços em um Barcode;
                return EncoderRelative(line, totalHeight);
            }
        
            return null;
        }

        public ImageSource EncoderRelative(double lineSize, double height)
        {
            if (IsValidCode())
            {
                lineSize = Math.Round(lineSize, 2); // Arredondar caso receba um valor muito quebrado);

                if (lineSize > 0.8) // Valor mínimo para impressão
                {
                    Canvas canvas = GetCanvasBarCode(lineSize, height);

                    // Get the size of canvas
                    Size size = new Size(Math.Ceiling(canvas.Width), Math.Ceiling(canvas.Height));
                    // Measure and arrange the surface
                    // VERY IMPORTANT
                    canvas.Measure(size);
                    canvas.Arrange(new Rect(size));

                    RenderTargetBitmap rtb = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96, 96, System.Windows.Media.PixelFormats.Default);
                    rtb.Render(canvas);

                    return rtb;
                }
                
            }
            return null;
        }

        public Canvas CanvasEncoderFixed(double totalWidth, double totalHeight)
        {

            if (totalWidth >= 76) //Mínimo de tamanho recomendado.
            {
                this.widthBarCode = totalWidth;

                double line = totalWidth / 95; //Total de Traços em um Barcode;
                return CanvasEncoderRelative(line, totalHeight);
            }
            throw new ArgumentException("totalHeight não pode ser menor que 76");
        }

        public Canvas CanvasEncoderRelative(double lineSize, double height)
        {
            if (IsValidCode())
            {
                lineSize = Math.Round(lineSize, 2); // Arredondar caso receba um valor muito quebrado);

                if (lineSize >= 0.8) // Valor mínimo para impressão
                {

                    return GetCanvasBarCode(lineSize, height);
                }

            }
            return null;

        }
        #endregion

        #region [ Private Methods ]
        private Canvas GetCanvasBarCode(double lineSize, double height)
        {
            this.lineBarCode = lineSize;
            this.heightBarCode = height;

            if (this.widthBarCode <= 0)
            {
                this.widthBarCode = (double)lineSize * 95; //Total de Traços em um Barcode;
            }

            string codification = GetCodification();

            Canvas canvas = new Canvas();
            canvas.Width = Math.Ceiling(this.widthBarCode);
            canvas.Height = this.heightBarCode;
            canvas.SnapsToDevicePixels = true; // renderização

            for (int i = 0; i < codification.Length; i++)
            {
                Rectangle bar = new Rectangle();
                bar.Width = lineSize;
                bar.Height = this.heightBarCode;
                bar.SnapsToDevicePixels = true;

                Canvas.SetLeft(bar, ((double)i * lineSize));
                canvas.Children.Add(bar);
                if (codification[i] == '1')
                {
                    bar.Fill = Brushes.Black;
                }
                if (codification[i] == '0')
                {
                    bar.Fill = Brushes.White;
                }
            }

            return canvas;

        }

        private bool IsValidCode()
        {
            Int64 numberCode = 0;

            if (!Int64.TryParse(this.TotalCode, out numberCode))
            {
                throw new Exception("The code only accepts numbers");
            }

            if (this.TotalCode.Length != 13)
            {
                throw new Exception("The code must have twelve digits.");
            }

            return true;
        }

        private int CalculateChecksumDigit(string code)
        {
            Int64 codeTest = 0;
            if (code.Length != 12 || !(Int64.TryParse(code, out codeTest)))
            {
                throw new Exception("The code only accepts numbers and must have twelve digits.");
            }

            int iSum = 0;
            int iDigit = 0;

            // Calculate the checksum digit here.
            for (int i = code.Length; i >= 1; i--)
            {
                iDigit = Convert.ToInt32(code.Substring(i - 1, 1));
                if (i % 2 == 0)
                {	// odd
                    iSum += iDigit * 3;
                }
                else
                {	// even
                    iSum += iDigit * 1;
                }
            }

            int iCheckSum = (10 - (iSum % 10)) % 10;
            return iCheckSum;

        }
        #endregion

        #region [ Codification Methods ] 
        //http://www.codeproject.com/KB/graphics/ean_13_barcodes.aspx

        private string GetCodification()
        {
            string sLeftPattern = "";
            StringBuilder codification = new StringBuilder();
            // Convert the left hand numbers.
            sLeftPattern = ConvertLeftPattern(this.TotalCode.Substring(0, 7));

            codification.AppendFormat("{0}{1}{2}{3}{0}",
            this._sTail,
            sLeftPattern,
            this._sSeparator,
            ConvertToDigitPatterns(TotalCode.Substring(7), this._aRight));

            return codification.ToString();
        }

        private string ConvertLeftPattern(string sLeft)
        {
            switch (sLeft.Substring(0, 1))
            {
                case "0":
                    return CountryCode0(sLeft.Substring(1));

                case "1":
                    return CountryCode1(sLeft.Substring(1));

                case "2":
                    return CountryCode2(sLeft.Substring(1));

                case "3":
                    return CountryCode3(sLeft.Substring(1));

                case "4":
                    return CountryCode4(sLeft.Substring(1));

                case "5":
                    return CountryCode5(sLeft.Substring(1));

                case "6":
                    return CountryCode6(sLeft.Substring(1));

                case "7":
                    return CountryCode7(sLeft.Substring(1));

                case "8":
                    return CountryCode8(sLeft.Substring(1));

                case "9":
                    return CountryCode9(sLeft.Substring(1));

                default:
                    return "";
            }
        }

        private string ConvertToDigitPatterns(string inputNumber, string[] patterns)
        {
            System.Text.StringBuilder sbTemp = new StringBuilder();
            int iIndex = 0;
            for (int i = 0; i < inputNumber.Length; i++)
            {
                iIndex = Convert.ToInt32(inputNumber.Substring(i, 1));
                sbTemp.Append(patterns[iIndex]);
            }
            return sbTemp.ToString();
        }

        private string CountryCode0(string sLeft)
        {
            // 0 Odd Odd  Odd  Odd  Odd  Odd 
            return ConvertToDigitPatterns(sLeft, this._aOddLeft);
        }

        private string CountryCode1(string sLeft)
        {
            // 1 Odd Odd  Even Odd  Even Even 
            System.Text.StringBuilder sReturn = new StringBuilder();
            // The two lines below could be replaced with this:
            // sReturn.Append( ConvertToDigitPatterns( sLeft.Substring( 0, 2 ), this._aOddLeft ) );
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(0, 1), this._aOddLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(1, 1), this._aOddLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(2, 1), this._aEvenLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(3, 1), this._aOddLeft));
            // The two lines below could be replaced with this:
            // sReturn.Append( ConvertToDigitPatterns( sLeft.Substring( 4, 2 ), this._aEvenLeft ) );
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(4, 1), this._aEvenLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(5, 1), this._aEvenLeft));
            return sReturn.ToString();
        }

        private string CountryCode2(string sLeft)
        {
            // 2 Odd Odd  Even Even Odd  Even 
            System.Text.StringBuilder sReturn = new StringBuilder();
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(0, 1), this._aOddLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(1, 1), this._aOddLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(2, 1), this._aEvenLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(3, 1), this._aEvenLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(4, 1), this._aOddLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(5, 1), this._aEvenLeft));
            return sReturn.ToString();
        }

        private string CountryCode3(string sLeft)
        {
            // 3 Odd Odd  Even Even Even Odd 
            System.Text.StringBuilder sReturn = new StringBuilder();
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(0, 1), this._aOddLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(1, 1), this._aOddLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(2, 1), this._aEvenLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(3, 1), this._aEvenLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(4, 1), this._aEvenLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(5, 1), this._aOddLeft));
            return sReturn.ToString();
        }

        private string CountryCode4(string sLeft)
        {
            // 4 Odd Even Odd  Odd  Even Even 
            System.Text.StringBuilder sReturn = new StringBuilder();
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(0, 1), this._aOddLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(1, 1), this._aEvenLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(2, 1), this._aOddLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(3, 1), this._aOddLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(4, 1), this._aEvenLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(5, 1), this._aEvenLeft));
            return sReturn.ToString();
        }

        private string CountryCode5(string sLeft)
        {
            // 5 Odd Even Even Odd  Odd  Even 
            System.Text.StringBuilder sReturn = new StringBuilder();
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(0, 1), this._aOddLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(1, 1), this._aEvenLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(2, 1), this._aEvenLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(3, 1), this._aOddLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(4, 1), this._aOddLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(5, 1), this._aEvenLeft));
            return sReturn.ToString();
        }

        private string CountryCode6(string sLeft)
        {
            // 6 Odd Even Even Even Odd  Odd 
            System.Text.StringBuilder sReturn = new StringBuilder();
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(0, 1), this._aOddLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(1, 1), this._aEvenLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(2, 1), this._aEvenLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(3, 1), this._aEvenLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(4, 1), this._aOddLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(5, 1), this._aOddLeft));
            return sReturn.ToString();
        }

        private string CountryCode7(string sLeft)
        {
            // 7 Odd Even Odd  Even Odd  Even
            System.Text.StringBuilder sReturn = new StringBuilder();
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(0, 1), this._aOddLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(1, 1), this._aEvenLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(2, 1), this._aOddLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(3, 1), this._aEvenLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(4, 1), this._aOddLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(5, 1), this._aEvenLeft));
            return sReturn.ToString();
        }

        private string CountryCode8(string sLeft)
        {
            // 8 Odd Even Odd  Even Even Odd 
            System.Text.StringBuilder sReturn = new StringBuilder();
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(0, 1), this._aOddLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(1, 1), this._aEvenLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(2, 1), this._aOddLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(3, 1), this._aEvenLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(4, 1), this._aEvenLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(5, 1), this._aOddLeft));
            return sReturn.ToString();
        }

        private string CountryCode9(string sLeft)
        {
            // 9 Odd Even Even Odd  Even Odd 
            System.Text.StringBuilder sReturn = new StringBuilder();
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(0, 1), this._aOddLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(1, 1), this._aEvenLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(2, 1), this._aEvenLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(3, 1), this._aOddLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(4, 1), this._aEvenLeft));
            sReturn.Append(ConvertToDigitPatterns(sLeft.Substring(5, 1), this._aOddLeft));
            return sReturn.ToString();
        }
        #endregion
    }
}
