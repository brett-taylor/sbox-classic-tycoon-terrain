using System.Collections.Generic;
using System.Linq;
using Sandbox;
using TycoonTerrain.Cache.Biome;

namespace TycoonTerrain.Game.World;

public partial class BiomeManager
{
	private Dictionary<string, BiomeDefinition> BiomeTypes { get; set; } = new();

	protected override void OnAwake()
	{
		base.OnAwake();

		LoadBiomeTypes();
	}

	private BiomeDefinition CalculateBiome( float temperature, float rainfall )
	{
		var vector = new Vector2( temperature, rainfall );
		
		var bestDistance = float.MaxValue;
		BiomeDefinition bestMatch = null;
		
		foreach ( var biome in BiomeTypes.Values )
		{
			if ( biome.Disabled )
				continue;
			
			var biomeVector = new Vector2( biome.Temperature, biome.Rainfall );
			var distance = (vector - biomeVector).LengthSquared;
			
			if ( distance < bestDistance )
			{
				bestDistance = distance;
				bestMatch = biome;
			}
		}
		
		return bestMatch;
	}
	
	private BiomeDefinition RandomBiome()
	{
		var count = Sandbox.Game.Random.Next( BiomeTypes.Count );
		return BiomeTypes.ElementAt( count ).Value;
	}
	
	private void LoadBiomeTypes()
	{
		foreach ( var biomeDefinition in ResourceLibrary.GetAll<BiomeDefinition>() )
		{
			if ( BiomeTypes.TryAdd( biomeDefinition.ResourceName, biomeDefinition ) )
				continue;

			Log.Error( $"Duplicate Biome Type Id with: {biomeDefinition.ResourceName}. Can not continue." );
			return;
		}

		Log.Info( $"Loaded {BiomeTypes.Count} Biome Types." );
	}
}
