README
======
**AquaPic Main Control**

Handles all functionality for the AquaPic aquarium controller, and provides a graphical user interface. Physical input and outputs are handled by [seperate modules or cards](https://github.com/brandtks/AquaPicModules).

### Status
All code is a work in progress. 

### Prerequisites
 * [Mono](http://www.mono-project.com/) is required for both building and running the AquaPic application. 
 * .NET on Windows should work. However, development is done on Linux so Windows is not supported. Finally .NET Core does not work at this time.
 * [GTK-Sharp Version 2.12](http://www.mono-project.com/docs/gui/gtksharp/) is required for running the AquaPic application. Recommend installing from the repos because all the packages in NuGet are now wrapping Gtk3.
 * NuGet version 2.12 or higher is required to install all the dependecies and MSBuild. NuGet can be installed via the repos. If Nuget isn't available or the version in the repos is too old, download nuget.exe using wget: 
 
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`wget https://dist.nuget.org/win-x86-commandline/latest/nuget.exe`. 
 
 * MSBuild is optional. The build script will install MSBuild if it can't be found on the system. 
 
### Building
Execute `./build`. If NuGet was installed via wget, then the build script will require the argument `-n "mono /path/to/nuget.exe"`. Another option is to use a C# IDE: [MonoDevelop](http://www.monodevelop.com/download/) (Linux), or [Visual Studio](https://www.visualstudio.com/vs/) (Windows and Mac).

### Dependencies
 * [cs-script](http://www.csscript.net/) - Oleg Shilo
 * [Json.NET](http://www.newtonsoft.com/json) - Newtonsoft
 * [FileHeloper](http://www.filehelpers.net/) - Devoo - Marcos Meli

### License
The AquaPic main control code is released under the terms of the GNU General Public License (GPL), version 3 or later. See COPYING for details.

### Website
 * [HackADay Build Log](https://hackaday.io/project/1436-aquapic-aquarium-controller)
 * [Less Updated Build Log](https://sites.google.com/site/aquapicbuildlog/)
