
// Controller icon for Sprite3D nodes.
//
// [b]Deprecated[/b]: Use the new [ControllerIconTexture] texture resource and set it
// directly in [member Sprite3D.texture].
//
// @deprecated
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static ControllerIcons;

[Tool]
public partial class ControllerSprite3D : Sprite3D
{
	public override string[] _GetConfigurationWarnings()
	{
		return ["This node is deprecated, and will be removed in a future version.\n\nRemove this script and use the new ControllerIconTexture resource\nby setting it directly in Sprite3D's texture property."];
	}	

	[Export]
	public string path { 
		get 
		{ 
			return _path; 
		}

		set 
		{
			_path = value;
			if( IsInsideTree() )
			{
				Texture = CI.ParsePath(path, force_type);
			}
		} 
	}
	private string _path = "";

	[Export]
	public EShowMode show_only {
		get 
		{
			return _show_only;
		}

		set
		{
			_show_only = value;
			OnInputTypeChanged(CI.LastInputType, CI.LastController);
		}
	}
	private EShowMode _show_only = EShowMode.ANY;

	[Export]
	public EInputType force_type
	{
		get
		{
			return _force_type;
		}

		set
		{
			_force_type = value;
			OnInputTypeChanged(CI.LastInputType, CI.LastController);
		}
	}
	private EInputType _force_type = EInputType.NONE;

	public override void _Ready()
	{
		CI.InputTypeChanged += OnInputTypeChanged;
		this.path = path;
	}

	public void OnInputTypeChanged( EInputType inputType, int controller )
	{
		if( show_only == EShowMode.ANY ||
			(show_only == EShowMode.KEYBOARD_MOUSE && inputType == EInputType.KEYBOARD_MOUSE) ||
			(show_only == EShowMode.CONTROLLER && inputType == EInputType.CONTROLLER))
		{
			Visible = true;
			this.path = path;
		}
		else
			Visible = false;
	}

	private string GetTTSString()
	{
		return CI.ParsePathToTTS(path, force_type);
	}	
}
