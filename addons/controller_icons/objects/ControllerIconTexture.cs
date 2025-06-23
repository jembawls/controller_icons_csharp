using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static ControllerIcons;

// [Texture2D] proxy for displaying controller icons
//
// A 2D texture representing a controller icon. The underlying system provides
// a [Texture2D] that may react to changes in the current input method, and also detect the user's controller type.
// Specify the [member path] property to setup the desired icon and behavior.[br]
// [br]
// For a more technical overview, this resource functions as a proxy for any
// node that accepts a [Texture2D], redefining draw commands to use an
// underlying plain [Texture2D], which may be swapped by the remapping system.[br]
// [br]
// This resource works out-of-the box with many default nodes, such as [Sprite2D],
// [Sprite3D], [TextureRect], [RichTextLabel], and others. If you are
// integrating this resource on a custom node, you will need to connect to the
// [signal Resource.changed] signal to properly handle changes to the underlying
// texture. You might also need to force a redraw with methods such as
// [method CanvasItem.queue_redraw].
//
// @tutorial(Online documentation): https://github.com/rsubtil/controller_icons/blob/master/DOCS.md

[Tool]
[GlobalClass, Icon("res://addons/controller_icons/objects/controller_texture_icon.svg")]
public partial class ControllerIconTexture : Texture2D
{
	// A path describing the desired icon. This is a generic path that can be one
	// of three different types:
	// [br][br]
	// [b]- Input Action[/b]: Specify the exact name of an existing input action. The
	// icon will be swapped automatically depending on whether the keyboard/mouse or the
	// controller is being used. When using a controller, it also changes according to
	// the controller type.[br][br]
	// [i]This is the recommended approach, as it will handle all input methods
	// automatically, and supports any input remapping done at runtime[/i].
	// [codeblock]
	// # "Enter" on keyboard, "Cross" on Sony,
	// # "A" on Xbox, "B" on Nintendo
	// path = "ui_accept"
	// [/codeblock]
	// [b]- Joypad Path[/b]: Specify a generic joypad path resembling the layout of a
	// Xbox 360 controller, starting with the [code]joypad/[/code] prefix. The icon will only
	// display controller icons, but it will still change according to the controller type.
	// [codeblock]
	// # "Square" on Sony, "X" on Xbox, "Y" on Nintendo
	// path = "joypad/x"
	// [/codeblock]
	// [b]- Specific Path[/b]: Specify a direct asset path from the addon assets.
	// With this path type, there is no dynamic remapping, and the icon will always
	// remain the same. The path to use is the path to an icon file, minus the base
	// path and extension.
	// [codeblock]
	// # res://addons/controller_icons/assets/steam/gyro.png
	// path = "steam/gyro"
	// [/codeblock]
	[Export]
	public string path { 
		get 
		{ 
			return _path;
		}

		set 
		{
			_path = value;
			LoadTexturePath();
		} 
	}
	private string _path = "";

	// Show the icon only if a specific input method is being used. When hidden, 
	// the icon will not occupy have any space (no width and height).	
	[Export]
	public EShowMode show_mode { 
		get 
		{ 
			return _show_mode;
		}

		set 
		{
			_show_mode = value;
			LoadTexturePath();
		} 
	}
	private EShowMode _show_mode = EShowMode.ANY;

	// Forces the icon to show a specific controller style, regardless of the
	// currently used controller type.
	//[br][br]
	// This will override force_device if set to a value other than NONE.
	//[br][br]
	// This is only relevant for paths using input actions, and has no effect on
	// other scenarios.
	[Export]
	public ControllerSettings.Devices force_controller_icon_style { 
		get 
		{ 
			return _force_controller_icon_style;
		}

		set 
		{
			_force_controller_icon_style = value;
			LoadTexturePath();
		} 
	}
	private ControllerSettings.Devices _force_controller_icon_style = ControllerSettings.Devices.NONE;
	
	// Forces the icon to show either the keyboard/mouse or controller icon,
	// regardless of the currently used input method.
	//[br][br]
	// This is only relevant for paths using input actions, and has no effect on
	// other scenarios.
	[Export]
	public EInputType force_type { 
		get 
		{ 
			return _force_type;
		}

		set 
		{
			_force_type = value;
			LoadTexturePath();
		} 
	}
	private EInputType _force_type = EInputType.NONE;

	public enum EForceDevice {
		DEVICE_0,
		DEVICE_1,
		DEVICE_2,
		DEVICE_3,
		DEVICE_4,
		DEVICE_5,
		DEVICE_6,
		DEVICE_7,
		DEVICE_8,
		DEVICE_9,
		DEVICE_10,
		DEVICE_11,
		DEVICE_12,
		DEVICE_13,
		DEVICE_14,
		DEVICE_15,
		ANY // No device will be forced
	}

	// Forces the icon to use the textures for the device connected at the specified index.
	// For example, if a PlayStation 5 controller is connected at device_index 0,
	// the icon will always show PlayStation 5 textures.
	[Export]
	public EForceDevice force_device { 
		get 
		{ 
			return _force_device;
		}

		set 
		{
			_force_device = value;
			LoadTexturePath();
		} 
	}
	private EForceDevice _force_device = EForceDevice.ANY;

	[ExportSubgroup("Text Rendering")]
	// Custom LabelSettings. If set, overrides the addon's global label settings.
	[Export]
	public LabelSettings custom_label_settings { 
		get 
		{ 
			return _custom_label_settings;
		}

		set 
		{
			_custom_label_settings = value;
			LoadTexturePath();
			
			// Call _textures setter, which handles signal connections for label settings
			Textures = Textures;
		} 
	}
	private LabelSettings _custom_label_settings;

	// Returns a text representation of the displayed icon, useful for TTS
	// (text-to-speech) scenarios.
	// [br][br]
	// This takes into consideration the currently displayed icon, and will thus be
	// different if the icon is from keyboard/mouse or controller. It also takes
	// into consideration the controller type, and will thus use native button
	// names (e.g. [code]A[/code] for Xbox, [code]Cross[/code] for PlayStation, etc).
	public string GetTTSString()
	{
		if( force_type != EInputType.NONE )
			return CI.ParsePathToTTS(path, force_type - 1);
		else
			return CI.ParsePathToTTS(path);
	}

	private bool CanBeShown()
	{
		return show_mode switch
		{
			EShowMode.KEYBOARD_MOUSE => CI.LastInputType == EInputType.KEYBOARD_MOUSE,
			EShowMode.CONTROLLER => CI.LastInputType == EInputType.CONTROLLER,
			_ => true,
		};
	}

	public List<Texture2D> Textures
	{
		get
		{
			return _Textures;
		}

		set
		{
			// UPGRADE: In Godot 4.2, for-loop variables can be
			// statically typed:
			// for tex:Texture in value:
			foreach( Texture2D tex in value ) 
			{
				if( tex != null && tex.IsConnected(SignalName.Changed, Callable.From( ReloadResource )) )
					tex.Changed -= ReloadResource;
			}

			if( LabelSettings != null && LabelSettings.IsConnected(SignalName.Changed, Callable.From(OnLabelSettingsChanged)) )
				LabelSettings.Changed -= OnLabelSettingsChanged;

			_Textures = value;
			LabelSettings = null;
			if( _Textures != null && _Textures.Count > 1 )
			{
				LabelSettings = custom_label_settings;
				if( LabelSettings == null )
				{
					LabelSettings = CI.Settings.custom_label_settings;
				}

				if( LabelSettings == null )
				{
					LabelSettings = new();
				}

				LabelSettings.Changed += OnLabelSettingsChanged;
				Font = LabelSettings.Font == null ? ThemeDB.FallbackFont : LabelSettings.Font;
				OnLabelSettingsChanged();
			}
			// UPGRADE: In Godot 4.2, for-loop variables can be
			// statically typed:
			// for tex:Texture in value:
			foreach( Texture2D tex in value)
			{
				if( tex != null )
					tex.Changed += ReloadResource;
			}
		}
	}
	private List<Texture2D> _Textures = new();

	private const int _NULL_SIZE = 2;
	private Font Font;
	private LabelSettings LabelSettings;
	private Vector2 TextSize;
	
	public ControllerIconTexture()
	{
		//if plugin isn't ready ready, keep checking on draw until it is then set it up
		if( !IsControllerIconsPluginReady() )
		{
			RenderingServer.FramePostDraw += OnFramePostDraw;
		}
		else
		{
			Setup();
		}
	}

	private void Setup()
	{
		CI.InputTypeChanged += OnInputTypeChanged;
	}

	public void OnLabelSettingsChanged() 
	{		
		Font = LabelSettings.Font == null ? ThemeDB.FallbackFont : LabelSettings.Font;
		TextSize = Font.GetStringSize("+", HorizontalAlignment.Left, -1, LabelSettings.FontSize);

		ReloadResource();
	}

	private void OnFramePostDraw()
	{
		//Check if plugin is ready
		if( IsControllerIconsPluginReady() )
		{
			RenderingServer.FramePostDraw -= OnFramePostDraw;
			Setup();
			LoadTexturePath();
		}
	}

	private void ReloadResource()
	{
		Dirty = true;
		EmitChanged();
	}

	private void LoadTexturePathImpl()
	{
		List<Texture2D> textures = new();

		if( CanBeShown() )
		{
			EInputType input_type = force_type == EInputType.NONE ? CI.LastInputType : force_type;
			if( CI.GetPathType(path) == EPathType.INPUT_ACTION )
			{
				InputEvent e = CI.GetMatchingEvent(path, input_type);
				textures.AddRange(CI.ParseEventModifiers(e));				
			}
			int target_device = force_device != EForceDevice.ANY ? (int)force_device : CI.LastController;
			Texture2D tex = CI.ParsePath(path, input_type, target_device, force_controller_icon_style);
			if( tex != null )
				textures.Add(tex);
		}

		Textures = textures;
		ReloadResource();		
	}

	private void LoadTexturePath()
	{
		if( IsControllerIconsPluginReady() )
		{
			// Ensure loading only occurs on the main thread
			if( OS.GetThreadCallerId() != OS.GetMainThreadId() )
			{
				// In Godot 4.3, call_deferred no longer makes this function
				// execute on the main thread due to changes in resource loading.
				// To ensure this, we instead rely on ControllerIcons for this
				CI.DeferTextureLoad(Callable.From( LoadTexturePathImpl ));
			}
			else
			{
				LoadTexturePathImpl();
			}
		}
	}

	public void OnInputTypeChanged( int inputType, int controller )
	{
		LoadTexturePath();
	}

	public override int _GetWidth()
	{
		if( CanBeShown() )
		{
			int ret = 0;
			foreach( Texture2D texture in Textures )
			{
				if( texture != null )
				{
					ret += texture.GetWidth();
				}
			}

			if( LabelSettings != null )
			{
				ret += Mathf.RoundToInt( Math.Max(0, Textures.Count - 1) * TextSize.X );
			}

			// If ret is 0, return a size of 2 to prevent triggering engine checks
			// for null sizes. The correct size will be set at a later frame.
			return ret > 0 ? ret : _NULL_SIZE;
		}

		return _NULL_SIZE;
	}

	public override int _GetHeight()
	{
		if( CanBeShown() )
		{
			int ret = 0;
			foreach( Texture2D texture in Textures )
			{
				if( texture != null )
				{
					ret = Mathf.RoundToInt( Math.Max( ret, texture.GetHeight() ) );
				}
			}

			if( LabelSettings != null && Textures.Count > 1 )
			{
				ret = Mathf.RoundToInt( Math.Max(ret, TextSize.Y) );
			}

			// If ret is 0, return a size of 2 to prevent triggering engine checks
			// for null sizes. The correct size will be set at a later frame.
			return ret > 0 ? ret : _NULL_SIZE;
		}

		return _NULL_SIZE;
	}

	public override bool _HasAlpha()
	{
		return Textures.Any(t => t.HasAlpha());
	}

	public override bool _IsPixelOpaque( int x, int y)
	{
		// TODO: Not exposed to GDScript; however, since this seems to be used for editor stuff, it's
		// seemingly fine to just report all pixels as opaque. Otherwise, mouse picking for Sprite2D
		// stops working.
		return true;
	}

	public override void _Draw( Rid toCanvasItem, Vector2 pos, Color modulate, bool transpose)
	{
		Vector2 position = pos;

		for (int i = 0; i < Textures.Count; ++i)
		{
			Texture2D tex = Textures[i];

			if( tex == null ) continue;

			if( i != 0 )
			{
				// Draw text char '+'
				Vector2 font_position = new Vector2(
					position.X,
					position.Y + (GetHeight() - TextSize.Y) / 2.0f
				);

				DrawText(toCanvasItem, font_position, "+");
			}

			position += new Vector2(TextSize.X, 0);
			tex.Draw(toCanvasItem, position, modulate, transpose);
			position += new Vector2( tex.GetWidth(), 0 );
		}
	}

	public override void _DrawRect( Rid toCanvasItem, Rect2 rect, bool tile, Color modulate, bool transpose )
	{
		Vector2 position = rect.Position;
		float widthRatio = rect.Size.X / _GetWidth();
		float heightRatio = rect.Size.Y / _GetHeight();

		for (int i = 0; i < Textures.Count; ++i )
		{
			Texture2D tex = Textures[i];

			if( tex == null) continue;

			if( i != 0 )
			{
				// Draw text char '+'
				Vector2 fontPosition = new Vector2(
					position.X + (TextSize.X * widthRatio) / 2 - (TextSize.X / 2),
					position.Y + (rect.Size.Y - TextSize.Y) / 2.0f
				);
				DrawText(toCanvasItem, fontPosition, "+");
				position += new Vector2( TextSize.X * widthRatio, 0 );
			}

			Vector2 size = tex.GetSize() * new Vector2(widthRatio, heightRatio);
			tex.DrawRect(toCanvasItem, new Rect2(position, size), tile, modulate, transpose);
			position += new Vector2( size.X, 0 );
		}
	}

	public override void _DrawRectRegion( Rid toCanvasItem, Rect2 rect, Rect2 srcRect, Color modulate, bool transpose, bool clipUV)
	{
		Vector2 position = rect.Position;
		float widthRatio = rect.Size.X / _GetWidth();
		float heightRatio = rect.Size.Y / _GetHeight();

		for (int i = 0; i < Textures.Count; ++i )
		{
			Texture2D tex = Textures[i];

			if( tex == null) continue;

			if( i != 0 )
			{
				// Draw text char '+'
				Vector2 fontPosition = new(
					position.X + (TextSize.X * widthRatio) / 2 - (TextSize.X / 2),
					position.Y + (rect.Size.Y - TextSize.Y) / 2.0f
				);
				DrawText(toCanvasItem, fontPosition, "+");

				position += new Vector2(TextSize.X * widthRatio, 0);
			} 

			Vector2 size = tex.GetSize() * new Vector2(widthRatio, heightRatio);

			Vector2 srcRectRatio = new(
				tex.GetWidth() / (float)_GetWidth(),
				tex.GetHeight() / (float)_GetHeight()
			);
			Rect2 texSrcRect = new(
				srcRect.Position * srcRectRatio,
				srcRect.Size * srcRectRatio
			);

			tex.DrawRectRegion(toCanvasItem, new Rect2(position, size), texSrcRect, modulate, transpose, clipUV);
			position += new Vector2( size.X, 0 );
		}
	}

	private void DrawText( Rid toCanvasItem, Vector2 fontPosition, string text )
	{
		fontPosition += new Vector2(0, Font.GetAscent(LabelSettings.FontSize));

		if( LabelSettings.ShadowColor.A > 0 )
		{
			Font.DrawString(toCanvasItem, fontPosition + LabelSettings.ShadowOffset, text, HorizontalAlignment.Left, -1, LabelSettings.FontSize, LabelSettings.ShadowColor);
			if( LabelSettings.ShadowSize > 0 )
				Font.DrawStringOutline(toCanvasItem, fontPosition + LabelSettings.ShadowOffset, text, HorizontalAlignment.Left, -1, LabelSettings.FontSize, LabelSettings.ShadowSize, LabelSettings.ShadowColor);
		}
		if( LabelSettings.OutlineColor.A > 0 && LabelSettings.OutlineSize > 0 )
		{
			Font.DrawStringOutline(toCanvasItem, fontPosition, text, HorizontalAlignment.Left, -1, LabelSettings.FontSize, LabelSettings.OutlineSize, LabelSettings.OutlineColor);
		}

		Font.DrawString(toCanvasItem, fontPosition, text, HorizontalAlignment.Center, -1, LabelSettings.FontSize, LabelSettings.FontColor);
	}

	private SubViewport HelperViewport;
	private bool IsStitchingTexture = false;
	private async void StitchTexture()
	{
		if( Textures.Count == 0 )
			return;

		IsStitchingTexture = true;

		Image fontImage = null;
		if( Textures.Count > 1 )
		{
			// Generate a viewport to draw the text
			HelperViewport = new SubViewport
			{
				// FIXME: We need a 3px margin for some reason
				Size = (Vector2I)(TextSize + new Vector2(3, 0)),

				RenderTargetUpdateMode = SubViewport.UpdateMode.Once,
				RenderTargetClearMode = SubViewport.ClearMode.Once,
				TransparentBg = true
			};

			Label label = new()
			{
				LabelSettings = LabelSettings,
				Text = "+",

				Position = Vector2.Zero
			};

			HelperViewport.AddChild(label);

			CI.AddChild(HelperViewport);
			//await RenderingServer.FramePostDraw;
			await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
			fontImage = HelperViewport.GetTexture().GetImage();

			CI.RemoveChild(HelperViewport);
			HelperViewport.Free();
		}

		Vector2I position = new(0, 0);

		Image img = new();
		for (int i = 0; i < Textures.Count; ++i )
		{
			if( Textures[i] == null ) continue;

			if( i != 0 )
			{
				// Draw text char '+'
				Rect2I region = fontImage.GetUsedRect();
				Vector2I fontPosition = new(
					position.X,
					position.Y + (GetHeight() - region.Size.Y) / 2
				);

				img.BlitRect( fontImage, region, fontPosition );
				position += new Vector2I( region.Size.X, 0 );
			}

			Image textureRaw = Textures[i].GetImage();
			textureRaw.Decompress();
		#if GODOT4_3_OR_GREATER
			img ??= Image.CreateEmpty(_GetWidth(), _GetHeight(), true, textureRaw.GetFormat());
		#else
			img ??= Image.Create(_GetWidth(), _GetHeight(), true, textureRaw.GetFormat());
		#endif

			img.BlitRect(textureRaw, new Rect2I(0, 0, textureRaw.GetWidth(), textureRaw.GetHeight()), position);

			position += new Vector2I( textureRaw.GetWidth(), 0 );
		}

		IsStitchingTexture = false;

		Dirty = false;
		Texture3D = ImageTexture.CreateFromImage(img);
		EmitChanged();
	}

	// This is necessary for 3D sprites, as the texture is assigned to a material, and not drawn directly.
	// For multi prompts, we need to generate a texture
	private bool Dirty = true;

	private Texture Texture3D;
	public override Rid _GetRid()
	{
		if( Dirty )
		{
			if( !IsStitchingTexture )
				// FIXME: Function may await, but because this is an internal engine call, we can't do anything about it.
				// This results in a one-frame white texture being displayed, which is not ideal. Investigate later.
				StitchTexture();
				
			if( IsStitchingTexture )
				return new Rid(null);

			else
			{
				return new Rid(null);
			}
				
		}
		return Textures.Count > 0 ? Texture3D.GetRid() : new Rid(null);
	}
}
