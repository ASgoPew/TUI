using System;
using System.Reflection;
using Terraria;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;
using TerrariaUI.Base;
using TerrariaUI.Base.Style;
using TerrariaUI.Widgets;
using TUIPlugin;
using TerrariaUI;

#pragma warning disable CS0162 // Unreachable code detected
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
            Create(true);
            Create(false);
        }

        private void Create(bool mainTileProvider)
        {
            // Determine the position and size of the interface.
            int x = 0, y = 0, w = 50, h = 40;
            // Pass an empty provider to the panel (the interface will be drawn on Main.tile).
            object provider = null;
            if (mainTileProvider)
                provider = new MainTileProvider();
            // Although we can use as a provider, for example, FakeTileRectangle from FakeManager:
            //object provider = FakeManager.FakeManager.Common.Add("TestPanelProvider", x, y, w, h);

            // Create a panel with a wall of diamond gemspark wall with black paint.
            Panel root = TUI.Create(new Panel(mainTileProvider ? "ExamplePluginPanel_MainTileProvider" : "ExamplePluginPanel_AutoProvider", x, y, w, h, null,
                new PanelStyle() { Wall = WallID.DiamondGemspark, WallColor = PaintID2.Black }, provider));
            // Create a Label widget (text display) with white characters.
            Label label1 = new Label(1, 1, 17, 2, "some text", new LabelStyle() { TextColor = PaintID2.White });
            // Add to panel
            root.Add(label1);

            // Create a container that occupies the lower (larger) half of our panel, painted over with white paint.
            // The Add function returns the newly added object in the VisualObject type,
            // so adding an element can be implemented as follows:
            VisualContainer node = root.Add(
                new VisualContainer(0, 15, w, 25, null, new ContainerStyle() { WallColor = PaintID2.White })
            );
            // Add a button to this container, which, when clicked, will send the clicker to the chat.
            /*node.Add(new Button(5, 0, 12, 4, "lol", null, new ButtonStyle()
            { Wall = 165, WallColor = PaintID2.DeepGreen }, (self, touch) =>
                  touch.Player().SendInfoMessage("You pressed lol button!")));*/

            if (false)
            {
                // Set the layout configuration.
                node.SetupLayout(Alignment.Center, Direction.Right, Side.Center, null, 3);
                // Add the InputLabel widget to the layout that allows you to input text.
                node.AddToLayout(new InputLabel(0, 0, new InputLabelStyle()
                    { TextColor = PaintID2.Black, Type = InputLabelType.All, TextUnderline = LabelUnderline.None },
                    new Input<string>("000", "000")));
                /*// Add to the layout one more ItemRack widget that corresponds to the Weapon rack: displaying an item
                // on a 3x3 rack. By clicking displays the relative and absolute coordinates of this click.
                node.AddToLayout(new ItemRack(0, 0, new ItemRackStyle() { Type = 200, Left = true }, (self, touch) =>
                    Console.WriteLine($"Touch: {touch.X}, {touch.Y}; absolute: {touch.AbsoluteX}, {touch.AbsoluteY}")));
                ItemRack irack1 = node.AddToLayout(new ItemRack(0, 0,
                    new ItemRackStyle() { Type = 201, Left = true }));
                // ItemRack allows you to add text on top using a sign:
                irack1.SetText("lololo\nkekeke");*/
                // Finally, add the slider to the layout.
                node.AddToLayout(new Slider(0, 0, 10, 2, new SliderStyle()
                    { Wall = WallID.AmberGemsparkOff, WallColor = PaintID2.White }));
            }

            if (false)
            {
                // Set up the grid configuration. Specify that it has to fill all the cells automatically.
                // Two columns (right size of 15, left - everything else) and two lines, occupying the same amount of space.
                node.SetupGrid(
                    new ISize[] { new Relative(100), new Absolute(15) }, // Размеры колонок
                    new ISize[] { new Relative(50), new Relative(50) }, // Размеры линий
                    null);
                // In the top left cell (at the intersection of the first column and the first line), set the background color to orange.
                node[0, 0].Style.WallColor = PaintID2.DeepOrange;
                // At the top right, we put a sapphire (blue) wall without paint.
                node[1, 0].Style.Wall = WallID.SapphireGemspark;
                node[1, 0].Style.WallColor = PaintID2.None;
                // In the bottom left cell, you can place the Label widget with the SandStoneSlab block.
                // Although the coordinates and sizes are specified as 0, they will automatically be
                // set, since the object is in the Grid.
                node[0, 1] = new Label(0, 0, 0, 0, "testing", null, new LabelStyle()
                {
                    Tile = TileID.SandStoneSlab,
                    TileColor = PaintID2.Red,
                    TextColor = PaintID2.Black
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
                    WallColor = PaintID2.DeepBlue,
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
                node.Add(new Label(0, 0, 16, 6, "test", new LabelStyle() { WallColor = PaintID2.DeepPink }))
                    .SetParentAlignment(Alignment.DownRight, new ExternalIndent() { Right = 3, Down = 1 });
            }

            if (false)
            {
                // Let's make our node container the size of the root in width.
                node.SetWidthParentStretch();
            }

            node.SetupLayout(Alignment.Center, Direction.Down, Side.Center, null, 1);

            // VisualObject
            VisualObject obj = node.AddToLayout(new VisualObject(5, 5, 8, 4, null, new UIStyle()
            {
                Wall = WallID.AmethystGemspark,
                WallColor = PaintID2.DeepPurple
            }, (self, touch) =>
                TSPlayer.All.SendInfoMessage($"Relative: ({touch.X}, {touch.Y}); Absolute: ({touch.AbsoluteX}, {touch.AbsoluteY})")));

            // VisualContainer
            //VisualContainer node2 = node.Add(
            //    new VisualContainer(5, 5, 20, 10, null, new ContainerStyle() { WallColor = PaintID2.Black })
            //) as VisualContainer;

            // Label
            Label label = node.AddToLayout(new Label(15, 5, 19, 4, "some text", new LabelStyle()
            {
                WallColor = PaintID2.DeepLime,
                TextColor = PaintID2.DeepRed
            }));

            // Button
            Button button = node.AddToLayout(new Button(15, 5, 12, 4, "lol", null, new ButtonStyle()
            {
                WallColor = PaintID2.DeepGreen,
                BlinkColor = PaintID2.Shadow,
                TriggerStyle = ButtonTriggerStyle.TouchEnd
            }, (self, touch) => touch.Player().SendInfoMessage("You released lol button!")));

            // Slider
            Slider slider = node.AddToLayout(new Slider(15, 5, 10, 2, new SliderStyle()
            {
                Wall = WallID.EmeraldGemspark,
                WallColor = PaintID2.White,
                SeparatorColor = PaintID2.Black,
                UsedColor = PaintID2.DeepOrange
            }, new Input<int>(0, 0, (self, value, playerIndex) =>
                (playerIndex >= 0
                    ? TShock.Players[playerIndex]
                    : TSPlayer.All)
                .SendInfoMessage("Slider value: " + value))));

            // Checkbox
            Checkbox checkbox = node.AddToLayout(new Checkbox(15, 5, 2, new CheckboxStyle()
            {
                Wall = WallID.EmeraldGemspark,
                WallColor = PaintID2.White,
                CheckedColor = PaintID2.DeepRed
            }, new Input<bool>(false, false, (self, value, playerIndex) =>
                TSPlayer.All.SendInfoMessage("Checkbox value: " + value))));

            // Separator
            Separator separator = node.AddToLayout(new Separator(6, new UIStyle()
            {
                Wall = 156,
                WallColor = PaintID2.DeepRed
            }));

            // InputLabel
            InputLabel input = node.AddToLayout(new InputLabel(15, 5, new InputLabelStyle()
            {
                Type = InputLabelType.All,
                TextUnderline = LabelUnderline.Underline,
                TextColor = PaintID2.DeepRed,
                TextUnderlineColor = PaintID2.Black // Этот параметр из LabelStyle
            }, new Input<string>("12345", "12345", (self, value, playerIndex) =>
                TSPlayer.All.SendInfoMessage("InputLabel value: " + value))));

            /*// ItemRack
            ItemRack irack = node.AddToLayout(new ItemRack(15, 5, new ItemRackStyle()
            {
                Type = ItemID.LargeDiamond,
                Size = ItemSize.Biggest,
                Left = true
            }));
            ItemRack irack2 = node.AddToLayout(new ItemRack(20, 5, new ItemRackStyle()
            {
                Type = ItemID.SnowmanCannon,
                Size = ItemSize.Smallest,
                Left = true
            }));
            irack2.SetText("This is a snowman cannon.");*/

            // VisualSign
            //VisualSign vsign = node.AddToLayout(new VisualSign(0, 0, "lmfao sosi(te pozhaluista)"));
            //VisualSign vsign2 = node.AddToLayout(new VisualSign(0, 0, "This is an example of what can happen " +
            //    "if you use signs in TUI without FakeManager (only $399!)." +
            //    "Text above would be empty. Even tho it has to have it..."));

            // FormField
            FormField ffield = node.AddToLayout(new FormField(
                new Checkbox(0, 0, 2, new CheckboxStyle()
                {
                    Wall = WallID.AmberGemspark,
                    WallColor = PaintID2.White,
                    CheckedColor = PaintID2.DeepRed
                }), 15, 5, 20, 2, "check me", new LabelStyle()
                {
                    TextColor = PaintID2.Shadow,
                    TextAlignment = Alignment.Left
                }, new ExternalIndent() { Right = 1 }));

            // Image
            Image image = node.AddToLayout(new Image(15, 5, "Media\\Image.TEditSch"));

            // Video
            Video video = node.AddToLayout(new Video(15, 5, 0, 0, null, new VideoStyle()
            {
                VideoName = "Media\\Animation-1",
                Delay = 100,
                TileColor = PaintID2.DeepTeal
            }, (self, touch) => (self as Video).ToggleStart()));

            // AlertWindow
            Button alertButton = node.AddToLayout(new Button(15, 10, 16, 4, "alert", null, new ButtonStyle()
            {
                Wall = WallID.AmberGemspark,
                WallColor = PaintID2.DeepOrange
            }, (self, touch) => ((Panel)node.Root).Alert("Hello world")));

            // ConfirmWindow
            Button confirmButton = node.AddToLayout(new Button(15, 13, 20, 4, "confirm\npls", null, new ButtonStyle()
            {
                Wall = WallID.AmberGemspark,
                WallColor = PaintID2.DeepTeal
            }, (self, touch) => ((Panel)node.Root).Confirm("Very nice", value => TSPlayer.All.SendInfoMessage("Confirmed? " + value))));

            // ScrollBackground
            // <Adding a lot of widgets to layout>
            // Specifying layer value as Int32.MinValue so that this widget would be under all other child objects,
            // although ScrollBackground specifies this layer by default in custructor so we don't have to do it manually.
            ScrollBackground scrollbg = node.Add(new ScrollBackground(true, true, true), Int32.MinValue);

            // ScrollBar
            ScrollBar scrollbar = node.Add(new ScrollBar(Direction.Right));

            // Arrow
            Arrow arrow = node.AddToLayout(new Arrow(15, 5, new ArrowStyle()
            {
                TileColor = PaintID2.DeepBlue,
                Direction = Direction.Left
            }));
        }
    }
}
