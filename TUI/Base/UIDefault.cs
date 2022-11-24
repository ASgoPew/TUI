﻿using TerrariaUI.Base.Style;

namespace TerrariaUI.Base
{
    /// <summary>
    /// Default style settings for VisualObjects without specified styles.
    /// Be careful and don't change them accidentally.
    /// </summary>
    public static class UIDefault
    {
        public static Indent Indent { get; set; } = new Indent();
        public static InternalIndent InternalIndent { get; set; } = new InternalIndent();
        public static ExternalIndent ExternalIndent { get; set; } = new ExternalIndent();
        public static int CellsIndent { get; set; } = 0;
        public static Alignment Alignment { get; set; } = Alignment.Center;
        public static Direction Direction { get; set; } = Direction.Down;
        public static Side Side { get; set; } = Side.Center;

        public static Indent LabelIndent { get; set; } = new Indent() { Horizontal = 1, Vertical = 1 };
        public static byte LabelTextColor { get; set; } = 25;
        public static byte ButtonBlinkColor { get; set; } = 26;
        public static byte SliderSeparatorColor { get; set; } = 13;
        public static byte SliderUsedColor { get; set; } = 25;

        public static int LockDelay { get; set; } = 300;
    }
}
