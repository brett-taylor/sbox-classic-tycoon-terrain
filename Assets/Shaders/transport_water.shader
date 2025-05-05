FEATURES
{
    #include "common/features.hlsl"
}

MODES
{
    Forward();
    Depth();
}

COMMON
{
	#ifndef S_ALPHA_TEST
	#define S_ALPHA_TEST 0
	#endif

	#ifndef S_TRANSLUCENT
	#define S_TRANSLUCENT 1
	#endif

	#include "common/shared.hlsl"
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
	
	float3 positionInWorldSpace : TEXCOORD15;
	float3 CameraToPositionRay	: TEXCOORD16;
};

VS
{
	#include "common/vertex.hlsl"
	#include "procedural.hlsl"

	float g_flWaveScrollSpeed < UiGroup( "Waves,0/,0/0" ); Default1( 0.21 ); Range1( 0, 1 ); >;
	float g_flWaveScale < UiGroup( "Waves,0/,0/0" ); Default1( 14 ); Range1( 0, 50 ); >;

	float CalculateWaveHeight( PixelInput o )
	{
		float2 offsetCoordinatesBy = float2( g_flTime * g_flWaveScrollSpeed, g_flTime * g_flWaveScrollSpeed );
		float2 tileOffset = TileAndOffsetUv( o.vPositionWs.xy, float2( 1, 1 ), offsetCoordinatesBy );

		float noiseValue = Simplex2D( tileOffset );

		return saturate( noiseValue ) * g_flWaveScale;
	}

	PixelInput MainVs( VertexInput i )
	{
		PixelInput o = ProcessVertex( i );

		o.vPositionWs.z += CalculateWaveHeight( o );	
		o.vPositionPs.xyzw = Position3WsToPs( o.vPositionWs.xyz );

		o.positionInWorldSpace = o.vPositionWs;
		o.CameraToPositionRay.xyz = CalculateCameraToPositionRayWs( o.vPositionWs.xyz );

		return FinalizeVertex( o );
	}
}

PS
{
	#define CUSTOM_MATERIAL_INPUTS
    #include "common/pixel.hlsl"

	#define BLEND_MODE_ALREADY_SET
    RenderState( BlendEnable, true );
    RenderState( BlendOp, ADD );
    RenderState( SrcBlend, SRC_ALPHA );
    RenderState( DstBlend, INV_SRC_ALPHA );

	#define DEPTH_STATE_ALREADY_SET
	RenderState( DepthWriteEnable, false );

	float g_flDeepWaterColorHeight < UiGroup( "Water,0/,0/0" ); Range( 0, 200 ); Default( 25 ); >;
	float4 g_flDeepWaterColor < UiType( Color ); UiGroup( "Water,0/,0/0" ); Default4( 0.17, 0.46, 0.81, 1 ); >;
	float4 g_flShallowWaterColor < UiType( Color ); UiGroup( "Water,0/,0/0" ); Default4( 0.50, 0.65, 0.80, 0.40 ); >;

	float g_flFoamDepth < UiGroup( "Foam,0/,0/0" ); Range( 0, 100); Default( 25 ); >;
	float g_flFoamFade < UiGroup( "Foam,0/,0/0" ); Range( 0, 0.9 ); Default( 0.2 ); >;

	float2 g_flLightbias < Default2( 0.5, 0.5 ); Range2( 0, 0, 1, 1 ); UiGroup( "Water,0/,0/0" ); >;

	float3 CalculateWorldSpacePosition( float3 vCameraToPositionRayWs, float2 vPositionSs )
	{
		float depth = Depth::GetNormalized( vPositionSs.xy );
		return RecoverWorldPosFromProjectedDepthAndRay( depth, vCameraToPositionRayWs );
	}

	float4 CalculateWaveColor( float4 vPositionSs, float3 positionInWorldSpace, float3 cameraRay ) 
	{
		float depth = CalculateWorldSpacePosition( cameraRay, vPositionSs.xy ).z;
		
		float differenceInHeight = positionInWorldSpace.z - depth;
		float mapped = RemapValClamped( differenceInHeight, 0, g_flDeepWaterColorHeight, 1, 0 );

		float4 deepWaterColor = float4( SrgbGammaToLinear( g_flDeepWaterColor.rgb ), g_flDeepWaterColor.a );
		float4 shallowWaterColor = float4( SrgbGammaToLinear( g_flShallowWaterColor.rgb ), g_flShallowWaterColor.a );
		float4 lerpedColour = lerp( deepWaterColor,  shallowWaterColor, mapped );
		
		return lerpedColour;
	}

	float4 CalculateIntersectionFoam( float4 vPositionSs, float3 positionInWorldSpace, float3 cameraRay ) 
	{
		float depth = CalculateWorldSpacePosition( cameraRay, vPositionSs.xy ).z;
		float differenceInHeight = positionInWorldSpace.z - depth;

		// Create the depth fade mask and do one minus so the intersection is white
		float depthFadeMask01 = 1 - saturate( differenceInHeight / g_flFoamDepth );
		
		// Add a smoothsection to the depth fade mask so we can get a intersection mask that is smooth
		float intersectionMask01 = smoothstep( 1 - g_flFoamFade, 1, depthFadeMask01 + 0.1 );
		
		// TODO Do we want some better form?
		return intersectionMask01;
	}

	float3 CalculateLighting( float3 normal )
	{
		float3 lightDirection = normalize( BinnedLightBuffer[0].GetPosition() );
		float3 lightColour = BinnedLightBuffer[0].GetColor();

		float brightness = max( dot( lightDirection, normal ), 0.0f );
		return ( lightColour * g_flLightbias.x ) + ( brightness * lightColour * g_flLightbias.y );
	}

	// https://ameye.dev/notes/stylized-water-shader/ and https://roystan.net/articles/toon-water/
	float4 MainPs( PixelInput i ) : SV_Target0
	{
		// float4 waveColor = CalculateWaveColor( i.vPositionWithOffsetWs, i.vPositionSs );
		float4 waveColor = CalculateWaveColor( i.vPositionSs, i.positionInWorldSpace, i.CameraToPositionRay );

		// Do some foam where the water intersects with the terrain and add it to the current wave colour
		float4 intersectionFoam = CalculateIntersectionFoam( i.vPositionSs, i.positionInWorldSpace, i.CameraToPositionRay );
		waveColor = saturate( waveColor + intersectionFoam );

		// Calculate the wave normal and apply a flat shading lighting effect to it
		float3 calculatedNormal = normalize( cross( ddy( i.positionInWorldSpace ), ddx( i.positionInWorldSpace ) ) );
		float4 waveColorLit = float4( waveColor.rgb * CalculateLighting( calculatedNormal ), waveColor.a );

		Material mat = Material::Init();
		mat.Albedo = waveColorLit;
		mat.Normal = calculatedNormal;
		mat.Roughness = 1;
		mat.Metalness = 0.3f;
		mat.AmbientOcclusion = 0;
		mat.TintMask = 0;

		return ShadingModelStandard::Shade( i, mat );
	}
}
