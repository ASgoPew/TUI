### Публичные поля и свойства VisualObject
* string Name
	Object name. Class type name by default.
* string FullName
	Object full name (names of all objects up to the Root).
* int X
	X coordinate relative to Parent object.
* int Y
	Y coordinate relative to Parent object.
* int Width
* int Height
* VisualObject Parent
	Parent object. Null for RootVisualObject.
* RootVisualObject Root
	Root of the interface tree. Null before first Update() call. Use GetRoot() to calculate manually.
* UIConfiguration Configuration
	Object touching and drawing settings.
* UIStyle Style
	Object tile and wall style.
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
* VisualObject SetFullSize(bool horizontal = false, bool vertical = false)
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

* bool DBRead()
* void DBWrite()

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
* virtual void DBReadNative(BinaryReader br)
* virtual void DBWriteNative(BinaryWriter bw)

### Операторы VisualObject
* this[string key]
* this[int column, int line]