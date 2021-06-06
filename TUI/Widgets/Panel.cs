using System;
using System.Collections.Generic;
using System.IO;
using TerrariaUI.Base;
using TerrariaUI.Base.Style;

namespace TerrariaUI.Widgets
{
    #region PanelStyle

    /// <summary>
    /// Drawing styles for Panel widget.
    /// </summary>
    public class PanelStyle : ContainerStyle
    {
        public bool SavePosition { get; set; } = true;
        public bool SaveSize { get; set; } = true;
        public bool SaveEnabled { get; set; } = true;

        public PanelStyle() : base() { }

        public PanelStyle(PanelStyle style)
            : base(style)
        {
            SavePosition = style.SavePosition;
            SaveSize = style.SaveSize;
            SaveEnabled = style.SaveEnabled;
        }
    }

    #endregion
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
        internal bool SaveDataNow { get; set; } = true;
        protected Summoning Summoning { get; set; }

        public PanelStyle PanelStyle => Style as PanelStyle;
        public VisualObject Summoned => Summoning?.Top?.Node;

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
                UIConfiguration configuration = null, PanelStyle style = null, object provider = null, HashSet<int> observers = null)
            : base(name, x, y, width, height, configuration ?? new UIConfiguration() { UseBegin = true,
                UseMoving = true, UseEnd = true }, style ?? new PanelStyle(), provider, observers)
        {
            if (drag != null)
                DragObject = Add(drag);
            if (resize != null)
                ResizeObject = Add(resize);
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
                PanelStyle style = null, object provider = null, HashSet<int> observers = null)
            : this(name, x, y, width, height, new DefaultPanelDrag(), new DefaultPanelResize(), configuration, style, provider, observers)
        { }

        #endregion
        #region DrawReposition

        protected override void DrawReposition(int oldX, int oldY, int oldWidth, int oldHeight)
        {
            base.DrawReposition(oldX, oldY, oldWidth, oldHeight);
            if (SaveDataNow)
                SavePanel();
        }

        #endregion
        #region LoadThisNative

        protected override void LoadThisNative()
        {
            base.LoadThisNative();
            if (!Personal)
                UDBRead(TUI.WorldID);
        }

        #endregion
        #region UDBReadNative

        protected override void UDBReadNative(BinaryReader br, int id)
        {
            int x, y, width, height;
            bool enabled;
            try
            {
                x = br.ReadInt32();
                y = br.ReadInt32();
                width = br.ReadInt32();
                height = br.ReadInt32();
                enabled = br.ReadBoolean();
            }
            catch (EndOfStreamException)
            {
                TUI.Log($"Panel invalid database data", LogType.Warning);
                DBWrite();
                return;
            }

            if (x + width < TUI.MaxTilesX && y + height < TUI.MaxTilesY)
            {
                if (PanelStyle.SavePosition && PanelStyle.SaveSize)
                    SetXYWH(x, y, width, height, false);
                else if (PanelStyle.SavePosition)
                    SetXY(x, y, false);
                else if (PanelStyle.SaveSize)
                    SetWH(width, height, false);

                if (PanelStyle.SaveEnabled)
                {
                    if (Enabled && !enabled)
                        Disable(false);
                    else if (!Enabled && enabled)
                        Enable(false);
                }

                base.UDBReadNative(br, id);
            }
            else
            {
                TUI.Log(this, $"Panel can't be placed at {x},{y}: map is too small", LogType.Warning);
                //SetXYWH(0, 0, width, height);
                Disable(false);
                return;
            }
        }

        #endregion
        #region UDBWriteNative

        protected override void UDBWriteNative(BinaryWriter bw, int id)
        {
            bw.Write((int)X);
            bw.Write((int)Y);
            bw.Write((int)Width);
            bw.Write((int)Height);
            bw.Write((bool)Enabled);

            base.UDBWriteNative(bw, id);
        }

        #endregion

        #region SavePanel

        /// <summary>
        /// Save panel position and size
        /// </summary>
        public void SavePanel()
        {
            if (Summoning == null && !Personal)
                UDBWrite(TUI.WorldID);
        }

        #endregion

        #region ShowPopUp

        /// <summary>
        /// Draws popup object on top of all other child objects.
        /// </summary>
        /// <param name="style">Style of popup background</param>
        /// <param name="cancelCallback">Action to call when player touches popup background but not popup itself</param>
        /// <returns>PopUpBackground</returns>
        public virtual VisualContainer ShowPopUp(VisualObject popup, ContainerStyle style = null, Action<VisualObject> cancelCallback = null)
        {
            style = style ?? new ContainerStyle();
            style.Transparent = true;
            if (PopUpBackground == null)
            {
                VisualContainer popUpBackground = new VisualContainer(0, 0, 0, 0, new UIConfiguration()
                { SessionAcquire = true }, style, (self, touch) =>
                {
                    VisualObject selected = ((VisualContainer)self).Selected;
                    if (selected != null && PopUpCancelCallbacks.TryGetValue(selected, out Action<VisualObject> cancel))
                        cancel.Invoke(this);
                    else
                        HidePopUp();
                });
                Add(popUpBackground, Int32.MaxValue);
                PopUpBackground = popUpBackground;
            }
            if (style != null)
                PopUpBackground.Style = style;
            PopUpBackground.SetFullSize(FullSize.Both);
            PopUpBackground.Add(popup);
            if (cancelCallback != null)
                PopUpCancelCallbacks[popup] = cancelCallback;
            PopUpBackground.DrawWithSection = DrawWithSection;
            PopUpBackground.Select(popup, false).Enable(false);
            Update();
            PopUpBackground.Apply().Draw();
            return PopUpBackground;
        }

        #endregion
        #region HidePopUp

        public virtual RootVisualObject HidePopUp()
        {
            if (PopUpBackground is VisualContainer popUpBackground)
            {
                popUpBackground.Disable(false);
                Apply().Draw();
            }
            return this;
        }

        #endregion
        #region Alert

        /// <summary>
        /// Show alert window with information text and "ok" button.
        /// </summary>
        /// <returns>this</returns>
        public virtual VisualContainer Alert(string text, ContainerStyle windowStyle = null, ButtonStyle okButtonStyle = null) =>
            ShowPopUp(new AlertWindow(text, windowStyle, okButtonStyle));

        #endregion
        #region Confirm

        /// <summary>
        /// Show confirm window with information text and "yes", "no" buttons.
        /// </summary>
        /// <returns>this</returns>
        public virtual VisualContainer Confirm(string text, Action<bool> callback, ContainerStyle windowStyle = null,
                ButtonStyle yesButtonStyle = null, ButtonStyle noButtonStyle = null) =>
            ShowPopUp(new ConfirmWindow(text, callback, windowStyle, yesButtonStyle, noButtonStyle));

        #endregion
        #region Summon

        public Panel Summon(VisualObject node, Alignment alignment = Alignment.Center,
            bool replace = false, bool drag = false, bool resize = false)
        {
            if (Summoned == node)
                return this;

            if (Summoning == null)
                Summoning = new Summoning(X, Y, Width, Height);
            else if (replace)
                UnsummonNode();

            SummonNode(node, alignment, drag, resize);
            ApplySummoned();

            return this;
        }

        #endregion
        #region SummonNode

        private void SummonNode(VisualObject node, Alignment alignment, bool drag, bool resize)
        {
            bool wasChild = HasChild(node);
            if (!wasChild)
                Add(node);
            Summoning.Push(node, wasChild, alignment, drag, resize);
            node.SetXY(0, 0, false);
        }

        #endregion
        #region ApplySummoned

        private void ApplySummoned()
        {
            SummoningNode summoningNode = Summoning.Top;
            VisualObject node = summoningNode.Node;
            Alignment alignment = summoningNode.Alignment;
            (int oldX, int oldY, int oldWidth, int oldHeight) = XYWH();

            Select(node, false);
            if (summoningNode.Drag)
                DragObject?.Enable(false);
            else
                DragObject?.Disable(false);
            if (summoningNode.Resize)
                ResizeObject?.Enable(false);
            else
                ResizeObject?.Disable(false);

            int w = node.Width;
            int h = node.Height;

            int x;
            if (alignment == Alignment.UpLeft || alignment == Alignment.Left || alignment == Alignment.DownLeft)
                x = Summoning.OldX;
            else if (alignment == Alignment.UpRight || alignment == Alignment.Right || alignment == Alignment.DownRight)
                x = Summoning.OldX + (Summoning.OldWidth - w);
            else
                x = Summoning.OldX + (Summoning.OldWidth - w) / 2;

            int y;
            if (alignment == Alignment.UpLeft || alignment == Alignment.Up || alignment == Alignment.UpRight)
                y = Summoning.OldY;
            else if (alignment == Alignment.DownLeft || alignment == Alignment.Down || alignment == Alignment.DownRight)
                y = Summoning.OldY + (Summoning.OldHeight - h);
            else
                y = Summoning.OldY + (Summoning.OldHeight - h) / 2;

            SetXYWH(x, y, w, h, false);
            Update().Apply();
            DrawReposition(oldX, oldY, oldWidth, oldHeight);
        }

        #endregion
        #region Unsummon

        public Panel Unsummon(int levels = 1)
        {
            if (Summoning == null)
                return this;
            else if (levels < 1)
                throw new ArgumentOutOfRangeException(nameof(levels));
            else if (levels > Summoning.Count)
                levels = Summoning.Count;

            (int oldX, int oldY, int oldWidth, int oldHeight) = XYWH();
            while (levels-- > 0)
                UnsummonNode();

            if (Summoning.Count > 0)
                ApplySummoned();
            else
            {
                SetXYWH(Summoning.OldX, Summoning.OldY, Summoning.OldWidth, Summoning.OldHeight, false);
                DragObject?.Enable(false);
                ResizeObject?.Enable(false);
                Deselect(false);
                DrawReposition(oldX, oldY, oldWidth, oldHeight);
                Summoning = null;
            }

            return this;
        }

        #endregion
        #region UnsummonAll

        public Panel UnsummonAll() =>
            Unsummon(Int32.MaxValue);

        #endregion
        #region UnsummonNode

        private void UnsummonNode()
        {
            SummoningNode node = Summoning.Pop();
            if (!node.WasChild)
                Remove(node.Node);
            else // Restoring Summoned position in parent since it was a child before summoning
                node.Node.SetXY(node.OldX, node.OldY, false);
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
                    panel.SetXY(panel.DragX + dx, panel.DragY + dy, true);
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
                    panel.SetWH(panel.ResizeW + dw, panel.ResizeH + dh, true);
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
            : base(0, 0, 1, 1, new UIConfiguration() { UseMoving=useMoving, UseEnd=true, UseOutsideTouches=true, Permission=TUI.ControlPermission })
        {
        }
    }

    #endregion
    #region DefaultPanelResize

    public sealed class DefaultPanelResize : PanelResize
    {
        public DefaultPanelResize()
            : base(0, 0, 1, 1, new UIConfiguration() { UseMoving=true, UseEnd=true, UseOutsideTouches=true, Permission=TUI.ControlPermission })
        {
            SetAlignmentInParent(Alignment.DownRight);
        }
    }

    #endregion
}
