
using Godot;
using System;
using System.Collections.Generic;
using static ControllerIcons;

[Tool]
public partial class SpecificPathSelector : Panel
{
	[Signal]
	private delegate void DoneEventHandler();

    private LineEdit n_name_filter;
    private Tree n_base_asset_names;
    private HFlowContainer n_assets_container;

    private ControllerIcons_Icon _last_pressed_icon;
    private ulong _last_pressed_timestamp;

    private Color color_text_enabled;
	private Color color_text_disabled;
	
    Dictionary<string,Dictionary<string,ControllerIcons_Icon>> button_nodes = [];
    TreeItem asset_names_root;

	public override void _Ready()
	{
        n_name_filter = GetNode<LineEdit>("%NameFilter");
        n_base_asset_names = GetNode<Tree>("%BaseAssetNames");
		n_assets_container = GetNode<HFlowContainer>("%AssetsContainer");
	}

	class ControllerIcons_Icon
	{
		public static ButtonGroup group = new();

        public Button button;
        public string category;
        public string path;

        public bool selected
		{
			get	{ return _selected; }
			set
			{
                _selected = value;
                _query_visibility();
            }

		}
        private bool _selected;

        public bool filtered
		{
			get	{ return _filtered; }
			set
			{
                _filtered = value;
                _query_visibility();
            }

		}
        private bool _filtered;
		
		private void _query_visibility()
		{
			if( IsInstanceValid(button) )
			{
				button.Visible = selected && filtered;
            }
		}

		public ControllerIcons_Icon( string category, string path)
		{
            this.category = category;
            filtered = true;
            this.path = path.Split("/")[1];

            button = new();
            button.CustomMinimumSize = new Vector2(100, 100);
            button.ClipText = true;
			button.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
			button.IconAlignment = HorizontalAlignment.Center;
			button.VerticalIconAlignment = VerticalAlignment.Top;
			button.ExpandIcon = true;
			button.ToggleMode = true;
			button.ButtonGroup = group;
			button.Text = this.path;

			ControllerIconTexture icon = new();
            icon.path = path;
            button.Icon = icon;
        }
	}

    public void populate( EditorInterface editor_interface )
	{
        // Using clear() triggers a signal and uses freed nodes.
        // Setting the text directly does not.
        n_name_filter.Text = "";
        n_base_asset_names.Clear();
        button_nodes.Clear();
        foreach( Node c in n_assets_container.GetChildren() )
		{
            n_assets_container.RemoveChild(c);
            c.QueueFree();
        }

		// UPGRADE: In Godot 4.2, there's no need to have an instance to
		// EditorInterface, since it's now a static call:
		// var editor_control := EditorInterface.get_base_control()
		Control editor_control = editor_interface.GetBaseControl();
        color_text_enabled = editor_control.GetThemeColor("font_color", "Editor");
        color_text_disabled = editor_control.GetThemeColor("disabled_font_color", "Editor");
        n_name_filter.RightIcon = editor_control.GetThemeIcon("Search", "EditorIcons");

        asset_names_root = n_base_asset_names.CreateItem();

        Godot.Collections.Array<string> base_paths = [
            CI._settings.custom_asset_dir,
            "res://addons/controller_icons/assets"
        ];

        // UPGRADE: In Godot 4.2, for-loop variables can be
        // statically typed:
        // for base_path:string in base_paths:
        foreach( string base_path in base_paths )
		{
			if( base_path.Length == 0 || !base_path.StartsWith("res://") )
                continue;

            // Files first
            handle_files("", base_path);

            // Directories next
            foreach( string dir in DirAccess.GetDirectoriesAt(base_path) )
			{
                handle_files(dir, base_path.PathJoin(dir));
            }
		}

        TreeItem child = asset_names_root.GetNextInTree();
        if( child != null )
            child.Select(0);
    }

	private void handle_files( string category, string base_path )
	{
		foreach( string file in DirAccess.GetFilesAt(base_path) )
		{
			if( file.GetExtension() == CI._base_extension )
				create_icon(category, base_path.PathJoin(file));
        }
	}

	private void create_icon( string category, string path )
	{
        string map_category = category.Length == 0 ? "<no category>" : category;
		
        if( !button_nodes.ContainsKey(map_category) )
		{
			button_nodes[map_category] = [];
            TreeItem item = n_base_asset_names.CreateItem(asset_names_root);
            item.SetText(0, map_category);
        }

		string filename = path.GetFile();
        if( button_nodes[map_category].ContainsKey(filename) ) return;

        string icon_path = (category.Length == 0 ? "" : category ) + "/" + path.GetFile().GetBaseName();
        ControllerIcons_Icon icon = new(map_category, icon_path);

        button_nodes[map_category][filename] = icon;
        n_assets_container.AddChild(icon.button);

        icon.button.Pressed += () => {
			if( _last_pressed_icon == icon )
			{
				if( Time.GetTicksMsec() < _last_pressed_timestamp )
                    EmitSignalDone();
                else
                    _last_pressed_timestamp = Time.GetTicksMsec() + 1000;        
            }
			else
			{
				_last_pressed_icon = icon;
				_last_pressed_timestamp = Time.GetTicksMsec() + 1000;
			}
        };		
	}

	public string get_icon_path()
	{
		Button button = ControllerIcons_Icon.group.GetPressedButton() as Button;
        if( button != null )
        	return (button.Icon as ControllerIconTexture).path;

        return "";
    }

	private void grab_focus()
	{
		n_name_filter.GrabFocus();
    }

	private void _on_base_asset_names_item_selected()
	{
        TreeItem selected = n_base_asset_names.GetSelected();
        if( selected == null ) return;

        string category = selected.GetText(0);
        if( !button_nodes.ContainsKey(category) ) return;

        // UPGRADE: In Godot 4.2, for-loop variables can be
        // statically typed:
        // for key:string in button_nodes.keys():
        // 	for icon:ControllerIcon_Icon in button_nodes[key].values():
        foreach( string key in button_nodes.Keys )
		{
			foreach( ControllerIcons_Icon icon in button_nodes[key].Values )
			{
                icon.selected = key == category;
            }
		}
	}

	private void _on_name_filter_text_changed( string new_text )
	{
		Godot.Collections.Dictionary<string,bool> any_visible = [];
		TreeItem asset_name = asset_names_root.GetNextInTree();
		while( asset_name != null )
		{
			any_visible[asset_name.GetText(0)] = false;
			asset_name = asset_name.GetNextInTree();
		}
		
		TreeItem selected_category = n_base_asset_names.GetSelected();

		// UPGRADE: In Godot 4.2, for-loop variables can be
		// statically typed:
		// for key:string in button_nodes.keys():
		// 	for icon:Icon in button_nodes[key].values():
		foreach( string key in button_nodes.Keys )
		{
			foreach( ControllerIcons_Icon icon in button_nodes[key].Values )
			{
				bool filtered = new_text.Length == 0 || icon.path.FindN(new_text) != -1;
				icon.filtered = filtered;
				any_visible[key] = any_visible[key] || filtered;
			}
		}

        asset_name = asset_names_root.GetNextInTree();
        while( asset_name != null )
		{
			string category = asset_name.GetText(0);
			if( any_visible.ContainsKey(category) )
			{
				bool selectable = any_visible[category];
				asset_name.SetSelectable(0, selectable);
				if( !selectable )
					asset_name.Deselect(0);
                asset_name.SetCustomColor(0, selectable ? color_text_enabled : color_text_disabled);
			}

			asset_name = asset_name.GetNextInTree();
		}

	}
}
