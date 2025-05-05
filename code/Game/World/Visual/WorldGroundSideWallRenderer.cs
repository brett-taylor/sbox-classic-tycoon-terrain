using Sandbox;
using TycoonTerrain.Game.World.Data;

namespace TycoonTerrain.Game.World.Visual;

public class WorldGroundSideWallRenderer : Component
{
	public WorldChunk WorldChunk { get; set; }

	private GameObject GameObjectContainer { get; set; }

	protected override void OnStart()
	{
		base.OnStart();

		if ( WorldChunk.IsEdgeChunk() )
			Render();
		else
			Destroy();
	}

	private void Render()
	{
		if ( !GameObjectContainer.IsValid() )
		{
			GameObjectContainer = new GameObject();
			GameObjectContainer.Name = "GroundSideWall";
			GameObjectContainer.SetParent( GameObject );
		}

		var modelRenderer = GameObjectContainer.GetOrAddComponent<ModelRenderer>();
		modelRenderer.Model = GenerateEdgeModel();
	}

	private Model GenerateEdgeModel()
	{
		var vb = new VertexBuffer();
		vb.Init( true );

		if ( WorldChunk.IsWestEdgeChunk() )
			AddWestEdgeMesh( vb );

		if ( WorldChunk.IsSouthEdgeChunk() )
			AddSouthEdgeMesh( vb );

		if ( WorldChunk.IsNorthEdgeChunk() )
			AddNorthEdgeMesh( vb );

		if ( WorldChunk.IsEastEdgeChunk() )
			AddEastEdgeMesh( vb );

		var mesh = new Mesh();
		mesh.CreateBuffers( vb );
		mesh.Material = Material.Load( "world/transport_world_edge.vmat" );

		return new ModelBuilder()
			.AddMesh( mesh )
			.Create();
	}

	private void AddWestEdgeMesh( VertexBuffer vb )
	{
		for ( var y = 0; y < WorldSettings.Instance.ChunkSizeY; y++ )
		{
			var worldCell = WorldCell.FromLocalXy( 0, y, WorldChunk );
			var northWestCorner = worldCell.NorthWestWorldPosition();
			var southWestCorner = worldCell.SouthWestWorldPosition();

			var northWestCornerHeight = WorldHeightManager.Instance.GetPhysicalHeight( worldCell.NorthWestCornerHeight() );
			var southWestCornerHeight = WorldHeightManager.Instance.GetPhysicalHeight( worldCell.SouthWestCornerHeight() );

			var pointA = new Vector3( northWestCorner, northWestCornerHeight );
			var pointB = new Vector3( southWestCorner, southWestCornerHeight );
			var pointC = new Vector3( southWestCorner, 0f ) + Vector3.Down * WorldSettings.Instance.SideWallHeight;
			var pointD = new Vector3( northWestCorner, 0f ) + Vector3.Down * WorldSettings.Instance.SideWallHeight;

			AddEdgeWall( vb, pointD, pointC, pointB, pointA, Vector3.Backward );
		}
	}

	private void AddEastEdgeMesh( VertexBuffer vb )
	{
		for ( var y = 0; y < WorldSettings.Instance.ChunkSizeY; y++ )
		{
			var worldCell = WorldCell.FromLocalXy( WorldSettings.Instance.ChunkSizeX - 1, y, WorldChunk );
			var northEastCorner = worldCell.NorthEastWorldPosition();
			var southEastCorner = worldCell.SouthEastWorldPosition();

			var northEastCornerHeight = WorldHeightManager.Instance.GetPhysicalHeight( worldCell.NorthEastCornerHeight() );
			var southEastCornerHeight = WorldHeightManager.Instance.GetPhysicalHeight( worldCell.SouthEastCornerHeight() );

			var pointA = new Vector3( northEastCorner, northEastCornerHeight );
			var pointB = new Vector3( southEastCorner, southEastCornerHeight );
			var pointC = new Vector3( southEastCorner, 0f ) + Vector3.Down * WorldSettings.Instance.SideWallHeight;
			var pointD = new Vector3( northEastCorner, 0f ) + Vector3.Down * WorldSettings.Instance.SideWallHeight;

			AddEdgeWall( vb, pointA, pointB, pointC, pointD, Vector3.Forward );
		}
	}

	private void AddSouthEdgeMesh( VertexBuffer vb )
	{
		for ( var x = 0; x < WorldSettings.Instance.ChunkSizeY; x++ )
		{
			var worldCell = WorldCell.FromLocalXy( x, 0, WorldChunk );
			var southWestCorner = worldCell.SouthWestWorldPosition();
			var southEastCorner = worldCell.SouthEastWorldPosition();

			var southWestCornerHeight = WorldHeightManager.Instance.GetPhysicalHeight( worldCell.SouthWestCornerHeight() );
			var southEastCornerHeight = WorldHeightManager.Instance.GetPhysicalHeight( worldCell.SouthEastCornerHeight() );

			var pointA = new Vector3( southWestCorner, southWestCornerHeight );
			var pointB = new Vector3( southEastCorner, southEastCornerHeight );
			var pointC = new Vector3( southEastCorner, 0f ) + Vector3.Down * WorldSettings.Instance.SideWallHeight;
			var pointD = new Vector3( southWestCorner, 0f ) + Vector3.Down * WorldSettings.Instance.SideWallHeight;

			AddEdgeWall( vb, pointD, pointC, pointB, pointA, Vector3.Right );
		}
	}

	private void AddNorthEdgeMesh( VertexBuffer vb )
	{
		for ( var x = 0; x < WorldSettings.Instance.ChunkSizeY; x++ )
		{
			var worldCell = WorldCell.FromLocalXy( x, WorldSettings.Instance.ChunkSizeY - 1, WorldChunk );
			var northWestCorner = worldCell.NorthWestWorldPosition();
			var northEastCorner = worldCell.NorthEastWorldPosition();

			var northWestCornerHeight = WorldHeightManager.Instance.GetPhysicalHeight( worldCell.NorthWestCornerHeight() );
			var northEastCornerHeight = WorldHeightManager.Instance.GetPhysicalHeight( worldCell.NorthEastCornerHeight() );

			var pointA = new Vector3( northWestCorner, northWestCornerHeight );
			var pointB = new Vector3( northEastCorner, northEastCornerHeight );
			var pointC = new Vector3( northEastCorner, 0f ) + Vector3.Down * WorldSettings.Instance.SideWallHeight;
			var pointD = new Vector3( northWestCorner, 0f ) + Vector3.Down * WorldSettings.Instance.SideWallHeight;

			AddEdgeWall( vb, pointA, pointB, pointC, pointD, Vector3.Left );
		}
	}

	private void AddEdgeWall( VertexBuffer vb, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Vector3 normal )
	{
		vb.Add( CreateVertex( vb, p1, normal ) );
		vb.Add( CreateVertex( vb, p2, normal ) );
		vb.Add( CreateVertex( vb, p3, normal ) );
		vb.Add( CreateVertex( vb, p4, normal ) );

		vb.AddTriangleIndex( 4, 3, 2 );
		vb.AddTriangleIndex( 2, 1, 4 );
	}

	private Vertex CreateVertex( VertexBuffer vb, Vector3 position, Vector3 normal )
	{
		var v = vb.Default;
		v.Position = position;
		v.Normal = normal;

		return v;
	}
}
