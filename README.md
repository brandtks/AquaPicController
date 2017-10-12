README
======
**AquaPic Main Control**

Handles all functionality for the AquaPic aquarium controller. Also provides a graphical user interface.
### Status
All code is a work in progress. 
### Prerequisites
 * [Mono](http://www.mono-project.com/) is required for both building and running the AquaPic application. 
 * [GTK-Sharp Version 2.12](http://www.mono-project.com/docs/gui/gtksharp/) is required for running the AquaPic application. All the packages in NuGet are now wrapping Gtk3.
 Recommend installing though a package manager such as apt because that should put all the required libraries somewhere the mono runtime can find.

.NET on Windows will also work. However, development is done on Linux so Windows is not supported. .NET Core does not work.
### Toolchain
NuGet version 2.12 or higher is required to install all the dependecies and MSBuild. Recommend not installing NuGet though a package manager. 
The version of NuGet in some repositories is too old to download all the required dependecies. Instead download nuget.exe using wget, 

```wget https://dist.nuget.org/win-x86-commandline/latest/nuget.exe```. 

Then use mono to execute NuGet, ```mono /path/to/nuget.exe```. 

MSBuild is optional. The build script will install MSBuild if it can't be found on the system. 

Another option is to use a C# IDE: [MonoDevelop](http://www.monodevelop.com/download/) (Linux), or [Visual Studio](https://www.visualstudio.com/vs/) (Windows and Mac) 
### Building
Edit defines.build to point to the NuGet _'executable'_, then execute ```./build```. Or just use one of the chunk graphical IDEs. 
### Dependencies
 * [cs-script](http://www.csscript.net/) - Oleg Shilo
 * [Json.NET](http://www.newtonsoft.com/json) - Newtonsoft
 * [FileHeloper](http://www.filehelpers.net/) - Devoo - Marcos Meli
### License
The AquaPic main control code is released under the terms of the GNU General Public License (GPL), version 3 or later. See COPYING for details.
### Website
 * [HackADay Build Log](https://hackaday.io/project/1436-aquapic-aquarium-controller)
 * [Less Updated Build Log](https://sites.google.com/site/aquapicbuildlog/)
