using System.Numerics;
using U32 = uint;

namespace MapGen;

using U8 = byte;
using S32 = int;
using SINGLE = float;

public class MapGen(Globals globals, BT_MAP_GEN mapgen) : IMapGen {
	private Random rand;
	private BT_MAP_GEN _mapGen = mapgen;

	private Globals _globals = globals;
	/* IMapGen methods */

	public void GenerateMap(FULLCQGAME game, S32 seed) {
		rand = new Random(seed);
		Console.WriteLine($"MAP GENERATION SEED = {seed}");

		//init the map struct to set up the generation
		MapGenUtils.GenStruct map = new();

		initMap(map, game);
		GenerateSystems(map);
		SelectThemes(map);
		CreateSystems(map);
		RunHomeMacros(map);
		CreateJumpgates(map);

		PopulateSystems(map);
	}

	public U32 GetBestSystemNumber(FULLCQGAME game, U32 approxNumber) {
		U32 numPlayers = 0;

		U32[] assignments = new U32[CQGAME.MAX_PLAYERS + 1];
		memset<U32>(assignments, 0, assignments.Length);
		for (S32 i = 0; i < (S32)game.activeSlots; ++i) {
			if ((game.slot[i].state == STATE.READY) || (game.slot[i].state == STATE.ACTIVE))
				assignments[(int)game.slot[i].color] = 1;
		}

		for (S32 i = 1; i <= CQGAME.MAX_PLAYERS; i++)
			numPlayers += assignments[i];

		if (game.templateType == RANDOM_TEMPLATE.TEMPLATE_RANDOM ||
		    game.templateType == RANDOM_TEMPLATE.TEMPLATE_NEW_RANDOM) {
			return Math.Max(numPlayers, approxNumber);
		} else if (game.templateType == RANDOM_TEMPLATE.TEMPLATE_RING) {
			return Math.Max(numPlayers * (approxNumber / numPlayers), numPlayers);
		} else if (game.templateType == RANDOM_TEMPLATE.TEMPLATE_STAR) {
			if (numPlayers < 6) {
				for (U32 i = 3; i > 0; --i) {
					U32 number = 1 + (i * numPlayers);
					if (number <= approxNumber)
						return number;
				}

				return 1 + numPlayers;
			} else if (numPlayers < 8) {
				for (U32 i = 2; i > 0; --i) {
					U32 number = 1 + (i * numPlayers);
					if (number <= approxNumber)
						return number;
				}

				return 1 + numPlayers;
			} else {
				return 9;
			}
		}

		return approxNumber;
	}

	private static void memset<T>(T[] arr, T value, int length) {
		for (int i = 0; i < length; i++) {
			arr[i] = value;
		}
	}

	public U32 GetPosibleSystemNumbers(FULLCQGAME game, List<uint> list) {
		throw new NotImplementedException();
	}

	//map gen stuff

	void initMap(MapGenUtils.GenStruct map, FULLCQGAME game) {
		BT_MAP_GEN data = _mapGen;
		S32 i;
		for(i = 0; i <17;++i)
		{
			map.sectorGrid[i] = 0;
		}

		map.sectorLayout = game.templateType switch {
			RANDOM_TEMPLATE.TEMPLATE_RANDOM => DMapGen.SECTOR_FORMATION.SF_RANDOM,
			RANDOM_TEMPLATE.TEMPLATE_NEW_RANDOM => DMapGen.SECTOR_FORMATION.SF_MULTI_RANDOM,
			RANDOM_TEMPLATE.TEMPLATE_RING => DMapGen.SECTOR_FORMATION.SF_RING,
			RANDOM_TEMPLATE.TEMPLATE_STAR => DMapGen.SECTOR_FORMATION.SF_STAR,
			_ => map.sectorLayout
		};

		map.data = data;
		
		map.systemCount = 0;
		map.numJumpGates = 0;
		
		map.numPlayers = 0;
		
		U32[] assignments = new U32[CQGAME.MAX_PLAYERS+1];
		memset<U32>(assignments, 0, assignments.Length);
		for(i = 0; i < game.activeSlots; ++i)
		{
			if (game.slot[i].state == STATE.READY)
				assignments[(int)game.slot[i].color] = 1;
		}
		
		for (i = 1; i <= CQGAME.MAX_PLAYERS; i++)
			map.numPlayers += assignments[i];

		map.gameSize = game.mapSize switch {
			MAPSIZE.SMALL_MAP => 0,
			MAPSIZE.MEDIUM_MAP => 1,
			_ => 2
		};

		map.terrainSize = game.terrain switch {
			TERRAIN.LIGHT_TERRAIN => 0,
			TERRAIN.MEDIUM_TERRAIN => 1,
			_ => 2
		};

		map.systemsToMake = GetBestSystemNumber(game,game.numSystems);
		
		map.sectorSize = 8;
		_globals.SetFileMaxPlayers(map.numPlayers);
	}

	void insertObject(char[] obj, Vector3 position, U32 playerID, U32 systemID, ref MapGenUtils.GenSystem system) {
	}

	//Util funcs

	U32 GetRand(U32 min, U32 max, DMapGen.DMAP_FUNC mapFunc) {
		throw new NotImplementedException();

		return 0;
	}

	void GenerateSystems(MapGenUtils.GenStruct map) {
		throw new NotImplementedException();

	}

	void generateSystemsRandom(MapGenUtils.GenStruct map) {
		throw new NotImplementedException();

	}

	void generateSystemsRing(MapGenUtils.GenStruct map) {
		throw new NotImplementedException();

	}

	void generateSystemsStar(MapGenUtils.GenStruct map) {
		throw new NotImplementedException();

	}

	bool SystemsOverlap(MapGenUtils.GenStruct map, MapGenUtils.GenSystem system) {
		throw new NotImplementedException();

		return false;
	}

	void GetJumpgatePositions(MapGenUtils.GenStruct map, MapGenUtils.GenSystem sys1,
		MapGenUtils.GenSystem sys2,
		out U32 jx1, out U32 jy1, out U32 jx2, out U32 jy2) {
		throw new NotImplementedException();

		jx1 = 0;
		jy1 = 0;
		jx2 = 0;
		jy2 = 0;
	}

	bool CrossesAnotherSystem(MapGenUtils.GenStruct map, MapGenUtils.GenSystem sys1,
		MapGenUtils.GenSystem sys2,
		U32 jx1, U32 jy1, U32 jx2, U32 jy2) {
		throw new NotImplementedException();

		return false;
	}

	bool CrossesAnotherLink(MapGenUtils.GenStruct map, MapGenUtils.GenJumpgate gate) {
		throw new NotImplementedException();

		return false;
	}

	bool LinesCross(S32 minX1, S32 minY1, S32 maxX1, S32 maxY1, S32 minX2, S32 minY2, S32 maxX2, S32 maxY2) {
		SINGLE deltaX1 = maxX1 - minX1;
		SINGLE deltaY1 = maxY1 - minY1;
		SINGLE deltaX2 = maxX2 - minX2;
		SINGLE deltaY2 = maxY2 - minY2;

		SINGLE delta = deltaX1 * deltaY2 - deltaY1 * deltaX2;

		if (MathF.Abs(delta) < 0.00001f) return false;

		SINGLE mu1 = ((minX2 - minX1) * deltaY2 - (minY2 - minY1) * deltaX2) / delta;
		SINGLE mu2 = ((minX1 - minX2) * deltaY1 - (minY1 - minY2) * deltaX1) / -delta;

		return (mu1 >= 0.0f && mu1 <= 1.0f && mu2 >= 0.0f && mu2 <= 1.0f);
	}

	void SelectThemes(MapGenUtils.GenStruct map) {
		throw new NotImplementedException();

	}

	void CreateSystems(MapGenUtils.GenStruct map) {
		throw new NotImplementedException();

	}

	void RunHomeMacros(MapGenUtils.GenStruct map) {
		throw new NotImplementedException();

	}

	bool findMacroPosition(MapGenUtils.GenSystem system, S32 centerX, S32 centerY, U32 range, U32 size,
		DMapGen.OVERLAP overlap, out U32 posX, out U32 posY) {
		throw new NotImplementedException();

		posX = 0;
		posY = 0;
		return false;
	}

	void getMacroCenterPos(ref MapGenUtils.GenSystem system, out U32 x, out U32 y) {
		throw new NotImplementedException();

		x = 0;
		y = 0;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="xPos"></param>
	/// <param name="yPos"></param>
	/// <param name="system"></param>
	/// <param name="range"></param>
	/// <param name="planetList">planetList[][GT_PATH]</param>
	void createPlanetFromList(S32 xPos, S32 yPos, MapGenUtils.GenSystem system, U32 range, char[,] planetList) {
	}

	void placeMacroTerrain(S32 centerX, S32 centerY, MapGenUtils.GenSystem system, S32 range,
		_terrainInfo terrainInfo) {
		throw new NotImplementedException();

	}

	void CreateJumpgates(MapGenUtils.GenStruct map) {
		throw new NotImplementedException();

	}

	void createRandomGates3(MapGenUtils.GenStruct map) {
		throw new NotImplementedException();

	}


	void createGateLevel2(MapGenUtils.GenStruct map, U32 totalLevel, U32 levelSystems, U32 targetSystems,
		U32 gateNum, U32[] currentGates, U32 score,
		U32[] bestGates, out U32 bestScore, out U32 bestGateNum, bool moreAllowed) {
		throw new NotImplementedException();

		bestScore = 0;
		bestGateNum = 0;
	}

	void createRandomGates2(MapGenUtils.GenStruct map) {
		throw new NotImplementedException();

	}

	void createGateLevel(MapGenUtils.GenStruct map, U32 totalLevel, U32 levelSystems, U32 targetSystems,
		U32 gateNum, U32[] currentGates, U32 score,
		U32[] bestGates, ref U32 bestScore, ref U32 bestGateNum, bool moreAllowed) {
		throw new NotImplementedException();

	}

	U32 scoreGate(ref MapGenUtils.GenStruct map, U32 gateIndex) {
		throw new NotImplementedException();

		return 0;
	}

	void markSystems(ref U32 systemUnconnected, MapGenUtils.GenSystem[] system, ref U32 systemsVisited) {
		throw new NotImplementedException();

	}

	void createRandomGates(MapGenUtils.GenStruct map) {
		throw new NotImplementedException();

	}

	void createRingGates(MapGenUtils.GenStruct map) {
		throw new NotImplementedException();

	}

	void createStarGates(MapGenUtils.GenStruct map) {
		throw new NotImplementedException();

	}

	void createJumpgatesForIndexs(MapGenUtils.GenStruct map, U32 index1, U32 index2) {
		throw new NotImplementedException();

	}

	void PopulateSystems(MapGenUtils.GenStruct map) {
		throw new NotImplementedException();

	}

	void PopulateSystem(MapGenUtils.GenStruct map, MapGenUtils.GenSystem[] system) {
		throw new NotImplementedException();

	}

	void placePlanetsMoons(MapGenUtils.GenStruct map, MapGenUtils.GenSystem[] system, U32 planetPosX,
		U32 planetPosY) {
		throw new NotImplementedException();

	}

	bool SpaceEmpty(MapGenUtils.GenSystem system, U32 xPos, U32 yPos, DMapGen.OVERLAP overlap, U32 size) {
		throw new NotImplementedException();

		return false;
	}

	void FillPosition(MapGenUtils.GenSystem system, U32 xPos, U32 yPos, U32 size, DMapGen.OVERLAP overlap) {
		throw new NotImplementedException();

	}

	bool FindPosition(MapGenUtils.GenSystem[] system, U32 width, DMapGen.OVERLAP overlap, ref U32 xPos, ref U32 yPos) {
		throw new NotImplementedException();

		return false;
	}

//	bool ColisionWithObject(GenObj * obj,Vector vect,U32 rad,MAP_GEN_ENUM::OVERLAP overlap);

	void GenerateTerain(MapGenUtils.GenStruct map, MapGenUtils.GenSystem[] system) {
		throw new NotImplementedException();

	}

	void PlaceTerrain(MapGenUtils.GenStruct map, _terrainInfo terrain, MapGenUtils.GenSystem system) {
		throw new NotImplementedException();

	}

	void PlaceRandomField(_terrainInfo terrain, U32 numToPlace, S32 startX, S32 startY,
		ref MapGenUtils.GenSystem system) {
		throw new NotImplementedException();

	}

	void PlaceSpottyField(_terrainInfo terrain, U32 numToPlace, S32 startX, S32 startY,
		MapGenUtils.GenSystem system) {
		throw new NotImplementedException();

	}

	void PlaceRingField(_terrainInfo terrain, MapGenUtils.GenSystem system) {
		throw new NotImplementedException();

	}

	void PlaceRandomRibbon(_terrainInfo terrain, U32 length, S32 startX, S32 startY,
		MapGenUtils.GenSystem system) {
		throw new NotImplementedException();

	}

	void BuildPaths(MapGenUtils.GenSystem system) {
		throw new NotImplementedException();

	}

	//other
	void init() { }

	void removeFromArray(U32 nx, U32 ny, U32[] tempX, U32[] tempY, out U32 tempIndex) {
		throw new NotImplementedException();

		tempIndex = 0;
	}

	bool isInArray(U32[] arrX, U32[] arrY, U32 index, U32 nx, U32 ny) {
		throw new NotImplementedException();

		return false;
	}

	bool isOverlapping(U32[] arrX, U32[] arrY, U32 index, U32 nx, U32 ny) {
		throw new NotImplementedException();

		return false;
	}

	void checkNewXY(U32[] tempX, U32[] tempY, ref U32 tempIndex, U32[] finalX, U32[] finalY, U32 finalIndex,
		_terrainInfo terrain, MapGenUtils.GenSystem system, U32 newX, U32 newY) {
		throw new NotImplementedException();

	}

	void connectPosts(MapGenUtils.FlagPost post1, MapGenUtils.FlagPost post2,
		MapGenUtils.GenSystem system) {
		throw new NotImplementedException();

	}
}
