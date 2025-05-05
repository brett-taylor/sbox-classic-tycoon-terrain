using Sandbox;
using TycoonTerrain.Utils;

namespace TycoonTerrain.Game.World;

public class WorldSettings : SingletonComponent<WorldSettings>
{
	[Property, Category( "Core" ), Order( 1 ) ] 
	public int Seed { get; set; }
	
	[Property, Category( "Size" ), Order( 1 ) ] 
	public int ChunkSizeX { get; private set; } = 100;

	[Property, Category( "Size" ), Order( 1 ) ] 
	public int ChunkSizeY { get; private set; } = 100;

	[Property, Category( "Size" ), Order( 1 ) ] 
	public int ChunkCountX { get; private set; } = 5;

	[Property, Category( "Size" ), Order( 1 ) ] 
	public int ChunkCountY { get; private set; } = 5;

	[Property, Category( "Size" ), Order( 1 )]
	public float CellSize { get; private set; } = 118.11f;

	[Property, Category( "Height" ), Order( 2 )]
	public float HeightLevel { get; private set; } = 90f;

	[Property, Category( "Height" ), Order( 2 ) ] 
	public float FallOffStrength { get; private set; } = 7f;

	[Property, Category( "Height" ), Order( 2 ) ] 
	public byte SeaLevel { get; private set; } = 3;

	[Property, Category( "Height" ), Order( 2 )]
	public float SeaHeightOffset { get; private set; } = 45f;

	[Property, Category( "Height" ), Order( 2 ) ] 
	public float SideWallHeight { get; private set; } = 1000f;
	
	[Property, Category( "Biome" ), Order( 3 ) ] 
	public int BiomeFuzzinessRange { get; private set; } = 1;
	
	[Property, Category( "Biome" ), Order( 3 ) ] 
	public float BiomeFuzzinessDropOff { get; private set; }= 2f;
	
	[Property, Category( "Biome" ), Order( 3 ) ] 
	public float BiomeDistortionFrequency { get; set; } = 1f;
	
	[Property, Category( "Biome" ), Order( 3 ) ] 
	public float BiomeDistortionMultiplier { get; set; } = 15f;
	
	[Property, Category( "Biome" ), Order( 3 ) ]
	public float BiomeRainfallFrequency { get; set; } = 0.005f;
	
	[Property, Category( "Biome" ), Order( 3 ) ]
	public float BiomeTemperatureFrequency { get; set; } = 0.005f;

	public Vector2Int WorldSizeInCell()
	{
		return new Vector2Int( ChunkSizeX * ChunkCountX, ChunkSizeY * ChunkCountY );
	}
}
