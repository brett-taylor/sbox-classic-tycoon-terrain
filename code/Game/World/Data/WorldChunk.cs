using System;
using Sandbox;

namespace TycoonTerrain.Game.World.Data;

public struct WorldChunk : IEquatable<WorldChunk>
{
	public static WorldChunk From( int x, int y )
	{
		return new WorldChunk( x, y );
	}

	public static WorldChunk FromWorldPosition( Vector2 position )
	{
		var cell = WorldCell.FromWorldPosition( position );
		var chunkX = cell.Xy.x / WorldSettings.Instance.ChunkSizeX;
		var chunkY = cell.Xy.y / WorldSettings.Instance.ChunkSizeY;
		
		return From( chunkX, chunkY );
	}

	[Property] public Vector2Int Xy { get; private set; }
	
	private WorldChunk( int x, int y )
	{
		Xy = new Vector2Int( x, y );
	}

	public bool IsNorthEdgeChunk()
	{
		return Xy.y == WorldSettings.Instance.ChunkCountY - 1;
	}

	public bool IsSouthEdgeChunk()
	{
		return Xy.y == 0;
	}

	public bool IsWestEdgeChunk()
	{
		return Xy.x == 0;
	}

	public bool IsEastEdgeChunk()
	{
		return Xy.x == WorldSettings.Instance.ChunkCountX - 1;
	}

	public bool IsEdgeChunk()
	{
		return IsNorthEdgeChunk() || IsSouthEdgeChunk() || IsWestEdgeChunk() || IsEastEdgeChunk();
	}

	public bool IsValid()
	{
		var isInXRange = Xy.x >= 0 && Xy.x < WorldSettings.Instance.ChunkCountX;
		var isInYRange = Xy.y >= 0 && Xy.y < WorldSettings.Instance.ChunkCountY;
		return isInXRange && isInYRange;
	}
	
	public override string ToString()
	{
		return $"WorldChunk(x={Xy.x}, y={Xy.y})";
	}

	public bool Equals( WorldChunk other )
	{
		return Xy == other.Xy;
	}

	public override bool Equals( object obj )
	{
		return obj is WorldChunk other && Equals( other );
	}

	public override int GetHashCode()
	{
		return Xy.GetHashCode();
	}
}
