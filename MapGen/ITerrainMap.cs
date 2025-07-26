namespace MapGen;

public interface ITerrainMap {
	public void SetWorldRect(RECT worldRect);

	public void SetFootprint(GRIDVECTOR squares, int numSquares, FootprintInfo info);

	public void UndoFootprint(GRIDVECTOR squares, int numSquares, FootprintInfo info);

	// enumerates all elements in a square, call with from == to to enumerate all elements of one square
	// returns false if the user's callback function stopped the enum early, else returns true.
	public bool TestSegment(GRIDVECTOR from, GRIDVECTOR to, ITerrainSegCallback callback);

	public int FindPath(GRIDVECTOR from, GRIDVECTOR to, uint dwMissionID, uint flags, IFindPathCallback callback);

	public void RenderEdit(); // want the terrain map to draw stuff in editor mode

	public bool IsGridEmpty(GRIDVECTOR grid, uint dwIgnoreMissionID, bool bFullSquare = true);

	public bool IsParkedAtGrid(GRIDVECTOR grid, uint dwMissionID, bool bFullSquare);

	public bool IsGridValid(GRIDVECTOR grid);

	public bool IsGridInSystem(GRIDVECTOR grid);

	public uint GetFieldID(GRIDVECTOR grid);

	public bool IsOkForBuilding(GRIDVECTOR grid, bool checkParkedUnits, bool bFullSquare);
}
