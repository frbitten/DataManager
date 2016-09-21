using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils
{
    public class DataContextCollection
    {
        #region [ Fields ]
        private Dictionary<string, object> data;
        #endregion

        #region [ Constructors ]
        public DataContextCollection()
        {
            this.data = new Dictionary<string, object>();
        }
        #endregion

        #region [ Data Methods ]
        public void AddData(string name, object data)
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
        public Dictionary<string, object> DataContextItem
        {
            get
            {
                return data;
            }
        }
        #endregion
    }
}
