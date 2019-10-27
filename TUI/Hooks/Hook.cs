using System;

namespace TUI.Hooks
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
                TUI.HandleException(e);
            }
        }
        public void Clear()
        {
            Event = null;
        }
    }
}
