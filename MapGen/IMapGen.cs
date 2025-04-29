namespace MapGen;

public interface IMapGen {
	public void GenerateMap(FULLCQGAME game, int seed);

	public uint GetBestSystemNumber(FULLCQGAME game, uint approxNumber);

	public int GetPosibleSystemNumbers(FULLCQGAME game, int[] list);
}
