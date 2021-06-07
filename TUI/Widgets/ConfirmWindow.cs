using System;
using System.Linq;
using TerrariaUI.Base;
using TerrariaUI.Base.Style;

namespace TerrariaUI.Widgets
{
    public class ConfirmWindow : VisualObject
    {
        #region Data

        public Label Label { get; set; }
        public Button YesButton { get; set; }
        public Button NoButton { get; set; }
        public Action<bool> ConfirmCallback { get; set; }
        private VisualContainer Container { get; set; }

        #endregion

        #region Constructor

        public ConfirmWindow(string text, Action<bool> callback, ContainerStyle style = null,
                ButtonStyle yesButtonStyle = null, ButtonStyle noButtonStyle = null)
            : base(0, 0, 0, 0)
        {
            ConfirmCallback = callback ?? throw new ArgumentNullException(nameof(callback));

            SetParentAlignment(Alignment.Center);
            SetParentStretch(FullSize.Both);

            Container = Add(new VisualContainer(style ?? new ContainerStyle()
                { Wall = 165, WallColor = 27 }));
            Container.SetParentAlignment(Alignment.Center)
                .SetParentStretch(FullSize.Horizontal)
                .SetupLayout(Alignment.Center, Direction.Down, childIndent: 0);

            int lines = (text?.Count(c => c == '\n') ?? 0) + 1;
            Label = Container.AddToLayout(new Label(0, 0, 0, 1 + lines * 3, text, null,
                new LabelStyle() { TextIndent = new Indent() { Horizontal = 1, Vertical = 1 } }));
            Label.SetParentStretch(FullSize.Horizontal);

            VisualContainer yesno = Container.AddToLayout(new VisualContainer(0, 0, 24, 4));

            yesButtonStyle = yesButtonStyle ?? new ButtonStyle()
            {
                WallColor = PaintID2.DeepGreen,
                BlinkStyle = ButtonBlinkStyle.Full,
                BlinkColor = PaintID2.White
            };
            yesButtonStyle.TriggerStyle = ButtonTriggerStyle.TouchEnd;
            YesButton = yesno.Add(new Button(0, 0, 12, 4, "yes", null, yesButtonStyle,
                ((self, touch) =>
                {
                    ((Panel)self.Root).HidePopUp();
                    callback.Invoke(true);
                })));

            noButtonStyle = noButtonStyle ?? new ButtonStyle()
            {
                WallColor = PaintID2.DeepRed,
                BlinkStyle = ButtonBlinkStyle.Full,
                BlinkColor = PaintID2.White
            };
            noButtonStyle.TriggerStyle = ButtonTriggerStyle.TouchEnd;
            NoButton = yesno.Add(new Button(12, 0, 12, 4, "no", null, noButtonStyle,
                ((self, touch) =>
                {
                    ((Panel)self.Root).HidePopUp();
                    callback.Invoke(false);
                })));

            Callback = CancelCallback;
            Container.SetWH(0, Label.Height + yesno.Height, false);
        }

        #endregion
        #region CancelCallback

        private void CancelCallback(VisualObject self, Touch touch)
        {
            ((ConfirmWindow)self).ConfirmCallback.Invoke(false);
            ((Panel)self.Root).HidePopUp();
        }

        #endregion
        #region Copy

        public ConfirmWindow(ConfirmWindow confirmWindow)
            : this(confirmWindow.Label.GetText(), confirmWindow.ConfirmCallback.Clone() as Action<bool>,
                new ContainerStyle(confirmWindow.Container.ContainerStyle),
                new ButtonStyle(confirmWindow.YesButton.ButtonStyle),
                new ButtonStyle(confirmWindow.NoButton.ButtonStyle))
        {
        }

        #endregion
    }
}
