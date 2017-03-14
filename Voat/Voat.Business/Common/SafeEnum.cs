using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.ModelBinding;

namespace Voat.Common
{
    /// <summary>
    /// Allows only valid defined enum values into underlying enumeration. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [System.Diagnostics.DebuggerDisplay("{Value}")]
    [TypeConverter(typeof(SafeEnumConverter))]
    public class SafeEnum<T> where T : struct, IConvertible
    {
        private T _value;

        static SafeEnum()
        {
            if (!typeof(T).IsEnum)
            {
                throw new TypeLoadException(String.Format("{0} is not an Enum type", typeof(T).FullName));
            }
        }

        public SafeEnum(T value)
        {
            Value = value;
        }
        public SafeEnum(int value)
        {
            Value = (T)(object)value;
        }
        public SafeEnum(string value)
        {
            Value = (T)Enum.Parse(typeof(T), value);
        }
        public Type Type
        {
            get
            {
                return typeof(T);
            }
        }
        public T Value
        {
            get {
                return _value;
            }
            set {
                if (!Extensions.IsValidEnumValue<T>(value))
                {
                    var message = String.Format("A value of {0} is not defined in type {1}", value, typeof(T).Name);
                    var exception = new ArgumentOutOfRangeException(nameof(value), value, message);
                    throw exception;
                }
                _value = value;
            }
        }
        public static implicit operator Enum(SafeEnum<T> value)
        {
            return (Enum)(object)value._value;
        }
        public static implicit operator T(SafeEnum<T> value)
        {
            return value._value;
        }
        public static implicit operator SafeEnum<T>(T value)
        {
            return new SafeEnum<T>(value);
        }
        public static implicit operator SafeEnum<T>(int value)
        {
            return new SafeEnum<T>(value);
        }
        public static implicit operator SafeEnum<T>(string value)
        {
            return new SafeEnum<T>(value);
        }
        public override string ToString()
        {
            return _value.ToString();
        }
    }

    public class SafeEnumConverter : TypeConverter
    {
        private Type _type = null;

        public SafeEnumConverter(Type type)
        {
            _type = type;
        }
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return base.CanConvertTo(context, destinationType);
        }
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (_type != null && (sourceType == typeof(string) || sourceType == typeof(int)));
        }
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value != null)
            {
                try
                {
                    var constructor = _type.GetConstructor(new Type[] { value.GetType() });
                    var o = constructor.Invoke(new object[] { value });
                    return o;
                }
                catch (Exception ex) {
                    throw ex;
                }
            }
            return null;
        }
    }

}
