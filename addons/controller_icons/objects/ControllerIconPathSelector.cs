
using Godot;
using System;

[Tool]
public partial class ControllerIconPathSelector : PanelContainer
{
    [Signal]
    public delegate void PathSelectedEventHandler(string path);

    private TabContainer n_tab_container;
    private InputActionSelector n_input_action;
    private JoypadPathSelector n_joypad_path;
    private SpecificPathSelector n_specific_path;

    private bool input_action_populated = false;
    private bool joypad_path_populated = false;
    private bool specific_path_populated = false;

    public EditorInterface editor_interface;	

	public override void _Ready()
	{
        n_tab_container = GetNode<TabContainer>("%TabContainer");
        n_input_action = GetNode<InputActionSelector>("%Input Action");
		n_joypad_path = GetNode<JoypadPathSelector>("%Joypad Path");
		n_specific_path = GetNode<SpecificPathSelector>("%Specific Path");
    }

    public void populate( EditorInterface editor_interface )
	{
        this.editor_interface = editor_interface;
        input_action_populated = false;
        joypad_path_populated = false;
        specific_path_populated = false;
        n_tab_container.CurrentTab = 0;
    }

	public string get_icon_path()
	{
		if( n_tab_container.GetCurrentTabControl() is InputActionSelector ia )
		{
            return ia.get_icon_path();
        }
		else if( n_tab_container.GetCurrentTabControl() is JoypadPathSelector jp )
		{
            return jp.get_icon_path();
        }
		else if( n_tab_container.GetCurrentTabControl() is SpecificPathSelector sp )
		{
            return sp.get_icon_path();
        }

        return "";
    }

	private async void _on_tab_container_tab_selected( int tab )
	{
		// if the tab container's default tab has a non-default value set in the tscn file
		// (ie. there is a "current_tab" value set at all, even if it is 0)
		// then this signal may get called even before _Ready() is called
		// Therefore we need to check that stuff has been setup first.
		// Ideally: Don't touch the tab container.
		if( n_tab_container == null || editor_interface == null ) return;

        if( n_tab_container.GetCurrentTabControl() == n_input_action )
		{
			if( !input_action_populated )
			{
				input_action_populated = true;
				n_input_action.populate(editor_interface);
			}
		}
		else if( n_tab_container.GetCurrentTabControl() == n_joypad_path )
		{
			if( !joypad_path_populated )
			{
                joypad_path_populated = true;
                n_joypad_path.populate(editor_interface);
            }
		}
		else if( n_tab_container.GetCurrentTabControl() == n_specific_path )
		{
			if( !specific_path_populated )
			{
				specific_path_populated = true;
				n_specific_path.populate(editor_interface);
			}			
		}

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        n_tab_container.GetCurrentTabControl().GrabFocus();
    }

	private void _on_input_action_done()
	{
		EmitSignalPathSelected(n_input_action.get_icon_path());
    }

	private void _on_joypad_path_done()
	{
		EmitSignalPathSelected(n_joypad_path.get_icon_path());
    }

	private void _on_specific_path_done()
	{
		EmitSignalPathSelected(n_specific_path.get_icon_path());
    }
}
