namespace MapGen;

using U32 = UInt32;

public interface IMapGen {
	public void GenerateMap(FULLCQGAME game, U32 seed);

	public U32 GetBestSystemNumber(FULLCQGAME game, U32 approxNumber);

	public U32 GetPosibleSystemNumbers(FULLCQGAME game, List<U32> list);
}
