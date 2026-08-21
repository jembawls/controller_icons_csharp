
using Godot;
using System;
using System.Collections.Generic;
using static ControllerIcons;

public partial class SelectorPanel : Panel
{
	private const int Godot46Hex = 0x040600;

	public new virtual void GrabFocus( bool hideFocus = false )
	{
		//override me
	}

	protected static void GrabFocusOn( Control control, bool hideFocus = false )
	{
		// UPGRADE: In Godot 4.6, grab_focus has a new internal arg
		if( Engine.GetVersionInfo()["hex"].AsInt32() >= Godot46Hex )
			control.Call( "grab_focus", hideFocus );
		else
			control.GrabFocus();
	}
}