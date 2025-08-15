#if TOOLS
using Godot;
using System;
using System.Collections.Generic;
using static ControllerIcons;

[Tool]
public partial class InputActionSelector : SelectorPanel
{
	[Signal]
	public delegate void DoneEventHandler();

	private LineEdit nNameFilter;
	private CheckButton nBuiltInActionButton;
	private Tree nTree;

	private TreeItem root;
	private List<ControllerIcons_Item> items = new();

	public override void _Ready()
	{
		nNameFilter = GetNode<LineEdit>("%NameFilter");
		nBuiltInActionButton = GetNode<CheckButton>("%BuiltinActionButton");
		nTree = GetNode<Tree>("%Tree");
	}

	class ControllerIcons_Item
	{
		public bool IsDefault;
		public TreeItem nTreeItem;
		private ControllerIconTexture ControllerIcon_Key;
		private ControllerIconTexture ControllerIcon_Joy;

		public bool ShowDefault
		{
			get { return _ShowDefault; }
			set
			{
				_ShowDefault = value;
				QueryVisibility();
			}

		}
		private bool _ShowDefault;
		
		public bool Filtered
		{
			get { return _Filtered; }
			set
			{
				_Filtered = value;
				QueryVisibility();
			}

		}
		private bool _Filtered;

		public ControllerIcons_Item(Tree tree, TreeItem root, string path, bool is_default )
		{
			this.IsDefault = is_default;
			this.Filtered = true;
			nTreeItem = tree.CreateItem(root);

			nTreeItem.SetText(0, path);

			ControllerIcon_Key = new()
			{
				path = path,
				force_type = EInputType.KEYBOARD_MOUSE
			};

			ControllerIcon_Joy = new()
			{
				path = path,
				force_type = EInputType.CONTROLLER
			};

			nTreeItem.SetIconMaxWidth(1, 48 * ControllerIcon_Key.Textures.Count);

			nTreeItem.SetIconMaxWidth(2, 48 * ControllerIcon_Key.Textures.Count);
			nTreeItem.SetIcon(1, ControllerIcon_Key);
			nTreeItem.SetIcon(2, ControllerIcon_Joy);
		}

		private void QueryVisibility()
		{
			if( IsInstanceValid(nTreeItem) )
				nTreeItem.Visible = ShowDefault && Filtered;
		}
	}

	public void Populate( EditorInterface editor_interface )
	{
		// Clear
		nTree.Clear();

		// Using clear() triggers a signal and uses freed nodes.
		// Setting the text directly does not.
		nNameFilter.Text = "";
		items.Clear();

		nNameFilter.RightIcon = editor_interface.GetBaseControl().GetThemeIcon("Search", "EditorIcons");

		// Setup tree columns
		nTree.SetColumnTitle(0, "Action");

		nTree.SetColumnTitle(1, "Preview");
		nTree.SetColumnExpand(1, false);
		nTree.SetColumnExpand(2, false);

		// Force ControllerIcons to reload the input map
		CI.ParseInputActions();

		// List with all default input actions
		List<string> default_actions = new();		
		foreach( string key in CI.BuiltInKeys )
		{
			default_actions.Add( key.TrimPrefix("input/") );
		}

		// Map with all input actions
		root = nTree.CreateItem();
		foreach( string data in CI.CustomInputActions.Keys )
		{
			ControllerIcons_Item child = new(nTree, root, data, default_actions.Contains(data) );
			items.Add(child);
		}

		SetDefaultActionsVisibility(nBuiltInActionButton.ButtonPressed);
	}

	public string GetIconPath()
	{
		TreeItem item = nTree.GetSelected();
		if( IsInstanceValid(item) )
			return item.GetText(0);

		return "";
	}

	private void SetDefaultActionsVisibility( bool display )
	{
		// UPGRADE: In Godot 4.2, for-loop variables can be
		// statically typed:
		// for item:ControllerIcons_Item in items:
		foreach ( ControllerIcons_Item item in items )
		{
			item.ShowDefault = display || !item.IsDefault;
		}
	}

	public override void GrabFocus()
	{
		nNameFilter.GrabFocus();
	}

	private void OnBuiltInActionButtonToggled( bool toggled_on )
	{
		SetDefaultActionsVisibility(toggled_on);
	}

	private void OnTreeItemActivated()
	{
	#if GODOT4_4_OR_GREATER
		EmitSignalDone();
	#else
		EmitSignal(SignalName.Done);
	#endif
	}

	private void OnNameFilterTextChanged( string new_text )
	{
		// UPGRADE: In Godot 4.2, for-loop variables can be
		// statically typed:
		// for item:ControllerIcons_Item in items:
		foreach( ControllerIcons_Item item in items )
		{
			bool filtered = new_text.Length == 0 || item.nTreeItem.GetText(0).FindN(new_text) != -1;
			item.Filtered = filtered;
		}
	}

}
#endif