#if TOOLS
using Godot;
using System;
using System.Collections.Generic;
using static ControllerIcons;

[Tool]
public partial class JoypadPathSelector : SelectorPanel
{
	[Signal]
	public delegate void DoneEventHandler();

	private Label ButtonLabel;
	private Godot.Collections.Array<Button> ButtonNodes;

	private Button LastPressedButton;
	private ulong LastPressedTimestamp;

	public override void _Ready()
	{
		ButtonLabel = GetNode<Label>("%ButtonLabel");
		ButtonNodes = new(){
			GetNode<Button>("%LT"), GetNode<Button>("%RT"),
			GetNode<Button>("%LStick"), GetNode<Button>("%RStick"),
			GetNode<Button>("%LStickClick"), GetNode<Button>("%RStickClick"),
			GetNode<Button>("%LB"), GetNode<Button>("%RB"), GetNode<Button>("%A"), GetNode<Button>("%B"), GetNode<Button>("%X"), GetNode<Button>("%Y"),
			GetNode<Button>("%Select"), GetNode<Button>("%Start"),
			GetNode<Button>("%Home"), GetNode<Button>("%Share"), GetNode<Button>("%DPAD"),
			GetNode<Button>("%DPADDown"), GetNode<Button>("%DPADRight"),
			GetNode<Button>("%DPADLeft"), GetNode<Button>("%DPADUp")
		};
	}

	public void Populate( EditorInterface editorInterface )
	{
		// UPGRADE: In Godot 4.2, for-loop variables can be
		// statically typed:
		// for button:Button in button_nodes:
		foreach( Button button in ButtonNodes )
			button.ButtonPressed = false;
	}

	public string GetIconPath()
	{
		// UPGRADE: In Godot 4.2, for-loop variables can be
		// statically typed:
		// for button:Button in button_nodes:
		foreach( Button button in ButtonNodes )
		{
			if( button.ButtonPressed )
				return ( button.Icon as ControllerIconTexture ).path;
		}

		return "";
	}

	public override void _Input( InputEvent e )
	{
		if( !Visible ) return;

		if( e is InputEventJoypadMotion motionEvent )
			InputMotion(motionEvent);
		else if( e is InputEventJoypadButton buttonEvent )
			InputButton(buttonEvent);
	}

	private void InputMotion(InputEventJoypadMotion e )
	{
		if( Mathf.Abs(e.AxisValue) < 0.5f ) return;

		switch( e.Axis )
		{
			case JoyAxis.LeftX:
			case JoyAxis.LeftY:
				SimulateButtonPress(GetNode<Button>("%LStick"));
				break;
			case JoyAxis.RightX:
			case JoyAxis.RightY:
				SimulateButtonPress(GetNode<Button>("%RStick"));
				break;
			case JoyAxis.TriggerLeft:
				SimulateButtonPress(GetNode<Button>("%LT"));
				break;
			case JoyAxis.TriggerRight:
				SimulateButtonPress(GetNode<Button>("%RT"));
				break;
		}
	}

	private void InputButton( InputEventJoypadButton e )
	{
		if( !e.Pressed ) return;

		switch( e.ButtonIndex )
		{
			case JoyButton.A:
				SimulateButtonPress(GetNode<Button>("%A"));
				break;
			case JoyButton.B:
				SimulateButtonPress(GetNode<Button>("%B"));
				break;
			case JoyButton.X:
				SimulateButtonPress(GetNode<Button>("%X"));
				break;
			case JoyButton.Y:
				SimulateButtonPress(GetNode<Button>("%Y"));
				break;
			case JoyButton.LeftShoulder:
				SimulateButtonPress(GetNode<Button>("%LB"));
				break;
			case JoyButton.RightShoulder:
				SimulateButtonPress(GetNode<Button>("%RB"));
				break;
			case JoyButton.LeftStick:
				SimulateButtonPress(GetNode<Button>("%LStickClick"));
				break;
			case JoyButton.RightStick:
				SimulateButtonPress(GetNode<Button>("%RStickClick"));
				break;
			case JoyButton.DpadDown:
				SimulateButtonPress(GetNode<Button>("%DPADDown"));
				break;
			case JoyButton.DpadRight:
				SimulateButtonPress(GetNode<Button>("%DPADRight"));
				break;
			case JoyButton.DpadLeft:
				SimulateButtonPress(GetNode<Button>("%DPADLeft"));
				break;
			case JoyButton.DpadUp:
				SimulateButtonPress(GetNode<Button>("%DPADUp"));
				break;
			case JoyButton.Back:
				SimulateButtonPress(GetNode<Button>("%Select"));
				break;
			case JoyButton.Start:
				SimulateButtonPress(GetNode<Button>("%Start"));
				break;
			case JoyButton.Guide:
				SimulateButtonPress(GetNode<Button>("%Home"));
				break;
			case JoyButton.Misc1:
				SimulateButtonPress(GetNode<Button>("%Share"));
				break;		
		}

	}

	private void SimulateButtonPress( Button button )
	{
		button.GrabFocus();
		button.ButtonPressed = true;
		button.SetMeta("from_ui", false);

		button.EmitSignal(Button.SignalName.Pressed);
		button.SetMeta("from_ui", true);
	}

	private void OnButtonPressed()
	{
		// UPGRADE: In Godot 4.2, for-loop variables can be
		// statically typed:
		// for button:Button in button_nodes:
		foreach( Button button in ButtonNodes )
		{
			if( button.HasMeta("from_ui") && (bool)button.GetMeta("from_ui", true) == false ) return;

			if( button.ButtonPressed )
			{
				if( LastPressedButton == button )
				{
					if (Time.GetTicksMsec() < LastPressedTimestamp)
					{
					#if GODOT4_4_OR_GREATER
						EmitSignalDone();
					#else
						EmitSignal(SignalName.Done);
					#endif
					}
					else
						LastPressedTimestamp = Time.GetTicksMsec() + 1000;
				}
				else
				{
					LastPressedButton = button;
					LastPressedTimestamp = Time.GetTicksMsec() + 1000;
				}
			}

		}

	}

	private void _on_l_stick_pressed()
	{
		ButtonLabel.Text = "Axis 0/1\n(Left Stick, Joystick 0)\n[joypad/l_stick]";
	}

	private void _on_l_stick_click_pressed()
	{
		ButtonLabel.Text = "Button 7\n(Left Stick, Sony L3, Xbox L/LS)\n[joypad/l_stick_click]";
	}
	
	private void _on_r_stick_pressed()
	{
		ButtonLabel.Text = "Axis 2/3\n(Right Stick, Joystick 1)\n[joypad/r_stick]";
	}
	
	private void _on_r_stick_click_pressed()
	{
		ButtonLabel.Text = "Button 8\n(Right Stick, Sony R3, Xbox R/RS)\n[joypad/r_stick_click]";
	}
	
	private void _on_lb_pressed()
	{
		ButtonLabel.Text = "Button 9\n(Left Shoulder, Sony L1, Xbox LB)\n[joypad/lb]";
	}
	
	private void _on_lt_pressed()
	{
		ButtonLabel.Text = "Axis 4\n(Left Trigger, Sony L2, Xbox LT, Joystick 2 Right)\n[joypad/lt]";
	}
	
	private void _on_rb_pressed()
	{
		ButtonLabel.Text = "Button 10\n(Right Shoulder, Sony R1, Xbox RB)\n[joypad/rb]";
	}
	
	private void _on_rt_pressed()
	{
		ButtonLabel.Text = "Axis 5\n(Right Trigger, Sony R2, Xbox RT, Joystick 2 Down)\n[joypad/rt]";
	}
	
	private void _on_a_pressed()
	{
		ButtonLabel.Text = "Button 0\n(Bottom Action, Sony Cross, Xbox A, Nintendo B)\n[joypad/a]";
	}
	
	private void _on_b_pressed()
	{
		ButtonLabel.Text = "Button 1\n(Right Action, Sony Circle, Xbox B, Nintendo A)\n[joypad/b]";
	}
	
	private void _on_x_pressed()
	{
		ButtonLabel.Text = "Button 2\n(Left Action, Sony Square, Xbox X, Nintendo Y)\n[joypad/x]";
	}
	
	private void _on_y_pressed()
	{
		ButtonLabel.Text = "Button 3\n(Top Action, Sony Triangle, Xbox Y, Nintendo X)\n[joypad/y]";
	}
	
	private void _on_select_pressed()
	{
		ButtonLabel.Text = "Button 4\n(Back, Sony Select, Xbox Back, Nintendo -)\n[joypad/select]";
	}
	
	private void _on_start_pressed()
	{
		ButtonLabel.Text = "Button 6\n(Start, Xbox Menu, Nintendo +)\n[joypad/start]";
	}
	
	private void _on_home_pressed()
	{
		ButtonLabel.Text = "Button 5\n(Guide, Sony PS, Xbox Home)\n[joypad/home]";
	}
	
	private void _on_share_pressed()
	{
		ButtonLabel.Text = "Button 15\n(Xbox Share, PS5 Microphone, Nintendo Capture)\n[joypad/share]";
	}
	
	private void _on_dpad_pressed()
	{
		ButtonLabel.Text = "Button 11/12/13/14\n(D-pad)\n[joypad/dpad]";
	}
	
	private void _on_dpad_down_pressed()
	{
		ButtonLabel.Text = "Button 12\n(D-pad Down)\n[joypad/dpad_down]";
	}
	
	private void _on_dpad_right_pressed()
	{
		ButtonLabel.Text = "Button 14\n(D-pad Right)\n[joypad/dpad_right]";
	}
	
	private void _on_dpad_left_pressed()
	{
		ButtonLabel.Text = "Button 13\n(D-pad Left)\n[joypad/dpad_left]";
	}
	
	private void _on_dpad_up_pressed()
	{
		ButtonLabel.Text = "Button 11\n(D-pad Up)\n[joypad/dpad_up]";
	}
}
#endif