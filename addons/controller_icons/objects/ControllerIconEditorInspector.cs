
using Godot;
using System;
using System.Collections.Generic;
using static ControllerIcons;

[Tool]
public partial class ControllerIconEditorInspector : EditorInspectorPlugin
{
    public EditorInterface editor_interface;
	private ControllerIcons_TexturePreview preview;

	class ControllerIcons_TexturePreview
	{
        private MarginContainer n_root;
        private TextureRect n_background;
        private TextureRect n_texture;
        private Texture2D background;

        public Texture2D texture
		{
            get { return _texture;  }
			set
			{
                _texture = value;
                n_texture.Texture = _texture;
            }
        }
        private Texture2D _texture;

        public ControllerIcons_TexturePreview(EditorInterface editor_interface)
		{
			n_root = new();

			// UPGRADE: In Godot 4.2, there's no need to have an instance to
			// EditorInterface, since it's now a static call:
			// background = EditorInterface.get_base_control().get_theme_icon("Checkerboard", "EditorIcons")
			background = editor_interface.GetBaseControl().GetThemeIcon("Checkerboard", "EditorIcons");

			n_background = new ();
            n_background.StretchMode = TextureRect.StretchModeEnum.Tile;
            n_background.Texture = background;

            n_background.TextureRepeat = CanvasItem.TextureRepeatEnum.Enabled;
            n_background.CustomMinimumSize = new Vector2(0, 256);

			n_root.AddChild(n_background);

			n_texture = new();
			n_texture.TextureFilter = CanvasItem.TextureFilterEnum.NearestWithMipmaps;
			n_texture.SetAnchorsPreset(Control.LayoutPreset.FullRect);
			n_texture.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;

			n_texture.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;

			n_root.AddChild(n_texture);
		}

		public Control get_root()
		{
			return n_root;
		}
    }

	public override bool _CanHandle( GodotObject obj )
	{
        return obj is ControllerIconTexture;
    }

	public override void _ParseBegin( GodotObject obj )
	{
        preview = new(editor_interface);
        AddCustomControl(preview.get_root());

        if( obj is ControllerIconTexture icon )
			preview.texture = icon;
    }

	public override bool _ParseProperty( GodotObject obj, Variant.Type type, string name, PropertyHint hint_type, string hint_string, PropertyUsageFlags usage_flags, bool wide )
	{
		if( name == "path" )
		{
            ControllerIconPathEditorProperty path_selector_instance = new( editor_interface );
			AddPropertyEditor(name, path_selector_instance);
			return true;
		}
		return false;
	}

}
