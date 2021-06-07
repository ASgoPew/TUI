using System.Linq;
using TerrariaUI.Base;
using TerrariaUI.Base.Style;

namespace TerrariaUI.Widgets
{
    public class AlertWindow : VisualContainer
    {
        #region Data

        public Label Label { get; set; }
        public Button Button { get; set; }

        #endregion

        #region Constructor

        public AlertWindow(string text, ContainerStyle windowStyle = null, ButtonStyle buttonStyle = null)
            : base(0, 0, 0, 0, null, windowStyle ?? new ContainerStyle() { Wall = 165, WallColor = 27 })
        {
            SetParentAlignment(Alignment.Center);
            SetupLayout(Alignment.Center, Direction.Down, childIndent: 0);
            int lines = (text?.Count(c => c == '\n') ?? 0) + 1;
            Label = AddToLayout(new Label(0, 0, 0, 1 + lines * 3, text, null,
                new LabelStyle() { TextIndent = new Indent() { Horizontal = 1, Vertical = 1 } }));
            Label.SetParentStretch(FullSize.Horizontal);
            buttonStyle = buttonStyle ?? new ButtonStyle()
            {
                WallColor = PaintID2.DeepGreen,
                BlinkStyle = ButtonBlinkStyle.Full,
                BlinkColor = PaintID2.White
            };
            buttonStyle.TriggerStyle = ButtonTriggerStyle.TouchEnd;
            Button = AddToLayout(new Button(0, 0, 14, 4, "ok", null, buttonStyle,
                ((self, touch) => ((Panel)self.Root).HidePopUp())));
            SetWH(0, Label.Height + Button.Height, false);
            SetParentStretch(FullSize.Horizontal);
        }

        #endregion
        #region Copy

        public AlertWindow(AlertWindow alertWindow)
            : this(alertWindow.Label.GetText(), new ContainerStyle(alertWindow.ContainerStyle),
                new ButtonStyle(alertWindow.Button.ButtonStyle))
        {
        }

        #endregion
    }
}
