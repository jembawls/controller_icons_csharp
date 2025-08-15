#if TOOLS
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
		nSelector.PathSelected += OnControllerIconPathSelectorPathSelected;
		Confirmed += OnConfirmed;
		Canceled += OnCancelled;
	}

	public void Populate()
	{
		nSelector.Populate(EditorInterface);
	} 

	private void OnControllerIconPathSelectorPathSelected( string path )
	{		
	#if GODOT4_4_OR_GREATER
		EmitSignalPathSelected(path);
	#else
		EmitSignal(SignalName.PathSelected, path);
	#endif

		Cleanup();
		Hide();
	}

	private void OnConfirmed()
	{		
	#if GODOT4_4_OR_GREATER
		EmitSignalPathSelected(nSelector.GetIconPath());
	#else
		EmitSignal(SignalName.PathSelected, nSelector.GetIconPath());
	#endif

		Cleanup();
	}

	private void OnCancelled()
	{
		Cleanup();
	}

	private void Cleanup()
	{
		nSelector.Cleanup();
	}
}
#endif