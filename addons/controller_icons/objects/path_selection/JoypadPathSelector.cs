
using Godot;
using System;
using System.Collections.Generic;
using static ControllerIcons;

[Tool]
public partial class JoypadPathSelector : Panel
{
	[Signal]
	private delegate void DoneEventHandler();

    private Label n_button_label;
    private Godot.Collections.Array<Button> button_nodes;

    private Button _last_pressed_button;
    private ulong _last_pressed_timestamp;

    public override void _Ready()
	{
        n_button_label = GetNode<Label>("%ButtonLabel");
        button_nodes = [
			GetNode<Button>("%LT"), GetNode<Button>("%RT"),
			GetNode<Button>("%LStick"), GetNode<Button>("%RStick"),
			GetNode<Button>("%LStickClick"), GetNode<Button>("%RStickClick"),
			GetNode<Button>("%LB"), GetNode<Button>("%RB"), GetNode<Button>("%A"), GetNode<Button>("%B"), GetNode<Button>("%X"), GetNode<Button>("%Y"),
			GetNode<Button>("%Select"), GetNode<Button>("%Start"),
			GetNode<Button>("%Home"), GetNode<Button>("%Share"), GetNode<Button>("%DPAD"),
			GetNode<Button>("%DPADDown"), GetNode<Button>("%DPADRight"),
			GetNode<Button>("%DPADLeft"), GetNode<Button>("%DPADUp")
		];	
    }

    public void populate( EditorInterface editor_interface )
	{
		// UPGRADE: In Godot 4.2, for-loop variables can be
		// statically typed:
		// for button:Button in button_nodes:
		foreach( Button button in button_nodes )
            button.ButtonPressed = false;
    }

	public string get_icon_path()
	{
		// UPGRADE: In Godot 4.2, for-loop variables can be
		// statically typed:
		// for button:Button in button_nodes:
		foreach( Button button in button_nodes )
		{
			if( button.ButtonPressed )
				return ( button.Icon as ControllerIconTexture ).path;
        }

		return "";
	}

	public void grab_focus()
	{
		//do nothing
	}

	public override void _Input( InputEvent e )
	{
		if( !Visible ) return;

		if( e is InputEventJoypadMotion motionEvent )
			_input_motion(motionEvent);
		else if( e is InputEventJoypadButton buttonEvent )
			_input_button(buttonEvent);
	}

	private void _input_motion(InputEventJoypadMotion e )
	{
		if( Mathf.Abs(e.AxisValue) < 0.5f ) return;

		switch( e.Axis )
		{
			case JoyAxis.LeftX:
			case JoyAxis.LeftY:
				_simulate_button_press(GetNode<Button>("%LStick"));
				break;
			case JoyAxis.RightX:
			case JoyAxis.RightY:
				_simulate_button_press(GetNode<Button>("%RStick"));
				break;
			case JoyAxis.TriggerLeft:
				_simulate_button_press(GetNode<Button>("%LT"));
				break;
			case JoyAxis.TriggerRight:
				_simulate_button_press(GetNode<Button>("%RT"));
				break;
		}
	}

	private void _input_button( InputEventJoypadButton e )
	{
		if( !e.Pressed ) return;

		switch( e.ButtonIndex )
		{
			case JoyButton.A:
				_simulate_button_press(GetNode<Button>("%A"));
				break;
			case JoyButton.B:
				_simulate_button_press(GetNode<Button>("%B"));
				break;
			case JoyButton.X:
				_simulate_button_press(GetNode<Button>("%X"));
				break;
			case JoyButton.Y:
				_simulate_button_press(GetNode<Button>("%Y"));
				break;
			case JoyButton.LeftShoulder:
				_simulate_button_press(GetNode<Button>("%LB"));
				break;
			case JoyButton.RightShoulder:
				_simulate_button_press(GetNode<Button>("%RB"));
				break;
			case JoyButton.LeftStick:
				_simulate_button_press(GetNode<Button>("%LStickClick"));
				break;
			case JoyButton.RightStick:
				_simulate_button_press(GetNode<Button>("%RStickClick"));
				break;
			case JoyButton.DpadDown:
				_simulate_button_press(GetNode<Button>("%DPADDown"));
				break;
			case JoyButton.DpadRight:
				_simulate_button_press(GetNode<Button>("%DPADRight"));
				break;
			case JoyButton.DpadLeft:
				_simulate_button_press(GetNode<Button>("%DPADLeft"));
				break;
			case JoyButton.DpadUp:
				_simulate_button_press(GetNode<Button>("%DPADUp"));
				break;
			case JoyButton.Back:
				_simulate_button_press(GetNode<Button>("%Select"));
				break;
			case JoyButton.Start:
				_simulate_button_press(GetNode<Button>("%Start"));
				break;
			case JoyButton.Guide:
				_simulate_button_press(GetNode<Button>("%Home"));
				break;
			case JoyButton.Misc1:
				_simulate_button_press(GetNode<Button>("%Share"));
				break;		
		}

	}

	private void _simulate_button_press( Button button )
	{
		button.GrabFocus();
		button.ButtonPressed = true;
		button.SetMeta("from_ui", false);

		button.EmitSignal(Button.SignalName.Pressed);
		button.SetMeta("from_ui", true);
	}

	private void _on_button_pressed()
	{
		// UPGRADE: In Godot 4.2, for-loop variables can be
		// statically typed:
		// for button:Button in button_nodes:
		foreach( Button button in button_nodes )
		{
			if( button.HasMeta("from_ui") && (bool)button.GetMeta("from_ui", true) == false ) return;

			if( button.ButtonPressed )
			{
				if( _last_pressed_button == button )
				{
					if( Time.GetTicksMsec() < _last_pressed_timestamp )
						EmitSignalDone();
					else
						_last_pressed_timestamp = Time.GetTicksMsec() + 1000;
				}
				else
				{
					_last_pressed_button = button;
					_last_pressed_timestamp = Time.GetTicksMsec() + 1000;
				}
			}

		}

	}

	private void _on_l_stick_pressed()
	{
		n_button_label.Text = "Axis 0/1\n(Left Stick, Joystick 0)\n[joypad/l_stick]";
	}

	private void _on_l_stick_click_pressed()
	{
		n_button_label.Text = "Button 7\n(Left Stick, Sony L3, Xbox L/LS)\n[joypad/l_stick_click]";
	}
	
	private void _on_r_stick_pressed()
	{
		n_button_label.Text = "Axis 2/3\n(Right Stick, Joystick 1)\n[joypad/r_stick]";
	}
	
	private void _on_r_stick_click_pressed()
	{
		n_button_label.Text = "Button 8\n(Right Stick, Sony R3, Xbox R/RS)\n[joypad/r_stick_click]";
	}
	
	private void _on_lb_pressed()
	{
		n_button_label.Text = "Button 9\n(Left Shoulder, Sony L1, Xbox LB)\n[joypad/lb]";
	}
	
	private void _on_lt_pressed()
	{
		n_button_label.Text = "Axis 4\n(Left Trigger, Sony L2, Xbox LT, Joystick 2 Right)\n[joypad/lt]";
	}
	
	private void _on_rb_pressed()
	{
		n_button_label.Text = "Button 10\n(Right Shoulder, Sony R1, Xbox RB)\n[joypad/rb]";
	}
	
	private void _on_rt_pressed()
	{
		n_button_label.Text = "Axis 5\n(Right Trigger, Sony R2, Xbox RT, Joystick 2 Down)\n[joypad/rt]";
	}
	
	private void _on_a_pressed()
	{
		n_button_label.Text = "Button 0\n(Bottom Action, Sony Cross, Xbox A, Nintendo B)\n[joypad/a]";
	}
	
	private void _on_b_pressed()
	{
		n_button_label.Text = "Button 1\n(Right Action, Sony Circle, Xbox B, Nintendo A)\n[joypad/b]";
	}
	
	private void _on_x_pressed()
	{
		n_button_label.Text = "Button 2\n(Left Action, Sony Square, Xbox X, Nintendo Y)\n[joypad/x]";
	}
	
	private void _on_y_pressed()
	{
		n_button_label.Text = "Button 3\n(Top Action, Sony Triangle, Xbox Y, Nintendo X)\n[joypad/y]";
	}
	
	private void _on_select_pressed()
	{
		n_button_label.Text = "Button 4\n(Back, Sony Select, Xbox Back, Nintendo -)\n[joypad/select]";
	}
	
	private void _on_start_pressed()
	{
		n_button_label.Text = "Button 6\n(Start, Xbox Menu, Nintendo +)\n[joypad/start]";
	}
	
	private void _on_home_pressed()
	{
		n_button_label.Text = "Button 5\n(Guide, Sony PS, Xbox Home)\n[joypad/home]";
	}
	
	private void _on_share_pressed()
	{
		n_button_label.Text = "Button 15\n(Xbox Share, PS5 Microphone, Nintendo Capture)\n[joypad/share]";
	}
	
	private void _on_dpad_pressed()
	{
		n_button_label.Text = "Button 11/12/13/14\n(D-pad)\n[joypad/dpad]";
	}
	
	private void _on_dpad_down_pressed()
	{
		n_button_label.Text = "Button 12\n(D-pad Down)\n[joypad/dpad_down]";
	}
	
	private void _on_dpad_right_pressed()
	{
		n_button_label.Text = "Button 14\n(D-pad Right)\n[joypad/dpad_right]";
	}
	
	private void _on_dpad_left_pressed()
	{
		n_button_label.Text = "Button 13\n(D-pad Left)\n[joypad/dpad_left]";
	}
	
	private void _on_dpad_up_pressed()
	{
		n_button_label.Text = "Button 11\n(D-pad Up)\n[joypad/dpad_up]";
	}
}
