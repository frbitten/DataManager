using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Joo.Database.Cache
{
    public class CacheManager
    {
        #region [ Fields ]
        private Dictionary<Guid, Dictionary<int,BasicModel>> cacheOnDemand;
        private Dictionary<Guid, Dictionary<int, DateTime>> dateOnDemand;
        private Dictionary<Guid, Dictionary<int, BasicModel>> cacheFull;
        private Dictionary<Guid, Dictionary<int, DateTime>> dateFull;
        private bool disable;
        #endregion

        #region [ Constructor ]
        public CacheManager()
        {
            cacheFull=new Dictionary<Guid,Dictionary<int,BasicModel>>();
            cacheOnDemand = new Dictionary<Guid, Dictionary<int, BasicModel>>();
            dateFull = new Dictionary<Guid, Dictionary<int, DateTime>>();
            dateOnDemand = new Dictionary<Guid, Dictionary<int, DateTime>>();
            disable = true;
        }
        #endregion

        #region [ Static Methods ]
        private static CacheManager instance = null;
        public static CacheManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CacheManager();
                }
                return instance;
            }
        }
        #endregion

        #region [ Public Methods ]
        public bool HasObject(Type type,int id,bool isFull)
        {
            if (disable)
            {
                return false;
            }
            if (isFull)
            {
                if (cacheFull.ContainsKey(type.GUID))
                {
                    if (cacheFull[type.GUID].ContainsKey(id))
                    {
                        if (dateFull[type.GUID][id].AddMinutes(30) >= DateTime.Now)
                        {
                            return true;
                        }
                        else
                        {
                            dateFull[type.GUID].Remove(id);
                            if (dateFull[type.GUID].Count == 0)
                            {
                                dateFull.Remove(type.GUID);
                            }
                            cacheFull[type.GUID].Remove(id);
                            if (cacheFull[type.GUID].Count == 0)
                            {
                                cacheFull.Remove(type.GUID);
                            }
                        }
                    }
                }
            }
            else
            {
                if (cacheOnDemand.ContainsKey(type.GUID))
                {
                    if (cacheOnDemand[type.GUID].ContainsKey(id))
                    {
                        if (dateOnDemand[type.GUID][id].AddMinutes(30) >= DateTime.Now)
                        {
                            return true;
                        }
                        else
                        {
                            dateOnDemand[type.GUID].Remove(id);
                            if (dateOnDemand[type.GUID].Count == 0)
                            {
                                dateOnDemand.Remove(type.GUID);
                            }
                            cacheOnDemand[type.GUID].Remove(id);
                            if (cacheOnDemand[type.GUID].Count == 0)
                            {
                                cacheOnDemand.Remove(type.GUID);
                            }
                        }
                    }
                }
            }
            return false;
        }
        public BasicModel GetObject(Type type,int id, bool isFull)
        {
            if (isFull)
            {
                if (cacheFull.ContainsKey(type.GUID))
                {
                    if (cacheFull[type.GUID].ContainsKey(id))
                    {
                        return cacheFull[type.GUID][id].Clone<BasicModel>();
                    }
                }
            }
            else
            {
                if (cacheOnDemand.ContainsKey(type.GUID))
                {
                    if (cacheOnDemand[type.GUID].ContainsKey(id))
                    {
                        return cacheOnDemand[type.GUID][id].Clone<BasicModel>();
                    }
                }
            }
            return null;
        }
        public void Add(BasicModel model)
        {
            Type type = model.GetType();
            if (model.IsFull)
            {
                if (cacheFull.ContainsKey(type.GUID))
                {
                    if (cacheFull[type.GUID].ContainsKey(model.ID))
                    {
                        cacheFull[type.GUID][model.ID] = model;
                        dateFull[type.GUID][model.ID] = DateTime.Now;
                    }
                    else
                    {
                        cacheFull[type.GUID].Add(model.ID, model);
                        dateFull[type.GUID].Add(model.ID, DateTime.Now);
                    }
                }
                else
                {
                    Dictionary<int, BasicModel> dic = new Dictionary<int, BasicModel>();
                    dic.Add(model.ID, model);
                    cacheFull.Add(type.GUID, dic);

                    Dictionary<int, DateTime> date = new Dictionary<int, DateTime>();
                    date.Add(model.ID, DateTime.Now);
                    dateFull.Add(type.GUID, date);
                }
            }
            else
            {
                if (cacheOnDemand.ContainsKey(type.GUID))
                {
                    if (cacheOnDemand[type.GUID].ContainsKey(model.ID))
                    {
                        cacheOnDemand[type.GUID][model.ID] = model;
                        dateOnDemand[type.GUID][model.ID] = DateTime.Now;
                    }
                    else
                    {
                        cacheOnDemand[type.GUID].Add(model.ID, model);
                        dateOnDemand[type.GUID].Add(model.ID, DateTime.Now);
                    }
                }
                else
                {
                    Dictionary<int, BasicModel> dic = new Dictionary<int, BasicModel>();
                    dic.Add(model.ID, model);
                    cacheOnDemand.Add(type.GUID, dic);

                    Dictionary<int, DateTime> date = new Dictionary<int, DateTime>();
                    date.Add(model.ID, DateTime.Now);
                    dateOnDemand.Add(type.GUID, date);
                }
            }
        }

        //public void Remove(BasicModel model, Type type)
        //{
        //    if (cacheFull.ContainsKey(type.GUID))
        //    {
        //        if (cacheFull[type.GUID].ContainsKey(model.ID))
        //        {
        //            cacheFull[type.GUID].Remove(model.ID);
        //            dateFull[type.GUID].Remove(model.ID);

        //            if (cacheFull[type.GUID].Count <= 0)
        //            {
        //                cacheFull.Remove(type.GUID);
        //                dateFull.Remove(type.GUID);
        //            }
        //        }
        //    }

        //    if (cacheOnDemand.ContainsKey(type.GUID))
        //    {
        //        if (cacheOnDemand[type.GUID].ContainsKey(model.ID))
        //        {
        //            cacheOnDemand[type.GUID].Remove(model.ID);
        //            dateOnDemand[type.GUID].Remove(model.ID);

        //            if (cacheOnDemand[type.GUID].Count <= 0)
        //            {
        //                cacheOnDemand.Remove(type.GUID);
        //                dateOnDemand.Remove(type.GUID);
        //            }
        //        }
        //    }
            

        //}

        public void Remove(Type type,int id)
        {
            if (cacheFull.ContainsKey(type.GUID))
            {
                if (cacheFull[type.GUID].ContainsKey(id))
                {
                    cacheFull[type.GUID].Remove(id);
                    dateFull[type.GUID].Remove(id);

                    if (cacheFull[type.GUID].Count <= 0)
                    {
                        cacheFull.Remove(type.GUID);
                        dateFull.Remove(type.GUID);
                    }
                }
            }

            if (cacheOnDemand.ContainsKey(type.GUID))
            {
                if (cacheOnDemand[type.GUID].ContainsKey(id))
                {
                    cacheOnDemand[type.GUID].Remove(id);
                    dateOnDemand[type.GUID].Remove(id);

                    if (cacheOnDemand[type.GUID].Count <= 0)
                    {
                        cacheOnDemand.Remove(type.GUID);
                        dateOnDemand.Remove(type.GUID);
                    }
                }
            }
        }

        public void RemoveAll(Type type)
        {
            if (cacheFull.ContainsKey(type.GUID))
            {
                cacheFull[type.GUID].Clear();
                dateFull[type.GUID].Clear();
            }

            if (cacheOnDemand.ContainsKey(type.GUID))
            {
                cacheOnDemand[type.GUID].Clear();
                dateOnDemand[type.GUID].Clear();
            }
        }

        public void RemoveOldDate()
        {
            DateTime limit = DateTime.Now.AddMinutes(-60);
            for (int j = 0; j < dateFull.Count; j++)
            {
                var type = dateFull.ElementAt(j);
                for (int i = 0; i < type.Value.Count; i++)
                {
                    var date = type.Value.ElementAt(i);
                    if (date.Value < limit)
                    {
                        cacheFull[type.Key].Remove(date.Key);
                        if (cacheFull[type.Key].Count == 0)
                        {
                            cacheFull.Remove(type.Key);
                        }
                        dateFull[type.Key].Remove(date.Key);
                        if (dateFull[type.Key].Count == 0)
                        {
                            dateFull.Remove(type.Key);
                        }
                    }
                }
            }

            for (int j = 0; j < dateOnDemand.Count; j++)
            {
                var type = dateOnDemand.ElementAt(j);
                for (int i = 0; i < type.Value.Count; i++)
                {
                    var date = type.Value.ElementAt(i);
                    if (date.Value < limit)
                    {
                        cacheOnDemand[type.Key].Remove(date.Key);
                        if (cacheOnDemand[type.Key].Count == 0)
                        {
                            cacheOnDemand.Remove(type.Key);
                        }
                        dateOnDemand[type.Key].Remove(date.Key);
                        if (dateOnDemand[type.Key].Count == 0)
                        {
                            dateOnDemand.Remove(type.Key);
                        }
                    }
                }
            }
        }

        public void Clear()
        {
            dateFull.Clear();
            dateOnDemand.Clear();
            cacheOnDemand.Clear();
            cacheFull.Clear();
        }
        #endregion
    }
}
