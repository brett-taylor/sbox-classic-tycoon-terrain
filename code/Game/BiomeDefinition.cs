namespace TycoonTerrain.Cache.Biome;

[GameResource( "Biome Definition", "biome", "", Icon = "🌲" )]
public class BiomeDefinition : GameResource
{
	public string Name { get; set; }

	public float Temperature { get; set; } = 0.5f;
	
	public float Rainfall { get; set; } = 0.5f;

	public bool Disabled { get; set; } = false;
	
	public Color GroundColor { get; set; }

	[Range( -1f, 1f )] public float GroundColorOffsetMin { get; set; } = -0.05f;

	[Range( -1f, 1f )] public float GroundColorOffsetMax { get; set; } = 0.05f;
}
