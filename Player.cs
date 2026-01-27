using Godot;
using System;

public partial class Player : CharacterBody3D
{	
	public float PixelsPerMeter = 100F;
	
	[Export]
	public float groundSpeedMetersPerSecond = 5F;
	public float groundSpeed;
	
	[Export]
	public float jumpSpeedMetersPerSecond = 20F;
	public float jumpSpeed;
	
	[Export]
	public float gravityMetersPerSecond = 9.8F;
	public float gravity;

	[Export]
	public float mouseRotationSpeed = (float)MathF.PI / 400;//0.1125F; // 90 degrees/800 pixels

	public Vector2 prevMousePosition = Vector2.Zero;
	
	public Vector3 prevCameraPosition = Vector3.Zero;
	
	[Export]
	public float cameraDistanceHorizontal = 2F;
	
	[Export]
	public float cameraDistanceVertical = 1.5F;
	
	public Vector3 velocity = Vector3.Zero;
	
	public Vector3 prevPlayerRotation = Vector3.Zero;
	
	public Vector3 playerLookDirection = Vector3.Zero;
												
	Camera3D camera;
	
	Marker3D cameraMarker;
	
	Marker3D playerLookDirection3D;

	Player()
	{
		groundSpeed = groundSpeedMetersPerSecond * PixelsPerMeter;
		
		jumpSpeed = jumpSpeedMetersPerSecond * PixelsPerMeter;
		
		gravity = gravityMetersPerSecond * PixelsPerMeter;
		
		prevPlayerRotation = Rotation;
		
		playerLookDirection = new Vector3(1,0,0);
	}
	
	public override void _Ready()
	{		
		camera = GetNode<Camera3D>("Marker3D/Camera3D");
		camera.Position = new Vector3 (-cameraDistanceHorizontal, cameraDistanceVertical, 0);
		
		cameraMarker = GetNode<Marker3D>("Marker3D");
		
		playerLookDirection3D = GetNode<Marker3D>("PlayerLookDirection");
		
		prevCameraPosition = camera.Position;
		
		prevMousePosition = GetViewport().GetMousePosition();
	}
	
	public override void _PhysicsProcess(double delta)
	{
		applyGravity(delta);
		
		updatePlayerPerspective();
		
		getPlayerInput(delta);
		//camera.RotationDegrees = new Vector3();
		MoveAndSlide();
	}
	
	private void getPlayerInput(double delta)
	{	
		//GD.Print($"X Component: {playerLookDirection3D.GlobalPosition.X - Position.X}, Z Component: {playerLookDirection3D.GlobalPosition.Z - Position.Z}");
		if (Input.IsActionPressed("Forward"))
		{
			velocity.X += (playerLookDirection3D.GlobalPosition.X - Position.X) * groundSpeed * (float)delta;
			velocity.Z += (playerLookDirection3D.GlobalPosition.Z - Position.Z) * groundSpeed * (float)delta;
		}
		if (Input.IsActionPressed("Backward"))
		{
			velocity.X += -(playerLookDirection3D.GlobalPosition.X - Position.X) * groundSpeed * (float)delta;
			velocity.Z += -(playerLookDirection3D.GlobalPosition.Z - Position.Z) * groundSpeed * (float)delta;
		}
		if (Input.IsActionPressed("Left"))
		{
			velocity.X += (playerLookDirection3D.GlobalPosition.Z - Position.Z) * groundSpeed * (float)delta;
			velocity.Z += -(playerLookDirection3D.GlobalPosition.X - Position.X) * groundSpeed * (float)delta;
 		}
		if (Input.IsActionPressed("Right"))
		{
			velocity.X += -(playerLookDirection3D.GlobalPosition.Z - Position.Z) * groundSpeed * (float)delta;
			velocity.Z += (playerLookDirection3D.GlobalPosition.X - Position.X) * groundSpeed * (float)delta;
		}
		
		Vector3 velocityBefore = velocity;
		GD.Print("_________________________________");
		GD.Print($"Player Look Direciton: {playerLookDirection3D.GlobalRotation.Y}");
		GD.Print($"Velocity Before: {velocity}");
		GD.Print($"Speed Before: {MathF.Sqrt(MathF.Pow(velocity.X, 2.0F) + MathF.Pow(velocity.Z, 2.0F))}");
		 //this is a bit problematic - while it does bring the velocity closer to the intended velocity, its fucking up somehow
		if (velocity.X != 0 && velocity.Z != 0 )
		{
			float normalizeCoefficient = (groundSpeed * (float)delta) / MathF.Sqrt(MathF.Pow(velocity.X, 2.0F) + MathF.Pow(velocity.Z, 2.0F));
			velocity.X = velocity.X * normalizeCoefficient; 
			velocity.Z = velocity.Z * normalizeCoefficient;
		}
		if (Input.IsActionPressed("Jump")) //&& IsOnFloor()
		{
			velocity.Y = jumpSpeed * (float)delta;
		}

		Velocity = velocity;
		
		Vector3 velocityAfter = velocity;
		
		//GD.Print($"Velocity Ratio: {velocityBefore/velocityAfter}");
		
		GD.Print($"Look Direction Distance: {(float)Math.Sqrt((double)(playerLookDirection.X * playerLookDirection.X + playerLookDirection.Z * playerLookDirection.Z))}");
		GD.Print($"Velocity After: {velocity}");
		GD.Print($"Speed After: {MathF.Sqrt(MathF.Pow(velocity.X, 2.0F) + MathF.Pow(velocity.Z, 2.0F))}");
		GD.Print("_________________________________");
		velocity = Vector3.Zero;
	}
	
	private void updatePlayerPerspective()
	{
		Vector2 currentMousePosition = GetViewport().GetMousePosition();
		Vector2 mouseDelta = currentMousePosition - prevMousePosition;
		prevMousePosition = currentMousePosition;
		
		// rotate the entire Player instance
		Vector3 currentPlayerRotationRadians = Rotation;
		float mouseAngleDeltaHorizontal = -mouseDelta.X * mouseRotationSpeed;
		currentPlayerRotationRadians.Y += mouseAngleDeltaHorizontal;
		Rotation = currentPlayerRotationRadians;
		
		// rotate and move the camera
		// add a spherical marker on the player, and use that as a reference to rotate around
		Vector3 cameraMarkerRotationRadians = cameraMarker.Rotation;
		float mouseAngleDeltaVertical = -mouseDelta.Y * mouseRotationSpeed;
		cameraMarkerRotationRadians.Z += mouseAngleDeltaVertical;
		cameraMarker.Rotation = cameraMarkerRotationRadians;
		
	}
	
	private void applyGravity(double delta)
	{
		velocity.Y -= gravity * (float)delta;
	}
	
	
}
