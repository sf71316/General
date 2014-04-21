using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Data
{
    public class TypeConverterFactory
    {
        public static ITypeConverter GetConvertType<T>()
        {
            if (typeof(T) == typeof(int))
                return (new IntegerConverter());
            if (typeof(T) == typeof(long))
                return (new LongConverter());
            if (typeof(T) == typeof(short))
                return (new ShortConverter());
            if (typeof(T) == typeof(float))
                return (new FloatConverter());
            if (typeof(T) == typeof(double))
                return (new DoubleConverter());
            if (typeof(T) == typeof(decimal))
                return (new DecimalConverter());
            if (typeof(T) == typeof(bool))
                return (new BooleanConverter());
            if (typeof(T) == typeof(char))
                return (new CharConverter());
            if (typeof(T) == typeof(DateTime))
                return (new DateTimeConverter());
            if (typeof(T) == typeof(string))
                return (new StringConverter());
            if (typeof(T) == typeof(Guid))
                return (new GuildConverter());
            if (typeof(T) == typeof(System.Byte[]))
                return new ByteConverter();
            if (typeof(T).IsEnum)
                return (new EnumConverter());
            if (typeof(T).IsGenericType &&
                typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>))
                return new NullableConverter();
            return null;
        }

        public static ITypeConverter GetConvertType(Type T)
        {
            
            if (T == typeof(int))
                return (new IntegerConverter());
            if (T == typeof(long))
                return (new LongConverter());
            if (T == typeof(short))
                return (new ShortConverter());
            if (T == typeof(float))
                return (new FloatConverter());
            if (T == typeof(double))
                return (new DoubleConverter());
            if (T == typeof(decimal))
                return (new DecimalConverter());
            if (T == typeof(bool))
                return (new BooleanConverter());
            if (T == typeof(char))
                return (new CharConverter());
            if (T == typeof(DateTime))
                return (new DateTimeConverter());
            if (T == typeof(string))
                return (new StringConverter());
            if(T==typeof(Guid))
                return (new GuildConverter());
            if (T == typeof(System.Byte[]))
                return new ByteConverter();
            if (T.IsEnum)
                return (new EnumConverter());
            if (T.IsGenericType && T.GetGenericTypeDefinition() == typeof(Nullable<>))
                return new NullableConverter();
            return null;
        }
   


    }
}
