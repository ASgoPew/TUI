# Документация TUI

Библиотека предоставляет возможность пользователю возможность создавать области блоков на карте,
которые работают по принципу обычного пользовательского интерфейса: на эту область можно нажимать
с помощью the grand design (великий план) и что-то может происходить. Например, можно поставить
кнопку, которая по нажатию будет выводить в чат что-то. И в принципе разработчик ограничен лишь своей
фантазией в том, что можно создать на пользовательском интерфейсе. Примером может быть настольная
игра сапер, на которую ушло 314 строк кода (включая логику самой игры):

![](minesweeper.png)

Эти и некоторые другие примеры игр вы можете посмотреть в измерении games на сервере terraria-servers.ru:7777.

## Содержание
* [Основы интерфейса](#Основы-интерфейса)
* [Базовые операции VisualObject](#Базовые-операции-VisualObject)
* [4 независимых способа автоматического регулирования позиций и размеров объектов](#4-независимых-способа-автоматического-регулирования-позиций-и-размеров-объектов)
* [Layout](#Layout)
* [Grid](#Grid)
* [Alignment](#Alignment)
* [FullSize](#FullSize)
* [Как происходит нажатие](#Как-происходит-нажатие)
* [Класс UIConfiguration](#Класс-UIConfiguration)
* [Класс UIStyle](#Класс-UIStyle)
* [Сигналы PulseType](#Сигналы-PulseType)
* [База данных](#База-данных)
* [Виджеты](#Виджеты)
* [Общие факты о клиентской стороне управления интерфейсом](#Общие-факты-о-клиентской-стороне-управления-интерфейсом)

## Основы интерфейса

Каждый элемент, будь то кнопка, надпись, поле ввода, слайдер или что-то еще, является объектом класса,
наследуемого от базового класса VisualObject. Например, слайдер - это класс Slider, наследующийся сразу
от VisualObject. А кнопка - это класс Button, наследующийся от класса Label, который тоже в свою очередь
наследуется от VisualObject. Любой виджет, работающий в этой библиотеке обязан наследоваться от VisualObject.
Весь интерфейс сам по себе представляет из себя набор деревьев, каждая вершина которого - VisualObject
или объект класса, наследующегося от VisualObject. Таким образом, игра сапер представляет из себя одно из
таких деревьев. При этом корень дерева - это всегда объект класса RootVisualObject или класса,
наследующегося от RootVisualObject (например Panel). Для разработчика приложений на интерфейсе объект
класса RootVisualObject не особо отличается от обычного VisualObject, потому как RootVisualObject наследуется
от VisualObject и лишь добавляет некоторые поля и функционал (например, функция всплывающего окна).

![](VisualObjectTree.png)

Дочерние элементы объекта VisualObject находятся в поле закрытом поле Child (List<VisualObject>).
Обычно не требуется обращение к этому списку напрямую, но в целях отладки это возможно: GetChild(int index)
Родительсткая вершина хранится в поле Parent (VisualObject).
Корень дерева доступен по геттеру Root (RootVisualObject). Учтите, что получить доступ к этому полю
можно только после того, как будет вызван Update всего дерева. Чтобы получить корень дерева
до вызова Update(), воспользуйтесь методом GetRoot().
Добавить дочерний элемент можно несколькими способами, например - вызвав функцию Add:

```cs
VisualObject Add(VisualObject newChild)
```

## Базовые операции VisualObject

Есть несколько важных операций, которые можно применять к объектам VisualObject:
1. **Update()**
	* Рекурсивная функция, обновляющая каждый из объектов поддерева (устанавливает нужные значения в
	нужные поля, рассчитывает позицию относительных элементов, ...)
2. **Apply()**
	* Рекурсивная функция, отрисовывающая объект на карте (изменение блоков карты в соостветствии
	со стилем отображения элемента)
3. **Draw()**
	* Отправка отрисованного (хотя не обязательно) объекта игрокам с помощью SendSection или SendTileSquare
4. **Pulse(PulseType)**
	* Отправка указанного сигнала дереву объектов

Эти три операции обычно идут в указанном порядке и делают примерно следующее:
Вызов Update вычисляет некторые свои поля (Root, ProviderX, ProviderY, ...), потом вычисляет позиции
дочерних элементов и запускает рекурсивно вызов Update у дочерних элементов.
Вызов Apply, например, устанавливает во всей области объекта 155 стену (diamond gemspark wall),
затем рекурсивно вызывает Apply у дочерних элементов.
Вызов Draw отправляет секцию или SendTileSquare всем игрокам.

## Класс TUI

Существует статичный класс TUI, который представляет из себя список корней RootVisualObject
и обладает операциями, похожими на описанные выше для VisualObject:
Update, Apply, Draw
Эти функции делают одноименный вызов для всех корней. Таким образом, чтобы полностью обновить
и отрисовать все деревья пользовательского интерфейса, необходимо выполнить:
UI.Update();
UI.Apply();
UI.Draw();

Чтобы создать новый интерфейс, необходимо вызвать один из методов класса TUI:
```cs
RootVisualObject CreateRoot(string name, int x, int y, int width, int height,
	UIConfiguration configuration = null, UIStyle style = null, object provider = null)
```
```cs
Panel CreatePanel(string name, int x, int y, int width, int height,
	UIConfiguration configuration = null, UIStyle style = null, object provider = null)
```
```cs
Panel CreatePanel(string name, int x, int y, int width, int height, UIConfiguration configuration,
	UIStyle style, PanelDrag drag, PanelResize resize, object provider = null)
```

Обычно вам нужен второй из них. Этот метод CreatePanel создает объект Panel, наследующийся от RootVisualObject
(а RootVisualObject, в свою очередь, наследуется от VisualObject), затем добавляет его в список корней TUI.
Таким образом, система теперь при обработке нажатий *увидит* этот объект и проверит, не на него ли нажал игрок.
Все элементы этого интерфейса необходимо теперь добавлять уже к этой панели, вот пример создания
панели и добавления нескольких виджетов на нее:
```cs
// Создаем панель
Panel root = TUI.TUI.CreatePanel("TestPanel", 100, 100, 50, 40, null,
	new ContainerStyle() { Wall = WallID.DiamondGemspark });
// Создаем виджет Label (отображение текста)
Label label = new Label(1, 1, 17, 2, "some text");
// Добавляем к панели
root.Add(label);

// Создаем контейнер, занимающий правую половину нашей панели, закрашенный черной краской
// Функция Add возвращает только что добавленный объект в типе VisualObject,
// так что добавление элемента можно реализовать следующим образом:
VisualContainer node = root.Add(
	new VisualContainer(25, 0, 25, 40, null, new ContainerStyle() { WallColor = PaintID.Black })
) as VisualContainer;
// В этот контейнер добавим кнопку, которая по нажатию будет отправлять нажавшему текст в чат.
Button button = node.Add(new Button(0, 7, 12, 4, "lol", null, new ButtonStyle()
	{ WallColor = PaintID.DeepGreen }, (self, touch) =>
		touch.Player().SendInfoMessage("You pressed lol button!"))) as Button;
```
![](PanelExample.png)

## 4 независимых способа автоматического регулирования позиций и размеров объектов

#### Позиционирование дочерних объектов внутри текущей вершины:
* **[Layout](#Layout)** (разметка)
* **[Grid](#Grid)** (решетка)

#### Позиционирование текущей вершины внутри родительской:
* **[Alignment](#Alignment)** (отступ)

#### Регулирование размеров объекта относительно родителя:
* **[FullSize](#FullSize)** (полноразмерность)


## Layout

Этот метод позвоялет автоатически распологать детей, добавленных с помощью метода AddToLayout,
в определенном порядке друг за другом в указанном направлении:

```VisualObject SetupLayout(Alignment alignment, Direction direction, Side side, ExternalOffset offset, int childIndent, bool boundsIsOffset)```
* alignment - сторона/угол/центр, где будут распологаться объекты layout. Например, правый верхний угол - Alignment.TopRight
* direction - направление, по которому будут добавляться объекты. Например, вниз - Direction.Down
* side - сторона, к которой будут прилегать объекты. Например, по центру - Side.Center
* offset - отступ layout. Например, отступ сверху на 3 и слева на 2: new ExternalOffset() { Up=3, Left=2 }
* childIndent - расстояние между объектами в layout.
* boundsIsOffset - если установлено true, то блоки объектов, выходящие за границы layout, не будут рисоваться

Пример:
```cs
// Настраиваем конфигурацию layout
node.SetupLayout(Alignment.Center, Direction.Down, Side.Center, new ExternalOffset()
	{ Left = 5, Up = 5, Right = 5, Down = 5 }, 3, false);
// Добавляем в layout виджет InputLabel, позволяющий вводить текст
node.AddToLayout(new InputLabel(0, 0, new InputLabelStyle() { Default = "12345",
	TextColor =PaintID.White, Type = InputLabelType.All, TextUnderline = LabelUnderline.None }));
// Добавляем в layout еще один виджет ItemRack, который соответствует Weapon rack: отображение предмета
// на стойке размером 3х3. По нажатию выводит относительные и абсолютные координаты этого нажатия.
node.AddToLayout(new ItemRack(0, 0, new ItemRackStyle() { Type = 200, Left = true }, (self, touch) =>
	Console.WriteLine($"Touch: {touch.X}, {touch.Y}; absolute: {touch.AbsoluteX}, {touch.AbsoluteY}")));
ItemRack irack = node.AddToLayout(new ItemRack(0, 0,
	new ItemRackStyle() { Type = 201, Left = true })) as ItemRack;
// ItemRack позволяет сверху добавть текст с помощью таблички:
irack.Set("lololo\nkekeke");
// Наконец, добавляем слайдер в layout
node.AddToLayout(new Slider(0, 0, 10, 2, new SliderStyle() { Default = 3,
	Wall = WallID.AmberGemsparkOff, WallColor = PaintID.White }));
```
![](LayoutExample.png)

## Grid

Этот метод позволяет представить объект в виде решетки с абсолютными или относительными размерами колонок и линий:

```VisualObject SetupGrid(IEnumerable<ISize> columns, IEnumerable<ISize> lines, Offset offset, bool fillWithEmptyObjects)```
* columns - размеры колонок. Например, левая колонка размером 10 блоков, а правая - все оставшееся место: new ISize[] { Absolute(10), Relative(100) }
* lines - размеры линий. Например, центральная линия занимает 20 блоков, а верхняя и нижняя поровну делят оставшееся место: new ISize[] { Relative(50), Absolute(20), Relative(50) }
* offset - отступ сетки, включая внутренние отступы ячеек между собой и внешние отступы от границы объекта.
* fillWithEmptyObjects - заполнить ли автоматически все ячейки пустыми VisualContainer или нет.

Пример:
```cs
// Настраиваем конфигуарцию сетки grid. Указываем, что нужно все ячейки заполнить автоматически.
// Одна колонка размером с все доступное место и две линии: нижняя размером 16, остальное - верхняя.
node.SetupGrid(new ISize[] { new Relative(100) }, new ISize[] { new Relative(100), new Absolute(16) }, null, true);
// В первой ячейке (на пересечении первой колонки и первой линии) установим черный цвет фона
node[0, 0].Style.WallColor = PaintID.Black;
// А ячейке второй линии первой колонки назначим новый объект с белым цветом фона
node[0, 1] = new VisualContainer(new ContainerStyle() { WallColor = PaintID.White });
// Заметьте, что кнопка button не будет видна, потому что ее заслоняет объект первой линии сетки
```
![](GridExample.png)

Для тестов вы можете вызвать функцию ShowGrid(), чтобы увидеть решетку даже без объектов:
```cs
// Устанавливаем большую и сложную решетку
node.SetupGrid(new ISize[] { new Absolute(3), new Relative(50), new Absolute(6), new Relative(50) },
	new ISize[] { new Relative(20), new Absolute(5), new Relative(80) });
// Через 10 секунд отрисовываем сетку
Task.Delay(10000).ContinueWith(_ => node.ShowGrid());
```
![](ShowGridExample.png)

## Alignment

Этот метод позволяет автоматически распологать объект в относительной позиции в родителе:

```VisualObject SetAlignmentInParent(Alignment alignment, ExternalOffset offset, bool boundsIsOffset)```
* alignment - место расположения объекта в родителе.
* offset - отступы от границ родителя.
* boundsIsOffset - рисовать ли тайлы, которые вылезают за границы offset.

Пример (метод Add возвращает только что добавленный дочерний объект):
```cs
// Добавляем label и сразу устанавливаем Alignment с отступом 1 слева и снизу
node.Add(new Label(0, 0, 10, 4, "test"))
	.SetAlignmentInParent(Alignment.DownLeft, new ExternalOffset(Left=1, Down=1));
```
![](AlignmentExample.png)

## FullSize

Этот метод позволяет автоматически устанавливать размеры объекта (как по ширине, так и по высоте)
относительно размеров родителя, а именно - расширять в точности до размеров родителя:

```VisualObject SetFullSize(bool horizontal, bool vertical)```
* horizontal - устанавливать ширину объекта равной ширине родителя.
* vertical - устанавливать высоту объекта равной высоте родителя.

или
```VisualObject SetFullSize(FullSize fullSize)```
* fullSize - одно из значений: FullSize.None, FullSize.Horizontal, FullSize.Vertical, FullSize.Both.

Пример:
```cs
// Добавляем желтый контейнер, устанавливаем его ширину на 3, а по высоте делаем FullSize,
// затем указываем, что он должен быть в правом углу родителя.
// Таким образом у нас получается желтая полоса справа с высотой node и шириной 3.
node.Add(new VisualContainer(new ContainerStyle() { WallColor = PaintID.DeepYellow }))
	.SetWH(3, 0).SetFullSize(false, true).SetAlignmentInParent(Alignment.Right);
```
![](FullSizeExample.png)

## Как происходит нажатие
На каждый элемент интерфейса (являющегося объектом класса VisualObject) можно нажать
с помощью предмета "великий план" (the grand design). Каждому нажатия ставится в соответствие
объект нажатия Touch, содержащий всю необходимую информацию о нажатии.
#### Класс Touch
<details><summary> Нажмите сюда, чтобы развернуть </summary>
<p>
* int X
	* Координата по горизонтали относительно своей левой границы.
* int Y
	* Координата по вертикали относительно своей верхней границы.
* int AbsoluteX
	* Координата по горизонтали относительно левой границы мира.
* int AbsoluteY
	* Координата по вертикали относительно верхней границы мира.
* TouchState State
	* Состояние нажатие. Принимает одно из значений: Begin, Moving, End.
* UserSession Session
	* Объект сессии нажимающего пользователя.
* VisualObject Object
	* Объект, на который это нажатие попало.
* int Index
	* Номер нажатия, считая от начала нажатия (TouchState.Begin)
* int TouchSessionIndex
	* Индекс промежутка нажатия, в который этот Touch был совершен.
* bool Undo
	* true у нажатия с TouchState.End, если игрок окончил нажатие
	правой кнопкой мыши (отмена действия плана)
* byte Prefix
	* Префикс плана (the grand design).
* bool Red, Green, Blue, Yellow, Actuator, Cutter
	* Включен ли красный провод/зеленый/желтый/актуаток/резак. Актуально только
	на момент TouchState.End.
* DateTime Time
	* Время нажатия по Utc.
</p>
</details>

Промежутком нажатия считается промежуток времени от нажатия левой кнопки мыши
(создания проджектайла плана) до отпускания левой кнопки мыши.
Каждому нажатию Touch этого промежутка соответствует состояния нажатия State,
принимающее значение TouchState.Begin в первое нажатие из промежутка нажатия,
значение TouchState.End в последнее нажатие из промежутка нажатия
и значение TouchState.Moving во все промежуточные нажатия :)
Короче говоря, нажал планом - у нажатия TouchState.Begin.
Начал водить с зажатым планом мышью по экрану - у нажатий TouchState.Moving.
Отпустил кнопку мыши - у нажатия TouchState.End.
Каждому игроку ставится в соответствие объект UserSession (сессия игрока),
которая хранит некоторые общие данные об игроке.
#### Класс UserSession
<details><summary> Нажмите сюда, чтобы развернуть </summary>
<p>
* bool Enabled
	* Если установить значение false, то все нажатия вплоть до следующего TouchState.End
	будут проигнорированы. Затем игрок снова сможет нажимать.
* int UserIndex
	* Индекс пользователя, соответствующего этому объекту UserSession
* int TouchSessionIndex
	* Текущий индекс промежутка нажатия. Увеличивается на 1 с каждым TouchState.End.
* ProjectileID
	* ID проджектайла великого плана, соответствующего этому нажатия.
* Touch PreviousTouch
	* Объект предыдущего нажатия.
* Touch BeginTouch
	* Объект первого нажатия промежутка нажатия (TouchState.Begin)
* VisualObject Acquired
	* Привязанный к промежутку нажатия объект. Однажды привязав с помощью
	Configuration.SessionAcquire какой-то VisualObject к промежутку нажатия, все последующие нажатия будут проходить только к этому объекту вплоть до окончания нажатия (TouchState.End).
* ConcurrentDictionary<object, object> Data
	* Приватное runtime-хранилище данных, к которому можно обратиться через оператор[string key].
</p>
</details>

При нажатии, если этот объект удовлетворяет условиям нажатия (cм. UIConfiguration),
вызывается функция VisualObject.Invoke(Touch touch) с передающимся объектом нажатия Touch.
Фунция Invoke по дефолту вызывает функцию-колбэк, хранящуюся в поле VisualObject.Callback.
Это пользовательская функция, в которой программист указывает, что он хочет, чтобы происходило
по нажатию на этот объект. Виджеты, написанные на C#, могут не использовать эту функцию,
а напрямую переопределить Invoke.

## Класс UIConfiguration
Каждый объект VisualObject имеет настройки нажатия и отрисовки, хранящиеся в свойстве
Configuration класса UIConfiguration.
<details><summary> Нажмите сюда, чтобы развернуть </summary>
<p>
* bool UseBegin
	* Allows to touch this node if touch.State == TouchState.Begin. True by default.
* bool UseMoving
	* Allows to touch this node if touch.State == TouchState.Moving. False by default.
* bool UseEnd
	* Allows to touch this node if touch.State == TouchState.End. False by default.
* bool SessionAcquire
	* Once node is touched all future touches within the same session will pass to this node.
* bool BeginRequire
	* Allows to touch this node only if current session began with touching it.
* bool UseOutsideTouches
	* Only for nodes with SessionAcquire. Passes touches even if they are not inside of this object.
* bool Ordered
	* Touching child node would place it on top of Child array layer so that it would draw
	higher than other objects with the same layer and check for touching first.
* object Permission
	* Object that should be used for checking if user can touch this node (permission string for TShock).
* Lock Lock
	* Touching this node would prevent touches on it or on the whole root for some time.
* Action<VisualObject> CustomUpdate
* Func<VisualObject, Touch, bool> CustomCanTouch
* Action<VisualObject> CustomApply
* Action<VisualObject, PulseType> CustomPulse
</p>
</details>

## Класс UIStyle
Каждый объект VisualObject имеет стили отрисовки, хранящиеся в свойстве Style класса UIStyle.
<details><summary> Нажмите сюда, чтобы развернуть </summary>
<p>
* bool? Active
	* Sets tile.active(Style.Active) for every tile.
	If not specified sets to true in case Style.Tile is specified,
	otherwise to false in case Style.Wall is specified.
* ushort? Tile
	* Sets tile.type = Style.Tile for every tile.
* byte? TileColor
	* Sets tile.color(Style.TileColor) for every tile.
* byte? Wall
	* Sets tile.wall = Style.Wall for every tile.
* byte? WallColor
	* Sets tile.wallColor(Style.WallColor) for every tile.
* bool? InActive
	* Sets tile.inActive(Style.InActive) for every tile.
</p>
</details>

## Сигналы PulseType
С помощью функций Pulse(PulseType), PulseThis(PulseType) и PulseChild(PulseType)
можно распространять сигналы в дереве объектов интерфейса следующих типов:
* Reset
	* Сигнал сбрасывания объекта. В виджетах ввода данных устанавливает изначальные значения итд.
* PositionChanged
	* Сигнал посылается автоматически поддереву, если корень этого поддерева изменил свою позицию
	или размеры.
* User1
	* Пользовательский сигнал
* User2
* User3

## База данных
У каждого объекта VisualObject есть свойство Name и FullName.
Name обязательно задается при создании RootVisualObject/Panel, для остальных объектов это
свойство не обязательно. FullName - полное название в дереве интерфейса, например:
Если мы создадим панель Panel с именем "TestPanel" и добавим к ней контейнер VisualContainer,
а к этому контейнеру добавим слайдер Slider, то для этого слайдера свойство FullName будет
выдавать значение "TestPanel[0].VisualContainer[0].Slider", где 0 - индекс в массиве дочерних
элементов. Если мы контейнеру установим свойство Name в значение "lol", то то же самое
свойство FullName у слайдера теперь будет выдавать "TestPanel[0].lol[0].Slider".
Поле FullName является ключем, по которому хранятся данные объекта в базе данных в
таблице формата ключ-значение. Потому, если вы хотите хранить у какого-то объекта VisualObject
данные в базе данных, рекомендуется у него и всех его родителей установить уникальное свойство Name,
чтобы изменение индекса в массиве не поменяло ключ и, соответственно, данные.
Прочитать(записать) данные из(в) базы(у) данных можно с помощью функции DBRead(DBWrite).
Данные записываются и читаются в потоковом формате. Для определения поведения считывания
и поведения записи потока данных существуют переопределяемые функции чтения и записи
в BinaryReader и BinaryWriter:
```cs
virtual void DBReadNative(BinaryReader br)
virtual void DBWriteNative(BinaryWriter bw)
```
Так, например, виджет панели Panel использует это для того, чтобы запомнить позицию и размеры:
```cs
protected override void DBReadNative(BinaryReader br)
{
	int x = br.ReadInt32();
	int y = br.ReadInt32();
	int width = br.ReadInt32();
	int height = br.ReadInt32();
	SetXYWH(x, y, width, height);
}
protected override void DBWriteNative(BinaryWriter bw)
{
	bw.Write((int)X);
	bw.Write((int)Y);
	bw.Write((int)Width);
	bw.Write((int)Height);
}
```

## Виджеты
Большинство виджетов имеют параметр стиля в конструкторе, зачастую это не UIStyle,
а класс, наследующийся от UIStyle. Например, ButtonStyle включает в себя как
конфигурацию UIStyle, так и разные стили, связанные с морганием кнопки:
BlinkColor, BlinkDelay, TriggerStyle, BlinkStyle.
Если виджет имеет в конструкторе стиль с собственным типом (как ButtonStyle у Button),
тогда этот виджет имеет поле с именем соответствующего типа стиля.
Так, виджет кнопки Button имеет поле .ButtonStyle, которое содержит значение этого самого стиля.
Обращение же к полю .Style будет возвращать тот же самый объект, но в типе UIStyle,
это надо иметь в виду.

### VisualObject
Базовый объект интерфейса. Любой виджет наследуется от этого класса.
Как и в большинстве других виджетов, параметры configuration, style и callback не обязательны.
Структура конструктора VisualObject задает стиль конструкторов всех остальных виджетов:
[Координаты] -> [размеры] -> [текст (в данном случае его нет)] -> [конфигурация] -> [стиль] -> [функция].
```cs
VisualObject(int x, int y, int width, int height, UIConfiguration configuration,
	UIStyle style, Action<VisualObject, Touch> callback)
```
* x - *относительная* координата по горизонтали от левой стороны родителя (или от левого края карты мира, если нет родителя)
* y - *относительная* координата по вертикали от верхней стороны родителя (или от верхнего края карты мира, если нет родителя)
* width - ширина объекта
* height - высота объекта
* [configuration] - конфигурация объекта (когда можно нажимать, кому можно нажимать, ...)
* [style] - стиль отображения объекта (стены, краска стен, блоки, краска блоков, ...)
* [callback] - функция, вызываемая по нажатию на этот объект. Принимает 2 параметра:
VisualObject (это сам объект) и Touch - объект нажатия, хранящий информацию о координатах, состоянии нажатия,
об нажимающем игроке (.Session.UserIndex), ...
TSPlayer нажимающего игрока можно получить через функцию-расширения Touch.Player(),
доступную в сборке TUIPlugin.dll.

Пример:
```cs
VisualObject obj = node.Add(new VisualObject(0, 0, 8, 4, null, new UIStyle() { WallColor = 15 },
	(self, touch) => Console.WriteLine(touch.X + " " + touch.Y)));
```
![]()

[Подробнее о полях и методах VisualObject](ru_VisualObject.md)

***

### VisualContainer
Виджет-контейнер других виджетов. Рекомендуется использовать именно его, несмотря на то,
что можно использовать и обычный VisualObject для хранения других виджетов.
Этот виджет гарантирует правильную работу виджетов ScrollBackground и ScrollBar внутри себя.
```cs
VisualContainer(int x, int y, int width, int height, UIConfiguration configuration,
	ContainerStyle style, Action<VisualObject, Touch> callback)
VisualContainer()
VisualContainer(UIConfiguration configuration)
VisualContainer(ContainerStyle style)
```
Пример:
```cs
VisualContainer node = root.Add(
	new VisualContainer(25, 0, 25, 40, null, new ContainerStyle() { WallColor = PaintID.Black })
) as VisualContainer;
```
![]()

### RootVisualObject
Виджет, являющийся корнем дерева и выполняющий соответствующие функции.
Нельзя создать напрямую, только через TUI.CreateRoot():
```cs
RootVisualObject TUI.CreateRoot(string name, int x, int y, int width, int height,
	UIConfiguration configuration, ContainerStyle style, object provider)
```
* provider - объект провайдера тайлов (блоков), дефолтное значение - null (интерфейс будет
отрисовываться на карте Main.tile). В случае дефолтного значения блоки карты, находящиеся
внутри этого интерфейса, будут безвозвратно изменены.
В качестве значения можно использовать FakeTileRectangle из [FakeManager](https://github.com/AnzhelikaO/FakeManager),
тогда интерфейс будет отрисовываться на слое блоков поверх карты Main.tile.

Обладает следующими особенными функциями:
```cs
// Добавить объект в качестве всплывающего окна
VisualContainer ShowPopUp(VisualObject popup, ContainerStyle style = null,
	Action<VisualObject> cancelCallback = null)
// Скрыть всплывающее окно
RootVisualObject HidePopUp()
// Отобразить информирующее окно с текстом
RootVisualObject Alert(string text, UIStyle style = null, ButtonStyle buttonStyle = null)
// Отобразить запрашивающее подтверждение окно с текстом и вариантами ответа yes и no (да и нет)
RootVisualObject Confirm(string text, Action<bool> callback, ContainerStyle style = null,
	ButtonStyle yesButtonStyle = null, ButtonStyle noButtonStyle = null)
```

### Panel
Подразновидность RootVisualObject, обладающая некоторыми особенностями:
Панель можно перемещать и изменять ее размер прямо в процессе.
Исходно панель имеет 2 кнопки:
* Кнопка перемещения панели размером 1х1 в левом верхнем углу панели
* Кнопка изменения размера панели размером 1х1 в правом нижнем углу

Панель автоматически сохраняет свои позицию и размер в базу данных и загружает на старте.
Сохранить позицию панели напрямую можно вызывав функцию SavePosition().
Точно так же, как и RootVisualObject, нельзя создать напрямую, только через TUI.CreatePanel():
```cs
Panel CreatePanel(string name, int x, int y, int width, int height,
	UIConfiguration configuration = null, UIStyle style = null, object provider = null)
```
```cs
Panel CreatePanel(string name, int x, int y, int width, int height, UIConfiguration configuration,
	UIStyle style, PanelDrag drag, PanelResize resize, object provider = null)
```
![]()

### Label
Виджет отображения текста с помощью статуй символов и цифр.
```cs
Label(int x, int y, int width, int height, string text, LabelStyle style)
```
Свойства LabelStyle:
* Все свойства [UIStyle](#Класс-UIStyle)
* byte TextColor
* Offset TextOffset
* Alignment TextAlignment
	* Where to place the text (up right corner/down side/center/...)
* Side TextSide
	* Side to which shorter lines would adjoin.
* LabelUnderline TextUnderline
	* Whether to use underline part of statues for characters (makes their size 2x3 instead of 2x2).
* byte TextUnderlineColor
	* Color of statue underline part (if TextUnderline is LabelUnderLine.Underline).

Пример:
```cs
Label label = node.Add(new Label(1, 1, 15, 2, "some text", new LabelStyle() { TextColor=13 })) as Label;
```
![]()

### Button
Кнопка, выполняющая указанные действия по нажатию и моргающая тем или иным способом.
Может отображать указанный текст, ибо наследуется от Label.
```cs
Button(int x, int y, int width, int height, string text, UIConfiguration configuration,
	ButtonStyle style, Action<VisualObject, Touch> callback)
```
Свойства ButtonStyle:
* Все свойства [LabelStyle](#Label)
* ButtonTriggerStyle TriggerStyle
	* When to invoke Callback: on TouchState.Begin or on TouchState.End.
* ButtonBlinkStyle BlinkStyle
	* Style of blinking. Currently supports: left border blinking, right border blinking, full object blinking.
* byte BlinkColor
	* Color of blink if BlinkStyle is not None.
* int BlinkDelay
	* Minimal interval of blinking.

Пример:
```cs
Button button = node.Add(new Button(0, 7, 12, 4, "lol", null, new ButtonStyle()
{
	WallColor = PaintID.DeepGreen
}, (self, touch) => touch.Player().SendInfoMessage("You pressed lol button!"))) as Button;
```
![]()

### Slider
Слайдер (ползунок для указания относительной величины)
```cs
Slider(int x, int y, int width, int height, SliderStyle style, Action<Slider, int> callback)
```
Пример:
```cs
Slider slider = node.Add(new Slider(0, 0, 10, 2, new SliderStyle()
{
	Default = 3,
	Wall = 157,
	WallColor = PaintID.White
}, (self, value) => Console.WriteLine("Slider: " + value))) as Slider;
```
![]()

### Checkbox
Чекбокс: кнопка 2х2, имеющая 2 состояния (вкл/выкл)
```cs
Checkbox(int x, int y, int size, CheckboxStyle style, Action<Checkbox, bool> callback)
```
Пример:
```cs
Checkbox checkbox = node.Add(new Checkbox(0, 0, 2, new CheckboxStyle()
{
	Wall = 156,
	WallColor = PaintID.White
}, (self, value) => Console.WriteLine("Checkbox: " + value))) as Checkbox;
```
![]()

### Separator
Разделитель. Обычно пустой объект, необходимый для вставки с целью занятия пространства.
```cs
Separator(int size, UIStyle style)
Separator(int width, int height, UIStyle style)
```
Пример:
```cs
Separator separator = node.Add(new Separator(6, new UIStyle()
{
	Wall = 156,
	WallColor = PaintID.DeepRed
})) as Separator;
```
![]()

### InputLabel
Виджет для ввода текста. Ввод происходит посимвольно, путем зажатия мыши на символе
и таскании его вверх/вниз. Поддерживаются несколько наборов символов.
Наследуется от Label
```cs
InputLabel(int x, int y, InputLabelStyle style, Action<InputLabel, string> callback)
```
Пример:
```cs
InputLabel input = node.Add(new InputLabel(0, 0, new InputLabelStyle()
{
	Default = "12345",
	Type = InputLabelType.All,
	TextUnderline = LabelUnderline.None
})) as InputLabel;
```
![]()

### ItemRack
Виджет для отображения предмета. Рисуется в виде подставки для оружия (weapon rack),
верхняя часть может быть заменена на таблички для отображения надписи.
```cs
ItemRack(int x, int y, ItemRackStyle style, Action<VisualObject, Touch> callback)
```
Пример:
```cs
ItemRack irack = node.Add(new ItemRack(0, 0, new ItemRackStyle()
{
	Type = 200,
	Left = true
}, (self, touch) => { })) as ItemRack;
```
![]()

### VisualSign
Виджет отображения таблички с надписью.
```cs
VisualSign(int x, int y, int width, int height, string text, UIConfiguration configuration,
	UIStyle style, Action<VisualObject, Touch> callback)
```
Пример:
```cs
VisualSign vsign = node.Add(new VisualSign(0, 0, "lmfao sosi")) as VisualSign;
```
![]()

### FormField
Этот виджет нужен для добавления текста слева к другому виджету, например к
Checkbox, Button, InputLabel, Slider, ...
Наследуется от Label
```cs
FormField(VisualObject input, int x, int y, int width, int height, string text,
	LabelStyle style, ExternalOffset inputOffset)
```
Пример:
```cs
FormField ffield = node.Add(new FormField(new VisualSign(0, 0, "test"),
	0, 0, 2, 2, "VisualSign ->", new LabelStyle()
{
	TextColor = 29,
	TextAlignment = Alignment.Left
})) as FormField;
```
![]()

### Image
Виджет отображения картинки в формате WorldEdit (.dat) или TEdit (.TEditSch)
Поддерживает отображение табличек.
Отображает битую картинку в случае неудачи загрузки картинки.
```cs
Image(int x, int y, string path, UIConfiguration configuration, UIStyle style,
	Action<VisualObject, Touch> callback)
Image(int x, int y, ImageData data, UIConfiguration configuration, UIStyle style,
	Action<VisualObject, Touch> callback)
```
Пример:
```cs
Image image = node.Add(new Image(2, 2, "Media\\Help.TEditSch")) as Image;
```
![]()

### Video
Виджет отображения видео, состоящего из картинок Image.
Загружает в качестве path путь директории, в которой лежат все слайды в алфавитном порядке.
Отображает битую картинку в случае неудачи загрузки слайдов.
```cs
Video(int x, int y, UIConfiguration configuration, UIStyle style, Action<VisualObject, Touch> callback)
```
Пример:
```cs
Video video = node.Add(new Video(2, 2, null, new VideoStyle()
{
	Path = "Media\\Animation-1",
	Delay = 100,
	TileColor = PaintID.DeepTeal
}, (self, touch) => (self as Video).ToggleStart())) as Video;
```
![]()

### AlertWindow
Виджет всплывающего окна с информационным сообщением.
Создается с помощью метода корневого объекта RootVisualObject:
```cs
RootVisualObject Alert(string text, UIStyle style, ButtonStyle buttonStyle)
```
Пример:
```cs
node.Root.Alert("Hello world")
```
![]()

### ConfirmWindow
Виджет всплывающего окна с подтверждением действия.
Создается с помощью метода корневого объекта RootVisualObject:
```cs
RootVisualObject Confirm(string text, Action<bool> callback, ContainerStyle style,
	ButtonStyle yesButtonStyle, ButtonStyle noButtonStyle)
```
Пример:
```cs
node.Root.Confirm("Hello world", value => Console.WriteLine(value))
```
![]()

### ScrollBackground
Виджет прокручивания layout своего Parent (родительского) объекта как на сенсорном экране:
Потяни за задний фон вниз - layout поедет вниз. И наоборот.
Виджет всегда находится сзади всех остальных дочерних объектов, потому позволяет
прокручивать layout только в случае нажатия в пустую область (область, где нет
конкурирующих за нажатие других детей).
```cs
ScrollBackground(bool allowToPull, bool rememberTouchPosition, bool useMoving,
	Action<ScrollBackground, int> callback)
```
* allowToPull - 
* rememberTouchPosition - 
* useMoving - 
* callback - 
Пример:
```cs
// Указываем layer (слой) в значение Int32.MinValue, чтобы виджет был сзади всех прочих виджетов
ScrollBackground scrollbg = node.Add(new ScrollBackground(true, true, true), Int32.MinValue) as ScrollBackground;
```
![]()

### ScrollBar
Полоса прокручивания layout. Добавляется с одной из сторон (справа/слева/сверху/снизу).
На данный момент не поддерживает относительный скроллинг (когда размер layout слишком большой)
```cs
ScrollBar(Direction side, int width, ScrollBarStyle style)
```
Пример:
```cs
ScrollBar scrollbar = node.Add(new ScrollBar(Direction.Right)) as ScrollBar;
```
![]()

### Arrow
Простой виджет отображения стрелки.
```cs
Arrow(int x, int y, ArrowStyle style, Action<VisualObject, Touch> callback)
```
Пример:
```cs
Arrow arrow = node.Add(new Arrow(0, 0, new ArrowStyle() { Direction = Direction.Left })) as Arrow;
```
![]()



## Общие факты о клиентской стороне управления интерфейсом
1. Клиент работает так, что в начале нажатия планом пакеты перемещения мыши отправляются очень быстро,
	а начиная с момента примерно через секунду - скорость уменьшается и становится постоянной.
2. В режимах освещения retro и trippy отрисовка интерфейса происходит быстрее и плавнее.
3. Нажатием правой кнопки мыши во время использования плана можно отменить нажатие. Это действие особым
	образом обрабатывается в некоторых виджетах (трактуется как отмена действия)
4. Некоторые тайлы ломаются при определенных условиях при отправке с помощью SendTileSquare (например, это статуя без блоков под ней).
Для того, чтобы заставить объект рисоваться с помощью отправок секций, достаточно установить у него поле ForceSection в значение true.