# TUI
Ingame user interface library for Terraria by [ASgo](https://github.com/ASgoPew) and [Anzhelika](https://github.com/AnzhelikaO).

* You can find english documentation [here](Documentation/en.md).
* Вы можете найти документацию на русском [здесь](Documentation/ru.md).

***

![](Documentation/Images/Minesweeper.gif)

***

## Installing
This plugin is for TShock version 4.3.26.
To install the plugin you need to do this:
* Add TUI.dll from archive to server root directory (near TerrariaServer.exe).
* Add ServerPlugins/TUIPlugin.dll from archive to ServerPlugins directory.

***

## Example
There is an example of plugin usage [here](TUIExample/TUIExamplePlugin.cs).
You can also find it in archive and play with it.
By default example will spawn interface at (0, 0) coordinates, so you have to use
```/tuipanel "TestPanel" <x> <y>``` command to move it to the place you want.
Be aware that interface will modify your map tiles irreversible if you don't use [FakeManager](https://github.com/AnzhelikaO/FakeManager) as provider.
Use The Grand Design to interact with interface.

![](Images/TUIPanelCommand.gif)