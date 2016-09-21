using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data.Common;
using Joo.Database.Attributes;
using Joo.Database.Exceptions;
using Joo.Database.Structs;
using System.Data;
using Joo.Database.Types;
using System.IO;
using Joo.Database.Cache;

namespace Joo.Database.Connections
{
    public abstract class DataBaseConnection
    {
        #region [ Constants ]
        public enum LEVELS_OF_SEARCH : sbyte
        {
            ON_DEMAND = -1,
            NONE = 0,
            ONE = 1,
            TWO = 2,
            THREE = 3,
            FOUR = 4,
            FIVE=5,
            SIX=6,
            SEVEN=7,
            EIGTH=8,
            NINE=9,
            TEN=10,
            ALL = 99
        }
        #endregion

        #region [ Fields ]
        protected Dictionary<Guid, Dictionary<int, BasicModel>> cacheTransaction;
        #endregion

        #region [ Constructor ]
        public DataBaseConnection()
        {
            cacheTransaction = new Dictionary<Guid, Dictionary<int, BasicModel>>();
        }
        #endregion

        #region [ Abstract Methods ]
        public abstract void BeginTransaction();
        public abstract void ComitTransaction();
        public abstract void RoolbackTransaction();
        protected abstract List<BasicModel> GetItemsProtected(Type type, Where where, OrderBy orderby, int level,int start,int length);

        [Obsolete("Use o metodo com generics")]
        public abstract int GetLenghtItems(Type type, Where where);
        public abstract int GetLenghtItems<T>(Where where) where T : BasicModel;
        
        protected abstract bool DeleteItem(BasicModel model);
        protected abstract bool InsertItem(BasicModel model);
        protected abstract bool UpdateItem(BasicModel model);

        protected abstract bool DeleteOneToManyRelationships(DatabaseType type, int argsId);
        protected abstract bool DeleteOneToManyRelationships(BasicModel model);

        protected abstract bool ExistItemsInRelation(DatabaseRelationshipInfo relation, int argsId, out int[] relationships);
        protected abstract bool DeleteItemsFromTheRelation(DatabaseRelationshipInfo relation, int argsId); 


        public abstract bool CreateTable<T>();
        [Obsolete("Use o metodo com generics")]
        public abstract bool CreateTable(DatabaseType objectType);

        [Obsolete("Use o metodo com generics")]
        public abstract bool DeleteAllItems(DatabaseType objectType);
        public abstract bool DeleteAllItems<T>() where T : BasicModel;

        public abstract bool DropTable<T>() where T : BasicModel;
        public abstract DataTable ExecuteCustomSQL(string sql);
        public abstract DataTable ExecuteCustomSQL(DbCommand sql);
        public abstract int ExecuteCustomSQLNotQuery(string sql);
        public abstract string ConvertOperatorToString(Operator op);
        public abstract void ExecuteScript(string script, string delimiter);
        public abstract DataTable ExecuteStoredProcedure(string name);
        public abstract DataTable ExecuteStoredProcedure(string name, IEnumerable<Parameter> parameters);
        public abstract bool InsertIgnoringIdentify(BasicModel model);
        public abstract int Duplicate<T>(int id) where T : BasicModel;
        #endregion

        #region [ Public Methods ]
        [Obsolete("Utilizar metodo com generics")]
        public List<BasicModel> GetItems(Type type)
        {
            return GetItems(type, null, null, -1);
        }
        [Obsolete("Utilizar metodo com generics")]
        public List<BasicModel> GetItems(Type type, LEVELS_OF_SEARCH level)
        {
            return GetItems(type, null, null, (int)level);
        }
        [Obsolete("Utilizar metodo com generics")]
        public List<BasicModel> GetItems(Type type, Where where)
        {
            return GetItems(type, where, null, -1);
        }
        [Obsolete("Utilizar metodo com generics")]
        public List<BasicModel> GetItems(Type type, Where where, OrderBy orderby)
        {
            return GetItems(type, where, orderby, -1);
        }
        [Obsolete("Utilizar metodo com generics")]
        public List<BasicModel> GetItems(Type type, Where where, LEVELS_OF_SEARCH level)
        {
            return GetItems(type, where, null, (int)level);
        }
        [Obsolete("Utilizar metodo com generics")]
        public List<BasicModel> GetItems(Type type, Where where, OrderBy orderby, LEVELS_OF_SEARCH level)
        {
            return GetItems(type, where, orderby, (int)level);
        }
        [Obsolete("Utilizar metodo com generics")]
        public int GetLenghtItems(Type type)
        {
            return GetLenghtItems(type, null);
        }
        [Obsolete("Utilizar metodo com generics")]
        public List<BasicModel> GetItems(Type type, Where where, OrderBy orderby, int level)
        {
            List<BasicModel> ret = GetItemsProtected(type, where, orderby, level,0,0);
            cacheTransaction.Clear();
            return ret;
        }

        public List<T> GetItems<T>() where T : BasicModel
        {
            return GetItems<T>(null, null,0,0, -1);
        }

        public List<T> GetItems<T>(LEVELS_OF_SEARCH level) where T : BasicModel
        {
            return GetItems<T>(null, null,0,0, (int)level);
        }
        public List<T> GetItems<T>(int start,int length,LEVELS_OF_SEARCH level) where T : BasicModel
        {
            return GetItems<T>(null, null,start,length, (int)level);
        }

        public List<T> GetItems<T>(Where where) where T : BasicModel
        {
            return GetItems<T>(where, null,0,0, -1);
        }

        public List<T> GetItems<T>(Where where, OrderBy orderby) where T : BasicModel
        {
            return GetItems<T>(where, orderby,0,0, -1);
        }

        public List<T> GetItems<T>(Where where, LEVELS_OF_SEARCH level) where T : BasicModel
        {
            return GetItems<T>(where, null,0,0, (int)level);
        }
        public List<T> GetItems<T>(Where where,int start,int length, LEVELS_OF_SEARCH level) where T : BasicModel
        {
            return GetItems<T>(where, null,start,length, (int)level);
        }

        //public List<T> GetItems<T>(Where where, OrderBy orderby, LEVELS_OF_SEARCH level) where T : BasicModel
        //{
        //    return GetItems<T>(where, orderby, (int)level);
        //}

        public int GetLenghtItems<T>() where T : BasicModel
        {
            return GetLenghtItems<T>(null);
        }

        public List<T> GetItems<T>(Where where, OrderBy orderby, int level) where T : BasicModel
        {
            List<T> ret = GetItemsProtected(typeof(T), where, orderby, level,0,0).ConvertAll<T>(obj => (T)obj).ToList<T>();
            cacheTransaction.Clear();
            return ret;
        }
        public List<T> GetItems<T>(Where where, OrderBy orderby,int start,int length, int level) where T : BasicModel
        {
            List<T> ret = GetItemsProtected(typeof(T), where, orderby, level, start, length).ConvertAll<T>(obj => (T)obj).ToList<T>();
            cacheTransaction.Clear();
            return ret;
        }

        public BasicModel LoadItem(BasicModel model, LEVELS_OF_SEARCH level)
        {
            return LoadItem(model, (int)level);
        }
        public BasicModel LoadItem(BasicModel model)
        {
            return LoadItem(model, (int)LEVELS_OF_SEARCH.NONE);
        }
        public BasicModel LoadItem(BasicModel model, int level)
        {
            if (model.Status != Status.Normal)
            {
                throw new ArgumentException("Object status invalid for operation");
            }
            return LoadItem(model.GetType(), model.ID, level);
        }
        [Obsolete("Use o metodo com generics")]
        public BasicModel LoadItem(Type type, int modelId, LEVELS_OF_SEARCH level)
        {
            return LoadItem(type, modelId, (int)level);
        }

        public T LoadItem<T>(int modelId, LEVELS_OF_SEARCH level) where T : BasicModel
        {
            return LoadItem<T>(modelId, (int)level);
        }
        [Obsolete("Use o metodo com generics")]
        public BasicModel LoadItem(Type type, int modelId, int level)
        {
            if (CacheManager.Instance.HasObject(type, modelId, level > (int)LEVELS_OF_SEARCH.ON_DEMAND))
            {
                BasicModel model = CacheManager.Instance.GetObject(type, modelId, level > (int)LEVELS_OF_SEARCH.ON_DEMAND);
                DatabaseType databaseType = TypesManager.TypeOf(type);
                AddObjectTransactionCache(model);
                if (databaseType.DataBaseRelationship.Length > 0)
                {
                    ProcessRelationships(new List<BasicModel>() { model }, databaseType, level);
                }

                model.Status = Status.Normal;
                model.OnLoaded();

                cacheTransaction.Clear();
                return model;
            }

            Where where = new Where();
            where.AddItem(type, "ID");
            where.AddOperator(Operator.EQUAL);
            where.AddItem(modelId);

            List<BasicModel> list = GetItemsProtected(type, where, null,level,0,0);
            if (list.Count <= 0)
            {
                throw new ArgumentException("Object not found in database.");
            }
            if (list.Count > 1)
            {
                throw new ArgumentException("ID repeated in the database.");
            }
            cacheTransaction.Clear();
            return list[0];
        }

        public T LoadItem<T>(int modelId, int level) where T : BasicModel
        {
            Type type = typeof(T);
            if (CacheManager.Instance.HasObject(type, modelId, level > (int)LEVELS_OF_SEARCH.ON_DEMAND))
            {
                BasicModel model = CacheManager.Instance.GetObject(type, modelId, level > (int)LEVELS_OF_SEARCH.ON_DEMAND);
                DatabaseType databaseType = TypesManager.TypeOf(type);
                AddObjectTransactionCache(model);
                if (databaseType.DataBaseRelationship.Length > 0)
                {
                    ProcessRelationships(new List<BasicModel>() { model }, databaseType, level);
                }

                model.Status = Status.Normal;
                model.OnLoaded();

                cacheTransaction.Clear();
                return (T)model;
            }

            Where where = new Where();
            where.AddItem(type, "ID");
            where.AddOperator(Operator.EQUAL);
            where.AddItem(modelId);

            List<T> list = GetItemsProtected(typeof(T), where, null,level,0,0).ConvertAll<T>(obj => (T)obj).ToList<T>();
            if (list.Count <= 0)
            {
                throw new ArgumentException("Object not found in database.");
            }
            if (list.Count > 1)
            {
                throw new ArgumentException("ID repeated in the database.");
            }
            cacheTransaction.Clear();
            return list[0];
        }

        public bool SaveItem(BasicModel model)
        {
            DatabaseType databaseType = model.GetDatabaseType();

            //salva relação 1 para 1 primeiro, para ter ID do objeto e setar na propriedade
            var oneToOne = databaseType.DataBaseRelationship.Where(obj => obj.Property.PropertyType.IsArray == false);
            foreach (var rela in oneToOne)
            {
                BasicModel element = rela.FastGetValue(model) as BasicModel;
                if (element != null)
                {
                    Status oldStatus = element.Status;
                    if (oldStatus != Status.Normal && oldStatus != Status.Invalid)
                    {
                        if (SaveItem(element))
                        {
                            if (oldStatus == Status.New)
                            {
                                databaseType.GetPropertyInfo(rela.Attribute.FieldName).FastSetValue(model, element.ID);
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (oldStatus == Status.Normal)
                        {
                            var parentRelationship = databaseType.GetPropertyInfo(rela.Attribute.FieldName);
                            object auxId = parentRelationship.FastGetValue(model);

                            if (auxId is int && (int)auxId <= 0)
                            {
                                parentRelationship.FastSetValue(model, element.ID);
                            }
                        }
                    }
                }
            }

            model.OnStartSaving();
            //salva o objeto propriamente dito
            bool ret = false;
            switch (model.Status)
            {
                case Status.Delete:
                    ret = DeleteItem(model);
                    if (ret)
                    {
                        model.OnFinishSaving();
                        Cache.CacheManager.Instance.Remove(databaseType.Type, model.ID);
                        RemoveObjectTransactionCache(model, databaseType.Type);
                        return ret;
                    }
                    break;
                case Status.New:
                    ret = InsertItem(model);
                    break;
                case Status.Update:
                    ret = UpdateItem(model);
                    break;
                case Status.Normal:
                    ret = true;
                    break;
            }

            //salva relações 1 para muitos
            if (ret)
            {
                Cache.CacheManager.Instance.Remove(databaseType.Type,model.ID);
                RemoveObjectTransactionCache(model, databaseType.Type);

                var oneToMany = databaseType.DataBaseRelationship.Where(obj => obj.Property.PropertyType.IsArray);
                foreach (var rela in oneToMany)
                {
                    Type elementType = rela.ElementType;

                    BasicModel[] elements = rela.FastGetValue(model) as BasicModel[];
                    if (elements != null)
                    {
                        foreach (BasicModel element in elements)
                        {
                            if (element.Status == Status.New)
                            {
                                element.GetDatabaseType().GetPropertyInfo(rela.Attribute.FieldName).FastSetValue(element, model.ID);
                            }
                            if (element.Status != Status.Normal && element.Status != Status.Invalid)
                            {
                                SaveItem(element);
                            }
                        }

                        List<BasicModel> aux = new List<BasicModel>();
                        foreach (BasicModel element in elements)
                        {
                            if (element.Status != Status.Invalid)
                            {
                                aux.Add(element);
                            }
                        }
                        Array newElements = Array.CreateInstance(elementType, aux.Count);
                        for (int i = 0; i < aux.Count; i++)
                        {
                            newElements.SetValue(aux[i], i);
                        }
                        rela.FastSetValue(model, newElements);
                    }
                }
            }
            model.OnFinishSaving();
            //Cache.CacheManager.Instance.Add(model);
            return ret;
        }

        public void SaveItems(List<BasicModel> models)
        {
            foreach (BasicModel item in models)
            {
                SaveItem(item);
            }
        }

        public void SaveItems(BasicModel[] models)
        {
            foreach (BasicModel item in models)
            {
                SaveItem(item);
            }
        }
        #endregion

        #region [ Protected Methods ]

        protected void SaveFiles(BasicModel model)
        {
            DatabaseType type = new DatabaseType(model.GetType());
            String namefile = DataBaseManager.Instance.DirectoryFiles + "/" + model.GetType().FullName + "/" + model.ID + "/";
            foreach (DatabaseFileInfo info in type.DataBaseFiles)
            {
                var value = info.FastGetValue(model);

                if (!Directory.Exists(namefile))
                {
                    Directory.CreateDirectory(namefile);
                }
                namefile += info.Attribute.Name + "." + info.Attribute.Extension;
                if (File.Exists(namefile))
                {
                    File.Delete(namefile);
                }
                if (info.Property.PropertyType == typeof(String))
                {
                    File.WriteAllText(namefile, (String)value, UTF8Encoding.UTF8);
                }
                else
                {
                    File.WriteAllBytes(namefile, (byte[])value);
                }
            }
        }

        protected void DeleteFiles(BasicModel model)
        {
            DatabaseType type = new DatabaseType(model.GetType());
            String namefile = DataBaseManager.Instance.DirectoryFiles + "/" + model.GetType().FullName + "/" + model.ID + "/";
            foreach (DatabaseFileInfo info in type.DataBaseFiles)
            {
                var value = info.FastGetValue(model);

                if (!Directory.Exists(namefile))
                {
                    continue;
                }
                namefile += info.Attribute.Name + "." + info.Attribute.Extension;
                if (File.Exists(namefile))
                {
                    File.Delete(namefile);
                }
            }
        }

        protected void LoadFiles(BasicModel model)
        {
            DatabaseType type = new DatabaseType(model.GetType());
            String namefile = DataBaseManager.Instance.DirectoryFiles + "/" + model.GetType().FullName + "/" + model.ID + "/";
            foreach (DatabaseFileInfo info in type.DataBaseFiles)
            {
                if (!Directory.Exists(namefile))
                {
                    continue;
                }
                namefile += info.Attribute.Name + "." + info.Attribute.Extension;
                if (!File.Exists(namefile))
                {
                    continue;
                }
                if (info.Property.PropertyType == typeof(String))
                {
                    var value = File.ReadAllText(namefile, UTF8Encoding.UTF8);
                    info.FastSetValue(model, value);
                }
                else
                {
                    var value = File.ReadAllBytes(namefile);
                    info.FastSetValue(model, value);
                }
            }
        }

        protected virtual T ConvertValue<T>(Object value)
        {
            return (T)value;
        }

        protected virtual object ConvertValue(Object value,Type type)
        {
            return Convert.ChangeType(value, type);
        }

        protected bool HasObjectTransactionCache(Type type, int id)
        {
            return cacheTransaction.ContainsKey(type.GUID) && cacheTransaction[type.GUID].ContainsKey(id);
        }
        protected void AddObjectTransactionCache(BasicModel model)
        {
            Type type = model.GetType();
            if (cacheTransaction.ContainsKey(type.GUID))
            {
                if (cacheTransaction[type.GUID].ContainsKey(model.ID))
                {
                    cacheTransaction[type.GUID][model.ID] = model;
                }
                else
                {
                    cacheTransaction[type.GUID].Add(model.ID, model);
                }
            }
            else
            {
                Dictionary<int, BasicModel> dic = new Dictionary<int, BasicModel>();
                dic.Add(model.ID, model);
                cacheTransaction.Add(type.GUID, dic);
            }
        }

        protected void RemoveObjectTransactionCache(BasicModel model, Type type)
        {
            if (cacheTransaction.ContainsKey(type.GUID))
            {
                if (cacheTransaction[type.GUID].ContainsKey(model.ID))
                {
                    cacheTransaction[type.GUID].Remove(model.ID);

                    if (cacheTransaction[type.GUID].Count <= 0)
                    {
                        cacheTransaction.Remove(type.GUID);
                    }
                }
            }
        }

        protected BasicModel CreateModel(DbDataReader data, Type type, bool onDemand)
        {
            int id = ConvertValue<int>(data["ID"]);
            if (HasObjectTransactionCache(type, id))
            {
                return cacheTransaction[type.GUID][id];
            }
            if (CacheManager.Instance.HasObject(type, id, !onDemand))
            {
                BasicModel cacheModel = CacheManager.Instance.GetObject(type, id, !onDemand);
                AddObjectTransactionCache(cacheModel);
                return cacheModel;
            }

            BasicModel model = Activator.CreateInstance(type) as BasicModel;
            model.OnStartLoad();
            DatabaseType databaseType = model.GetDatabaseType();
            foreach (var field in databaseType.DataBaseProperties)
            {
                if (onDemand && field.IsOnDemandField)
                {
                    continue;
                }

                object value = ConvertValue(data[field.Attribute.Name], field.ElementType);
                if (value != null)
                {
                    if (value == DBNull.Value)
                    {
                        field.FastSetValue(model, null);
                    }
                    else
                    {
                        field.FastSetValue(model, value);
                    }
                }
            }
            model.IsFull = !onDemand;

            BasicModel aux = model.Clone<BasicModel>();
            aux.Status = Status.Normal;
            if (databaseType.SaveInCache)
            {
                CacheManager.Instance.Add(aux);
            }
            AddObjectTransactionCache(model);
            return model;
        }
        protected void PopulateRelationshipsOneToMany(DatabaseRelationshipInfo relaInfo, BasicModel model, int level)
        {
            BasicModel[] array = relaInfo.FastGetValue(model) as BasicModel[];
            if (array != null && array.Length > 0)
            {

                DatabaseType databaseType = TypesManager.TypeOf(relaInfo.ElementType);

                ProcessRelationships(array, databaseType, level - 1);
                foreach (BasicModel item in array)
                {
                    item.Status = Status.Normal;
                }
                return;
            }

            List<BasicModel> elements = null;
            Where where = null;

            if (relaInfo.Attribute.Where != null)
            {
                where = relaInfo.Attribute.Where.Clone();
            }
            else
            {
                where = new Where();
            }
            if (where.Items.Count > 0)
            {
                where.AddOperator(Operator.AND);
            }
            where.AddItem(relaInfo.ElementType, relaInfo.Attribute.FieldName);
            where.AddOperator(Operator.EQUAL);
            where.AddItem(model.ID);

            elements = GetItemsProtected(relaInfo.ElementType, where, null, level - 1,0,0);

            if (elements != null)
            {
                object[] newArray = Array.CreateInstance(relaInfo.ElementType, elements.Count) as object[];

                for (int i = 0; i < elements.Count; i++)
                {
                    newArray[i] = elements[i];
                }

                relaInfo.FastSetValue(model, newArray);
            }
        }

        protected void PopulateRelationshipsOneToOne(DatabaseRelationshipInfo relaInfo, BasicModel model, int parentId, int level)
        {
            //Type element = relaInfo.Property.PropertyType;
            // § // if (relaInfo.Property.GetValue(model, null) != null) // propriedade ja foi prenchida, só valida se as relações estão carregadas corretamente.
            if (relaInfo.FastGetValue(model) != null) // propriedade ja foi prenchida, só valida se as relações estão carregadas corretamente.
            {
                // § // BasicModel aux = relaInfo.Property.GetValue(model, null) as BasicModel;
                BasicModel aux = relaInfo.FastGetValue(model) as BasicModel;
                DatabaseType databaseType = TypesManager.TypeOf(aux);

                ProcessRelationships(new List<BasicModel>() { aux }, databaseType, level - 1);
                aux.Status = Status.Normal;
                return;
            }

            if (HasObjectTransactionCache(relaInfo.Property.PropertyType, parentId)) // pega o objeto do transactio cache e valida se as relações estão corretas
            {
                BasicModel aux = cacheTransaction[relaInfo.Property.PropertyType.GUID][parentId];
                DatabaseType databaseType = TypesManager.TypeOf(aux);
                ProcessRelationships(new List<BasicModel>() { aux }, databaseType, level - 1);
                aux.Status = Status.Normal;

                // § // relaInfo.Property.SetValue(model, aux, null);
                relaInfo.FastSetValue(model, aux);
                return;
            }
            if (CacheManager.Instance.HasObject(relaInfo.Property.PropertyType, parentId, level > (int)LEVELS_OF_SEARCH.ON_DEMAND)) // pega o objeto do cache e busca as relações
            {
                BasicModel aux = CacheManager.Instance.GetObject(relaInfo.Property.PropertyType, parentId, level > (int)LEVELS_OF_SEARCH.ON_DEMAND);
                DatabaseType databaseType = TypesManager.TypeOf(aux);
                AddObjectTransactionCache(aux);
                ProcessRelationships(new List<BasicModel>() { aux }, databaseType, level - 1);
                aux.Status = Status.Normal;

                // § // relaInfo.Property.SetValue(model, aux, null);
                relaInfo.FastSetValue(model, aux);
                return;
            }
            // busca o objeto no banco de dados e suas relaçoes

            Where where = new Where();
            where.AddItem(relaInfo.ElementType, "ID");
            where.AddOperator(Operator.EQUAL);
            where.AddItem(parentId);
            List<BasicModel> elements = null;
            elements = GetItemsProtected(relaInfo.ElementType, where, null, level - 1,0,0);
            if (elements.Count > 1)
            {
                throw new RelationshipException("Was found more than one item in the search for a non-array field.");
            }
            else
            {
                if (elements.Count == 0)
                {
                    // § // relaInfo.Property.SetValue(model, null, null);
                    relaInfo.FastSetValue(model, null);
                }
                else
                {
                    // § // relaInfo.Property.SetValue(model, elements[0], null);
                    relaInfo.FastSetValue(model, elements[0]);
                }
            }
        }

        protected void ProcessRelationships(IEnumerable<BasicModel> models, DatabaseType databaseType, int level)
        {
            foreach (BasicModel model in models)
            {
                if (model.LoadLevel >= level)
                {
                    continue;
                }
                model.LoadLevel = level;
                foreach (var rela in databaseType.DataBaseRelationship)
                {
                    if (rela.IsOnDemandField && level <= (int)LEVELS_OF_SEARCH.NONE)
                    {
                        continue;
                    }
                    if (rela.Property.PropertyType.IsArray)
                    {
                        PopulateRelationshipsOneToMany(rela, model, level);
                    }
                    else
                    {
                        // § // PropertyInfo property = databaseType.GetPropertyInfo(rela.Attribute.FieldName).Property;
                        var property = databaseType.GetPropertyInfo(rela.Attribute.FieldName);
                        // § // int id = (int)property.GetValue(model, null);
                        var id = (int)property.FastGetValue(model);
                        if (id > 0)
                        {
                            PopulateRelationshipsOneToOne(rela, model, id, level);
                        }
                        else
                        {
                            //rela.Property.SetValue(model, null, null);
                        }
                    }
                }
            }
        }
        #endregion
    }
}
