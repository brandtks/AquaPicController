README
======
**AquaPic Main Control**

Handles all functionality for the AquaPic aquarium controller. Also provides a graphical user interface.

### Status
All code is a work in progress. 

### Prerequisites
  * [Mono](http://www.mono-project.com/). ```sudo apt install mono-complete```
  * [.NET Core](https://www.microsoft.com/net/core). However this has not been tested.
  * .NET on Windows will probably also work. However, Windows isn't a real OS so that isn't tested or supported.

### Toolchain
NuGet is required to install all the dependecies and MSBuild. Recommend not installing NuGet though a package manager. 
The version of NuGet in some repositories is too old to download all the required dependecies. Instead download nuget.exe 
using wget, ```wget https://dist.nuget.org/win-x86-commandline/latest/nuget.exe```. Then use mono to execute NuGet, 
```mono /path/to/nuget.exe```. mono-complete must be installed for this to work. MSBuild is optional. The build script 
will install MSBuild if it can't be found on the system. 

Another option is to use a C# IDE:
 * MonoDeveloper
 * Xamarin Studio
 * Visual Studio if you feel the need to use Windows

### Building
 * Edit Makefile-defines.mk to point to the NuGet _'executable'_.
 * ```./build```
 * Or just use one of the chunk graphical IDEs 

### Dependencies
 * [GtkSharp](http://www.mono-project.com/docs/gui/gtksharp/) - The Mono Project
 * [cs-script](http://www.csscript.net/) - Oleg Shilo
 * [Json.NET](http://www.newtonsoft.com/json) - Newtonsoft
 * [FileHeloper](http://www.filehelpers.net/) - Devoo - Marcos Meli

### License
The AquaPic main control code is released under the terms of the GNU General Public License (GPL), version 3 or later. See COPYING for details.

### Website
 * [HackADay Build Log](https://hackaday.io/project/1436-aquapic-aquarium-controller)
 * [Less Updated Build Log](https://sites.google.com/site/aquapicbuildlog/)
