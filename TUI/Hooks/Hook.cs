using System;

namespace TUI
{
    public class Hook<T>
    {
        public delegate void HookD(T args);
        public event HookD Event;
        public void Invoke(T args)
        {
            try
            {
                Event?.Invoke(args);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public void Clear()
        {
            Event = null;
        }
    }
}
