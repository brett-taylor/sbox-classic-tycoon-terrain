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
};

VS
{
	#include "common/vertex.hlsl"
	#include "procedural.hlsl"

	float g_flWaveScrollSpeed < UiGroup( "Waves,0/,0/0" ); Default1( 0.21 ); Range1( 0, 1 ); >;
	float g_flWaveScale < UiGroup( "Waves,0/,0/0" ); Default1( 14 ); Range1( 0, 50 ); >;

	float CalculateWaveHeight( float3 vPositionWs, float3 vPositionOs )
	{
		float2 offsetCoordinatesBy = float2( g_flTime * g_flWaveScrollSpeed, g_flTime * g_flWaveScrollSpeed );
		float2 tileOffset = TileAndOffsetUv( vPositionWs.xy, float2( 1, 1 ), offsetCoordinatesBy );

		float noiseValue = Simplex2D( tileOffset );
		float height = saturate( noiseValue ) * g_flWaveScale;
		
		return height * ( 1 - step( vPositionOs.z, 250 ) );
	}

	PixelInput MainVs( VertexInput i )
	{
		PixelInput o = ProcessVertex( i );

		o.vPositionWs.z += CalculateWaveHeight( o.vPositionWs, i.vPositionOs );	
		o.vPositionPs.xyzw = Position3WsToPs( o.vPositionWs.xyz );

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

	float4 g_WaterColor < UiType( Color ); UiGroup( "Water,0/,0/0" ); Default4( 0.17, 0.46, 0.81, 0.3f ); >;

	float g_flFogEndDistanceWs < UiType( Slider ); Range( 0, 10000 ); Default( 512 ); UiGroup( "Fog,0/,0/0" ); >;
	float g_flFogDensity < UiType( Slider ); Range( 0, 1 ); Default( 1 ); UiGroup( "Fog,0/,0/0" ); >;
	float4 g_fogColor < UiType( Color ); Default4( 1, 0, 0.5, 1 ); UiGroup( "Fog,0/,0/0" ); >;
 

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		float4 baseColour = float4( SrgbGammaToLinear( g_WaterColor.rgb ), g_WaterColor.a );

		float flLinear = Depth::GetLinear( i.vPositionSs.xy );
		return lerp( g_fogColor, baseColour, smoothstep( 0, g_flFogDensity, 1 - flLinear / g_flFogEndDistanceWs ) );
	}
}
