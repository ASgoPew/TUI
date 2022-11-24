﻿using TerrariaUI.Base;
using TerrariaUI.Base.Style;

namespace TerrariaUI.Widgets
{
    public class Separator : VisualObject
    {
        public Separator(int size, UIStyle style = null)
            : base(0, 0, size, size, null, style)
        {
        }

        public Separator(int width, int height, UIStyle style = null)
            : base(0, 0, width, height, null, style)
        {
        }
    }
}
