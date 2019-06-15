using Newtonsoft.Json;
using System;

namespace TUIPlugin
{
    public class Formatter
    {
        public static string Serialize(object obj) =>
            JsonConvert.SerializeObject(obj);

        public static object Deserialize(string json, Type type) =>
            JsonConvert.DeserializeObject(json, type);
    }
}
