using System;
using Sandbox;
using TycoonTerrain.Game.World.Physics;
using TycoonTerrain.Utils;

namespace TycoonTerrain.Game.World;

public class WorldManager : SingletonComponent<WorldManager>
{
	protected override void OnStart()
	{
		base.OnStart();

		if ( WorldSettings.Instance.Seed == 0 )
			WorldSettings.Instance.Seed = Random.Shared.Next();
		
		StartWorld();
	}

	private void StartWorld()
	{
		WorldHeightManager.Instance.GenerateWorldHeight();
		BiomeManager.Instance.GenerateBiomes();

		WorldPhysicsManager.Instance.CreateAllPhysicsChunks();
		WorldVisualManager.Instance.Initialize();
	}

	[Button( "Regenerate World New Seed" )]
	private void DebugRegenerateWorldWithNewSeed()
	{
		WorldVisualManager.Instance.TerrainContainer.Children.ForEach( o => o.Destroy() );

		WorldSettings.Instance.Seed = Random.Shared.Next();
		StartWorld();
	}
	
	[Button( "Regenerate World Same Seed" )]
	private void DebugRegenerateWorldWithSameSeed()
	{
		WorldVisualManager.Instance.TerrainContainer.Children.ForEach( o => o.Destroy() );

		StartWorld();
	}
}
