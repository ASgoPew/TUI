using System;
using TUI.Base;
using TUI.Base.Style;
using TUI.Hooks.Args;
using TUI.Widgets.Data;

namespace TUI.Widgets
{
    /// <summary>
    /// Widget for drawing chest with items.
    /// </summary>
    public class VisualChest : VisualObject
    {
        #region Data
        
        protected ItemData[] Items { get; set; }
        protected dynamic Chest { get; set; }

        #endregion

        #region Constructor

        public VisualChest(int x, int y, ItemData[] items = null, UIConfiguration configuration = null,
                UIStyle style = null, Action<VisualObject, Touch> callback = null)
            : base(x, y, 2, 2, configuration, style, callback)
        {
            Set(items);

            // Needed for ApplyTiles() (it checks if any of UIStyle parameters are set)
            Style.Active = true;
        }

        #endregion
        #region Copy

        public VisualChest(VisualChest visualChest)
            : this(visualChest.X, visualChest.Y, visualChest.Items,
                  new UIConfiguration(visualChest.Configuration), new UIStyle(visualChest.Style), visualChest.Callback.Clone() as Action<VisualObject, Touch>)
        { }

        #endregion
        #region LoadThisNative

        protected override void LoadThisNative()
        {
            base.LoadThisNative();
            CreateChest();
        }

        #endregion
        #region DisposeThisNative

        protected override void DisposeThisNative()
        {
            base.DisposeThisNative();
            RemoveChest();
        }

        #endregion

        #region operator[]
        
        public ItemData this[int index]
        {
            get => Items[index];
            set => Items[index] = value != null
                ? new ItemData(value)
                : new ItemData();
        }

        #endregion

        #region Set

        public virtual void Set(ItemData[] items)
        {
            if (items?.Length > 40)
                throw new ArgumentOutOfRangeException(nameof(items),
                    "Chest item count must be lesser than or equal to 40.");

            Items = new ItemData[40];
            for (int i = 0; i < 40; i++)
                Items[i] = (items?[i] == null || i >= items.Length)
                    ? new ItemData()
                    : new ItemData(items[i]);
        }

        #endregion
        #region Get

        public ItemData[] Get() => Items;

        #endregion
        #region CreateChest

        protected void CreateChest()
        {
            (int x, int y) = AbsoluteXY();
            UpdateChestArgs args = new UpdateChestArgs(x, y, null, this);
            TUI.Hooks.UpdateChest.Invoke(args);
            if (args.Chest == null)
            {
                TUI.Hooks.Log.Invoke(new LogArgs("Can't create new chest.", LogType.Error));
                return;
            }
            Chest = args.Chest;
            UpdateItems();
        }

        #endregion
        #region RemoveChest

        protected void RemoveChest()
        {
            if (Chest == null)
                return;
            TUI.Hooks.RemoveChest.Invoke(new RemoveChestArgs(this, Chest));
            Chest = null;
        }

        #endregion
        #region UpdateItems

        protected void UpdateItems()
        {
            for (int i = 0; i < 40; i++)
            {
                dynamic item = Chest.item[i];
                ItemData data = Items[i];
                if (item.netID != data.NetID)
                    item.netDefaults(data.NetID);
                item.prefix = data.Prefix;
                item.stack = data.Stack;
            }
        }

        #endregion
        #region UpdateChest

        protected void UpdateChest()
        {
            if (Chest != null)
            {
                (int x, int y) = AbsoluteXY();
                Chest.x = x;
                Chest.y = y;
                UpdateItems();
            }
        }

        #endregion

        #region PulseThisNative

        protected override void PulseThisNative(PulseType type)
        {
            base.PulseThisNative(type);

            if (type == PulseType.PositionChanged)
                UpdateChest();
        }

        #endregion
        #region UpdateThisNative

        protected override void UpdateThisNative()
        {
            base.UpdateThisNative();

            UpdateChest();
        }

        #endregion
        #region ApplyThisNative

        protected override void ApplyThisNative()
        {
            base.ApplyThisNative();

            UpdateChest();
        }

        #endregion
        #region ApplyTile

        protected override void ApplyTile(int x, int y)
        {
            dynamic tile = Tile(x, y);
            if (tile == null)
                return;
            tile.active(true);
            if (Style.InActive != null)
                tile.inActive(Style.InActive.Value);
            tile.type = (ushort)21;
            if (Style.TileColor != null)
                tile.color(Style.TileColor.Value);
            if (Style.Wall != null)
                tile.wall = Style.Wall.Value;
            if (Style.WallColor != null)
                tile.wallColor(Style.WallColor.Value);

            tile.frameX = (short)((x == 0) ? 0 : 18);
            tile.frameY = (short)((y == 0) ? 0 : 18);
        }

        #endregion
    }
}
