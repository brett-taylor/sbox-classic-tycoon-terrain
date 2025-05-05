using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Sandbox;
using Sandbox.Utility;
using TycoonTerrain.Cache.Biome;
using TycoonTerrain.Game.World.Data;
using TycoonTerrain.Utils;

namespace TycoonTerrain.Game.World;

public partial class BiomeManager : SingletonComponent<BiomeManager>
{
	private BiomeDefinition[,] BiomeMap { get; set; }
	
	private float[,] TemperatureMap { get; set; }
	
	private float[,] RainfallMap { get; set; }
	
	public void GenerateBiomes()
	{
		var stopwatch = new Stopwatch();
		stopwatch.Start();

		var worldSizeInCell = WorldSettings.Instance.WorldSizeInCell();
		BiomeMap = new BiomeDefinition[worldSizeInCell.x, worldSizeInCell.y];
		TemperatureMap = new float[worldSizeInCell.x, worldSizeInCell.y];
		RainfallMap = new float[worldSizeInCell.x, worldSizeInCell.y];
		
		var rainfallParameters = new Noise.Parameters { Frequency = WorldSettings.Instance.BiomeRainfallFrequency, Seed = WorldSettings.Instance.Seed * 20, };
		var rainfallNoise = Noise.SimplexField( rainfallParameters );
		
		var temperatureParameters = new Noise.Parameters { Frequency = WorldSettings.Instance.BiomeTemperatureFrequency, Seed = WorldSettings.Instance.Seed * 40, };
		var temperatureNoise = Noise.PerlinField( temperatureParameters );
		
		var distortionParameters = new Noise.Parameters { Frequency = WorldSettings.Instance.BiomeDistortionFrequency, Seed = WorldSettings.Instance.Seed * 60, };
		var distortionNoise = Noise.SimplexField( distortionParameters );
		
		for ( var x = 0; x < worldSizeInCell.x; x++ )
		{
			for ( var y = 0; y < worldSizeInCell.y; y++ )
			{
				var distortion = (int) ( distortionNoise.Sample( x, y ) * WorldSettings.Instance.BiomeDistortionMultiplier );
				
				var temperature = temperatureNoise.Sample( x + distortion, y + distortion );
				var rainfall = rainfallNoise.Sample( x + distortion, y + distortion );
				
				TemperatureMap[x, y] = temperature;
				RainfallMap[x, y] = rainfall;
				BiomeMap[x, y] = CalculateBiome( temperature: temperature, rainfall: rainfall );
			}
		}
		
		var biomeMapSizeBytes = IntPtr.Size * BiomeMap.Length;
		var temperatureMapSizeBytes = sizeof(float) * TemperatureMap.Length;
		var rainfallMapSizeBytes = sizeof(float) * RainfallMap.Length;
		var totalMegabytes = ByteSizeUtils.ConvertBytesToKilobytesRounded( biomeMapSizeBytes + temperatureMapSizeBytes + rainfallMapSizeBytes );
		Log.Info( $"Generated Biomes in {stopwatch.ElapsedMilliseconds}ms. Total memory usage: {totalMegabytes}kb." );
	}
	
	public BiomeDefinition GetBiomeAt( WorldCell worldCell )
	{
		return BiomeMap[worldCell.ClampedX(), worldCell.ClampedY()];
	}
	
	public float GetTemperatureAt( WorldCell worldCell )
	{
		return TemperatureMap[worldCell.ClampedX(), worldCell.ClampedY()];
	}
	
	public float GetRainfallAt( WorldCell worldCell )
	{
		return RainfallMap[worldCell.ClampedX(), worldCell.ClampedY()];
	}
}
