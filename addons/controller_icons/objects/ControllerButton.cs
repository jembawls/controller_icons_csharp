
using Godot;
using System;
using System.Collections.Generic;
using static ControllerIcons;

[Tool]
public partial class ControllerButton : Button
{
	// Controller icon for Button nodes.
	//
	// [b]Deprecated[/b]: Use the new [ControllerIconTexture] texture resource and set it
	// directly in [member Button.icon].
	//
	// @deprecated
	public override string[] _GetConfigurationWarnings()
    {
        return ["This node is deprecated, and will be removed in a future version.\n\nRemove this script and use the new ControllerIconTexture resource\nby setting it directly in Button's icon property."];
    }	

	[Export]
	public string path
	{
        get { return _path; }
		set
		{
            _path = value;
			if( IsInsideTree() )
			{
                Icon = CI.parse_path(path, force_type);
            }
        }
    }
    private string _path = "";

	[Export]
	public ShowMode show_only {
		get 
		{
            return _show_only;
        }

		set
		{
            _show_only = value;
            _on_input_type_changed(CI._last_input_type, CI._last_controller);
        }
	}
    private ShowMode _show_only = ShowMode.ANY;

	[Export]
	public InputType force_type
	{
		get
		{
            return _force_type;
        }

		set
		{
            _force_type = value;
            _on_input_type_changed(CI._last_input_type, CI._last_controller);
        }
	}
    private InputType _force_type = InputType.NONE;

	public override void _Ready()
	{
        CI.InputTypeChanged += _on_input_type_changed;
        this.path = path;
    }

	public void _on_input_type_changed( InputType input_type, int controller )
	{
		if( show_only == ShowMode.ANY ||
			(show_only == ShowMode.KEYBOARD_MOUSE && input_type == InputType.KEYBOARD_MOUSE) ||
			(show_only == ShowMode.CONTROLLER && input_type == InputType.CONTROLLER))
		{
            Visible = true;
            this.path = path;
        }
		else
            Visible = false;
    }

	private string get_tts_string()
	{
		return CI.parse_path_to_tts(path, force_type);
    }	

}
