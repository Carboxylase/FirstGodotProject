using Godot;
using System;

public partial class Player : CharacterBody3D
{	
	public float PixelsPerMeter = 100F;
	
	// Player Movement Settings
	[Export]
	public float groundSpeedMetersPerSecond = 8.5F;
	[Export]
	public float groundSpeedSprintMetersPerSecond = 12.5F;
	public float groundSpeed;
	
	[Export]
	public float jumpSpeedMetersPerSecond = 4.5F;
	public float jumpSpeed;
	
	[Export]
	public float gravityMetersPerSecond = 9.8F;
	public float gravity;

	public Vector3 velocity = Vector3.Zero;

	// Player Mouse Settings
	[Export]
	public float mouseRotationSpeed = MathF.PI / 800;//0.1125F; // 90 degrees/800 pixels

	public Vector2 prevMousePosition = Vector2.Zero;
	
	public Vector2 mouseMovementDelta = Vector2.Zero;
	
	// Player Camera Settings
	public Vector3 prevCameraPosition = Vector3.Zero;
	
	[Export]
	public float cameraDistanceHorizontal = 2F;
	
	[Export]
	public float cameraDistanceVertical = 1.5F;
	
	public float minVerticalCameraAngleRadians = -0.25F * MathF.PI;
	
	public float maxVerticalCameraAngleRadians = 0.60F * MathF.PI;
	
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
		
		Input.MouseMode = Input.MouseModeEnum.Captured;
		Input.UseAccumulatedInput = true;
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
	
	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMovement)
		{
			mouseMovementDelta += mouseMovement.ScreenRelative;
		}
		if (@event is InputEventKey keyInput)
		{
			if (keyInput.Pressed && keyInput.Keycode == Key.Escape)
			{
				GetTree().Quit();
			}
		}	
	}
	
	public override void _PhysicsProcess(double delta)
	{
		applyGravity(delta);
		
		updatePlayerPerspective();
		
		getPlayerInput(delta);
		//camera.RotationDegrees = new Vector3();
		MoveAndSlide();
	}
	
	public override void _Process(double delta)
	{
		mouseMovementDelta = Vector2.Zero;
	}
	
	private void getPlayerInput(double delta)
	{	
		//GD.Print($"X Component: {playerLookDirection3D.GlobalPosition.X - Position.X}, Z Component: {playerLookDirection3D.GlobalPosition.Z - Position.Z}");
		
		if (Input.IsActionPressed("Sprint"))
		{
			groundSpeed = groundSpeedSprintMetersPerSecond * PixelsPerMeter;
		}
		else
		{
			groundSpeed = 	groundSpeedMetersPerSecond * PixelsPerMeter;
		}
		
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
		//GD.Print("_________________________________");
		//GD.Print($"Player Look Direciton: {playerLookDirection3D.GlobalRotation.Y}");
		//GD.Print($"Velocity Before: {velocity}");
		//GD.Print($"Speed Before: {MathF.Sqrt(MathF.Pow(velocity.X, 2.0F) + MathF.Pow(velocity.Z, 2.0F))}");
		 //this is a bit problematic - while it does bring the velocity closer to the intended velocity, its fucking up somehow
		if (velocity.X != 0 && velocity.Z != 0 )
		{
			float normalizeCoefficient = (groundSpeed * (float)delta) / MathF.Sqrt(MathF.Pow(velocity.X, 2.0F) + MathF.Pow(velocity.Z, 2.0F));
			velocity.X = velocity.X * normalizeCoefficient; 
			velocity.Z = velocity.Z * normalizeCoefficient;
		}
		
		//jump logic
		if (Input.IsActionPressed("Jump") && IsOnFloor()) //&& IsOnFloor()
		{
			velocity.Y = jumpSpeed * (float)delta;
		}

		Velocity = velocity;
		
		Vector3 velocityAfter = velocity;
		GD.Print($"Velocity: {velocity}");
		//GD.Print($"Velocity Ratio: {velocityBefore/velocityAfter}");
		
		//GD.Print($"Look Direction Distance: {(float)Math.Sqrt((double)(playerLookDirection.X * playerLookDirection.X + playerLookDirection.Z * playerLookDirection.Z))}");
		//GD.Print($"Velocity After: {velocity}");
		//GD.Print($"Speed After: {MathF.Sqrt(MathF.Pow(velocity.X, 2.0F) + MathF.Pow(velocity.Z, 2.0F))}");
		//GD.Print("_________________________________");
		float persistentVerticalVelocity = velocity.Y;
		Vector3 resetVelocity = Vector3.Zero;
		resetVelocity.Y = persistentVerticalVelocity;
		velocity = resetVelocity;
		
	}
	
	private void updatePlayerPerspective()
	{
		Vector2 currentMousePosition = GetViewport().GetMousePosition();
		//Vector2 mouseDelta = currentMousePosition - prevMousePosition;
		Vector2 mouseDelta = mouseMovementDelta;
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
		
		// moving the mouse down makes the cameraMarkerRotation increase
		float cameraRotationPlayerRelative = camera.Rotation.Z + cameraMarker.Rotation.Z;
		
		//looking down
		if (cameraRotationPlayerRelative <= minVerticalCameraAngleRadians && mouseDelta.Y > 1)
		{
			//cameraMarkerRotationRadians.Z = minVerticalCameraAngleRadians - camera.Rotation.Z;
			return;
		}
		
		//looking up
		if (cameraRotationPlayerRelative >= maxVerticalCameraAngleRadians && mouseDelta.Y < 0)
		{
			//cameraMarkerRotationRadians.Z = maxVerticalCameraAngleRadians - camera.Rotation.Z;
			return;
		}
		//GD.Print($"Mouse Delta: {mouseDelta.Y}");
		//GD.Print($"Rotation: {cameraMarker.Rotation}");
		cameraMarker.Rotation = cameraMarkerRotationRadians;
		return;
	}
	
	private void applyGravity(double delta)
	{
		velocity.Y -= gravity * (float)delta * (float)delta;
		if (IsOnFloor())
		{
			velocity.Y = 0F;
		}
	}
	
}
