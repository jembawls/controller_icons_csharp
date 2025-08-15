#if TOOLS
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class ControllerIconPlugin : EditorPlugin
{
	private ControllerIconEditorInspector inspectorPlugin;
	public static EditorInterface EditorInterface 
	{ 
		get
		{
		#if GODOT4_2_OR_GREATER
			return EditorInterface.Singleton;
		#else
			return staticRef.GetEditorInterface();
		#endif
		}
	}
	
#if GODOT4_1
	private static ControllerIconPlugin staticRef { get; set; }
#endif

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
#if GODOT4_1
		staticRef = this;
#endif
		inspectorPlugin = new();

		AddInspectorPlugin(inspectorPlugin);
	}

	public override void _ExitTree()
	{
		RemoveInspectorPlugin(inspectorPlugin);
	}
}
#endif