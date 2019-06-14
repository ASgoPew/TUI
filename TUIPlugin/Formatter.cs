using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
