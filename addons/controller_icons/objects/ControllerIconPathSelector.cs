#if TOOLS
using Godot;
using System;

[Tool]
public partial class ControllerIconPathSelector : PanelContainer
{
	[Signal]
	public delegate void PathSelectedEventHandler(string path);

	private TabContainer nTabContainer;
	private InputActionSelector nInputAction;
	private JoypadPathSelector nJoypadPath;
	private SpecificPathSelector nSpecificPath;

	private bool InputActionPopulated = false;
	private bool JoypadPathPopulated = false;
	private bool SpecificPathPopulated = false; 

	public EditorInterface EditorInterface;

	public override void _Ready()
	{
		nTabContainer = GetNode<TabContainer>("%TabContainer");
		nInputAction = GetNode<InputActionSelector>("%Input Action");
		nJoypadPath = GetNode<JoypadPathSelector>("%Joypad Path");
		nSpecificPath = GetNode<SpecificPathSelector>("%Specific Path");

		nInputAction.Done += OnInputActionDone;
		nJoypadPath.Done += OnJoypadPathDone;
		nSpecificPath.Done += OnSpecificPathDone;
	}

	public void Populate( EditorInterface editorInterface )
	{
		this.EditorInterface = editorInterface;
		InputActionPopulated = false;
		JoypadPathPopulated = false;
		SpecificPathPopulated = false;
		nTabContainer.CurrentTab = 0;
	}

	public string GetIconPath()
	{
		if( nTabContainer.GetCurrentTabControl() is InputActionSelector ia )
		{
			return ia.GetIconPath();
		}
		else if( nTabContainer.GetCurrentTabControl() is JoypadPathSelector jp )
		{
			return jp.GetIconPath();
		}
		else if( nTabContainer.GetCurrentTabControl() is SpecificPathSelector sp )
		{
			return sp.GetIconPath();
		}

		return "";
	}

	private async void OnTabContainerTabSelected( int tab )
	{
		// if the tab container's default tab has a non-default value set in the tscn file
		// (ie. there is a "current_tab" value set at all, even if it is 0)
		// then this signal may get called even before _Ready() is called
		// Therefore we need to check that stuff has been setup first.
		// Ideally: Don't touch the tab container.
		if( nTabContainer == null || EditorInterface == null ) return;

		if( nTabContainer.GetCurrentTabControl() == nInputAction )
		{
			if( !InputActionPopulated )
			{
				InputActionPopulated = true;
				nInputAction.Populate(EditorInterface);
			}
		}
		else if( nTabContainer.GetCurrentTabControl() == nJoypadPath )
		{
			if( !JoypadPathPopulated )
			{
				JoypadPathPopulated = true;
				nJoypadPath.Populate(EditorInterface);
			}
		}
		else if( nTabContainer.GetCurrentTabControl() == nSpecificPath )
		{
			if( !SpecificPathPopulated )
			{
				SpecificPathPopulated = true;
				nSpecificPath.Populate(EditorInterface);
			}
			else if( !nSpecificPath.IsSignalsConnected )
			{
				nSpecificPath.HookupSignals();
			}
		}

		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		(nTabContainer.GetCurrentTabControl() as SelectorPanel).GrabFocus();
	}

	private void OnInputActionDone()
	{
	#if GODOT4_4_OR_GREATER
		EmitSignalPathSelected(nInputAction.GetIconPath());
	#else
		EmitSignal(SignalName.PathSelected, nInputAction.GetIconPath());
	#endif
	}

	private void OnJoypadPathDone()
	{
	#if GODOT4_4_OR_GREATER
		EmitSignalPathSelected(nJoypadPath.GetIconPath());
	#else
		EmitSignal(SignalName.PathSelected, nJoypadPath.GetIconPath());
	#endif
	}

	private void OnSpecificPathDone()
	{
	#if GODOT4_4_OR_GREATER
		EmitSignalPathSelected(nSpecificPath.GetIconPath());
	#else
		EmitSignal(SignalName.PathSelected, nSpecificPath.GetIconPath());
	#endif
	}

	public void Cleanup()
	{
		//the goal with this cleanup is to remove any signal connections
		//to subclass objects (eg. ControllerIcons_Icon) so things
		//play nicer with hotreloading.
		//this means signals need to be re-hooked up if necessary when opened
		//in OnTabContainerTabSelected.
		nSpecificPath.CleanupSignals();
	}
}
#endif