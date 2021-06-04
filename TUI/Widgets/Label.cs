using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TerrariaUI.Base;
using TerrariaUI.Base.Style;
using TerrariaUI.Hooks.Args;

namespace TerrariaUI.Widgets
{
    #region LabelStyle

    public enum LabelUnderline
    {
        None = 0,
        Underline
    }

    /// <summary>
    /// Drawing styles for Label widget.
    /// </summary>
    public class LabelStyle : UIStyle
    {
        public byte TextColor { get; set; } = UIDefault.LabelTextColor;
        public Indent TextIndent { get; set; } = new Indent(UIDefault.LabelIndent);
        /// <summary>
        /// Where to place the text (up right corner/down side/center/...)
        /// </summary>
        public Alignment TextAlignment { get; set; } = UIDefault.Alignment;
        /// <summary>
        /// Side to which shorter lines would adjoin.
        /// </summary>
        public Side TextSide { get; set; } = UIDefault.Side;
        /// <summary>
        /// Whether to use underline part of statues for characters (makes their size 2x3 instead of 2x2).
        /// </summary>
        public LabelUnderline TextUnderline { get; set; } = LabelUnderline.None;
        /// <summary>
        /// Color of statue underline part (if TextUnderline is LabelUnderLine.Underline).
        /// </summary>
        public byte TextUnderlineColor { get; set; } = UIDefault.LabelTextColor;

        public LabelStyle() : base() { }

        public LabelStyle(LabelStyle style)
            : base(style)
        {
            this.TextColor = style.TextColor;
            this.TextIndent = style.TextIndent;
            this.TextAlignment = style.TextAlignment;
            this.TextSide = style.TextSide;
            this.TextUnderline = style.TextUnderline;
            this.TextUnderlineColor = style.TextUnderlineColor;
        }
    }

    #endregion

    /// <summary>
    /// Widget for showing text using character statues.
    /// </summary>
    public class Label : VisualObject
    {
        #region Data

        public const char ReservedCharacter = '\r';
        public const string ItemPattern = @"(?<!\\)\[(?<tag>i(tem)?)(\/(?<options>[^:]+))?:(?<id>\d+?)(?<!\\)\]";

        public override string Name => $"{GetType().Name} ({RawText})";
        protected string RawText { get; set; }
        protected List<(string Text, int Width)> Lines { get; set; }
        protected int TextW { get; set; }
        protected int TextH { get; set; }

        private List<int> StatuePlaceStyle = new List<int>();
        private int StatuePlaceStyleCounter = 0;

        public LabelStyle LabelStyle => Style as LabelStyle;

        #endregion

        #region Constructor

        /// <summary>
        /// Widget for showing text using character statues.
        /// </summary>
        public Label(int x, int y, int width, int height, string text, UIConfiguration configuration = null,
                LabelStyle style = null, Action<VisualObject, Touch> callback = null)
            : base(x, y, width, height, configuration ?? new UIConfiguration()
            {
                UseBegin = false
            }, style ?? new LabelStyle(), callback)
        {
            if (text.Contains(ReservedCharacter))
                throw new InvalidOperationException((int)ReservedCharacter + " is a reserved character.");
            RawText = text ?? throw new ArgumentNullException(nameof(text));
        }

        /// <summary>
        /// Widget for showing text using character statues.
        /// </summary>
        public Label(int x, int y, int width, int height, string text, LabelStyle style)
            : this(x, y, width, height, text, null, style)
        {
        }

        #endregion
        #region Copy

        public Label(Label label)
            : this(label.X, label.Y, label.Width, label.Height, string.Copy(label.RawText),new UIConfiguration(label.Configuration),
                  new LabelStyle(label.Style as LabelStyle), label.Callback?.Clone() as Action<VisualObject, Touch>)
        {
        }

        #endregion
        #region ApplyThisNative

        protected override void ApplyThisNative()    
        {
            base.ApplyThisNative();

            LabelStyle style = Style as LabelStyle;
            string text = GetText();
            if (string.IsNullOrWhiteSpace(text))
                return;
            int lineH = 2 + (int)style.TextUnderline;
            DrawWithSection = false;
            Indent indent = style.TextIndent;
            Alignment alignment = style.TextAlignment;
            Side side = style.TextSide;

            int spaceW = Width - indent.Left - indent.Right;
            int spaceH = Height - indent.Up - indent.Down;
		    int textW = TextW;
		    int textH = TextH;

            if (spaceH < lineH)
                return;

            int charX = 0;
            if (alignment == Alignment.Center || alignment == Alignment.Up || alignment == Alignment.Down)
                charX = indent.Left + (int)Math.Floor((spaceW - textW) / 2d);
            else if (alignment == Alignment.UpLeft || alignment == Alignment.Left || alignment == Alignment.DownLeft)
                charX = indent.Left;
            else if (alignment == Alignment.UpRight || alignment == Alignment.Right || alignment == Alignment.DownRight)
                charX = indent.Left + spaceW - textW;

            int charY = 0;
            if (alignment == Alignment.Center || alignment == Alignment.Left || alignment == Alignment.Right)
                charY = indent.Up + (int)Math.Floor((spaceH - textH) / 2d);
            else if (alignment == Alignment.UpLeft || alignment == Alignment.Up || alignment == Alignment.UpRight)
                charY = indent.Up;
            else if (alignment == Alignment.DownLeft || alignment == Alignment.Down || alignment == Alignment.DownRight)
                charY = indent.Up + spaceH - textH;

            int defaultCharX = charX;

            StatuePlaceStyleCounter = 0;
            foreach (var pair in Lines)
            {
                string line = pair.Text;
                int lineW = pair.Width;

                if (side == Side.Left)
                    charX = defaultCharX;
                else if (side == Side.Right)
                    charX = defaultCharX + textW - lineW;
                else if (side == Side.Center)
                    charX = defaultCharX + (int)Math.Floor((textW - lineW) / 2d);

			    for (int j = 0; j < line.Length; j++)
                {
				    char c = line[j];
                    int statueFrame = StatueTextFrame(c);
				    if (statueFrame >= 0)
                    {
					    for (int statueX = 0; statueX < 2; statueX++)
						    for (int statueY = 0; statueY <= Math.Min(lineH - 1, 3); statueY++)
                            {
                                dynamic tile = Tile(charX + statueX, charY + statueY);
                                if (tile == null)
                                    continue;
                                tile.frameX = (short)(statueFrame + statueX * 18);
                                tile.frameY = (short)(statueY * 18);
                                tile.active(true);
                                tile.type = (ushort)337; // TileID.AlphabetStatues
                                if (statueY < 2)
                                    tile.color(style.TextColor);
							    if (statueY == 2)
								    tile.color(style.TextUnderlineColor);
                                tile.inActive(style.InActive ?? false);
                            }
                        DrawWithSection = true;
                        charX += 2;
                    }
				    else
                    {
                        for (int x = 0; x < indent.Horizontal; x++)
                            for (int statueY = 0; statueY <= Math.Min(lineH - 1, 3); statueY++)
                                Tile(charX + x, charY + statueY)?.active(false);
					    charX += indent.Horizontal;
                    }
                }
                if (charY + 2 * lineH + indent.Vertical + indent.Down > Height)
                    break;
                charY = charY + lineH + indent.Vertical;
            }
        }

        #endregion
        #region UpdateThisNative

        protected override void UpdateThisNative()
        {
            base.UpdateThisNative();

            LabelStyle style = Style as LabelStyle;
            Indent indent = style.TextIndent;
            int spaceW = Width - indent.Left - indent.Right;
            int spaceH = Height - indent.Up - indent.Down;
            int lineH = 2 + (int)style.TextUnderline;

            StatuePlaceStyle.Clear();
            Regex.Replace(RawText, ItemPattern, match =>
            {
                GetPlaceStyleArgs args = new GetPlaceStyleArgs(int.Parse(match.Groups["id"].Value));
                TUI.Hooks.GetPlaceStyle.Invoke(args);
                StatuePlaceStyle.Add(args.PlaceStyle);
                return ReservedCharacter.ToString();
            });

            //Dividing text into lines
            (List<(string, int)> lines, int maxLineW) = LimitStatueText(RawText, spaceW, spaceH, indent.Horizontal, indent.Vertical, lineH);
            Lines = lines;
            TextW = maxLineW;
            TextH = lines.Count * (lineH + indent.Vertical) - indent.Vertical;
        }

        #endregion
        #region SetText

        /// <summary>
        /// Set text. Doesn't call update/apply/draw.
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetText(string value)
        {
            if (value.Contains(ReservedCharacter))
                throw new InvalidOperationException((int)ReservedCharacter + " is a reserved character.");
            RawText = value ?? throw new ArgumentNullException(nameof(value));
        }

        #endregion
        #region GetText

        public virtual string GetText() => RawText;

        #endregion

        #region StatueTextFrame

        public int StatueTextFrame(char c)
        {
            if (c == ReservedCharacter)
                return StatuePlaceStyle[StatuePlaceStyleCounter++] * 36;
            else if (c >= '0' && c <= '9')
                return (c - '0') * 36;
            else if (c >= 'a' && c <= 'z')
                return (10 + c - 'a') * 36;
            else if (c >= 'A' && c <= 'Z')
                return (10 + c - 'A') * 36;
            return -1;
        }

        #endregion
        #region LimitStatueText

        private static (List<(string, int)>, int MaxLineWidth) LimitStatueText(string text, int width, int height, int emptyIndentation, int linesIndentation, int lineH)
        {
            if (width < 2)
                return (new List<(string, int)>(), 0);
            List<(string, int)> result = new List<(string, int)>();
            int maxX = 0;
            // #result * (lineH + linesIndentation) - linesIndentation <= h
            int maxLines = (int)Math.Floor((double)(height + linesIndentation) / (lineH + linesIndentation));
            using (StringReader reader = new StringReader(text))
            {
                string line = string.Empty;
                do
                {
                    line = reader.ReadLine();
                    if (line != null)
                    {
                        int x = LimitStatueLine(line, width, emptyIndentation, result, maxLines - result.Count);
                        if (x > maxX)
                            maxX = x;
                        if (result.Count == maxLines)
                            break;
                    }

                } while (line != null);
            }
            return (result, maxX);
        }

        #endregion
        #region LimitStatueLine

        private static int LimitStatueLine(string line, int w, int emptyIndentation, List<(string, int)> result, int maxLines)
        {
            int lineLen = line.Length;
            int i = 0;
            int j = 0;
            int width = 0;
            int maxWidth = 0;
	        while (i < lineLen)
            {
                if (j >= lineLen)
                {
                    result.Add((line.Substring(i, j - i), width));
                    if (width > maxWidth)
                        maxWidth = width;
                    break;
                }
                char ch = line[j];
                int dx = IsStatueCharacter(ch) ? 2 : emptyIndentation;
		        if (width + dx > w)
                {
                    result.Add((line.Substring(i, (j - 1 - i + 1)), width));
                    if (width > maxWidth)
                        maxWidth = width;
                    if (result.Count == maxLines)
                        break;
			        while (j < lineLen - 1 && !IsStatueCharacter(ch))
				        j += 1;
                    i = j;
                    width = 0;
                }
		        else
                {
                    j += 1;
                    width += dx;
                }
            }
            return maxWidth;
        }

        #endregion
        #region IsStatueCharacter

        private static bool IsStatueCharacter(char c) =>
            c == ReservedCharacter || c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c >= '0' && c <= '9';

        #endregion
        #region FindMaxSize

        public static int FindMaxWidth(IEnumerable<string> values, int emptyIndentation)
        {
            int max = 0;
            foreach (var text in values)
            {
                int width = 0;
                foreach (char c in text)
                    if (c == '\n')
                    {
                        if (width > max)
                            max = width;
                        width = 0;
                    } else if (IsStatueCharacter(c))
                        width += 2;
                    else
                        width += emptyIndentation;
                if (width > max)
                    max = width;
            }
            return max;
        }

        #endregion
    }
}
