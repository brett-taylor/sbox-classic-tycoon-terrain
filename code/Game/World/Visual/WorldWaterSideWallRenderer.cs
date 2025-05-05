using Sandbox;
using TycoonTerrain.Game.World.Data;
using Component = Sandbox.Component;

namespace TycoonTerrain.Game.World.Visual;

public class WorldWaterSideWallRenderer : Component
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
			GameObjectContainer.Name = "WaterSideWall";
			GameObjectContainer.SetParent( GameObject );
		}

		var modelRenderer = GameObjectContainer.GetOrAddComponent<ModelRenderer>();
		modelRenderer.Model = GenerateEdgeModel();
				
		modelRenderer.SceneObject.Flags.IsTranslucent = true;
		modelRenderer.SceneObject.Flags.IsOpaque = false;

		if ( !modelRenderer.Model.IsValid() )
			modelRenderer.Enabled = false;
	}
	
	private Model GenerateEdgeModel()
	{
		var vb = new VertexBuffer();
		vb.Init( true );

		var includesWestWall = false;
		var includesEastWall = false;
		var includesSouthWall = false;
		var includesNorthWall = false;
		
		if ( WorldChunk.IsWestEdgeChunk() )
			includesWestWall = AddWestEdgeMesh( vb );
		
		if ( WorldChunk.IsSouthEdgeChunk() )
			includesSouthWall = AddSouthEdgeMesh( vb );
		
		if ( WorldChunk.IsEastEdgeChunk() )
			includesEastWall = AddEastEdgeMesh( vb );

		if ( WorldChunk.IsNorthEdgeChunk() )
			includesNorthWall = AddNorthEdgeMesh( vb );

		if ( !includesWestWall && !includesSouthWall && !includesEastWall && !includesNorthWall )
			return null;
		
		var mesh = new Mesh();
		mesh.CreateBuffers( vb );
		mesh.Material = Material.Load( "world/transport_water_edge.vmat" );

		return new ModelBuilder()
			.AddMesh( mesh )
			.Create();
	}

	private bool AddWestEdgeMesh( VertexBuffer vb )
	{
		var containedWater = false;
		
		for ( var y = 0; y < WorldSettings.Instance.ChunkSizeY; y++ )
		{
			var worldCell = WorldCell.FromLocalXy( 0, y, WorldChunk );
			var northWestCorner = worldCell.NorthWestWorldPosition();
			var southWestCorner = worldCell.SouthWestWorldPosition();

			var whm = WorldHeightManager.Instance;
			var northWestHeightSimulationHeight = worldCell.NorthWestCornerHeight();
			var southWestHeightSimulationHeight = worldCell.SouthWestCornerHeight();
			
			var seaLevel = WorldSettings.Instance.SeaLevel;
			if ( northWestHeightSimulationHeight > seaLevel && southWestHeightSimulationHeight > seaLevel )
				continue;
			
			containedWater = true;

			var seaLevelPhysicalHeight = whm.GetPhysicalHeight( seaLevel );
			var northWestCornerHeightPhysicalHeight = whm.GetPhysicalHeight( northWestHeightSimulationHeight );
			var southWestCornerHeightPhysicalHeight = whm.GetPhysicalHeight( southWestHeightSimulationHeight );
			
			var pointA = new Vector3( northWestCorner, seaLevelPhysicalHeight + WorldSettings.Instance.SeaHeightOffset );
			var pointB = new Vector3( southWestCorner, seaLevelPhysicalHeight + WorldSettings.Instance.SeaHeightOffset );
			var pointC = new Vector3( southWestCorner, southWestCornerHeightPhysicalHeight > seaLevelPhysicalHeight ? seaLevelPhysicalHeight : southWestCornerHeightPhysicalHeight );
			var pointD = new Vector3( northWestCorner, northWestCornerHeightPhysicalHeight > seaLevelPhysicalHeight ? seaLevelPhysicalHeight : northWestCornerHeightPhysicalHeight );

			AddQuad( vb, pointD, pointC, pointB, pointA, Vector3.Backward );
		}
		
		return containedWater;
	}

	private bool AddEastEdgeMesh( VertexBuffer vb )
	{
		var containedWater = false;

		for ( var y = 0; y < WorldSettings.Instance.ChunkSizeY; y++ )
		{
			var worldCell = WorldCell.FromLocalXy( WorldSettings.Instance.ChunkSizeX - 1, y, WorldChunk );
			var northEastCorner = worldCell.NorthEastWorldPosition();
			var southEastCorner = worldCell.SouthEastWorldPosition();
			
			var whm = WorldHeightManager.Instance;
			var northEastHeightSimulationHeight = worldCell.NorthEastCornerHeight();
			var southEastHeightSimulationHeight = worldCell.SouthEastCornerHeight();
			
			var seaLevel = WorldSettings.Instance.SeaLevel;
			if ( northEastHeightSimulationHeight > seaLevel && southEastHeightSimulationHeight > seaLevel )
				continue;
			
			containedWater = true;
			
			var seaLevelPhysicalHeight = whm.GetPhysicalHeight( seaLevel );
			var northWestCornerHeightPhysicalHeight = whm.GetPhysicalHeight( northEastHeightSimulationHeight );
			var southWestCornerHeightPhysicalHeight = whm.GetPhysicalHeight( southEastHeightSimulationHeight );
			
			var pointA = new Vector3( northEastCorner, seaLevelPhysicalHeight + WorldSettings.Instance.SeaHeightOffset );
			var pointB = new Vector3( southEastCorner, seaLevelPhysicalHeight + WorldSettings.Instance.SeaHeightOffset );
			var pointC = new Vector3( southEastCorner, southWestCornerHeightPhysicalHeight > seaLevelPhysicalHeight ? seaLevelPhysicalHeight : southWestCornerHeightPhysicalHeight );
			var pointD = new Vector3( northEastCorner, northWestCornerHeightPhysicalHeight > seaLevelPhysicalHeight ? seaLevelPhysicalHeight : northWestCornerHeightPhysicalHeight );

			AddQuad( vb, pointA, pointB, pointC, pointD, Vector3.Forward );
		}

		return containedWater;
	}

	private bool AddSouthEdgeMesh( VertexBuffer vb )
	{
		var containedWater = false;

		for ( var x = 0; x < WorldSettings.Instance.ChunkSizeY; x++ )
		{
			var worldCell = WorldCell.FromLocalXy( x, 0, WorldChunk );
			var southWestCorner = worldCell.SouthWestWorldPosition();
			var southEastCorner = worldCell.SouthEastWorldPosition();
			
			var whm = WorldHeightManager.Instance;
			var southWestHeightSimulationHeight = worldCell.SouthWestCornerHeight();
			var southEastHeightSimulationHeight = worldCell.SouthEastCornerHeight();
			
			var seaLevel = WorldSettings.Instance.SeaLevel;
			if ( southWestHeightSimulationHeight > seaLevel && southEastHeightSimulationHeight > seaLevel )
				continue;
			
			containedWater = true;

			var seaLevelPhysicalHeight = whm.GetPhysicalHeight( seaLevel );
			var southWestCornerHeightPhysicalHeight = whm.GetPhysicalHeight( southWestHeightSimulationHeight );
			var southEastCornerHeightPhysicalHeight = whm.GetPhysicalHeight( southEastHeightSimulationHeight );
			
			var pointA = new Vector3( southWestCorner, seaLevelPhysicalHeight + WorldSettings.Instance.SeaHeightOffset );
			var pointB = new Vector3( southEastCorner, seaLevelPhysicalHeight + WorldSettings.Instance.SeaHeightOffset );
			var pointC = new Vector3( southEastCorner, southEastCornerHeightPhysicalHeight > seaLevelPhysicalHeight ? seaLevelPhysicalHeight : southEastCornerHeightPhysicalHeight );
			var pointD = new Vector3( southWestCorner, southWestCornerHeightPhysicalHeight > seaLevelPhysicalHeight ? seaLevelPhysicalHeight : southWestCornerHeightPhysicalHeight  );
			

			AddQuad( vb, pointD, pointC, pointB, pointA, Vector3.Right );
		}
		
		return containedWater;
	}

	private bool AddNorthEdgeMesh( VertexBuffer vb )
	{
		var containedWater = false;

		for ( var x = 0; x < WorldSettings.Instance.ChunkSizeY; x++ )
		{
			var worldCell = WorldCell.FromLocalXy( x, WorldSettings.Instance.ChunkSizeY - 1, WorldChunk );
			var northWestCorner = worldCell.NorthWestWorldPosition();
			var northEastCorner = worldCell.NorthEastWorldPosition();

			var whm = WorldHeightManager.Instance;
			var northWestHeightSimulationHeight = worldCell.NorthWestCornerHeight();
			var northEastHeightSimulationHeight = worldCell.NorthEastCornerHeight();
			
			var seaLevel = WorldSettings.Instance.SeaLevel;
			if ( northWestHeightSimulationHeight > seaLevel && northEastHeightSimulationHeight > seaLevel )
				continue;
			
			containedWater = true;
			
			var seaLevelPhysicalHeight = whm.GetPhysicalHeight( seaLevel );
			var northWestCornerHeightPhysicalHeight = whm.GetPhysicalHeight( northWestHeightSimulationHeight );
			var northEastCornerHeightPhysicalHeight = whm.GetPhysicalHeight( northEastHeightSimulationHeight );
			
			var pointA = new Vector3( northWestCorner, seaLevelPhysicalHeight + WorldSettings.Instance.SeaHeightOffset );
			var pointB = new Vector3( northEastCorner, seaLevelPhysicalHeight + WorldSettings.Instance.SeaHeightOffset );
			var pointC = new Vector3( northEastCorner, northEastCornerHeightPhysicalHeight > seaLevelPhysicalHeight ? seaLevelPhysicalHeight : northEastCornerHeightPhysicalHeight  );
			var pointD = new Vector3( northWestCorner, northWestCornerHeightPhysicalHeight > seaLevelPhysicalHeight ? seaLevelPhysicalHeight : northWestCornerHeightPhysicalHeight  );
			
			AddQuad( vb, pointA, pointB, pointC, pointD, Vector3.Left );

		}

		return containedWater;
	}
	
	private void AddQuad( VertexBuffer vb, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Vector3 normal )
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
