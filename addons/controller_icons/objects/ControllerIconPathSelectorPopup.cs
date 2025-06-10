
using Godot;
using System;

[Tool]
public partial class ControllerIconPathSelectorPopup : ConfirmationDialog
{
    [Signal]
    public delegate void PathSelectedEventHandler(string path);

    public EditorInterface editor_interface;

    private ControllerIconPathSelector n_selector;

	public override void _Ready()
	{
		n_selector = GetNode<ControllerIconPathSelector>("ControllerIconPathSelector");
    }

	public void populate()
	{
        n_selector.populate(editor_interface);
    }

	private void _on_controller_icon_path_selector_path_selected( string path )
	{
        EmitSignalPathSelected(path);
        Hide();
    }

	private void _on_confirmed()
	{
        EmitSignalPathSelected(n_selector.get_icon_path());
	}
}
