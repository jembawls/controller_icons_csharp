#if TOOLS
using Godot;
using System;
using System.Collections.Generic;
using static ControllerIcons;

[Tool]
public partial class SpecificPathSelector : SelectorPanel
{
	[Signal]
	public delegate void DoneEventHandler();

	private LineEdit NameFilter;
	private Tree BaseAssetNames;
	private HFlowContainer AssetsContainer;

	private ControllerIcons_Icon _LastPressedIcon;
	private ulong _LastPressedTimestamp;

	private Color ColorTextEnabled;
	private Color ColorTextDisabled;
	
	Dictionary<string,Dictionary<string,ControllerIcons_Icon>> ButtonNodes = new();
	TreeItem AssetNamesRoot;

	public bool IsSignalsConnected { get; private set; }

	public override void _Ready()
	{
		NameFilter = GetNode<LineEdit>("%NameFilter");
		BaseAssetNames = GetNode<Tree>("%BaseAssetNames");
		AssetsContainer = GetNode<HFlowContainer>("%AssetsContainer");
	}
	
	//hooks up signals for EXISTING icons
	public void HookupSignals()
	{
		if( IsSignalsConnected )
			return;

		foreach( Dictionary<string, ControllerIcons_Icon> category in ButtonNodes.Values )
		{
			foreach( ControllerIcons_Icon icon in category.Values )
			{
				icon.Button.Pressed += icon.OnIconPressed;
			}
		}

		IsSignalsConnected = true;
	}
	
	public void CleanupSignals()
	{ 
		if( !IsSignalsConnected )
			return;

		foreach( Dictionary<string, ControllerIcons_Icon> category in ButtonNodes.Values )
		{
			foreach( ControllerIcons_Icon icon in category.Values )
			{
				icon.Button.Pressed -= icon.OnIconPressed;
			}
		}

		IsSignalsConnected = false;
	}

	class ControllerIcons_Icon
	{
		public static SpecificPathSelector PathSelector;
		public static ButtonGroup group = new();

		public Button Button;
		public string Category;
		public string Path;

		public bool Selected
		{
			get	{ return _Selected; }
			set
			{
				_Selected = value;
				QueryVisibility();
			}

		}
		private bool _Selected;

		public bool Filtered
		{
			get	{ return _Filtered; }
			set
			{
				_Filtered = value;
				QueryVisibility();
			}

		}
		private bool _Filtered;
		
		private void QueryVisibility()
		{
			if( IsInstanceValid(Button) )
			{
				Button.Visible = Selected && Filtered;
			}
		}

		public ControllerIcons_Icon( string category, string path)
		{
			Category = category;
			Filtered = true;
			Path = path.Split("/")[1];

			Button = new()
			{
				CustomMinimumSize = new Vector2(100, 100),
				ClipText = true,
				TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis,
				IconAlignment = HorizontalAlignment.Center,
				VerticalIconAlignment = VerticalAlignment.Top,
				ExpandIcon = true,
				ToggleMode = true,
				ButtonGroup = group,
				Text = this.Path
			};

			ControllerIconTexture icon = new()
			{
				path = path
			};
			Button.Icon = icon;
		}

		public void OnIconPressed()
		{
			if( PathSelector._LastPressedIcon == this )
			{
				if (Time.GetTicksMsec() < PathSelector._LastPressedTimestamp)
				{
				#if GODOT4_4_OR_GREATER
					PathSelector.EmitSignalDone();
				#else
					PathSelector.EmitSignal(SignalName.Done);
				#endif					
				}
				else
					PathSelector._LastPressedTimestamp = Time.GetTicksMsec() + 1000;
			}
			else
			{
				PathSelector._LastPressedIcon = this;
				PathSelector._LastPressedTimestamp = Time.GetTicksMsec() + 1000;
			}
		}
	}

	public void Populate( EditorInterface editorInterface )
	{
		// Using clear() triggers a signal and uses freed nodes.
		// Setting the text directly does not.
		NameFilter.Text = "";
		BaseAssetNames.Clear();
		ButtonNodes.Clear();
		foreach( Node c in AssetsContainer.GetChildren() )
		{
			AssetsContainer.RemoveChild(c);
			c.QueueFree();
		}

		// UPGRADE: In Godot 4.2, there's no need to have an instance to
		// EditorInterface, since it's now a static call:
		// var editor_control := EditorInterface.get_base_control()
		Control editorControl = editorInterface.GetBaseControl();
		ColorTextEnabled = editorControl.GetThemeColor("font_color", "Editor");
		ColorTextDisabled = editorControl.GetThemeColor("disabled_font_color", "Editor");
		NameFilter.RightIcon = editorControl.GetThemeIcon("Search", "EditorIcons");

		AssetNamesRoot = BaseAssetNames.CreateItem();

		Godot.Collections.Array<string> basePaths = new(){
			CI.Settings.custom_asset_dir,
			"res://addons/controller_icons/assets"
		};

		//Update the static reference to this now
		ControllerIcons_Icon.PathSelector = this;

		// UPGRADE: In Godot 4.2, for-loop variables can be
		// statically typed:
		// for base_path:string in base_paths:
		foreach( string basePath in basePaths )
		{
			if( basePath.Length == 0 || !basePath.StartsWith("res://") )
				continue;

			// Files first
			HandleFiles("", basePath);

			// Directories next
			foreach( string dir in DirAccess.GetDirectoriesAt(basePath) )
			{
				HandleFiles(dir, basePath.PathJoin(dir));
			}
		}

		IsSignalsConnected = true;

		TreeItem child = AssetNamesRoot.GetNextInTree();
		child?.Select(0);
	}

	private void HandleFiles( string category, string basePaths )
	{
		foreach( string file in DirAccess.GetFilesAt(basePaths) )
		{
			if( file.GetExtension() == CI.BaseExtension )
				CreateIcon(category, basePaths.PathJoin(file));
		}
	}

	private void CreateIcon( string category, string path )
	{
		string mapCategory = category.Length == 0 ? "<no category>" : category;
		
		if( !ButtonNodes.ContainsKey(mapCategory) )
		{
			ButtonNodes[mapCategory] = new();
			TreeItem item = BaseAssetNames.CreateItem(AssetNamesRoot);
			item.SetText(0, mapCategory);
		}

		string filename = path.GetFile();
		if( ButtonNodes[mapCategory].ContainsKey(filename) ) return;

		string icon_path = (category.Length == 0 ? "" : category ) + "/" + path.GetFile().GetBaseName();
		ControllerIcons_Icon icon = new( mapCategory, icon_path);

		ButtonNodes[mapCategory][filename] = icon;
		AssetsContainer.AddChild(icon.Button);

		icon.Button.Pressed += icon.OnIconPressed;	 	
	}

	public string GetIconPath()
	{
		if (ControllerIcons_Icon.group.GetPressedButton() is Button button)
			return (button.Icon as ControllerIconTexture).path;

		return "";
	}

	public override void GrabFocus()
	{
		NameFilter.GrabFocus();
	}

	private void OnBaseAssetNamesItemSelected()
	{
		TreeItem selected = BaseAssetNames.GetSelected();
		if( selected == null ) return;

		string category = selected.GetText(0);
		if( !ButtonNodes.ContainsKey(category) ) return;

		// UPGRADE: In Godot 4.2, for-loop variables can be
		// statically typed:
		// for key:string in button_nodes.keys():
		// 	for icon:ControllerIcon_Icon in button_nodes[key].values():
		foreach( string key in ButtonNodes.Keys )
		{
			foreach( ControllerIcons_Icon icon in ButtonNodes[key].Values )
			{
				icon.Selected = key == category;
			}
		}
	}

	private void OnNameFilterTextChanged( string new_text )
	{
		Godot.Collections.Dictionary<string,bool> any_visible = new();
		TreeItem asset_name = AssetNamesRoot.GetNextInTree();
		while( asset_name != null )
		{
			any_visible[asset_name.GetText(0)] = false;
			asset_name = asset_name.GetNextInTree();
		}
		
		TreeItem selectedCategory = BaseAssetNames.GetSelected();

		// UPGRADE: In Godot 4.2, for-loop variables can be
		// statically typed:
		// for key:string in button_nodes.keys():
		// 	for icon:Icon in button_nodes[key].values():
		foreach( string key in ButtonNodes.Keys )
		{
			foreach( ControllerIcons_Icon icon in ButtonNodes[key].Values )
			{
				bool filtered = new_text.Length == 0 || icon.Path.FindN(new_text) != -1;
				icon.Filtered = filtered;
				any_visible[key] = any_visible[key] || filtered;
			}
		}

		asset_name = AssetNamesRoot.GetNextInTree();
		while( asset_name != null )
		{
			string category = asset_name.GetText(0);
			if( any_visible.TryGetValue(category, out bool selectable) )
			{
				asset_name.SetSelectable(0, selectable);
				if( !selectable )
					asset_name.Deselect(0);
				asset_name.SetCustomColor(0, selectable ? ColorTextEnabled : ColorTextDisabled);
			}

			asset_name = asset_name.GetNextInTree();
		}
	}
}
#endif