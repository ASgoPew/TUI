namespace TUI
{
    public interface IUIUser
    {
        int Index { get; }
        string Name { get; }
        bool HasPermission(string permission);
        void Teleport(int x, int y);
    }
}