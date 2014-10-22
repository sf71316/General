using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace General.Data
{
    public class ExpressionFactory
    {
        public static Dictionary<PropertyKey, Action<object, object>> PropertyCache = new Dictionary<PropertyKey, Action<object, object>>();
        public static object lockObject = new object();


        public static void Set(object obj, object value, string propertyName)
        {
            PropertyKey key = new PropertyKey(obj.GetType(), propertyName);

            //先檢查有沒有該屬性的委派方法(塞值)
            if (!PropertyCache.ContainsKey(key))
            {
                lock (lockObject)
                {
                    if (!PropertyCache.ContainsKey(key))
                    {
                        //沒有的話，就建立利用ExpressionTree建立委派
                        var action = CreateSetAction(obj, propertyName);
                        //放入Cache
                        PropertyCache[key] = action;
                        //執行賦值動作
                        action(obj, value);
                        return;
                    }
                }
            }
            PropertyCache[key](obj, value);
        }
        private static object Get(PropertyInfo obj)
        {
            //有問題未解決

                lock (lockObject)
                {
                    //if (!PropertyCache.ContainsKey(key))
                    //{
                        //沒有的話，就建立利用ExpressionTree建立委派
                        var action = CreateGetAction(obj);
                        //放入Cache
                  
                        //執行取值動作
                        return action(obj);
                   // }
                }
    
          
        }
        private static Action<object, object> CreateSetAction(object obj, string propertyName)
        {
            var propertyInfo = obj.GetType().GetProperty(propertyName);

            //參數1 目標物件
            var targetObj = Expression.Parameter(typeof(object), "obj");
            //參數2 值
            var propertyValue = Expression.Parameter(typeof(object), "value");

            //主體
            //第一個參數：由於上方參數1是object型別，因此先轉成目標物件型別
            //第二個參數：Set Property 的方法主體
            //第三個參數：上方的參數2是object型別，因此要轉成目標屬性型別
            var setMethod = Expression.Call(
                          Expression.Convert(targetObj, obj.GetType()),
                          propertyInfo.GetSetMethod(),
                          Expression.Convert(propertyValue,
                                             propertyInfo.PropertyType)
                          );

            //將此 ExpressionTree 編譯成 Lambda，沒有回傳值，兩個參數的委派方法
            return Expression.Lambda<Action<object, object>>(setMethod, targetObj, propertyValue).Compile();
        }

        private static Func<object, object> CreateGetAction(PropertyInfo propertyInfo)
        {
            var instance = Expression.Parameter(propertyInfo.DeclaringType, "i");
            var property = Expression.Property(instance, propertyInfo);
            var convert = Expression.TypeAs(property, typeof(object));
            return (Func<object, object>)Expression.Lambda(convert, instance).Compile();
        }
    }

    public class PropertyKey
    {

        public Type PropertyType { get; private set; }
        public string PropertyName { get; private set; }

        public PropertyKey(Type type, string propertyName)
        {
            PropertyType = type;
            PropertyName = propertyName;
        }

        public override int GetHashCode()
        {
            return PropertyType.GetHashCode() ^ PropertyName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.GetHashCode().Equals(obj.GetHashCode());
        }
    }
}
