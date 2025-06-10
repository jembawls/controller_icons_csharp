
// Controller icon for Sprite2D nodes.
//
// [b]Deprecated[/b]: Use the new [ControllerIconTexture] texture resource and set it
// directly in [member Sprite2D.texture].
//
// @deprecated
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static ControllerIcons;

[Tool]
public partial class ControllerSprite2D : Sprite2D
{
	public override string[] _GetConfigurationWarnings()
    {
        return ["This node is deprecated, and will be removed in a future version.\n\nRemove this script and use the new ControllerIconTexture resource\nby setting it directly in Sprite2D's texture property."];
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
				if( force_type > 0 )
					Texture = CI.parse_path(path, force_type - 1);
                else
					Texture = CI.parse_path(path);
            }
		} 
	}
    private string _path = "";

	[Export(PropertyHint.Flags, "Both,Keyboard/Mouse,Controller")]
	public int show_only {
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
    private int _show_only = 0;

	[Export(PropertyHint.Flags, "None,Keyboard/Mouse,Controller")]
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
    private InputType _force_type = 0;

	public override void _Ready()
	{
        CI.InputTypeChanged += _on_input_type_changed;
        this.path = path;
    }

	public void _on_input_type_changed( InputType input_type, int controller )
	{
		if( show_only == 0 ||
			(show_only == 1 && input_type == InputType.KEYBOARD_MOUSE) ||
			(show_only == 2 && input_type == InputType.CONTROLLER))
		{
            Visible = true;
            this.path = path;
        }
		else
            Visible = false;
    }

	private string get_tts_string()
	{
		if( force_type != 0 )
			return CI.parse_path_to_tts(path, force_type - 1);
        else
			return CI.parse_path_to_tts(path);
    }	
}
