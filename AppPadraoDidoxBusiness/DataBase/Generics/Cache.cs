using System;
using System.Web;
using System.Web.Caching;
using System.Collections;
using System.Configuration;

namespace Didox.DataBase.Generics
{
    public class Cache
    {
        public static int MinutesOfEspirationCache()
        {
            try { return Convert.ToInt32(ConfigurationManager.AppSettings["MinutesCacheTime"]); }
            catch { }
            return 60;
        }

        public static void Add(string key, object value, DateTime dataExpiration)
        {
            try
            {
                HttpContext.Current.Cache.Add(key, value, null, dataExpiration, TimeSpan.Zero, CacheItemPriority.High, null);
            }
            catch { }
        }

        public static string KeyForClass(object value)
        {
            return "Model[" + value.GetType().Name + "]";
        }

        public static void Invalidate(string key)
        {
            try
            {
                HttpContext.Current.Cache.Remove(key);
            }
            catch { }
        }

        public static object Get(string key)
        {
            try
            {
                return HttpContext.Current.Cache[key];
            }
            catch { return null; }
        }

        public static void InvalidateByClass(object classType)
        {
            try
            {
                IDictionaryEnumerator cacheContents = HttpContext.Current.Cache.GetEnumerator();
                while (cacheContents.MoveNext())
                {
                    string currentKey = cacheContents.Key.ToString();
                    if (currentKey.Contains(KeyForClass(classType)))
                        HttpContext.Current.Cache.Remove(currentKey);
                }
            }
            catch { }
        }

        public static void ClearAll()
        {
            try
            {
                IDictionaryEnumerator cacheContents = HttpContext.Current.Cache.GetEnumerator();
                while (cacheContents.MoveNext())
                {
                    HttpContext.Current.Cache.Remove(cacheContents.Key.ToString());
                }
            }
            catch { }
        } 
    }
}
