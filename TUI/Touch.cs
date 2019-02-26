using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI
{
    public enum TouchState
    {
        Begin,
        Moving,
        End
    }

    public class Touch<T> : IVisual<Touch<T>>, ICloneable
        where T : Touchable<T>
    {
        #region Data

        public int AbsoluteX { get; set; }
        public int AbsoluteY { get; set; }
        public TouchState State { get; set; }
        public string Prefix { get; set; }
        public byte StateByte { get; set; }
        public UIUserSession<T> Session { get; set; }
        public T Root { get; set; }

        public bool Red => (StateByte & 1) > 0;
        public bool Green => (StateByte & 2) > 0;
        public bool Blue => (StateByte & 4) > 0;
        public bool Yellow => (StateByte & 8) > 0;
        public bool Actuator => (StateByte & 16) > 0;
        public bool Cutter => (StateByte & 32) > 0;
        public UIUser User => Session.User;

        #endregion

        #region IVisual

            #region Data

            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }

            public IEnumerable<(int, int)> Points { get { yield return (X, Y); } }
            public (int X, int Y, int Width, int Height) Padding(PaddingConfig paddingData) =>
                UI.Padding(X, Y, Width, Height, paddingData);

            #endregion

            #region Initialize

            public void InitializeVisual(int x, int y, int width = 1, int height = 1)
            {
                X = x;
                Y = y;
                Width = width;
                Height = height;
            }

            #endregion
            #region XYWH

            public (int X, int Y, int Width, int Height) XYWH(int dx = 0, int dy = 0)
            {
                return (X + dx, Y + dy, Width, Height);
            }

            public Touch<T> SetXYWH(int x, int y, int width = -1, int height = -1)
            {
                X = x;
                Y = y;
                Width = width >= 0 ? width : Width;
                Height = height >= 0 ? height : Height;
                return this;
            }

            public Touch<T> SetXYWH((int x, int y, int width, int height) data)
            {
                X = data.x;
                Y = data.y;
                Width = data.width;
                Height = data.height;
                return this;
            }

            #endregion
            #region Move

            public Touch<T> Move(int dx, int dy)
            {
                X = X + dx;
                Y = Y + dy;
                return this;
            }

            public Touch<T> MoveBack(int dx, int dy)
            {
                X = X - dx;
                Y = Y - dy;
                return this;
            }

            #endregion
            #region Contains, Intersects

            public bool Contains(int x, int y) =>
                x >= X && y >= Y && x < X + Width && y < Y + Height;
            public bool Intersecting(int x, int y, int width, int height) =>
                x < X + Width && X < x + width && y < Y + Height && Y < y + height;
            public bool Intersecting(Touch<T> o) => false;

        #endregion

        #endregion

        #region Initialize

        public Touch(int x, int y, TouchState state, UIUserSession<T> session, string prefix = null, byte stateByte = 0)
        {
            InitializeVisual(x, y);
            State = state;
            Session = session;
            Prefix = prefix;
            StateByte = stateByte;
        }

        #endregion
        #region Clone

        public object Clone()
        {
            Touch<T> result = MemberwiseClone() as Touch<T>;
            result.Prefix = result.Prefix != null ? String.Copy(Prefix) : null;
            return result;
        }

        #endregion
    }

    public class VisualTouch : Touch<VisualObject>
    {
        public VisualTouch(int x, int y, TouchState state, UIUserSession<VisualObject> session, string prefix = null, byte stateByte = 0)
            : base(x, y, state, session, prefix, stateByte) { }
    }
}
