using Sandbox;
using TycoonTerrain.Cache.Biome;
using TycoonTerrain.Game.World.Data;

namespace TycoonTerrain.Game.World.Visual;

public class WorldGroundVisualRenderer : Component
{
	public WorldChunk WorldChunk { get; set; }
	
	private GameObject GameObjectContainer { get; set; }

	protected override void OnStart()
	{
		base.OnStart();

		Render();
	}
	
	private void Render()
	{
		if ( !GameObjectContainer.IsValid() )
		{
			GameObjectContainer = new GameObject();
			GameObjectContainer.Name = "Ground";
			GameObjectContainer.SetParent( GameObject );
			
			GameObjectContainer.Tags.Add( TycoonTerrain.Tags.World );
			GameObjectContainer.Tags.Add( TycoonTerrain.Tags.Ground );
		}
		
		var modelRenderer = GameObjectContainer.GetOrAddComponent<ModelRenderer>();
		modelRenderer.Model = GenerateGroundModel();
	}

	private Model GenerateGroundModel()
	{
		return new ModelBuilder()
			.AddMesh( CreateGroundMesh() )
			.Create();
	}
	
	private Mesh CreateGroundMesh()
	{
		var vb = new VertexBuffer();
		vb.Init( true );
		
		for ( var y = 0; y < WorldSettings.Instance.ChunkSizeY; y++ )
		{
			for ( var x = 0; x < WorldSettings.Instance.ChunkSizeX; x++ )
			{
				var worldCell = WorldCell.FromLocalXy( x, y, WorldChunk.Xy.x, WorldChunk.Xy.y );
				CreateWorldCell( vb, worldCell );
			}
		}

		var mesh = new Mesh();
		mesh.CreateBuffers( vb );
		mesh.Material = Material.Load( "world/transport_terrain.vmat" );
		
		return mesh;
	}

	private void CreateWorldCell( VertexBuffer vb, WorldCell worldCell )
	{
		var northWestHeight = worldCell.NorthWestCornerHeight();
		var northEastHeight = worldCell.NorthEastCornerHeight();
		var southWestHeight = worldCell.SouthWestCornerHeight();
		var southEastHeight = worldCell.SouthEastCornerHeight();
		
		var isNorthWestHill = northWestHeight != northEastHeight && northEastHeight == southWestHeight && southWestHeight == southEastHeight;
		if ( isNorthWestHill )
		{
			CreateNorthWestHillWorldCell( vb, worldCell, northEastHeight, northWestHeight );
			return;
		}
		
		var isNorthEastHill = northWestHeight != northEastHeight && northWestHeight == southWestHeight && southWestHeight == southEastHeight;
		if ( isNorthEastHill )
		{
			CreateNorthEastHillWorldCell( vb, worldCell, northWestHeight, northEastHeight );
			return;
		}
		
		var isSouthWestHill = northWestHeight == northEastHeight && northWestHeight != southWestHeight && northWestHeight == southEastHeight;
		if ( isSouthWestHill )
		{
			CreateSouthWestHillWorldCell( vb, worldCell, northWestHeight, southWestHeight );
			return;
		}
		
		var isSouthEastHill = northWestHeight == northEastHeight && northWestHeight == southWestHeight && southWestHeight != southEastHeight;
		if ( isSouthEastHill )
		{
			CreateSouthEastHillWorldCell( vb, worldCell, northWestHeight, southEastHeight );
			return;
		}
		
		CreateWorldCell( vb, worldCell, northWestHeight, northEastHeight, southWestHeight, southEastHeight );
	}

	private void CreateWorldCell( VertexBuffer vb, WorldCell worldCell, byte northWestCornerSimulationHeight, byte northEastCornerSimulationHeight, byte southWestCornerSimulationHeight, byte southEastCornerSimulationHeight )
	{
		var northWestCorner = new Vector3( 
			worldCell.NorthWestWorldPosition(), 
			WorldHeightManager.Instance.GetPhysicalHeight( northWestCornerSimulationHeight ) 
		);
		
		var northEastCorner = new Vector3( 
			worldCell.NorthEastWorldPosition(), 
			WorldHeightManager.Instance.GetPhysicalHeight( northEastCornerSimulationHeight ) 
		);
		
		var southWestCorner = new Vector3( 
			worldCell.SouthWestWorldPosition(), 
			WorldHeightManager.Instance.GetPhysicalHeight( southWestCornerSimulationHeight ) 
		);
		
		var southEastCorner = new Vector3( 
			worldCell.SouthEastWorldPosition(), 
			WorldHeightManager.Instance.GetPhysicalHeight( southEastCornerSimulationHeight ) 
		);
		
		var groundColor = WorldSurfaceColorManager.Instance.GetSurfaceColorAt( worldCell );
		AddTriangle( vb, northWestCorner, southWestCorner, northEastCorner, groundColor );
		AddTriangle( vb, northEastCorner, southWestCorner, southEastCorner, groundColor );
	}

	private void CreateNorthWestHillWorldCell( VertexBuffer vb, WorldCell worldCell, byte footSimulationHeight, byte hillSimulationHeight )
	{
		var hillHeight = WorldHeightManager.Instance.GetPhysicalHeight( hillSimulationHeight );
		var footHeight = WorldHeightManager.Instance.GetPhysicalHeight( footSimulationHeight );
		var northWestCorner = new Vector3( worldCell.NorthWestWorldPosition(), hillHeight );
		var northEastCorner = new Vector3( worldCell.NorthEastWorldPosition(), footHeight );
		var southWestCorner = new Vector3( worldCell.SouthWestWorldPosition(), footHeight );
		var southEastCorner = new Vector3( worldCell.SouthEastWorldPosition(), footHeight );

		var groundColor = WorldSurfaceColorManager.Instance.GetSurfaceColorAt( worldCell );
		AddTriangle( vb, northWestCorner, southWestCorner, northEastCorner, groundColor );
		AddTriangle( vb, northEastCorner, southWestCorner, southEastCorner, groundColor );
	}
	
	private void CreateNorthEastHillWorldCell( VertexBuffer vb, WorldCell worldCell, byte footSimulationHeight, byte hillSimulationHeight )
	{
		var hillHeight = WorldHeightManager.Instance.GetPhysicalHeight( hillSimulationHeight );
		var footHeight = WorldHeightManager.Instance.GetPhysicalHeight( footSimulationHeight );
		var northWestCorner = new Vector3( worldCell.NorthWestWorldPosition(), footHeight );
		var northEastCorner = new Vector3( worldCell.NorthEastWorldPosition(), hillHeight );
		var southWestCorner = new Vector3( worldCell.SouthWestWorldPosition(), footHeight );
		var southEastCorner = new Vector3( worldCell.SouthEastWorldPosition(), footHeight );
		
		var groundColor = WorldSurfaceColorManager.Instance.GetSurfaceColorAt( worldCell );		
		AddTriangle( vb, northWestCorner, southEastCorner, northEastCorner, groundColor );
		AddTriangle( vb, northWestCorner, southWestCorner, southEastCorner, groundColor );
	}
	
	private void CreateSouthWestHillWorldCell( VertexBuffer vb, WorldCell worldCell, byte footSimulationHeight, byte hillSimulationHeight )
	{
		var hillHeight = WorldHeightManager.Instance.GetPhysicalHeight( hillSimulationHeight );
		var footHeight = WorldHeightManager.Instance.GetPhysicalHeight( footSimulationHeight );
		var northWestCorner = new Vector3( worldCell.NorthWestWorldPosition(), footHeight );
		var northEastCorner = new Vector3( worldCell.NorthEastWorldPosition(), footHeight );
		var southWestCorner = new Vector3( worldCell.SouthWestWorldPosition(), hillHeight );
		var southEastCorner = new Vector3( worldCell.SouthEastWorldPosition(), footHeight );

		var groundColor = WorldSurfaceColorManager.Instance.GetSurfaceColorAt( worldCell );
		AddTriangle( vb, northWestCorner, southEastCorner, northEastCorner, groundColor );
		AddTriangle( vb, northWestCorner, southWestCorner, southEastCorner, groundColor );
	}

	private void CreateSouthEastHillWorldCell( VertexBuffer vb, WorldCell worldCell, byte footSimulationHeight, byte hillSimulationHeight )
	{
		var hillHeight = WorldHeightManager.Instance.GetPhysicalHeight( hillSimulationHeight );
		var footHeight = WorldHeightManager.Instance.GetPhysicalHeight( footSimulationHeight );
		var northWestCorner = new Vector3( worldCell.NorthWestWorldPosition(), footHeight );
		var northEastCorner = new Vector3( worldCell.NorthEastWorldPosition(), footHeight );
		var southWestCorner = new Vector3( worldCell.SouthWestWorldPosition(), footHeight );
		var southEastCorner = new Vector3( worldCell.SouthEastWorldPosition(), hillHeight );

		var groundColor = WorldSurfaceColorManager.Instance.GetSurfaceColorAt( worldCell );
		AddTriangle( vb, northWestCorner, southWestCorner, northEastCorner, groundColor );
		AddTriangle( vb, northEastCorner, southWestCorner, southEastCorner, groundColor );
	}

	private void AddTriangle( VertexBuffer vb, Vector3 p1, Vector3 p2, Vector3 p3, Color color )
	{
		var normal = Vector3.Cross( p2 - p1, p3 - p1 ).Normal;
		
		vb.Add( CreateVertex( vb, p1, normal, color ) );
		vb.Add( CreateVertex( vb, p2, normal, color ) );
		vb.Add( CreateVertex( vb, p3, normal, color ) );

		vb.AddTriangleIndex( 3, 2, 1 );
	}
	
	private Vertex CreateVertex( VertexBuffer vb, Vector3 position, Vector3 normal, Color color )
	{
		var v = vb.Default;
		v.Color = color;
		v.Position = position;
		v.Normal = normal;
		
		return v;
	}
}
