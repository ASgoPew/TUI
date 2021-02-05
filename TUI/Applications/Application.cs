using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariaUI.Hooks.Args;
using TerrariaUI.Widgets;

namespace TerrariaUI
{
    public class Application
    {
        public string Name { get; protected set; }
        public Func<string, Panel> Generator { get; protected set; }
        public Dictionary<int, Panel> Instances { get; protected set; } = new Dictionary<int, Panel>();
        private ApplicationSaver Saver;
        private object Locker = new object();
        public Application(string name, Func<string, Panel> generator)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            else if (generator == null)
                throw new ArgumentNullException(nameof(generator));

            Name = name;
            Generator = generator;
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
                Panel instance = Generator($"{Name}_{index}");
                Instances[index] = instance;
                instance.SetXY(x - instance.Width / 2, y - instance.Height / 2, false);
                instance.SavePanel();
                TUI.Create(instance);
                Saver.DBWrite();
            }
        }

        internal void LoadInstance(int index)
        {
            Panel instance = Generator($"{Name}_{index}");
            lock (Locker)
            {
                if (Instances.TryGetValue(index, out Panel another))
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
    }
}
