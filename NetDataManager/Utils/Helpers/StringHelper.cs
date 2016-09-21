using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Utils.Helpers
{
    public abstract class StringHelper
    {
        public enum StringOrientation
        {
            LeftRight,
            RightLeft
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newText"></param>
        /// <param name="texts"></param>
        /// <returns></returns>
        public static String CreateTextNotRepeated(string newText, String[] texts)
        {
            int addCount = 0;
            foreach (String text in texts)
            {
                if (text.IndexOf(newText) == 0)
                {
                    if (text == newText && addCount == 0)
                    {
                        addCount = 1;
                        continue;
                    }
                    else
                    {
                        if (text.LastIndexOf('(') == newText.Length && text.LastIndexOf(')') == text.Length - 1)
                        {
                            try
                            {
                                int count = Int16.Parse(text.Substring(text.LastIndexOf('(') + 1, text.Length - text.LastIndexOf(')')));
                                if (addCount <= count)
                                {
                                    addCount = count + 1;
                                }
                            }
                            catch (Exception)
                            {
                                addCount = 0;
                            }
                        }
                    }
                }
            }
            if (addCount > 0)
            {
                newText += "(" + addCount + ")";
            }
            return newText;
        }

        public static bool isEmail(string inputEmail)
        {
            if (string.IsNullOrEmpty(inputEmail))
            {
                return false;
            }
            else
            {
                string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
      @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
      @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
                Regex re = new Regex(strRegex);
                if (re.IsMatch(inputEmail))
                    return (true);
                else
                    return (false);
            }

        }

        public static byte[] StringHexToByteArray(string hexValues)
        {
            if (hexValues.Length % 2 != 0)
            {
                throw new ArgumentException("Tamanho de string invalido. Tem que possuir numero pares de caracteres. Pois um hexadecimal é composto de 2 caracteres variando de 0 a F.");
            }
            int index = 0;
            int count = 0;
            byte[] ret = new byte[hexValues.Length / 2];
            while (index < hexValues.Length)
            {
                ret[count] = Convert.ToByte(hexValues.Substring(index, 2), 16);
                index += 2;
                count++;
            }
            return ret;
        }

        public static bool PaddingSpaces(ref string main, string add, StringOrientation so, bool abbreviate = false)
        {
            String spaceFree = " ";
            int[] sizes = GetGroupSizesOfSpaces(main);
            bool find = false;
            foreach (int size in sizes)
            {
                if (size > add.Length)
                {
                    find = true;
                    break;
                }
                else
                {
                    if (size > 4 && abbreviate)
                    {
                        find = true;
                        break;
                    }
                }

            }
            if (!find)
            {
                return false;
            }

            if (so == StringOrientation.LeftRight)
            {
                int size = 0;
                for (int i = 0; i < sizes.Length; i++)
                {
                    if (sizes[i] > add.Length)
                    {
                        size = sizes[i];
                        break;
                    }
                    else
                    {
                        if (sizes[i] > 4 && abbreviate)
                        {
                            size = sizes[i];
                            break;
                        }
                    }

                }

                spaceFree = new string(' ', size);

                int index = main.IndexOf(spaceFree);
                if (index < 0)
                {
                    return false;
                }

                if (size < add.Length)
                {
                    var difference = add.Length - size;

                    if (main.Length > index + (add.Length - difference) + 2)
                        add = add.Substring(0, add.Length - (difference + 2) - 1) + "..";
                    else
                        add = add.Substring(0, add.Length - (difference + 2)) + "..";
                }

                OverrideString(ref main, add, index);
            }
            else
            {
                int size = 0;
                for (int i = sizes.Length - 1; i >= 0; i--)
                {
                    if (sizes[i] > add.Length)
                    {
                        size = sizes[i];
                        break;
                    }
                    else
                    {
                        if (sizes[i] > 4 && abbreviate)
                        {
                            size = sizes[i];
                            break;
                        }
                    }
                }

                spaceFree = new string(' ', size);

                int index = main.LastIndexOf(spaceFree);
                if (index < 0)
                {
                    return false;
                }

                if (size < add.Length)
                {
                    var difference = add.Length - size;
                    add = add.Substring(0, add.Length - (difference + 2)) + "..";
                }

                index += (spaceFree.Length - add.Length);
                OverrideString(ref main, add, index);
            }
            return true;
        }
        public static void OverrideString(ref string main, string add, int start)
        {
            main = main.Remove(start, add.Length);
            main = main.Insert(start, add);
        }
        private static int[] GetGroupSizesOfSpaces(string main)
        {
            List<int> sizes = new List<int>();
            int nspaces = 0;
            for (int i = 0; i < main.Length; i++)
            {
                if (main[i] == ' ')
                {
                    nspaces++;
                }
                else
                {
                    sizes.Add(nspaces);
                    nspaces = 0;
                }
            }
            if (nspaces > 1)
            {
                sizes.Add(nspaces);
            }
            return sizes.ToArray();
        }

        public static string PrintCenter(string text, int rowSize, char characterToFill)
        {
            if (rowSize <= 0)
                throw new ArgumentException("Row Size Invalid");

            if (string.IsNullOrEmpty(text))
            {
                char[] newText = new char[rowSize];
                newText.Each(a => a = characterToFill);
                return new string(newText);
            }

            if (text.Length >= rowSize)
                return text;


            string bfspace = string.Empty;
            string afspace = string.Empty;
            string final = text;
            int spacenumber;
            if ((rowSize - text.Length) % 2 == 0)
            {
                spacenumber = (rowSize - text.Length) / 2;
                for (int i = 0; i < spacenumber; i++)
                {
                    bfspace = bfspace + characterToFill;
                }
                final = final.Insert(text.Length, bfspace);
                final = final.Insert(0, bfspace);
                return final;
            }
            else
            {
                double numberLeftAndRight = (double)(rowSize - text.Length) / 2;
                spacenumber = Convert.ToInt32(Math.Round(numberLeftAndRight, MidpointRounding.AwayFromZero));

                for (int i = 0; i < spacenumber; i++)
                {
                    afspace = afspace + characterToFill;
                }
                for (int i = 0; i < spacenumber - 1; i++)
                {
                    bfspace = bfspace + characterToFill;
                }
                final = final.Insert(text.Length, afspace);
                final = final.Insert(0, bfspace);
                return final;
            }
        }
        public static String[] WrapText(string text, int maxCol)
        {
            List<String> ret = new List<string>();
            string[] aux = text.Split('\n');
            foreach (string item in aux)
            {
                if (item.Length > maxCol)
                {
                    String[] aux2 = item.Split(' ');
                    String line=String.Empty;
                    foreach (var item2 in aux2)
                    {
                        if (line.Length + item2.Length + 1 > maxCol)
                        {
                            ret.Add(line);
                            line = String.Empty;
                        }
                        else
                        {
                            if (line != String.Empty)
                            {
                                line += " ";
                            }
                        }
                        line += item2;
                    }
                    ret.Add(line);
                    line = String.Empty;
                }
                else
                {
                    ret.Add(item);
                }
            }
            return ret.ToArray();
        }
    }
}
