using TycoonTerrain.Game.World.Data;
using TycoonTerrain.Game.World.Visual;
using TycoonTerrain.Utils;

namespace TycoonTerrain.Game.World;

public class WorldVisualManager : SingletonComponent<WorldVisualManager>
{
	[Property] public GameObject TerrainContainer { get; private set; }

	private bool IsInitialized { get; set; } = false;

	private Dictionary<WorldChunk, GameObject> Chunks { get; set; } = new();

	private HashSet<WorldChunk> RenderedChunks { get; set; } = new();

	public void Initialize()
	{
		Chunks.Clear();
		RenderedChunks.Clear();

		IsInitialized = true;
	}

	protected override void OnPreRender()
	{
		base.OnPreRender();

		if ( !IsInitialized )
			return;

		var cameraPosition = Scene.Camera.WorldPosition;
		var overWorldChunk = WorldChunk.FromWorldPosition( cameraPosition );

		var nowShouldBeRendered = new HashSet<WorldChunk>();
		for ( var y = -20; y <= 20; y++ )
		{
			for ( var x = -20; x <= 20; x++ )
			{
				var worldChunk = WorldChunk.From( overWorldChunk.Xy.x + x, overWorldChunk.Xy.y + y );
				if ( worldChunk.IsValid() )
					nowShouldBeRendered.Add( worldChunk );
			}
		}

		var chunksToUnRender = RenderedChunks.Except( nowShouldBeRendered );
		foreach ( var worldChunk in chunksToUnRender )
			HideChunk( worldChunk );

		var chunksToRender = nowShouldBeRendered.Except( RenderedChunks );
		foreach ( var worldChunk in chunksToRender )
			ShowChunk( worldChunk );
	}

	private void HideChunk( WorldChunk worldChunk )
	{
		RenderedChunks.Remove( worldChunk );

		Chunks[worldChunk].Enabled = false;
	}

	private void ShowChunk( WorldChunk worldChunk )
	{
		RenderedChunks.Add( worldChunk );

		var isChunkCreated = Chunks.ContainsKey( worldChunk );

		if ( isChunkCreated )
		{
			Chunks[worldChunk].Enabled = true;
			return;
		}

		CreateChunk( worldChunk );
	}

	private void CreateChunk( WorldChunk worldChunk )
	{
		var chunkGameObject = new GameObject();
		chunkGameObject.Name = $"Chunk(x={worldChunk.Xy.x}, y={worldChunk.Xy.y})";
		chunkGameObject.NetworkMode = NetworkMode.Never;
		chunkGameObject.SetParent( TerrainContainer );

		chunkGameObject.Tags.Add( TycoonTerrain.Tags.World );

		var worldGroundVisual = chunkGameObject.AddComponent<WorldGroundVisualRenderer>();
		worldGroundVisual.WorldChunk = worldChunk;

		var worldWaterVisual = chunkGameObject.AddComponent<WorldWaterVisualRenderer>();
		worldWaterVisual.WorldChunk = worldChunk;

		var worldGroundSideWall = chunkGameObject.AddComponent<WorldGroundSideWallRenderer>();
		worldGroundSideWall.WorldChunk = worldChunk;

		var worldWaterSideWall = chunkGameObject.AddComponent<WorldWaterSideWallRenderer>();
		worldWaterSideWall.WorldChunk = worldChunk;

		Chunks.Add( worldChunk, chunkGameObject );
	}
}
