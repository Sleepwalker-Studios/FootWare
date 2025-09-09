using Godot;
using System;
using System.Collections.Generic;

public partial class CharacterBody2d : CharacterBody2D
{
	
	public bool IsCaptured { get; set; } = false;
	
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
	
	private FootwareManager _footwareManager;
	
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
		_footwareManager = GetNode<FootwareManager>("/root/FootwareManager");
	}
	
	public override void _PhysicsProcess(double delta) {
		
		if(IsCaptured){
			return;
		}
		
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
	
	public void _on_coyote_timeout() {
		coyotebool = false;
	}
	
	public void _on_buffer_timeout() {
		bufferbool = false;
	}
	
	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("shoot"))
		{
			ShootBullet();
		}
		else if (@event.IsActionPressed("kick"))
		{
			PerformKick();
		}
		else if (@event.IsActionPressed("pull"))
		{
			CreatePullEffect();
		}
	}
	

	private void ShootBullet()
	{
		var mousePos = GetGlobalMousePosition();
		var direction = (mousePos - GlobalPosition).Normalized();
		
		var config = new FootwareConfig
		{
			StartPosition = GlobalPosition, // The bullet starts at the player's position
			Direction = direction,
			Speed = 500f,
			Lifetime = 20f,
			DestroyOnHit = false,
		};
		
		_footwareManager.SpawnFootware(config);
	}

	private void PerformKick()
	{
		var mousePos = GetGlobalMousePosition();
		var direction = (mousePos - GlobalPosition).Normalized();
		
		var config = new FootwareConfig
		{
			StartPosition = GlobalPosition, // The kick originates from the player
			Direction = direction,
			Speed = 1400f,
			Lifetime = 0.1f,
			DestroyOnHit = false,
		};
		_footwareManager.SpawnFootware(config);
	}

	private void CreatePullEffect()
	{
		var mousePos = GetGlobalMousePosition();
		var direction = (mousePos - GlobalPosition).Normalized();
		
		var config = new FootwareConfig
		{
			StartPosition = GlobalPosition, // The effect is centered on the player
			Direction = direction,
			Speed = 700f,
			Lifetime = 1f,
			Pull = true,
			DestroyOnHit = false,
		};
		_footwareManager.SpawnFootware(config);
	}

	
	[Export]
	public PackedScene FootwareTemplateScene { get; set; }

	private void CreateFootwareWithConfig(FootwareConfig config)
	{
		if (FootwareTemplateScene == null) 
		{
			GD.PrintErr("FootwareTemplateScene is not assigned in the inspector!");
			return;
		}
		
		// 1. Instantiate the scene
		var footware = FootwareTemplateScene.Instantiate<FootwareTemplate>();
		
		// 2. Set ALL properties BEFORE adding to the scene tree
		footware.GlobalPosition = config.StartPosition;
		footware.SetupFootware(config);
		
		// 3. Add the fully configured node to the scene
		GetTree().CurrentScene.AddChild(footware);
		
		// Debug output to check values
		GD.Print($"Spawned footware - Speed: {config.Speed}, Direction: {config.Direction}, Position: {config.StartPosition}");
	}


}
