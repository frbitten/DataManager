using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Joo.Utils
{
    public class JooDataContexCollection
    {
        #region [ Fields ]
        private string pathFile;
        //private string imageFile;

        private Dictionary<string, object[]> data;
        #endregion

        #region [ Data Methods ]
        public void AddData(string name, object[] data)
        {
            if (this.data.ContainsKey(name))
            {
                this.data[name] = data;
            }
            else
            {
                this.data.Add(name, data);
            }
        }
        public void RemoveData(string name)
        {
            this.data.Remove(name);
        }
        #endregion

        #region [ Properties ]
        public Dictionary<string, object[]> DataContexItem
        {
            get
            {
                return data;
            }
        }
        #endregion
    }
}
