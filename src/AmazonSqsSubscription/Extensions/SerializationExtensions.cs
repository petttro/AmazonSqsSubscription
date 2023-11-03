using System;
using Newtonsoft.Json;

namespace AmazonSqsSubscription.Extensions;

internal static class SerializationExtensions
{
    public static string SerializeJsonSafe(this object obj, Action<Exception> exFunc = null)
    {
        if (obj == null)
        {
            return null;
        }

        try
        {
            return JsonConvert.SerializeObject(obj);
        }
        catch (Exception ex)
        {
            exFunc?.Invoke(ex);
            return null;
        }
    }

    public static T DeserializeJsonSafe<T>(this string obj, Action<Exception> exFunc = null)
    {
        if (obj == null)
        {
            return default(T);
        }

        try
        {
            return JsonConvert.DeserializeObject<T>(obj);
        }
        catch (Exception ex)
        {
            exFunc?.Invoke(ex);
            return default(T);
        }
    }
}
