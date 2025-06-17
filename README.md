<img src="https://github.com/user-attachments/assets/a35304b7-66fc-4755-8127-71d578695662" width=15%>

# Controller Icons C#!

A C# port of rsubtil's Controller Icons plugin for Godot. It provides icons for all major controllers as well as automatic icon swapping system based on player input. The original plugin + more detailed information can be found here: https://github.com/rsubtil/controller_icons

Full credit to [Ricardo Subtil (rsubtil)](https://github.com/rsubtil) for putting together this incredible plugin.

## Installation and how to use
Please refer to [the original plugin](https://github.com/rsubtil/controller_icons) for detailed instructions. But there is one other additional installation step for C#.

> [!IMPORTANT]
> *Note: The minimum Godot version is 4.1.2 (stable). As this is a C# plugin, this only works with the .NET version of Godot.*

1. Download this repository and copy the `addons` folder to your project root directory.
2. If this is a new project, create a C# solution file if you haven't already (Project -> Tools -> C# -> Create C# Solution) and build your project (Alt+B by default - or click the little hammer on the top right, next to the play button).
3. Activate **Controller Icons** in your project plugins (Project -> Project Settings -> Plugins -> Click Enabled checkbox)

You're good to go! Create a ControllerIconTexture, configure it to your needs, then use it wherever you need it. Done!

## How can I switch from the GDScript version to the C# version in an existing project?
I've only done some basic testing on this but it seems to work. However, your project's specifics will have to be taken into consideration. Ensure you back things up before you try!

1. Close your editor.
2. Delete (or move) the GDScript ControllerIcons plugin from the `addons` directory.
3. Copy in the C# ControllerIcons plugin into the `addons` directory.
4. Open your IDE/Text Editor of choice (Even Notepad++ - anything with a search and replace feature) and search and replace within your project for all instances of:
    * "ControllerIcons.gd" --> "ControllerIcons.cs"
    * "ControllerIconTexture.gd" --> "ControllerIconTexture.cs"
5. __If you are using any of the Deprecated types__ in your project, you will need to search and replace them also. (__Note:__ Be careful with your search and replace here as some of the original GDScript filenames are fairly generic.)
    * "Sprite.gd" --> "ControllerSprite2D.cs"
    * "Sprite3D.gd" --> "ControllerSprite3D.cs"
    * "TextureRect.gd" --> "ControllerTextureRect.cs"
    * "Button.gd" --> "ControllerButton.cs"
6. Build your project.
     * If, somehow, you aren't using a text editor/IDE, you may have to launch the engine and build in there and then restart the engine.
8. From this point on, you will have a bunch of UID warnings/errors. This is because the UIDs of the old plugin won't match the new UIDs of the new plugin. This shouldn't block the plugin from working, but you'll likely want to hunt down each one manually and fix them. The way to fix it is to just copy the UID in the warning/error, look for where it is referenced in your project (likely some `metadata/_custom_type_script` line in a `*.tscn` file) and remove the line  - or replace it with the UID that corresponds with the new C# file (most likely the UID in `ControllerIconTexture.cs.uid` if you are not using deprecated features).

And you should be done!

## Why make this port?
Godot supports GDScript plugins with C# projects. However, there was an unfortunate intermittent issue when using this plugin that caused the plugin to very rarely crash on game startup. Issue: https://github.com/rsubtil/controller_icons/issues/95

It appears that this may be an engine-related issue. So I ported this plugin to ensure this crash cannot happen in my C# project. However, that doesn't mean my code is bug free 😂

## Will I maintain this plugin to match the original plugin's improvements/fixes over time?
I am not committing to this. This effort was largely done so I could use the plugin in my own project which I need to focus on. Now that the plugin works, I likely won't come back to it unless there is something else I need. Don't hesitate to reach out if there are any issues or requests though, if I have spare time or if it's quick I can try make the changes.

## Is there anything lacking from the original plugin?
In theory, no. However, I have only done the bare-bones testing of the deprecated features. So if you were using deprecated features before, I have no clue what will happen if you try this plugin. But if you're new to the plugin, you shouldn't be using them anyways so you Should™ be okay.

## License

The addon is licensed under the MIT license. Full details at [LICENSE](LICENSE). Original License at [ORIGINAL LICENSE](ORIGINAL_LICENSE). Original plugin by [Ricardo Subtil (rsubtil)](https://github.com/rsubtil).

### Additional credits (taken from rsubtil's repo):
The controller assets are [Xelu's FREE Controllers & Keyboard PROMPTS](https://thoseawesomeguys.com/prompts/), made by Nicolae (XELU) Berbece and under Creative Commons 0 _(CC0)_. Some extra icons were created and contributed to this addon, also on the same CC0 license:

- [@TacticalLaptopBag](https://github.com/TacticalLaptopBag): Apostrophe, backtick, comma, equals, forward slash and period keys.
- [@DataPlusProgram](https://github.com/DataPlusProgram): Mouse wheel up and down, mouse side buttons up and down.

The original icon was designed by [@adambelis](https://github.com/adambelis) ([#5](https://github.com/rsubtil/controller_icons/pull/5)) and is under Creative Commons 0 _(CC0)_. It uses the [Godot's logo](https://github.com/godotengine/godot/blob/master/icon.svg) which is under Creative Commons Attribution 4.0 International License _(CC-BY-4.0 International)_. I just added the C# to it, and this version is also under the Creative Commons 0 _(CC0)_.
