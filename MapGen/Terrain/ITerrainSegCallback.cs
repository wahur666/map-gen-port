namespace MapGen.Terrain;

public interface ITerrainSegCallback {
	public bool TerrainCallback (FootprintInfo info, GRIDVECTOR pos);		// return false to stop callback
}
