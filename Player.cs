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
	public float mouseRotationSpeed = 2 * 90 * (float)Math.PI / 180 / 800;//0.1125F; // 90 degrees/800 pixels
	
	public Vector3 velocity = Vector3.Zero;

	public Vector2 prevMousePosition = Vector2.Zero;
	
	public Vector3 prevCameraPosition = Vector3.Zero;
	
	[Export]
	public float cameraDistanceHorizontal = 2F;
	
	[Export]
	public float cameraDistanceVertical = 1.5F;
	
	public Vector3 prevPlayerRotation = Vector3.Zero;
	
	public Vector3 playerLookDirection = Vector3.Zero;
	
	//[Export]
	//public float playerLookDirectionX = 
												
	Camera3D camera;
	
	Marker3D cameraMarker;

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
		if (Input.IsActionPressed("Forward"))
		{
			velocity.X += playerLookDirection.X * groundSpeed * (float)delta;
			velocity.Z += playerLookDirection.Z * groundSpeed * (float)delta;
		}
		if (Input.IsActionPressed("Backward"))
		{
			velocity.X += -playerLookDirection.X * groundSpeed * (float)delta;
			velocity.Z += -playerLookDirection.Z * groundSpeed * (float)delta;
		}
		if (Input.IsActionPressed("Left"))
		{
			velocity.X += playerLookDirection.Z * groundSpeed * (float)delta;
			velocity.Z += -playerLookDirection.X * groundSpeed * (float)delta;
 		}
		if (Input.IsActionPressed("Right"))
		{
			velocity.X += -playerLookDirection.Z * groundSpeed * (float)delta;
			velocity.Z += playerLookDirection.X * groundSpeed * (float)delta;
		}
		GD.Print("_________________________________");
		GD.Print($"Velocity Before: {velocity}");
		GD.Print($"Speed Before: {(float)Math.Sqrt(Math.Pow((double)velocity.X, 2.0) + Math.Pow((double)velocity.Z, 2.0))}");
		// this is a bit problematic - while it does bring the velocity closer to the intended velocity, its fucking up somehow
		if (velocity.X != 0 && velocity.Z != 0 )
		{
			velocity.X = velocity.X * (groundSpeed * (float)delta) / (float)Math.Sqrt(Math.Pow((double)velocity.X, 2.0) + Math.Pow((double)velocity.Z, 2.0)); 
			velocity.Z = velocity.Z * (groundSpeed * (float)delta) / (float)Math.Sqrt(Math.Pow((double)velocity.X, 2.0) + Math.Pow((double)velocity.Z, 2.0));
		}
		if (Input.IsActionPressed("Jump") && IsOnFloor()) //&& IsOnFloor()
		{
			velocity.Y = jumpSpeed * (float)delta;
		}
		
		Velocity = velocity;
		
		//GD.Print($"Look Direction Distance: {(float)Math.Sqrt((double)(playerLookDirection.X * playerLookDirection.X + playerLookDirection.Z * playerLookDirection.Z))}");
		GD.Print($"Velocity After: {velocity}");
		GD.Print($"Speed After: {(float)Math.Sqrt(Math.Pow((double)velocity.X, 2.0) + Math.Pow((double)velocity.Z, 2.0))}");
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
				
		// rotate the look direction unit vector
		float playerLookAngleRadians = 0F;
		if (playerLookDirection.X >= 0 && playerLookDirection.Z >= 0)
		{
			playerLookAngleRadians = (float)Math.Acos((double)playerLookDirection.X);
			//GD.Print("First Quadrant");
		}
		else if (playerLookDirection.X <= 0 && playerLookDirection.Z >= 0)
		{
			playerLookAngleRadians = (float)Math.Acos((double)playerLookDirection.X);
			//GD.Print("Second Quadrant");
		}
		else if (playerLookDirection.X <= 0 && playerLookDirection.Z <= 0)
		{
			playerLookAngleRadians = 2*(float)Math.PI - (float)Math.Acos((double)playerLookDirection.X);
			//GD.Print("Third Quadrant");
		}
		else
		{
			playerLookAngleRadians = 2*(float)Math.PI - (float)Math.Acos((double)playerLookDirection.X);
			//GD.Print("Fourth Quadrant");
		}
		//GD.Print($"Look Angle Radians: {playerLookAngleRadians}");
		Vector3 newPlayerLookDirection = playerLookDirection;
		float newPlayerLookAngleRadians = playerLookAngleRadians - mouseAngleDeltaHorizontal;
		newPlayerLookDirection.X = (float)Math.Cos((double)(newPlayerLookAngleRadians));
		newPlayerLookDirection.Z = (float)Math.Sin((double)(newPlayerLookAngleRadians));
		playerLookDirection = newPlayerLookDirection;
		
	}
	
	private void applyGravity(double delta)
	{
		velocity.Y -= gravity * (float)delta;
	}
	
	
}
