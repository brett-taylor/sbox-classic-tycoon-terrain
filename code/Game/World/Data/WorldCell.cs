using System;
using Sandbox;

namespace TycoonTerrain.Game.World.Data;

public struct WorldCell : IEquatable<WorldCell>
{
	public static WorldCell FromAbsoluteXy( int x, int y )
	{
		return new WorldCell( x, y );
	}

	public static WorldCell FromLocalXy( int x, int y, WorldChunk chunk )
	{
		return FromLocalXy( x, y, chunk.Xy.x, chunk.Xy.y );
	}

	public static WorldCell FromLocalXy( int x, int y, int chunkX, int chunkY )
	{
		var absoluteX = x + ( chunkX * WorldSettings.Instance.ChunkSizeX );
		var absoluteY = y + ( chunkY * WorldSettings.Instance.ChunkSizeY );
		return new WorldCell( absoluteX, absoluteY );
	}

	public static WorldCell FromWorldPosition( Vector2 position )
	{
		var worldSize = WorldSettings.Instance.WorldSizeInCell();
		var cellSize = WorldSettings.Instance.CellSize;

		var maxX = worldSize.x * cellSize;
		var maxY = worldSize.y * cellSize;

		var clampedXyPosition = new Vector2(
			MathF.Max( 0f, MathF.Min( maxX, position.x ) ),
			MathF.Max( 0f, MathF.Min( maxY, position.y ) )
		);

		var cellX = clampedXyPosition.x / cellSize;
		var cellY = clampedXyPosition.y / cellSize;
		return FromAbsoluteXy( (int) MathF.Round( cellX ), (int) MathF.Round( cellY ) );
	}

	[Property] public Vector2Int Xy { get; private set; }
	
	private WorldCell( int x, int y )
	{
		Xy = new Vector2Int( x, y );
	}

	public Vector2 CenterWorldPosition()
	{
		return new Vector2( Xy.x * WorldSettings.Instance.CellSize, Xy.y * WorldSettings.Instance.CellSize );
	}

	public Vector2 NorthWestWorldPosition()
	{
		var centerWorldPosition = CenterWorldPosition();
		return centerWorldPosition + new Vector2( -WorldSettings.Instance.CellSize / 2, WorldSettings.Instance.CellSize / 2 );
	}

	public Vector2 NorthEastWorldPosition()
	{
		var centerWorldPosition = CenterWorldPosition();
		return centerWorldPosition + new Vector2( WorldSettings.Instance.CellSize / 2, WorldSettings.Instance.CellSize / 2 );
	}

	public Vector2 SouthWestWorldPosition()
	{
		var centerWorldPosition = CenterWorldPosition();
		return centerWorldPosition + new Vector2( -WorldSettings.Instance.CellSize / 2, -WorldSettings.Instance.CellSize / 2 );
	}

	public Vector2 SouthEastWorldPosition()
	{
		var centerWorldPosition = CenterWorldPosition();
		return centerWorldPosition + new Vector2( WorldSettings.Instance.CellSize / 2, -WorldSettings.Instance.CellSize / 2 );
	}
	
	public int SquaredDistance( WorldCell otherWorldCell )
	{
		var num = otherWorldCell.Xy.x - Xy.x;
		var num2 = otherWorldCell.Xy.y - Xy.y;

		return num * num + num2 * num2;
	}

	public int ClampedX()
	{
		var worldSize = WorldSettings.Instance.WorldSizeInCell();
		return System.Math.Max( 0, System.Math.Min( worldSize.x - 1, Xy.x ) );
	}
	
	public int ClampedY()
	{
		var worldSize = WorldSettings.Instance.WorldSizeInCell();
		return System.Math.Max( 0, System.Math.Min( worldSize.y - 1, Xy.y ) );
	}

	public byte NorthWestCornerHeight()
	{
		var nw = WorldHeightCoordinate.FromWorldCellNorthWestCorner( this );
		return WorldHeightManager.Instance.GetSimulationHeight( nw ); 
	}
	
	public byte NorthEastCornerHeight()
	{
		var ne = WorldHeightCoordinate.FromWorldCellNorthEastCorner( this );
		return WorldHeightManager.Instance.GetSimulationHeight( ne ); 
	}
	
	public byte SouthWestCornerHeight()
	{
		var sw = WorldHeightCoordinate.FromWorldCellSouthWestCorner( this );
		return WorldHeightManager.Instance.GetSimulationHeight( sw ); 
	}
	
	public byte SouthEastCornerHeight()
	{
		var se = WorldHeightCoordinate.FromWorldCellSouthEastCorner( this );
		return WorldHeightManager.Instance.GetSimulationHeight( se ); 
	}
	
	public bool IsFlat()
	{
		var nwHeight = NorthWestCornerHeight();
		var neHeight = NorthEastCornerHeight();
		var swHeight = SouthWestCornerHeight();
		var seHeight = SouthEastCornerHeight();

		return nwHeight == neHeight && neHeight == swHeight && swHeight == seHeight;
	}

	public bool IsAnyCornerUnderWater()
	{
		var nwHeight = NorthWestCornerHeight();
		var neHeight = NorthEastCornerHeight();
		var swHeight = SouthWestCornerHeight();
		var seHeight = SouthEastCornerHeight();

		var seaHeight = WorldSettings.Instance.SeaLevel;

		return nwHeight <= seaHeight || neHeight <= seaHeight || swHeight <= seaHeight || seHeight <= seaHeight;
	}

	public static bool operator ==( WorldCell a, WorldCell b )
	{
		return a.Xy == b.Xy;
	}
	
	public static bool operator !=( WorldCell a, WorldCell b )
	{
		return a.Xy != b.Xy;
	}
	
	public override string ToString()
	{
		return $"WorldCell(x={Xy.x}, y={Xy.y})";
	}

	public bool Equals( WorldCell other )
	{
		return Xy == other.Xy;
	}

	public override bool Equals( object obj )
	{
		return obj is WorldCell other && Equals( other );
	}

	public override int GetHashCode()
	{
		return Xy.GetHashCode();
	}
}
