using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class ControllerIconPlugin : EditorPlugin
{
    private ControllerIconEditorInspector inspector_plugin;

    public override void _EnablePlugin()
	{
        AddAutoloadSingleton("ControllerIcons", "res://addons/controller_icons/ControllerIcons.cs");
    }

	public override void _DisablePlugin()
	{
        RemoveAutoloadSingleton("ControllerIcons");
    }

	public override void _EnterTree()
	{
        inspector_plugin = new()
        {
            editor_interface = EditorInterface.Singleton
        };

        AddInspectorPlugin(inspector_plugin);
    }

	public override void _ExitTree()
	{
        RemoveInspectorPlugin(inspector_plugin);
    }
}
