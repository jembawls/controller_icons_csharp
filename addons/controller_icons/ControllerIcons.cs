using Godot;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class ControllerIcons : Node
{
	[Signal]
	public delegate void InputTypeChangedEventHandler(int inputType, int controller);

	public enum EInputType
	{
		NONE,
		KEYBOARD_MOUSE, // The input is from the keyboard and/or mouse.
		CONTROLLER // The input is from a controller.
	}

	public enum EPathType
	{
		INPUT_ACTION, // The path is an input action.
		JOYPAD_PATH, // The path is a generic joypad path.
		SPECIFIC_PATH // The path is a specific path.
	}

	public enum EShowMode
	{
		ANY, // Icon will be display on any input method.
		KEYBOARD_MOUSE, // Icon will be display only when the keyboard/mouse is being used.
		CONTROLLER // Icon will be display only when a controller is being used.
	}

	public static ControllerIcons CI { get; set; }

	private Godot.Collections.Dictionary<string, Texture2D> _CachedIcons = new();
	public Godot.Collections.Dictionary<string, Godot.Collections.Array<InputEvent>> CustomInputActions = new();

	private Mutex _CachedCallablesLock = new();
	private readonly List<Callable> _CachedCallables = new();

	public EInputType LastInputType = EInputType.KEYBOARD_MOUSE;
	public int LastController;
	public ControllerSettings Settings;
	public string BaseExtension = "png";

	// Custom mouse velocity calculation, because Godot
	// doesn't implement it on some OSes apparently
	private const float _MOUSE_VELOCITY_DELTA = 0.1f;
	private float _t;
	private int MouseVelocity;

	private bool setLikelyInput = false;

	private ControllerMapper Mapper = new();

	// Default actions will be the builtin editor actions when
	// the script is at editor ("tool") level. To pickup more
	// actions available, these have to be queried manually
	public readonly Godot.Collections.Array<string> BuiltInKeys = new(){
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
	};

	public ControllerIcons()
	{
		CI = this;
	}

	public override void _EnterTree()
	{
		// setup moved to EnterTree from constructor to handle cases where 
		// ControllerSettings is not yet setup. typically during hotreload.
		Setup();
	}

	private void SetLastInputType( EInputType lastInputType, int lastController)
	{
		LastInputType = lastInputType;
		LastController = lastController;
	#if GODOT4_4_OR_GREATER
		CI.EmitSignalInputTypeChanged((int)LastInputType, LastController);
	#else
		CI.EmitSignal( SignalName.InputTypeChanged, (int)LastInputType, LastController);
	#endif
	}

	public void Setup()
	{
		ProcessMode = Node.ProcessModeEnum.Always;
		if( Engine.IsEditorHint() )
		{
			ParseInputActions();
		}
		Settings = ResourceLoader.Load<ControllerSettings>("res://addons/controller_icons/settings.tres");
	}

	public static bool IsControllerIconsPluginReady()
	{
		return CI != null && CI.Settings != null;
	}

	public override void _ExitTree()
	{
		if( Input.Singleton.IsConnected( Input.SignalName.JoyConnectionChanged, Callable.From<long, bool>(OnJoyConnectionChangedEventHandler) ) )
		{
			Input.JoyConnectionChanged -= OnJoyConnectionChangedEventHandler;
		}
		Mapper = null;
	}

	public void ParseInputActions()
	{
		CustomInputActions.Clear();

		foreach( string key in BuiltInKeys )
		{
			Godot.Collections.Dictionary data = (Godot.Collections.Dictionary)ProjectSettings.GetSetting(key);
			if( data.Count > 1 && 
				data.ContainsKey("events") && 
				data["events"].AsGodotArray<InputEvent>() is Godot.Collections.Array<InputEvent> events )
			{
				AddCustomInputAction(key.TrimPrefix("input/"), events);
			}
		}

		// A script running at editor ("tool") level only has
		// the default mappings. The way to get around this is
		// manually parsing the project file and adding the
		// new input actions to lookup.
		ConfigFile projFile = new();
		if( projFile.Load("res://project.godot") != Error.Ok )
		{
			GD.PrintErr("Failed to open \"project.godot\"! Custom input actions will not work on editor view!");
			return;
		}

		if( projFile.HasSection("input") )
		{
			foreach( string input_action in projFile.GetSectionKeys("input") )
			{
				Godot.Collections.Dictionary data = (Godot.Collections.Dictionary)projFile.GetValue("input", input_action);
				AddCustomInputAction(input_action, data["events"].AsGodotArray<InputEvent>());
			}
		}
	}

	public override void _Ready()
	{
		Input.JoyConnectionChanged += OnJoyConnectionChangedEventHandler;

		Settings ??= new();
		Mapper ??= new();

		if( !string.IsNullOrWhiteSpace(Settings.custom_file_extension) )
		{
			BaseExtension = Settings.custom_file_extension;
		}

		// Wait a frame to give a chance for the app to initialize
		setLikelyInput = true;
	}
	private void OnJoyConnectionChangedEventHandler( long device, bool connected )
	{
		if( connected )
		{
			SetLastInputType(EInputType.CONTROLLER, (int)device);
		}
		else
		{
			if( Input.GetConnectedJoypads().Count == 0 )
			{
				SetLastInputType(EInputType.KEYBOARD_MOUSE, -1);
			}
			else
			{
				SetLastInputType(EInputType.CONTROLLER, Input.GetConnectedJoypads().First());
			}
		}
	}

	public override void _Input( InputEvent e )
	{
		//Input can fire before controller is ready, typically
		//during hotreload
		if( !IsControllerIconsPluginReady() )
			return;

		EInputType inputType = LastInputType;
		int controller = LastController;
		switch( e.GetClass() )
		{
			case "InputEventKey":
			case "InputEventMouseButton":
				inputType = EInputType.KEYBOARD_MOUSE;
				break;
			case "InputEventMouseMotion":
				if( Settings.allow_mouse_remap && TestMouseVelocity((e as InputEventMouseMotion).Relative) )
				{
					inputType = EInputType.KEYBOARD_MOUSE;
				}
				break;
			case "InputEventJoypadButton":
				inputType = EInputType.CONTROLLER;
				controller = e.Device;
				break;
			case "InputEventJoypadMotion":
				if( Mathf.Abs((e as InputEventJoypadMotion).AxisValue) > Settings.joypad_deadzone )
				{
					inputType = EInputType.CONTROLLER;
					controller = e.Device;
				}
				break;
		}

		if( inputType != LastInputType || controller != LastController )
		{
			SetLastInputType(inputType, controller);
		}
	}

	private bool TestMouseVelocity(Vector2 relative_vec )
	{
		if( _t > _MOUSE_VELOCITY_DELTA )
		{
			_t = 0;
			MouseVelocity = 0;
		}

		// We do a component sum instead of a length, to save on a
		// sqrt operation, and because length_squared is negatively
		// affected by low value vectors (<10).
		// It is also good enough for this system, so reliability
		// is sacrificed in favor of speed.
		MouseVelocity += Mathf.RoundToInt(Mathf.Abs(relative_vec.X) + Mathf.Abs(relative_vec.Y));

		return MouseVelocity / _MOUSE_VELOCITY_DELTA > Settings.mouse_min_movement;

	}

	private void SetLikelyCurrentInputType()
	{
		// Set input type to what's likely being used currently
		if( Input.GetConnectedJoypads().Count == 0 )
		{
			SetLastInputType(EInputType.KEYBOARD_MOUSE, -1);
		}
		else
		{
			SetLastInputType(EInputType.CONTROLLER, Input.GetConnectedJoypads().First());
		}
	}

	public override void _Process( double delta )
	{
		_t += (float)delta;
		if( setLikelyInput )
		{
			SetLikelyCurrentInputType();
			setLikelyInput = false;
		}

		if( _CachedCallables.Count > 0 && _CachedCallablesLock.TryLock() )
		{
			// UPGRADE: In Godot 4.2, for-loop variables can be
			// statically typed:
			// for f: Callable in _cached_callables:
			foreach( Callable f in _CachedCallables )
			{
				if( f.Target != null && f.Delegate != null ) 
					f.Call();
			}
		}

		_CachedCallables.Clear();
		_CachedCallablesLock.Unlock();
	}

	private void AddCustomInputAction( string input_action , Godot.Collections.Array<InputEvent> events )
	{
		CustomInputActions[input_action] = events;
	}

	private void refresh()
	{
		// All it takes is to signal icons to refresh paths		
	#if GODOT4_4_OR_GREATER
		EmitSignalInputTypeChanged((int)LastInputType, LastController);
	#else
		EmitSignal(SignalName.InputTypeChanged, (int)LastInputType, LastController);
	#endif
	}

	private ControllerSettings.Devices GetJoypadType( int controller = int.MinValue )
	{
		if( controller == int.MinValue )
		{
			controller = LastController;
		}

		return Mapper.GetJoypadType(controller, Settings.joypad_fallback);
	}

	public Texture2D ParsePath( string path, EInputType? inputType = EInputType.NONE, int lastController = int.MinValue, ControllerSettings.Devices forcedControllerIconStyle = ControllerSettings.Devices.NONE )
	{		
		if( inputType == null )
		{
			return null;
		}

		if( inputType == EInputType.NONE )
		{
			inputType = LastInputType;
		}

		if( lastController == int.MinValue )
		{
			lastController = LastController;
		}

		List<string> root_paths = ExpandPath(path, inputType.Value, lastController, forcedControllerIconStyle);
		foreach( string root_path in root_paths )
		{
			if( LoadIcon(root_path) != Error.Ok )
			{
				continue;
			}

			return _CachedIcons[root_path];
		}

		return null;
	}

	public List<Texture2D> ParseEventModifiers(InputEvent e )
	{
		if( e == null || e is not InputEventWithModifiers )
			return new();

		InputEventWithModifiers eModifiers = e as InputEventWithModifiers;

		List<Texture2D> icons = new();
		List<string> modifiers = new();
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
			foreach( string iconPath in ExpandPath(modifier, EInputType.KEYBOARD_MOUSE, -1) )
			{
				if( LoadIcon( iconPath ) == Error.Ok )
				{
					icons.Add( _CachedIcons[iconPath] );
				}
			}
		}

		return icons;
	}

	public string ParsePathToTTS(string path, EInputType? input_type = EInputType.NONE, int controller = int.MinValue)
	{
		if( input_type == null )
			return "";

		if( input_type == EInputType.NONE )
		{
			input_type = LastInputType;
		}

		if( controller == int.MinValue )
		{
			controller = LastController;
		}

		var tts = ConvertPathToAssetFile(path, input_type.Value, controller);
		return ConvertAssetFileToTTS(tts.GetBaseName().GetFile());
	}

	private Texture ParseEvent( InputEvent e )
	{
		string path = ConvertEventToPath( e );
		if( string.IsNullOrWhiteSpace(path) )
			return null;

		List<string> basePaths = new(){
			Settings.custom_asset_dir + "/",
			"res://addons/controller_icons/assets/"
		};

		foreach( string basePath in basePaths )
		{
			if( string.IsNullOrWhiteSpace(basePath) )
				continue;

			string dictPath = basePath + path + "." + BaseExtension;
			if( LoadIcon(dictPath) != Error.Ok )
				continue;

			return _CachedIcons[dictPath];
		}

		return null;
	}

	public EPathType GetPathType(string path)
	{
		if (CustomInputActions.ContainsKey(path) || InputMap.HasAction(path))
			return EPathType.INPUT_ACTION;
		else if( path.Split("/")[0] == "joypad" )
			return EPathType.JOYPAD_PATH;
		else
			return EPathType.SPECIFIC_PATH;
	}

	public InputEvent GetMatchingEvent( string path, EInputType inputType = EInputType.NONE, long controller = int.MinValue )
	{
		if( inputType == EInputType.NONE )
		{
			inputType = LastInputType;
		}
		
		if( controller == int.MinValue )
		{
			controller = LastController;
		}

		Godot.Collections.Array<InputEvent> events;
		if( CustomInputActions.TryGetValue(path, out Godot.Collections.Array<InputEvent> value) )
			events = value;
		else
			events = InputMap.ActionGetEvents(path);

		List<InputEvent> fallbacks = new();
		foreach( InputEvent inputEvent in events )
		{
			if( !IsInstanceValid(inputEvent) ) continue;

			switch( inputEvent.GetClass() )
			{
				case "InputEventKey":
				case "InputEventMouse":
				case "InputEventMouseMotion":
				case "InputEventMouseButton":
					if( inputType == EInputType.KEYBOARD_MOUSE )
						return inputEvent;
					break;
				case "InputEventJoypadButton":
				case "InputEventJoypadMotion":
					if( inputType == EInputType.CONTROLLER )
					{
						// Use the first device specific mapping if there is one.
						if( inputEvent.Device == controller )
							return inputEvent;
						// Otherwise use the first "all devices" mapping.
						if( inputEvent.Device < 0 ) // All-device event
							fallbacks.Insert(0, inputEvent );
						else
							fallbacks.Add(inputEvent);
					}
				break;
			}
		}

		return fallbacks.Count > 0 ? fallbacks[0] : null;
	}

	private List<string> ExpandPath(string path, EInputType inputType, int controller, ControllerSettings.Devices forceControllerIconStyle = ControllerSettings.Devices.NONE)
	{
		List<string> paths = new();
		List<string> basePaths = new(){
			Settings.custom_asset_dir + "/",
			"res://addons/controller_icons/assets/"
		};
		foreach( string basePath in basePaths )
		{
			if( string.IsNullOrWhiteSpace(basePath) )
				continue;
			string asset_path = basePath + ConvertPathToAssetFile(path, inputType, controller, forceControllerIconStyle);

			paths.Add(asset_path + "." + BaseExtension);
		}

		return paths;
	}

	private string ConvertPathToAssetFile( string path, EInputType inputType, int controller, ControllerSettings.Devices forceControllerIconStyle = ControllerSettings.Devices.NONE )
	{
		switch( (EPathType)GetPathType(path) )
		{
			case EPathType.INPUT_ACTION:
				InputEvent e = GetMatchingEvent(path, inputType, controller);
				if( e != null)
					return ConvertEventToPath(e, controller, forceControllerIconStyle);
				return path;
			case EPathType.JOYPAD_PATH:
				return Mapper.ConvertJoypadPath(path, controller, Settings.joypad_fallback, forceControllerIconStyle);
			case EPathType.SPECIFIC_PATH:
			default:
				return path;
		}
	}

	private static string ConvertAssetFileToTTS( string path )
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

	private string ConvertEventToPath( InputEvent e, int controller = int.MinValue, ControllerSettings.Devices forcedControllerIconStyle = ControllerSettings.Devices.NONE )
	{
		if( controller == int.MinValue )
		{
			controller = LastController;
		}

		if( e is InputEventKey keyEvent )
		{
			// If this is a physical key, convert to localized scancode
			if( keyEvent.Keycode == 0 )
				return ConvertKeyToPath(DisplayServer.KeyboardGetKeycodeFromPhysical(keyEvent.PhysicalKeycode));
			return ConvertKeyToPath(keyEvent.Keycode);
		}
		else if( e is InputEventMouseButton mouseEvent )
			return ConvertMouseButtonToPath(mouseEvent.ButtonIndex);
		else if( e is InputEventJoypadButton joypadButtonEvent )
			return ConvertJoypadButtonToPath(joypadButtonEvent.ButtonIndex, controller, forcedControllerIconStyle);
		else if( e is InputEventJoypadMotion joypadMotionEvent )
			return ConvertJoypadMotionToPath(joypadMotionEvent.Axis, controller, forcedControllerIconStyle);

		return "";
	}

	private static string ConvertKeyToPath( Key keycode )
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

	private static string ConvertMouseButtonToPath( MouseButton button )
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

	private string ConvertJoypadButtonToPath( JoyButton button , int controller, ControllerSettings.Devices forcedControllerIconStyle = ControllerSettings.Devices.NONE )
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

		return Mapper.ConvertJoypadPath(path, controller, Settings.joypad_fallback, forcedControllerIconStyle);
	}

	private string ConvertJoypadMotionToPath(JoyAxis axis, int controller, ControllerSettings.Devices forcedControllerIconStyle = ControllerSettings.Devices.NONE)
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

		return Mapper.ConvertJoypadPath(path, controller, Settings.joypad_fallback, forcedControllerIconStyle);
	}

	private Error LoadIcon( string path )
	{
		if( _CachedIcons.ContainsKey(path) ) return Error.Ok;

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
		_CachedIcons[path] = tex;

		return Error.Ok;
	}

	public void DeferTextureLoad( Callable f )
	{
		_CachedCallablesLock.Lock();
		_CachedCallables.Add(f);
		_CachedCallablesLock.Unlock();
	}
}	
