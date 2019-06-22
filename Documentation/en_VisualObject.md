# VisualObject
## Public fields and properties of VisualObject
* string **Name**
	* Object name. Class type name by default.
* string **FullName**
	* Object full name (names of all objects up to the Root).
* int **X**
	* X coordinate relative to Parent object.
* int **Y**
	* Y coordinate relative to Parent object.
* int **Width**
* int **Height**
* VisualObject **Parent**
	* Parent object. Null for RootVisualObject.
* RootVisualObject **Root**
	* Root of the interface tree. Null before first Update() call. Use GetRoot() to calculate manually.
* UIConfiguration **Configuration**
	* Object touching and drawing settings.
* UIStyle **Style**
	* Object tile and wall style.
* Action<VisualObject, Touch> **Callback**
	* Function to call on touching this object with the grand design.
* virtual dynamic **Provider**
	* Tile provider of current interface tree (ITileProvider).
* bool **UsesDefaultMainProvider**
	* True if Provider links to Main.tile provider.
* int **ChildCount**
	* DEBUG property: Child array size.
* bool **Enabled**
	* Whether the object is enabled. Disabled objects are invisible, you can't touch them and they don't receive updates.
* bool **Visible**
	* Whether the object is visible. Object becomes invisible when it is outside of bounds of layout.
	Invisible object receive updates.
* virtual bool **Active**
	* Object is Active when it is Enabled and Visible
* bool **Loaded**
	* True once the object was loaded.
* bool **Disposed**
	* True once the object was disposed.
* int **Layer**
	* Layer in Parent's Child array. Objects with higher layer are always higher in this array.
* virtual bool **Orderable**
	* Overridable property for disabling ability to be ordered in Parent's Child array.
* GridCell **Cell**
	* Cell of Parent's grid in which this object is. Null if not in Parent's grid.
* bool **ForceSection**
	* Objects draw with SentTileSquare by default. Set this field to force drawing this object with SendSection.
* int **ProviderX**
	* X coordinate relative to tile provider. Sets in Update() and by signal PulseType.PositionChanged.
* int **ProviderY**
	* Y coordinate relative to tile provider. Sets in Update() and by signal PulseType.PositionChanged.
* ExternalIndent **Bounds**
	* Bounds (relative to this object) in which tiles of this object is allowed to draw (and tiles of child sub-tree as well).
* IEnumerable<VisualObject> **DescendantDFS**
	* Deep Fast Search method of iterating objects in sub-tree including this node.
* IEnumerable<VisualObject> **DescendantBFS**
	* Broad Fast Search method of iterating objects in sub-tree including this node.

## Protected fields and properties of VisualObject
* object **Locker**
	* Locker for locking node related operations.
* ConcurrentDictionary<string, object> **Shortcuts**
	* Runtime storage for node related data. Can be accessed by operator this[string key].
* List<VisualObject> **Child**
	* List of child objects.
* VisualObject[,] **Grid**
	* Child grid. Use operator[,] to get or set grid elements.
* IEnumerable<VisualObject> **ChildrenFromTop**
	* Iterates over Child array starting with objects on top.
* IEnumerable<VisualObject> **ChildrenFromBottom**
	* Iterates over Child array starting with objects at bottom.
* IEnumerable<(int, int)> **Points**
	* Iterates over object points (coordinates) relative to this node.
* IEnumerable<(int, int)> **AbsolutePoints**
	* Iterates over object points (coordinates) relative to world map.
* IEnumerable<(int, int)> **ProviderPoints**
	* Iterates over object points (coordinates) relative to tile provider.

## Public methods of VisualObject
* virtual VisualObject **Add**(VisualObject child, int? layer)
	* Adds object as a child in specified layer (use object's layer by default). Does nothing if object is already a child.
* VisualObject **Remove**(VisualObject child)
	* Removes child object. Calls Dispose() on removed object so you can't use this object anymore.
* VisualObject **Select**(VisualObject o)
	* Enable specified child and disable all other child objects.
* VisualObject **Selected**()
	* Searches for first Enabled child.
* VisualObject **Deselect**()
	* Enables all child objects.
* VisualObject **GetRoot**()
	* Searches for root node (VisualObject) in hierarchy. Must be a RootVisualObject in a valid TUI tree.
* bool **IsAncestorFor**(VisualObject child)
	* Checks if this node is an ancestor for an object.
* virtual bool **SetTop**(VisualObject child)
	* Places child object on top of layer. This function will be called automatically on child touch
	if Parent has Configuration.Ordered equal to true and this child has Orderable equal to true.
* (int X, int Y, int Width, int Height) **XYWH**(int dx, int dy)
	* Get object position and size.
		* dx - X coordinate delta
		* dy - Y coordinate delta
* virtual VisualObject **SetXYWH**(int x, int y, int width, int height)
	* Sets object position and size.
* VisualObject **SetXY**(int x, int y)
* VisualObject **SetWH**(int width, int height)
* VisualObject **Move**(int dx, int dy)
	* Move object by delta x and delta y.
* VisualObject **MoveBack**(int dx, int dy)
	* Inverse function for Move()
* VisualObject **GetChild**(int index)
	* DEBUG function. Get child object by index in Child array.
* VisualObject **Enable**()
	* Enables object. See Enabled field for more.
* VisualObject **Disable**()
* bool **CalculateActive**()
	* Finds out if every object including this node and up to the Root is Active. Root must be a RootVisualObject.
* (int X, int Y) **RelativeXY**(int x, int y, VisualDOM parent)
	* Calculates coordinates relative to specified parent object.
* (int X, int Y) **AbsoluteXY**(int dx, int dy)
	* Calculates coordinates relative to world map.
* (int X, int Y) **ProviderXY**(int dx, int dy)
	* Calculates coordinates relative to tile provider.
* virtual dynamic **Tile**(int x, int y)
	* Returns tile by coordinates relative to this node point (x=0, y=0 is a top left point of this object).
* VisualObject **AddToLayout**(VisualObject child, int? layer)
	* Add object as a child in layout. Removes child alignment and grid positioning.
* VisualObject **SetupLayout**(Alignment alignment, Direction direction, Side side,
		ExternalIndent indent, int childOffset, bool boundsIsIndent)
	* Setup layout for child positioning.
		* alignment - Where to place all layout objects row/line.
		* direction - Direction of placing objects.
		* side - Side to which objects adjoin, relative to direction.
		* indent - Layout indent.
		* childOffset - Distance between objects in layout.
		* boundsIsIndent - Whether to draw objects/object tiles that are outside of bounds of indent or not.
* VisualObject **SetupGrid**(IEnumerable<ISize> columns, IEnumerable<ISize> lines, Indent indent, bool fillWithEmptyObjects)
	* Setup grid for child positioning. Use Absolute and Relative classes for specifying sizes.
		* columns - Column sizes (i.e. new ISize[] { new Absolute(10), new Relative(100) }).
		* lines - Line sizes.
		* indent - Grid indent.
		* fillWithEmptyObjects - Whether to fills all grid cells with empty VisualContainers.
* VisualObject **SetAlignmentInParent**(Alignment alignment, ExternalIndent indent, bool boundsIsIndent)
	* Setup alignment positioning inside parent. Removes layout and grid positioning.
* VisualObject **SetFullSize**(bool horizontal, bool vertical)
	* Set automatic stretching to parent size. Removes grid positioning.
* VisualObject **LayoutOffset**(int value)
	* Scrolling offset of layout in tiles. Used in ScrollBackground and ScrollBar widgets.

* VisualObject **Pulse**(PulseType type)
	* Send specified signal to all sub-tree including this node.
* VisualObject **PulseThis**(PulseType type)
	* Send specified signal only to this node.
* VisualObject **PulseChild**(PulseType type)
	* Send specified signal to sub-tree without this node.

* VisualObject **Update**()
	* Updates the node and the child sub-tree (only Enabled objects).
* VisualObject **UpdateThis**()
	* Updates related to this node only.
* VisualObject **UpdateChildPositioning**()
	* First updates child sizes, then calculates child positions based on sizes (layout, grid, alignment).
* VisualObject **UpdateChild**()
	* Updates all Enabled child objects (sub-tree without this node).
* VisualObject **PostUpdateThis**()
	* Updates related to this node and dependant on child updates. Executes after calling Update() on each child.

* VisualObject **Apply**()
	* Draws everything related to this VisualObject incluing all child sub-tree (directly changes tiles on tile provider).
* VisualObject **ApplyThis**()
	* Draws everything related to this particular VisualObject. Doesn't include drawing child objects.
* VisualObject **ApplyTiles**()
	* Apply tiles and walls for this node.
* VisualObject **ApplyChild**()
	* Apply sub-tree without applying this node.
* VisualObject **Clear**()
	* Clear all tiles inside this object with ITile.ClearEverything()

* VisualObject **Draw**(int dx, int dy, int width, int height, int playerIndex, int exceptPlayerIndex, bool? forceSection, bool frame)
	* Sends drawn object (rectangle of tiles) by SendTileSquare/SendSection packet to clients.
		* dx - X coordinate delta.
		* dy - Y coordinate delta.
		* width - Sending rectangle width, -1 for object.Width.
		* height - Sending rectangle height, -1 for object.Height.
		* playerIndex - Index of player to send to, -1 for all players.
		* exceptPlayerIndex - Index of player to ignore on sending.
		* forceSection - Whether to send with SendTileSquare or with SendSection. SendTileSquare (false) by default.
		* frame - Whether to send SectionFrame if sending with SendSection.
* VisualObject **DrawPoints**(IEnumerable<(int, int)> points, int playerIndex, int exceptPlayerIndex, bool? forceSection)
	* Send rectangle of tiles that contains each point in specified list of points.
* void **ShowGrid**()
	* DEBUG function for showing grid bounds.

* bool **DBRead**()
	* Read object data from database using overridable DBReadNative method.
* void **DBWrite**()
	* Write object data to database using overridable DBWriteNative method.
* bool **UDBRead**(int user)
	* Read user related object data from database using overridable UDBReadNative method.
* void **UDBWrite**(int user)
	* Write user related object data to database using overridable UDBWriteNative method.

## Protected methods of VisualObject
* virtual bool **CanTouch**(Touch touch)
	* Checks if specified touch can press this object child objects in sub-tree.
* virtual void **PostSetTop**(VisualObject o)
	* Overridable function that is called when object comes on top of the Parent's Child array layer.
* virtual bool **CanTouchThis**(Touch touch)
	* Checks if specified touch can press this object. Not to be confused with CanTouch method.
* virtual void **Invoke**(Touch touch)
	* Overridable function which is called when touch satisfies the conditions of pressing this object.
	Invokes Callback function by default.
* virtual void **PulseThisNative**(PulseType type)
	* Overridable function to handle pulse signal for this node.
* virtual void **UpdateThisNative**()
	* Overridable method for updates related to this node. Don't change this object's position/size in in this method.
* void **UpdateBounds**()
	* Calculate Bounds for this node (intersection of Parent's layout indent/alignment indent and Parent's Bounds).
* void **UpdateChildSize**()
	* Updates child sizes with call of overridable child.UpdateSizeNative().
* virtual (int, int) **UpdateSizeNative**()
	* Overridable method for determining object size depending on own data (image/text/etc size).
* void **UpdateFullSize**()
	* Updates this object size relative to Parent size if Configuration.FullSize is not None.
* void **UpdateAlignment**()
	* Sets position of child objects with set Configuration.Alignment.
* void **UpdateLayout**()
	* Set position for children in layout.
* void **UpdateGrid**()
	* Sets position for children in grid.
* virtual void **PostUpdateThisNative**()
	* Overridable method for updates related to this node and dependant on child updates.
* virtual void **ApplyThisNative**()
	* Overridable method for apply related to this node. By default draws tiles and walls if specified in Style.
* virtual void **ApplyTile**(int x, int y)
	* Overridable method for applying particular tile in ApplyTiles().
* virtual void **DBReadNative**(BinaryReader br)
	* Overridable method for reading from BinaryReader based on data from database.
* virtual void **DBWriteNative**(BinaryWriter bw)
	* Overridable method for writing to BinaryWriter for data to be stored in database.
* virtual void **UDBReadNative**(BinaryReader br, int user)
	* Overridable method for reading user related object data from database with BinaryReader.
* virtual void **UDBWriteNative**(BinaryWriter bw, int user)
	* Overridable method for writing user related object data to BinaryWriter to database.

## VisualObject operators
* **this**[string key]
	* Get/set node related data in runtime storage.
* **this**[int column, int line]
	* Get: Get a child in grid.

# RootVisualObject : VisualObject

## Public unique fields and properties of RootVisualObject:
* HashSet<int> **Players**
	* Set of players that are currently close enough to this interface.
* dynamic **Provider**
	* Read about Provider on main page.

## Public unique methods of RootVisualObject:
* virtual VisualContainer **ShowPopUp**(VisualObject popup, ContainerStyle style, Action<VisualObject> cancelCallback)
	* Draws popup object on top of all other child objects.
		* ContainerStyle style - Style of popup background.
		* Action<VisualObject> cancelCallback - Action to call when player touches popup background but not popup itself.
* virtual RootVisualObject **HidePopUp**()
* virtual RootVisualObject Alert(string text, UIStyle style, ButtonStyle okButtonStyle)
	* Show alert window with information text and "ok" button.
* virtual RootVisualObject Confirm(string text, Action<bool> callback, ContainerStyle windowStyle,
		ButtonStyle yesButtonStyle, ButtonStyle noButtonStyle)
	* Show confirm window with information text and "yes", "no" buttons.