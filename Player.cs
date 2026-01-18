using Godot;
using System;

public partial class Player : CharacterBody3D
{
	[Export]
	public float groundSpeed {get; set;} = 500F;
	
	[Export]
	public float jumpSpeed {get; set;} = 200F;
	
	[Export]
	public float gravity {get; set;} = 5F;

	[Export]
	public float mouseRotationSpeed = 0.2F;
	
	private Vector3 velocity = Vector3.Zero;
	
	private Vector2 prevMousePosition = Vector2.Zero;
	
	Camera3D camera;
	
	public Player()
	{
		//prevMousePosition = GetViewport().GetMousePosition();
		//camera = GetNode<Camera3D>("Marker3D/Camera3D");
	}
	
	public override void _PhysicsProcess(double delta)
	{
		if (Input.IsActionPressed("Forward"))
		{
			GD.Print("Pressing W");
			velocity.X = groundSpeed * (float)delta;
		}
		if (Input.IsActionPressed("Backward"))
		{
			velocity.X = -groundSpeed * (float)delta;
		}
		if (Input.IsActionPressed("Left"))
		{
			velocity.Z = -groundSpeed * (float)delta;
		}
		if (Input.IsActionPressed("Right"))
		{
			velocity.Z = groundSpeed * (float)delta;
		}
		if (Input.IsActionPressed("Jump") && IsOnFloor())
		{
			velocity.Y = jumpSpeed * (float)delta;
		}
		
		velocity.Y -= gravity * (float)delta;
		
		Vector2 currentMousePosition = GetViewport().GetMousePosition();
		GD.Print($"Mouse Position: {currentMousePosition.X}");
		
		Vector2 mouseDelta = currentMousePosition - prevMousePosition;
		
		//float currentCameraAngleY = camera.GetRotationDegrees();
		float cameraAngleDeltaY = (-mouseDelta.X/mouseRotationSpeed);
		camera = GetNode<Camera3D>("Marker3D/Camera3D");
		camera.RotationDegrees = new Vector3(camera.RotationDegrees.X, cameraAngleDeltaY + camera.RotationDegrees.Y, camera.RotationDegrees.Z);
		
		prevMousePosition = currentMousePosition;
		//camera.RotationDegrees = new Vector3();
		GD.Print($"Velocity: {velocity}");
		Velocity = velocity;
		velocity = Vector3.Zero;
		MoveAndSlide();
	}
	
	private void applyGravity()
	{
		return;
	}
	
	
}
