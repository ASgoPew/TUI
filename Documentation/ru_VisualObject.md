# VisualObject
## Публичные поля и свойства VisualObject
* string **Name**
	* Имя объекта. По умолчанию - имя класса.
* string **FullName**
	* Полное имя объекта (имена всех объектов вплоть до корня).
* int **X**
	* X координата относительно родительского объекта.
* int **Y**
	* Y координата относительно родительского объекта.
* int **Width**
* int **Height**
* VisualObject **Parent**
	* Родительский объект. Null для RootVisualObject.
* RootVisualObject **Root**
	* Корень дерева интерфейса. Null до первого вызова Update(). Используйте GetRoot(), чтобы вычислить вручную.
* UIConfiguration **Configuration**
	* Настройки нажатия и отрисовки.
* UIStyle **Style**
	* Стили отображения стен и блоков.
* Action<VisualObject, Touch> **Callback**
	* Функция, вызываемая по нажатию на объект с помощью Великого плана (The grand design).
* virtual dynamic **Provider**
	* Провайдер тайлов текущего дерева интерфейса (ITileProvider).
* bool **UsesDefaultMainProvider**
	* Истина, если Provider ссылается на Main.tile.
* int **ChildCount**
	* Свойство для ОТЛАДКИ: Количество дочерних объектов.
* bool **Enabled**
	* Включен ли объект или нет. Выключенные объекты не рисуются, ненажимаемы и не получают обновлений Update().
* bool **Visible**
	* Видим ли объект. Объект становится невидимым, когда вылезает за границы layout.
	Невидимые объекты получают обновления Update().
* virtual bool **Active**
	* Объект считается Active, когда он Enabled и Visible.
* bool **Loaded**
	* Был ли загружен объект.
* bool **Disposed**
	* Был ли освобожден объект.
* int **Layer**
	* Слой в, котором находится объект в массиве дочерних объектов Child у родительского объекта.
	Объекты с большим слоем всегда находятся выше в этом массиве.
* virtual bool **Orderable**
	* Переопределяемое свойство для отключения возможности перемещать объект в массиве дочерних объектов у родителя.
* GridCell **Cell**
	* Ячейка родительского объекта, в которой находится объект. Null, если объект не в сетке Grid.
* bool **ForceSection**
	* Объект отрисовывается с помощью SentTileSquare по умолчанию.
	Установите это свойство, чтобы объект отрисовывался с помощью отправки секций SendSection.
* int **ProviderX**
	* X координата относительно провайдера Provider. Устанавливается в Update() и по сигналу PulseType.PositionChanged.
* int **ProviderY**
	* Y координата относительно провайдера Provider. Устанавливается в Update() и по сигналу PulseType.PositionChanged.
* ExternalOffset **Bounds**
	* Рамки (относительно текущего объекта) в котором могут рисоваться блоки текущего объекта (и блоки поддерева дочерних объектов).
* IEnumerable<VisualObject> **DescendantDFS**
	* Метод обхода поддерева интерфейса (включая текущую вершину) Deep Fast Search (в глубину).
* IEnumerable<VisualObject> **DescendantBFS**
	* Метод обхода поддерева интерфейса (включая текущую вершину) Broad Fast Search (в ширину).

## Защищенные поля и свойства VisualObject
* object **Locker**
	* Объект, для lock-блокирования операций, связанных с этой вершиной.
* ConcurrentDictionary<string, object> **Shortcuts**
	* Runtime-хранилище данных, связанных с этой вершиной. Получить доступ можно по оператору this[string key].
* List<VisualObject> **Child**
	* Список дочерних объектов.
* VisualObject[,] **Grid**
	* Сетка дочерних объектов. Используйте оператор[,], чтобы получить элемент сетки.
* IEnumerable<VisualObject> **ChildrenFromTop**
	* Итератор перебора дочерних объектов, начиная с верхнего.
* IEnumerable<VisualObject> **ChildrenFromBottom**
	* Итератор перебора дочерних объектов, начиная с нижнего.
* IEnumerable<(int, int)> **Points**
	* Итератор перебора внутренних точек (координат) объекта, относительно текущей вершины.
* IEnumerable<(int, int)> **AbsolutePoints**
	* Итератор перебора внутренних точек (координат) объекта, относительно карты мира.
* IEnumerable<(int, int)> **ProviderPoints**
	* Итератор перебора внутренних точек (координат) объекта, относительно провайдера Provider.

## Публичные методы VisualObject
* virtual VisualObject **Add**(VisualObject child, int? layer)
	* Добавляет объект в качестве дочернего в указанный слой (использовать слой объекта).
	Не делает ничего, если объект уже является дочерним.
* VisualObject **Remove**(VisualObject child)
	* Удаляет дочерний объект. Вызывает Dispose() у удаленного объекта, так что использовать его больше нельзя.
* VisualObject **Select**(VisualObject o)
	* Включает определенный дочерний объект и выключает все остальные.
* VisualObject **Selected**()
	* Ищет первый попавшийся включенный Enabled объект.
* VisualObject **Deselect**()
	* Включает все дочерние объекты.
* VisualObject **GetRoot**()
	* Найти корень дерева интерфейса (VisualObject). Должен быть типа RootVisualObjectin в корректном дереве интерфейса.
* bool **IsAncestorFor**(VisualObject child)
	* Проверяет, является ли объект дочерним (this предком какого-либо поколения для child).
* virtual bool **SetTop**(VisualObject child)
	* Поднимает объект на верх своего слоя. Эта функция вызывается автоматически по нажатия на дочерний объект,
	если родитель Parent имеет истинное значение Configuration.Ordered и этот дочерний объект имеет истинное свойство Orderable.
* (int X, int Y, int Width, int Height) **XYWH**(int dx, int dy)
	* Возвращает позицию и размеры объекта.
		* dx - X coordinate delta
		* dy - Y coordinate delta
* virtual VisualObject **SetXYWH**(int x, int y, int width, int height)
	* Устанавливает позицию и размеры объекта.
* VisualObject **SetXY**(int x, int y)
* VisualObject **SetWH**(int width, int height)
* VisualObject **Move**(int dx, int dy)
	* Сдвинуть объект на dx и dy.
* VisualObject **MoveBack**(int dx, int dy)
	* Операция, обратная к Move().
* VisualObject **GetChild**(int index)
	* Функция для отладки. Получить дочений объект по индексу в массиве дочерних объектов Child.
* VisualObject **Enable**()
	* Включает объект. Читайте про свойство Enabled.
* VisualObject **Disable**()
* bool **CalculateActive**()
	* Рассчитывает, является ли каждая вершина от текущей вплоть до корня активной (Active). Корень должен быть объектом RootVisualObject.
* (int X, int Y) **RelativeXY**(int x, int y, VisualDOM parent)
	* Рассчитывает координаты относительно указанного родителя (какого-либо поколения).
* (int X, int Y) **AbsoluteXY**(int dx, int dy)
	* Рассчитывает координаты относительно карты мира.
* (int X, int Y) **ProviderXY**(int dx, int dy)
	* Рассчитывает координаты относительно провайдера Provider.
* virtual dynamic **Tile**(int x, int y)
	* Возвращает тайл (блок) по координатам, относительно текущей вершины (x=0, y=0 - левый верхний блок объекта).
* VisualObject **AddToLayout**(VisualObject child, int? layer)
	* Добавляет объект в качестве дочернего в разметку layout. Удаляет Alignment и ячейку Grid.
* VisualObject **SetupLayout**(Alignment alignment, Direction direction, Side side,
		ExternalOffset offset, int childIndent, bool boundsIsOffset)
	* Устанавливает разметку layout для позиционирования в ней дочерних объектов.
		* alignment - Где расположить набор дочерних объектов в разметке.
		* direction - Направление расположения дочерних объектов.
		* side - Сторона, к которой примыкают объекты в наборе объектов разметки, относительна по отношению к направлению.
		* offset - Отступы разметки.
		* childIndent - Расстояние между объектами в разметке.
		* boundsIsOffset - Рисовать ли объекты/блоки объектов, которые выходят за границу отступа Offset.
* VisualObject **SetupGrid**(IEnumerable<ISize> columns, IEnumerable<ISize> lines, Offset offset, bool fillWithEmptyObjects)
	* Установить решетку для расположения дочерних объектов в ней. Используйте классы Absolute и Relative для указания размеров.
		* columns - Размеры колонок (например, new ISize[] { new Absolute(10), new Relative(100) }).
		* lines - Размеры линий.
		* offset - Отступы решетки.
		* fillWithEmptyObjects - Заполнить ли решетку пустыми объектами VisualContainer.
* VisualObject **SetAlignmentInParent**(Alignment alignment, ExternalOffset offset, bool boundsIsOffset)
	* Установить Alignment-позиционирование текущей вершины в родителе. Запрещает позиционирование в разметке layout и решетке grid.
* VisualObject **SetFullSize**(bool horizontal, bool vertical)
	* Установить автоматическое растягивание размеров объекта относительно родителя. Запрещает позиционирование в grid.
* VisualObject **LayoutIndent**(int value)
	* Отступ разметки в блоках. Используется в виджетах ScrollBackground и ScrollBar.

* VisualObject **Pulse**(PulseType type)
	* Отправить сигнал указанного типа всему поддереву, включая текущий объект.
* VisualObject **PulseThis**(PulseType type)
	* Отправить сигнал указанного типа только текущему объекту.
* VisualObject **PulseChild**(PulseType type)
	* Отправить сигнал указанного типа всему поддереву, исключая текущий объект.

* VisualObject **Update**()
	* Обновить объект и поддерево дочерних объектов (только Enabled объекты).
* VisualObject **UpdateThis**()
	* Обновить только текущий объект.
* VisualObject **UpdateChildPositioning**()
	* Сначала обновляет размеры дочерних объектов, затем высчитывает позицию, основываясь на размерах (layout, grid, alignment).
* VisualObject **UpdateChild**()
	* Обновить поддерево дочерних объектов (только Enabled объекты).
* VisualObject **PostUpdateThis**()
	* Обновления, последующие обновлению всех дочерних объектов.

* VisualObject **Apply**()
	* Нарисовать все связанное с текущим VisualObject, включая все дочерние объекты (напрямую изменяет блоки провайдера Provider).
* VisualObject **ApplyThis**()
	* Нарисовать все связанное с текущим объектом и только. Не рисует дочерних объектов.
* VisualObject **ApplyTiles**()
	* Нарисовать стены и блоки текущего объекта.
* VisualObject **ApplyChild**()
	* Нарисовать дочерние объекты.
* VisualObject **Clear**()
	* Очистить все тайлы внутри объекта с помощью ITile.ClearEverything()

* VisualObject **Draw**(int dx, int dy, int width, int height, int playerIndex, int exceptPlayerIndex, bool? forceSection, bool frame)
	* Отправить нарисованный объект (прямоугольник блоков) пакетом SendTileSquare/SendSection клиенту.
		* dx - Сдвиг по X координате.
		* dy - Сдвиг по Y координате.
		* width - Ширина отправляемого прямоугольника, укажите -1, чтобы выбрать в качестве значение ширину самого объекта.
		* height - Высота отправляемого прямоугольника, укажите -1, чтобы выбрать в качестве значение высоту самого объекта.
		* playerIndex - Индекс игрока, которому отправляется прямоугольник, укажите -1, чтобы.
		* exceptPlayerIndex - Индекс игрока, которого нужно игнорировать при отправке.
		* forceSection - Отправлять ли с помощью SendTileSquare или с помощью секций SendSection. SendTileSquare (false) по умолчанию.
		* frame - Отправлять ли пакет SectionFrame, если отправка идет с помощью секции.
* VisualObject **DrawPoints**(IEnumerable<(int, int)> points, int playerIndex, int exceptPlayerIndex, bool? forceSection)
	* Отправить прямоугольник блоков по указанным точкам (итоговый прямоугольник включает их все).
* void **ShowGrid**()
	* ОТЛАДОЧНАЯ функция для отправки нарисованной сетки.

* bool **DBRead**()
	* Прочитать данные текущей вершины из базы данных, используя переопределяемый метод DBReadNative.
* void **DBWrite**()
	* Записать данные текущей вершины в базу данных, используя переопределяемый метод DBReadNative.
* bool **UDBRead**(int user)
	* Прочитать данные пользователя для текущей вершины из базы данных, используя переопределяемый метод DBReadNative.
* void **UDBWrite**(int user)
	* Записать данные пользователя для текущей вершины в базу данных, используя переопределяемый метод DBReadNative.

## Protected methods of VisualObject
* virtual bool **CanTouch**(Touch touch)
	* Проверяет, может ли указанное нажатие touch нажать на текущий объект и дочерние объекты.
* virtual void **PostSetTop**(VisualObject o)
	* Переопределяемая функция, которая вызывается, когда объект поднимается на верх слоя в списке дочерних объектов родителя.
* virtual bool **CanTouchThis**(Touch touch)
	* Проверяет, может ли указанное нажатие touch нажать на текущий объект. Не путать с методом CanTouch.
* virtual void **Invoke**(Touch touch)
	* Переопределяемая функция, которая вызывается, когда нажатие удовлетворяет условиям нажатия на текущий объект.
	Вызывает колбэк-функцию Callback по умолчанию.
* virtual void **PulseThisNative**(PulseType type)
	* Переопределяемая функция для обработки сигнала текущей вершиной.
* virtual void **UpdateThisNative**()
	* Переопределяемый метод для обновлений, связанных с текущей вершиной. Не меняйте размер/позицию текущего объекта в этом методе.
* void **UpdateBounds**()
	* Рассчитать рамку текущего объекта (пересечение отступа Offset родительской разметки/отступа Alignment текущего объекта и рамок родительского объекта).
* void **UpdateChildSize**()
	* Обновление размеров дочерних объектов с помощью вызова child.UpdateSizeNative().
* virtual (int, int) **UpdateSizeNative**()
	* Переопределяемый метод для определения размера текущего объекта, в зависимости от собственных данных (размер изображения image/текста label).
* void **UpdateFullSize**()
	* Обновляет размеры объекта в зависимости от размеров родителя, если у текущей вершины установлено свойство Configuration.FullSize.
* void **UpdateAlignment**()
	* Обновляет позиции дочерних объектов с установленным Alignment.
* void **UpdateLayout**()
	* Обновляет позиции дочерних объектов в разметке.
* void **UpdateGrid**()
	* Обновляет позиции дочерних объектов в сетке.
* virtual void **PostUpdateThisNative**()
	* Переопределяемый метод для обновления текущей вершины, зависимого от обновления дочерних объектов.
* virtual void **ApplyThisNative**()
	* Переопределяемый метод для отрисовки текущего объекта. По умолчанию отрисовывает стены и блоки, если указаны в стиле Style.
* virtual void **ApplyTile**(int x, int y)
	* Переопределяемый метод для отрисовки отдельного блока в ApplyTiles().
* virtual void **DBReadNative**(BinaryReader br)
	* Переопределяемый метод для чтения данных базы данных из BinaryReader.
* virtual void **DBWriteNative**(BinaryWriter bw)
	* Переопределяемый метод для записи данных в BinaryWriter в базу данных.
* virtual void **UDBReadNative**(BinaryReader br, int user)
	* Переопределяемый метод для чтения пользовательских данных базы данных из BinaryReader.
* virtual void **UDBWriteNative**(BinaryWriter bw, int user)
	* Переопределяемый метод для записи пользовательских данных в BinaryWriter в базу данных.

## Операторы VisualObject
* **this**[string key]
	* Прочитать/записать данные объекта в runtime-хранилище данных.
* **this**[int column, int line]
	* Get: Получить объект в решетке Grid.

# RootVisualObject : VisualObject

## Уникальные публичные поля и свойства RootVisualObject:
* HashSet<int> **Players**
	* Множество игроков, находящися достаточно близко к текущему интерфейсу.
* dynamic **Provider**
	* Читайте о провайдере Provider на основной странице.

## Уникальные публичные методы RootVisualObject:
* virtual VisualContainer **ShowPopUp**(VisualObject popup, ContainerStyle style, Action<VisualObject> cancelCallback)
	* Рисует всплывающий объект поверх всех других дочерних объектов.
		* ContainerStyle style - Стиль фона всплывающего окна.
		* Action<VisualObject> cancelCallback - Функция, вызывающаяся, когда игрок нажимает на фон всплывающего окна, но не на сам объект.
* virtual RootVisualObject **HidePopUp**()
* virtual RootVisualObject Alert(string text, UIStyle style, ButtonStyle okButtonStyle)
	* Показать окно уведомления с указанным текcтом и кнопкой "ok".
* virtual RootVisualObject Confirm(string text, Action<bool> callback, ContainerStyle windowStyle,
		ButtonStyle yesButtonStyle, ButtonStyle noButtonStyle)
	* Показать окно подтверждения с указанным текстом и кнопками "yes", "no".