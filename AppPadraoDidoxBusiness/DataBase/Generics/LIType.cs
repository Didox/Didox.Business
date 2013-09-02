using System;
using System.Data.Common;
using System.Collections.Generic;
using System.Reflection;

namespace Didox.DataBase.Generics
{
    public class LIType : List<IType>
    {
        public string ToJson()
        {
            var jsonItens = new List<string>();
            foreach(var iType in this)
            {
                var jsonFields = new List<string>();
                foreach (PropertyInfo pi in iType.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    PropertyAttribute[] pAttrProperty = (PropertyAttribute[])pi.GetCustomAttributes(typeof(PropertyAttribute), false);
                    if (pAttrProperty != null && pAttrProperty.Length > 0)
                    {
                        if (pAttrProperty[0].IsFieldBase)
                        {
                            var value = pi.GetValue(iType, null);
                            if (value != null) jsonFields.Add(pi.Name + ":'" + value + "'");
                        }
                    }
                }
                if(jsonFields.Count < 1) continue;
                jsonItens.Add("{" + iType.GetType().Name + ":{" + string.Join(",", jsonFields.ToArray()) + "}}");
            }

            return "[" + string.Join(",", jsonItens.ToArray()) + "]";
        }
    }
}
