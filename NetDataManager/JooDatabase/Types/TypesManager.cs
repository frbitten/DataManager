using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Joo.Database.Exceptions;

namespace Joo.Database.Types
{
    public abstract class TypesManager
    {
        #region [ Static ]
        private static Dictionary<Type, DatabaseType> ReflectionCache = new Dictionary<Type, DatabaseType>();
        #endregion

        #region [ Public Methods]
        public static DatabaseType TypeOf(BasicModel model)
        {
            return TypeOf(model.GetType());
        }
        public static DatabaseType TypeOf(Type modelType)
        {
            if (ReflectionCache.ContainsKey(modelType))
            {
                return ReflectionCache[modelType];
            }
            
            DatabaseType databaseType = new DatabaseType(modelType);
            Validate(databaseType);
            ReflectionCache.Add(modelType, databaseType);
            return databaseType;
        }
        #endregion

        #region [ Private Methods ]
        private static void Validate(DatabaseType type)
        {
            if (type.DataBaseProperties.Length <= 1)
            {
                throw new FieldException("Not found field in " + type.Name + ".");
            }
        }
        #endregion
    }
}
