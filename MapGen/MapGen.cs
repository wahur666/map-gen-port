using System.Numerics;
using U32 = uint;

namespace MapGen;

using U8 = byte;
using S32 = int;
using SINGLE = float;

public class MapGen : IMapGen {
	private Random rand;

	/* IMapGen methods */

	public void GenerateMap(FULLCQGAME game, S32 seed) {
		rand = new Random(seed);
		Console.WriteLine($"MAP GENERATION SEED = {seed}");

		//init the map struct to set up the generation
		MapGenUtils.GenStruct map = default;

		initMap(ref map, game);
		GenerateSystems(ref map);
		SelectThemes(ref map);
		CreateSystems(ref map);
		RunHomeMacros(ref map);
		CreateJumpgates(ref map);

		PopulateSystems(ref map);
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

	void initMap(ref MapGenUtils.GenStruct map, FULLCQGAME game) {
	}

	void insertObject(char[] obj, Vector3 position, U32 playerID, U32 systemID, ref MapGenUtils.GenSystem system) {
	}

	//Util funcs

	U32 GetRand(U32 min, U32 max, DMapGen.DMAP_FUNC mapFunc) {
		return 0;
	}

	void GenerateSystems(ref MapGenUtils.GenStruct map) {
	}

	void generateSystemsRandom(ref MapGenUtils.GenStruct map) {
	}

	void generateSystemsRing(ref MapGenUtils.GenStruct map) {
	}

	void generateSystemsStar(ref MapGenUtils.GenStruct map) {
	}

	bool SystemsOverlap(ref MapGenUtils.GenStruct map, ref MapGenUtils.GenSystem system) {
		return false;
	}

	void GetJumpgatePositions(ref MapGenUtils.GenStruct map, ref MapGenUtils.GenSystem sys1,
		ref MapGenUtils.GenSystem sys2,
		out U32 jx1, out U32 jy1, out U32 jx2, out U32 jy2) {
		jx1 = 0;
		jy1 = 0;
		jx2 = 0;
		jy2 = 0;
	}

	bool CrossesAnotherSystem(ref MapGenUtils.GenStruct map, ref MapGenUtils.GenSystem sys1,
		ref MapGenUtils.GenSystem sys2,
		U32 jx1, U32 jy1, U32 jx2, U32 jy2) {
		return false;
	}

	bool CrossesAnotherLink(ref MapGenUtils.GenStruct map, ref MapGenUtils.GenJumpgate gate) {
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

	void SelectThemes(ref MapGenUtils.GenStruct map) {
	}

	void CreateSystems(ref MapGenUtils.GenStruct map) {
	}

	void RunHomeMacros(ref MapGenUtils.GenStruct map) {
	}

	bool findMacroPosition(ref MapGenUtils.GenSystem system, S32 centerX, S32 centerY, U32 range, U32 size,
		DMapGen.OVERLAP overlap, out U32 posX, out U32 posY) {
		posX = 0;
		posY = 0;
		return false;
	}

	void getMacroCenterPos(ref MapGenUtils.GenSystem system, out U32 x, out U32 y) {
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
	void createPlanetFromList(S32 xPos, S32 yPos, ref MapGenUtils.GenSystem system, U32 range, char[,] planetList) {
	}

	void placeMacroTerrain(S32 centerX, S32 centerY, ref MapGenUtils.GenSystem system, S32 range,
		ref _terrainInfo terrainInfo) {
	}

	void CreateJumpgates(ref MapGenUtils.GenStruct map) {
	}

	void createRandomGates3(ref MapGenUtils.GenStruct map) {
	}


	void createGateLevel2(ref MapGenUtils.GenStruct map, U32 totalLevel, U32 levelSystems, U32 targetSystems,
		U32 gateNum, U32[] currentGates, U32 score,
		U32[] bestGates, out U32 bestScore, out U32 bestGateNum, bool moreAllowed) {
		bestScore = 0;
		bestGateNum = 0;
	}

	void createRandomGates2(ref MapGenUtils.GenStruct map) {
	}

	void createGateLevel(ref MapGenUtils.GenStruct map, U32 totalLevel, U32 levelSystems, U32 targetSystems,
		U32 gateNum, U32[] currentGates, U32 score,
		U32[] bestGates, ref U32 bestScore, ref U32 bestGateNum, bool moreAllowed) {
	}

	U32 scoreGate(ref MapGenUtils.GenStruct map, U32 gateIndex) {
		return 0;
	}

	void markSystems(ref U32 systemUnconnected, MapGenUtils.GenSystem[] system, ref U32 systemsVisited) { }

	void createRandomGates(ref MapGenUtils.GenStruct map) { }

	void createRingGates(ref MapGenUtils.GenStruct map) { }
	void createStarGates(ref MapGenUtils.GenStruct map) { }
	void createJumpgatesForIndexs(ref MapGenUtils.GenStruct map, U32 index1, U32 index2) { }

	void PopulateSystems(ref MapGenUtils.GenStruct map) { }

	void PopulateSystem(ref MapGenUtils.GenStruct map, MapGenUtils.GenSystem[] system) {
	}

	void placePlanetsMoons(ref MapGenUtils.GenStruct map, MapGenUtils.GenSystem[] system, U32 planetPosX,
		U32 planetPosY) {
	}

	bool SpaceEmpty(ref MapGenUtils.GenSystem system, U32 xPos, U32 yPos, DMapGen.OVERLAP overlap, U32 size) {
		return false;
	}

	void FillPosition(ref MapGenUtils.GenSystem system, U32 xPos, U32 yPos, U32 size, DMapGen.OVERLAP overlap) {
	}

	bool FindPosition(MapGenUtils.GenSystem[] system, U32 width, DMapGen.OVERLAP overlap, ref U32 xPos, ref U32 yPos) {
		return false;
	}

//	bool ColisionWithObject(GenObj * obj,Vector vect,U32 rad,MAP_GEN_ENUM::OVERLAP overlap);

	void GenerateTerain(ref MapGenUtils.GenStruct map, MapGenUtils.GenSystem[] system) {
	}

	void PlaceTerrain(ref MapGenUtils.GenStruct map, ref _terrainInfo terrain, ref MapGenUtils.GenSystem system) {
	}

	void PlaceRandomField(ref _terrainInfo terrain, U32 numToPlace, S32 startX, S32 startY,
		ref MapGenUtils.GenSystem system) {
	}

	void PlaceSpottyField(ref _terrainInfo terrain, U32 numToPlace, S32 startX, S32 startY,
		ref MapGenUtils.GenSystem system) {
	}

	void PlaceRingField(ref _terrainInfo terrain, ref MapGenUtils.GenSystem system) {
	}

	void PlaceRandomRibbon(ref _terrainInfo terrain, U32 length, S32 startX, S32 startY,
		ref MapGenUtils.GenSystem system) {
	}

	void BuildPaths(ref MapGenUtils.GenSystem system) {
	}

	//other
	void init() { }

	void removeFromArray(U32 nx, U32 ny, U32[] tempX, U32[] tempY, out U32 tempIndex) {
		tempIndex = 0;
	}

	bool isInArray(U32[] arrX, U32[] arrY, U32 index, U32 nx, U32 ny) {
		return false;
	}

	bool isOverlapping(U32[] arrX, U32[] arrY, U32 index, U32 nx, U32 ny) {
		return false;
	}

	void checkNewXY(U32[] tempX, U32[] tempY, ref U32 tempIndex, U32[] finalX, U32[] finalY, U32 finalIndex,
		_terrainInfo terrain, ref MapGenUtils.GenSystem system, U32 newX, U32 newY) {
	}

	void connectPosts(ref MapGenUtils.FlagPost post1, ref MapGenUtils.FlagPost post2,
		ref MapGenUtils.GenSystem system) {
	}
}
