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
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput i )
	{
		PixelInput o = ProcessVertex( i );

		o.positionInWorldSpace = o.vPositionWs;

		return FinalizeVertex( o );
	}
}

PS
{
	#define CUSTOM_MATERIAL_INPUTS
    #include "common/pixel.hlsl"
	#include "procedural.hlsl"

	// Lava fade
	float g_flLavaHeight < UiGroup( "Lava Fade,0/,0/0" ); Default1( -700.000 ); Range1( -1000, 1000 ); >;
	float g_flLavaFadeEndHeight < UiGroup( "Lava Fade,0/,0/0" ); Default1( -382.456 ); Range1( -1000, 1000 ); >;
	float g_flLavaGrowShrinkAmount < UiGroup( "Lava Fade,0/,0/0" ); Default1( 100 ); Range1( 0, 300 ); >;
	float g_flLavaGrowShrinkTime < UiGroup( "Lava Fade,0/,0/0" ); Default1( 1 ); Range1( 0, 10 ); >;

	// Lava colour
	float3 g_vBaseLavaColour < UiType( Color ); UiGroup( "Lava Appearance,0/,0/0" ); Default3( 1, 0, 0 ); >;

	float g_flLavaVoroniAngle < UiGroup( "Lava Appearance,0/,0/0" ); Default1( 3 ); Range1( 0, 360 ); >;
	float g_flLavaVoroniDensity < UiGroup( "Lava Appearance,0/,0/0" ); Default1( 3 ); Range1( 0, 10 ); >;
	float g_flLavaVoroniScale < UiGroup( "Lava Appearance,0/,0/0" ); Default1( 300 ); Range1( 0, 1000 ); >;
	float g_flLavaVoroniStrength < UiGroup( "Lava Appearance,0/,0/0" ); Default1( 300 ); Range1( 0, 100 ); >;
	float g_flLavaVoroniMovementSpeed < UiGroup( "Lava Appearance,0/,0/0" ); Default1( 0.25 ); Range1( 0, 3 ); >;
	float3 g_vLavaVoroniColour < UiType( Color ); UiGroup( "Lava Appearance,0/,0/0" ); Default3( 0, 1, 0 ); >;
	float g_flLavaVoroniEmissionStrength < UiGroup( "Lava Appearance,0/,0/0" ); Default1( 300 ); Range1( 0, 100 ); >;

	// Dirt
	float3 g_vBaseDirtColour < UiType( Color ); UiGroup( "Terrain,0/,0/0" ); Default3( 0.72, 0.07, 0.18 ); >;

	float CalculateLavaMask01( PixelInput i, float2 texCoords ) 
	{
		float remappedSin = RemapValClamped( sin( g_flTime * g_flLavaGrowShrinkTime ), -1, 1, 0, 1 );
		float lavaEndHeight = g_flLavaFadeEndHeight + ( g_flLavaGrowShrinkAmount * remappedSin );
		float heightFadeFactor = 1 - saturate( ( i.positionInWorldSpace.z ) - ( lavaEndHeight ) );
		
		return step( 0.5, heightFadeFactor );
	}

	float3 CalculateLavaColour( PixelInput i, float2 texCoords, float lavaMask01 )
	{
		float layerOneOffset = RemapValClamped( sin( g_flTime * g_flLavaVoroniMovementSpeed ), -1, 1, 0, 1 );
		float layerOne = VoronoiNoise( texCoords / g_flLavaVoroniScale, g_flLavaVoroniAngle + layerOneOffset, g_flLavaVoroniDensity );

		float3 highlight = SrgbGammaToLinear( g_vLavaVoroniColour ) * pow( layerOne, g_flLavaVoroniStrength );
		
		return ( g_vBaseLavaColour + highlight ) * lavaMask01;
	}

	float3 CalculateDirtColour( PixelInput i, float2 texCoords, float lavaMask01 )
	{
		return g_vBaseDirtColour * ( 1 - lavaMask01 );
	}

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		float2 texCoords = float2( i.positionInWorldSpace.x + i.positionInWorldSpace.y, i.positionInWorldSpace.z );

		float lavaMask01 = CalculateLavaMask01( i, texCoords );
		float3 lavaColour = CalculateLavaColour( i, texCoords, lavaMask01 );
		float3 dirtColour = CalculateDirtColour( i, texCoords, lavaMask01 );

		float3 albedo = lavaColour + dirtColour;
		
		Material mat = Material::Init();
		mat.Albedo = SrgbGammaToLinear( albedo );
		mat.Normal = i.vNormalWs;
		mat.Emission = lavaColour * g_flLavaVoroniEmissionStrength;
		return ShadingModelStandard::Shade( i, mat );
	}
}
