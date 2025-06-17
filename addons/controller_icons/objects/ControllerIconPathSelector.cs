
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
	}

	public void populate( EditorInterface editorInterface )
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
		}

		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		nTabContainer.GetCurrentTabControl().GrabFocus();
	}

	private void OnInputActionDone()
	{
		EmitSignalPathSelected(nInputAction.GetIconPath());
	}

	private void OnJoypadPathDone()
	{
		EmitSignalPathSelected(nJoypadPath.GetIconPath());
	}

	private void OnSpecificPathDone()
	{
		EmitSignalPathSelected(nSpecificPath.GetIconPath());
	}
}
