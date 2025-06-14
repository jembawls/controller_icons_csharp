using Godot;
using System;
using System.Linq;

[Tool]
public partial class ControllerMapper : RefCounted
{
    public string _convert_joypad_path( string path, int device, ControllerSettings.Devices fallback, ControllerSettings.Devices force_controller_icon_style = ControllerSettings.Devices.NONE )
	{
        return _get_joypad_type(device, fallback, force_controller_icon_style) switch
        {
            ControllerSettings.Devices.LUNA => _convert_joypad_to_luna(path),
            ControllerSettings.Devices.PS3 => _convert_joypad_to_ps3(path),
            ControllerSettings.Devices.PS4 => _convert_joypad_to_ps4(path),
            ControllerSettings.Devices.PS5 => _convert_joypad_to_ps5(path),
            ControllerSettings.Devices.STADIA => _convert_joypad_to_stadia(path),
            ControllerSettings.Devices.STEAM => _convert_joypad_to_steam(path),
            ControllerSettings.Devices.SWITCH => _convert_joypad_to_switch(path),
            ControllerSettings.Devices.JOYCON => _convert_joypad_to_joycon(path),
            ControllerSettings.Devices.XBOX360 => _convert_joypad_to_xbox360(path),
            ControllerSettings.Devices.XBOXONE => _convert_joypad_to_xboxone(path),
            ControllerSettings.Devices.XBOXSERIES => _convert_joypad_to_xboxseries(path),
            ControllerSettings.Devices.STEAM_DECK => _convert_joypad_to_steamdeck(path),
            ControllerSettings.Devices.OUYA => _convert_joypad_to_ouya(path),
            _ => "",
        };
    }

	public ControllerSettings.Devices _get_joypad_type( int device, ControllerSettings.Devices fallback, ControllerSettings.Devices force_controller_icon_style = ControllerSettings.Devices.NONE)
	{
		if( force_controller_icon_style != ControllerSettings.Devices.NONE )
		{
            return force_controller_icon_style;
        }

		Godot.Collections.Array<int> available = Input.GetConnectedJoypads();
		if( available.Count == 0 )
		{
			return fallback;
		}

		// If the requested joypad is not on the connected joypad list, try using the last known connected joypad
		if( !available.Contains( device ) )
			device = ControllerIcons.CI._last_controller;

		// If that fails too, then use whatever joypad we have connected right now
		if( !available.Contains( device ) )
			device = available.First();

		var controller_name = Input.GetJoyName(device);
		if( controller_name.Contains("Luna Controller") )
			return ControllerSettings.Devices.LUNA;
		else if( controller_name.Contains("PS3 Controller") )
			return ControllerSettings.Devices.PS3;
		else if( controller_name.Contains("PS4 Controller") || controller_name.Contains("DUALSHOCK 4") )
			return ControllerSettings.Devices.PS4;
		else if( controller_name.Contains("PS5 Controller") || controller_name.Contains("DualSense") )
			return ControllerSettings.Devices.PS5;
		else if( controller_name.Contains("Stadia Controller") )
			return ControllerSettings.Devices.STADIA;
		else if( controller_name.Contains("Steam Controller") )
			return ControllerSettings.Devices.STEAM;
		else if( controller_name.Contains("Switch Controller") || controller_name.Contains("Switch Pro Controller") )
			return ControllerSettings.Devices.SWITCH;
		else if( controller_name.Contains("Joy-Con") )
			return ControllerSettings.Devices.JOYCON;
		else if( controller_name.Contains("Xbox 360 Controller") )
			return ControllerSettings.Devices.XBOX360;
		else if( controller_name.Contains("Xbox One") || controller_name.Contains("X-Box One") || controller_name.Contains("Xbox Wireless Controller") )
			return ControllerSettings.Devices.XBOXONE;
		else if( controller_name.Contains("Xbox Series") )
			return ControllerSettings.Devices.XBOXSERIES;
		else if( controller_name.Contains("Steam Deck") || controller_name.Contains("Steam Virtual Gamepad") )
			return ControllerSettings.Devices.STEAM_DECK;
		else if( controller_name.Contains("OUYA Controller") )
			return ControllerSettings.Devices.OUYA;
		else
			return fallback;
	}


	public string _convert_joypad_to_luna( string path )
	{
		path = path.Replace("joypad", "luna");
		switch( path.Substring(path.Find("/") + 1) )
		{
			case "select":
				return path.Replace("/select", "/circle");
			case "start":
				return path.Replace("/start", "/menu");
			case "share":
				return path.Replace("/share", "/microphone");
			default:
				return path;
		}
	}

	public string _convert_joypad_to_playstation( string path )
	{
		switch( path.Substring(path.Find("/") + 1) )
		{
			case "a":
				return path.Replace("/a", "/cross");
			case "b":
				return path.Replace("/b", "/circle");
			case "x":
				return path.Replace("/x", "/square");
			case "y":
				return path.Replace("/y", "/triangle");
			case "lb":
				return path.Replace("/lb", "/l1");
			case "rb":
				return path.Replace("/rb", "/r1");
			case "lt":
				return path.Replace("/lt", "/l2");
			case "rt":
				return path.Replace("/rt", "/r2");
			default:
				return path;
		}
	}

	public string _convert_joypad_to_ps3( string path )
	{
		return _convert_joypad_to_playstation( path.Replace("joypad", "ps3") );
	}

	public string _convert_joypad_to_ps4( string path )
	{
		path = _convert_joypad_to_playstation(path.Replace("joypad", "ps4"));
		switch( path.Substring(path.Find("/") + 1) )
		{
			case "select":
				return path.Replace("/select", "/share");
			case "start":
				return path.Replace("/start", "/options");
			case "share":
				return path.Replace("/share", "/");
			default:
				return path;
		}
	}

	public string _convert_joypad_to_ps5(string path)
	{
		path = _convert_joypad_to_playstation(path.Replace("joypad", "ps5"));
		switch( path.Substring(path.Find("/") + 1) )
		{
			case "select":
				return path.Replace("/select", "/share");
			case "start":
				return path.Replace("/start", "/options");
			case "home":
				return path.Replace("/home", "/assistant");
			case "share":
				return path.Replace("/share", "/microphone");
			default:
				return path;
		}
	}

	public string _convert_joypad_to_stadia( string path )
	{
		path = path.Replace("joypad", "stadia");
		switch( path.Substring(path.Find("/") + 1) )
		{
			case "lb":
				return path.Replace("/lb", "/l1");
			case "rb":
				return path.Replace("/rb", "/r1");
			case "lt":
				return path.Replace("/lt", "/l2");
			case "rt":
				return path.Replace("/rt", "/r2");
			case "select":
				return path.Replace("/select", "/dots");
			case "start":
				return path.Replace("/start", "/menu");
			case "share":
				return path.Replace("/share", "/select");
			default:
				return path;
		}
	}


	public string _convert_joypad_to_steam( string path )
	{
		path = path.Replace("joypad", "steam");
		switch( path.Substring(path.Find("/") + 1) )
		{
			case "r_stick_click":
				return path.Replace("/r_stick_click", "/right_track_center");
			case "select":
				return path.Replace("/select", "/back");
			case "home":
				return path.Replace("/home", "/system");
			case "dpad":
				return path.Replace("/dpad", "/left_track");
			case "dpad_up":
				return path.Replace("/dpad_up", "/left_track_up");
			case "dpad_down":
				return path.Replace("/dpad_down", "/left_track_down");
			case "dpad_left":
				return path.Replace("/dpad_left", "/left_track_left");
			case "dpad_right":
				return path.Replace("/dpad_right", "/left_track_right");
			case "l_stick":
				return path.Replace("/l_stick", "/stick");
			case "r_stick":
				return path.Replace("/r_stick", "/right_track");
			default:
				return path;
		}
	}


	public string _convert_joypad_to_switch( string path )
	{
        path = path.Replace("joypad", "switch");
        switch( path.Substring(path.Find("/") + 1) )
		{
			case "a":
				return path.Replace("/a", "/b");
			case "b":
				return path.Replace("/b", "/a");
			case "x":
				return path.Replace("/x", "/y");
			case "y":
				return path.Replace("/y", "/x");
			case "lb":
				return path.Replace("/lb", "/l");
			case "rb":
				return path.Replace("/rb", "/r");
			case "lt":
				return path.Replace("/lt", "/zl");
			case "rt":
				return path.Replace("/rt", "/zr");
			case "select":
				return path.Replace("/select", "/minus");
			case "start":
				return path.Replace("/start", "/plus");
			case "share":
				return path.Replace("/share", "/square");
			default:
				return path;
		}
	}

	public string _convert_joypad_to_joycon( string path )
	{
		path = _convert_joypad_to_switch(path);

		switch( path.Substring(path.Find("/") + 1) )
		{
			case "dpad_up":
				return path.Replace("/dpad_up", "/up");
			case "dpad_down":
				return path.Replace("/dpad_down", "/down");
			case "dpad_left":
				return path.Replace("/dpad_left", "/left");
			case "dpad_right":
				return path.Replace("/dpad_right", "/right");
			default:
				return path;
		}
	}


	public string _convert_joypad_to_xbox360( string path )
	{
		path = path.Replace("joypad", "xbox360");
		switch( path.Substring(path.Find("/") + 1) )
		{
			case "select":
				return path.Replace("/select", "/back");
			default:
				return path;
		}
	}

	public string _convert_joypad_to_xbox_modern(string path)
	{
		switch( path.Substring(path.Find("/") + 1) )
		{
			case "select":
				return path.Replace("/select", "/view");
			case "start":
				return path.Replace("/start", "/menu");
			default:
				return path;
		}
	}

	public string _convert_joypad_to_xboxone(string path)
	{
		return _convert_joypad_to_xbox_modern(path.Replace("joypad", "xboxone"));
	}

	public string _convert_joypad_to_xboxseries(string path)
	{
		return _convert_joypad_to_xbox_modern(path.Replace("joypad", "xboxseries"));
	}

	public string _convert_joypad_to_steamdeck(string path)
	{
		path = path.Replace("joypad", "steamdeck");
		switch( path.Substring(path.Find("/") + 1) )
		{
			case "lb":
				return path.Replace("/lb", "/l1");
			case "rb":
				return path.Replace("/rb", "/r1");
			case "lt":
				return path.Replace("/lt", "/l2");
			case "rt":
				return path.Replace("/rt", "/r2");
			case "select":
				return path.Replace("/select", "/inventory");
			case "start":
				return path.Replace("/start", "/menu");
			case "home":
				return path.Replace("/home", "/steam");
			case "share":
				return path.Replace("/share", "/dots");
			default:
				return path;
		}
	}


	public string _convert_joypad_to_ouya(string path)
	{
        path = path.Replace("joypad", "ouya");
        switch( path.Substring(path.Find("/") + 1) )
		{
			case "a":
				return path.Replace("/a", "/o");
			case "x":
				return path.Replace("/x", "/u");
			case "b":
				return path.Replace("/b", "/a");
			case "lb":
				return path.Replace("/lb", "/l1");
			case "rb":
				return path.Replace("/rb", "/r1");
			case "lt":
				return path.Replace("/lt", "/l2");
			case "rt":
				return path.Replace("/rt", "/r2");
			case "start":
				return path.Replace("/start", "/menu");
			case "share":
				return path.Replace("/share", "/microphone");
			default:
				return path;
		}
}
}
