using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariaUI.Hooks.Args;
using TerrariaUI.Widgets;
using TerrariaUI.Widgets.Media;

namespace TerrariaUI
{
    public class ApplicationType
    {
        public string Name { get; protected set; }
        public Func<string, Application> Generator { get; protected set; }
        protected Dictionary<int, Application> Instances { get; set; } = new Dictionary<int, Application>();
        protected ApplicationSaver Saver;
        protected object Locker = new object();

        public ImageData Icon { get; set; } = null;

        public int InstancesCount
        {
            get
            {
                lock (Locker)
                    return Instances.Count;
            }
        }
        public IEnumerable<KeyValuePair<int, Application>> IterateInstances
        {
            get
            {
                lock (Locker)
                    foreach (var instance in Instances)
                        yield return instance;
            }
        }

        public ApplicationType(string name, Func<string, Application> generator, ImageData icon = null)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            else if (generator == null)
                throw new ArgumentNullException(nameof(generator));

            Name = name;
            Generator = generator;
            Icon = icon;
            Saver = new ApplicationSaver(this);
            Saver.DBRead();
        }

        public override string ToString() => Name;

        public void CreateInstance(int x, int y)
        {
            lock (Locker)
            {
                int index = 0;
                while (Instances.ContainsKey(index))
                    index++;
                Application instance = Generator($"{Name}_{index}");
                instance.Type = this;
                Instances[index] = instance;
                instance.SetXY(x - instance.Width / 2, y - instance.Height / 2, false);
                instance.SavePanel();
                TUI.Create(instance);
                Saver.DBWrite();
            }
        }

        internal void LoadInstance(int index)
        {
            lock (Locker)
            {
                Application instance = Generator($"{Name}_{index}");
                instance.Type = this;
                if (Instances.TryGetValue(index, out Application another))
                {
                    TUI.Destroy(another);
                    TUI.Log($"Application {another.Name} was replaced by a new one.", LogType.Warning);
                }
                Instances[index] = instance;
                TUI.Create(instance);
            }
        }

        public void DestroyInstance(int index)
        {
            lock (Locker)
            {
                TUI.Destroy(Instances[index]);
                Instances.Remove(index);
                Saver.DBWrite();
            }
        }

        public void DestroyInstance(Application app)
        {
            lock (Locker)
            {
                var pair2 = Instances.FirstOrDefault(pair => pair.Value == app);
                if (pair2.Value == app)
                    DestroyInstance(pair2.Key);
            }
        }

        public bool TryDestroy(int x, int y, out string name)
        {
            name = null;
            lock (Locker)
            {
                foreach (var pair in Instances)
                    if (pair.Value.Contains(x, y))
                    {
                        name = pair.Value.Name;
                        DestroyInstance(pair.Key);
                        return true;
                    }
            }
            return false;
        }

        public void DestroyAll()
        {
            lock (Locker)
                foreach (int key in Instances.Keys.ToArray())
                    DestroyInstance(key);
        }

        public void Write(BinaryWriter bw)
        {
            lock (Locker)
            {
                bw.Write((byte)Instances.Count);
                foreach (var pair in Instances)
                    bw.Write((int)pair.Key);
            }
        }
    }
}
