namespace MapGen.Terrain;

public class PathNode {
	public float g = 0f;
	public float h = 0f;
	public float f = 0f;

	public PathNode? pNext = null;
	public PathNode? pPrev = null;

	public uint priorityID = 0;

	public CellRef? currentCell = null;
	public PathNode? parentNode = null;
}
