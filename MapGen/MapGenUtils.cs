namespace MapGen;

public static class MapGenUtils {
	public static readonly int GRIDSIZE = 4096; // (also defined in GridVector.h)
	public static readonly int MAX_SYS_SIZE = GRIDSIZE * 64; //0x1FFFF   ( also defined in DSector.h )
	public static readonly float PLANETSIZE = GRIDSIZE * 2.0f;
	public static readonly int MAX_MAP_GRID = 64;
	public static readonly int MAX_MAP_SIZE = MAX_MAP_GRID * GRIDSIZE;
	public static readonly uint RND_MAX_PLAYER_SYSTEMS = 8;
	public static readonly int[] rndPlayerX = { 0, 2, 5, 8, 9, 7, 4, 1 };
	public static readonly int[] rndPlayerY = { 4, 1, 0, 2, 5, 8, 9, 7 };
	public static readonly uint RND_MAX_REMOTE_SYSTEMS = 20;
	public static readonly int[] rndRemoteX = { 4, 6, 1, 3, 5, 7, 2, 4, 6, 8, 1, 3, 5, 7, 2, 4, 6, 8, 3, 5 };
	public static readonly int[] rndRemoteY = { 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 6, 6, 6, 6, 7, 7 };
	public static readonly int RING_MAX_SYSTEMS = 16;
	public static readonly int[] ringSystemX = { 5, 6, 8, 7, 9, 7, 7, 5, 4, 3, 1, 2, 0, 2, 2, 4 };
	public static readonly int[] ringSystemY = { 0, 2, 2, 4, 5, 6, 8, 7, 9, 7, 7, 5, 4, 3, 1, 2 };
	public static readonly int STAR_MAX_TREE = 8;
	public static readonly int starCenterX = 4;
	public static readonly int starCenterY = 4;
	public static readonly int MAX_SYSTEMS = 16;
	public static readonly int MAX_PLAYERS = 8;


	public static readonly int[,] starTreeX =
		{ { 4, 3, 5 }, { 6, 7, 8 }, { 6, 8, 8 }, { 6, 8, 7 }, { 4, 5, 3 }, { 2, 1, 0 }, { 2, 0, 0 }, { 2, 0, 1 } };

	public static readonly int[,] starTreeY =
		{ { 2, 0, 0 }, { 2, 0, 1 }, { 4, 3, 5 }, { 6, 7, 8 }, { 6, 8, 8 }, { 6, 8, 7 }, { 4, 5, 3 }, { 2, 1, 0 } };

	//the random numbers caqn be understood to be in fixed 15 floating point.
	public static readonly int FIX15 = 15;

	public static Random rand = null;

	public static void InitializeRandom(int seed) {
		rand = new Random(seed);
		Console.WriteLine($"MAP GENERATION SEED = {seed}");
	}
	
	private static uint linearFunc() {
		return (uint)rand.Next(0, short.MaxValue + 1);
	}

	private static uint lessIsLikelyFunc() {
		uint v = linearFunc();

		v = v * v >> FIX15;
		v = v * v >> FIX15;
		return v;
	}

	private static uint moreIsLikelyFunc() {
		uint v = linearFunc();

		v = v * v >> FIX15;
		v = v * v >> FIX15;

		return 0x00007FFF - v;
	}

	public static Func<uint>[] randFunc = { linearFunc, lessIsLikelyFunc, moreIsLikelyFunc };

	public static readonly byte GENMAP_TAKEN = 1;
	public static readonly byte GENMAP_LEVEL1 = 2;
	public static readonly byte GENMAP_LEVEL2 = 3;
	public static readonly int GENMAP_PATH = 4;
	public static readonly int FLAG_PLANET = 0x01;
	public static readonly int FLAG_PATHON = 0x02;
	public static readonly int MAX_FLAGS = 20;

	public class FlagPost {
		public int type = 0;
		public int xPos = 0;
		public int yPos = 0;
	};

	public class GenSystem {
		public FlagPost[] flags;
		public uint numFlags = 0;

		public int sectorGridX = 0;
		public int sectorGridY = 0;
		public int index = 0;

		public uint planetNameCount = 0;

		public uint x = 0;
		public uint y = 0;
		public uint size = 0;
		public int jumpgateCount = 0;

		public uint[] distToSystems = new uint[MAX_SYSTEMS];

		public GenJumpgate[] jumpgates = new GenJumpgate[MAX_SYSTEMS];
		public int systemID = 0;
		public int playerID = 0;
		public int connectionOrder = 0;
		public uint[,] playerDistToSystems = new uint[MAX_PLAYERS, MAX_SYSTEMS];
		public _terrainTheme theme = new ();

		public uint omStartEmpty = 0;
		public uint omUsed = 0;
		public byte[,] objectMap = new byte[MAX_MAP_GRID, MAX_MAP_GRID];

		public GenSystem() {
			flags = new FlagPost[MAX_FLAGS];
			for (int i = 0; i < flags.Length; i++) {
				flags[i] = new FlagPost();
			}
		}
		
		public void initObjectMap() {
			omUsed = 0;
			omStartEmpty = 0;
			int centerDist = (int)(size / 2);
			int centerBoarder = centerDist - 1;
			int centerBoarder2 = centerBoarder * centerBoarder;
			int centerDist2 = centerDist * centerDist;
			for (int i = 0; i < (int)size; ++i) {
				for (int j = 0; j < (int)size; ++j) {
					int dist = (i - centerDist) * (i - centerDist) + (j - centerDist) * (j - centerDist);
					if (dist >= centerDist2) {
						objectMap[i, j] = GENMAP_TAKEN;
					} else if (dist >= centerBoarder2) {
						objectMap[i, j] = GENMAP_LEVEL1;
					} else {
						++omStartEmpty;
						objectMap[i, j] = 0;
					}
				}
			}
		}
	}

	public class GenJumpgate() {
		public GenSystem system1 = new();
		public GenSystem system2 = new();

		public int x1 = 0;
		public int y1 = 0;
		public int x2 = 0;
		public int y2 = 0;

		public int dist = 0;
		public bool created = false; // 1 BIT
	};

	public class GenStruct() {
		public BT_MAP_GEN data = new();
		public int numPlayers = 0;
		public uint[] sectorGrid = new uint[17]; //17 hight, use a shift to get the width.
		public uint gameSize = 0;
		public DMapGen.SECTOR_FORMATION sectorLayout = DMapGen.SECTOR_FORMATION.SF_RANDOM;
		public int systemsToMake = 0;
		public byte terrainSize = 0;
		public GenSystem[] systems = new GenSystem[MAX_SYSTEMS];
		public int systemCount = 0;
		public GenJumpgate[] jumpgate = new GenJumpgate[MAX_SYSTEMS * MAX_SYSTEMS];
		public uint numJumpGates = 0;
	};
}
