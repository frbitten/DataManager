using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Database.Structs
{
    public class OrderBy
    {
        #region [ Fields ]
        private List<object[]> items;
        #endregion [ Fields ]

        #region [ Constructors ]
        public OrderBy()
        {
            items = new List<object[]>();
        }
        #endregion

        #region [ Public Methods ]
        public void AddItem(Type type, string propertyName)
        {
            Object[] item = new object[2];
            item[0] = type;
            item[1] = propertyName;
            items.Add(item);
        }
        #endregion

        #region [ Properties ]
        public List<Object[]> Items
        {
            get { return items; }
        } 
        #endregion
    }
}
