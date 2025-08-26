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
	
	public enum footware{none, test};
	
	public footware current;
	
	public int speed = 400;
	public int defaultspeed = 400;
	public int gravityspeed = 1300;
	public int jumpspeed = -500;
	public int downspeed = 800;
	public int state = 1;
	
	public bool flipped;
	public bool paused;
	
	public override void _Ready() {
		panel = GetParent().GetNode<Panel>("CanvasLayer/Panel");
		test = GetParent().GetNode<TextureButton>("CanvasLayer/Panel/TestFootWare");
		current = footware.none;
	}
	
	public override void _PhysicsProcess(double delta) {

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
		if(IsOnFloor() && Input.IsActionPressed("ui_jump") && !paused){
			flipped = false;
			velocity.Y = jumpspeed;
		}
		
		if(!IsOnFloor() && Input.IsActionJustPressed("ui_up") && !flipped && !paused){
			velocity.Y += jumpspeed - 200;
			flipped = true;
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
}
