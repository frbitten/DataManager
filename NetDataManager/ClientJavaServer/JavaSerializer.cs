using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Joo.Database;
using System.Xml;
using Joo.Database.Types;
using System.Reflection;
using System.Globalization;
using System.IO;
using System.IO.Compression;

namespace Joo.Server
{
    public static class JavaSerializer
    {
        private static String TAG_RELATIONSHIP_ONE_TO_ONE = "relationshipsOneToOne";
        private static String TAG_RELATIONSHIP_ONE_TO_MANY = "relationshipsOneToMany";
        private static String TAG_FIELDS = "fields";
        private static String TAG_MODEL = "model";
        private static String TAG_CHILDREN = "children";
        private static String TAG_CLASS = "class";
        private static String TAG_ID = "ID";
        private static String TAG_FIELD_NAME = "fieldName";
        private static String TAG_RELATIONSHIP_ITEM = "item";
        private static String TAG_STATUS = "status";

        public static byte[] Serializer(BasicModel model)
        {
            String xml = SerializerToXML(model);
            if (xml != null)
            {
                byte[] data=System.Text.Encoding.UTF8.GetBytes(xml);
                return Compress(data);
            }
            return null;
        }
        public static String SerializerToXML(BasicModel model)
        {
            List<BasicModel> list = new List<BasicModel>();
            List<int> isSerialized = new List<int>();
            StringBuilder builder = new StringBuilder();
            XmlWriter xmlSerializer = XmlWriter.Create(builder);
            xmlSerializer.WriteStartDocument();
            xmlSerializer.WriteStartElement("serializer");
            list = SerializeModel(model, xmlSerializer);
            isSerialized.Add(model.GetHashCode());
            xmlSerializer.WriteStartElement(TAG_CHILDREN);
            while (list.Count() > 0)
            {
                BasicModel removed = list[0];
                list.RemoveAt(0);
                bool find = false;
                foreach (int id in isSerialized)
                {
                    if (id == removed.GetHashCode())
                    {
                        find = true;
                        break;
                    }
                }
                if (find)
                {
                    continue;
                }
                isSerialized.Add(removed.GetHashCode());
                list.AddRange(SerializeModel(removed, xmlSerializer));
            }
            xmlSerializer.WriteEndElement();
            xmlSerializer.WriteEndElement();
            xmlSerializer.WriteEndDocument();
            xmlSerializer.Flush();
            xmlSerializer.Close();
            return builder.ToString();
        }

        private static String GetStringByValue(DatabaseFieldInfo info, BasicModel model)
        {
            Object value = info.Property.GetValue(model, null);
            if (value == null)
            {
                return null;
            }
            if (typeof(String).IsInstanceOfType(value))
            {
                return (String)value;
            }
            if (typeof(int).IsInstanceOfType(value))
            {
                return value.ToString();
            }
            if (typeof(short).IsInstanceOfType(value))
            {
                return value.ToString();
            }
            if (typeof(Int16).IsInstanceOfType(value))
            {
                return value.ToString();
            }
            if (typeof(Int64).IsInstanceOfType(value))
            {
                return value.ToString();
            }
            if (typeof(decimal).IsInstanceOfType(value))
            {
                CultureInfo culture = new CultureInfo("en-US");
                return ((Decimal)value).ToString("N4", culture.NumberFormat);
            }
            if (typeof(DateTime).IsInstanceOfType(value))
            {
                return ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");
            }
            if (typeof(byte[]).Equals(info.ElementType))
            {
                return Convert.ToBase64String((byte[])value);
            }

            return null;
        }

        private static Object GetValueByString(DatabaseFieldInfo info, string value)
        {

            if (typeof(String).Equals(info.ElementType))
            {
                return (String)value;
            }
            if (typeof(int).Equals(info.ElementType) )
            {
                return int.Parse(value);
            }
            if (typeof(Int16).Equals(info.ElementType))
            {
                return Int16.Parse(value);
            }
            if (typeof(Int64).Equals(info.ElementType))
            {
                return Int64.Parse(value);
            }
            if (typeof(Int32).Equals(info.ElementType))
            {
                return Int32.Parse(value);
            }
            if (typeof(short).Equals(info.ElementType))
            {
                return short.Parse(value);
            }
            if (typeof(decimal).Equals(info.ElementType))
            {
                CultureInfo culture = new CultureInfo("en-US");
                return Decimal.Parse(value, culture.NumberFormat);
            }
            if (typeof(DateTime).Equals(info.ElementType))
            {
                return DateTime.Parse(value);
            }
            if (typeof(byte[]).Equals(info.Property.PropertyType))
            {
                if (value.Length > 12)
                {
                    value = value.Substring(9, value.Count() - 12);
                    return Convert.FromBase64String(value);
                }
                else
                {
                    return new byte[0];
                }
            }

            throw new ArgumentException("O tipo "+info.ElementType.Name+" não é suportado.");
        }

        private static List<BasicModel> SerializeModel(BasicModel model, XmlWriter xmlSerializer)
        {
            List<BasicModel> ret = new List<BasicModel>();
            xmlSerializer.WriteStartElement(TAG_MODEL);
            xmlSerializer.WriteElementString(TAG_CLASS, model.GetType().FullName);
            xmlSerializer.WriteElementString(TAG_ID, model.GetHashCode().ToString());
            xmlSerializer.WriteStartElement(TAG_FIELDS);
            DatabaseType type = TypesManager.TypeOf(model);
            foreach (DatabaseFieldInfo info in type.DataBaseProperties)
            {
                String value = GetStringByValue(info, model);
                if (value != null)
                {
                    xmlSerializer.WriteElementString(info.Attribute.Name, value);
                }
            }
            switch (model.Status)
            {
                case Status.Delete:
                    xmlSerializer.WriteElementString(TAG_STATUS, "delete");
                    break;
                case Status.New:
                    xmlSerializer.WriteElementString(TAG_STATUS, "new");
                    break;
                case Status.Normal:
                    xmlSerializer.WriteElementString(TAG_STATUS, "normal");
                    break;
                case Status.Update:
                    xmlSerializer.WriteElementString(TAG_STATUS, "update");
                    break;
                case Status.Invalid:
                    xmlSerializer.WriteElementString(TAG_STATUS, "invalid");
                    break;
            }
            xmlSerializer.WriteEndElement();
            xmlSerializer.WriteStartElement(TAG_RELATIONSHIP_ONE_TO_ONE);
            foreach (DatabaseRelationshipInfo info in type.DataBaseRelationship)
            {
                if (info.Property.PropertyType.IsArray)
                {
                    continue;
                }
                BasicModel rela = (BasicModel)info.FastGetValue(model);
                if (rela == null)
                {
                    continue;
                }
                xmlSerializer.WriteStartElement(info.Property.Name);
                xmlSerializer.WriteElementString(TAG_CLASS, info.ElementType.FullName);
                xmlSerializer.WriteElementString(TAG_FIELD_NAME, info.Attribute.FieldName);
                xmlSerializer.WriteElementString(TAG_ID, rela.GetHashCode().ToString());
                xmlSerializer.WriteEndElement();
                ret.Add(rela);

            }
            xmlSerializer.WriteEndElement();
            xmlSerializer.WriteStartElement(TAG_RELATIONSHIP_ONE_TO_MANY);
            foreach (DatabaseRelationshipInfo info in type.DataBaseRelationship)
            {
                if (!info.Property.PropertyType.IsArray)
                {
                    continue;
                }
                BasicModel[] items = info.FastGetValue(model) as BasicModel[];
                if (items == null)
                {
                    continue;
                }
                xmlSerializer.WriteStartElement(info.Property.Name);
                foreach (BasicModel item in items)
                {
                    xmlSerializer.WriteStartElement(TAG_RELATIONSHIP_ITEM);
                    xmlSerializer.WriteElementString(TAG_CLASS, item.GetType().FullName);
                    xmlSerializer.WriteElementString(TAG_FIELD_NAME, info.Attribute.FieldName);
                    xmlSerializer.WriteElementString(TAG_ID, item.GetHashCode().ToString());
                    ret.Add(item);
                    xmlSerializer.WriteEndElement();
                }
                xmlSerializer.WriteEndElement();
            }
            xmlSerializer.WriteEndElement();
            xmlSerializer.WriteEndElement();
            return ret;
        }


        public static T Deserialize<T>(byte[] buffer, String assemblyFullName) where T : BasicModel
        {
            byte[] data = Decompress(buffer);
            String xml = System.Text.Encoding.UTF8.GetString(data);
            return DeserializeByXML<T>(xml,assemblyFullName);
        }

        public static T DeserializeByXML<T>(String xml, String assemblyFullName) where T : BasicModel
        {
            Dictionary<String, Dictionary<int, BasicModel>> cache = new Dictionary<String, Dictionary<int, BasicModel>>();
            List<Object[]> cacheRalationship = new List<Object[]>();
            XmlReader parser = XmlReader.Create(new StringReader(xml));
            parser.ReadStartElement();
            T ret = (T)loadModel(parser, cache, assemblyFullName, cacheRalationship);
            if (!parser.IsEmptyElement)
            {
                parser.ReadStartElement();
                while (parser.NodeType!=XmlNodeType.EndElement && parser.Name!=TAG_CHILDREN)
                {
                    loadModel(parser, cache, assemblyFullName,cacheRalationship);
                }
                parser.ReadEndElement();
            }
            else
            {
                parser.ReadStartElement();
            }
            parser.ReadEndElement();
            refreshRelations(cacheRalationship);
            return ret;
        }
        private static void refreshRelations(List<Object[]> cacheRalationship)
        {
            foreach (Object[] entry in cacheRalationship)
            {
                BasicModel model = (BasicModel)entry[1];
                model.OnStartSerializing();
                ((DatabasePropertyInfo)entry[0]).FastSetValue(model, entry[2]);
                model.onSerialized();
            }
        }
        //private static void refreshRelationIds(BasicModel model, Dictionary<Type, List<int>> cache)
        //{
        //    if (cache == null)
        //    {
        //        cache = new Dictionary<Type, List<int>>();
        //    }

        //    if (!cache.ContainsKey(model.GetType()))
        //    {
        //        cache.Add(model.GetType(), new List<int>());
        //    }
        //    cache[model.GetType()].Add(model.ID);

        //    Status status = model.Status;
        //    DatabaseType dataType = TypesManager.TypeOf(model.GetType());
        //    foreach (DatabaseRelationshipInfo relation in dataType.DataBaseRelationship)
        //    {
                 
        //        if (relation.Property.PropertyType.IsArray)
        //        {
        //            BasicModel[] items = relation.FastGetValue(model) as BasicModel[];
        //            foreach (BasicModel item in items)
        //            {
        //                if (!cache.ContainsKey(item.GetType()))
        //                {
        //                    refreshRelationIds(item,cache);
        //                }
        //                else
        //                {
        //                    if (!cache[item.GetType()].Contains(item.ID))
        //                    {
        //                        refreshRelationIds(item, cache);
        //                    }
        //                }
        //            }
        //            continue;
        //        }
        //        BasicModel obj = (BasicModel)relation.FastGetValue(model);
        //        if (obj == null)
        //        {
        //            continue;
        //        }

        //        if (!cache.ContainsKey(obj.GetType()))
        //        {
        //            refreshRelationIds(obj, cache);
        //        }
        //        else
        //        {
        //            if (!cache[obj.GetType()].Contains(obj.ID))
        //            {
        //                refreshRelationIds(obj, cache);
        //            }
        //        }
        //        DatabasePropertyInfo info = dataType.GetPropertyInfo(relation.Attribute.FieldName);
        //        if (info != null)
        //        {
        //            info.FastSetValue(model, obj.ID);
        //        }
        //    }
        //    model.Status = status;
        //}

        private static BasicModel loadModel(XmlReader parser, Dictionary<String, Dictionary<int, BasicModel>> cache, String assemblyFullName, List<Object[]> cacheRalationship)
        {
            parser.ReadStartElement(TAG_MODEL);
            String className = parser.ReadElementString(TAG_CLASS);
            int id = int.Parse(parser.ReadElementString(TAG_ID));

            BasicModel o = null;
            //check cache para ver se o objeto ja foi criado
            if (cache.ContainsKey(className))
            {
                Dictionary<int, BasicModel> objs = cache[className];
                if (objs.ContainsKey(id))
                {
                    o = objs[id];
                }
            }
            else
            {
                Dictionary<int, BasicModel> map = new Dictionary<int, BasicModel>();
                cache.Add(className, map);
            }
            if (o == null)
            {
                Type type = Type.GetType(className+", "+assemblyFullName);
                if (type != null)
                {
                    o = (BasicModel)Activator.CreateInstance(type);
                    Dictionary<int, BasicModel> map = cache[className];
                    map.Add(id, o);
                }
                else
                {
                    throw new ArgumentException("Não foi possivel achar o objeto de tipo "+className+" no assembly "+assemblyFullName+". Verifique se namespace e assembly estão corretos.");
                }
            }
            o.OnStartSerializing();
            LoadFields(parser, o);
            Status status = o.Status;

            LoadRelationshipOneToOne(parser, o, cache,cacheRalationship);

            LoadRelationshipOneToMany(parser, o, cache,cacheRalationship);

            o.Status = status;
            o.onSerialized();
            parser.ReadEndElement();
            return o;

        }
        private static void LoadRelationshipOneToMany(XmlReader parser, BasicModel model, Dictionary<String, Dictionary<int, BasicModel>> cache, List<Object[]> cacheRalationship)
        {
            if (parser.IsEmptyElement)
            {
                parser.ReadStartElement(TAG_RELATIONSHIP_ONE_TO_MANY);
                return;
            }
            parser.ReadStartElement();
            while (parser.Name != TAG_RELATIONSHIP_ONE_TO_MANY)
            {
                String propertyName = parser.Name;
                if (parser.IsEmptyElement)
                {
                    parser.ReadStartElement();
                    continue;
                }
                List<BasicModel> items = new List<BasicModel>();
                parser.ReadStartElement();
                while (parser.NodeType == XmlNodeType.Element && parser.Name == TAG_RELATIONSHIP_ITEM)
                {
                    parser.ReadStartElement();
                    String className = parser.ReadElementString(TAG_CLASS);
                    String fieldName = parser.ReadElementString(TAG_FIELD_NAME);
                    int id = int.Parse(parser.ReadElementString(TAG_ID));
                    parser.ReadEndElement(); // fecha TAG_RELATIONSHIP_ITEM
                    if (className == null || className == "")
                    {
                        throw new ArgumentException("xml formato invalido. Não foi encontrada a tag <" + TAG_CLASS + "> dentro da tag <" + TAG_RELATIONSHIP_ITEM + ">.");
                    }
                    if (fieldName == null || fieldName == "")
                    {
                        throw new ArgumentException("xml formato invalido. Não foi encontrada a tag <" + TAG_FIELD_NAME + "> dentro da tag <" + TAG_RELATIONSHIP_ITEM + ">.");
                    }
                    if (id == 0)
                    {
                        throw new ArgumentException("xml formato invalido. Não foi encontrada a tag <" + TAG_ID + "> dentro da tag <" + TAG_RELATIONSHIP_ITEM + ">.");
                    }

                    BasicModel o = null;
                    //check cache para ver se o objeto ja foi criado
                    if (cache.ContainsKey(className))
                    {
                        Dictionary<int, BasicModel> objs = cache[className];
                        if (objs.ContainsKey(id))
                        {
                            o = objs[id];
                        }
                    }
                    else
                    {
                        Dictionary<int, BasicModel> map = new Dictionary<int, BasicModel>();
                        cache.Add(className, map);
                    }
                    if (o == null)
                    {
                        Type type = Type.GetType(className + ", " + model.GetType().Assembly.FullName);
                        if (type != null)
                        {
                            o = (BasicModel)Activator.CreateInstance(type);
                            Dictionary<int, BasicModel> map = cache[className];
                            map.Add(id, o);
                        }
                        else
                        {
                            throw new ArgumentException("Não foi possivel achar o objeto de tipo " + className + " no assembly " + model.GetType().Assembly.FullName + ". Verifique se namespace e assembly estão corretos.");
                        }
                    }
                    items.Add(o);
                }
                parser.ReadEndElement(); // fecha a propriedade
                DatabaseType dataType = TypesManager.TypeOf(model.GetType());
                DatabasePropertyInfo info = dataType.GetPropertyInfo(propertyName);
                
                object[] newArray = Array.CreateInstance(info.ElementType, items.Count) as object[];

                for (int i = 0; i < items.Count; i++)
                {
                    newArray[i] = items[i];
                }
                //info.FastSetValue(model, newArray);
                cacheRalationship.Add(new Object[] { info,model, newArray });

            }
            parser.ReadEndElement(); // fecha o TAG_RELATIONSHIP_ONE_TO_MANY
        }

        private static void LoadRelationshipOneToOne(XmlReader parser, BasicModel model, Dictionary<String, Dictionary<int, BasicModel>> cache, List<Object[]> cacheRalationship)
        {
            if (parser.IsEmptyElement)
            {
                parser.ReadStartElement(TAG_RELATIONSHIP_ONE_TO_ONE);
                return;
            }
            parser.ReadStartElement();
            while (parser.Name != TAG_RELATIONSHIP_ONE_TO_ONE)
            {
                String propertyName = parser.Name;
                if (parser.IsEmptyElement)
                {
                    parser.ReadStartElement();
                    continue;
                }
                parser.ReadStartElement();
                String className = parser.ReadElementString(TAG_CLASS);
                String fieldName = parser.ReadElementString(TAG_FIELD_NAME);
                int id = int.Parse(parser.ReadElementString(TAG_ID));
                parser.ReadEndElement();
                if (className == null || className == "")
                {
                    throw new ArgumentException("xml formato invalido. Não foi encontrada a tag <" + TAG_CLASS + "> dentro da tag <" + TAG_RELATIONSHIP_ITEM + ">.");
                }
                if (fieldName == null || fieldName == "")
                {
                    throw new ArgumentException("xml formato invalido. Não foi encontrada a tag <" + TAG_FIELD_NAME + "> dentro da tag <" + TAG_RELATIONSHIP_ITEM + ">.");
                }
                if (id == 0)
                {
                    throw new ArgumentException("xml formato invalido. Não foi encontrada a tag <" + TAG_ID + "> dentro da tag <" + TAG_RELATIONSHIP_ITEM + ">.");
                }

                BasicModel o = null;
                //check cache para ver se o objeto ja foi criado
                if (cache.ContainsKey(className))
                {
                    Dictionary<int, BasicModel> objs = cache[className];
                    if (objs.ContainsKey(id))
                    {
                        o = objs[id];
                    }
                }
                else
                {
                    Dictionary<int, BasicModel> map = new Dictionary<int, BasicModel>();
                    cache.Add(className, map);
                }
                if (o == null)
                {
                    Type type = Type.GetType(className + ", " + model.GetType().Assembly.FullName);
                    if (type != null)
                    {
                        o = (BasicModel)Activator.CreateInstance(type);
                        Dictionary<int, BasicModel> map = cache[className];
                        map.Add(id, o);
                    }
                    else
                    {
                        throw new ArgumentException("Não foi possivel achar o objeto de tipo " + className + " no assembly " + model.GetType().Assembly.FullName + ". Verifique se namespace e assembly estão corretos.");
                    }
                }
                DatabaseType dataType = TypesManager.TypeOf(model.GetType());
                DatabasePropertyInfo info = dataType.GetPropertyInfo(propertyName);
                //info.FastSetValue(model, o);
                cacheRalationship.Add(new Object[] { info,model, o });
            }
            parser.ReadEndElement(); // fecha o TAG_RELATIONSHIP_ONE_TO_ONE
        }
        private static void LoadFields(XmlReader parser, BasicModel model)
        {
            parser.ReadStartElement(TAG_FIELDS);
            //parser.ReadStartElement();
            if (parser.IsEmptyElement)
            {
                return;
            }
            while (parser.Name != TAG_FIELDS)
            {
                String name = parser.Name;
                String value = parser.ReadElementString();
                if (name == TAG_STATUS)
                {
                    switch (value)
                    {
                        case "update":
                            model.Status = Status.Update;
                            break;
                        case "new":
                            model.Status = Status.New;
                            break;
                        case "normal":
                            model.Status = Status.Normal;
                            break;
                        case "delete":
                            model.Status = Status.Delete;
                            break;
                        case "invalid":
                            model.Status = Status.Invalid;
                            break;
                    }
                }
                else
                {
                    DatabaseType dataType = TypesManager.TypeOf(model.GetType());
                    DatabasePropertyInfo info = dataType.GetPropertyInfo(name);
                    info.FastSetValue(model, GetValueByString((DatabaseFieldInfo)info, value));
                }
            }
            parser.ReadEndElement();
        }
        public static byte[] Compress(byte[] data)
        {
            using (var compressedStream = new MemoryStream())
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
            {
                zipStream.Write(data, 0, data.Length);
                zipStream.Close();
                return compressedStream.ToArray();
            }
        }

        public static byte[] Decompress(byte[] data)
        {
            using (var compressedStream = new MemoryStream(data))
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (var resultStream = new MemoryStream())
            {

                var buffer = new byte[4096];
                int read;

                while ((read = zipStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    resultStream.Write(buffer, 0, read);
                }

                return resultStream.ToArray();
            }
        }
    }
}
