using Sandbox;

namespace TycoonTerrain.Game.World.Data;

public struct WorldHeightCoordinate
{
	public static WorldHeightCoordinate FromLocalXy( int x, int y, WorldChunk chunk )
	{
		var absoluteX = x + ( chunk.Xy.x * ( WorldSettings.Instance.ChunkSizeX ) );
		var absoluteY = y + ( chunk.Xy.y * ( WorldSettings.Instance.ChunkSizeY ) );
		return new WorldHeightCoordinate( absoluteX, absoluteY );
	}
	
	public static WorldHeightCoordinate FromWorldCellNorthWestCorner( WorldCell worldCell )
	{
		return new WorldHeightCoordinate( worldCell.Xy.x, worldCell.Xy.y + 1 );
	}
	
	public static WorldHeightCoordinate FromWorldCellNorthEastCorner( WorldCell worldCell )
	{
		return new WorldHeightCoordinate( worldCell.Xy.x + 1, worldCell.Xy.y + 1 );
	}
	
	public static WorldHeightCoordinate FromWorldCellSouthEastCorner( WorldCell worldCell )
	{
		return new WorldHeightCoordinate( worldCell.Xy.x + 1, worldCell.Xy.y );
	}
	
	public static WorldHeightCoordinate FromWorldCellSouthWestCorner( WorldCell worldCell )
	{
		return new WorldHeightCoordinate( worldCell.Xy.x, worldCell.Xy.y );
	}
	
	[Property] public Vector2Int Xy { get; private set; }

	private WorldHeightCoordinate( int x, int y )
	{
		Xy = new Vector2Int( x, y );
	}
	
	public override string ToString()
	{
		return $"WorldHeightCoordinate(x={Xy.x}, y={Xy.y})";
	}
}
