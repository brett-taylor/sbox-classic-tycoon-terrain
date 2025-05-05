using System;
using TycoonTerrain.Game.World;
using TycoonTerrain.Game.World.Data;
using TycoonTerrain.Utils;

namespace TycoonTerrain.Game.Player;

public class PlayerCameraController : SingletonComponent<PlayerCameraController>
{
	[Category( "Core Settings" ), Order( 1 )] 
	[Property]
	public GameObject CameraGameObject { get; private set; }
	
	[Category( "Core Settings" ), Order( 1 )] 
	[Property]
	public CameraComponent CameraComponent { get; private set; }
	
	[Category( "Core Settings" ), Order( 1 )] 
	[Property] 
	private GameObject TargetLookObject { get; set; }
	
	[Category( "Move Speeds" ), Order( 2 )] 
	[Property]
	private float MinCameraDistance { get; set; } = 250f;	
	
	[Category( "Move Speeds" ), Order( 2 )] 
	[Property]
	private float MaxCameraDistance { get; set; } = 1500f;
	
	[Category( "Move Speeds" ), Order( 2 )] 
	[Property] 
	private float MinCameraPitch { get; set; } = 10f;
	
	[Category( "Move Speeds" ), Order( 2 )] 
	[Property] 
	private float MaxCameraPitch { get; set; } = 60f;
	
	[Category( "Move Speeds" ), Order( 2 )] 
	[Property] 
	private float PanLerpTime { get; set; } = 5f;
	
	[Category( "Move Speeds" ), Order( 2 )] 
	[Property] 
	private Curve PanKeyboardSpeed { get; set; } = 500f;
	
	[Category( "Move Speeds" ), Order( 2 )] 
	[Property] 
	private Curve PanMouseSpeed { get; set; } = 500f;
	
	[Category( "Move Speeds" ), Order( 2 )] 
	[Property] 
	private float RotateSpeed { get; set; } = -0.2f;
	
	[Category( "Move Speeds" ), Order( 2 )] 
	[Property] 
	private float RotateLerpTime { get; set; } = 20f;
	
	[Category( "Move Speeds" ), Order( 2 )] 
	[Property] 
	private Curve ZoomSpeed { get; set; } = -0.05f;
	
	[Category( "Move Speeds" ), Order( 2 )] 
	[Property]
	private float ZoomLerpTime { get; set; } = 15f;
	
	[Category( "Move Speeds" ), Order( 2 )] 
	[Property] 
	private float PositionZOffset { get; set; } = 50f;
	
	[Category( "Starting Settings" ), Order( 3 )] 
	[Property] 
	private float TargetCameraYaw { get; set; } = 150;
	
	[Category( "Starting Settings" ), Order( 3 )] 
	[Property]  
	private float TargetCameraPitch { get; set; } = 45f;
	
	[Category( "Starting Settings" ), Order( 3 )] 
	[Property] 
	private float TargetCameraZoom { get; set; } = 0.9f;
	
	private float CurrentCameraYaw { get; set; }
	
	private float CurrentCameraPitch { get; set; }
	
	private float CurrentCameraZoom { get; set; }
	
	private Vector3 CurrentLookPosition { get; set; }
	
	private Vector3 TargetLookPosition { get; set; }
	
	protected override void OnStart()
	{
		base.OnStart();
		
		CameraComponent.Enabled = true;
		
		TargetLookPosition = TargetLookObject.WorldPosition;
		CurrentLookPosition = TargetLookPosition;
		
		CurrentCameraYaw = TargetCameraYaw;
		CurrentCameraZoom = TargetCameraZoom;
		CurrentCameraPitch = TargetCameraPitch;
	}

	protected override void OnUpdate()
	{
		var isNotRotating = !Input.Down( "CameraRotate" );
		var isNotPanning = !Input.Down( "CameraPan" );
		
		Mouse.Visibility = isNotRotating && isNotPanning ? MouseVisibility.Visible : MouseVisibility.Hidden;
	}

	protected override void OnPreRender()
	{
		if ( CameraGameObject is null )
			return;
		
		HandleZoomAndRotationMouseInput();
		
		HandlePan();

		PositionTargetLookObjectAtTerrainHeight();
		
		TargetLookPosition = TargetLookObject.WorldPosition + new Vector3( 0, 0, PositionZOffset );		
		
		PositionCamera();
	}
	
	private void HandleZoomAndRotationMouseInput()
	{
		if ( Input.Down( "CameraRotate" ) )
		{
			TargetCameraYaw += Input.MouseDelta.x * RotateSpeed;

			TargetCameraPitch -= Input.MouseDelta.y * RotateSpeed;
			TargetCameraPitch = TargetCameraPitch.Clamp( MinCameraPitch, MaxCameraPitch );
		}
		
		TargetCameraZoom += Input.MouseWheel.y * -ZoomSpeed.Evaluate( CurrentCameraZoom );
		TargetCameraZoom = TargetCameraZoom.Clamp( 0f, 1f );
	}

	private void HandlePan()
	{
		if ( Input.Down( "CameraPan" ) )
		{
			var panMouseSpeedAdjustedForZoom = PanMouseSpeed.Evaluate( CurrentCameraZoom );
			
			var forwardDiff = Input.MouseDelta.y * CameraGameObject.WorldRotation.Forward * Time.Delta * panMouseSpeedAdjustedForZoom;
			forwardDiff = forwardDiff.WithZ( 0f );
			
			var rightDiff = Input.MouseDelta.x * CameraGameObject.WorldRotation.Left * Time.Delta * panMouseSpeedAdjustedForZoom;
			rightDiff = rightDiff.WithZ( 0f );

			TargetLookObject.WorldPosition += forwardDiff + rightDiff;
		}
		
		var panKeyboardSpeedAdjustedForZoom = PanKeyboardSpeed.Evaluate( CurrentCameraZoom );
		
		if ( Input.Down( "CameraPanUp" ) )
			TargetLookObject.WorldPosition += ( CameraGameObject.WorldRotation.Forward * Time.Delta * panKeyboardSpeedAdjustedForZoom ).WithZ( 0f );
		
		if ( Input.Down( "CameraPanDown" ) )
			TargetLookObject.WorldPosition += ( CameraGameObject.WorldRotation.Backward * Time.Delta * panKeyboardSpeedAdjustedForZoom ).WithZ( 0f );
		
		if ( Input.Down( "CameraPanLeft" ) )
			TargetLookObject.WorldPosition += ( CameraGameObject.WorldRotation.Left * Time.Delta * panKeyboardSpeedAdjustedForZoom ).WithZ( 0f );
		
		if ( Input.Down( "CameraPanRight" ) )
			TargetLookObject.WorldPosition += ( CameraGameObject.WorldRotation.Right * Time.Delta * panKeyboardSpeedAdjustedForZoom ).WithZ( 0f );
	}

	private void PositionTargetLookObjectAtTerrainHeight()
	{
		var targetLookWorldCell = WorldCell.FromWorldPosition( TargetLookObject.WorldPosition );
		var height = WorldHeightManager.Instance.GetPhysicalHeight( targetLookWorldCell.SouthWestCornerHeight() );

		TargetLookObject.WorldPosition = new Vector3(
			TargetLookObject.WorldPosition.x,
			TargetLookObject.WorldPosition.y,
			height
		);
	}
	
	private void PositionCamera()
	{
		CurrentCameraYaw = MathX.Lerp( CurrentCameraYaw, TargetCameraYaw, RotateLerpTime * Time.Delta );
		CurrentCameraPitch = MathX.Lerp( CurrentCameraPitch, TargetCameraPitch, RotateLerpTime * Time.Delta );
		CameraGameObject.WorldRotation = Rotation.From( CurrentCameraPitch, CurrentCameraYaw, 0 );
		
		CurrentCameraZoom = MathX.Lerp( CurrentCameraZoom, TargetCameraZoom, ZoomLerpTime * Time.Delta );
		var cameraPitchDistance = MathX.Lerp( MinCameraDistance, MaxCameraDistance, CurrentCameraZoom );
		var cameraHeight = cameraPitchDistance * MathF.Sin( CurrentCameraPitch.DegreeToRadian() );
		var cameraDistance = MathF.Sqrt( MathF.Pow( cameraPitchDistance, 2 ) - MathF.Pow( cameraHeight, 2 ) );

		CurrentLookPosition = Vector3.Lerp( CurrentLookPosition, TargetLookPosition, PanLerpTime * Time.Delta );
		CameraGameObject.WorldPosition = CurrentLookPosition + (Vector3.Up * cameraHeight) + (Vector3.Forward * Rotation.FromYaw( CurrentCameraYaw ) * -cameraDistance);
	}
	
}
