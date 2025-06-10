
using Godot;
using System;
using System.Collections.Generic;
using static ControllerIcons;

[Tool]
public partial class ControllerIconPathEditorProperty : EditorProperty
{
    private ControllerIconPathSelectorPopup selector;
    private LineEdit line_edit;

	public ControllerIconPathEditorProperty( EditorInterface editor_interface )
	{
        AddChild(build_tree( editor_interface ));
    }

	private Node build_tree( EditorInterface editor_interface )
	{
		selector = ResourceLoader.Load<PackedScene>("res://addons/controller_icons/objects/ControllerIconPathSelectorPopup.tscn").Instantiate<ControllerIconPathSelectorPopup>();

		selector.Visible = false;
        selector.editor_interface = editor_interface;
        selector.PathSelected += ( string path ) => {
			if( path.Length > 0 )
			{
				EmitChanged(GetEditedProperty(), path);
			}
        };

		HBoxContainer root = new();

		line_edit = new();
		line_edit.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        line_edit.TextChanged += ( string text ) => {
			EmitChanged(GetEditedProperty(), text);

        };

		Button button = new();
		// UPGRADE: In Godot 4.2, there's no need to have an instance to
		// EditorInterface, since it's now a static call:
		// button.icon = EditorInterface.get_base_control().get_theme_icon("ListSelect", "EditorIcons")
		button.Icon = editor_interface.GetBaseControl().GetThemeIcon("ListSelect", "EditorIcons");

		button.TooltipText = "Select an icon path";
        button.Pressed += () => {
            selector.populate();
            selector.PopupCentered();
        };

        root.AddChild(line_edit);
		root.AddChild(button);
		root.AddChild(selector);

		return root;
    }

	public override void _UpdateProperty()
	{
		string new_text = (string)GetEditedObject().Get(GetEditedProperty());
		if( line_edit.Text != new_text )
			line_edit.Text = new_text;
    }

}
