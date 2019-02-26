namespace TUI
{
    public class UIUser
    {
        public virtual int Index => 0;
        public virtual bool HasPermission(string permission) =>
            true;
        public virtual void Teleport(int x, int y) { }
    }
}