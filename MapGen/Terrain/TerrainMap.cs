namespace MapGen.Terrain;

public class TerrainMap {
	public const int MAX_MAP_SIZE = 64;
	public const int MAX_FOOTPRINT = 16;
	public const float AXIAL_LENGTH = 1.0f;
	public const float DIAGONAL_LENGTH = 1.41421f;
	public const float INFTY = 10000000.0f;
	public const float INFTY_LITE = 4000000.0f;

	public const int OPEN_START = 1;
	public const int CLOSED_START = 2;

	public static readonly BlockAllocator<FootprintInfo> _blockAllocator = new();

	private void ResetTerrainBlockAllocator() {
		_blockAllocator.Reset();
	}

}
