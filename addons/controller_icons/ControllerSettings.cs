using Godot;
using System;

[Tool]
public partial class ControllerSettings : Resource
{
	public enum Devices
	{
		NONE = -1,
		LUNA,
		OUYA,
		PS3,
		PS4,
		PS5,
		STADIA,
		STEAM,
		SWITCH,
		JOYCON,
		XBOX360,
		XBOXONE,
		XBOXSERIES,
		STEAM_DECK
	}

	// General addon settings
	[ExportSubgroup("General")]

	// Controller type to fallback to if automatic
	// controller detection fails
	[Export]
	public Devices joypad_fallback = Devices.XBOX360;

	// Controller deadzone for triggering an icon remap when input
	// is analogic (movement sticks or triggers)
	[Export(PropertyHint.Range, "0.0,1.0")]
	public float joypad_deadzone = 0.5f;

	// Allow mouse movement to trigger an icon remap
	[Export]
	public bool allow_mouse_remap = true;

	// Minimum mouse "instantaneous" movement for
	// triggering an icon remap
	[Export(PropertyHint.Range, "0,10000")]
	public int mouse_min_movement = 200;

	// Settings related to advanced custom assets usage and remapping
	[ExportSubgroup("Custom assets")]

	// Custom asset lookup folder for custom icons
	[Export]
	public string custom_asset_dir = "";

	// Custom generic joystick mapper script
	[Export]
	private Script custom_mapper;

	// Custom icon file extension
	[Export]
	public string custom_file_extension = "";

	// Custom settings related to any text rendering required on prompts
	[ExportSubgroup("Text Rendering")]

	// Custom LabelSettings. If unset, uses engine default settings.
	[Export]
	public LabelSettings custom_label_settings;
}
