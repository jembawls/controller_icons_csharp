#if TOOLS
using Godot;
using System;
using System.Collections.Generic;
using static ControllerIcons;

[Tool]
public partial class ControllerIconPathEditorProperty : EditorProperty
{
	private ControllerIconPathSelectorPopup selector;
	private LineEdit line_edit;

	public ControllerIconPathEditorProperty()
	{
	}

	public ControllerIconPathEditorProperty( EditorInterface editorInterface )
	{
		AddChild(BuildTree( editorInterface ));
	}

	private HBoxContainer BuildTree( EditorInterface editorInterface )
	{
		selector = ResourceLoader.Load<PackedScene>("res://addons/controller_icons/objects/ControllerIconPathSelectorPopup.tscn").Instantiate<ControllerIconPathSelectorPopup>();

		selector.Visible = false;
		selector.EditorInterface = editorInterface;
		selector.PathSelected += OnPathSelected;

		HBoxContainer root = new();

		line_edit = new()
		{
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
		};
		line_edit.TextChanged += OnTextChanged;

		Button button = new()
		{
			// UPGRADE: In Godot 4.2, there's no need to have an instance to
			// EditorInterface, since it's now a static call:
			// button.icon = EditorInterface.get_base_control().get_theme_icon("ListSelect", "EditorIcons")
			Icon = editorInterface.GetBaseControl().GetThemeIcon("ListSelect", "EditorIcons"),

			TooltipText = "Select an icon path"
		};
		button.Pressed += OnButtonPressed;

		root.AddChild(line_edit);
		root.AddChild(button);
		root.AddChild(selector);

		return root;
	}

	private void OnTextChanged( string text )
	{
		EmitChanged(GetEditedProperty(), text);
	}

	private void OnPathSelected(string path)
	{
		if( path.Length > 0 )
		{
			EmitChanged(GetEditedProperty(), path);
		}
	}

	private void OnButtonPressed()
	{
		selector.Populate();
		selector.PopupCentered();
	}

	public override void _UpdateProperty()
	{
		string new_text = (string)GetEditedObject().Get(GetEditedProperty());
		if( line_edit.Text != new_text )
			line_edit.Text = new_text;
	}

}
#endif