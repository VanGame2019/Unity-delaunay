using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Color = UnityEngine.Color;

namespace ETModel
{
    public static class MyModelTool
    {
        public static Transform FindChildRecursive(string _Name, Transform _T = null)
        {
            if (_T.name == _Name)
            {
                return _T;
            }
            Transform r = _T.Find(_Name);
            if (r != null)
                return r;
            foreach (Transform c in _T)
            {
                r = FindChildRecursive(_Name, c);
                if (r != null)
                    return r;
            }
            return null;
        }

        
        
        public static List<T> RandomList<T>(List<T> list, bool isTrueRandom = false)
        {
            List<T> from = new List<T>(list);

            List<T> to = new List<T>();

            while (from.Count > 0)
            {
                if (isTrueRandom)
                {
                    int randIndex = Random.Range(0, from.Count);
                    T remove = from[randIndex];

                    from.Remove(remove);
                    to.Add(remove);
                }
                else
                {
                    int randIndex = MyRandom.Range(0, from.Count);
                    T remove = from[randIndex];

                    from.Remove(remove);
                    to.Add(remove);
                }
            }

            return to;
        }

        public static T RandomElement<T>(List<T> list)
        {
            if (list.Count == 0)
            {
                return default(T);
            }

            return list[MyRandom.Range(0, list.Count)];
        }

        public static int RandomIndex<T>(List<T> list)
        {
            if (list.Count == 0)
            {
                return 0;
            }

            return MyRandom.Range(0, list.Count);
        }
        
        //hex 
        //#5c031e
        public static Color getColorFromRBG(string str)
        {
            Color c;
            ColorUtility.TryParseHtmlString(str, out c);
            return c;
        }
        
        
        private static BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        public static bool ContainsProperty(object obj, string property) {
            if (obj == null) return false;
            return obj.GetType().GetProperty(property,bindingFlags) != null;
        }

        public static object GetProperty(object obj, string property) {
            if (!ContainsProperty(obj, property)) return null;
            var prop = obj.GetType().GetProperty(property,bindingFlags);
            return prop?.GetValue(obj);
        }

        public static bool ContainsField(object obj, string field) {
            if (obj == null) return false;
            return obj.GetType().GetField(field,bindingFlags) != null;
        }
        
        public static object GetField(object obj, string field) {
            if (!ContainsField(obj, field)) return null;
            var prop = obj.GetType().GetField(field,bindingFlags);
            return prop?.GetValue(obj);
        }
        
        public static bool ContainsMethod(object obj, string method ) {
            if (obj == null) return false;
            return obj.GetType().GetMethod(method,bindingFlags) != null;
        }
        
        public static object RunMethod(object obj, string method,params object[] args) {
            if (!ContainsMethod(obj, method)) return null;
            var prop = obj.GetType().GetMethod(method,bindingFlags);
            return prop?.Invoke(obj,args);
        }
        
    }
    
}