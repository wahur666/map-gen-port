﻿namespace MapGen;

using S32 = Int32;
using U32 = UInt32;
using U8 = Byte;

public static class MapGenUtils {
	public static readonly int GRIDSIZE = 4096; // (also defined in GridVector.h)
	public static readonly int MAX_SYS_SIZE = GRIDSIZE * 64; //0x1FFFF   ( also defined in DSector.h )
	public static readonly float PLANETSIZE = GRIDSIZE * 2.0f;
	public static readonly int MAX_MAP_GRID = 64;
	public static readonly int MAX_MAP_SIZE = MAX_MAP_GRID * GRIDSIZE;
	public static readonly int RND_MAX_PLAYER_SYSTEMS = 8;
	public static readonly U32[] rndPlayerX = { 0, 2, 5, 8, 9, 7, 4, 1 };
	public static readonly U32[] rndPlayerY = { 4, 1, 0, 2, 5, 8, 9, 7 };
	public static readonly int RND_MAX_REMOTE_SYSTEMS = 20;
	public static readonly U32[] rndRemoteX = { 4, 6, 1, 3, 5, 7, 2, 4, 6, 8, 1, 3, 5, 7, 2, 4, 6, 8, 3, 5 };
	public static readonly U32[] rndRemoteY = { 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 6, 6, 6, 6, 7, 7 };
	public static readonly int RING_MAX_SYSTEMS = 16;
	public static readonly U32[] ringSystemX = { 5, 6, 8, 7, 9, 7, 7, 5, 4, 3, 1, 2, 0, 2, 2, 4 };
	public static readonly U32[] ringSystemY = { 0, 2, 2, 4, 5, 6, 8, 7, 9, 7, 7, 5, 4, 3, 1, 2 };
	public static readonly int STAR_MAX_TREE = 8;
	public static readonly U32 starCenterX = 4;
	public static readonly U32 starCenterY = 4;
	public static readonly int MAX_SYSTEMS = 16;
	public static readonly int MAX_PLAYERS = 8;


	public static readonly U32[,] starTreeX =
		{ { 4, 3, 5 }, { 6, 7, 8 }, { 6, 8, 8 }, { 6, 8, 7 }, { 4, 5, 3 }, { 2, 1, 0 }, { 2, 0, 0 }, { 2, 0, 1 } };

	public static readonly U32[,] starTreeY =
		{ { 2, 0, 0 }, { 2, 0, 1 }, { 4, 3, 5 }, { 6, 7, 8 }, { 6, 8, 8 }, { 6, 8, 7 }, { 4, 5, 3 }, { 2, 1, 0 } };

	//the random numbers caqn be understood to be in fixed 15 floating point.
	public static readonly int FIX15 = 15;

	private static Random rand = new();

	private static U32 linearFunc() {
		return (U32)rand.Next(0, short.MaxValue + 1);
	}

	private static U32 lessIsLikelyFunc() {
		U32 v = linearFunc();

		v = v * v >> FIX15;
		v = v * v >> FIX15;
		return v;
	}

	private static U32 moreIsLikelyFunc() {
		U32 v = linearFunc();

		v = v * v >> FIX15;
		v = v * v >> FIX15;

		return 0x00007FFF - v;
	}

	public static Func<U32>[] randFunc = { linearFunc, lessIsLikelyFunc, moreIsLikelyFunc };

	public static readonly byte GENMAP_TAKEN = 1;
	public static readonly byte GENMAP_LEVEL1 = 2;
	public static readonly int GENMAP_LEVEL2 = 3;
	public static readonly int GENMAP_PATH = 4;
	public static readonly int FLAG_PLANET = 0x01;
	public static readonly int FLAG_PATHON = 0x02;
	public static readonly int MAX_FLAGS = 20;

	public struct FlagPost() {
		public U8 type = 0;
		public U8 xPos = 0;
		public U8 yPos = 0;
	};

	public struct GenSystem() {
		public FlagPost[] flags = new FlagPost[MAX_FLAGS];
		public U32 numFlags = 0;

		public U32 sectorGridX = 0;
		public U32 sectorGridY = 0;
		public U32 index = 0;

		public U32 planetNameCount = 0;

		public U32 x = 0;
		public U32 y = 0;
		public U32 size = 0;
		public U32 jumpgateCount = 0;

		public U32[] distToSystems = new U32[MAX_SYSTEMS];

		public GenJumpgate[] jumpgates = new GenJumpgate[MAX_SYSTEMS];
		public U32 systemID = 0;
		public U32 playerID = 0;
		public U32 connectionOrder = 0;
		public U32[,] playerDistToSystems = new U32[MAX_PLAYERS, MAX_SYSTEMS];
		public _terrainTheme theme;

		public U32 omStartEmpty = 0;
		public U32 omUsed = 0;
		public U8[,] objectMap = new U8[MAX_MAP_GRID, MAX_MAP_GRID];

		public void initObjectMap() {
			omUsed = 0;
			omStartEmpty = 0;
			S32 centerDist = (S32)(size / 2);
			S32 centerBoarder = centerDist - 1;
			S32 centerBoarder2 = centerBoarder * centerBoarder;
			S32 centerDist2 = centerDist * centerDist;
			for (S32 i = 0; i < (S32)size; ++i) {
				for (S32 j = 0; j < (S32)size; ++j) {
					S32 dist = (i - centerDist) * (i - centerDist) + (j - centerDist) * (j - centerDist);
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

	public struct GenJumpgate() {
		public GenSystem system1 = new();
		public GenSystem system2 = new();

		public U32 x1 = 0;
		public U32 y1 = 0;
		public U32 x2 = 0;
		public U32 y2 = 0;

		public U32 dist = 0;
		bool created = false; // 1 BIT
	};

	public struct GenStruct() {
		public BT_MAP_GEN data = new();

		public U32 numPlayers = 0;

		public U32 sectorSize = 0;
		public U32[] sectorGrid = new U32[17]; //17 hight, use a shift to get the width.

		public U32 gameSize = 0;
		public DMapGen.SECTOR_FORMATION sectorLayout = DMapGen.SECTOR_FORMATION.SF_RANDOM;

		public U32 systemsToMake = 0;

		public U8 terrainSize = 0;

		public S32 objectBoarder = 0;

		public GenSystem[] systems = new GenSystem[MAX_SYSTEMS];
		public U32 systemCount = 0;

		public GenJumpgate[] jumpgate = new GenJumpgate[MAX_SYSTEMS * MAX_SYSTEMS];
		public U32 numJumpGates = 0;
	};
}
