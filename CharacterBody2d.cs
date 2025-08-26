using Godot;
using System;
using System.Collections.Generic;

public partial class CharacterBody2d : CharacterBody2D
{
	public Dictionary<footware, int> footwarespeeds = new Dictionary<footware, int>() {
		{ footware.none, 400 },
		{ footware.test, 800 },
	};
	
	public Panel panel;
	public TextureButton test;
	public Camera2D camera;
	public VScrollBar speedbar;
	public VScrollBar jumpbar;
	public VScrollBar gravbar;
	public VScrollBar djbar;
	public VScrollBar djtbar;
	public Label speedlabel;
	public Label gravlabel;
	public Label jumplabel;
	public Label djlabel;
	public Label djtlabel;
	public Timer coyote;
	public Timer buffer;
	
	public enum footware{none, test};
	
	public footware current;
	
	public int speed = 400;
	public int defaultspeed = 400;
	public int gravityspeed = 1300;
	public int jumpspeed = -400;
	public int downspeed = 800;
	public int doublejump = 300;
	public int state = 1;
	public int jumpboost = 30;
	
	public double jtimer;
	public double djt;
	public double djtimer;
	
	public bool bufferbool;
	public bool coyotebool;
	public bool left;
	public bool jumping;
	public bool jumpingbool;
	public bool flipped;
	public bool paused;
	
	public override void _Ready() {
		panel = GetParent().GetNode<Panel>("CanvasLayer/Panel");
		test = GetParent().GetNode<TextureButton>("CanvasLayer/Panel/TestFootWare");
		camera = GetNode<Camera2D>("Camera2D");
		speedbar = GetNode<VScrollBar>("SpeedBar");
		gravbar = GetNode<VScrollBar>("GravBar");
		jumpbar = GetNode<VScrollBar>("JumpBar");
		djbar = GetNode<VScrollBar>("DJBar");
		djtbar = GetNode<VScrollBar>("DJTBar");
		speedlabel = GetNode<Label>("SpeedBar/Label");
		gravlabel = GetNode<Label>("GravBar/Label");
		jumplabel = GetNode<Label>("JumpBar/Label");
		djlabel = GetNode<Label>("DJBar/Label");
		djtlabel = GetNode<Label>("DJTBar/Label");
		coyote = GetParent().GetNode<Timer>("Coyote");
		buffer = GetParent().GetNode<Timer>("Buffer");
		current = footware.none;
	}
	
	public override void _PhysicsProcess(double delta) {
		
		GD.Print(coyotebool);
		
		speed = (int)speedbar.Value;
		gravityspeed = (int)gravbar.Value;
		jumpspeed = (int)jumpbar.Value;
		doublejump = (int)djbar.Value;
		djt = djtbar.Value;
		speedlabel.Text = ((int)speedbar.Value).ToString();
		gravlabel.Text = ((int)gravbar.Value).ToString();
		jumplabel.Text = ((int)jumpbar.Value).ToString();
		djlabel.Text = ((int)djbar.Value).ToString();
		djtlabel.Text = (djtbar.Value).ToString();
		
		if (Input.IsActionPressed("ui_z")) {
			camera.Zoom += new Vector2(-0.01f, -0.01f);
		}
		if (Input.IsActionPressed("ui_x")) {
			camera.Zoom += new Vector2(0.01f, 0.01f);
		}
		
		if(!IsOnFloor() && !left && !jumping) {
			left = true;
			coyote.Start();
			coyotebool = true;
		}
		
		if(IsOnFloor()) {
			left = false;
			jumping = false;
		}


		Vector2 velocity = Velocity;
		if(paused && IsOnFloor()) {
			velocity = Vector2.Zero;
		}
		if(!paused) {
			Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
			if(IsOnFloor()){
				if(Input.IsActionPressed("shift")){
					velocity.X = direction.X * speed/3;
				}
				else {
					velocity.X = direction.X * speed;
				}
			}
			else{
				velocity.X += direction.X * (speed/30);
				if(velocity.X >= 400 || velocity.X <= -400){
					if(velocity.X > 0){
						velocity.X = 400;
					}
					else{
						velocity.X = -400;
					}
				}
			}
			
		}	
		if(!IsOnFloor()){
			velocity.Y += gravityspeed * (float)delta;
		}
		else{
			velocity.Y = 0;
		}
		if((IsOnFloor() || coyotebool) && (Input.IsActionJustPressed("ui_jump") || (bufferbool && Input.IsActionPressed("ui_jump"))) && !paused && !jumping){
			flipped = false;
			bufferbool = false;
			jumping = true;
			jumpingbool = true;
			jtimer = 0.2;
			velocity.Y = jumpspeed;
		}
		if(!IsOnFloor() && Input.IsActionJustPressed("ui_jump")) {
			buffer.Start();
			bufferbool = true;
		}
		
		if(!IsOnFloor() && Input.IsActionJustPressed("ui_up") && !flipped && !paused){
			djtimer = djt;
			flipped = true;
		}
		if(jumpingbool && Input.IsActionPressed("ui_jump")) {
			velocity.Y -= jumpboost;
		}
		if(jtimer > 0.0) {
			jtimer -= delta;
		}
		else {
			jtimer = 0.0;
			jumpingbool = false;
		}
		if(djtimer > 0.0) {
			velocity.Y /= doublejump;
			djtimer -= delta;
		}
		else {
			djtimer = 0.0;
		}
		
		if(!IsOnFloor() && Input.IsActionPressed("ui_down") && !paused){
			velocity.Y += downspeed * (float)delta;
		}
		Velocity = velocity;
		MoveAndSlide();

		
		if(Input.IsActionJustPressed("e")){
			togglemenu();
		}
	}
	
	public void togglemenu(){
		if(panel.Visible == true){
			paused = false;
			Engine.TimeScale = 1.0f;
			state = 1;
		}
		else {
			paused = true;
			Engine.TimeScale = 0.2f;
			state = 2;
		}
		panel.Visible = !panel.Visible;
	}
	
	public void updatefootware(){
		speed = footwarespeeds[current];
	}
	
	public void _on_test_foot_ware_pressed(){
		current = footware.test;
		GD.Print("yo");
		updatefootware();
		
	}
	
	public void _on_coyote_timeout() {
		coyotebool = false;
	}
	
	public void _on_buffer_timeout() {
		bufferbool = false;
	}
}
