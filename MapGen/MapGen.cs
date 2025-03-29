using System.Diagnostics;
using System.Numerics;
using Newtonsoft.Json;
using U32 = uint;

namespace MapGen;

using U8 = byte;
using S32 = int;
using U64 = UInt64;
using SINGLE = float;

public class MapGen(Globals globals, BT_MAP_GEN mapgen) : IMapGen {
	private BT_MAP_GEN _mapGen = mapgen;

	private Globals _globals = globals;

	private static void CQASSERT(bool expression, string message) {
		if (expression) {
			return;
		}

		Console.WriteLine("CQASSERT triggered");
		if (Debugger.IsAttached) {
			// This will pause execution and activate the debugger
			Debugger.Break();
		} else {
			Console.WriteLine($"No debugger attached, passing by {message}");
		}
	}

	/* IMapGen methods */

	public void GenerateMap(FULLCQGAME game, S32 seed) {
		MapGenUtils.InitializeRandom(seed);

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
		for (i = 0; i < 17; ++i) {
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

		U32[] assignments = new U32[CQGAME.MAX_PLAYERS + 1];
		memset<U32>(assignments, 0, assignments.Length);
		for (i = 0; i < game.activeSlots; ++i) {
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

		map.systemsToMake = GetBestSystemNumber(game, game.numSystems);

		map.sectorSize = 8;
		_globals.SetFileMaxPlayers(map.numPlayers);
	}

	void insertObject(char[] obj, Vector3 position, U32 playerID, U32 systemID, MapGenUtils.GenSystem system) {
	}

	//Util funcs

	U32 GetRand(U32 min, U32 max, DMapGen.DMAP_FUNC mapFunc) {
		max++;
		U64 val = MapGenUtils.randFunc[(int)mapFunc]();
		val = (val * (max - min)) >> MapGenUtils.FIX15;
		val += min;
		CQASSERT(val >= min && val <= max - 1, val.ToString());
		return (U32)val;
	}

	void GenerateSystems(MapGenUtils.GenStruct map) {
		switch (map.sectorLayout) {
			case DMapGen.SECTOR_FORMATION.SF_RANDOM or DMapGen.SECTOR_FORMATION.SF_MULTI_RANDOM:
				generateSystemsRandom(map);
				break;
			case DMapGen.SECTOR_FORMATION.SF_RING:
				generateSystemsRing(map);
				break;
			case DMapGen.SECTOR_FORMATION.SF_STAR:
				generateSystemsStar(map);
				break;
		}
	}

	void generateSystemsRandom(MapGenUtils.GenStruct map) {
		U32 s1;

		for (s1 = 0; s1 < map.numPlayers; s1++) {
			map.systems[s1] = new MapGenUtils.GenSystem();
			MapGenUtils.GenSystem system1 = map.systems[s1];
			do {
				U32 val = GetRand(0, MapGenUtils.RND_MAX_PLAYER_SYSTEMS - 1, DMapGen.DMAP_FUNC.LINEAR);
				system1.sectorGridX = MapGenUtils.rndPlayerX[val];
				system1.sectorGridY = MapGenUtils.rndPlayerY[val];
				system1.connectionOrder = val;
			} while (SystemsOverlap(map, system1));

			map.sectorGrid[system1.sectorGridX] |= (uint)(0x00000001 << (int)system1.sectorGridY);

			system1.index = s1;
			system1.playerID = s1 + 1;

			map.systemCount++;
		}

		for (s1 = map.numPlayers; s1 < map.systemsToMake; s1++) {
			map.systems[s1] = new MapGenUtils.GenSystem();
			MapGenUtils.GenSystem system1 = map.systems[s1];
			do {
				U32 val = GetRand(0, MapGenUtils.RND_MAX_REMOTE_SYSTEMS - 1, DMapGen.DMAP_FUNC.LINEAR);
				system1.sectorGridX = MapGenUtils.rndRemoteX[val];
				system1.sectorGridY = MapGenUtils.rndRemoteY[val];
			} while (SystemsOverlap(map, system1));

			map.sectorGrid[system1.sectorGridX] |= (uint)(0x00000001 << (int)system1.sectorGridY);

			system1.index = s1;

			map.systemCount++;
		}
	}

	void generateSystemsRing(MapGenUtils.GenStruct map) {
		throw new NotImplementedException();
	}

	void generateSystemsStar(MapGenUtils.GenStruct map) {
		throw new NotImplementedException();
	}

	bool SystemsOverlap(MapGenUtils.GenStruct map, MapGenUtils.GenSystem system) {
		for (U32 count = 0; count < map.systemCount; ++count) {
			if ((((int)(map.sectorGrid[system.sectorGridX]) >> (int)system.sectorGridY) & 0x01) > 0)
				return true;
		}

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
		U32 playerThemeCount = 0;
		U32 i;
		for (i = 0; i < BT_MAP_GEN.MAX_THEMES; ++i) {
			if (map.data.themes[i].okForPlayerStart &&
			    ((int)map.data.themes[i].sizeOk & (0x01 << (int)map.gameSize)) > 0) {
				++playerThemeCount;
			}
		}

		U32 themeCount = 0;
		for (i = 0; i < BT_MAP_GEN.MAX_THEMES; ++i) {
			if (map.data.themes[i].okForRemoteSystem &&
			    ((int)map.data.themes[i].sizeOk & (0x01 << (int)map.gameSize)) > 0) {
				++themeCount;
			}
		}

		for (U32 s1 = 0; s1 < map.systemCount; s1++) {
			MapGenUtils.GenSystem system = map.systems[s1];

			if (system.playerID != 0) {
				U32 themeNumber = GetRand(1, playerThemeCount, DMapGen.DMAP_FUNC.LINEAR);
				U32 theme = 0;
				while (themeNumber > 0) {
					if (map.data.themes[theme].okForPlayerStart &&
					    ((int)map.data.themes[theme].sizeOk & (0x01 << (int)map.gameSize)) > 0) {
						--themeNumber;
					}

					++theme;
				}

				--theme;
				system.theme = map.data.themes[theme];
			} else {
				U32 themeNumber = GetRand(1, themeCount, DMapGen.DMAP_FUNC.LINEAR);
				U32 theme = 0;
				while (themeNumber > 0) {
					if (map.data.themes[theme].okForRemoteSystem &&
					    ((int)map.data.themes[theme].sizeOk & (0x01 << (int)map.gameSize)) > 0) {
						--themeNumber;
					}

					++theme;
				}

				--theme;
				system.theme = map.data.themes[theme];
			}

			system.size = GetRand(system.theme.minSize, system.theme.maxSize, system.theme.sizeFunc);
			system.x = GetRand(0, (uint)MapGenUtils.MAX_MAP_GRID - system.size, DMapGen.DMAP_FUNC.LINEAR);
			system.y = GetRand(0, (uint)MapGenUtils.MAX_MAP_GRID - system.size, DMapGen.DMAP_FUNC.LINEAR);

			system.initObjectMap();
		}
	}

	static void PrettyPrintByteArray(byte[,] array)
	{
		int rows = array.GetLength(0);
		int cols = array.GetLength(1);
		Console.WriteLine("[");
		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < cols; j++)
			{
				Console.Write(array[i, j].ToString("D1") + ",");
			}
			Console.WriteLine();
		}
		Console.WriteLine("]");
	}
	
	void CreateSystems(MapGenUtils.GenStruct map) {
		for(U32 s1 = 0; s1 < map.systemCount; ++s1)
		{
			MapGenUtils.GenSystem system = map.systems[s1];
			// system.systemID = SECTOR.CreateSystem(system->x*GRIDSIZE+system->sectorGridX*2*MAX_MAP_SIZE,system->y*GRIDSIZE+system->sectorGridY*2*MAX_MAP_SIZE,system->size*GRIDSIZE+(GRIDSIZE/2),system->size*GRIDSIZE+(GRIDSIZE/2));
			system.systemID = s1;
		}

		// U32 namesUsed = 0;
		// for(U32 i = 0; i < map.systemCount; ++i)
		// {
		// 	U32 name;
		// 	do
		// 	{
		// 		name = GetRand(1,24,MAP_GEN_ENUM::LINEAR);
		// 	}while(namesUsed & (0x01<< name));
		// 	namesUsed |= (0x01 << name);
		// 	SECTOR->SetSystemName(map.systems[i].systemID,IDS_BEGIN_SYSTEM_NAMES+name);
		// }
		//
		// OBJMAP->Init();
	}

	void RunHomeMacros(MapGenUtils.GenStruct map) {
		U32 xPos,yPos;
		throw new NotImplementedException();
		// for(U32 s1 = 0; s1 < map.systemCount; ++s1)
		// {
		// 	MapGenUtils.GenSystem system = map.systems[s1];
		// 	if(system.playerID > 0)
		// 	{
		// 		getMacroCenterPos(system,out xPos, out yPos);
		// 		for(U32 i = 0; i < MAX_MACROS;++i)
		// 		{
		// 			BT_MAP_GEN::_terrainTheme::_macros * macro = &(system->theme->macros[i]);
		// 			if(macro->active)
		// 			{
		// 				switch(macro->operation)
		// 				{
		// 					case MAP_GEN_ENUM::MC_PLACE_HABITABLE_PLANET:
		// 						createPlanetFromList(xPos,yPos,system,macro->range,system->theme->habitablePlanets);
		// 						break;
		// 					case MAP_GEN_ENUM::MC_PLACE_GAS_PLANET:
		// 						createPlanetFromList(xPos,yPos,system,macro->range,system->theme->gasPlanets);
		// 						break;
		// 					case MAP_GEN_ENUM::MC_PLACE_METAL_PLANET:
		// 						createPlanetFromList(xPos,yPos,system,macro->range,system->theme->metalPlanets);
		// 						break;
		// 					case MAP_GEN_ENUM::MC_PLACE_OTHER_PLANET:
		// 						createPlanetFromList(xPos,yPos,system,macro->range,system->theme->otherPlanets);
		// 						break;
		// 					case MAP_GEN_ENUM::MC_PLACE_TERRAIN:
		// 						placeMacroTerrain(xPos,yPos,system,macro->range,&(macro->info.terrainInfo));
		// 						break;
		// 					case MAP_GEN_ENUM::MC_PLACE_PLAYER_BOMB:
		// 					{
		// 						U32 halfWidth = (GRIDSIZE)/2;
		// 						insertObject("MISSION!!PLAYERBOMB",Vector(xPos*GRIDSIZE+halfWidth,yPos*GRIDSIZE+halfWidth,0),system->playerID,system->systemID,system);
		// 					}
		// 						break;
		// 					case MAP_GEN_ENUM::MC_MARK_RING:
		// 					{
		// 						FillPosition(system,xPos-(macro->range-1),yPos-(macro->range-1),(2*macro->range),macro->info.overlap);
		// 					}
		// 						break;
		// 					default:
		// 						CQBOMB2("Unsupported macro:%d in theme:%d",macro->operation,((system->theme)-(map.data->themes))/sizeof(BT_MAP_GEN::_terrainTheme));
		// 				}
		// 			}
		// 		}
		// 	}
		// }
	}

	bool findMacroPosition(MapGenUtils.GenSystem system, S32 centerX, S32 centerY, U32 range, U32 size,
		DMapGen.OVERLAP overlap, out U32 posX, out U32 posY) {
		throw new NotImplementedException();

		posX = 0;
		posY = 0;
		return false;
	}

	void getMacroCenterPos(MapGenUtils.GenSystem system, out U32 x, out U32 y) {
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
