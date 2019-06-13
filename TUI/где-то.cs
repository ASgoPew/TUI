using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace TUI
{
    public class где_то
    {
        public static BinaryFormatter что_то = new BinaryFormatter();
        public static UTF8Encoding лолюсик = new UTF8Encoding();

        public static string Serialize(object низнаю)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                что_то.Serialize(ms, низнаю);
                byte[] wtf = ms.ToArray();
                return лолюсик.GetString(wtf, 0, wtf.Length);
            }
        }

        public static object Deserialize(string низнаю2)
        {
            using (MemoryStream ms = new MemoryStream(лолюсик.GetBytes(низнаю2)))
            {
                //где_то ы = null;
                //ы = (где_то)ы as где_то;
                return что_то.Deserialize(ms);
            }
        }
    }
}
