using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Joo.Utils.Helpers
{
    public abstract class CEPHelper
    {
        #region [ Constants ]
        private static Dictionary<string,int[]> cepsUfs=new Dictionary<string,int[]>();
        private static List<int[]> cepsCapitais = new List<int[]>();
        #endregion

        #region [ Constructor ]
        static CEPHelper()
        {
            cepsUfs.Add("AC", new int[] { 69900000, 69999999 });
            cepsUfs.Add("AL", new int[] { 57000000, 57999999 });
            cepsUfs.Add("AM", new int[] { 69000000, 69299999, 69400000, 69899999 });
            cepsUfs.Add("AP", new int[] { 68900000, 68999999 });
            cepsUfs.Add("BA", new int[] { 40000000, 48999999 });
            cepsUfs.Add("CE", new int[] { 60000000, 63999999 });
            cepsUfs.Add("DF", new int[] { 70000000, 72799999, 73000000, 73699999 });
            cepsUfs.Add("ES", new int[] { 29000000, 29999999 });
            cepsUfs.Add("GO", new int[] { 72800000, 72999999, 73700000, 76799999 });
            cepsUfs.Add("MA", new int[] { 65000000, 65999999 });
            cepsUfs.Add("MG", new int[] { 30000000, 39999999 });
            cepsUfs.Add("MS", new int[] { 79000000, 79999999 });
            cepsUfs.Add("MT", new int[] { 78000000, 78899999 });
            cepsUfs.Add("PA", new int[] { 66000000, 68899999 });
            cepsUfs.Add("PB", new int[] { 58000000, 58999999 });
            cepsUfs.Add("PE", new int[] { 50000000, 56999999 });
            cepsUfs.Add("PI", new int[] { 64000000, 64999999 });
            cepsUfs.Add("PR", new int[] { 80000000, 87999999 });
            cepsUfs.Add("RJ", new int[] { 20000000, 28999999 });
            cepsUfs.Add("RN", new int[] { 59000000, 59999999 });
            cepsUfs.Add("RO", new int[] { 76800000, 76999999 });
            cepsUfs.Add("RR", new int[] { 69300000, 69399999 });
            cepsUfs.Add("RS", new int[] { 90000000, 99999999 });
            cepsUfs.Add("SC", new int[] { 88000000, 89999999 });
            cepsUfs.Add("SE", new int[] { 49000000, 49999999 });
            cepsUfs.Add("SP", new int[] { 01000000, 19999999 });
            cepsUfs.Add("TO", new int[] { 77000000, 77999999 });

            cepsCapitais.Add(new int[] { 69900001, 69920999 });//Rio Branco
            cepsCapitais.Add(new int[] { 57000001, 57099999 });//Maceio
            cepsCapitais.Add(new int[] { 69000001, 69099999 });//Manuaus
            cepsCapitais.Add(new int[] { 68900001, 68911999 });//Macapa
            cepsCapitais.Add(new int[] { 40000001, 42499999 });//Salvador
            cepsCapitais.Add(new int[] { 60000001, 61599999 });//Fortaleza
            cepsCapitais.Add(new int[] { 70000001, 72799999 });//Brasilia
            cepsCapitais.Add(new int[] { 73000001, 73699999 });//Brasilia
            cepsCapitais.Add(new int[] { 29000001, 29099999 });//Vitoria
            cepsCapitais.Add(new int[] { 74000001, 74899999 });//Goiania
            cepsCapitais.Add(new int[] { 65000001, 65109999 });//São Luis
            cepsCapitais.Add(new int[] { 30000001, 31999999 });//Belo Horizonte
            cepsCapitais.Add(new int[] { 79000001, 79124999 });//campo grande
            cepsCapitais.Add(new int[] { 78000001, 78099999 });//cuiaba
            cepsCapitais.Add(new int[] { 66000001, 66999999 });//belem
            cepsCapitais.Add(new int[] { 58000001, 58099999 });//João pessoa
            cepsCapitais.Add(new int[] { 50000001, 52999999 });//recife
            cepsCapitais.Add(new int[] { 64000001, 64099999 });//teresina
            cepsCapitais.Add(new int[] { 80000001, 82999999 });//curitiba
            cepsCapitais.Add(new int[] { 20000001, 23799999 });//rio de janeiro
            cepsCapitais.Add(new int[] { 59000001, 59139999 });//natal
            cepsCapitais.Add(new int[] { 76800001, 76834999 });//porto velho
            cepsCapitais.Add(new int[] { 69300001, 69339999 });//boa vista
            cepsCapitais.Add(new int[] { 90000001, 91999999 });//porto alegre
            cepsCapitais.Add(new int[] { 88000001, 88099999 });//florianopolis
            cepsCapitais.Add(new int[] { 49000001, 49098999 });//aracaju
            cepsCapitais.Add(new int[] { 01000001, 05999999 });//são paulo
            cepsCapitais.Add(new int[] { 08000000, 08499999 });//são paulo
            cepsCapitais.Add(new int[] { 77000001, 77249999 });//palmas
        }
        #endregion
        public static String GetUFByCEP(string cep)
        {
            if (cep[5]=='-')
            {
                cep = cep.Remove(5, 1);
            }
            try
            {
                int number = int.Parse(cep);
                foreach (var item in cepsUfs)
                {
                    for (int i = 0; i < item.Value.Length; i+=2)
                    {
                        if (number >= item.Value[i] && number <= item.Value[i+1])
                        {
                            return item.Key;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                throw new ArgumentException("cep incorreto. Utilize o formato XXXXX-XXX");
            }
            return String.Empty;
        }

        public static String[] GetUFs
        {
            get{ return cepsUfs.Keys.ToArray();}
        }

        public static bool IsCapital(string cep)
        {

            if (cep[5] == '-')
            {
                cep = cep.Remove(5, 1);
            }
            try
            {
                int number = int.Parse(cep);
                foreach (var item in cepsCapitais)
                {
                    for (int i = 0; i < item.Length; i += 2)
                    {
                        if (number >= item[i] && number <= item[i + 1])
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                throw new ArgumentException("cep incorreto. Utilize o formato XXXXX-XXX");
            }
            return false;
        }
    }
}
