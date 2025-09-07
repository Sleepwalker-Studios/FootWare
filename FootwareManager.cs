using Godot;

public partial class FootwareManager : Node
{
	[Export]
	public PackedScene FootwareTemplateScene { get; set; }
	
	public void SpawnFootware(Vector2 position, Vector2 direction, FootwareConfig config)
	{
		if (FootwareTemplateScene == null) return;
		
		var instance = FootwareTemplateScene.Instantiate<FootwareTemplate>();
		instance.SetupFootware(config);
		GetTree().CurrentScene.AddChild(instance);
		instance.GlobalPosition = position;
		instance.SetDirection(direction);
	}
}
