using System;
using System.Reflection;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;
using TUI.Base;
using TUI.Base.Style;
using TUI.Widgets;
using TUIPlugin;

namespace TUIExample
{
    [ApiVersion(2, 1)]
    public class TUIExamplePlugin : TerrariaPlugin
    {
        public override string Author => "ASgo";
        public override string Name => "TUIExamplePlugin";
        public override string Description => "Panel example, described in documentation";
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public TUIExamplePlugin(Main game) : base(game) { }

        public override void Initialize()
        {
            // Determine the position and size of the interface.
            int x = 0, y = 0, w = 50, h = 40;
            // Pass an empty provider to the panel (the interface will be drawn on Main.tile).
            object provider = null;
            // Although we can use as a provider, for example, FakeTileRectangle from FakeManager:
            //object provider = FakeManager.FakeManager.Common.Add("TestPanelProvider", x, y, w, h);

            // Create a panel with a wall of diamond gemspark wall with black paint.
            Panel root = TUI.TUI.Create(new Panel("TestPanel", x, y, w, h, null,
                new ContainerStyle() { Wall = WallID.DiamondGemspark, WallColor = TUI.Base.Style.PaintID.Black }, provider)) as Panel;
            // Create a Label widget (text display) with white characters.
            Label label1 = new Label(1, 1, 17, 2, "some text", new LabelStyle() { TextColor = TUI.Base.Style.PaintID.White });
            // Add to panel
            root.Add(label1);

            // Create a container that occupies the lower (larger) half of our panel, painted over with white paint.
            // The Add function returns the newly added object in the VisualObject type,
            // so adding an element can be implemented as follows:
            VisualContainer node = root.Add(
                new VisualContainer(0, 15, w, 25, null, new ContainerStyle() { WallColor = TUI.Base.Style.PaintID.White })
            ) as VisualContainer;
            // Add a button to this container, which, when clicked, will send the clicker to the chat.
            /*node.Add(new Button(5, 0, 12, 4, "lol", null, new ButtonStyle()
            { Wall = 165, WallColor = TUI.Base.Style.PaintID.DeepGreen }, (self, touch) =>
                  touch.Player().SendInfoMessage("You pressed lol button!")));*/

            if (false)
            {
                // Set the layout configuration.
                node.SetupLayout(Alignment.Center, Direction.Right, Side.Center, null, 3, false);
                // Add the InputLabel widget to the layout that allows you to input text.
                node.AddToLayout(new InputLabel(0, 0, new InputLabelStyle()
                    { TextColor = TUI.Base.Style.PaintID.Black, Type = InputLabelType.All, TextUnderline = LabelUnderline.None },
                    new Input<string>("000", "000")));
                // Add to the layout one more ItemRack widget that corresponds to the Weapon rack: displaying an item
                // on a 3x3 rack. By clicking displays the relative and absolute coordinates of this click.
                node.AddToLayout(new ItemRack(0, 0, new ItemRackStyle() { Type = 200, Left = true }, (self, touch) =>
                    Console.WriteLine($"Touch: {touch.X}, {touch.Y}; absolute: {touch.AbsoluteX}, {touch.AbsoluteY}")));
                ItemRack irack1 = node.AddToLayout(new ItemRack(0, 0,
                    new ItemRackStyle() { Type = 201, Left = true })) as ItemRack;
                // ItemRack allows you to add text on top using a sign:
                irack1.SetText("lololo\nkekeke");
                // Finally, add the slider to the layout.
                node.AddToLayout(new Slider(0, 0, 10, 2, new SliderStyle()
                    { Wall = WallID.AmberGemsparkOff, WallColor = TUI.Base.Style.PaintID.White }));
            }

            if (false)
            {
                // Set up the grid configuration. Specify that it has to fill all the cells automatically.
                // Two columns (right size of 15, left - everything else) and two lines, occupying the same amount of space.
                node.SetupGrid(
                    new ISize[] { new Relative(100), new Absolute(15) }, // Размеры колонок
                    new ISize[] { new Relative(50), new Relative(50) }, // Размеры линий
                    null, true);
                // In the top left cell (at the intersection of the first column and the first line), set the background color to orange.
                node[0, 0].Style.WallColor = TUI.Base.Style.PaintID.DeepOrange;
                // At the top right, we put a sapphire (blue) wall without paint.
                node[1, 0].Style.Wall = WallID.SapphireGemspark;
                node[1, 0].Style.WallColor = TUI.Base.Style.PaintID.None;
                // In the bottom left cell, you can place the Label widget with the SandStoneSlab block.
                // Although the coordinates and sizes are specified as 0, they will automatically be
                // set, since the object is in the Grid.
                node[0, 1] = new Label(0, 0, 0, 0, "testing", null, new LabelStyle()
                {
                    Tile = TileID.SandStoneSlab,
                    TileColor = TUI.Base.Style.PaintID.Red,
                    TextColor = TUI.Base.Style.PaintID.Black
                });
            }

            if (false)
            {
                // Install a large and complex grid.
                node.SetupGrid(new ISize[] { new Absolute(3), new Relative(50), new Absolute(6), new Relative(50) },
                    new ISize[] { new Relative(20), new Absolute(5), new Relative(80) });
                // Although we set the grid on the node, we can still add objects as before.
                // Add a button that draws the grid by pressing, and hides it when released.
                node.Add(new Button(3, 3, 10, 4, "show", null, new ButtonStyle()
                {
                    WallColor = TUI.Base.Style.PaintID.DeepBlue,
                    BlinkStyle = ButtonBlinkStyle.Full,
                    TriggerStyle = ButtonTriggerStyle.Both
                }, (self, touch) =>
                {
                    if (touch.State == TouchState.Begin)
                        node.ShowGrid();
                    else
                        node.Apply().Draw();
                }));
            }

            if (false)
            {
                // Add a label and immediately set Alignment.DownRight with indent 3 blocks to the right and 1 below.
                node.Add(new Label(0, 0, 16, 6, "test", new LabelStyle() { WallColor = TUI.Base.Style.PaintID.DeepPink }))
                    .SetAlignmentInParent(Alignment.DownRight, new ExternalIndent() { Right = 3, Down = 1 });
            }

            if (false)
            {
                // Let's make our node container the size of the root in width.
                node.SetFullSize(true, false);
            }

            node.SetupLayout(Alignment.Center, Direction.Down, Side.Center, null, 1, false);

            // VisualObject
            VisualObject obj = node.AddToLayout(new VisualObject(5, 5, 8, 4, null, new UIStyle()
            {
                Wall = WallID.AmethystGemspark,
                WallColor = TUI.Base.Style.PaintID.DeepPurple
            }, (self, touch) =>
                TSPlayer.All.SendInfoMessage($"Relative: ({touch.X}, {touch.Y}); Absolute: ({touch.AbsoluteX}, {touch.AbsoluteY})")));

            // VisualContainer
            //VisualContainer node2 = node.Add(
            //    new VisualContainer(5, 5, 20, 10, null, new ContainerStyle() { WallColor = TUI.Base.Style.PaintID.Black })
            //) as VisualContainer;

            // Label
            Label label = node.AddToLayout(new Label(15, 5, 19, 4, "some text", new LabelStyle()
            {
                WallColor = TUI.Base.Style.PaintID.DeepLime,
                TextColor = TUI.Base.Style.PaintID.DeepRed
            })) as Label;

            // Button
            Button button = node.AddToLayout(new Button(15, 5, 12, 4, "lol", null, new ButtonStyle()
            {
                WallColor = TUI.Base.Style.PaintID.DeepGreen,
                BlinkColor = TUI.Base.Style.PaintID.Shadow,
                TriggerStyle = ButtonTriggerStyle.TouchEnd
            }, (self, touch) => touch.Player().SendInfoMessage("You released lol button!"))) as Button;

            // Slider
            Slider slider = node.AddToLayout(new Slider(15, 5, 10, 2, new SliderStyle()
            {
                Wall = WallID.EmeraldGemspark,
                WallColor = TUI.Base.Style.PaintID.White,
                SeparatorColor = TUI.Base.Style.PaintID.Black,
                UsedColor = TUI.Base.Style.PaintID.DeepOrange
            }, new Input<int>(0, 0, (self, value, playerIndex) =>
                TShock.Players[playerIndex].SendInfoMessage("Slider value: " + value)))) as Slider;

            // Checkbox
            Checkbox checkbox = node.AddToLayout(new Checkbox(15, 5, 2, new CheckboxStyle()
            {
                Wall = WallID.EmeraldGemspark,
                WallColor = TUI.Base.Style.PaintID.White,
                CheckedColor = TUI.Base.Style.PaintID.DeepRed
            }, new Input<bool>(false, false, (self, value, playerIndex) =>
                TSPlayer.All.SendInfoMessage("Checkbox value: " + value)))) as Checkbox;

            // Separator
            Separator separator = node.AddToLayout(new Separator(6, new UIStyle()
            {
                Wall = 156,
                WallColor = TUI.Base.Style.PaintID.DeepRed
            })) as Separator;

            // InputLabel
            InputLabel input = node.AddToLayout(new InputLabel(15, 5, new InputLabelStyle()
            {
                Type = InputLabelType.All,
                TextUnderline = LabelUnderline.Underline,
                TextColor = TUI.Base.Style.PaintID.DeepRed,
                TextUnderlineColor = TUI.Base.Style.PaintID.Black // Этот параметр из LabelStyle
            }, new Input<string>("12345", "12345", (self, value, playerIndex) =>
                TSPlayer.All.SendInfoMessage("InputLabel value: " + value)))) as InputLabel;

            // ItemRack
            ItemRack irack = node.AddToLayout(new ItemRack(15, 5, new ItemRackStyle()
            {
                Type = ItemID.LargeDiamond,
                Size = ItemSize.Biggest,
                Left = true
            })) as ItemRack;
            ItemRack irack2 = node.AddToLayout(new ItemRack(20, 5, new ItemRackStyle()
            {
                Type = ItemID.SnowmanCannon,
                Size = ItemSize.Smallest,
                Left = true
            })) as ItemRack;
            irack2.SetText("This is a snowman cannon.");

            // VisualSign
            VisualSign vsign = node.AddToLayout(new VisualSign(0, 0, "lmfao sosi(te pozhaluista)")) as VisualSign;
            VisualSign vsign2 = node.AddToLayout(new VisualSign(0, 0, "This is an example of what can happen " +
                "if you use signs in TUI without FakeManager (only $399!)." +
                "Text above would be empty. Even tho it has to have it...")) as VisualSign;

            // FormField
            FormField ffield = node.AddToLayout(new FormField(
                new Checkbox(0, 0, 2, new CheckboxStyle()
                {
                    Wall = WallID.AmberGemspark,
                    WallColor = TUI.Base.Style.PaintID.White,
                    CheckedColor = TUI.Base.Style.PaintID.DeepRed
                }), 15, 5, 20, 2, "check me", new LabelStyle()
                {
                    TextColor = TUI.Base.Style.PaintID.Shadow,
                    TextAlignment = Alignment.Left
                }, new ExternalIndent() { Right = 1 })) as FormField;

            // Image
            Image image = node.AddToLayout(new Image(15, 5, "Media\\Image.TEditSch")) as Image;

            // Video
            Video video = node.AddToLayout(new Video(15, 5, null, new VideoStyle()
            {
                Path = "Media\\Animation-1",
                Delay = 100,
                TileColor = TUI.Base.Style.PaintID.DeepTeal
            }, (self, touch) => (self as Video).ToggleStart())) as Video;

            // AlertWindow
            Button alertButton = node.AddToLayout(new Button(15, 10, 16, 4, "alert", null, new ButtonStyle()
            {
                Wall = WallID.AmberGemspark,
                WallColor = TUI.Base.Style.PaintID.DeepOrange
            }, (self, touch) => node.Root.Alert("Hello world"))) as Button;

            // ConfirmWindow
            Button confirmButton = node.AddToLayout(new Button(15, 13, 20, 4, "confirm\npls", null, new ButtonStyle()
            {
                Wall = WallID.AmberGemspark,
                WallColor = TUI.Base.Style.PaintID.DeepTeal
            }, (self, touch) => node.Root.Confirm("Very nice", value => TSPlayer.All.SendInfoMessage("Confirmed? " + value)))) as Button;

            // ScrollBackground
            // <Adding a lot of widgets to layout>
            // Specifying layer value as Int32.MinValue so that this widget would be under all other child objects,
            // although ScrollBackground specifies this layer by default in custructor so we don't have to do it manually.
            ScrollBackground scrollbg = node.Add(new ScrollBackground(true, true, true), Int32.MinValue) as ScrollBackground;

            // ScrollBar
            ScrollBar scrollbar = node.Add(new ScrollBar(Direction.Right)) as ScrollBar;

            // Arrow
            Arrow arrow = node.AddToLayout(new Arrow(15, 5, new ArrowStyle()
            {
                TileColor = TUI.Base.Style.PaintID.DeepBlue,
                Direction = Direction.Left
            })) as Arrow;
        }
    }
}
