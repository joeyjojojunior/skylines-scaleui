# skylines-scaleui
ScaleUI mod for Cities: Skylines (http://steamcommunity.com/sharedfiles/filedetails/?id=409338401)

This mod adds a slider to the options menu to scale / resize the complete user interface in Cities: Skylines.

## Usage
Go to ScaleUI in the Options menu, and adjust the slider. Then hit Apply. 

To reset the scale to default values, hit the Reset button.

The applied scale will be saved and re-applied every time you start/load a game. 

## Known issues
* If you use other mods which add UI elements, this mod will probably reposition them off-screen. To work around this, position them in the center of the screen before scaling, and then reposition them once you are done.

* The close buttons in a few tools might move to a different position.

Please report any issues you find.

## Known Incompatibilities

* **Extended Public Transport UI:** Adding a new transportation route will mess up the spacing of the Lines Overview window. Resetting and re-applying scaling after adding a new line will fix this (until you add another line). 

## Notice on building the project
I set up MonoDevelop to automatically delete and copy the resulting .dll using Pre-/After-Build commands. It uses deldll.cmd to achieve, which will **delete** a file, so be careful. Additionally, the project references the assemblies on my local hard drive.

## Attributions 

Inspired by [TextScaleMod](http://steamcommunity.com/sharedfiles/filedetails/?id=407225523).

Thanks to nlight for help with Reflection.

Thanks to [@githubpermutation](https://github.com/githubpermutation) for doing most of the hard work and creating the [first version of the mod](http://steamcommunity.com/sharedfiles/filedetails/?id=409338401).

Thanks to [@keallu](https://github.com/keallu) whose code I used to figure out how to implement the slider and configuration file.