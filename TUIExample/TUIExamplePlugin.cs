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
            // Определяем позицию (по умолчанию) и размеры интерфейса.
            int x = 100, y = 100, w = 50, h = 40;
            // Передаем в панель пустой провайдер (интерфейс будет рисоваться на Main.tile).
            object provider = null;
            // Хотя можем использовать в качестве провайдера, например, FakeTileRectangle из FakeManager:
            //object provider = FakeManager.FakeManager.Common.Add("TestPanelProvider", x, y, w, h);

            // Создаем панель со стеной Diamond gemspark wall с черной краской.
            Panel root = TUI.TUI.Create(new Panel("TestPanel", x, y, w, h, null,
                new ContainerStyle() { Wall = WallID.DiamondGemspark, WallColor = PaintID.Black }, provider)) as Panel;
            // Создаем виджет Label (отображение текста) с белыми символами.
            Label label1 = new Label(1, 1, 17, 2, "some text", new LabelStyle() { TextColor = PaintID.White });
            // Добавляем к панели
            root.Add(label1);

            // Создаем контейнер, занимающий нижнюю (большую) половину нашей панели, закрашенный белой краской.
            // Функция Add возвращает только что добавленный объект в типе VisualObject,
            // так что добавление элемента можно реализовать следующим образом:
            VisualContainer node = root.Add(
                new VisualContainer(0, 15, w, 25, null, new ContainerStyle() { WallColor = PaintID.White })
            ) as VisualContainer;
            // В этот контейнер добавим кнопку, которая по нажатию будет отправлять нажавшему текст в чат.
            //node.Add(new Button(5, 0, 12, 4, "lol", null, new ButtonStyle()
            //    { Wall=165, WallColor = PaintID.DeepGreen }, (self, touch) =>
            //        touch.Player().SendInfoMessage("You pressed lol button!")));

            if (false)
            {
                // Настраиваем конфигурацию layout.
                node.SetupLayout(Alignment.Center, Direction.Right, Side.Center, null, 3, false);
                // Добавляем в layout виджет InputLabel, позволяющий вводить текст.
                node.AddToLayout(new InputLabel(0, 0, new InputLabelStyle()
                    { TextColor = PaintID.Black, Type = InputLabelType.All, TextUnderline = LabelUnderline.None },
                    new Input<string>("000", "000")));
                // Добавляем в layout еще один виджет ItemRack, который соответствует Weapon rack: отображение предмета
                // на стойке размером 3х3. По нажатию выводит относительные и абсолютные координаты этого нажатия.
                node.AddToLayout(new ItemRack(0, 0, new ItemRackStyle() { Type = 200, Left = true }, (self, touch) =>
                    Console.WriteLine($"Touch: {touch.X}, {touch.Y}; absolute: {touch.AbsoluteX}, {touch.AbsoluteY}")));
                ItemRack irack1 = node.AddToLayout(new ItemRack(0, 0,
                    new ItemRackStyle() { Type = 201, Left = true })) as ItemRack;
                // ItemRack позволяет сверху добавть текст с помощью таблички:
                irack1.SetText("lololo\nkekeke");
                // Наконец, добавляем слайдер в layout.
                node.AddToLayout(new Slider(0, 0, 10, 2, new SliderStyle() {
                    Wall = WallID.AmberGemsparkOff, WallColor = PaintID.White }));
            }

            if (false)
            {
                // Настраиваем конфигуарцию сетки grid. Указываем, что нужно все ячейки заполнить автоматически.
                // Две колонки (правая размером 15, левая - все остальное) и две линии, занимающие одинаковое количество места.
                node.SetupGrid(
                    new ISize[] { new Relative(100), new Absolute(15) }, // Размеры колонок
                    new ISize[] { new Relative(50), new Relative(50) }, // Размеры линий
                    null, true);
                // В левой верхней ячейке (на пересечении первой колонки и первой линии) установим оранжевый цвет фона.
                node[0, 0].Style.WallColor = PaintID.DeepOrange;
                // В правой верхней поставим сапфировую (синюю) стену без краски.
                node[1, 0].Style.Wall = WallID.SapphireGemspark;
                node[1, 0].Style.WallColor = PaintID.None;
                // В левой нижней ячейке можно расположить виджет Label с блоком SandStoneSlab.
                // Несмотря на то, что координаты и размеры указаны как 0, они автоматически будут
                // установлены, так как объект находится в решетке Grid.
                node[0, 1] = new Label(0, 0, 0, 0, "testing", null, new LabelStyle()
                {
                    Tile = TileID.SandStoneSlab,
                    TileColor = PaintID.Red,
                    TextColor = PaintID.Black
                });
            }

            if (false)
            {
                // Устанавливаем большую и сложную решетку.
                node.SetupGrid(new ISize[] { new Absolute(3), new Relative(50), new Absolute(6), new Relative(50) },
                    new ISize[] { new Relative(20), new Absolute(5), new Relative(80) });
                // Хоть мы и установили решетку у node, мы все еще можем добавлять объекты по-старому.
                // Добавим кнопку, которая по нажатию отрисовывает сетку, а по отпусканию скрывает ее.
                node.Add(new Button(3, 3, 10, 4, "show", null, new ButtonStyle()
                {
                    WallColor = PaintID.DeepBlue,
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
                // Добавляем label и сразу устанавливаем Alignment.DownRight с отступом 3 блока справа и 1 снизу.
                node.Add(new Label(0, 0, 16, 6, "test", new LabelStyle() { WallColor = PaintID.DeepPink }))
                    .SetAlignmentInParent(Alignment.DownRight, new ExternalOffset() { Right = 3, Down = 1 });
            }

            if (false)
            {
                // Добавляем желтый контейнер, устанавливаем его ширину на 5, а по высоте делаем FullSize,
                // затем указываем, что он должен быть в правом углу родителя.
                // Таким образом у нас получается желтая полоса справа с высотой node и шириной 5.
                /*node.Add(new VisualContainer(new ContainerStyle() { WallColor = PaintID.DeepYellow }))
                    .SetWH(5, 0)
                    .SetFullSize(false, true)
                    .SetAlignmentInParent(Alignment.Right);*/

                // Сделаем наш контейнер node размером с корневой root по ширине.
                node.SetFullSize(true, false);
            }

            node.SetupLayout(Alignment.Center, Direction.Down, Side.Center, null, 1, false);

            // VisualObject
            //VisualObject obj = node.AddToLayout(new VisualObject(5, 5, 8, 4, null, new UIStyle()
            //{
            //    Wall = WallID.AmethystGemspark,
            //    WallColor = PaintID.DeepPurple
            //}, (self, touch) =>
            //    TSPlayer.All.SendInfoMessage($"Relative: ({touch.X}, {touch.Y}); Absolute: ({touch.AbsoluteX}, {touch.AbsoluteY})")));

            // VisualContainer
            //VisualContainer node2 = node.Add(
            //    new VisualContainer(5, 5, 20, 10, null, new ContainerStyle() { WallColor = PaintID.Black })
            //) as VisualContainer;

            // Label
            //Label label = node.AddToLayout(new Label(15, 5, 19, 4, "some text", new LabelStyle()
            //{
            //    WallColor = PaintID.DeepLime,
            //    TextColor = PaintID.DeepRed
            //})) as Label;

            // Button
            //Button button = node.AddToLayout(new Button(15, 5, 12, 4, "lol", null, new ButtonStyle()
            //{
            //    WallColor = PaintID.DeepGreen,
            //    BlinkColor = PaintID.Shadow,
            //    TriggerStyle = ButtonTriggerStyle.TouchEnd
            //}, (self, touch) => touch.Player().SendInfoMessage("You released lol button!"))) as Button;

            // Slider
            //Slider slider = node.AddToLayout(new Slider(15, 5, 10, 2, new SliderStyle()
            //{
            //    Wall = WallID.EmeraldGemspark,
            //    WallColor = PaintID.White,
            //    SeparatorColor = PaintID.Black,
            //    UsedColor = PaintID.DeepOrange
            //}, new Input<int>(0, 0, (self, value, playerIndex) =>
            //    TShock.Players[playerIndex].SendInfoMessage("Slider value: " + value)))) as Slider;

            // Checkbox
            //Checkbox checkbox = node.AddToLayout(new Checkbox(15, 5, 2, new CheckboxStyle()
            //{
            //    Wall = WallID.EmeraldGemspark,
            //    WallColor = PaintID.White,
            //    CheckedColor = PaintID.DeepRed
            //}, new Input<bool>(false, false, (self, value, playerIndex) =>
            //    TSPlayer.All.SendInfoMessage("Checkbox value: " + value)))) as Checkbox;

            // Separator
            //Separator separator = node.AddToLayout(new Separator(6, new UIStyle()
            //{
            //    Wall = 156,
            //    WallColor = PaintID.DeepRed
            //})) as Separator;

            // InputLabel
            //InputLabel input = node.AddToLayout(new InputLabel(15, 5, new InputLabelStyle()
            //{
            //    Type = InputLabelType.All,
            //    TextUnderline = LabelUnderline.Underline,
            //    TextColor = PaintID.DeepRed,
            //    TextUnderlineColor = PaintID.Black // Этот параметр из LabelStyle
            //}, new Input<string>("12345", "12345", (self, value, playerIndex) =>
            //    TSPlayer.All.SendInfoMessage("InputLabel value: " + value)))) as InputLabel;

            // ItemRack
            //ItemRack irack = node.AddToLayout(new ItemRack(15, 5, new ItemRackStyle()
            //{
            //    Type = ItemID.LargeDiamond,
            //    Size = ItemSize.Biggest,
            //    Left = true
            //})) as ItemRack;
            //ItemRack irack2 = node.AddToLayout(new ItemRack(20, 5, new ItemRackStyle()
            //{
            //    Type = ItemID.SnowmanCannon,
            //    Size = ItemSize.Smallest,
            //    Left = true
            //})) as ItemRack;
            //irack2.SetText("This is a snowman cannon.");

            // VisualSign
            //VisualSign vsign = node.AddToLayout(new VisualSign(0, 0, "lmfao sosi(te pozhaluista)")) as VisualSign;
            //VisualSign vsign2 = node.AddToLayout(new VisualSign(0, 0, "This is an example of what can happen " +
            //    "if you use signs in TUI without FakeManager (only $399!)." +
            //    "Text above would be empty. Even tho it has to have it...")) as VisualSign;

            // FormField
            //FormField ffield = node.AddToLayout(new FormField(
            //    new Checkbox(0, 0, 2, new CheckboxStyle()
            //    {
            //        Wall = WallID.AmberGemspark,
            //        WallColor = PaintID.White,
            //        CheckedColor = PaintID.DeepRed
            //    }), 15, 5, 20, 2, "check me", new LabelStyle()
            //{
            //    TextColor = PaintID.Shadow,
            //    TextAlignment = Alignment.Left
            //}, new ExternalOffset() { Right = 1 })) as FormField;
            //ffield.SetFullSize(true);

            // Image
            Image image = node.Add(new Image(15, 0, "Media\\Image.TEditSch")) as Image;

            // Video
            //Video video = node.AddToLayout(new Video(15, 5, null, new VideoStyle()
            //{
            //    Path = "Media\\Animation-1",
            //    Delay = 100,
            //    TileColor = PaintID.DeepTeal
            //}, (self, touch) => (self as Video).ToggleStart())) as Video;

            // AlertWindow
            //Button alertButton = node.AddToLayout(new Button(15, 10, 16, 4, "alert", null, new ButtonStyle()
            //{
            //    Wall = WallID.AmberGemspark,
            //    WallColor = PaintID.DeepOrange
            //}, (self, touch) => node.Root.Alert("Hello world"))) as Button;

            // ConfirmWindow
            //Button confirmButton = node.AddToLayout(new Button(15, 13, 20, 4, "confirm\npls", null, new ButtonStyle()
            //{
            //    Wall = WallID.AmberGemspark,
            //    WallColor = PaintID.DeepTeal
            //}, (self, touch) => node.Root.Confirm("Very nice", value => TSPlayer.All.SendInfoMessage("Confirmed? " + value)))) as Button;

            // ScrollBackground
            // Указываем layer (слой) в значение Int32.MinValue, чтобы виджет был сзади всех прочих виджетов
            //ScrollBackground scrollbg = node.Add(new ScrollBackground(true, true, true), Int32.MinValue) as ScrollBackground;

            // ScrollBar
            //ScrollBar scrollbar = node.Add(new ScrollBar(Direction.Right)) as ScrollBar;

            // Arrow
            //Arrow arrow = node.Add(new Arrow(15, 5, new ArrowStyle()
            //{
            //    TileColor = PaintID.DeepBlue,
            //    Direction = Direction.Left
            //})) as Arrow;
        }
    }
}
