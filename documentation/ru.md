# Документация TUI

Библиотека предоставляет возможность пользователю возможность создавать области блоков на карте,
которые работают по принципу обычного пользовательского интерфейса: на эту область можно нажимать
с помощью the grand design (великий план) и что-то может происходить. Например, можно поставить
кнопку, которая по нажатию будет выводить в чат что-то. И в принципе разработчик ограничен лишь своей
фантазией в том, что можно создать на пользовательском интерфейсе. Примером может быть настольная
игра сапер, на которую ушло 314 строк кода (включая логику самой игры):

![](minesweeper.png)

Эти и некоторые другие примеры игр вы можете посмотреть в измерении games на сервере terraria-servers.ru:7777.

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

```VisualObject *Add*(VisualObject newChild)```

## Базовые операции над VisualObject

Есть несколько важных операций, которые можно применять к объектам VisualObject:
1. **Update()**
Рекурсивная функция, обновляющая каждый из объектов поддерева (устанавливает нужные значения в
нужные поля, рассчитывает позицию относительных элементов, ...)
2. **Apply()**
Рекурсивная функция, отрисовывающая объект на карте (изменение блоков карты в соостветствии
со стилем отображения элемента)
3. **Draw()**
Отправка отрисованного (хотя не обязательно) объекта игрокам с помощью SendSection или SendTileSquare

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

Чтобы создать новый интерфейс, необходимо вызвать один из методов класса UI:
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

# 4 независимых способа автоматического регулирования позиций и/или размеров объектов

### Позиционирование дочерних объектов внутри текущей вершины:
* **Layout** (разметка)
* **Grid** (решетка)

### Позиционирование текущей вершины внутри родительской:
* **Alignment** (отступ)

### Регулирование размеров объекта относительно родителя:
* **FullSize** (полноразмерность)


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

# Базовые классы UIConfiguration и UIStyle

# База данных

# Виджеты
Большинство виджетов имеют параметр стиля в конструкторе, зачастую это не UIStyle,
а класс, наследующийся от UIStyle. Например, ButtonStyle включает в себя как
конфигурацию UIStyle, так и разные стили, связанные с морганием кнопки:
BlinkColor, BlinkDelay, TriggerStyle, BlinkStyle.
Если виджет имеет в конструкторе стиль с собственным типом (как ButtonStyle у Button),
тогда этот виджет имеет поле с именем соответствующего типа стиля.
Так, виджет кнопки Button имеет поле .ButtonStyle, которое содержит значение этого самого стиля.
Обращение же к полю .Style будет возвращать тот же самый объект, но в типе UIStyle,
это надо иметь в виду.

## VisualObject
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

### Подробнее о полях и методах...
<details><summary> Нажмите сюда, чтобы развернуть </summary>
<p>

### Публичные поля и свойства VisualObject
* string Name
* string FullName
* int X
* int Y
* int Width
* int Height
* VisualObject Parent
* RootVisualObject Root
* UIConfiguration Configuration
* UIStyle Style
* Action<VisualObject, Touch> Callback
* dynamic Provider
* bool UsesDefaultMainProvider
* int ChildCount
* bool Active
* bool Loaded
* bool Disposed
* bool Enabled
* bool Visible
* int Layer
* bool Orderable
* GridCell Cell
* bool ForceSection
* int ProviderX
* int ProviderY
* ExternalOffset Bounds
* object Data
* IEnumerable<VisualObject> DescendantDFS
* IEnumerable<VisualObject> DescendantBFS

### Защищенные (protected) поля VisualObject
* object Locker
* ConcurrentDictionary<string, object> Shortcuts
* List<VisualObject> Child
* VisualObject[,] Grid
* IEnumerable<VisualObject> ChildrenFromTop
* IEnumerable<VisualObject> ChildrenFromBottom
* IEnumerable<(int, int)> Points
* IEnumerable<(int, int)> AbsolutePoints
* IEnumerable<(int, int)> ProviderPoints

### Публичные методы VisualObject
* VisualObject Add(VisualObject child, int layer = 0)
* VisualObject Remove(VisualObject child)
* VisualObject Select(VisualObject o)
* VisualObject Selected()
* VisualObject Deselect()
* VisualObject GetRoot()
* bool IsAncestorFor(VisualObject child)
* bool SetTop(VisualObject child)
* (int X, int Y, int Width, int Height) XYWH(int dx = 0, int dy = 0)
* VisualObject SetXYWH(int x, int y, int width, int height)
* VisualObject SetXY(int x, int y)
* VisualObject SetWH(int width, int height)
* VisualObject Move(int dx, int dy)
* VisualObject MoveBack(int dx, int dy)
* VisualObject GetChild(int index)
* VisualObject Enable()
* VisualObject Disable()
* bool CalculateActive()
* (int X, int Y) RelativeXY(int x = 0, int y = 0, VisualDOM parent = null)
* (int X, int Y) AbsoluteXY(int dx = 0, int dy = 0)
* (int X, int Y) ProviderXY(int dx = 0, int dy = 0)
* dynamic Tile(int x, int y)
* VisualObject AddToLayout(VisualObject child, int layer = 0)
* SetupLayout(Alignment alignment = Alignment.Center, Direction direction = Direction.Down,
	Side side = Side.Center, ExternalOffset offset = null, int childIndent = 1, bool boundsIsOffset = true)
* VisualObject SetupGrid(IEnumerable<ISize> columns = null, IEnumerable<ISize> lines = null,
	Offset offset = null, bool fillWithEmptyObjects = true)
* VisualObject SetAlignmentInParent(Alignment alignment, ExternalOffset offset = null, bool boundsIsOffset = true)
* VisualObject SetFullSize(bool horizontal = true, bool vertical = true)
* VisualObject LayoutIndent(int value)

* VisualObject Pulse(PulseType type)
* VisualObject PulseThis(PulseType type)
* VisualObject PulseChild(PulseType type)

* VisualObject Update()
* VisualObject UpdateThis()
* VisualObject UpdateChildPositioning()
* VisualObject UpdateChild()
* VisualObject PostUpdateThis()

* VisualObject Apply()
* VisualObject ApplyThis()
* VisualObject ApplyTiles()
* VisualObject ApplyChild()
* VisualObject Clear()

* VisualObject Draw(int dx = 0, int dy = 0, int width = -1, int height = -1, int userIndex = -1,
	int exceptUserIndex = -1, bool? forceSection = null, bool frame = true)
* VisualObject DrawPoints(IEnumerable<(int, int)> points, int userIndex = -1,
	int exceptUserIndex = -1, bool? forceSection = null)
* void ShowGrid()

* void Database(Type dataType)
* void SetData(object data)
* void SetData(object data)

### Защищенные методы VisualObject
* virtual bool CanTouch(Touch touch)
* virtual void PostSetTop(VisualObject o)
* virtual bool CanTouchThis(Touch touch)
* virtual void Invoke(Touch touch)
* virtual void PulseThisNative(PulseType type)
* virtual void UpdateThisNative()
* void UpdateBounds()
* void UpdateChildSize()
* virtual (int, int) UpdateSizeNative()
* void UpdateFullSize()
* void UpdateAlignment()
* void UpdateLayout()
* void UpdateGrid()
* virtual void PostUpdateThisNative()
* virtual void ApplyThisNative()
* virtual void ApplyTile(int x, int y)

### Операторы VisualObject
* this[string key]
* this[int column, int line]

</p>
</details>

***

## VisualContainer
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

## RootVisualObject
Виджет, являющийся корнем дерева и выполняющий соответствующие функции.
Нельзя создать напрямую, только через TUI.CreateRoot():
```cs
public static RootVisualObject CreateRoot(string name, int x, int y, int width, int height,
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

## Panel
Подразновидность RootVisualObject, обладающая некоторыми особенностями:
Панель можно перемещать и изменять ее размер прямо в процессе.
Исходно панель имеет 2 кнопки:
* Кнопка перемещения панели размером 1х1 в левом верхнем углу панели
* Кнопка изменения размера панели размером 1х1 в правом нижнем углу

Панель автоматически сохраняет свои позицию и размер в базу данных и загружает на старте.
Сохранить позицию панели напрямую можно вызывав функцию SavePosition().
Точно так же, как и RootVisualObject, нельзя создать напрямую, только через TUI.CreatePanel().
![]()

## Label
Виджет отображения текста с помощью статуй символов и цифр.
```Label(int x, int y, int width, int height, string text, LabelStyle style)```
Пример:
```cs
Label label = node.Add(new Label(1, 1, 15, 2, "some text", new LabelStyle() { TextColor=13 })) as Label;
```
![]()

## Button
Кнопка, выполняющая указанные действия по нажатию и моргающая тем или иным способом.
Может отображать указанный текст, ибо наследуется от Label.
```cs
Button(int x, int y, int width, int height, string text, UIConfiguration configuration,
	ButtonStyle style, Action<VisualObject, Touch> callback)
```
Пример:
```cs
Button button = node.Add(new Button(0, 7, 12, 4, "lol", null, new ButtonStyle()
{
	WallColor = PaintID.DeepGreen
}, (self, touch) => touch.Player().SendInfoMessage("You pressed lol button!"))) as Button;
```
![]()

## Slider
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

## Checkbox
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

## Separator
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

## InputLabel
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

## ItemRack
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

## VisualSign
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

## FormField
Этот виджет нужен для добавления текста слева к другому виджету, например к
Checkbox, Button, InputLabel, Slider, ...
Наследуется от Label
```cs
FormField(VisualObject input, int x, int y, int width, int height, string text, LabelStyle style)
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

## Image
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

## Video
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

## AlertWindow
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

## ConfirmWindow
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

## ScrollBackground
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

## ScrollBar
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

## Arrow
Простой виджет отображения стрелки
```cs
Arrow(int x, int y, ArrowStyle style, Action<VisualObject, Touch> callback)
```
Пример:
```cs
Arrow arrow = node.Add(new Arrow(0, 0, new ArrowStyle() { Direction = Direction.Left })) as Arrow;
```
![]()



## Общие факты о клиентской стороне управления интерфейсом:
1. Клиент работает так, что в начале нажатия планом пакеты перемещения мыши отправляются очень быстро,
	а начиная с момента примерно через секунду - скорость уменьшается и становится постоянной.
2. В режимах освещения retro и trippy отрисовка интерфейса происходит быстрее и плавнее.
3. Нажатием правой кнопки мыши во время использования плана можно отменить нажатие. Это действие особым
	образом обрабатывается в некоторых виджетах (трактуется как отмена действия)
4. Некоторые тайлы ломаются при определенных условиях при отправке с помощью SendTileSquare (например, это статуя без блоков под ней).
Для того, чтобы заставить объект рисоваться с помощью отправок секций, достаточно установить у него поле ForceSection в значение true.