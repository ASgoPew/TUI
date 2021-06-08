using System;

namespace TerrariaUI.Hooks
{
    public class Hook<T>
        where T : EventArgs
    {
        public delegate void HookD(T args);
        public event HookD Event;
        public T Invoke(T args)
        {
            try
            {
                Event?.Invoke(args);
            }
            catch (Exception e)
            {
                TUI.HandleException(e);
            }
            return args;
        }
        public void Clear()
        {
            Event = null;
        }
    }
}
