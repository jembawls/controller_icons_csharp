using Godot;
using System;
using System.Linq;

[Tool]
public partial class ControllerMapper : RefCounted
{
	public string ConvertJoypadPath( string path, int device, ControllerSettings.Devices fallback, ControllerSettings.Devices forceControllerIconStyle = ControllerSettings.Devices.NONE )
	{
		return GetJoypadType(device, fallback, forceControllerIconStyle) switch
		{
			ControllerSettings.Devices.LUNA => ConvertJoypadToLuna(path),
			ControllerSettings.Devices.PS3 => ConvertJoypadToPS3(path),
			ControllerSettings.Devices.PS4 => ConvertJoypadToPS4(path),
			ControllerSettings.Devices.PS5 => ConvertJoypadToPS5(path),
			ControllerSettings.Devices.STADIA => ConvertJoypadToStadia(path),
			ControllerSettings.Devices.STEAM => ConvertJoypadToSteam(path),
			ControllerSettings.Devices.SWITCH => ConvertJoypadToSwitch(path),
			ControllerSettings.Devices.JOYCON => ConvertJoypadToJoycon(path),
			ControllerSettings.Devices.XBOX360 => ConvertJoypadToXbox360(path),
			ControllerSettings.Devices.XBOXONE => ConvertJoypadToXboxOne(path),
			ControllerSettings.Devices.XBOXSERIES => ConvertJoypadToXboxSeries(path),
			ControllerSettings.Devices.STEAM_DECK => ConvertJoypadToSteamDeck(path),
			ControllerSettings.Devices.OUYA => ConvertJoypadToOuya(path),
			_ => "",
		};
	}

	public ControllerSettings.Devices GetJoypadType( int device, ControllerSettings.Devices fallback, ControllerSettings.Devices forceControllerIconStyle = ControllerSettings.Devices.NONE)
	{
		if( forceControllerIconStyle != ControllerSettings.Devices.NONE )
		{
			return forceControllerIconStyle;
		}

		Godot.Collections.Array<int> available = Input.GetConnectedJoypads();
		if( available.Count == 0 )
		{
			return fallback;
		}

		// If the requested joypad is not on the connected joypad list, try using the last known connected joypad
		if( !available.Contains( device ) )
			device = ControllerIcons.CI.LastController;

		// If that fails too, then use whatever joypad we have connected right now
		if( !available.Contains( device ) )
			device = available.First();

		string controllerName = Input.GetJoyName(device);
		if( controllerName.Contains("Luna Controller") )
			return ControllerSettings.Devices.LUNA;
		else if( controllerName.Contains("PS3 Controller") )
			return ControllerSettings.Devices.PS3;
		else if( controllerName.Contains("PS4 Controller") || controllerName.Contains("DUALSHOCK 4") )
			return ControllerSettings.Devices.PS4;
		else if( controllerName.Contains("PS5 Controller") || controllerName.Contains("DualSense") )
			return ControllerSettings.Devices.PS5;
		else if( controllerName.Contains("Stadia Controller") )
			return ControllerSettings.Devices.STADIA;
		else if( controllerName.Contains("Steam Controller") )
			return ControllerSettings.Devices.STEAM;
		else if( controllerName.Contains("Switch Controller") || controllerName.Contains("Switch Pro Controller") )
			return ControllerSettings.Devices.SWITCH;
		else if( controllerName.Contains("Joy-Con") )
			return ControllerSettings.Devices.JOYCON;
		else if( controllerName.Contains("Xbox 360 Controller") )
			return ControllerSettings.Devices.XBOX360;
		else if( controllerName.Contains("Xbox One") || controllerName.Contains("X-Box One") || controllerName.Contains("Xbox Wireless Controller") )
			return ControllerSettings.Devices.XBOXONE;
		else if( controllerName.Contains("Xbox Series") )
			return ControllerSettings.Devices.XBOXSERIES;
		else if( controllerName.Contains("Steam Deck") || controllerName.Contains("Steam Virtual Gamepad") )
			return ControllerSettings.Devices.STEAM_DECK;
		else if( controllerName.Contains("OUYA Controller") )
			return ControllerSettings.Devices.OUYA;
		else
			return fallback;
	}


	public string ConvertJoypadToLuna( string path )
	{
		path = path.Replace("joypad", "luna");
		return path.Substring(path.Find("/") + 1) switch
		{
			"select" => path.Replace("/select", "/circle"),
			"start" => path.Replace("/start", "/menu"),
			"share" => path.Replace("/share", "/microphone"),
			_ => path,
		};
	}

	public string ConvertJoypadToPlaystation( string path )
	{
		return path.Substring(path.Find("/") + 1) switch
		{
			"a" => path.Replace("/a", "/cross"),
			"b" => path.Replace("/b", "/circle"),
			"x" => path.Replace("/x", "/square"),
			"y" => path.Replace("/y", "/triangle"),
			"lb" => path.Replace("/lb", "/l1"),
			"rb" => path.Replace("/rb", "/r1"),
			"lt" => path.Replace("/lt", "/l2"),
			"rt" => path.Replace("/rt", "/r2"),
			_ => path,
		};
	}

	public string ConvertJoypadToPS3( string path )
	{
		return ConvertJoypadToPlaystation( path.Replace("joypad", "ps3") );
	}

	public string ConvertJoypadToPS4( string path )
	{
		path = ConvertJoypadToPlaystation(path.Replace("joypad", "ps4"));
		return path.Substring(path.Find("/") + 1) switch
		{
			"select" => path.Replace("/select", "/share"),
			"start" => path.Replace("/start", "/options"),
			"share" => path.Replace("/share", "/"),
			_ => path,
		};
	}

	public string ConvertJoypadToPS5(string path)
	{
		path = ConvertJoypadToPlaystation(path.Replace("joypad", "ps5"));
		return path.Substring(path.Find("/") + 1) switch
		{
			"select" => path.Replace("/select", "/share"),
			"start" => path.Replace("/start", "/options"),
			"home" => path.Replace("/home", "/assistant"),
			"share" => path.Replace("/share", "/microphone"),
			_ => path,
		};
	}

	public string ConvertJoypadToStadia( string path )
	{
		path = path.Replace("joypad", "stadia");
		return path.Substring(path.Find("/") + 1) switch
		{
			"lb" => path.Replace("/lb", "/l1"),
			"rb" => path.Replace("/rb", "/r1"),
			"lt" => path.Replace("/lt", "/l2"),
			"rt" => path.Replace("/rt", "/r2"),
			"select" => path.Replace("/select", "/dots"),
			"start" => path.Replace("/start", "/menu"),
			"share" => path.Replace("/share", "/select"),
			_ => path,
		};
	}


	public string ConvertJoypadToSteam( string path )
	{
		path = path.Replace("joypad", "steam");
		return path.Substring(path.Find("/") + 1) switch
		{
			"r_stick_click" => path.Replace("/r_stick_click", "/right_track_center"),
			"select" => path.Replace("/select", "/back"),
			"home" => path.Replace("/home", "/system"),
			"dpad" => path.Replace("/dpad", "/left_track"),
			"dpad_up" => path.Replace("/dpad_up", "/left_track_up"),
			"dpad_down" => path.Replace("/dpad_down", "/left_track_down"),
			"dpad_left" => path.Replace("/dpad_left", "/left_track_left"),
			"dpad_right" => path.Replace("/dpad_right", "/left_track_right"),
			"l_stick" => path.Replace("/l_stick", "/stick"),
			"r_stick" => path.Replace("/r_stick", "/right_track"),
			_ => path,
		};
	}


	public string ConvertJoypadToSwitch( string path )
	{
		path = path.Replace("joypad", "switch");
		return path.Substring(path.Find("/") + 1) switch
		{
			"a" => path.Replace("/a", "/b"),
			"b" => path.Replace("/b", "/a"),
			"x" => path.Replace("/x", "/y"),
			"y" => path.Replace("/y", "/x"),
			"lb" => path.Replace("/lb", "/l"),
			"rb" => path.Replace("/rb", "/r"),
			"lt" => path.Replace("/lt", "/zl"),
			"rt" => path.Replace("/rt", "/zr"),
			"select" => path.Replace("/select", "/minus"),
			"start" => path.Replace("/start", "/plus"),
			"share" => path.Replace("/share", "/square"),
			_ => path,
		};
	}

	public string ConvertJoypadToJoycon( string path )
	{
		path = ConvertJoypadToSwitch(path);
		return path.Substring(path.Find("/") + 1) switch
		{
			"dpad_up" => path.Replace("/dpad_up", "/up"),
			"dpad_down" => path.Replace("/dpad_down", "/down"),
			"dpad_left" => path.Replace("/dpad_left", "/left"),
			"dpad_right" => path.Replace("/dpad_right", "/right"),
			_ => path,
		};
	}


	public string ConvertJoypadToXbox360( string path )
	{
		path = path.Replace("joypad", "xbox360");
		return path.Substring(path.Find("/") + 1) switch
		{
			"select" => path.Replace("/select", "/back"),
			_ => path,
		};
	}

	public string ConvertJoypadToXboxModern(string path)
	{
		return path.Substring(path.Find("/") + 1) switch
		{
			"select" => path.Replace("/select", "/view"),
			"start" => path.Replace("/start", "/menu"),
			_ => path,
		};
	}

	public string ConvertJoypadToXboxOne(string path)
	{
		return ConvertJoypadToXboxModern(path.Replace("joypad", "xboxone"));
	}

	public string ConvertJoypadToXboxSeries(string path)
	{
		return ConvertJoypadToXboxModern(path.Replace("joypad", "xboxseries"));
	}

	public string ConvertJoypadToSteamDeck(string path)
	{
		path = path.Replace("joypad", "steamdeck");
		return path.Substring(path.Find("/") + 1) switch
		{
			"lb" => path.Replace("/lb", "/l1"),
			"rb" => path.Replace("/rb", "/r1"),
			"lt" => path.Replace("/lt", "/l2"),
			"rt" => path.Replace("/rt", "/r2"),
			"select" => path.Replace("/select", "/inventory"),
			"start" => path.Replace("/start", "/menu"),
			"home" => path.Replace("/home", "/steam"),
			"share" => path.Replace("/share", "/dots"),
			_ => path,
		};
	}

	public string ConvertJoypadToOuya(string path)
	{
		path = path.Replace("joypad", "ouya");
		return path.Substring(path.Find("/") + 1) switch
		{
			"a" => path.Replace("/a", "/o"),
			"x" => path.Replace("/x", "/u"),
			"b" => path.Replace("/b", "/a"),
			"lb" => path.Replace("/lb", "/l1"),
			"rb" => path.Replace("/rb", "/r1"),
			"lt" => path.Replace("/lt", "/l2"),
			"rt" => path.Replace("/rt", "/r2"),
			"start" => path.Replace("/start", "/menu"),
			"share" => path.Replace("/share", "/microphone"),
			_ => path,
		};
	}
}
