using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils.Helpers
{
    public abstract class IpHelper
    {
        /// <summary> Utilizado para filtrar os zeros a esquerda de um ip (com eles a comunicação com a impressora da erro); </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static  string GetRealIP(string ip)
        {
            string[] numbersString = ip.Split('.');
            int[] numbersInt = new int[4];
            string realIp = string.Empty;

            for (int i = 0; i < numbersString.Length; i++)
            {
                if (string.IsNullOrEmpty(realIp) == false)
                {
                    realIp += ".";
                }
                numbersInt[i] = Convert.ToInt32(numbersString[i]);
                realIp += numbersInt[i].ToString();
            }
            return realIp;
        }
    }
}
