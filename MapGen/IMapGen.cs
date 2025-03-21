namespace MapGen;

using U32 = UInt32;
using S32 = int;

public interface IMapGen {
	public void GenerateMap(FULLCQGAME game, S32 seed);

	public U32 GetBestSystemNumber(FULLCQGAME game, U32 approxNumber);

	public U32 GetPosibleSystemNumbers(FULLCQGAME game, List<U32> list);
}
