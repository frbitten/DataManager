using System;
using System.Collections.Generic;
using System.Reflection;
using Joo.Database.Attributes;
using Joo.Database.Exceptions;
using System.Windows;
using Joo.Database.Types;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq.Expressions;

namespace Joo.Database.Structs
{
    [Serializable]
    public class Where
    {
        #region [ Fields ]
        private List<Object> items;
        protected static Dictionary<Tuple<Type, string>, DatabasePropertyInfo> PropertyInfoDictionary = new Dictionary<Tuple<Type, string>, DatabasePropertyInfo>();
        #endregion [ Fields ]

        #region [ Constructors ]
        public Where()
        {
            items = new List<Object>();
        }
        #endregion

        #region [ Public Methods ]
        public void AddItem<T>(Expression<Func<T, object>> expression) where T : BasicModel
        {
            if (expression == null)
            {
                throw new ArgumentException("Expression invalid.");
            }

            switch (expression.Body.NodeType)
            {
                case ExpressionType.Convert:
                    {
                        UnaryExpression unary = expression.Body as UnaryExpression;
                        switch (unary.Operand.NodeType)
                        {
                            case ExpressionType.Call:
                                {
                                    MethodCallExpression call = unary.Operand as MethodCallExpression;
                                    this.Concat(ConvertCallExpression(call));
                                }
                                break;
                            case ExpressionType.MemberAccess:
                                {
                                    this.Concat(ConvertMemberExpression(unary.Operand as MemberExpression));
                                }
                                break;
                            default:
                                BinaryExpression binary = unary.Operand as BinaryExpression;
                                if (binary == null)
                                {
                                    throw new ArgumentException("Expression not suported.");
                                }
                                this.Concat(ConvertBynaryExpression(binary));
                                break;
                        }
                    }
                    break;
                case ExpressionType.MemberAccess:
                    {
                        Where where = ConvertMemberExpression(expression.Body as MemberExpression);
                        this.Concat(where);
                    }
                    break;
                default:
                    throw new ArgumentException("Expression not suported.");
            }


            //var propertyName = GetFullPropertyName(expression);
            //AddItem<T>(propertyName);
        }

        private Where ConvertCallExpression(MethodCallExpression expression)
        {
            Where where = new Where();

            switch (expression.Method.Name)
            {
                case "Contains":
                    switch (expression.Object.NodeType)
                    {
                        case ExpressionType.MemberAccess:
                            where.Concat(ConvertMemberExpression(expression.Object as MemberExpression));
                            break;
                        default:
                            throw new ArgumentException("Expression not suported.");
                    }
                    where.AddOperator(Operator.CONTAINS);

                    switch ((expression.Arguments[0] as System.Linq.Expressions.Expression).NodeType)
                    {
                        case ExpressionType.Constant:
                            where.AddItem((expression.Arguments[0] as ConstantExpression).Value);
                            break;
                        case ExpressionType.MemberAccess:
                            var member = expression.Arguments[0] as MemberExpression;
                            where.AddItem(System.Linq.Expressions.Expression.Lambda(expression.Arguments[0]).Compile().DynamicInvoke());
                            break;
                        default:
                            throw new ArgumentException("Expression not suported.");

                    }

                    break;
                default:
                    throw new ArgumentException("Expression not suported.");
            }

            return where;
        }

        private ConstantExpression GetConstantExpressionAndAcessor(MemberExpression member, out MemberInfo info)
        {
            if (member.Expression is ConstantExpression)
            {
                info = member.Member;
                return member.Expression as ConstantExpression;
            }

            if (member.Expression == null)
            {
                info = null;
                return null;
            }

            if (!(member.Expression is MemberExpression))
            {
                info = null;
                return null;
            }

            return GetConstantExpressionAndAcessor(member.Expression as MemberExpression, out info);
        }

        private Where ConvertBynaryExpression(BinaryExpression expression)
        {
            Where where = new Where();
            where.AddOperator(Operator.OPEN_PARENTHESIS);
            if (expression.NodeType == ExpressionType.Equal || expression.NodeType == ExpressionType.LessThan || expression.NodeType == ExpressionType.LessThanOrEqual || expression.NodeType == ExpressionType.GreaterThan || expression.NodeType == ExpressionType.GreaterThanOrEqual || expression.NodeType == ExpressionType.NotEqual)
            {
                switch (expression.Left.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        {
                            where.Concat(ConvertMemberExpression(expression.Left as MemberExpression));
                        }
                        break;
                    case ExpressionType.Constant:
                        {
                            ConstantExpression constant = expression.Left as ConstantExpression;
                            where.AddItem(constant.Value);
                        }
                        break;
                    case ExpressionType.Convert:
                        {
                            UnaryExpression unary = expression.Left as UnaryExpression;
                            switch (unary.Operand.NodeType)
                            {
                                case ExpressionType.MemberAccess:
                                    where.Concat(ConvertMemberExpression(unary.Operand as MemberExpression));
                                    break;
                                default:
                                    throw new ArgumentException("Expression not suported.");
                            }
                        }
                        break;
                    default:
                        throw new ArgumentException("Expression not suported.");
                }
                switch (expression.NodeType)
                {
                    case ExpressionType.Equal:
                        where.AddOperator(Operator.EQUAL);
                        break;
                    case ExpressionType.LessThan:
                        where.AddOperator(Operator.MINOR);
                        break;
                    case ExpressionType.LessThanOrEqual:
                        where.AddOperator(Operator.LESS_EQUAL);
                        break;
                    case ExpressionType.NotEqual:
                        where.AddOperator(Operator.DIFFERENT);
                        break;
                    case ExpressionType.GreaterThan:
                        where.AddOperator(Operator.MORE);
                        break;
                    case ExpressionType.GreaterThanOrEqual:
                        where.AddOperator(Operator.MORE_EQUAL);
                        break;
                    default:
                        throw new ArgumentException("Expression invalid.");
                }
                switch (expression.Right.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        where.Concat(ConvertMemberExpression(expression.Right as MemberExpression));
                        break;
                    case ExpressionType.Constant:
                        ConstantExpression constant = expression.Right as ConstantExpression;
                        if (where.Items[where.Items.Count - 2] is DatabaseFieldInfo && constant.Value == null)
                        {
                            DatabaseFieldInfo info = where.Items[where.Items.Count - 2] as DatabaseFieldInfo;
                            if (info.ElementType == typeof(int))
                            {
                                where.AddItem(0);
                            }
                            else
                            {
                                if (info.ElementType == typeof(string))
                                {
                                    where.AddItem(String.Empty);
                                }
                                else
                                {
                                    throw new ArgumentException("Expression not suported.");
                                }
                            }
                        }
                        else
                        {
                            where.AddItem(constant.Value);
                        }
                        break;
                    case ExpressionType.Call:
                        object obj;
                        if (TryGetValue((expression.Right as MethodCallExpression), out obj))
                        {
                            if (where.Items.Count == 1)
                                where.Items.Clear();

                            where.AddItem(obj);
                        }
                        else
                        {
                            throw new ArgumentException("Expression not suported.");
                        }
                        break;
                    default:
                        throw new ArgumentException("Expression not suported.");
                }
            }
            else
            {
                if (expression.NodeType == ExpressionType.AndAlso || expression.NodeType == ExpressionType.OrElse)
                {
                    switch (expression.Left.NodeType)
                    {
                        case ExpressionType.Call:
                            where.Concat(ConvertCallExpression(expression.Left as MethodCallExpression));
                            break;
                        default:
                            where.Concat(ConvertBynaryExpression(expression.Left as BinaryExpression));
                            break;
                    }

                    switch (expression.NodeType)
                    {
                        case ExpressionType.AndAlso:
                            where.AddOperator(Operator.AND);
                            break;
                        case ExpressionType.OrElse:
                            where.AddOperator(Operator.OR);
                            break;
                        default:
                            throw new ArgumentException("Expression not suported.");
                    }
                    switch (expression.Right.NodeType)
                    {
                        case ExpressionType.Call:
                            where.Concat(ConvertCallExpression(expression.Right as MethodCallExpression));
                            break;
                        default:
                            where.Concat(ConvertBynaryExpression(expression.Right as BinaryExpression));
                            break;
                    }

                }
                else
                {
                    throw new ArgumentException("Expression not suported.");
                }
            }

            where.AddOperator(Operator.CLOSE_PARENTHESIS);
            return where;
        }


        private Where ConvertMemberExpression(MemberExpression expression)
        {
            Where where = new Where();

            if (expression.Expression == null)
            {
                switch (expression.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        object obj;
                        if (TryGetValue(expression, out obj))
                        {
                            where.AddItem(obj);
                            return where;
                        }
                        break;
                }
            }
            else
            {
                where.AddOperator(Operator.OPEN_PARENTHESIS);
                bool finish = false;
                MemberExpression memberExpression = expression;
                while (!finish)
                {
                    switch (memberExpression.Expression.NodeType)
                    {
                        case ExpressionType.MemberAccess:
                            if (where.Items.Count > 1)
                            {
                                where.AddOperator(Operator.AND);
                            }

                            object obj;
                            if (TryGetValue((memberExpression.Expression as MemberExpression), out obj))
                            {
                                if (where.Items.Count == 1)
                                    where.Items.Clear();

                                where.AddItem(obj);
                                return where;
                            }
                            else
                            {
                                memberExpression = memberExpression.Expression as MemberExpression;
                                object[] attributes = memberExpression.Member.GetCustomAttributes(typeof(RelationshipAttribute), true);
                                if (attributes.Length <= 0)
                                {
                                    throw new ArgumentException("Expression not suported. Propriedade " + expression.Member.Name + " não é um campo de base de dados");
                                }
                                RelationshipAttribute attribute = (RelationshipAttribute)attributes[0];
                                where.AddItem(memberExpression.Member.ReflectedType, attribute.FieldName);
                                where.AddOperator(Operator.EQUAL);
                                where.AddItem(memberExpression.Type, "ID");
                            }
                            break;
                        case ExpressionType.Parameter:
                            finish = true;
                            break;
                        case ExpressionType.Constant:
                            ConstantExpression constant = memberExpression.Expression as ConstantExpression;
                            where.Items.Clear();
                            where.AddItem((memberExpression.Member as FieldInfo).GetValue(constant.Value));
                            return where;
                        default:
                            throw new ArgumentException("Expression not suported.");
                    }
                }
                if (where.Items.Count > 1)
                {
                    where.AddOperator(Operator.CLOSE_PARENTHESIS);
                    where.AddOperator(Operator.AND);
                }
                else
                {
                    where.Items.Clear();
                }
                if (expression.Type.IsSubclassOf(typeof(BasicModel)))
                {
                    //object[] attributes = memberExpression.Member.GetCustomAttributes(typeof(RelationshipAttribute), true);
                    //if (attributes.Length > 0)
                    //{
                    //    RelationshipAttribute attribute = (RelationshipAttribute)attributes[0];
                    //    where.AddItem(memberExpression.Member.ReflectedType, attribute.FieldName);
                    //}
                    //else
                    //{
                        //throw new ArgumentException("Expression not suported. Propriedade " + expression.Member.Name + " não é um campo de base de dados");
                    //}                    
                    throw new ArgumentException("Expression não suportada. Propriedade " + expression.Member.Name + " não pode ser um campo de relação");
                }
                else
                {
                    object[] attributes = expression.Member.GetCustomAttributes(typeof(FieldAttribute),true);
                    if (attributes.Length > 0)
                    {
                        FieldAttribute attribute = (FieldAttribute)attributes[0];
                        where.AddItem(expression.Expression.Type, attribute.Name);
                    }
                    else
                    {
                        throw new ArgumentException("Expression not suported. Propriedade " + expression.Member.Name + " não é um campo de base de dados");
                    }
                }                
            }
            return where;
        }

        private bool TryGetValue(System.Linq.Expressions.Expression member, out object result)
        {
            try
            {
                var objectMember = System.Linq.Expressions.Expression.Convert(member, typeof(object));
                var getterLambda = System.Linq.Expressions.Expression.Lambda<Func<object>>(objectMember);
                var getter = getterLambda.Compile();
                result = getter();
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        public void AddItem<T>(string propertyName) where T : BasicModel
        {
            propertyName = propertyName.ToLower();
            Type typeT = typeof(T);
            if (!typeT.IsSubclassOf(typeof(BasicModel)))
            {
                throw new ArgumentException("O Tipo " + typeT.Name + " não herda BasicModel");
            }
            Tuple<Type, string> tuple = new Tuple<Type, string>(typeT, propertyName);

            if (PropertyInfoDictionary.ContainsKey(tuple))
            {
                AddItemInfo(PropertyInfoDictionary[tuple]);
            }
            else
            {
                var propertyInfo = TypesManager.TypeOf(typeT).GetPropertyInfo(propertyName);

                if (propertyInfo == null)
                    throw new ArgumentException("A propriedade " + propertyName + " não foi encontrada no objeto " + typeT.Name);

                PropertyInfoDictionary.Add(tuple, propertyInfo);
                AddItemInfo(propertyInfo);
            }

        }

        public void AddItem(Type type, string propertyName)
        {
            if (!type.IsSubclassOf(typeof(BasicModel)))
            {
                throw new ArgumentException("O Tipo " + type.Name + " não herda BasicModel");
            }
            Tuple<Type, string> tuple = new Tuple<Type, string>(type, propertyName);
            if (PropertyInfoDictionary.ContainsKey(tuple))
            {
                AddItemInfo(PropertyInfoDictionary[tuple]);
            }
            else
            {
                var propertyInfo = TypesManager.TypeOf(type).GetPropertyInfo(propertyName);
                if (propertyInfo == null)
                    throw new ArgumentException("A propriedade " + propertyName + " não foi encontrada no objeto " + type.Name);
                PropertyInfoDictionary.Add(tuple, propertyInfo);
                AddItemInfo(propertyInfo);
            }
        }

        public void AddItemInfo(Joo.Database.Types.DatabasePropertyInfo info)
        {
            if (info == null)
            {
                throw new ArgumentException("Info is null");
            }
            if (info.IsFieldAttribute == false)
            {
                throw new ArgumentException("Not found FieldAttribute in PropertyInfo.");
            }
            else
            {
                items.Add(info);
            }
        }

        /// <summary>
        /// Adds the item. (PropertyInfo || Operator)
        /// </summary>
        /// <param name="item">The item. (PropertyInfo || Operator)</param>
        public void AddItem(Object item)
        {
            items.Add(item);
        }

        /// <summary>
        /// Adds the operator.
        /// </summary>
        /// <param name="oper">The oper.</param>
        public void AddOperator(Operator oper)
        {
            items.Add(oper);
        }

        /// <summary>
        /// Gets the items.
        /// </summary>
        /// <value>The items.</value>
        public List<Object> Items
        {
            get { return items; }
        }

        public virtual Where Clone()
        {
            var stream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, this);
            byte[] buffer = stream.GetBuffer();
            stream.Close();
            stream = new MemoryStream(buffer);
            Where ret = formatter.Deserialize(stream) as Where;
            stream.Close();
            return ret;
        }

        public virtual void Concat(Where where)
        {
            this.Items.AddRange(where.Items);
        }
        #endregion


        #region [ Expresssion Converter ]
        public string GetName<TSource, TField>(Expression<Func<TSource, TField>> Field)
        {
            return (Field.Body as MemberExpression).Member.Name;
        }

        // code adjusted to prevent horizontal overflow
        static string GetFullPropertyName<T, TProperty>
        (Expression<Func<T, TProperty>> exp)
        {
            MemberExpression memberExp;
            if (!TryFindMemberExpression(exp.Body, out memberExp))
                return string.Empty;

            var memberNames = new Stack<string>();
            do
            {
                memberNames.Push(memberExp.Member.Name);
            }
            while (TryFindMemberExpression(memberExp.Expression, out memberExp));

            return string.Join(".", memberNames.ToArray());
        }

        // code adjusted to prevent horizontal overflow
        static bool TryFindMemberExpression
        (System.Linq.Expressions.Expression exp, out MemberExpression memberExp)
        {
            memberExp = exp as MemberExpression;
            if (memberExp != null)
            {
                // heyo! that was easy enough
                return true;
            }

            // if the compiler created an automatic conversion,
            // it'll look something like...
            // obj => Convert(obj.Property) [e.g., int -> object]
            // OR:
            // obj => ConvertChecked(obj.Property) [e.g., int -> long]
            // ...which are the cases checked in IsConversion
            if (IsConversion(exp) && exp is UnaryExpression)
            {
                memberExp = ((UnaryExpression)exp).Operand as MemberExpression;
                if (memberExp != null)
                {
                    return true;
                }
            }

            return false;
        }

        static bool IsConversion(System.Linq.Expressions.Expression exp)
        {
            return (
                exp.NodeType == ExpressionType.Convert ||
                exp.NodeType == ExpressionType.ConvertChecked
            );
        }
        #endregion

    }
}