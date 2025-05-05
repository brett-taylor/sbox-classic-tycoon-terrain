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
	
	float3 vColor : COLOR0 < Semantic( Color ); >;
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput i )
	{
		PixelInput o = ProcessVertex( i );

		o.vVertexColor = float4( i.vColor.rgb, 1 );

		return FinalizeVertex( o );
	}
}

PS
{
	#define CUSTOM_MATERIAL_INPUTS
    #include "common/pixel.hlsl"
	#include "procedural.hlsl"

	float g_GridSize < Default( 39.37f ); Range( 0, 200 ); UiGroup( "Terrain Grid,0/,0/0" ); >;
	float g_GridLineWidth < Default( 0.1f ); Range( 0, 1 ); UiGroup( "Terrain Grid,0/,0/0" ); >;
	float4 g_GridColor < UiType( Color ); UiGroup( "Terrain Grid,0/,0/0" ); Default4( 0, 0, 0, 0.58 ); >;

	float g_CloudsScale < Default( 2048 ); Range( 0, 20000 ); UiGroup( "Clouds Layer,0/,0/0" ); >;
	float g_CloudsIntensity < Default( 0.2 ); Range( 0, 1 ); UiGroup( "Clouds Layer,0/,0/0" ); >;
	float2 g_CloudsMovement < Default2( 0.024, 0.010 ); Range2( 0, 0, 0.5, 0.5 ); UiGroup( "Clouds Layer,0/,0/0" ); >;

	// https://bgolus.medium.com/the-best-darn-grid-shader-yet-727f9278b9d8
	float CalculateGridMask( float3 vPositionWithOffsetWs ) 
	{
		float3 positionInWorldSpace = vPositionWithOffsetWs.xyz + g_vCameraPositionWs.xyz;
		float x = fmod( abs( positionInWorldSpace.x - ( g_GridSize / 2 ) ), g_GridSize ) / g_GridSize;
		float y = fmod( abs( positionInWorldSpace.y - ( g_GridSize / 2 ) ), g_GridSize ) / g_GridSize;
		float2 uv = float2( x, y );
	
		float4 uvDDXY = float4( ddx( uv ), ddy( uv ) );
		float2 uvDeriv = float2( length( uvDDXY.xz ), length( uvDDXY.yw ) );

		float2 drawWidth = uvDeriv * g_GridLineWidth;
		float2 lineAA = uvDeriv * 1.5;
		float2 gridUV = abs( frac( uv ) * 2.0 - 1.0 );
		gridUV = 1.0 - gridUV;
		
		float2 grid2 = smoothstep( drawWidth + lineAA, drawWidth - lineAA, gridUV );
		grid2 *= saturate( g_GridLineWidth / drawWidth );
		grid2 = lerp( grid2, g_GridLineWidth, saturate( uvDeriv * 2.0 - 1.0 ) );
		
		return lerp( grid2.x, 1.0, grid2.y );
	}

	float CalculateCloudShadowMask( float3 vPositionWithOffsetWs ) 
	{
		float3 positionInWorldSpace = vPositionWithOffsetWs + g_vCameraPositionWs;
		positionInWorldSpace /= g_CloudsScale;

		float2 offset = float2( g_flTime * g_CloudsMovement.x, g_flTime * g_CloudsMovement.y );
		float2 tileOffset = TileAndOffsetUv( positionInWorldSpace.xy, float2( 1, 1 ), offset );
		float noise = saturate( Simplex2D( tileOffset ) );
		
		return noise;
	}

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		Material mat = Material::Init();
		mat.Albedo = SrgbGammaToLinear( i.vVertexColor.rgb );
		mat.Normal = i.vNormalWs;
		mat.Roughness = 1.5;
		mat.Metalness = 0;
		mat.AmbientOcclusion = 0.5;
		mat.TintMask = 0;

		float cloudShadowMask = CalculateCloudShadowMask( i.vPositionWithOffsetWs );
		mat.Albedo = mat.Albedo - ( cloudShadowMask * mat.Albedo * g_CloudsIntensity );

		float4 shadedTerrainColor = ShadingModelStandard::Shade( i, mat );

		float gridMask = CalculateGridMask( i.vPositionWithOffsetWs );
		float terrainMask = 1 - gridMask;

		float alpha = g_GridColor.a;

		float4 gridColor = float4( lerp( shadedTerrainColor.rgb, g_GridColor.rgb, alpha ) * gridMask, 1 );
		shadedTerrainColor = ( shadedTerrainColor * terrainMask ) + gridColor;

		return float4( shadedTerrainColor.rgb, 1 );
	}
}
