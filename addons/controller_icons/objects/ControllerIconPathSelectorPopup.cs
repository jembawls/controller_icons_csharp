
using Godot;
using System;

[Tool]
public partial class ControllerIconPathSelectorPopup : ConfirmationDialog
{
	[Signal]
	public delegate void PathSelectedEventHandler(string path);

	public EditorInterface EditorInterface;

	private ControllerIconPathSelector nSelector;

	public override void _Ready()
	{
		nSelector = GetNode<ControllerIconPathSelector>("ControllerIconPathSelector");
	}

	public void Populate()
	{
		nSelector.populate(EditorInterface);
	}

	private void OnControllerIconPathSelectorPathSelected( string path )
	{
		EmitSignalPathSelected(path);
		Hide();
	}

	private void OnConfirmed()
	{
		EmitSignalPathSelected(nSelector.GetIconPath());
	}
}
