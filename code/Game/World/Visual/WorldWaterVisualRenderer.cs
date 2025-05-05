using Sandbox;
using TycoonTerrain.Game.World.Data;

namespace TycoonTerrain.Game.World.Visual;

public class WorldWaterVisualRenderer : Component
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
			GameObjectContainer.Name = "Water";
			GameObjectContainer.SetParent( GameObject );
			
			GameObjectContainer.Tags.Add( TycoonTerrain.Tags.World );
			GameObjectContainer.Tags.Add( TycoonTerrain.Tags.Water );
		}

		var waterModel = GenerateWaterModel();
		if ( !waterModel.IsValid() )
			return;

		var modelRenderer = GameObjectContainer.GetOrAddComponent<ModelRenderer>();
		modelRenderer.Model = GenerateWaterModel();

		modelRenderer.SceneObject.Flags.IsTranslucent = true;
		modelRenderer.SceneObject.Flags.IsOpaque = false;
	}

	private Model GenerateWaterModel()
	{
		var mesh = CreateWaterMesh();
		if ( !mesh.IsValid() )
			return null;

		return new ModelBuilder().AddMesh( mesh ).Create();
	}

	private Mesh CreateWaterMesh()
	{
		var vb = new VertexBuffer();
		vb.Init( true );

		var createdAnyCell = false;

		for ( var y = 0; y < WorldSettings.Instance.ChunkSizeY; y++ )
		{
			for ( var x = 0; x < WorldSettings.Instance.ChunkSizeX; x++ )
			{
				var worldCell = WorldCell.FromLocalXy( x, y, WorldChunk.Xy.x, WorldChunk.Xy.y );

				var didCreateCell = CreateWaterWorldCell( vb, worldCell );
				if ( didCreateCell )
					createdAnyCell = true;
			}
		}

		if ( !createdAnyCell )
			return null;

		var mesh = new Mesh();
		mesh.CreateBuffers( vb );
		mesh.Material = Material.Load( "world/transport_water.vmat" );

		return mesh;
	}

	/**
	 * True if we created a cell, false if we didn't create a cell
	 */
	private bool CreateWaterWorldCell( VertexBuffer vb, WorldCell worldCell )
	{
		var seaLevelSimulationHeight = WorldSettings.Instance.SeaLevel;

		if ( !worldCell.IsAnyCornerUnderWater() )
			return false;

		var seaLevelPhysicalHeight = WorldHeightManager.Instance.GetPhysicalHeight( seaLevelSimulationHeight );
		seaLevelPhysicalHeight += WorldSettings.Instance.SeaHeightOffset;

		var northWestCorner = new Vector3( worldCell.NorthWestWorldPosition(), seaLevelPhysicalHeight );
		var northEastCorner = new Vector3( worldCell.NorthEastWorldPosition(), seaLevelPhysicalHeight );
		var southWestCorner = new Vector3( worldCell.SouthWestWorldPosition(), seaLevelPhysicalHeight );
		var southEastCorner = new Vector3( worldCell.SouthEastWorldPosition(), seaLevelPhysicalHeight );

		AddTriangle( vb, northWestCorner, southWestCorner, northEastCorner );
		AddTriangle( vb, northEastCorner, southWestCorner, southEastCorner );

		return true;
	}

	private void AddTriangle( VertexBuffer vb, Vector3 p1, Vector3 p2, Vector3 p3 )
	{
		var normal = Vector3.Cross( p2 - p1, p3 - p1 ).Normal;

		vb.Add( CreateVertex( vb, p1, normal ) );
		vb.Add( CreateVertex( vb, p2, normal ) );
		vb.Add( CreateVertex( vb, p3, normal ) );

		vb.AddTriangleIndex( 3, 2, 1 );
	}

	private Vertex CreateVertex( VertexBuffer vb, Vector3 position, Vector3 normal )
	{
		var v = vb.Default;
		v.Position = position;
		v.Normal = normal;

		return v;
	}
}
