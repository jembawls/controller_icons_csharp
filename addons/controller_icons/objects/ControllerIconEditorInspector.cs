#if TOOLS
using Godot;
using System;
using System.Collections.Generic;
using static ControllerIcons;

[Tool]
public partial class ControllerIconEditorInspector : EditorInspectorPlugin
{
	private ControllerIcons_TexturePreview preview;

	class ControllerIcons_TexturePreview
	{
		private MarginContainer nRoot;
		private TextureRect nBackground;
		private TextureRect nIconTexture;
		private Texture2D BackgroundTexture;

		public Texture2D Texture
		{
			get { return _Texture;  }
			set
			{
				_Texture = value;
				nIconTexture.Texture = _Texture;
			}
		}
		private Texture2D _Texture;

		public ControllerIcons_TexturePreview(EditorInterface editorInterface)
		{
			nRoot = new();

			// UPGRADE: In Godot 4.2, there's no need to have an instance to
			// EditorInterface, since it's now a static call:
			// background = EditorInterface.get_base_control().get_theme_icon("Checkerboard", "EditorIcons")
			BackgroundTexture = editorInterface.GetBaseControl().GetThemeIcon("Checkerboard", "EditorIcons");

			nBackground = new()
			{
				StretchMode = TextureRect.StretchModeEnum.Tile,
				Texture = BackgroundTexture,
				TextureRepeat = CanvasItem.TextureRepeatEnum.Enabled,
				CustomMinimumSize = new Vector2(0, 256)
			};

			nRoot.AddChild(nBackground);

			nIconTexture = new()
			{
				TextureFilter = CanvasItem.TextureFilterEnum.NearestWithMipmaps
			};
			nIconTexture.SetAnchorsPreset(Control.LayoutPreset.FullRect);
			nIconTexture.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
			nIconTexture.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;

			nRoot.AddChild(nIconTexture);
		}

		public Control get_root()
		{
			return nRoot;
		}
	}

	public override bool _CanHandle( GodotObject obj )
	{
		return obj is ControllerIconTexture;
	}

	public override void _ParseBegin( GodotObject obj )
	{
		preview = new( ControllerIconPlugin.EditorInterface );
		AddCustomControl(preview.get_root());

		if( obj is ControllerIconTexture icon )
			preview.Texture = icon;
	}

	public override bool _ParseProperty( GodotObject obj, Variant.Type type, string name, PropertyHint hint_type, string hint_string, PropertyUsageFlags usage_flags, bool wide )
	{
		if( name == "path" )
		{
			ControllerIconPathEditorProperty path_selector_instance = new( ControllerIconPlugin.EditorInterface );
			AddPropertyEditor(name, path_selector_instance);
			return true;
		}
		return false;
	}

}
#endif