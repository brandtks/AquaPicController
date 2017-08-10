README
======
**AquaPic Main Control**

Handles all functionality for the AquaPic aquarium controller. Also provides a graphical user interface.

### Status
All code is a work in progress. 

### Prerequisites
 * [GtkSharp](http://www.mono-project.com/docs/gui/gtksharp/) - The Mono Project
 * [cs-script](http://www.csscript.net/) - Oleg Shilo
 * [Json.NET](http://www.newtonsoft.com/json) - Newtonsoft
 * [FileHeloper](http://www.filehelpers.net/) - Devoo - Marcos Meli
 * Either [Mono](http://www.mono-project.com/), or [.NET](https://www.microsoft.com/net/download). _.NET Core_ has not been tested.

### Toolchain
Unfortunately xbuild has been deprecated by the Mono project and will be removed in future updates. Instead msbuild is supposed to be used. msbuild is available though the AUR if using Arch, but for other distros or MacOS, good luck.
Another option is to use a C# IDE:
 * MonoDeveloper
 * Xamarin Studio
 * Visual Studio 

###Building
 * Add CSScript.dll, FileHelper.dll, and Newtonsoft.Json.dll to the AquaPicController/AquaPic/ directory. Must be in the same directory as AquaPic.csproj.
 * ```msbuild.exe /p:Configuration=Release AquaPicController/AquaPic/AquaPic.csproj```
 * Or just use one of the chunk graphical IDEs 

### License
The AquaPic main control code is released under the terms of the GNU General Public License (GPL), version 3 or later. See COPYING for details.

### Website
 * [HackADay Build Log](https://hackaday.io/project/1436-aquapic-aquarium-controller)
 * [Less Updated Build Log](https://sites.google.com/site/aquapicbuildlog/)
