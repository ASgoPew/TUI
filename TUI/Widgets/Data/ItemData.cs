namespace TUI.Widgets.Data
{
    public class ItemData
    {
        public int NetID;
        public byte Prefix;
        public int Stack;

        public ItemData() { }
        public ItemData(ItemData itemData)
        {
            NetID = itemData.NetID;
            Prefix = itemData.Prefix;
            Stack = itemData.Stack;
        }
    }
}
