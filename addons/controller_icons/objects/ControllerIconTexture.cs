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
            _load_texture_path();
		} 
	}
    private string _path = "";

    // Show the icon only if a specific input method is being used. When hidden, 
    // the icon will not occupy have any space (no width and height).	
    [Export]
    public ShowMode show_mode { 
		get 
		{ 
			return _show_mode; 
		}

        set 
		{
			_show_mode = value;
            _load_texture_path();
		} 
	}
    private ShowMode _show_mode = ShowMode.ANY;

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
            _load_texture_path();
		} 
	}
    private ControllerSettings.Devices _force_controller_icon_style = ControllerSettings.Devices.NONE;
    
    // Forces the icon to show either the keyboard/mouse or controller icon,
    // regardless of the currently used input method.
    //[br][br]
    // This is only relevant for paths using input actions, and has no effect on
    // other scenarios.
    [Export]
    public InputType force_type { 
		get 
		{ 
			return _force_type; 
		}

        set 
		{
			_force_type = value;
            _load_texture_path();
		} 
	}
    private InputType _force_type = InputType.NONE;

    public enum ForceDevice {
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
    public ForceDevice force_device { 
		get 
		{ 
			return _force_device; 
		}

        set 
		{
			_force_device = value;
            _load_texture_path();
		} 
	}
    private ForceDevice _force_device = ForceDevice.ANY;

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
            _load_texture_path();
			
			// Call _textures setter, which handles signal connections for label settings
            _textures = _textures;
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
    public string get_tts_string()
	{
		if( force_type != InputType.NONE )
			return CI.parse_path_to_tts(path, force_type - 1);
        else
			return CI.parse_path_to_tts(path);
    }

    private bool _can_be_shown()
	{
        switch( show_mode )
		{
			case ShowMode.KEYBOARD_MOUSE:
                return CI._last_input_type == InputType.KEYBOARD_MOUSE;
            case ShowMode.CONTROLLER:
                return CI._last_input_type == InputType.CONTROLLER;
            case ShowMode.ANY:
			default:
        		return true;
		}
    }

	public List<Texture2D> _textures
	{
		get
		{
            return __textures;
        }

		set
		{
			// UPGRADE: In Godot 4.2, for-loop variables can be
			// statically typed:
			// for tex:Texture in value:
			foreach( Texture2D tex in value )
			{
				if( tex != null && tex.IsConnected(SignalName.Changed, Callable.From( _reload_resource )) )
					tex.Changed -= _reload_resource;
            }

			if( _label_settings != null && _label_settings.IsConnected(SignalName.Changed, Callable.From(_on_label_settings_changed)) )
                _label_settings.Changed -= _on_label_settings_changed;

            __textures = value;
            _label_settings = null;
            if( __textures != null && __textures.Count > 1 )
			{
                _label_settings = custom_label_settings;
                if( _label_settings == null )
				{
                    _label_settings = CI._settings.custom_label_settings;
                }

				if( _label_settings == null )
				{
                    _label_settings = new();
                }

                _label_settings.Changed += _on_label_settings_changed;
                _font = _label_settings.Font == null ? ThemeDB.FallbackFont : _label_settings.Font;
                _on_label_settings_changed();
            }
			// UPGRADE: In Godot 4.2, for-loop variables can be
			// statically typed:
			// for tex:Texture in value:
			foreach( Texture2D tex in value)
			{
				if( tex != null )
                    tex.Changed += _reload_resource;
            }
		}
	}
    private List<Texture2D> __textures = [];

    private const int _NULL_SIZE = 2;
    private Font _font;
    private LabelSettings _label_settings;
    private Vector2 _text_size;
	
    public ControllerIconTexture()
	{
        CI.InputTypeChanged += _on_input_type_changed;
    }

    public void _on_label_settings_changed()
	{		
		_font = _label_settings.Font == null ? ThemeDB.FallbackFont : _label_settings.Font;
        _text_size = _font.GetStringSize("+", HorizontalAlignment.Left, -1, _label_settings.FontSize);

        _reload_resource();
    }

    private void _reload_resource()
	{
        _dirty = true;
        EmitChanged();
    }

    private void _load_texture_path_impl()
	{
        List<Texture2D> textures = [];

        if( _can_be_shown() )
		{
            InputType input_type = force_type == InputType.NONE ? CI._last_input_type : force_type;
            if( CI.get_path_type(path) == PathType.INPUT_ACTION )
			{
                InputEvent e = CI.get_matching_event(path, input_type);
                textures.AddRange(CI.parse_event_modifiers(e));				
			}
            int target_device = force_device != ForceDevice.ANY ? (int)force_device : CI._last_controller;
            Texture2D tex = CI.parse_path(path, input_type, target_device, force_controller_icon_style);
			if( tex != null )
				textures.Add(tex);
		}

        _textures = textures;
        _reload_resource();		
    }

    private void _load_texture_path()
	{
		// Ensure loading only occurs on the main thread
		if( OS.GetThreadCallerId() != OS.GetMainThreadId() )
		{
            // In Godot 4.3, call_deferred no longer makes this function
            // execute on the main thread due to changes in resource loading.
            // To ensure this, we instead rely on ControllerIcons for this
            CI._defer_texture_load(Callable.From( _load_texture_path_impl ));
        }
		else
		{
            _load_texture_path_impl();
        }
	}

    public void _on_input_type_changed( InputType input_type, int controller )
	{
        _load_texture_path();
    }

    public override int _GetWidth()
	{
		if( _can_be_shown() )
		{
			int ret = 0;
			foreach( Texture2D texture in _textures )
			{
				if( texture != null )
				{
                    ret += texture.GetWidth();
                }
			}

            if( _label_settings != null )
			{
                ret += Mathf.RoundToInt( Math.Max(0, _textures.Count - 1) * _text_size.X );
            }

			// If ret is 0, return a size of 2 to prevent triggering engine checks
			// for null sizes. The correct size will be set at a later frame.
			return ret > 0 ? ret : _NULL_SIZE;
        }

        return _NULL_SIZE;
    }

    public override int _GetHeight()
	{
		if( _can_be_shown() )
		{
			int ret = 0;
			foreach( Texture2D texture in _textures )
			{
				if( texture != null )
				{
                    ret += texture.GetHeight();
                }
			}

            if( _label_settings != null )
			{
                ret += Mathf.RoundToInt( Math.Max(0, _textures.Count - 1) * _text_size.Y );
            }

			// If ret is 0, return a size of 2 to prevent triggering engine checks
			// for null sizes. The correct size will be set at a later frame.
			return ret > 0 ? ret : _NULL_SIZE;
		}

        return _NULL_SIZE;
    }

    public override bool _HasAlpha()
	{
        return _textures.Any(t => t.HasAlpha());
    }

	public override bool _IsPixelOpaque( int x, int y)
	{
        // TODO: Not exposed to GDScript; however, since this seems to be used for editor stuff, it's
        // seemingly fine to just report all pixels as opaque. Otherwise, mouse picking for Sprite2D
        // stops working.
        return true;
    }

	public override void _Draw( Rid to_canvas_item, Vector2 pos, Color modulate, bool transpose)
	{
        Vector2 position = pos;

        for (int i = 0; i < _textures.Count; ++i)
		{
            Texture2D tex = _textures[i];

            if( tex == null ) continue;

            if( i != 0 )
			{
                // Draw text char '+'
                Vector2 font_position = new Vector2(
                    position.X,
                    position.Y + (GetHeight() - _text_size.Y) / 2.0f
                );

                _draw_text(to_canvas_item, font_position, "+");
			}

            position += new Vector2(_text_size.X, 0);
            tex.Draw(to_canvas_item, position, modulate, transpose);
            position += new Vector2( tex.GetWidth(), 0 );
        }
	}

    public override void _DrawRect( Rid to_canvas_item, Rect2 rect, bool tile, Color modulate, bool transpose )
	{
        Vector2 position = rect.Position;
        float width_ratio = rect.Size.X / _GetWidth();
        float height_ratio = rect.Size.Y / _GetHeight();

        for (int i = 0; i < _textures.Count; ++i )
		{
            Texture2D tex = _textures[i];

            if( tex == null) continue;

            if( i != 0 )
			{
                // Draw text char '+'
                Vector2 font_position = new Vector2(
                    position.X + (_text_size.X * width_ratio) / 2 - (_text_size.X / 2),
                    position.Y + (rect.Size.Y - _text_size.Y) / 2.0f
                );
                _draw_text(to_canvas_item, font_position, "+");
                position += new Vector2( _text_size.X * width_ratio, 0 );
            }

            Vector2 size = tex.GetSize() * new Vector2(width_ratio, height_ratio);
            tex.DrawRect(to_canvas_item, new Rect2(position, size), tile, modulate, transpose);
            position += new Vector2( size.X, 0 );
		}
    }

    public override void _DrawRectRegion( Rid to_canvas_item, Rect2 rect, Rect2 src_rect, Color modulate, bool transpose, bool clip_uv)
	{
        Vector2 position = rect.Position;
        float width_ratio = rect.Size.X / _GetWidth();
        float height_ratio = rect.Size.Y / _GetHeight();

        for (int i = 0; i < _textures.Count; ++i )
		{
            Texture2D tex = _textures[i];

            if( tex == null) continue;

            if( i != 0 )
			{
                // Draw text char '+'
                Vector2 font_position = new Vector2(
                    position.X + (_text_size.X * width_ratio) / 2 - (_text_size.X / 2),
                    position.Y + (rect.Size.Y - _text_size.Y) / 2.0f
                );
                _draw_text(to_canvas_item, font_position, "+");

                position += new Vector2(_text_size.X * width_ratio, 0);
            }


            Vector2 size = tex.GetSize() * new Vector2(width_ratio, height_ratio);

            Vector2 src_rect_ratio = new Vector2(
                tex.GetWidth() / (float)_GetWidth(),
                tex.GetHeight() / (float)_GetHeight()
            );
            Rect2 tex_src_rect = new Rect2(
                src_rect.Position * src_rect_ratio,
                src_rect.Size * src_rect_ratio
            );

            tex.DrawRectRegion(to_canvas_item, new Rect2(position, size), tex_src_rect, modulate, transpose, clip_uv);
            position += new Vector2( size.X, 0 );
		}
    }

    private void _draw_text( Rid to_canvas_item, Vector2 font_position, string text )
	{
        font_position += new Vector2(0, _font.GetAscent(_label_settings.FontSize));

        if( _label_settings.ShadowColor.A > 0 )
		{
            _font.DrawString(to_canvas_item, font_position + _label_settings.ShadowOffset, text, HorizontalAlignment.Left, -1, _label_settings.FontSize, _label_settings.ShadowColor);
            if( _label_settings.ShadowSize > 0 )
				_font.DrawStringOutline(to_canvas_item, font_position + _label_settings.ShadowOffset, text, HorizontalAlignment.Left, -1, _label_settings.FontSize, _label_settings.ShadowSize, _label_settings.ShadowColor);
        }
		if( _label_settings.OutlineColor.A > 0 && _label_settings.OutlineSize > 0 )
		{
            _font.DrawStringOutline(to_canvas_item, font_position, text, HorizontalAlignment.Left, -1, _label_settings.FontSize, _label_settings.OutlineSize, _label_settings.OutlineColor);
        }

        _font.DrawString(to_canvas_item, font_position, text, HorizontalAlignment.Center, -1, _label_settings.FontSize, _label_settings.FontColor);
    }

    private SubViewport _helper_viewport;
    private bool _is_stitching_texture = false;
    private async void _stitch_texture()
	{
		if( _textures.Count == 0 )
            return;

        _is_stitching_texture = true;

        Image font_image = null;
        if( _textures.Count > 1 )
		{
            // Generate a viewport to draw the text
            _helper_viewport = new SubViewport();

            // FIXME: We need a 3px margin for some reason
            _helper_viewport.Size = (Vector2I)(_text_size + new Vector2(3, 0));

            _helper_viewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Once;
            _helper_viewport.RenderTargetClearMode = SubViewport.ClearMode.Once;
            _helper_viewport.TransparentBg = true;

            Label label = new();
            label.LabelSettings = _label_settings;
            label.Text = "+";

            label.Position = Vector2.Zero;

            _helper_viewport.AddChild(label);

            CI.AddChild(_helper_viewport);
            //await RenderingServer.FramePostDraw;
            await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            font_image = _helper_viewport.GetTexture().GetImage();

            CI.RemoveChild(_helper_viewport);
            _helper_viewport.Free();
        }

        Vector2I position = new Vector2I(0, 0);

        Image img = new Image();
        for (int i = 0; i < _textures.Count; ++i )
		{
			if( _textures[i] == null ) continue;

            if( i != 0 )
			{
				// Draw text char '+'
				Rect2I region = font_image.GetUsedRect();
                Vector2I font_position = new Vector2I(
                    position.X,
                    position.Y + (GetHeight() - region.Size.Y) / 2
                );

                img.BlitRect( font_image, region, font_position );
                position += new Vector2I( region.Size.X, 0 );
            }

            Image texture_raw = _textures[i].GetImage();
            texture_raw.Decompress();
            if( img == null )
			{
            	img = Image.CreateEmpty(_GetWidth(), _GetHeight(), true, texture_raw.GetFormat());
			}

            img.BlitRect(texture_raw, new Rect2I(0, 0, texture_raw.GetWidth(), texture_raw.GetHeight()), position);

            position += new Vector2I( texture_raw.GetWidth(), 0 );
        }

        _is_stitching_texture = false;

        _dirty = false;
        _texture_3d = ImageTexture.CreateFromImage(img);
        EmitChanged();
    }

    // This is necessary for 3D sprites, as the texture is assigned to a material, and not drawn directly.
    // For multi prompts, we need to generate a texture
    private bool _dirty = true;

    private Texture _texture_3d;
    public override Rid _GetRid()
	{
		if( _dirty )
		{
			if( !_is_stitching_texture )
				// FIXME: Function may await, but because this is an internal engine call, we can't do anything about it.
				// This results in a one-frame white texture being displayed, which is not ideal. Investigate later.
				_stitch_texture();
				
            if( _is_stitching_texture )
                return new Rid(null);

            else
			{
                return new Rid(null);
			}
				
        }
		return _textures.Count > 0 ? _texture_3d.GetRid() : new Rid(null);
    }
}
