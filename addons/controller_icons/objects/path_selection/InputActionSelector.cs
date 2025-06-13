
using Godot;
using System;
using System.Collections.Generic;
using static ControllerIcons;

[Tool]
public partial class InputActionSelector : Panel
{
	[Signal]
	private delegate void DoneEventHandler();

    private LineEdit n_name_filter;
    private CheckButton n_builtin_action_button;
    private Tree n_tree;

    private TreeItem root;
    private List<ControllerIcons_Item> items = [];

	public override void _Ready()
	{
        n_name_filter = GetNode<LineEdit>("%NameFilter");
		n_builtin_action_button = GetNode<CheckButton>("%BuiltinActionButton");
		n_tree = GetNode<Tree>("%Tree");
    }

    class ControllerIcons_Item
    {
        public bool is_default;
        public TreeItem tree_item;
        private ControllerIconTexture controller_icon_key;
        private ControllerIconTexture controller_icon_joy;

        public bool show_default
        {
            get { return _show_default; }
            set
			{
                _show_default = value;
                _query_visibility();
            }

        }
    	private bool _show_default;
		
        public bool filtered
        {
            get { return _filtered; }
            set
			{
                _filtered = value;
                _query_visibility();
            }

        }
    	private bool _filtered;

		public ControllerIcons_Item(Tree tree, TreeItem root, string path, bool is_default )
		{
			this.is_default = is_default;
			this.filtered = true;
			tree_item = tree.CreateItem(root);

			tree_item.SetText(0, path);

			controller_icon_key = new();
			controller_icon_key.path = path;
			controller_icon_key.force_type = InputType.KEYBOARD_MOUSE;

			controller_icon_joy = new();
			controller_icon_joy.path = path;
			controller_icon_joy.force_type = InputType.CONTROLLER;

			tree_item.SetIconMaxWidth(1, 48 * controller_icon_key._textures.Count);

			tree_item.SetIconMaxWidth(2, 48 * controller_icon_key._textures.Count);
			tree_item.SetIcon(1, controller_icon_key);
			tree_item.SetIcon(2, controller_icon_joy);
		}

		private void _query_visibility()
		{
			if( IsInstanceValid(tree_item) )
				tree_item.Visible = show_default && filtered;
        }
	}

    public void populate( EditorInterface editor_interface )
	{
        // Clear
        n_tree.Clear();

        // Using clear() triggers a signal and uses freed nodes.
        // Setting the text directly does not.
        n_name_filter.Text = "";
        items.Clear();

		n_name_filter.RightIcon = editor_interface.GetBaseControl().GetThemeIcon("Search", "EditorIcons");

		// Setup tree columns
		n_tree.SetColumnTitle(0, "Action");

		n_tree.SetColumnTitle(1, "Preview");
		n_tree.SetColumnExpand(1, false);
		n_tree.SetColumnExpand(2, false);

		// Force ControllerIcons to reload the input map
		CI.ParseInputActions();

		// List with all default input actions
		List<string> default_actions = [];		
		foreach( string key in CI._builtin_keys )
		{
			default_actions.Add( key.TrimPrefix("input/") );
        }

        // Map with all input actions
        root = n_tree.CreateItem();
        foreach( string data in CI._custom_input_actions.Keys )
		{
			ControllerIcons_Item child = new(n_tree, root, data, default_actions.Contains(data) );
			items.Add(child);
		}

		set_default_actions_visibility(n_builtin_action_button.ButtonPressed);
	}

	public string get_icon_path()
	{
        TreeItem item = n_tree.GetSelected();
        if( IsInstanceValid(item) )
            return item.GetText(0);

        return "";
    }

	private void set_default_actions_visibility( bool display )
	{
		// UPGRADE: In Godot 4.2, for-loop variables can be
		// statically typed:
		// for item:ControllerIcons_Item in items:
		foreach ( ControllerIcons_Item item in items )
		{
			item.show_default = display || !item.is_default;
		}
	}

	private void grab_focus()
	{
        n_name_filter.GrabFocus();
    }

	private void _on_builtin_action_button_toggled( bool toggled_on )
	{
        set_default_actions_visibility(toggled_on);
    }

	private void _on_tree_item_activated()
	{
        EmitSignalDone();
    }

	private void _on_name_filter_text_changed( string new_text )
	{
		// UPGRADE: In Godot 4.2, for-loop variables can be
		// statically typed:
		// for item:ControllerIcons_Item in items:
		foreach( ControllerIcons_Item item in items )
		{
			bool filtered = new_text.Length == 0 || item.tree_item.GetText(0).FindN(new_text) != -1;
        	item.filtered = filtered;
		}
    }

}
