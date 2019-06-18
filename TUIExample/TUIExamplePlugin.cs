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
            // Создаем панель
            Panel root = TUI.TUI.CreatePanel("TestPanel", 100, 100, 50, 40, null,
                new ContainerStyle() { Wall = WallID.DiamondGemspark });
            // Создаем виджет Label (отображение текста)
            Label label1 = new Label(1, 1, 17, 2, "some text");
            // Добавляем к панели
            root.Add(label1);

            // Создаем контейнер, занимающий правую половину нашей панели, закрашенный черной краской
            // Функция Add возвращает только что добавленный объект в типе VisualObject,
            // так что добавление элемента можно реализовать следующим образом:
            VisualContainer node = root.Add(
                new VisualContainer(25, 0, 25, 40, null, new ContainerStyle() { WallColor = PaintID.Black })
            ) as VisualContainer;
            // В этот контейнер добавим кнопку, которая по нажатию будет отправлять нажавшему текст в чат.
            /*node.Add(new Button(0, 7, 12, 4, "lol", null, new ButtonStyle()
                { WallColor = PaintID.DeepGreen }, (self, touch) =>
                    touch.Player().SendInfoMessage("You pressed lol button!")));*/

            if (false)
            {
                // Настраиваем конфигурацию layout
                node.SetupLayout(Alignment.Center, Direction.Down, Side.Center, new ExternalOffset()
                    { Left = 5, Up = 5, Right = 5, Down = 5 }, 3, false);
                // Добавляем в layout виджет InputLabel, позволяющий вводить текст
                node.AddToLayout(new InputLabel(0, 0, new InputLabelStyle()
                    { TextColor = PaintID.White, Type = InputLabelType.All, TextUnderline = LabelUnderline.None }));
                // Добавляем в layout еще один виджет ItemRack, который соответствует Weapon rack: отображение предмета
                // на стойке размером 3х3. По нажатию выводит относительные и абсолютные координаты этого нажатия.
                node.AddToLayout(new ItemRack(0, 0, new ItemRackStyle() { Type = 200, Left = true }, (self, touch) =>
                    Console.WriteLine($"Touch: {touch.X}, {touch.Y}; absolute: {touch.AbsoluteX}, {touch.AbsoluteY}")));
                ItemRack irack1 = node.AddToLayout(new ItemRack(0, 0,
                    new ItemRackStyle() { Type = 201, Left = true })) as ItemRack;
                // ItemRack позволяет сверху добавть текст с помощью таблички:
                irack1.Set("lololo\nkekeke");
                // Наконец, добавляем слайдер в layout
                node.AddToLayout(new Slider(0, 0, 10, 2, new SliderStyle() {
                    Wall = WallID.AmberGemsparkOff, WallColor = PaintID.White }));
            }

            if (false)
            {
                // Настраиваем конфигуарцию сетки grid. Указываем, что нужно все ячейки заполнить автоматически.
                // Одна колонка размером с все доступное место и две линии: нижняя размером 16, остальное - верхняя.
                node.SetupGrid(new ISize[] { new Relative(100) }, new ISize[] { new Relative(100), new Absolute(16) }, null, true);
                // В первой ячейке (на пересечении первой колонки и первой линии) установим черный цвет фона
                node[0, 0].Style.WallColor = PaintID.Black;
                // А ячейке второй линии первой колонки назначим новый объект с белым цветом фона
                node[0, 1] = new VisualContainer(new ContainerStyle() { WallColor = PaintID.White });
                // Заметьте, что кнопка button не будет видна, потому что ее заслоняет объект первой линии сетки
            }

            if (false)
            {
                // Устанавливаем большую и сложную решетку
                node.SetupGrid(new ISize[] { new Absolute(3), new Relative(50), new Absolute(6), new Relative(50) },
                    new ISize[] { new Relative(20), new Absolute(5), new Relative(80) });
                // Через 10 секунд отрисовываем сетку
                Task.Delay(10000).ContinueWith(_ => node.ShowGrid());
            }

            if (false)
            {
                // Добавляем label и сразу устанавливаем Alignment с отступом 1 слева и снизу
                node.Add(new Label(0, 0, 8, 2, "test"))
                    .SetAlignmentInParent(Alignment.DownLeft, new ExternalOffset() { Left = 1, Down = 1 });
            }

            if (false)
            {
                // Добавляем желтый контейнер, устанавливаем его ширину на 3, а по высоте делаем FullSize,
                // затем указываем, что он должен быть в правом углу родителя.
                // Таким образом у нас получается желтая полоса справа с высотой node и шириной 3.
                node.Add(new VisualContainer(new ContainerStyle() { WallColor = PaintID.DeepYellow }))
                    .SetWH(3, 0).SetFullSize(false, true).SetAlignmentInParent(Alignment.Right);
            }



            node.SetupLayout(Alignment.Center, Direction.Down, Side.Center, null, 1, false);

            // VisualObject
            VisualObject obj = node.AddToLayout(new VisualObject(0, 0, 8, 4, null, new UIStyle() { WallColor = 15 },
                (self, touch) => Console.WriteLine(touch.X + " " + touch.Y)));

            // VisualContainer
            //VisualContainer node = root.Add(
            //    new VisualContainer(25, 0, 25, 40, null, new ContainerStyle() { WallColor = PaintID.Black })
            //) as VisualContainer;

            // Label
            Label label = node.AddToLayout(new Label(1, 1, 15, 2, "some text", new LabelStyle() { TextColor = 13 })) as Label;

            // Button
            Button button = node.AddToLayout(new Button(0, 7, 12, 4, "lol", null, new ButtonStyle()
            {
                WallColor = PaintID.DeepGreen
            }, (self, touch) => touch.Player().SendInfoMessage("You pressed lol button!"))) as Button;

            // Slider
            Slider slider = node.AddToLayout(new Slider(0, 0, 10, 2, new SliderStyle()
            {
                Wall = 157,
                WallColor = PaintID.White
            }, new Input<int>(0, 0, (self, value, playerIndex) =>
                TShock.Players[playerIndex].SendInfoMessage("Slider: " + value)))) as Slider;

            // Checkbox
            Checkbox checkbox = node.AddToLayout(new Checkbox(0, 0, 2, new CheckboxStyle()
            {
                Wall = 156,
                WallColor = PaintID.White
            }, new Input<bool>(false, false, (self, value, playerIndex) =>
                Console.WriteLine("Checkbox: " + value)))) as Checkbox;

            // Separator
            Separator separator = node.AddToLayout(new Separator(6, new UIStyle()
            {
                Wall = 156,
                WallColor = PaintID.DeepRed
            })) as Separator;

            // InputLabel
            InputLabel input = node.AddToLayout(new InputLabel(0, 0, new InputLabelStyle()
            {
                Type = InputLabelType.All,
                TextUnderline = LabelUnderline.None
            }, new Input<string>("12345", "12345", (self, value, playerIndex) =>
                Console.WriteLine("InputLabel: " + value)))) as InputLabel;

            // ItemRack
            ItemRack irack = node.AddToLayout(new ItemRack(0, 0, new ItemRackStyle()
            {
                Type = 200,
                Left = true
            }, (self, touch) => { })) as ItemRack;

            // VisualSign
            VisualSign vsign = node.AddToLayout(new VisualSign(0, 0, "lmfao sosi(te pozhaluista)")) as VisualSign;
            VisualSign vsign2 = node.AddToLayout(new VisualSign(0, 0, "This is an example of what can happen " +
                "if you use signs in TUI without FakeManager (only $399!)." +
                "Text above would be empty. Even tho it has to have it...")) as VisualSign;

            // FormField
            FormField ffield = node.AddToLayout(new FormField(
                new Checkbox(0, 0, 2, new CheckboxStyle() { CheckedColor=13 }),
                0, 0, 0, 2, "VSign", new LabelStyle()
            {
                TextColor = 29,
                TextAlignment = Alignment.Left
            }, new ExternalOffset() { Right = 1 })) as FormField;
            ffield.SetFullSize(true);

            // Image
            Image image = node.AddToLayout(new Image(2, 2, "Media\\Help.TEditSch")) as Image;

            // Video
            Video video = node.AddToLayout(new Video(2, 2, null, new VideoStyle()
            {
                Path = "Media\\Animation-1",
                Delay = 100,
                TileColor = PaintID.DeepTeal
            }, (self, touch) => (self as Video).ToggleStart())) as Video;

            // AlertWindow
            Task.Delay(10000).ContinueWith(_ => node.Root.Alert("Hello world"));

            // ConfirmWindow
            Task.Delay(10000).ContinueWith(_ => node.Root.Confirm("Hello world", value => Console.WriteLine(value)));

            // ScrollBackground
            // Указываем layer (слой) в значение Int32.MinValue, чтобы виджет был сзади всех прочих виджетов
            ScrollBackground scrollbg = node.Add(new ScrollBackground(true, true, true), Int32.MinValue) as ScrollBackground;

            // ScrollBar
            ScrollBar scrollbar = node.Add(new ScrollBar(Direction.Right)) as ScrollBar;

            // Arrow
            Arrow arrow = node.AddToLayout(new Arrow(0, 0, new ArrowStyle() { Direction = Direction.Left })) as Arrow;
        }
    }
}
