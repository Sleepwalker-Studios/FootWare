using Godot;

public partial class FootwareManager : Node
{
	[Export]
	public PackedScene FootwareTemplateScene { get; set; }

	// This method now only needs the config object, which contains all data.
	public void SpawnFootware(FootwareConfig config)
	{
		if (FootwareTemplateScene == null) 
		{
			GD.PrintErr("Footware Manager: FootwareTemplateScene is not set in the inspector!");
			return;
		}

		// 1. Instantiate the scene
		var instance = FootwareTemplateScene.Instantiate<FootwareTemplate>();

		// 2. Set up ALL properties from the config BEFORE adding to the scene.
		// This single method call should set position, direction, speed, etc.
		instance.SetupFootware(config); 

		// 3. Add the fully configured node to the scene tree.
		GetTree().CurrentScene.AddChild(instance);

		GD.Print($"Spawned footware at {instance.GlobalPosition} with Speed: {instance.Speed}");
	}
}
