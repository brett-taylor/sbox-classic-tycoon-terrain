using Sandbox;
using TycoonTerrain.Game.World.Data;
using TycoonTerrain.Utils;

namespace TycoonTerrain.Game.World.Physics;

public class WorldPhysicsManager : SingletonComponent<WorldPhysicsManager>
{
	[Property] private GameObject TerrainContainer { get; set; }

	public async void CreateAllPhysicsChunks()
	{
		for ( var x = 0; x < WorldSettings.Instance.ChunkCountX; x++ )
		{
			for ( var y = 0; y < WorldSettings.Instance.ChunkCountY; y++ )
			{
				CreatePhysicsChunk( WorldChunk.From( x, y ) );
				await Task.Frame();
			}
		}
	}

	private void CreatePhysicsChunk( WorldChunk worldChunk )
	{
		var chunkGameObject = new GameObject();
		chunkGameObject.Name = $"ChunkPhysics(x={worldChunk.Xy.x}, y={worldChunk.Xy.y})";
		chunkGameObject.NetworkMode = NetworkMode.Never;
		chunkGameObject.SetParent( TerrainContainer );
		
		chunkGameObject.LocalPosition = WorldCell.FromLocalXy( 0, 0, worldChunk ).SouthWestWorldPosition();
		chunkGameObject.LocalRotation = Rotation.From( 0f, 90f, 90f );

		chunkGameObject.Tags.Add( TycoonTerrain.Tags.World );
		chunkGameObject.Tags.Add( TycoonTerrain.Tags.Ground );
		
		var groundCollider = chunkGameObject.AddComponent<WorldGroundCollider>();
		groundCollider.WorldChunk = worldChunk;
		groundCollider.Static = true;
	}
}
