using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TUI.Base;

namespace TUI.Widgets
{
    public enum LabelUnderline
    {
        Nothing = 0,
        Underline
    }

    public class LabelStyle : UIStyle
    {
        public Indentation Indentation { get; set; } = (Indentation)UIDefault.LabelIndentation.Clone();
        public Alignment Alignment { get; set; } = UIDefault.Alignment;
        public Side Side { get; set; } = UIDefault.Side;
        public byte TextColor { get; set; } = UIDefault.LabelTextColor;
        public LabelUnderline Underline { get; set; } = LabelUnderline.Nothing;
        public byte UnderlineColor { get; set; } = UIDefault.LabelTextColor;
    }

    public class Label : VisualObject
    {
        #region Data

        public override string Name => $"{GetType().Name} ({RawText})";
        private string RawText { get; set; }
        private List<(string Text, int Width)> Lines { get; set; }
        private int TextW { get; set; }
        private int TextH { get; set; }

        #endregion

        #region Initialize

        public Label(int x, int y, int width, int height, string text, UIConfiguration configuration = null, LabelStyle style = null, Func<VisualObject, Touch, bool> callback = null)
            : base(x, y, width, height, configuration, style, callback)
        {
            RawText = text;
        }

        #endregion
        #region ApplyThisNative

        protected override void ApplyThisNative(bool forceClear = false)    
        {
            base.ApplyThisNative(forceClear);

            LabelStyle style = Style as LabelStyle;
            string text = GetText();
            if (string.IsNullOrWhiteSpace(text))
                return;
            int lineH = 2 + (int)style.Underline;
            ForceSection = false;
            Indentation indentation = style.Indentation;
            Alignment alignment = style.Alignment;
            Side side = style.Side;

            (int sx, int sy) = AbsoluteXY();
            int spaceW = Width - indentation.Left - indentation.Right;
            int spaceH = Height - indentation.Up - indentation.Down;
		    int textW = TextW;
		    int textH = TextH;

            if (spaceH < lineH)
                return;

            int charX = 0;
            if (alignment == Alignment.Center || alignment == Alignment.Up || alignment == Alignment.Down)
                charX = indentation.Left + (int)Math.Floor((spaceW - textW) / 2d);
            else if (alignment == Alignment.UpLeft || alignment == Alignment.Left || alignment == Alignment.DownLeft)
                charX = indentation.Left;
            else if (alignment == Alignment.UpRight || alignment == Alignment.Right || alignment == Alignment.DownRight)
                charX = indentation.Left + spaceW - textW;

            int charY = 0;
            if (alignment == Alignment.Center || alignment == Alignment.Left || alignment == Alignment.Right)
                charY = indentation.Up + (int)Math.Floor((spaceH - textH) / 2d);
            else if (alignment == Alignment.UpLeft || alignment == Alignment.Up || alignment == Alignment.UpRight)
                charY = indentation.Up;
            else if (alignment == Alignment.DownLeft || alignment == Alignment.Down || alignment == Alignment.DownRight)
                charY = indentation.Up + spaceH - textH;

            int defaultCharX = charX;

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
                                dynamic t = Provider.Tile[sx + charX + statueX - Provider.X, sy + charY + statueY - Provider.Y];
                                t.frameX = (short)(statueFrame + statueX * 18);
                                t.frameY = (short)(statueY * 18);
                                t.active(true);
                                t.type = (ushort)337; // TileID.AlphabetStatues
                                if (statueY < 2)
                                    t.color(style.TextColor);
							    if (statueY == 2)
								    t.color(style.UnderlineColor);
                                t.inActive(false);
                            }
                        ForceSection = true;
                        Console.WriteLine("WTF FORCESECTION TRUE");
                        charX += 2;
                    }
				    else
					    charX += indentation.Horizontal;
                }
                if (charY + 2 * lineH + indentation.Vertical + indentation.Down > Height)
                    break;
                charY = charY + lineH + indentation.Vertical;
            }
        }

        #endregion
        #region UpdateThisNative

        protected override void UpdateThisNative()
        {
            base.UpdateThisNative();

            LabelStyle style = Style as LabelStyle;
            Indentation indentation = style.Indentation;
            int spaceW = Width - indentation.Left - indentation.Right;
            int spaceH = Height - indentation.Up - indentation.Down;
            int lineH = 2 + (int)style.Underline;

            //Dividing text into lines
            (List<(string, int)> lines, int maxLineW) = LimitStatueText(RawText, spaceW, spaceH, indentation.Horizontal, indentation.Vertical, lineH);
            Lines = lines;
            TextW = maxLineW;
            TextH = lines.Count * (lineH + indentation.Vertical) - indentation.Vertical;
        }

        #endregion
        #region SetText

        public void SetText(string text)
        {
            RawText = text;
            UpdateThis();
        }

        #endregion
        #region GetText

        public string GetText() => RawText;

        #endregion
        #region Clone

        public override object Clone() =>
            new Label(X, Y, Width, Height, string.Copy(RawText), (UIConfiguration)Configuration.Clone(), (LabelStyle)Style.Clone(), (Func<VisualObject, Touch, bool>)Callback.Clone());

        #endregion

        #region StatueTextFrame

        private static int StatueTextFrame(char c)
        {
            if (c >= '0' && c <= '9')
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
                return (null, 0);
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
                        int x = LimitStatueLine(line.Trim(), width, emptyIndentation, result, maxLines - result.Count);
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
            c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c >= '0' && c <= '9';

        #endregion
    }
}
