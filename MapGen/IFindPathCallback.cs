namespace MapGen;

public interface IFindPathCallback {
	public void SetPath (ITerrainMap map, GRIDVECTOR squares, int numSquares);
}
