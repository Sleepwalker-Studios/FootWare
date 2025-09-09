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
	
	private CharacterBody2D _capturedPlayer = null;
	private bool _isPlayerCaptured = false;
	private const float CAPTURE_DISTANCE = 40f;
	
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
			GD.Print("Footware lifetime expired, destroying.");
			
			if (_isPlayerCaptured && IsInstanceValid(_capturedPlayer) && _capturedPlayer is CharacterBody2d playerScript)
			{
				playerScript.IsCaptured = false; 
				
				var originalParent = GetParent();
				_capturedPlayer.Reparent(originalParent); // Give player back to the main scene.
				GD.Print("Released captured player.");
			}

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
		// This method correctly sets everything from the config object.
		GlobalPosition = config.StartPosition;
		direction = config.Direction.Normalized();
		Speed = config.Speed;
		Pull = config.Pull;
		Damage = config.Damage;
		Lifetime = config.Lifetime;
		DestroyOnHit = config.DestroyOnHit;
		PullRadius = config.PullRadius;
		PullForce = config.PullForce;
		currentLifetime = Lifetime; 
	}


	public void SetDirection(Vector2 newDirection)
	{
		direction = newDirection.Normalized();
		GD.Print($"Direction set to: {direction}");
	}

	private void HandlePullBehavior(double delta)
	{
		// If the player is already captured, we don't need to do anything else.
		// The parenting system will handle the movement.
		if (_isPlayerCaptured)
		{
			return;
		}

		// --- Part 1: Find a Player to Pull ---
		if (_capturedPlayer == null)
		{
			// Use GetOverlappingBodies to find potential targets.
			var bodies = GetOverlappingBodies();
			foreach (var body in bodies)
			{
				if (body is CharacterBody2D player && player.IsInGroup("player"))
				{
					_capturedPlayer = player;
					GD.Print($"Found player '{player.Name}' to pull.");
					break; // Found our target, no need to look further.
				}
			}
		}

		// --- Part 2: Pull the Player Towards the Center ---
		if (_capturedPlayer is CharacterBody2d playerScript)
		{
			var distanceToCenter = GlobalPosition.DistanceTo(_capturedPlayer.GlobalPosition);

			// If the player is close enough, capture them.
			if (distanceToCenter <= CAPTURE_DISTANCE)
			{
				playerScript.IsCaptured = true; 
				GD.Print("Player is close enough. Capturing!");
				_isPlayerCaptured = true;
				_capturedPlayer.Reparent(this); // Make the player a child of this pull area.
				_capturedPlayer.GlobalPosition = GlobalPosition; // Snap to the center.
			}
			else // Otherwise, keep pulling them in.
			{
				// Use Vector2.MoveToward for smooth, frame-rate independent movement.
				var newPosition = _capturedPlayer.GlobalPosition.MoveToward(GlobalPosition, PullForce * (float)delta);
				_capturedPlayer.GlobalPosition = newPosition;
				GD.Print($"Pulling player. Distance: {distanceToCenter}");
			}
		}
	}


	private void OnBodyEntered(Node2D body)
	{
		GD.Print($"Body entered: {body.Name}");
		
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
