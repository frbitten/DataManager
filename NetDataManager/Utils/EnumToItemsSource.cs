using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.ComponentModel;

namespace Utils
{
    public class EnumToItemsSource : MarkupExtension
    {
        private Type _enumType;

        public EnumToItemsSource(Type enumType)
        {
            if (enumType == null)
                throw new ArgumentNullException("enumType");

            EnumType = enumType;
        }

        public Type EnumType
        {
            get { return _enumType; }
            private set
            {
                if (_enumType == value)
                    return;

                var enumType = Nullable.GetUnderlyingType(value) ?? value;

                if (enumType.IsEnum == false)
                    throw new ArgumentException("Type must be an Enum.");

                _enumType = value;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var enumValues = System.Enum.GetValues(EnumType);

            return (
              from object enumValue in enumValues
              select new EnumerationMember
              {
                  Value = enumValue,
                  Description = GetDescription(enumValue)
              }).ToArray();
        }

        private string GetDescription(object enumValue)
        {
            var descriptionAttribute = EnumType
              .GetField(enumValue.ToString())
              .GetCustomAttributes(typeof(DescriptionAttribute), false)
              .FirstOrDefault() as DescriptionAttribute;


            return descriptionAttribute != null
              ? descriptionAttribute.Description
              : enumValue.ToString();
        }

        public class EnumerationMember
        {
            public string Description { get; set; }
            public object Value { get; set; }
        }
    }

    public class Type2Extension : System.Windows.Markup.TypeExtension
    {
        public Type2Extension()
        {
        }

        public Type2Extension(string typeName)
        {
            base.TypeName = typeName;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            IXamlTypeResolver typeResolver = (IXamlTypeResolver)serviceProvider.GetService(typeof(IXamlTypeResolver));
            int sepindex = TypeName.IndexOf('+');
            if (sepindex < 0)
                return typeResolver.Resolve(TypeName);
            else
            {
                Type outerType = typeResolver.Resolve(TypeName.Substring(0, sepindex));
                return outerType.Assembly.GetType(outerType.FullName + "+" + TypeName.Substring(sepindex + 1));
            }
        }
    }
}
