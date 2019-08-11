using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class DictionaryExtensions
{
    public static string ToEncodeString<TKey, TValue>(this Dictionary<TKey, TValue> r)
    {
        string src = string.Empty;
        foreach (var i in r)
            src += i.Key.ToString() + "=" + Uri.EscapeDataString(i.Value.ToString()) + "&";
        return src.TrimEnd('&');
    }
}
