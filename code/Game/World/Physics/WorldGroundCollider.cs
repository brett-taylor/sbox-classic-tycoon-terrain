using System.Collections.Generic;
using Sandbox;
using TycoonTerrain.Game.World.Data;

namespace TycoonTerrain.Game.World.Physics;

public class WorldGroundCollider : Collider
{
	[Property]
	public WorldChunk? WorldChunk
	{
		get => _worldChunk;
		set
		{
			_worldChunk = value;
			Rebuild();
		}
	}

	private WorldChunk? _worldChunk;

	protected override IEnumerable<PhysicsShape> CreatePhysicsShapes( PhysicsBody targetBody )
	{
		if ( _worldChunk is null )
		{
			targetBody.ClearShapes();
			yield break;
		}
		
		var worldSettings = WorldSettings.Instance;

		var heights = new ushort[ ( worldSettings.ChunkSizeX + 1 ) * ( worldSettings.ChunkSizeY + 1 ) ];
		var materials = new byte[ ( worldSettings.ChunkSizeX + 1 ) * ( worldSettings.ChunkSizeY + 1 ) ];
		
		for ( var y = 0; y <= worldSettings.ChunkSizeY; y++ )
		{
			for ( var x = 0; x <= worldSettings.ChunkSizeX; x++ )
			{
				var i = ( y * ( worldSettings.ChunkSizeX + 1 ) ) + x;
				
				var worldHeightCoordinate = WorldHeightCoordinate.FromLocalXy( x, y, _worldChunk.Value );
				heights[ i ] = WorldHeightManager.Instance.GetSimulationHeight( worldHeightCoordinate );
				materials[ i ] = 0;
			}
		}

		var shape = targetBody.AddHeightFieldShape(
			heights,
			materials,
			worldSettings.ChunkSizeX + 1,
			worldSettings.ChunkSizeY + 1,
			worldSettings.CellSize, 
			worldSettings.HeightLevel
		);

		yield return shape;
	}
}
