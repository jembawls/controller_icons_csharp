using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class ControllerIcons : Node
{
	[Signal]
	public delegate void InputTypeChangedEventHandler(InputType input_type, int controller);

	public enum InputType {
        NONE,
		KEYBOARD_MOUSE, // The input is from the keyboard and/or mouse.
		CONTROLLER // The input is from a controller.
	}

	public enum PathType {
		INPUT_ACTION, // The path is an input action.
		JOYPAD_PATH, // The path is a generic joypad path.
		SPECIFIC_PATH // The path is a specific path.
	}

    public static ControllerIcons CI { get; set; }

    private Godot.Collections.Dictionary<string, Texture2D> _cached_icons = [];
    public Godot.Collections.Dictionary<string, Godot.Collections.Array<InputEvent>> _custom_input_actions = [];

    private Mutex _cached_callables_lock = new();
	private readonly List<Callable> _cached_callables = [];

	public InputType _last_input_type = InputType.KEYBOARD_MOUSE;
	public int _last_controller;
	public ControllerSettings _settings = ResourceLoader.Load<ControllerSettings>("res://addons/controller_icons/settings.tres");
	public string _base_extension = "png";

    // Custom mouse velocity calculation, because Godot
    // doesn't implement it on some OSes apparently
    private const float _MOUSE_VELOCITY_DELTA = 0.1f;
    private float _t;
    private int _mouse_velocity;

    private bool setLikelyInput = false;

    private ControllerMapper _mapper = new();

    // Default actions will be the builtin editor actions when
    // the script is at editor ("tool") level. To pickup more
    // actions available, these have to be queried manually
    public readonly Godot.Collections.Array<string> _builtin_keys = [
        "input/ui_accept", "input/ui_cancel", "input/ui_copy",
        "input/ui_cut", "input/ui_down", "input/ui_end",
        "input/ui_filedialog_refresh", "input/ui_filedialog_show_hidden",
        "input/ui_filedialog_up_one_level", "input/ui_focus_next",
        "input/ui_focus_prev", "input/ui_graph_delete",
        "input/ui_graph_duplicate", "input/ui_home",
        "input/ui_left", "input/ui_menu", "input/ui_page_down",
        "input/ui_page_up", "input/ui_paste", "input/ui_redo",
        "input/ui_right", "input/ui_select", "input/ui_swap_input_direction",
        "input/ui_text_add_selection_for_next_occurrence",
        "input/ui_text_backspace", "input/ui_text_backspace_all_to_left",
        "input/ui_text_backspace_all_to_left.macos",
        "input/ui_text_backspace_word", "input/ui_text_backspace_word.macos",
        "input/ui_text_caret_add_above", "input/ui_text_caret_add_above.macos",
        "input/ui_text_caret_add_below", "input/ui_text_caret_add_below.macos",
        "input/ui_text_caret_document_end", "input/ui_text_caret_document_end.macos",
        "input/ui_text_caret_document_start", "input/ui_text_caret_document_start.macos",
        "input/ui_text_caret_down", "input/ui_text_caret_left",
        "input/ui_text_caret_line_end", "input/ui_text_caret_line_end.macos",
        "input/ui_text_caret_line_start", "input/ui_text_caret_line_start.macos",
        "input/ui_text_caret_page_down", "input/ui_text_caret_page_up",
        "input/ui_text_caret_right", "input/ui_text_caret_up",
        "input/ui_text_caret_word_left", "input/ui_text_caret_word_left.macos",
        "input/ui_text_caret_word_right", "input/ui_text_caret_word_right.macos",
        "input/ui_text_clear_carets_and_selection", "input/ui_text_completion_accept",
        "input/ui_text_completion_query", "input/ui_text_completion_replace",
        "input/ui_text_dedent", "input/ui_text_delete",
        "input/ui_text_delete_all_to_right", "input/ui_text_delete_all_to_right.macos",
        "input/ui_text_delete_word", "input/ui_text_delete_word.macos",
        "input/ui_text_indent", "input/ui_text_newline", "input/ui_text_newline_above",
        "input/ui_text_newline_blank", "input/ui_text_scroll_down",
        "input/ui_text_scroll_down.macos", "input/ui_text_scroll_up",
        "input/ui_text_scroll_up.macos", "input/ui_text_select_all",
        "input/ui_text_select_word_under_caret", "input/ui_text_select_word_under_caret.macos",
        "input/ui_text_submit", "input/ui_text_toggle_insert_mode", "input/ui_undo",
        "input/ui_up",
    ];

    public ControllerIcons()
    {
        CI = this;
        Setup();
    }

    private void _set_last_input_type( InputType __last_input_type, int __last_controller)
    {
        _last_input_type = __last_input_type;
        _last_controller = __last_controller;
        CI.EmitSignalInputTypeChanged(_last_input_type, _last_controller);
    }

    public void Setup()
	{
        ProcessMode = Node.ProcessModeEnum.Always;
        if( Engine.IsEditorHint() )
		{
            ParseInputActions();
        }
	}

    public override void _ExitTree()
    {
        Cleanup();
    }

	public void Cleanup()
	{
        if( _mapper != null && !_mapper.IsQueuedForDeletion() )
        {
            _mapper.Free();
        }
    }

	public void ParseInputActions()
	{
        _custom_input_actions.Clear();

        foreach( string key in _builtin_keys )
		{
            Godot.Collections.Dictionary data = (Godot.Collections.Dictionary)ProjectSettings.GetSetting(key);
            if( data.Count > 1 && data.ContainsKey("events") && data["events"].GetType() == typeof( Godot.Collections.Array ))
			{
                _add_custom_input_action(key.TrimPrefix("input/"), data);
            }
		}

        // A script running at editor ("tool") level only has
        // the default mappings. The way to get around this is
        // manually parsing the project file and adding the
        // new input actions to lookup.
        ConfigFile proj_file = new();
        if( proj_file.Load("res://project.godot") != Error.Ok )
		{
            GD.PrintErr("Failed to open \"project.godot\"! Custom input actions will not work on editor view!");
            return;
        }

		if( proj_file.HasSection("input") )
		{
            foreach( string input_action in proj_file.GetSectionKeys("input") )
			{
                Godot.Collections.Dictionary data = (Godot.Collections.Dictionary)proj_file.GetValue("input", input_action);
                _add_custom_input_action(input_action, data);
			}
        }
	}

	public override void _Ready()
	{
        Input.JoyConnectionChanged += OnJoyConnectionChangedEventHandler;
        _settings = ResourceLoader.Load<ControllerSettings>("res://addons/controller_icons/settings.tres");

		_settings ??= new();
        _mapper ??= new();

        if( !string.IsNullOrWhiteSpace(_settings.custom_file_extension) )
		{
            _base_extension = _settings.custom_file_extension;
        }

        // Wait a frame to give a chance for the app to initialize
        setLikelyInput = true;
    }

    private void OnJoyConnectionChangedEventHandler( long device, bool connected )
    {
        if( connected )
		{
            _set_last_input_type(InputType.CONTROLLER, (int)device);
        }
        else
		{
			if( Input.GetConnectedJoypads().Count == 0 )
			{
				_set_last_input_type(InputType.KEYBOARD_MOUSE, -1);
			}
            else
			{
				_set_last_input_type(InputType.CONTROLLER, Input.GetConnectedJoypads().First());
			}
        }
	}

    public override void _Input( InputEvent e )
	{
        InputType input_type = _last_input_type;
        int controller = _last_controller;
        switch( e.GetClass() )
		{
			case "InputEventKey":
			case "InputEventMouseButton":
                input_type = InputType.KEYBOARD_MOUSE;
                break;
			case "InputEventMouseMotion":
				if( _settings.allow_mouse_remap && _test_mouse_velocity((e as InputEventMouseMotion).Relative) )
				{
					input_type = InputType.KEYBOARD_MOUSE;
				}
                break;
			case "InputEventJoypadButton":
				input_type = InputType.CONTROLLER;
                controller = e.Device;
                break;
            case "InputEventJoypadMotion":
                if( Mathf.Abs((e as InputEventJoypadMotion).AxisValue) > _settings.joypad_deadzone )
				{
					input_type = InputType.CONTROLLER;
					controller = e.Device;
				}
                break;
		}

		if( input_type != _last_input_type || controller != _last_controller )
		{
            _set_last_input_type(input_type, controller);
        }
	}

	private bool _test_mouse_velocity(Vector2 relative_vec )
	{
		if( _t > _MOUSE_VELOCITY_DELTA )
		{
            _t = 0;
            _mouse_velocity = 0;
        }

        // We do a component sum instead of a length, to save on a
		// sqrt operation, and because length_squared is negatively
		// affected by low value vectors (<10).
		// It is also good enough for this system, so reliability
		// is sacrificed in favor of speed.
        _mouse_velocity += Mathf.RoundToInt(Mathf.Abs(relative_vec.X) + Mathf.Abs(relative_vec.Y));

        return _mouse_velocity / _MOUSE_VELOCITY_DELTA > _settings.mouse_min_movement;

    }

	private void set_likely_current_input_type()
	{
        // Set input type to what's likely being used currently
        if( Input.GetConnectedJoypads().Count == 0 )
		{
            _set_last_input_type(InputType.KEYBOARD_MOUSE, -1);
        }
		else
		{
            _set_last_input_type(InputType.CONTROLLER, Input.GetConnectedJoypads().First());
        }
	}

	public override void _Process( double delta )
	{
        _t += (float)delta;
		if( setLikelyInput )
		{
            set_likely_current_input_type();
            setLikelyInput = false;
        }

        if( _cached_callables.Count > 0 && _cached_callables_lock.TryLock() )
		{
			// UPGRADE: In Godot 4.2, for-loop variables can be
			// statically typed:
			// for f: Callable in _cached_callables:
			foreach( Callable f in _cached_callables )
			{
				if( f.Target != null && f.Delegate != null ) 
					f.Call();
			}
		}

        _cached_callables.Clear();
        _cached_callables_lock.Unlock();
    }

	private void _add_custom_input_action( string input_action , Godot.Collections.Dictionary data )
	{
        _custom_input_actions[input_action] = (Godot.Collections.Array<InputEvent>)data["events"];
    }

	private void refresh()
	{
		// All it takes is to signal icons to refresh paths
        EmitSignalInputTypeChanged(_last_input_type, _last_controller);
    }

	private ControllerSettings.Devices get_joypad_type( int controller = int.MinValue )
	{
		if( controller == int.MinValue )
		{
            controller = _last_controller;
        }

        return _mapper._get_joypad_type(controller, _settings.joypad_fallback);
    }

	public Texture2D parse_path( string path, InputType? input_type = InputType.NONE, int last_controller = int.MinValue )
	{		
		if( input_type == null )
		{
			return null;
		}

		if( input_type == InputType.NONE )
		{
            input_type = _last_input_type;
        }

		if( last_controller == int.MinValue )
		{
            last_controller = _last_controller;
        }

        List<string> root_paths = _expand_path(path, input_type.Value, last_controller);
        foreach( string root_path in root_paths )
		{
			if( _load_icon(root_path) != Error.Ok )
			{
				continue;
			}

            return _cached_icons[root_path];
		}

        return null;
    }

	public List<Texture2D> parse_event_modifiers(InputEvent e )
	{
		if( e == null || e is not InputEventWithModifiers )
			return [];

        InputEventWithModifiers eModifiers = e as InputEventWithModifiers;

        List<Texture2D> icons = [];
		List<string> modifiers = [];
		if( eModifiers.CommandOrControlAutoremap )
		{
			switch( OS.GetName() )
			{
				case "macOS":
					modifiers.Add("key/command");
                    break;
                default:
					modifiers.Add("key/ctrl");
                    break;
			}
		}

		if( eModifiers.CtrlPressed && !eModifiers.CommandOrControlAutoremap )
		{
			modifiers.Add("key/ctrl");
		}
		
		if( eModifiers.ShiftPressed )
		{
			modifiers.Add("key/shift");
		}

		if( eModifiers.AltPressed )
		{
			modifiers.Add("key/alt");
		}

		if( eModifiers.MetaPressed && !eModifiers.CommandOrControlAutoremap )
		{
			switch( OS.GetName() )
			{
				case "macOS":
					modifiers.Add("key/command");
		            break;
        	    default:
					modifiers.Add("key/win");
					break;
			}
		}

		foreach( string modifier in modifiers )
		{
			foreach( string icon_path in _expand_path(modifier, InputType.KEYBOARD_MOUSE, -1) )
			{
				if( _load_icon( icon_path ) == Error.Ok )
				{
					icons.Add( _cached_icons[icon_path] );
				}
			}
		}

        return icons;
    }

	public string parse_path_to_tts(string path, InputType? input_type = InputType.NONE, int controller = int.MinValue)
	{
		if( input_type == null )
            return "";

		if( input_type == InputType.NONE )
		{
            input_type = _last_input_type;
        }

		if( controller == int.MinValue )
		{
            controller = _last_controller;
        }

        var tts = _convert_path_to_asset_file(path, input_type.Value, controller);
        return _convert_asset_file_to_tts(tts.GetBaseName().GetFile());
    }

	private Texture parse_event( InputEvent e )
	{
		string path = _convert_event_to_path( e );
		if( string.IsNullOrWhiteSpace(path) )
			return null;

		List<string> base_paths = [
			_settings.custom_asset_dir + "/",
			"res://addons/controller_icons/assets/"
		];

		foreach( string base_path in base_paths )
		{
			if( string.IsNullOrWhiteSpace(base_path) )
				continue;

			string dictPath = base_path + path + "." + _base_extension;
			if( _load_icon(dictPath) != Error.Ok )
				continue;

			return _cached_icons[dictPath];
		}

        return null;
    }

    public PathType get_path_type(string path)
    {
        if (_custom_input_actions.ContainsKey(path) || InputMap.HasAction(path))
            return PathType.INPUT_ACTION;
        else if( path.Split("/")[0] == "joypad" )
			return PathType.JOYPAD_PATH;
		else
			return PathType.SPECIFIC_PATH;
    }

    public InputEvent get_matching_event( string path, InputType input_type = InputType.NONE, long controller = int.MinValue )
	{
		if( input_type == InputType.NONE )
		{
            input_type = _last_input_type;
        }
		
		if( controller == int.MinValue )
		{
            controller = _last_controller;
        }

		Godot.Collections.Array<InputEvent> events;
		if( _custom_input_actions.TryGetValue(path, out Godot.Collections.Array<InputEvent> value) )
			events = value;
		else
			events = InputMap.ActionGetEvents(path);

		InputEvent fallback = null;
		foreach( InputEvent inputEvent in events )
		{
			if( !IsInstanceValid(inputEvent) ) continue;

			switch( inputEvent.GetClass() )
			{
				case "InputEventKey":
				case "InputEventMouse":
				case "InputEventMouseMotion":
				case "InputEventMouseButton":
					if( input_type == InputType.KEYBOARD_MOUSE )
						return inputEvent;
                    break;
                case "InputEventJoypadButton":
				case "InputEventJoypadMotion":
					if( input_type == InputType.CONTROLLER )
					{
						// Use the first device specific mapping if there is one.
						if( inputEvent.Device == controller )
							return inputEvent;
						// Otherwise use the first "all devices" mapping.
						else if( fallback == null && inputEvent.Device < 0 )
							fallback = inputEvent;
					}
                break;
            }
		}

		return fallback;
    }

	private List<string> _expand_path(string path, InputType input_type, int controller)
	{
		List<string> paths = [];
		List<string> base_paths = [
			_settings.custom_asset_dir + "/",
			"res://addons/controller_icons/assets/"
		];
		foreach( string base_path in base_paths )
		{
			if( string.IsNullOrWhiteSpace(base_path) )
				continue;
			string asset_path = base_path + _convert_path_to_asset_file(path, input_type, controller);

			paths.Add(asset_path + "." + _base_extension);
		}

        return paths;
    }

	private string _convert_path_to_asset_file( string path, InputType input_type, int controller )
	{
		switch( (PathType)get_path_type(path) )
		{
			case PathType.INPUT_ACTION:
                InputEvent e = get_matching_event(path, input_type, controller);
                if( e != null)
                    return _convert_event_to_path(e);
                return path;
			case PathType.JOYPAD_PATH:
				return _mapper._convert_joypad_path(path, controller, _settings.joypad_fallback);
            case PathType.SPECIFIC_PATH:
			default:
				return path;
		}
	}

	private static string _convert_asset_file_to_tts( string path )
	{
        return path switch
        {
            "shift_alt" => "shift",
            "esc" => "escape",
            "backspace_alt" => "backspace",
            "enter_alt" => "enter",
            "enter_tall" => "keypad enter",
            "arrow_left" => "left arrow",
            "arrow_right" => "right arrow",
            "del" => "delete",
            "arrow_up" => "up arrow",
            "arrow_down" => "down arrow",
            "ctrl" => "control",
            "kp_add" => "keypad plus",
            "mark_left" => "left mark",
            "mark_right" => "right mark",
            "bracket_left" => "left bracket",
            "bracket_right" => "right bracket",
            "tilda" => "tilde",
            "lb" => "left bumper",
            "rb" => "right bumper",
            "lt" => "left trigger",
            "rt" => "right trigger",
            "l_stick_click" => "left stick click",
            "r_stick_click" => "right stick click",
            "l_stick" => "left stick",
            "r_stick" => "right stick",
            _ => path,
        };
    }

	private string _convert_event_to_path( InputEvent e )
	{
		if( e is InputEventKey keyEvent )
		{
            // If this is a physical key, convert to localized scancode
            if( keyEvent.Keycode == 0 )
				return _convert_key_to_path(DisplayServer.KeyboardGetKeycodeFromPhysical(keyEvent.PhysicalKeycode));
			return _convert_key_to_path(keyEvent.Keycode);
		}
		else if( e is InputEventMouseButton mouseEvent )
			return _convert_mouse_button_to_path(mouseEvent.ButtonIndex);
		else if( e is InputEventJoypadButton joypadButtonEvent )
			return _convert_joypad_button_to_path(joypadButtonEvent.ButtonIndex, joypadButtonEvent.Device);
		else if( e is InputEventJoypadMotion joypadMotionEvent )
			return _convert_joypad_motion_to_path(joypadMotionEvent.Axis, joypadMotionEvent.Device);

		return "";
    }

	private static string _convert_key_to_path( Key keycode )
	{
        return keycode switch
        {
            Key.Escape => "key/esc",
            Key.Tab => "key/tab",
            Key.Backspace => "key/backspace_alt",
            Key.Enter => "key/enter_alt",
            Key.KpEnter => "key/enter_tall",
            Key.Insert => "key/insert",
            Key.Delete => "key/del",
            Key.Print => "key/print_screen",
            Key.Home => "key/home",
            Key.End => "key/end",
            Key.Left => "key/arrow_left",
            Key.Up => "key/arrow_up",
            Key.Right => "key/arrow_right",
            Key.Down => "key/arrow_down",
            Key.Pageup => "key/page_up",
            Key.Pagedown => "key/page_down",
            Key.Shift => "key/shift_alt",
            Key.Ctrl => "key/ctrl",
            Key.Meta => OS.GetName() switch
            {
                "macOS" => "key/command",
                _ => "key/meta",
            },
            Key.Alt => "key/alt",
            Key.Capslock => "key/caps_lock",
            Key.Numlock => "key/num_lock",
            Key.F1 => "key/f1",
            Key.F2 => "key/f2",
            Key.F3 => "key/f3",
            Key.F4 => "key/f4",
            Key.F5 => "key/f5",
            Key.F6 => "key/f6",
            Key.F7 => "key/f7",
            Key.F8 => "key/f8",
            Key.F9 => "key/f9",
            Key.F10 => "key/f10",
            Key.F11 => "key/f11",
            Key.F12 => "key/f12",
            Key.KpMultiply or Key.Asterisk => "key/asterisk",
            Key.KpSubtract or Key.Minus => "key/minus",
            Key.KpAdd => "key/plus_tall",
            Key.Kp0 => "key/0",
            Key.Kp1 => "key/1",
            Key.Kp2 => "key/2",
            Key.Kp3 => "key/3",
            Key.Kp4 => "key/4",
            Key.Kp5 => "key/5",
            Key.Kp6 => "key/6",
            Key.Kp7 => "key/7",
            Key.Kp8 => "key/8",
            Key.Kp9 => "key/9",
            Key.Unknown => "",
            Key.Space => "key/space",
            Key.Quotedbl => "key/quote",
            Key.Plus => "key/plus",
            Key.Key0 => "key/0",
            Key.Key1 => "key/1",
            Key.Key2 => "key/2",
            Key.Key3 => "key/3",
            Key.Key4 => "key/4",
            Key.Key5 => "key/5",
            Key.Key6 => "key/6",
            Key.Key7 => "key/7",
            Key.Key8 => "key/8",
            Key.Key9 => "key/9",
            Key.Semicolon => "key/semicolon",
            Key.Less => "key/mark_left",
            Key.Greater => "key/mark_right",
            Key.Question => "key/question",
            Key.A => "key/a",
            Key.B => "key/b",
            Key.C => "key/c",
            Key.D => "key/d",
            Key.E => "key/e",
            Key.F => "key/f",
            Key.G => "key/g",
            Key.H => "key/h",
            Key.I => "key/i",
            Key.J => "key/j",
            Key.K => "key/k",
            Key.L => "key/l",
            Key.M => "key/m",
            Key.N => "key/n",
            Key.O => "key/o",
            Key.P => "key/p",
            Key.Q => "key/q",
            Key.R => "key/r",
            Key.S => "key/s",
            Key.T => "key/t",
            Key.U => "key/u",
            Key.V => "key/v",
            Key.W => "key/w",
            Key.X => "key/x",
            Key.Y => "key/y",
            Key.Z => "key/z",
            Key.Bracketleft => "key/bracket_left",
            Key.Backslash => "key/slash",
            Key.Slash => "key/forward_slash",
            Key.Bracketright => "key/bracket_right",
            Key.Asciitilde => "key/tilda",
            Key.Quoteleft => "key/backtick",
            Key.Apostrophe => "key/apostrophe",
            Key.Comma => "key/comma",
            Key.Equal => "key/equals",
            Key.Period or Key.KpPeriod => "key/period",
            _ => "",
        };
    }

	private static string _convert_mouse_button_to_path( MouseButton button )
	{
        return button switch
        {
            MouseButton.Left => "mouse/left",
            MouseButton.Right => "mouse/right",
            MouseButton.Middle => "mouse/middle",
            MouseButton.WheelUp => "mouse/wheel_up",
            MouseButton.WheelDown => "mouse/wheel_down",
            MouseButton.Xbutton1 => "mouse/side_down",
            MouseButton.Xbutton2 => "mouse/side_up",
            _ => "mouse/sample",
        };
    }

	private string _convert_joypad_button_to_path( JoyButton button , int controller )
	{
        string path;
        switch( button )
		{
			case JoyButton.A:
				path = "joypad/a";
                break;
            case JoyButton.B:
				path = "joypad/b";
                break;
			case JoyButton.X:
				path = "joypad/x";
                break;
			case JoyButton.Y:
				path = "joypad/y";
                break;
			case JoyButton.LeftShoulder:
				path = "joypad/lb";
                break;
			case JoyButton.RightShoulder:
				path = "joypad/rb";
                break;
			case JoyButton.LeftStick:
				path = "joypad/l_stick_click";
                break;
			case JoyButton.RightStick:
				path = "joypad/r_stick_click";
                break;
			case JoyButton.Back:
				path = "joypad/select";
                break;
			case JoyButton.Start:
				path = "joypad/start";
                break;
			case JoyButton.DpadUp:
				path = "joypad/dpad_up";
                break;
			case JoyButton.DpadDown:
				path = "joypad/dpad_down";
                break;
			case JoyButton.DpadLeft:
				path = "joypad/dpad_left";
                break;
			case JoyButton.DpadRight:
				path = "joypad/dpad_right";
                break;
			case JoyButton.Guide:
				path = "joypad/home";
                break;
			case JoyButton.Misc1:
				path = "joypad/share";
        		break;
    		default:
        		return "";
		};

        return _mapper._convert_joypad_path(path, controller, _settings.joypad_fallback);
    }

    private string _convert_joypad_motion_to_path(JoyAxis axis, int controller)
    {
        string path;
		switch( axis )
		{
			case JoyAxis.LeftX:
			case JoyAxis.LeftY:
				path = "joypad/l_stick";
                break;
            case JoyAxis.RightX:
			case JoyAxis.RightY:
				path = "joypad/r_stick";
                break;
            case JoyAxis.TriggerLeft:
				path = "joypad/lt";
                break;
            case JoyAxis.TriggerRight:
				path = "joypad/rt";
                break;
            default:
				return "";
		}

        return _mapper._convert_joypad_path(path, controller, _settings.joypad_fallback);
    }

    private Error _load_icon( string path )
	{
		if( _cached_icons.ContainsKey(path) ) return Error.Ok;

		Texture2D tex;
		if( path.StartsWith("res://") )
		{
			if( ResourceLoader.Exists(path) )
			{
				tex = ResourceLoader.Load<Texture2D>(path);
				if( tex == null )
					return Error.FileCorrupt;
			}
			else
				return Error.FileNotFound;
		}
		else
		{
			if( !FileAccess.FileExists(path) )
				return Error.FileNotFound;

			Image img = new();
			Error err = img.Load(path);
			if( err != Error.Ok )
				return err;
			tex = ImageTexture.CreateFromImage(img);			
		}
		_cached_icons[path] = tex;

        return Error.Ok;
    }

	public void _defer_texture_load( Callable f )
	{
        _cached_callables_lock.Lock();
        _cached_callables.Add(f);
        _cached_callables_lock.Unlock();
    }
}	
