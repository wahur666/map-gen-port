namespace MapGen;

public static class DMapGen {
	public enum DMAP_FUNC {
		LINEAR = 0,
		LESS_IS_LIKLY,
		MORE_IS_LIKLY
	};

	public enum PLACEMENT {
		RANDOM = 0,
		SPOTS = 4,
	};

	public enum OVERLAP {
		NO_OVERLAP = 0,
		LEVEL1, //can overlap another LEVEL1 or LEVEL2
		LEVEL2 //may be overlaped by a level1
	};

	public enum SECTOR_SIZE {
		SMALL_SIZE = 0x01,
		MEDIUM_SIZE = 0x02,
		LARGE_SIZE = 0x04,
		S_M_SIZE = 0x03,
		S_L_SIZE = 0x05,
		M_L_SIZE = 0x06,
		ALL_SIZE = 0x07
	};

	public enum SECTOR_FORMATION {
		SF_RANDOM,
		SF_RING,
		SF_STAR,
		SF_MULTI_RANDOM
	};

	public enum MACRO_OPERATION {
		MC_PLACE_HABITABLE_PLANET,
		MC_PLACE_GAS_PLANET,
		MC_PLACE_METAL_PLANET,
		MC_PLACE_OTHER_PLANET,
		MC_PLACE_TERRAIN,
		MC_PLACE_PLAYER_BOMB,
		MC_MARK_RING
	};
};
