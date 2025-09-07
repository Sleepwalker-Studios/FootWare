using Godot;
using System;

public partial class FootwareTemplate : Area2D
{
	[Export] public float Speed { get; set; } = 0f;
	[Export] public bool Pull { get; set; } = false;
	[Export] public int Damage { get; set; } = 10;
	[Export] public float Lifetime { get; set; } = 5f;
	[Export] public bool DestroyOnHit { get; set; } = true;
	[Export] public float PullRadius { get; set; } = 100f;
	[Export] public float PullForce { get; set; } = 200f;
	
	private Vector2 direction = Vector2.Zero;
	private float currentLifetime;
	private CollisionShape2D pullDetectionArea;
	private Godot.Collections.Array<Node2D> nearbyTargets = new();

	public override void _Ready()
	{
		currentLifetime = Lifetime;
		
		// Connect collision signals
		AreaEntered += OnAreaEntered;
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;
		
		// Set up pull detection if needed
		if (Pull)
		{
			SetupPullDetection();
		}
		
		// Debug output
		GD.Print($"FootwareTemplate ready - Speed: {Speed}, Pull: {Pull}, Lifetime: {Lifetime}");
	}
	
	private void SetupPullDetection()
	{
		// Create a larger detection area for pulling
		pullDetectionArea = new CollisionShape2D();
		var circleShape = new CircleShape2D();
		circleShape.Radius = PullRadius;
		pullDetectionArea.Shape = circleShape;
		pullDetectionArea.Name = "PullDetection";
		AddChild(pullDetectionArea);
		
		GD.Print($"Pull detection setup with radius: {PullRadius}");
	}

	public override void _Process(double delta)
	{
		// Handle movement
		if (Speed > 0)
		{
			Vector2 movement = direction * Speed * (float)delta;
			GlobalPosition += movement;
			// Debug output for first few frames
			if (currentLifetime > Lifetime - 0.5f)
			{
				GD.Print($"Moving: {movement}, New Position: {GlobalPosition}");
			}
		}
		
		// Handle lifetime
		currentLifetime -= (float)delta;
		if (currentLifetime <= 0)
		{
			GD.Print("Footware lifetime expired, destroying");
			QueueFree();
			return;
		}
		
		// Handle pull behavior
		if (Pull)
		{
			HandlePullBehavior(delta);
		}
	}

	public void SetupFootware(FootwareConfig config)
	{
		direction = config.Direction.Normalized();
		Speed = config.Speed;
		Pull = config.Pull;
		Damage = config.Damage;
		Lifetime = config.Lifetime;
		DestroyOnHit = config.DestroyOnHit;
		PullRadius = config.PullRadius;
		PullForce = config.PullForce;
		currentLifetime = Lifetime;
		
		GD.Print($"SetupFootware called - Speed: {Speed}, Direction: {direction}, Pull: {Pull}");
		
		// Update pull detection if it exists
		if (Pull && pullDetectionArea != null)
		{
			if (pullDetectionArea.Shape is CircleShape2D circle)
			{
				circle.Radius = PullRadius;
			}
		}
	}

	public void SetDirection(Vector2 newDirection)
	{
		direction = newDirection.Normalized();
		GD.Print($"Direction set to: {direction}");
	}

	private void HandlePullBehavior(double delta)
	{
		var spaceState = GetWorld2D().DirectSpaceState;
		var query = new PhysicsShapeQueryParameters2D();
		
		var circleShape = new CircleShape2D();
		circleShape.Radius = PullRadius;
		query.Shape = circleShape;
		query.Transform = Transform2D.Identity.Translated(GlobalPosition);
		query.CollisionMask = 1; // Player layer
		
		var results = spaceState.IntersectShape(query);
		
		foreach (var result in results)
		{
			var body = result["collider"].As<Node2D>();
			// FIX: Convert StringName to string before calling ToLower()
			if (body != null && (body.IsInGroup("player") || body.Name.ToString().ToLower().Contains("character")))
			{
				var distance = GlobalPosition.DistanceTo(body.GlobalPosition);
				if (distance > 5f) // Avoid pulling when too close
				{
					var pullDirection = (GlobalPosition - body.GlobalPosition).Normalized();
					
					if (body is CharacterBody2D characterBody)
					{
						var pullVelocity = pullDirection * PullForce * (float)delta;
						characterBody.Velocity += pullVelocity;
						GD.Print($"Pulling {body.Name} with force: {pullVelocity}");
					}
				}
			}
		}
	}

	private void OnBodyEntered(Node2D body)
	{
		GD.Print($"Body entered: {body.Name}");
		
		// FIX: Convert StringName to string before calling ToLower()
		if (Pull && (body.IsInGroup("player") || body.Name.ToString().ToLower().Contains("character")))
		{
			nearbyTargets.Add(body);
			GD.Print($"Added {body.Name} to pull targets");
		}
		else if (Speed > 0) // Regular projectile behavior
		{
			if (body.HasMethod("TakeDamage"))
			{
				body.Call("TakeDamage", Damage);
			}
			
			if (DestroyOnHit)
			{
				QueueFree();
			}
		}
	}
	
	private void OnBodyExited(Node2D body)
	{
		if (nearbyTargets.Contains(body))
		{
			nearbyTargets.Remove(body);
			GD.Print($"Removed {body.Name} from pull targets");
		}
	}

	private void OnAreaEntered(Area2D area)
	{
		GD.Print($"Area entered: {area.Name}");
		
		if (area.HasMethod("TakeDamage"))
		{
			area.Call("TakeDamage", Damage);
		}
		
		if (Speed > 0 && DestroyOnHit)
		{
			QueueFree();
		}
	}
}

// Configuration class for easier parameter passing
public class FootwareConfig
{
	public Vector2 StartPosition { get; set; }
	public Vector2 Direction { get; set; }
	public float Speed { get; set; } = 0f;
	public bool Pull { get; set; } = false;
	public int Damage { get; set; } = 10;
	public float Lifetime { get; set; } = 5f;
	public bool DestroyOnHit { get; set; } = true;
	public float PullRadius { get; set; } = 100f;
	public float PullForce { get; set; } = 200f;
}
