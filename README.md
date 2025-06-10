# Controller Icons C#
A C# port of rsubtil's Controller Icons plugin for Godot. It provides icons for all major controllers as well as automatic icon swapping system based on player input. The original plugin can be found here: https://github.com/rsubtil/controller_icons

Full credit to Ricardo Subtil for putting together this incredible plugin.

## Installation and how to use
Please refer to [the original plugin](https://github.com/rsubtil/controller_icons) for detailed instructions. But TL;DR;

The minimum Godot version is 4.1.2 (stable).

Download this repository and copy the `addons` folder to your project root directory.

Then activate **Controller Icons** in your project plugins.

## Why make this plugin?
Godot supports GDScript plugins with C# projects. However, there was an unfortunate intermittent issue when using this plugin that caused the plugin to very rarely crash on game startup. Issue: https://github.com/rsubtil/controller_icons/issues/95

I ported this plugin to ensure this crash cannot happen. However, that doesn't mean my code is bug free 😂

## Will I maintain this plugin to match the original plugin's improvements/fixes over time?
I am not committing to this. This effort was largely done so I could use the plugin in my own project which I need to focus on. Now that the plugin works, I likely won't come back to it unless there is something else I need. Don't hesitate to reach out if there are any issues or requests though, if I have spare time or if it's quick I can try make the changes.

## Is there anything lacking from the original plugin?
In theory, no. However, I have not tested the deprecated features at all yet. So if you were using deprecated features before, I have no clue what will happen if you try this plugin and you Shouldn't™ be using them if you're new to this plugin.

Feel free to let me know, but I also do intend on testing this myself someday and ironing out any issues when I get around to it.

## License

The addon is licensed under the MIT license. Full details at [LICENSE](LICENSE). Original License at [ORIGINAL LICENSE](ORIGINAL_LICENSE).

### Additional credits (taken from rsubtil's repo):
The controller assets are [Xelu's FREE Controllers & Keyboard PROMPTS](https://thoseawesomeguys.com/prompts/), made by Nicolae (XELU) Berbece and under Creative Commons 0 _(CC0)_. Some extra icons were created and contributed to this addon, also on the same CC0 license:

- [@TacticalLaptopBag](https://github.com/TacticalLaptopBag): Apostrophe, backtick, comma, equals, forward slash and period keys.
- [@DataPlusProgram](https://github.com/DataPlusProgram): Mouse wheel up and down, mouse side buttons up and down.

The icon was designed by [@adambelis](https://github.com/adambelis) ([#5](https://github.com/rsubtil/controller_icons/pull/5)) and is under Create Commons 0 _(CC0)_. It uses the [Godot's logo](https://github.com/godotengine/godot/blob/master/icon.svg) which is under Creative Commons Attribution 4.0 International License _(CC-BY-4.0 International)_
