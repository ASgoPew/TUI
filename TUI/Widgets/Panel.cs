using System;
using System.IO;
using TerrariaUI.Base;
using TerrariaUI.Base.Style;
using TerrariaUI.Hooks.Args;

namespace TerrariaUI.Widgets
{
    enum PanelState
    {
        Moving = 0,
        Resizing
    }

    /// <summary>
    /// Root widget that saves its position and size and has a button for changing position
    /// and a button for changing size (top left corner 1x1 and bottom right corner 1x1 by default).
    /// </summary>
    public class Panel : RootVisualObject
    {
        #region Data

        internal int DragX { get; set; }
        internal int DragY { get; set; }
        internal int ResizeW { get; set; }
        internal int ResizeH { get; set; }

        public PanelDrag DragObject { get; set; }
        public PanelResize ResizeObject { get; set; }
        internal bool SaveDataNow { get; set; } = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Root widget that saves its position and size and has a button for changing position
        /// and a button for changing size (top left corner 1x1 and bottom right corner 1x1 by default).
        /// </summary>
        /// <param name="name">Unique interface identifier</param>
        /// <param name="provider">Tile provider object, default value - null (interface would
        /// be drawn on the Main.tile, tiles would be irrevocably modified).
        /// <para></para>
        /// FakeTileRectangle from [FakeManager](https://github.com/AnzhelikaO/FakeManager)
        /// can be passed as a value so that interface would be drawn above the Main.tile.</param>
        public Panel(string name, int x, int y, int width, int height, PanelDrag drag, PanelResize resize,
                UIConfiguration configuration = null, ContainerStyle style = null, object provider = null)
            : base(name, x, y, width, height, configuration ?? new UIConfiguration() { UseBegin = true,
                UseMoving = true, UseEnd = true }, style, provider)
        {
            if (drag != null)
                DragObject = Add(drag) as PanelDrag;
            if (resize != null)
                ResizeObject = Add(resize) as PanelResize;

            DBRead();
        }

        /// <summary>
        /// Root widget that saves its position and size and has a button for changing position
        /// and a button for changing size (top left corner 1x1 and bottom right corner 1x1 by default).
        /// </summary>
        /// <param name="name">Unique interface identifier</param>
        /// <param name="provider">Tile provider object, default value - null (interface would
        /// be drawn on the Main.tile, tiles would be irrevocably modified).
        /// <para></para>
        /// FakeTileRectangle from [FakeManager](https://github.com/AnzhelikaO/FakeManager)
        /// can be passed as a value so that interface would be drawn above the Main.tile.</param>
        public Panel(string name, int x, int y, int width, int height, UIConfiguration configuration = null,
                ContainerStyle style = null, object provider = null)
            : this(name, x, y, width, height, new DefaultPanelDrag(), new DefaultPanelResize(), configuration, style, provider)
        { }

        #endregion
        #region SetXYWH

        public override VisualObject SetXYWH(int x, int y, int width, int height)
        {
            int oldX = X, oldY = Y, oldWidth = Width, oldHeight = Height;
            if (oldX != x || oldY != y || oldWidth != width || oldHeight != height)
            {
                base.SetXYWH(x, y, width, height);
#if DEBUG
                Console.WriteLine($"{FullName}.SetXYWH({x}, {y}, {width}, {height}){(SaveDataNow ? " SAVING" : "")}");
#endif
                if (SaveDataNow)
                    SavePanel();
                SaveDataNow = false;
            }
            return this;
        }

        #endregion
        #region ReadDataNative

        protected override void DBReadNative(BinaryReader br)
        {
            int x = br.ReadInt32();
            int y = br.ReadInt32();
            int width = br.ReadInt32();
            int height = br.ReadInt32();
            if (x + width < TUI.MaxTilesX && y + height < TUI.MaxTilesY)
                SetXYWH(x, y, width, height);
            else
            {
                TUI.Log(this, $"Panel can't be placed at {x},{y}: map is too small", LogType.Warning);
                //SetXYWH(0, 0, width, height);
                Disable();
            }
        }

        #endregion
        #region WriteDataNative

        protected override void DBWriteNative(BinaryWriter bw)
        {
            bw.Write((int)X);
            bw.Write((int)Y);
            bw.Write((int)Width);
            bw.Write((int)Height);
        }

        #endregion

        #region SavePanel

        /// <summary>
        /// Save panel position and size
        /// </summary>
        public void SavePanel() =>
            DBWrite();

        #endregion
        #region Drag

        /// <summary>
        /// Change panel position.
        /// </summary>
        public void Drag(int x, int y)
        {
            if (x == X && y == Y)
                return;
            if (UsesDefaultMainProvider)
                Clear()
                .Draw(toEveryone: true)
                .SetXY(x, y)
                .Update()
                .Apply()
                .Draw(toEveryone: true);
            else
            {
                int oldX = X, oldY = Y;
                SetXY(x, y)
                .Draw(oldX - x, oldY - y, toEveryone: true)
                .Draw(toEveryone: true);
            }
        }

        #endregion
        #region Resize

        /// <summary>
        /// Change panel size.
        /// </summary>
        public void Resize(int width, int height)
        {
            GridConfiguration grid = Configuration.Grid;
            int minWidth = grid?.MinWidth ?? 1;
            int minHeight = grid?.MinHeight ?? 1;
            if (width < minWidth)
                width = minWidth;
            if (height < minHeight)
                height = minHeight;
            if (width == Width && height == Height)
                return;
            if (UsesDefaultMainProvider)
                Clear()
                .Draw(frameSection: false, toEveryone: true)
                .SetWH(width, height)
                .Update()
                .Apply()
                .Draw(toEveryone: true);
            else
            {
                int oldWidth = Width, oldHeight = Height;
                SetWH(width, height)
                .Update()
                .Apply()
                .Draw(0, 0, oldWidth, oldHeight, frameSection: false, toEveryone: true)
                .Draw(toEveryone: true);
            }
        }

        #endregion
    }

    #region PanelDrag

    public class PanelDrag : VisualObject
    {
        public PanelDrag(int x, int y, int width, int height, UIConfiguration configuration = null,
                UIStyle style = null, Action<VisualObject, Touch> callback = null)
            : base(x, y, width, height, configuration, style, callback)
        {
            Configuration.UseEnd = true;
            Layer = Int32.MaxValue;
            if (Callback == null)
                Callback = CustomCallback;
        }

        private static void CustomCallback(VisualObject @this, Touch touch)
        {
            if (touch.State == TouchState.Begin)
            {
                Panel panel = (Panel)@this.Parent;
                panel.DragX = panel.X;
                panel.DragY = panel.Y;
                touch.Session[@this] = PanelState.Moving;
            }
            else if (touch.Session[@this] is PanelState state && state == PanelState.Moving)
            {
                bool ending = touch.State == TouchState.End;
                if (@this.Configuration.UseMoving && touch.State == TouchState.Moving || ending)
                {
                    int dx = touch.AbsoluteX - touch.Session.BeginTouch.AbsoluteX;
                    int dy = touch.AbsoluteY - touch.Session.BeginTouch.AbsoluteY;
                    Panel panel = (Panel)@this.Parent;
                    panel.SaveDataNow = ending;
                    panel.Drag(panel.DragX + dx, panel.DragY + dy);
                    if (ending)
                    {
                        touch.Session[@this] = null;
                        panel.SavePanel();
                    }
                }
            }
        }
    }

    #endregion
    #region PanelResize

    public class PanelResize : VisualObject
    {
        public PanelResize(int x, int y, int width, int height, UIConfiguration configuration = null,
                UIStyle style = null, Action<VisualObject, Touch> callback = null)
            : base(x, y, width, height, configuration, style, callback)
        {
            Configuration.UseEnd = true;
            Layer = Int32.MaxValue;
            if (Callback == null)
                Callback = CustomCallback;
        }

        private static void CustomCallback(VisualObject @this, Touch touch)
        {
            if (touch.State == TouchState.Begin)
            {
                Panel panel = (Panel)@this.Parent;
                panel.ResizeW = panel.Width;
                panel.ResizeH = panel.Height;
                touch.Session[@this] = PanelState.Resizing;
            }
            else if (touch.Session[@this] is PanelState state && state == PanelState.Resizing)
            {
                bool ending = touch.State == TouchState.End;
                if (@this.Configuration.UseMoving && touch.State == TouchState.Moving || ending)
                {
                    int dw = touch.AbsoluteX - touch.Session.BeginTouch.AbsoluteX;
                    int dh = touch.AbsoluteY - touch.Session.BeginTouch.AbsoluteY;
                    Panel panel = (Panel)@this.Parent;
                    panel.SaveDataNow = ending;
                    panel.Resize(panel.ResizeW + dw, panel.ResizeH + dh);
                    if (ending)
                    {
                        touch.Session[@this] = null;
                        panel.SavePanel();
                    }
                }
            }
        }
    }

    #endregion
    #region DefaultPanelDrag

    public sealed class DefaultPanelDrag : PanelDrag
    {
        public DefaultPanelDrag(bool useMoving = true)
            : base(0, 0, 1, 1, new UIConfiguration() { UseMoving=useMoving, UseEnd=true, UseOutsideTouches=true, Permission="TUI.Control" })
        {
        }
    }

    #endregion
    #region DefaultPanelResize

    public sealed class DefaultPanelResize : PanelResize
    {
        public DefaultPanelResize()
            : base(0, 0, 1, 1, new UIConfiguration() { UseMoving=true, UseEnd=true, UseOutsideTouches=true, Permission="TUI.Control" })
        {
            SetAlignmentInParent(Alignment.DownRight);
        }
    }

    #endregion
}
