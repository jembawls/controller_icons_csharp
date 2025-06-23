// Controller icon for TextureRect nodes.
//
// [b]Deprecated[/b]: Use the new [ControllerIconTexture] texture resource and set it
// directly in [member TextureRect.texture].
//
// @deprecated

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static ControllerIcons;

[Tool]
public partial class ControllerTextureRect : TextureRect
{
	public override string[] _GetConfigurationWarnings()
	{
		return new string[] { "This node is deprecated, and will be removed in a future version.\n\nRemove this script and use the new ControllerIconTexture resource\nby setting it directly in TextureRect's texture property." };
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
			if( IsInsideTree() && IsControllerIconsPluginReady() )
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
			
			if( IsControllerIconsPluginReady() )
				OnInputTypeChanged((int)CI.LastInputType, CI.LastController);
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

			if( IsControllerIconsPluginReady() )
				OnInputTypeChanged((int)CI.LastInputType, CI.LastController);
		}
	}
	private EInputType _force_type = EInputType.NONE;

	[Export]
	public int max_width
	{
		get
		{
			return _max_width;
		}

		set
		{
			_max_width = value;
			if( IsInsideTree() )
			{
				if( _max_width < 0 )
					ExpandMode = ExpandModeEnum.KeepSize;
				else
				{
					ExpandMode = ExpandModeEnum.IgnoreSize;
					CustomMinimumSize = new Vector2( _max_width, CustomMinimumSize.Y );
					if( Texture != null )
						CustomMinimumSize = new Vector2( CustomMinimumSize.X, Texture.GetHeight() * _max_width / Texture.GetWidth() );
					else
						CustomMinimumSize = new Vector2( CustomMinimumSize.X, CustomMinimumSize.X );
				}


			}
		}
	}
	private int _max_width = 40;

	public override void _Ready()
	{
		if( IsControllerIconsPluginReady() )
		{
			Setup();
		}
		else
		{
			RenderingServer.FramePostDraw += OnFramePostDraw;
		}
	}

	private void Setup()
	{
		CI.InputTypeChanged += OnInputTypeChanged;
		this.path = path;
		this.max_width = max_width;
	}

	private void OnFramePostDraw()
	{
		if( IsControllerIconsPluginReady() )
		{
			RenderingServer.FramePostDraw -= OnFramePostDraw;
			Setup();
		}
	}

	public void OnInputTypeChanged( int inputType, int controller )
	{
		if( show_only == EShowMode.ANY ||
			(show_only == EShowMode.KEYBOARD_MOUSE && (EInputType)inputType == EInputType.KEYBOARD_MOUSE) ||
			(show_only == EShowMode.CONTROLLER && (EInputType)inputType == EInputType.CONTROLLER))
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
