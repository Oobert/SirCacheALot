using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace Oobert.SirCacheALot
{
    public static class CacheExtension
    {
        static CacheExtension()
        {
            CacheLifetime = 2000;
        }

        public static int CacheLifetime { get; set; }

        public static T Cache<O, T>(this O obj, Expression<Func<O, T>> expression, string key)
        {
            Func<T> func = () => { return expression.Compile().Invoke(obj); };
            return Cache(func, key);
        }
        
        public static T Cache<T>(Expression<Func<T>> expression)
        {
            Func<T> func = () => { return expression.Compile().Invoke(); };
            return Cache(func, CreateKey(expression));
        }

        public static T Cache<O, T>(this O obj, Expression<Func<O, T>> expression)
        {
            Func<T> func = () => { return expression.Compile().Invoke(obj); };
            return Cache(func, CreateKey(expression));
        }

        public static T Cache<T>(Func<T> func, string key)
        {      
            if (MemoryCache.Default.Contains(key))
            {
                return (T)MemoryCache.Default.Get(key);
            }

            T value = func();
            MemoryCache.Default.Set(key, value, DateTimeOffset.Now.AddMilliseconds(CacheLifetime));
            return value;
        }

        private static string CreateKey<T>(Expression<Func<T>> expression)
        {
            return CreateKey(expression.Body as MethodCallExpression);
        }

        private static string CreateKey<O, T>(Expression<Func<O, T>> expression)
        {
            return CreateKey(expression.Body as MethodCallExpression);  
        }

        private static string CreateKey(MethodCallExpression methodExpression)
        {
            StringBuilder hashBuilder = new StringBuilder();

            hashBuilder.Append(string.Format("{0}.{1}(", methodExpression.Method.DeclaringType.FullName, methodExpression.Method.Name));
 
            for (int index = 0; index < methodExpression.Arguments.Count; index++)
            {
                var methodArgument = methodExpression.Arguments[index];
                if (index > 0)
                {
                    hashBuilder.Append(", ");
                }

                object argumentValue = null;
                var constantArgument = (methodArgument as ConstantExpression);
                if (constantArgument != null)
                {
                    argumentValue = constantArgument.Value;
                }

                if (argumentValue == null)
                {
                    argumentValue = GetValue(methodArgument);
                }
                string argumentString = argumentValue.ToString();

                if (argumentString == methodArgument.Type.FullName)
                {
                    throw new InvalidOperationException("");
                }

                hashBuilder.Append(string.Format("{{{0} {1} {2}}}", index, methodArgument.Type.FullName, argumentString));

            
            }
            hashBuilder.Append(")");

            return hashBuilder.ToString();
        }

        private static object GetValue(Expression member)
        {
            var objectMember = Expression.Convert(member, typeof(object));
            var getterLambda = Expression.Lambda<Func<object>>(objectMember);
            var getter = getterLambda.Compile();
            return getter();
        }      
    }
}
