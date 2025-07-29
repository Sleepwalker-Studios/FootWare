using Godot;
using System;

public partial class TestFootWare : TextureButton
{
	public CharacterBody2d wendy;
	public override void _Ready() {
		wendy = GetParent().GetParent().GetParent().GetNode<CharacterBody2d>("Wendy");
		this.Pressed += OnPressed;
		GD.Print("yo");
	}
	
	private void OnPressed(){
		wendy.current = CharacterBody2d.footware.test;
		wendy.updatefootware();
		GD.Print("yo");
	}
}
