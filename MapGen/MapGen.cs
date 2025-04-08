using System.Diagnostics;
using System.Numerics;
using Newtonsoft.Json;
using U32 = uint;

namespace MapGen;

using U8 = byte;
using S32 = int;
using U64 = UInt64;
using SINGLE = float;

public class MapGen(Globals globals, BT_MAP_GEN mapgen, List<BASE_FIELD_DATA> baseFieldData) : IMapGen {
	private BT_MAP_GEN _mapGen = mapgen;
	private Globals _globals = globals;
	public S32[] mapGenMacroX = [-2, 0, 2, 2, 2, 0, -2, -2, -1, 1, 2, 2, 1, -1, -2, -2];
	public S32[] mapGenMacroY = [-2, -2, -2, 0, 2, 2, 2, 0, -2, -2, -1, 1, 2, 2, 1, -1];
	private List<BASE_FIELD_DATA> _baseFieldData = baseFieldData;

	private static void CQASSERT(bool expression, string message = "") {
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

		S32[] assignments = new S32[CQGAME.MAX_PLAYERS + 1];
		memset(assignments, 0, assignments.Length);
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

	void insertObject(string obj, Vector3 position, S32 playerID, S32 systemID, MapGenUtils.GenSystem system) {
		Console.WriteLine(
			$"Inserting {obj} to position {position.ToString()}, playerID: {playerID}, systemID: {systemID}");
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
		S32 s1;

		for (s1 = 0; s1 < map.numPlayers; s1++) {
			map.systems[s1] = new MapGenUtils.GenSystem();
			MapGenUtils.GenSystem system1 = map.systems[s1];
			do {
				S32 val = (int)GetRand(0, MapGenUtils.RND_MAX_PLAYER_SYSTEMS - 1, DMapGen.DMAP_FUNC.LINEAR);
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

			map.sectorGrid[system1.sectorGridX] |= (uint)(0x00000001 << system1.sectorGridY);

			system1.index = s1;

			map.systemCount++;
		}
	}

	void generateSystemsRing(MapGenUtils.GenStruct map) {
		S32 playerSpace = MapGenUtils.RING_MAX_SYSTEMS / map.numPlayers;
		U32 remotePicks = (U32)(map.systemsToMake / map.numPlayers) - 1;
		S32 s1;
		U32 playersPlaced = 0;
		for (s1 = 0; s1 < map.numPlayers; ++s1) {
			map.systems[s1] = new MapGenUtils.GenSystem();
			MapGenUtils.GenSystem system1 = map.systems[s1];

			S32 val = s1 * playerSpace;
			system1.sectorGridX = MapGenUtils.ringSystemX[val];
			system1.sectorGridY = MapGenUtils.ringSystemY[val];
			system1.connectionOrder = val;

			map.sectorGrid[system1.sectorGridX] |= (U32)(0x00000001 << system1.sectorGridY);

			system1.index = s1;
			do {
				system1.playerID = (int)GetRand(1, (uint)map.numPlayers, DMapGen.DMAP_FUNC.LINEAR);
			} while (((0x01 << system1.playerID) & playersPlaced) != 0);

			playersPlaced |= (uint)(0x01 << system1.playerID);

			map.systemCount++;
		}

		for (S32 i = 0; i < map.numPlayers; ++i) {
			for (S32 j = 0; j < remotePicks; ++j) {
				s1 = map.systemCount;
				map.systems[s1] = new MapGenUtils.GenSystem();
				MapGenUtils.GenSystem system1 = map.systems[s1];

				do {
					U32 maxVal = (uint)((i + 1) * playerSpace) - 1;
					if (maxVal > 15)
						maxVal = 15;
					S32 val = (int)GetRand((uint)(i * playerSpace + 1), maxVal, DMapGen.DMAP_FUNC.LINEAR);
					system1.sectorGridX = MapGenUtils.ringSystemX[val];
					system1.sectorGridY = MapGenUtils.ringSystemY[val];
					system1.connectionOrder = val;
				} while (SystemsOverlap(map, system1));

				map.sectorGrid[system1.sectorGridX] |= (uint)(0x00000001 << system1.sectorGridY);

				system1.index = s1;

				map.systemCount++;
			}
		}
	}

	void generateSystemsStar(MapGenUtils.GenStruct map) {
		//create center system

		map.systems[0] = new MapGenUtils.GenSystem();
		MapGenUtils.GenSystem system1 = map.systems[0];			

		system1.sectorGridX = MapGenUtils.starCenterX;
		system1.sectorGridY = MapGenUtils.starCenterY;
	
		map.sectorGrid[system1.sectorGridX] |= (uint)(0x00000001 <<system1.sectorGridY); 

		system1.index = 0;

		map.systemCount++;

		//create trees
		U32 systemsPerPlayer = (map.systemsToMake-1)/(uint)map.numPlayers;
		U32 treeUsed = 0;
		for(S32 i = 0; i < map.numPlayers; ++i)
		{
			S32 tree;
			do
			{
				tree = (S32)GetRand(0,(uint)MapGenUtils.STAR_MAX_TREE-1,DMapGen.DMAP_FUNC.LINEAR);
			}while(((0x01 << tree)&treeUsed) != 0);
			treeUsed |= (uint)(0x01 << tree);

			map.systems[map.systemCount] = new MapGenUtils.GenSystem();
			//create home system
			system1 = map.systems[map.systemCount];			

			system1.sectorGridX = MapGenUtils.starTreeX[tree,0];
			system1.sectorGridY = MapGenUtils.starTreeY[tree,0];
		
			map.sectorGrid[system1.sectorGridX] |= (uint)(0x00000001 <<system1.sectorGridY); 

			system1.index = map.systemCount;
			system1.playerID = i+1;

			map.systemCount++;

			//create leaf systems
			bool createSys1 = false;
			bool createSys2 = false;
			if(systemsPerPlayer == 2)
			{
				if(GetRand(1,10,DMapGen.DMAP_FUNC.LINEAR) > 5)
					createSys1 = true;
				else
					createSys2 = true;
			}else if(systemsPerPlayer == 3)
			{
				createSys1 = true;
				createSys2 = true;
			}
			if(createSys1) {
				map.systems[map.systemCount] = new MapGenUtils.GenSystem();
				system1 = map.systems[map.systemCount];			
				system1.sectorGridX = MapGenUtils.starTreeX[tree,1];
				system1.sectorGridY = MapGenUtils.starTreeY[tree,1];
			
				map.sectorGrid[system1.sectorGridX] |= (uint)(0x00000001 <<system1.sectorGridY); 

				system1.index = map.systemCount;

				map.systemCount++;
			}
			if(createSys2) {
				map.systems[map.systemCount] = new MapGenUtils.GenSystem();
				system1 = map.systems[map.systemCount];			

				system1.sectorGridX = MapGenUtils.starTreeX[tree, 2];
				system1.sectorGridY = MapGenUtils.starTreeY[tree, 2];
			
				map.sectorGrid[system1.sectorGridX] |= (uint)(0x00000001 <<system1.sectorGridY); 

				system1.index = map.systemCount;

				map.systemCount++;
			}
		}
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
		out S32 jx1, out S32 jy1, out S32 jx2, out S32 jy2) {
		S32 xDif = sys1.sectorGridX - sys2.sectorGridX;
		S32 yDif = sys1.sectorGridY - sys2.sectorGridY;
		SINGLE dist = MathF.Sqrt(xDif * xDif + yDif * yDif);

		S32 cSize = (S32)sys1.size / 2;

		S32 edge = (S32)(((cSize - 1) * xDif) / dist) + cSize;
		S32 t = (S32)GetRand(0, 0x00004FFF, DMapGen.DMAP_FUNC.MORE_IS_LIKLY);
		jx1 = (((cSize - edge) * t) >> MapGenUtils.FIX15) + cSize;

		edge = (S32)(((cSize - 1) * yDif) / dist) + cSize;
		t = (S32)GetRand(0, 0x00004FFF, DMapGen.DMAP_FUNC.MORE_IS_LIKLY);
		jy1 = (((cSize - edge) * t) >> MapGenUtils.FIX15) + cSize;

		while (!SpaceEmpty(sys1, jx1, jy1, DMapGen.OVERLAP.NO_OVERLAP, 3)) {
			if (jx1 == (U32)cSize && jy1 == (U32)cSize) {
				bool findSuccess = FindPosition(sys1, 3, DMapGen.OVERLAP.NO_OVERLAP, ref jx1, ref jy1);
				CQASSERT(findSuccess, "Full System could not place jumpgate");
				break;
			} else {
				if (jx1 < (U32)cSize)
					++jx1;
				else if (jx1 > (U32)cSize)
					--jx1;
			}

			if (!SpaceEmpty(sys1, jx1, jy1, DMapGen.OVERLAP.NO_OVERLAP, 3)) {
				if (jy1 < (U32)cSize)
					++jy1;
				else if (jy1 > (U32)cSize)
					--jy1;
			}
		}

		cSize = (S32)sys2.size / 2;

		edge = (S32)(((cSize - 1) * (-xDif)) / dist) + cSize;
		t = (S32)GetRand(0, 0x00004FFF, DMapGen.DMAP_FUNC.MORE_IS_LIKLY);
		jx2 = (((cSize - edge) * t) >> MapGenUtils.FIX15) + cSize;

		edge = (S32)(((cSize - 1) * (-yDif)) / dist) + cSize;
		t = (S32)GetRand(0, 0x00004FFF, DMapGen.DMAP_FUNC.MORE_IS_LIKLY);
		jy2 = (((cSize - edge) * t) >> MapGenUtils.FIX15) + cSize;

		while (!SpaceEmpty(sys2, jx2, jy2, DMapGen.OVERLAP.NO_OVERLAP, 3)) {
			if (jx2 == (U32)cSize && jy2 == (U32)cSize) {
				bool findSuccess = FindPosition(sys2, 3, DMapGen.OVERLAP.NO_OVERLAP, ref jx2, ref jy2);
				CQASSERT(findSuccess, "Full System could not place jumpgate");
				break;
			} else {
				if (jx2 < (U32)cSize)
					++jx2;
				else if (jx2 > (U32)cSize)
					--jx2;
			}

			if (!SpaceEmpty(sys2, jx2, jy2, DMapGen.OVERLAP.NO_OVERLAP, 3)) {
				if (jy2 < (U32)cSize)
					++jy2;
				else if (jy2 > (U32)cSize)
					--jy2;
			}
		}
	}

	bool CrossesAnotherSystem(MapGenUtils.GenStruct map, MapGenUtils.GenSystem sys1,
		MapGenUtils.GenSystem sys2,
		S32 jx1, S32 jy1, S32 jx2, S32 jy2) {
		U32 s;

		MapGenUtils.GenSystem sys;

		S32 halfSystemSize = MapGenUtils.MAX_MAP_SIZE / 2;

		jx1 = jx1 * 2 * MapGenUtils.MAX_MAP_SIZE + halfSystemSize;
		jy1 = jy1 * 2 * MapGenUtils.MAX_MAP_SIZE + halfSystemSize;
		jx2 = jx2 * 2 * MapGenUtils.MAX_MAP_SIZE + halfSystemSize;
		jy2 = jy2 * 2 * MapGenUtils.MAX_MAP_SIZE + halfSystemSize;
		for (s = 0; s < map.systemCount; s++) {
			if (s != sys1.index && s != sys2.index) {
				sys = (map.systems[s]);

				if (LinesCross(jx1, jy1, jx2, jy2,
					    sys.sectorGridX * 2 * MapGenUtils.MAX_MAP_SIZE, sys.sectorGridY * 2 * MapGenUtils.MAX_MAP_SIZE,
					    sys.sectorGridX * 2 * MapGenUtils.MAX_MAP_SIZE + MapGenUtils.MAX_MAP_SIZE,
					    sys.sectorGridY * 2 * MapGenUtils.MAX_MAP_SIZE))
					return true;
				if (LinesCross(jx1, jy1, jx2, jy2,
					    sys.sectorGridX * 2 * MapGenUtils.MAX_MAP_SIZE,
					    sys.sectorGridY * 2 * MapGenUtils.MAX_MAP_SIZE + MapGenUtils.MAX_MAP_SIZE,
					    sys.sectorGridX * 2 * MapGenUtils.MAX_MAP_SIZE + MapGenUtils.MAX_MAP_SIZE,
					    sys.sectorGridY * 2 * MapGenUtils.MAX_MAP_SIZE + MapGenUtils.MAX_MAP_SIZE))
					return true;
				if (LinesCross(jx1, jy1, jx2, jy2,
					    sys.sectorGridX * 2 * MapGenUtils.MAX_MAP_SIZE, sys.sectorGridY * 2 * MapGenUtils.MAX_MAP_SIZE,
					    sys.sectorGridX * 2 * MapGenUtils.MAX_MAP_SIZE,
					    sys.sectorGridY * 2 * MapGenUtils.MAX_MAP_SIZE + MapGenUtils.MAX_MAP_SIZE))
					return true;
				if (LinesCross(jx1, jy1, jx2, jy2,
					    sys.sectorGridX * 2 * MapGenUtils.MAX_MAP_SIZE + MapGenUtils.MAX_MAP_SIZE,
					    sys.sectorGridY * 2 * MapGenUtils.MAX_MAP_SIZE,
					    sys.sectorGridX * 2 * MapGenUtils.MAX_MAP_SIZE + MapGenUtils.MAX_MAP_SIZE,
					    sys.sectorGridY * 2 * MapGenUtils.MAX_MAP_SIZE + MapGenUtils.MAX_MAP_SIZE))
					return true;
			}
		}

		return false;
	}

	bool CrossesAnotherLink(MapGenUtils.GenStruct map, MapGenUtils.GenJumpgate gate) {
		MapGenUtils.GenJumpgate other;
		U32 j;

		for (j = 0; j < map.numJumpGates; j++) {
			other = (map.jumpgate[j]);
			if (other != gate && other.created &&
			    ((other.system1 != gate.system1) && (other.system1 != gate.system2) &&
			     (other.system2 != gate.system1) && (other.system2 != gate.system2))) {
				if (LinesCross(gate.x1, gate.y1, gate.x2, gate.y2,
					    other.x1, other.y1, other.x2, other.y2)) {
					return true;
				}
			}
		}

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

	static void PrettyPrintByteArray(byte[,] array) {
		int rows = array.GetLength(0);
		int cols = array.GetLength(1);
		Console.WriteLine("[");
		for (int i = 0; i < rows; i++) {
			for (int j = 0; j < cols; j++) {
				Console.Write(array[i, j].ToString("D1") + ",");
			}

			Console.WriteLine();
		}

		Console.WriteLine("]");
	}

	void CreateSystems(MapGenUtils.GenStruct map) {
		for (S32 s1 = 0; s1 < map.systemCount; ++s1) {
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
		S32 xPos = 0;
		S32 yPos = 0;
		for (U32 s1 = 0; s1 < map.systemCount; ++s1) {
			MapGenUtils.GenSystem system = map.systems[s1];
			if (system.playerID > 0) {
				getMacroCenterPos(system, ref xPos, ref yPos);
				for (U32 i = 0; i < _terrainTheme.MAX_MACROS; ++i) {
					_macros macro = system.theme.macros[i];
					if (macro.active) {
						switch (macro.operation) {
							case DMapGen.MACRO_OPERATION.MC_PLACE_HABITABLE_PLANET:
								createPlanetFromList(xPos, yPos, system, macro.range, system.theme.habitablePlanets);
								break;
							// 					case MAP_GEN_ENUM::MC_PLACE_GAS_PLANET:
							// 						createPlanetFromList(xPos,yPos,system,macro->range,system->theme->gasPlanets);
							// 						break;
							// 					case MAP_GEN_ENUM::MC_PLACE_METAL_PLANET:
							// 						createPlanetFromList(xPos,yPos,system,macro->range,system->theme->metalPlanets);
							// 						break;
							// 					case MAP_GEN_ENUM::MC_PLACE_OTHER_PLANET:
							// 						createPlanetFromList(xPos,yPos,system,macro->range,system->theme->otherPlanets);
							// 						break;
							case DMapGen.MACRO_OPERATION.MC_PLACE_TERRAIN:
								Console.WriteLine("Running MC_PLACE_TERRAIN macro");
								placeMacroTerrain(xPos, yPos, system, macro.range, macro.info.terrainInfo);
								break;
							case DMapGen.MACRO_OPERATION.MC_PLACE_PLAYER_BOMB:
								insertObject("MISSION!!PLAYERBOMB - Base Start!", new Vector3(xPos, yPos, 0),
									system.playerID, system.systemID, system);
								break;
							case DMapGen.MACRO_OPERATION.MC_MARK_RING:
								Console.WriteLine("Running MC_MARK_RING macro");
								if (macro.info.overlap is { } overlap) {
									FillPosition(system, xPos - ((int)macro.range - 1), yPos - ((int)macro.range - 1),
										(2 * macro.range), overlap);
								}

								break;
							default:
								Console.WriteLine(
									$"Unsupported macro:{macro.operation.ToString()} in theme: {system.theme}");
								break;
						}
					}
				}
			}
		}
	}

	bool findMacroPosition(MapGenUtils.GenSystem system, S32 centerX, S32 centerY, U32 range, U32 size,
		DMapGen.OVERLAP overlap, out S32 posX, out S32 posY) {
		S32 currentRange = (S32)range;
		U32 numPos = 0;
		while (numPos == 0) {
			for (U32 i = 0; i < 16; ++i) {
				if (SpaceEmpty(system, centerX + ((mapGenMacroX[i] * currentRange) / 2),
					    centerY + ((mapGenMacroY[i] * currentRange) / 2), overlap, size))
					++numPos;
			}

			if (numPos == 0) {
				++currentRange;
				if (currentRange > ((S32)(system.size))) {
					posX = 0;
					posY = 0;
					return false;
				}
			}
		}

		U32 t = GetRand(0, numPos - 1, DMapGen.DMAP_FUNC.LINEAR);
		for (U32 i = 0; i < 16; ++i) {
			if (SpaceEmpty(system, centerX + ((mapGenMacroX[i] * currentRange) / 2),
				    centerY + ((mapGenMacroY[i] * currentRange) / 2), overlap, size)) {
				if (t == 0) {
					posX = (centerX + ((mapGenMacroX[i] * currentRange) / 2));
					posY = (centerY + ((mapGenMacroY[i] * currentRange) / 2));
					CQASSERT(posX + size - 1 < system.size);
					CQASSERT(posY + size - 1 < system.size);
					return true;
				}

				--t;
			}
		}

		posX = 0;
		posY = 0;
		return false;
	}

	void getMacroCenterPos(MapGenUtils.GenSystem system, ref S32 x, ref S32 y) {
		FindPosition(system, 6, DMapGen.OVERLAP.NO_OVERLAP, ref x, ref y);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="xPos"></param>
	/// <param name="yPos"></param>
	/// <param name="system"></param>
	/// <param name="range"></param>
	/// <param name="planetList">planetList[][GT_PATH]</param>
	void createPlanetFromList(S32 xPos, S32 yPos, MapGenUtils.GenSystem system, U32 range, string[] planetList) {
		S32 posX, posY;
		bool bSuccess = findMacroPosition(system, xPos, yPos, range, 4, DMapGen.OVERLAP.NO_OVERLAP, out posX, out posY);
		if (bSuccess) {
			FillPosition(system, posX, posY, 4, DMapGen.OVERLAP.NO_OVERLAP);
			if (system.numFlags < MapGenUtils.MAX_FLAGS) {
				system.flags[system.numFlags].xPos = (byte)(posX + 2);
				system.flags[system.numFlags].yPos = (byte)(posY + 2);
				system.flags[system.numFlags].type = (byte)(MapGenUtils.FLAG_PLANET | MapGenUtils.FLAG_PATHON);
				system.numFlags++;
			}

			U32 maxPlanetIndex = 0;
			for (U32 i = 0; i < _terrainTheme.MAX_TYPES; ++i) {
				if (planetList[i] != "")
					++maxPlanetIndex;
				else
					break;
			}

			U32 planetID = GetRand(0, maxPlanetIndex - 1, DMapGen.DMAP_FUNC.LINEAR);
			insertObject(planetList[planetID], new Vector3((posX + 2), (posY + 2), 0), 0, system.systemID, system);
		}
	}

	void placeMacroTerrain(S32 centerX, S32 centerY, MapGenUtils.GenSystem system, U32 range,
		_terrainInfo? terrainInfo) {
		if (terrainInfo is null) {
			return;
		}

		Console.WriteLine($"Gettitng basic bitch data for: {terrainInfo.terrainArchType}");
		var data = _baseFieldData.Find(x => x.terrainArchType == terrainInfo.terrainArchType);

		if (data is null) {
			throw new Exception(
				$"Bad archtype name in random map generator.  Fix the data not a code bug, Name:{terrainInfo.terrainArchType}");
		}

		if ((data.objClass == ObjClass.OC_NEBULA) || (data.objClass == ObjClass.OC_FIELD)) {
			U32 numToPlace = GetRand(terrainInfo.minToPlace, terrainInfo.maxToPlace, terrainInfo.numberFunc);
			S32 startX, startY;
			if (findMacroPosition(system, centerX, centerY, range, 1, terrainInfo.overlap, out startX, out startY)) {
				BASE_FIELD_DATA fData = data;
				if (fData.fieldClass == FIELDCLASS.FC_ANTIMATTER) {
					switch (terrainInfo.placement) {
						case DMapGen.PLACEMENT.RANDOM:
							PlaceRandomRibbon(terrainInfo, numToPlace, startX, startY, system);
							break;
						case DMapGen.PLACEMENT.SPOTS:
							CQASSERT(false, "NOT SUPPORTED");
							break;
						case DMapGen.PLACEMENT.CLUSTER:
							CQASSERT(false, "NOT SUPPORTED");
							break;
						case DMapGen.PLACEMENT.PLANET_RING:
							CQASSERT(false, "NOT SUPPORTED");
							break;
						case DMapGen.PLACEMENT.STREEKS:
							CQASSERT(false, "NOT SUPPORTED");
							break;
					}
				} else //a regular nebula
				{
					switch (terrainInfo.placement) {
						case DMapGen.PLACEMENT.RANDOM:
							PlaceRandomField(terrainInfo, numToPlace, startX, startY, system);
							break;
						case DMapGen.PLACEMENT.SPOTS:
							PlaceSpottyField(terrainInfo, numToPlace, startX, startY, system);
							break;
						case DMapGen.PLACEMENT.CLUSTER:
							CQASSERT(false, "NOT SUPPORTED");
							break;
						case DMapGen.PLACEMENT.PLANET_RING:
							PlaceRingField(terrainInfo, system);
							break;
						case DMapGen.PLACEMENT.STREEKS:
							CQASSERT(false, "NOT SUPPORTED");
							break;
					}
				}
			}
		} else // it is a regular object
		{
			switch (terrainInfo.placement) {
				case DMapGen.PLACEMENT.RANDOM: {
					U32 numToPlace = GetRand(terrainInfo.minToPlace, terrainInfo.maxToPlace, terrainInfo.numberFunc);
					for (U32 i = 0; i < numToPlace; ++i) {
						S32 xPos, yPos;
						bool bSuccess = findMacroPosition(system, centerX, centerY, range, terrainInfo.size,
							terrainInfo.overlap, out xPos, out yPos);
						if (bSuccess) {
							FillPosition(system, xPos, yPos, terrainInfo.size, terrainInfo.overlap);
							U32 halfWidth = (terrainInfo.size) / 2;
							S32 playerID = 0;
							if ((data.objClass & ObjClass.CF_PLAYERALIGNED) > 0)
								playerID = system.playerID;
							insertObject(terrainInfo.terrainArchType,
								new Vector3(xPos + halfWidth, yPos * halfWidth, 0), playerID,
								system.systemID, system);
						} else {
							system.omUsed += terrainInfo.size * terrainInfo.size;
						}
					}
				}
					break;
				case DMapGen.PLACEMENT.SPOTS:
					CQASSERT(false, "NOT SUPPORTED");
					break;
				case DMapGen.PLACEMENT.CLUSTER:
					CQASSERT(false, "NOT SUPPORTED");
					break;
				case DMapGen.PLACEMENT.PLANET_RING:
					CQASSERT(false, "NOT SUPPORTED");
					break;
				case DMapGen.PLACEMENT.STREEKS:
					CQASSERT(false, "NOT SUPPORTED");
					break;
			}
		}
	}

	void CreateJumpgates(MapGenUtils.GenStruct map) {
		for (U32 i = 0; i < map.systemCount; ++i) {
			MapGenUtils.GenSystem system1 = (map.systems[i]);
			S32 cx1 = system1.sectorGridX;
			S32 cy1 = system1.sectorGridY;
			for (U32 j = i + 1; j < map.systemCount; ++j) {
				MapGenUtils.GenSystem system2 = (map.systems[j]);
				S32 cx2 = system2.sectorGridX;
				S32 cy2 = system2.sectorGridY;
				if (!CrossesAnotherSystem(map, system1, system2, cx1, cy1, cx2, cy2)) {
					if (map.jumpgate[map.numJumpGates] is null) {
						map.jumpgate[map.numJumpGates] = new MapGenUtils.GenJumpgate();
					}

					MapGenUtils.GenJumpgate jumpgate = (map.jumpgate[map.numJumpGates]);
					++map.numJumpGates;

					jumpgate.system1 = system1;
					jumpgate.system2 = system2;
					jumpgate.dist = (cx1 - cx2) * (cx1 - cx2) + (cy1 - cy2) * (cy1 - cy2);
					jumpgate.x1 = cx1;
					jumpgate.y1 = cy1;
					jumpgate.x2 = cx2;
					jumpgate.y2 = cy2;
					jumpgate.created = false;
				}
			}
		}

		if (map.sectorLayout == DMapGen.SECTOR_FORMATION.SF_RANDOM) {
			createRandomGates2(map);
		} else if (map.sectorLayout == DMapGen.SECTOR_FORMATION.SF_MULTI_RANDOM) {
			createRandomGates3(map);
		} else if (map.sectorLayout == DMapGen.SECTOR_FORMATION.SF_RING) {
			createRingGates(map);
		} else if (map.sectorLayout == DMapGen.SECTOR_FORMATION.SF_STAR) {
			createStarGates(map);
		}

		for (U32 j1 = 0; j1 < map.numJumpGates; ++j1) {
			MapGenUtils.GenJumpgate gate = (map.jumpgate[j1]);
			if (gate.created) {
				S32 posX1, posY1, posX2, posY2;
				GetJumpgatePositions(map, gate.system1, gate.system2, out posX1, out posY1, out posX2, out posY2);
				if (gate.system1.numFlags < MapGenUtils.MAX_FLAGS) {
					gate.system1.flags[gate.system1.numFlags].xPos = posX1;
					gate.system1.flags[gate.system1.numFlags].yPos = posY1;
					gate.system1.flags[gate.system1.numFlags].type = MapGenUtils.FLAG_PATHON;
					gate.system1.numFlags++;
				}

				if (gate.system2.numFlags < MapGenUtils.MAX_FLAGS) {
					gate.system2.flags[gate.system2.numFlags].xPos = posX2;
					gate.system2.flags[gate.system2.numFlags].yPos = posY2;
					gate.system2.flags[gate.system2.numFlags].type = MapGenUtils.FLAG_PATHON;
					gate.system2.numFlags++;
				}

				FillPosition(gate.system1, posX1, posY1, 3, DMapGen.OVERLAP.NO_OVERLAP);
				FillPosition(gate.system2, posX2, posY2, 3, DMapGen.OVERLAP.NO_OVERLAP);
				U32 id1, id2;

				Console.WriteLine("CreateJumpgate from {0} to {1}", gate.system1.systemID, gate.system2.systemID);
				Console.WriteLine("Coordinates: {0} {1}, {2} {3}", posX1, posY1, posX2, posY2);
				// SECTOR->CreateJumpGate(gate->system1->systemID, posX1 + 1.5,
				// 	posY1 + 1.5, id1,
				// 	gate->system2->systemID, posX2 * GRIDSIZE + GRIDSIZE * 1.5, posY2 * GRIDSIZE + GRIDSIZE * 1.5, id2,
				// 	"JGATE!!Jumpgate");
			}
		}
	}

	void createRandomGates3(MapGenUtils.GenStruct map) {
		throw new NotImplementedException();
	}


	void createGateLevel2(MapGenUtils.GenStruct map, U32 totalLevel, U32 levelSystems, U32 targetSystems,
		U32 gateNum, U32[] currentGates, U32 score,
		U32[] bestGates, ref U32 bestScore, ref U32 bestGateNum, bool moreAllowed) {
		throw new NotImplementedException();
		// if (levelSystems == 0) {
		// 	if (bestScore != 0) {
		// 		if (bestScore > score) {
		// 			bestScore = score;
		// 			bestGateNum = gateNum;
		// 			Array.Copy(currentGates, bestGates, gateNum);
		// 		}
		// 	} else {
		// 		bestScore = score;
		// 		bestGateNum = gateNum;
		// 		Array.Copy(currentGates, bestGates, gateNum);
		// 	}
		// } else {
		// 	S32 currentSystem = 0;
		// 	S32 i;
		// 	for (i = 0; i < map.systemCount; ++i) {
		// 		if ((levelSystems & (0x01 << i)) != 0) {
		// 			currentSystem = i;
		// 			break;
		// 		}
		// 	}
		//
		// 	U32 newLevel = levelSystems & (~(0x01 << currentSystem));
		// 	bool gateMade = !moreAllowed; // only do back up if more are allowed as well
		// 	for (i = 0; i < map.numJumpGates; ++i) {
		// 		if (!(map.jumpgate[i].created)) {
		// 			if ((map.jumpgate[i].system1.index == currentSystem &&
		// 			     (targetSystems & (0x01 << (map.jumpgate[i].system2.index))) != 0) ||
		// 			    (map.jumpgate[i].system2.index == currentSystem &&
		// 			     (targetSystems & (0x01 << (map.jumpgate[i].system1.index))) != 0)) {
		// 				if ((!CrossesAnotherLink(map, ref map.jumpgate[i]))) {
		// 					gateMade = true;
		// 					U32 gateScore = scoreGate(map, i);
		// 					map.jumpgate[i].system1.jumpgates[map.jumpgate[i].system1.jumpgateCount++] =
		// 						ref map.jumpgate[i];
		// 					map.jumpgate[i].system2.jumpgates[map.jumpgate[i].system2.jumpgateCount++] =
		// 						ref map.jumpgate[i];
		// 					map.jumpgate[i].created = true;
		// 					currentGates[gateNum] = i;
		// 					if (moreAllowed) {
		// 						createGateLevel(map, totalLevel, levelSystems, targetSystems, gateNum + 1, currentGates,
		// 							score + gateScore, bestGates, ref bestScore, ref bestGateNum, false);
		// 					} else
		// 						createGateLevel(map, totalLevel, newLevel, targetSystems, gateNum + 1, currentGates,
		// 							score + gateScore, bestGates, ref bestScore, ref bestGateNum, true);
		//
		// 					map.jumpgate[i].system1.jumpgates[--map.jumpgate[i].system1.jumpgateCount] = null;
		// 					map.jumpgate[i].system2.jumpgates[--map.jumpgate[i].system2.jumpgateCount] = null;
		// 					map.jumpgate[i].created = false;
		// 				}
		// 			}
		// 		}
		// 	}
		//
		// 	if (!gateMade) {
		// 		for (i = 0; i < map.numJumpGates; ++i) {
		// 			if (!(map.jumpgate[i].created)) {
		// 				if ((map.jumpgate[i].system1.index == currentSystem &&
		// 				     (totalLevel & (0x01 << (map.jumpgate[i].system2.index))) != 0) ||
		// 				    (map.jumpgate[i].system2.index == currentSystem &&
		// 				     (totalLevel & (0x01 << (map.jumpgate[i].system1.index))) != 0)) {
		// 					if ((!CrossesAnotherLink(map, ref map.jumpgate[i]))) {
		// 						U32 gateScore = scoreGate(map, i);
		// 						map.jumpgate[i].system1.jumpgates[map.jumpgate[i].system1.jumpgateCount++] =
		// 							ref map.jumpgate[i];
		// 						map.jumpgate[i].system2.jumpgates[map.jumpgate[i].system2.jumpgateCount++] =
		// 							ref map.jumpgate[i];
		// 						map.jumpgate[i].created = true;
		// 						currentGates[gateNum] = i;
		// 						createGateLevel(map, totalLevel, newLevel, targetSystems, gateNum + 1, currentGates,
		// 							score + gateScore, bestGates, ref bestScore, ref bestGateNum, true);
		// 						map.jumpgate[i].system1.jumpgates[--map.jumpgate[i].system1.jumpgateCount] = null;
		// 						map.jumpgate[i].system2.jumpgates[--map.jumpgate[i].system2.jumpgateCount] = null;
		// 						map.jumpgate[i].created = false;
		// 					}
		// 				}
		// 			}
		// 		}
		// 	}
		// }
	}

	void createRandomGates2(MapGenUtils.GenStruct map) {
		if (map.systemCount == map.numPlayers) {
			createRingGates(map);
		} else {
			S32 levelSystems = 0;

			S32 targetSystems = 0;

			S32 i;
			for (i = 0; i < map.systemCount; ++i) {
				if (map.systems[i].playerID != 0) {
					levelSystems |= (0x01 << i);
				} else {
					targetSystems |= (0x01 << i);
				}
			}

			//create web of systems;
			bool freeSystems = false;
			while (targetSystems > 0) {
				U32[] currentGates = new U32[64];
				U32[] bestGates = new U32[64];
				S32 bestScore = 0;
				S32 bestGateNum = 0;
				createGateLevel(map, levelSystems, levelSystems, targetSystems, 0, currentGates, 0, bestGates,
					ref bestScore, ref bestGateNum, false);
				if (bestGateNum == 0) {
					freeSystems = true;
					break;
				} else {
					S32 newLevel = 0;
					for (i = 0; i < bestGateNum; ++i) {
						if (((0x01 << (map.jumpgate[bestGates[i]].system1.index)) & levelSystems) != 0)
							newLevel |= (0x01 << map.jumpgate[bestGates[i]].system2.index);
						else
							newLevel |= (0x01 << map.jumpgate[bestGates[i]].system1.index);
						map.jumpgate[bestGates[i]].system1
							.jumpgates[map.jumpgate[bestGates[i]].system1.jumpgateCount++] = map.jumpgate[bestGates[i]];
						map.jumpgate[bestGates[i]].system2
							.jumpgates[map.jumpgate[bestGates[i]].system2.jumpgateCount++] = map.jumpgate[bestGates[i]];
						map.jumpgate[bestGates[i]].created = true;
					}

					targetSystems &= (~newLevel);
					levelSystems = newLevel;
				}
			}

			//attempt to connect all of the systems on the last level to one another
			if (levelSystems != 0) {
				for (i = 0; i < map.numJumpGates; ++i) {
					if (!(map.jumpgate[i].created)) {
						if (((0x01 << map.jumpgate[i].system1.index) & levelSystems) != 0 &&
						    ((0x01 << map.jumpgate[i].system2.index) & levelSystems) != 0) {
							if (!CrossesAnotherLink(map, (map.jumpgate[i]))) {
								map.jumpgate[i].system1.jumpgates[map.jumpgate[i].system1.jumpgateCount++] =
									(map.jumpgate[i]);
								map.jumpgate[i].system2.jumpgates[map.jumpgate[i].system2.jumpgateCount++] =
									(map.jumpgate[i]);
								map.jumpgate[i].created = true;
							}
						}
					}
				}
			}

			//make sure all players are connected
			U32 systemUnconnected = ~(0xFFFFFFFF << map.systemCount);
			U32 systemsVisited = 0;
			MarkSystems(ref systemUnconnected, map.systems[0],
				ref systemsVisited); // Note: Passing first element of systems array

			while (systemUnconnected != 0) {
				for (i = 0; i < map.systemCount; ++i) {
					uint currentSystemBit = 1U << i;
					bool isSystemUnconnected = (currentSystemBit & systemUnconnected) != 0;

					if (isSystemUnconnected) {
						bool breakOk = false;

						for (uint j = 0; j < map.numJumpGates; ++j) {
							if (!map.jumpgate[j].created) {
								// Break down the complex condition into separate variables for readability
								bool isSystem1Disconnected = (map.jumpgate[j].system1.index == i);
								bool isSystem2Connected =
									((1U << map.jumpgate[j].system2.index) & systemUnconnected) == 0;
								bool isSystem2NotOwnedByPlayer = (map.jumpgate[j].system2.playerID == 0);

								bool isSystem2Disconnected = (map.jumpgate[j].system2.index == i);
								bool isSystem1Connected =
									((1U << map.jumpgate[j].system1.index) & systemUnconnected) == 0;
								bool isSystem1NotOwnedByPlayer = (map.jumpgate[j].system1.playerID == 0);

								bool canConnectFromSystem1ToSystem2 = isSystem1Disconnected && isSystem2Connected &&
								                                      isSystem2NotOwnedByPlayer;
								bool canConnectFromSystem2ToSystem1 = isSystem2Disconnected && isSystem1Connected &&
								                                      isSystem1NotOwnedByPlayer;

								if (!canConnectFromSystem1ToSystem2 && !canConnectFromSystem2ToSystem1) {
									continue;
								}

								if (CrossesAnotherLink(map, map.jumpgate[j])) {
									continue;
								}

								if ((breakOk) && GetRand(1, 10, DMapGen.DMAP_FUNC.LINEAR) <= 5) {
									continue;
								}

								int system1JumpgateIndex = map.jumpgate[j].system1.jumpgateCount;
								map.jumpgate[j].system1.jumpgates[system1JumpgateIndex] =
									map.jumpgate[j];
								map.jumpgate[j].system1.jumpgateCount++;

								int system2JumpgateIndex = map.jumpgate[j].system2.jumpgateCount;
								map.jumpgate[j].system2.jumpgates[system2JumpgateIndex] =
									map.jumpgate[j];
								map.jumpgate[j].system2.jumpgateCount++;

								map.jumpgate[j].created = true;
								breakOk = true;
							}
						}

						if (breakOk)
							break;
					}
				}

				systemUnconnected = (1U << map.systemCount) - 1; // Equivalent to ~(0xFFFFFFFF << map.systemCount)
				systemsVisited = 0;
				MarkSystems(ref systemUnconnected, map.systems[0], ref systemsVisited);
			}
		}
	}

	void createGateLevel(MapGenUtils.GenStruct map, S32 totalLevel, S32 levelSystems, S32 targetSystems,
		S32 gateNum, U32[] currentGates, S32 score,
		U32[] bestGates, ref S32 bestScore, ref S32 bestGateNum, bool moreAllowed) {
		if (levelSystems == 0) {
			if (bestScore != 0) {
				if (bestScore > score) {
					bestScore = score;
					bestGateNum = gateNum;
					Array.Copy(currentGates, bestGates, gateNum);
				}
			} else {
				bestScore = score;
				bestGateNum = gateNum;
				Array.Copy(currentGates, bestGates, gateNum);
			}
		} else {
			S32 currentSystem = 0;
			S32 i;
			for (i = 0; i < map.systemCount; ++i) {
				if ((levelSystems & (0x01 << i)) != 0) {
					currentSystem = i;
					break;
				}
			}

			S32 newLevel = levelSystems & (~(0x01 << currentSystem));
			bool gateMade = !moreAllowed; //only do back up if more are alowed as well.
			for (i = 0; i < map.numJumpGates; ++i) {
				if (!(map.jumpgate[i].created)) {
					if ((map.jumpgate[i].system1.index == currentSystem &&
					     (targetSystems & (0x01 << map.jumpgate[i].system2.index)) != 0) ||
					    (map.jumpgate[i].system2.index == currentSystem &&
					     (targetSystems & (0x01 << map.jumpgate[i].system1.index)) != 0)) {
						if ((!CrossesAnotherLink(map, (map.jumpgate[i])))) {
							gateMade = true;
							S32 gateScore = scoreGate(map, i);
							map.jumpgate[i].system1.jumpgates[map.jumpgate[i].system1.jumpgateCount++] =
								(map.jumpgate[i]);
							map.jumpgate[i].system2.jumpgates[map.jumpgate[i].system2.jumpgateCount++] =
								(map.jumpgate[i]);
							map.jumpgate[i].created = true;
							currentGates[gateNum] = (uint)i;
							if (map.systems[currentSystem].playerID != 0) {
								createGateLevel(map, totalLevel, newLevel, targetSystems, gateNum + 1, currentGates,
									score + gateScore, bestGates, ref bestScore, ref bestGateNum, true);
							} else if (moreAllowed) {
								createGateLevel(map, totalLevel, levelSystems, targetSystems, gateNum + 1, currentGates,
									score + gateScore, bestGates, ref bestScore, ref bestGateNum, false);
							} else
								createGateLevel(map, totalLevel, newLevel, targetSystems, gateNum + 1, currentGates,
									score + gateScore, bestGates, ref bestScore, ref bestGateNum, true);

							map.jumpgate[i].system1.jumpgates[map.jumpgate[i].system1.jumpgateCount--] = null;
							map.jumpgate[i].system2.jumpgates[map.jumpgate[i].system2.jumpgateCount--] = null;
							map.jumpgate[i].created = false;
						}
					}
				}
			}

			if (!gateMade) {
				for (i = 0; i < map.numJumpGates; ++i) {
					if (!(map.jumpgate[i].created)) {
						if ((map.jumpgate[i].system1.index == currentSystem &&
						     (totalLevel & (0x01 << map.jumpgate[i].system2.index)) != 0) ||
						    (map.jumpgate[i].system2.index == currentSystem &&
						     (totalLevel & (0x01 << map.jumpgate[i].system1.index)) != 0)) {
							if ((!CrossesAnotherLink(map, (map.jumpgate[i])))) {
								S32 gateScore = scoreGate(map, i);
								map.jumpgate[i].system1.jumpgates[map.jumpgate[i].system1.jumpgateCount++] =
									(map.jumpgate[i]);
								map.jumpgate[i].system2.jumpgates[map.jumpgate[i].system2.jumpgateCount++] =
									(map.jumpgate[i]);
								map.jumpgate[i].created = true;
								currentGates[gateNum] = (uint)i;
								createGateLevel(map, totalLevel, newLevel, targetSystems, gateNum + 1, currentGates,
									score + gateScore, bestGates, ref bestScore, ref bestGateNum, true);
								map.jumpgate[i].system1.jumpgates[map.jumpgate[i].system1.jumpgateCount--] = null;
								map.jumpgate[i].system2.jumpgates[map.jumpgate[i].system2.jumpgateCount--] = null;
								map.jumpgate[i].created = false;
							}
						}
					}
				}
			}
		}
	}

	S32 scoreGate(MapGenUtils.GenStruct map, S32 gateIndex) {
		MapGenUtils.GenJumpgate gate = (map.jumpgate[gateIndex]);
		S32 score = gate.dist;
		score += gate.system1.jumpgateCount * 100;
		score += gate.system2.jumpgateCount * 100;
		return score;
	}

	void MarkSystems(ref uint systemUnconnected, MapGenUtils.GenSystem system, ref uint systemsVisited) {
		// Create a bitmask for the current system
		uint systemBitMask = 1U << system.index;

		// Mark system as connected (remove from unconnected systems)
		systemUnconnected &= ~systemBitMask;

		// Mark system as visited
		systemsVisited |= systemBitMask;

		// Recursively visit all connected systems through jumpgates
		for (uint i = 0; i < system.jumpgateCount; ++i) {
			// Check if system1 has not been visited yet
			uint system1BitMask = 1U << system.jumpgates[i].system1.index;
			bool isSystem1Unvisited = (system1BitMask & systemsVisited) == 0;

			if (isSystem1Unvisited) {
				MarkSystems(ref systemUnconnected, system.jumpgates[i].system1, ref systemsVisited);
			}

			// Check if system2 has not been visited yet
			uint system2BitMask = 1U << system.jumpgates[i].system2.index;
			bool isSystem2Unvisited = (system2BitMask & systemsVisited) == 0;

			if (isSystem2Unvisited) {
				MarkSystems(ref systemUnconnected, system.jumpgates[i].system2, ref systemsVisited);
			}
		}
	}

	void markSystems(ref uint systemUnconnected, MapGenUtils.GenSystem system, ref uint systemsVisited) {
		systemUnconnected &= ~(0x01U << system.index);
		systemsVisited |= (0x01U << system.index);

		for (uint i = 0; i < system.jumpgateCount; ++i) {
			if (((0x01U << system.jumpgates[i].system1.index) & systemsVisited) == 0) {
				markSystems(ref systemUnconnected, system.jumpgates[i].system1, ref systemsVisited);
			}

			if (((0x01U << system.jumpgates[i].system2.index) & systemsVisited) == 0) {
				markSystems(ref systemUnconnected, system.jumpgates[i].system2, ref systemsVisited);
			}
		}
	}

	void createRandomGates(MapGenUtils.GenStruct map) {
		throw new NotImplementedException();
	}

	void createRingGates(MapGenUtils.GenStruct map) {
		for (U32 i = 0; i < map.systemCount; ++i) {
			S32 connectVal = map.systems[i].connectionOrder;
			S32 bestConnectVal = 0;
			MapGenUtils.GenJumpgate bestJumpgate = null;
			for (U32 j = 0; j < map.numJumpGates; ++j) {
				MapGenUtils.GenJumpgate jumpgate = (map.jumpgate[j]);
				if (!jumpgate.created) {
					if (jumpgate.system1.index == i) {
						if (bestJumpgate is not null) {
							if (connectVal < jumpgate.system2.connectionOrder) {
								if (bestConnectVal > connectVal) {
									if (bestConnectVal > jumpgate.system2.connectionOrder) {
										bestJumpgate = jumpgate;
										bestConnectVal = jumpgate.system2.connectionOrder;
									}
								} else {
									bestJumpgate = jumpgate;
									bestConnectVal = jumpgate.system2.connectionOrder;
								}
							} else if (bestConnectVal < connectVal) {
								if (jumpgate.system2.connectionOrder < bestConnectVal) {
									bestJumpgate = jumpgate;
									bestConnectVal = jumpgate.system2.connectionOrder;
								}
							}
						} else {
							bestJumpgate = jumpgate;
							bestConnectVal = jumpgate.system2.connectionOrder;
						}
					} else if (jumpgate.system2.index == i) {
						if (bestJumpgate is not null) {
							if (connectVal < jumpgate.system1.connectionOrder) {
								if (bestConnectVal > connectVal) {
									if (bestConnectVal > jumpgate.system1.connectionOrder) {
										bestJumpgate = jumpgate;
										bestConnectVal = jumpgate.system1.connectionOrder;
									}
								} else {
									bestJumpgate = jumpgate;
									bestConnectVal = jumpgate.system1.connectionOrder;
								}
							} else if (bestConnectVal < connectVal) {
								if (jumpgate.system1.connectionOrder < bestConnectVal) {
									bestJumpgate = jumpgate;
									bestConnectVal = jumpgate.system1.connectionOrder;
								}
							}
						} else {
							bestJumpgate = jumpgate;
							bestConnectVal = jumpgate.system1.connectionOrder;
						}
					}
				}
			}

			if (bestJumpgate is not null) {
				bestJumpgate.system1.jumpgates[bestJumpgate.system1.jumpgateCount++] = bestJumpgate;
				bestJumpgate.system2.jumpgates[bestJumpgate.system2.jumpgateCount++] = bestJumpgate;
				bestJumpgate.created = true;
			}
		}
	}

	void createStarGates(MapGenUtils.GenStruct map) {
		for(U32 i = 0; i < map.numPlayers; ++i)
		{
			U32 systemsPerPlayer = (map.systemsToMake-1)/(U32)map.numPlayers;
			createJumpgatesForIndexs(map,0,1+(i*systemsPerPlayer));
			if(systemsPerPlayer > 1)
			{
				createJumpgatesForIndexs(map,1+(i*systemsPerPlayer),2+(i*systemsPerPlayer));
			}
			if(systemsPerPlayer > 2)
			{
				createJumpgatesForIndexs(map,1+(i*systemsPerPlayer),3+(i*systemsPerPlayer));
			}
		}
	}

	void createJumpgatesForIndexs(MapGenUtils.GenStruct map, U32 index1, U32 index2) {
		for(U32 i = 0; i < map.numJumpGates; ++i)
		{
			MapGenUtils.GenJumpgate jumpgate = (map.jumpgate[i]);
			if((index1 == jumpgate.system1.index && index2 == jumpgate.system2.index) ||
			   (index2 == jumpgate.system1.index && index2 == jumpgate.system1.index))
			{
				jumpgate.system1.jumpgates[jumpgate.system1.jumpgateCount++] = jumpgate;
				jumpgate.system2.jumpgates[jumpgate.system2.jumpgateCount++] = jumpgate;
				jumpgate.created = true;
				return;
			}
		}
	}

	void PopulateSystems(MapGenUtils.GenStruct map) {
		U32 s1;
		MapGenUtils.GenSystem system;

		// have to cut up 75% of map generation between the number of systems
		CQASSERT(map.systemCount > 0);
		SINGLE rate = 0.50f / map.systemCount;
		SINGLE progress = 0.25f;

		for (s1 = 0; s1 < map.systemCount; s1++) {
			system = (map.systems[s1]);
			PopulateSystem(map, system);
		}
	}

	void PopulateSystem(MapGenUtils.GenStruct map, MapGenUtils.GenSystem system) {
		//habitable Planets
		U32 maxPlanetIndex = 0;
		U32 i;
		for (i = 0; i < _terrainTheme.MAX_TYPES; ++i) {
			if (system.theme.habitablePlanets[i] != "")
				++maxPlanetIndex;
			else
				break;
		}

		U32 numPlanets = system.theme.numHabitablePlanets[map.terrainSize];
		for (i = 0; i < numPlanets; ++i) {
			var posX = 0;
			var posY = 0;
			bool bSuccess = FindPosition(system, 4, DMapGen.OVERLAP.NO_OVERLAP, ref posX, ref posY);
			if (bSuccess) {
				if (system.numFlags < MapGenUtils.MAX_FLAGS) {
					system.flags[system.numFlags].xPos = posX + 2;
					system.flags[system.numFlags].yPos = posY + 2;
					system.flags[system.numFlags].type = MapGenUtils.FLAG_PLANET | MapGenUtils.FLAG_PATHON;
					system.numFlags++;
				}

				U32 planetID = GetRand(0, maxPlanetIndex - 1, DMapGen.DMAP_FUNC.LINEAR);
				insertObject(system.theme.habitablePlanets[planetID],
					new Vector3((posX + 2), (posY + 2), 0), 0, system.systemID, system);
				placePlanetsMoons(map, system, posX + 2, posY + 2);
				FillPosition(system, posX, posY, 4, DMapGen.OVERLAP.NO_OVERLAP);
			}
		}

		//Ore Planets
		maxPlanetIndex = 0;
		for (i = 0; i < _terrainTheme.MAX_TYPES; ++i) {
			if (system.theme.metalPlanets[i] != "")
				++maxPlanetIndex;
			else
				break;
		}

		numPlanets = system.theme.numMetalPlanets[map.terrainSize];
		for (i = 0; i < numPlanets; ++i) {
			S32 posX = 0;
			S32 posY = 0;
			bool bSuccess = FindPosition(system, 4, DMapGen.OVERLAP.NO_OVERLAP, ref posX, ref posY);
			if (bSuccess) {
				if (system.numFlags < MapGenUtils.MAX_FLAGS) {
					system.flags[system.numFlags].xPos = posX + 2;
					system.flags[system.numFlags].yPos = posY + 2;
					system.flags[system.numFlags].type = MapGenUtils.FLAG_PLANET | MapGenUtils.FLAG_PATHON;
					system.numFlags++;
				}

				U32 planetID = GetRand(0, maxPlanetIndex - 1, DMapGen.DMAP_FUNC.LINEAR);
				insertObject(system.theme.metalPlanets[planetID],
					new Vector3((posX + 2), (posY + 2), 0), 0, system.systemID, system);
				placePlanetsMoons(map, system, posX + 2, posY + 2);
				FillPosition(system, posX, posY, 4, DMapGen.OVERLAP.NO_OVERLAP);
			}
		}

		//gas Planets
		maxPlanetIndex = 0;
		for (i = 0; i < _terrainTheme.MAX_TYPES; ++i) {
			if (system.theme.gasPlanets[i] != "")
				++maxPlanetIndex;
			else
				break;
		}

		numPlanets = system.theme.numGasPlanets[map.terrainSize];
		for (i = 0; i < numPlanets; ++i) {
			S32 posX = 0;
			S32 posY = 0;
			bool bSuccess = FindPosition(system, 4, DMapGen.OVERLAP.NO_OVERLAP, ref posX, ref posY);
			if (bSuccess) {
				if (system.numFlags < MapGenUtils.MAX_FLAGS) {
					system.flags[system.numFlags].xPos = posX + 2;
					system.flags[system.numFlags].yPos = posY + 2;
					system.flags[system.numFlags].type = MapGenUtils.FLAG_PLANET | MapGenUtils.FLAG_PATHON;
					system.numFlags++;
				}

				U32 planetID = GetRand(0, maxPlanetIndex - 1, DMapGen.DMAP_FUNC.LINEAR);
				insertObject(system.theme.gasPlanets[planetID],
					new Vector3((posX + 2), (posY + 2), 0), 0, system.systemID, system);
				placePlanetsMoons(map, system, posX + 2, posY + 2);
				FillPosition(system, posX, posY, 4, DMapGen.OVERLAP.NO_OVERLAP);
			}
		}

		//crew Planets
		maxPlanetIndex = 0;
		for (i = 0; i < _terrainTheme.MAX_TYPES; ++i) {
			if (system.theme.otherPlanets[i] != "")
				++maxPlanetIndex;
			else
				break;
		}

		numPlanets = system.theme.numOtherPlanets[map.terrainSize];
		for (i = 0; i < numPlanets; ++i) {
			var posX = 0;
			var posY = 0;
			bool bSuccess = FindPosition(system, 4, DMapGen.OVERLAP.NO_OVERLAP, ref posX, ref posY);
			if (bSuccess) {
				if (system.numFlags < MapGenUtils.MAX_FLAGS) {
					system.flags[system.numFlags].xPos = posX + 2;
					system.flags[system.numFlags].yPos = posY + 2;
					system.flags[system.numFlags].type = MapGenUtils.FLAG_PLANET | MapGenUtils.FLAG_PATHON;
					system.numFlags++;
				}

				U32 planetID = GetRand(0, maxPlanetIndex - 1, DMapGen.DMAP_FUNC.LINEAR);
				insertObject(system.theme.otherPlanets[planetID],
					new Vector3((posX + 2), (posY + 2), 0), 0, system.systemID, system);
				placePlanetsMoons(map, system, posX + 2, posY + 2);
				FillPosition(system, posX, posY, 4, DMapGen.OVERLAP.NO_OVERLAP);
			}
		}

		BuildPaths(system);
		GenerateTerain(map, system);
	}

	public const U32 NUM_MOON_POINTS = 16;

	public class Point(int x, int y) {
		public int X = x;
		public int Y = y;
	}


	private List<Point> moonPlaces = [
		new(-3, -2),
		new(-3, -1),
		new(-3, 0),
		new(-3, 1),
		new(2, -2),
		new(2, -1),
		new(2, 0),
		new(2, 1),
		new(-2, -3),
		new(-1, -3),
		new(0, -3),
		new(1, -3),
		new(-2, 2),
		new(-1, 2),
		new(0, 2),
		new(1, 2),
	];

	void placePlanetsMoons(MapGenUtils.GenStruct map, MapGenUtils.GenSystem system, S32 planetPosX,
		S32 planetPosY) {
		if (!_globals.MoonsEnabled) {
			return;
		}

		U32 maxMoonIndex = 0;
		for (U32 i = 0; i < _terrainTheme.MAX_TYPES; ++i) {
			if (system.theme.moonTypes[i] != "")
				++maxMoonIndex;
			else
				break;
		}

		U32 numMoons = GetRand(system.theme.minMoonsPerPlanet, system.theme.maxMoonsPerPlanet,
			system.theme.moonNumberFunc);
		while (numMoons > 0) {
			//find a good place near our planet
			U32 numPos = 0;
			S32 index;
			for (index = 0; index < NUM_MOON_POINTS; ++index) {
				if (SpaceEmpty(system, planetPosX + moonPlaces[index].X - 1, planetPosY + moonPlaces[index].Y - 1,
					    DMapGen.OVERLAP.NO_OVERLAP, 3))
					++numPos;
			}

			if (numPos == 0)
				return;
			U32 t = GetRand(0, numPos - 1, DMapGen.DMAP_FUNC.LINEAR);
			for (index = 0; index < NUM_MOON_POINTS; ++index) {
				if (SpaceEmpty(system, planetPosX + moonPlaces[index].X - 1, planetPosY + moonPlaces[index].Y - 1,
					    DMapGen.OVERLAP.NO_OVERLAP, 3)) {
					if (t == 0) {
						//place the moon
						FillPosition(system, planetPosX + moonPlaces[index].X - 1, planetPosY + moonPlaces[index].Y - 1,
							3, DMapGen.OVERLAP.NO_OVERLAP);
						S32 xPos = planetPosX + moonPlaces[index].X;
						S32 yPos = planetPosY + moonPlaces[index].Y;
						U32 moonID = GetRand(0, maxMoonIndex - 1, DMapGen.DMAP_FUNC.LINEAR);
						insertObject(system.theme.moonTypes[moonID],
							new Vector3(xPos + 0.5f, yPos + 0.5f, 0),
							0, system.systemID, system);
						break;
					}

					--t;
				}
			}

			--numMoons;
		}
	}

	bool SpaceEmpty(MapGenUtils.GenSystem system, S32 xPos, S32 yPos, DMapGen.OVERLAP overlap, U32 size) {
		if (xPos < 0 || yPos < 0) {
			return false;
		}
		if (xPos + size - 1 >= system.size)
			return false;
		if (yPos + size - 1 >= system.size)
			return false;
		for (U32 ix = 0; ix < size; ++ix) {
			for (U32 iy = 0; iy < size; ++iy) {
				U8 value = system.objectMap[xPos + ix, yPos + iy];
				if (value == MapGenUtils.GENMAP_TAKEN)
					return false;
				if (value == MapGenUtils.GENMAP_LEVEL1 && overlap == DMapGen.OVERLAP.NO_OVERLAP)
					return false;
				if (value == MapGenUtils.GENMAP_LEVEL2 && overlap != DMapGen.OVERLAP.LEVEL1)
					return false;
				if (value == MapGenUtils.GENMAP_PATH && overlap == DMapGen.OVERLAP.NO_OVERLAP)
					return false;
			}
		}

		return true;
	}

	void FillPosition(MapGenUtils.GenSystem system, S32 xPos, S32 yPos, U32 size, DMapGen.OVERLAP overlap) {
		for (U32 ix = 0; ix < size; ++ix) {
			if (xPos + ix >= 0 && xPos + ix < system.size) {
				for (U32 iy = 0; iy < size; ++iy) {
					if (yPos + iy >= 0 && yPos + iy < system.size) {
						system.omUsed++;
						if (overlap == DMapGen.OVERLAP.NO_OVERLAP)
							system.objectMap[xPos + ix, yPos + iy] = MapGenUtils.GENMAP_TAKEN;
						else if (overlap == DMapGen.OVERLAP.LEVEL1) {
							U8 value = system.objectMap[xPos + ix, yPos + iy];
							if (value == 0 || value == MapGenUtils.GENMAP_PATH)
								system.objectMap[xPos + ix, yPos + iy] = MapGenUtils.GENMAP_LEVEL1;
						} else if (overlap == DMapGen.OVERLAP.LEVEL2) {
							U8 value = system.objectMap[xPos + ix, yPos + iy];
							if (value == 0 || value == MapGenUtils.GENMAP_LEVEL1 || value == MapGenUtils.GENMAP_PATH)
								system.objectMap[xPos + ix, yPos + iy] = MapGenUtils.GENMAP_LEVEL2;
						}
					}
				}
			}
		}
	}

	bool FindPosition(MapGenUtils.GenSystem system, U32 width, DMapGen.OVERLAP overlap, ref S32 xPos, ref S32 yPos) {
		U32 numPos = 0;
		S32 ix;
		for (ix = 0; ix < system.size; ++ix) {
			for (S32 iy = 0; iy < system.size; ++iy) {
				if (SpaceEmpty(system, ix, iy, overlap, width))
					++numPos;
			}
		}

		if (numPos == 0) {
			xPos = -1;
			yPos = -1;
			return false;
		}

		U32 t = GetRand(0, numPos - 1, DMapGen.DMAP_FUNC.LINEAR);
		for (ix = 0; ix < system.size - width + 1; ++ix) {
			for (S32 iy = 0; iy < system.size - width + 1; ++iy) {
				if (SpaceEmpty(system, ix, iy, overlap, width)) {
					if (t == 0) {
						xPos = ix;
						yPos = iy;
						CQASSERT(xPos + width - 1 < system.size, "xPos+width-1 < system.size");
						CQASSERT(yPos + width - 1 < system.size, "yPos+width-1 < system.size");
						return true;
					}

					--t;
				}
			}
		}

		xPos = -1;
		yPos = -1;
		return false;
	}

//	bool ColisionWithObject(GenObj * obj,Vector vect,U32 rad,MAP_GEN_ENUM::OVERLAP overlap);

	void GenerateTerain(MapGenUtils.GenStruct map, MapGenUtils.GenSystem system) {
		//select system Kit
		U32 numTypes = 0;
		while (system.theme.systemKit[numTypes] != "" && numTypes < _terrainTheme.MAX_TYPES) {
			++numTypes;
		}

		if (numTypes > 0) {
			Console.WriteLine("Setting background kit to {0}",
				system.theme.systemKit[GetRand(0, numTypes - 1, DMapGen.DMAP_FUNC.LINEAR)]);
			// SECTOR->SetLightingKit(system->systemID,system->theme->systemKit[GetRand(0,numTypes-1,MAP_GEN_ENUM::LINEAR)]);
		}

		//gas Nebulas
		S32 resourceAmount = (S32)system.theme.numNuggetPatchesGas[map.terrainSize];
		numTypes = 0;
		while (system.theme.nuggetGasTypes[numTypes].terrainArchType != "" && numTypes < _terrainTheme.MAX_TYPES) {
			++numTypes;
		}

		if (numTypes > 0) {
			while (resourceAmount > 0) {
				U32 pos = GetRand(0, numTypes - 1, DMapGen.DMAP_FUNC.LINEAR);
				PlaceTerrain(map, system.theme.nuggetGasTypes[pos], system);
				--resourceAmount;
			}
		}

		//ore nebulas
		resourceAmount = (int)system.theme.numNuggetPatchesMetal[map.terrainSize];
		numTypes = 0;
		while (system.theme.nuggetMetalTypes[numTypes].terrainArchType != "" && numTypes < _terrainTheme.MAX_TYPES) {
			++numTypes;
		}

		if (numTypes > 0) {
			while (resourceAmount > 0) {
				U32 pos = GetRand(0, numTypes - 1, DMapGen.DMAP_FUNC.LINEAR);
				PlaceTerrain(map, system.theme.nuggetMetalTypes[pos], system);
				--resourceAmount;
			}
		}

		U32[] typeProb = new U32[_terrainTheme.MAX_TERRAIN];
		//find number of terrain types in theme and place required terrain
		numTypes = 0;
		U32 totalProb = 0;
		U32 totalPlaced = 0;
		while (system.theme.terrain[numTypes].terrainArchType != "" && numTypes < _terrainTheme.MAX_TERRAIN) {
			U32 numPlaced = 0;
			while (numPlaced < system.theme.terrain[numTypes].requiredToPlace) {
				++numPlaced;
				++totalPlaced;
				PlaceTerrain(map, system.theme.terrain[numTypes], system);
			}

			typeProb[numTypes] = (U32)((system.theme.terrain[numTypes].probability) * 10000);
			totalProb += typeProb[numTypes];
			++numTypes;
		}

		if (numTypes == 0 || totalProb == 0)
			return;
		//now generate the rest of the terrain
		while (system.theme.density[map.terrainSize] > (system.omUsed / ((SINGLE)(system.omStartEmpty)))) {
			U32 prob = GetRand(0, totalProb, DMapGen.DMAP_FUNC.LINEAR);
			U32 index = 0;
			while (index < numTypes) {
				if (typeProb[index] >= prob)
					break;
				else
					prob -= typeProb[index];
				++index;
			}

			CQASSERT(index < numTypes);

			PlaceTerrain(map, system.theme.terrain[index], system);
		}
	}

	void PlaceTerrain(MapGenUtils.GenStruct map, _terrainInfo terrain, MapGenUtils.GenSystem system) {
		//special types to place
		//field
		//antimatter ribbons
		Console.WriteLine($"Gettitng basic bitch data for: {terrain.terrainArchType}");
		var data = _baseFieldData.Find(x => x.terrainArchType == terrain.terrainArchType);

		if (data is null) {
			throw new Exception(
				$"Bad archtype name in random map generator.  Fix the data not a code bug, Name:{terrain.terrainArchType}");
		}

		if ((data.objClass == ObjClass.OC_NEBULA) || (data.objClass == ObjClass.OC_FIELD)) {
			BASE_FIELD_DATA fData = data;
			if (fData.fieldClass == FIELDCLASS.FC_ANTIMATTER) {
				switch (terrain.placement) {
					case DMapGen.PLACEMENT.RANDOM: {
						U32 length = GetRand(terrain.minToPlace, terrain.maxToPlace, terrain.numberFunc);
						PlaceRandomRibbon(terrain, length, 0, 0, system);
					}
						break;
					case DMapGen.PLACEMENT.SPOTS:
						CQASSERT(false, "NOT SUPPORTED");
						break;
					case DMapGen.PLACEMENT.CLUSTER:
						CQASSERT(false, "NOT SUPPORTED");
						break;
					case DMapGen.PLACEMENT.PLANET_RING:
						CQASSERT(false, "NOT SUPPORTED");
						break;
					case DMapGen.PLACEMENT.STREEKS:
						CQASSERT(false, "NOT SUPPORTED");
						break;
				}
			} else //a regular nebula
			{
				switch (terrain.placement) {
					case DMapGen.PLACEMENT.RANDOM: {
						U32 numToPlace = GetRand(terrain.minToPlace, terrain.maxToPlace, terrain.numberFunc);
						PlaceRandomField(terrain, numToPlace, 0, 0, system);
					}
						break;
					case DMapGen.PLACEMENT.SPOTS: {
						U32 numToPlace = GetRand(terrain.minToPlace, terrain.maxToPlace, terrain.numberFunc);
						PlaceSpottyField(terrain, numToPlace, 0, 0, system);
					}
						break;
					case DMapGen.PLACEMENT.CLUSTER:
						CQASSERT(false, "NOT SUPPORTED");
						break;
					case DMapGen.PLACEMENT.PLANET_RING: {
						PlaceRingField(terrain, system);
					}
						break;
					case DMapGen.PLACEMENT.STREEKS:
						CQASSERT(false, "NOT SUPPORTED");
						break;
				}
			}
		} else // it is a regular object
		{
			switch (terrain.placement) {
				case DMapGen.PLACEMENT.RANDOM: {
					U32 numToPlace = GetRand(terrain.minToPlace, terrain.maxToPlace, terrain.numberFunc);
					for (U32 i = 0; i < numToPlace; ++i) {
						S32 xPos = 0;
						S32 yPos = 0;
						bool bSuccess = FindPosition(system, terrain.size, terrain.overlap, ref xPos, ref yPos);
						if (bSuccess) {
							FillPosition(system, xPos, yPos, terrain.size, terrain.overlap);
							U32 halfWidth = (terrain.size) / 2;
							insertObject(terrain.terrainArchType,
								new Vector3(xPos + halfWidth, yPos + halfWidth, 0), 0,
								system.systemID, system);
						} else {
							system.omUsed += terrain.size * terrain.size;
						}
					}
				}
					break;
				case DMapGen.PLACEMENT.SPOTS:
					CQASSERT(false, "NOT SUPPORTED");
					break;
				case DMapGen.PLACEMENT.CLUSTER: {
					U32 numToPlace = GetRand(terrain.minToPlace, terrain.maxToPlace, terrain.numberFunc);
					var xPos = 0;
					var yPos = 0;
					bool bSuccess = FindPosition(system, terrain.size, terrain.overlap, ref xPos, ref yPos);
					if (bSuccess) {
						for (U32 i = 0; i < numToPlace; ++i) {
							U32 angle = GetRand(0, 360, DMapGen.DMAP_FUNC.LINEAR);
							U32 dist = GetRand(0, terrain.size / 2, DMapGen.DMAP_FUNC.LINEAR);
							Vector3 position =
								new Vector3(xPos + 0.5f, yPos + 0.5f, 0) +
								new Vector3(MathF.Cos(angle * MathF.PI / 180) * dist,
									MathF.Sin(angle * MathF.PI / 180) * dist, 0);
							insertObject(terrain.terrainArchType, position, 0, system.systemID, system);
						}

						FillPosition(system, xPos, yPos, terrain.size, terrain.overlap);
					} else {
						system.omUsed = terrain.size * terrain.size;
					}
				}
					break;
				case DMapGen.PLACEMENT.PLANET_RING:
					CQASSERT(false, "NOT SUPPORTED");
					break;
				case DMapGen.PLACEMENT.STREEKS:
					CQASSERT(false, "NOT SUPPORTED");
					break;
			}
		}
	}

	void PlaceRandomField(_terrainInfo terrain, U32 numToPlace, S32 startX, S32 startY,
		MapGenUtils.GenSystem system) {
		CQASSERT(numToPlace < 256, "If you are getting this I can make the number bigger");
		S32[] tempX = new S32[256];
		S32[] tempY = new S32[256];
		S32 tempIndex = 0;
		S32[] finalX = new S32[256];
		S32[] finalY = new S32[256];
		S32 finalIndex = 1;
		if (startX == 0 && startY == 0) {
			bool bSucess = FindPosition(system, 1, terrain.overlap, ref finalX[0], ref finalY[0]);
			if (!bSucess) {
				system.omUsed += numToPlace;
				return;
			}
		} else {
			finalX[0] = startX;
			finalY[0] = startY;
		}

		S32 lastFinal = 0;
		S32 numMade = 1;
		while (numMade < numToPlace) {
			lastFinal = finalIndex - 1;
			checkNewXY(tempX, tempY, ref tempIndex, finalX, finalY, finalIndex, terrain, system, finalX[lastFinal] + 1,
				finalY[lastFinal]);
			checkNewXY(tempX, tempY, ref tempIndex, finalX, finalY, finalIndex, terrain, system, finalX[lastFinal],
				finalY[lastFinal] + 1);
			if (finalX[lastFinal] != 0)
				checkNewXY(tempX, tempY, ref tempIndex, finalX, finalY, finalIndex, terrain, system,
					finalX[lastFinal] - 1, finalY[lastFinal]);
			if (finalY[lastFinal] != 0)
				checkNewXY(tempX, tempY, ref tempIndex, finalX, finalY, finalIndex, terrain, system, finalX[lastFinal],
					finalY[lastFinal] - 1);

			if (tempIndex == 0)
				return;
			U32 newIndex = GetRand(0, (uint)tempIndex - 1, DMapGen.DMAP_FUNC.LINEAR);
			finalX[finalIndex] = tempX[newIndex];
			finalY[finalIndex] = tempY[newIndex];

			removeFromArray(finalX[finalIndex], finalY[finalIndex], tempX, tempY, ref tempIndex);

			++finalIndex;
			++numMade;
		}

		U32 i;
		for (i = 0; i < numMade; ++i) {
			FillPosition(system, finalX[i], finalY[i], 1, terrain.overlap);
		}

		for (i = 0; i < finalIndex; ++i) {
			// TODO: felülvigyálni
			finalX[i] += 1;
			finalY[i] += 1;
		}

		// FIELDMGR->CreateField(terrain->terrainArchType,(S32 *)finalX,(S32 *)finalY,finalIndex,system->systemID);
		Console.WriteLine(
			$"CreateField {terrain.terrainArchType} finalx; {P(finalX[..finalIndex])}, finaly: {P(finalY[..finalIndex])}, finalIndex: {finalIndex}, systemId: {system.systemID}");
	}

	private string P<T>(IEnumerable<T> finalx) {
		return string.Join(",", finalx);
	}

	void PlaceSpottyField(_terrainInfo terrain, U32 numToPlace, S32 startX, S32 startY,
		MapGenUtils.GenSystem system) {
		numToPlace *= 2;
		CQASSERT(numToPlace < 256, "If you are getting this I can make the number bigger");
		S32[] tempX = new S32[256];
		S32[] tempY = new S32[256];
		S32 tempIndex = 0;
		S32[] finalX = new S32[256];
		S32[] finalY = new S32[256];
		S32 finalIndex = 1;
		if (startX == 0 && startY == 0) {
			bool bSucess = FindPosition(system, 1, terrain.overlap, ref finalX[0], ref finalY[0]);
			if (!bSucess) {
				system.omUsed += numToPlace;
				return;
			}
		} else {
			finalX[0] = startX;
			finalY[0] = startY;
		}

		S32 lastFinal = 0;
		S32 numMade = 1;
		while (numMade < numToPlace) {
			lastFinal = finalIndex - 1;
			checkNewXY(tempX, tempY, ref tempIndex, finalX, finalY, finalIndex, terrain, system, finalX[lastFinal] + 1,
				finalY[lastFinal]);
			checkNewXY(tempX, tempY, ref tempIndex, finalX, finalY, finalIndex, terrain, system, finalX[lastFinal],
				finalY[lastFinal] + 1);
			if (finalX[lastFinal] != 0)
				checkNewXY(tempX, tempY, ref tempIndex, finalX, finalY, finalIndex, terrain, system,
					finalX[lastFinal] - 1, finalY[lastFinal]);
			if (finalY[lastFinal] != 0)
				checkNewXY(tempX, tempY, ref tempIndex, finalX, finalY, finalIndex, terrain, system, finalX[lastFinal],
					finalY[lastFinal] - 1);

			if (tempIndex == 0)
				return;
			U32 newIndex = GetRand(0, (uint)tempIndex - 1, DMapGen.DMAP_FUNC.LINEAR);
			finalX[finalIndex] = tempX[newIndex];
			finalY[finalIndex] = tempY[newIndex];

			removeFromArray(finalX[finalIndex], finalY[finalIndex], tempX, tempY, ref tempIndex);

			++finalIndex;
			++numMade;
		}

		for (U32 count = 0; count < (finalIndex / 2); ++count) {
			finalX[count] = finalX[(count * 2) + 1];
			finalY[count] = finalY[(count * 2) + 1];
		}

		finalIndex = finalIndex / 2;
		U32 i;
		for (i = 0; i < numMade; ++i) {
			FillPosition(system, finalX[i], finalY[i], 1, terrain.overlap);
		}

		for (i = 0; i < finalIndex; ++i) {
			//felülvizsgálni
			finalX[i] += 1;
			finalY[i] += 1;
		}

		Console.WriteLine(
			$"CreateField {terrain.terrainArchType} finalx; {P(finalX[..finalIndex])}, finaly: {P(finalY[..finalIndex])}, finalIndex: {finalIndex}, systemId: {system.systemID}");

		// FIELDMGR->CreateField(terrain->terrainArchType,(S32 *)finalX,(S32 *)finalY,finalIndex,system->systemID);
	}

	void PlaceRingField(_terrainInfo terrain, MapGenUtils.GenSystem system) {
		throw new NotImplementedException();
	}

	void PlaceRandomRibbon(_terrainInfo terrain, U32 length, S32 startX, S32 startY,
		MapGenUtils.GenSystem system) {
		throw new NotImplementedException();
	}

	void BuildPaths(MapGenUtils.GenSystem system) {
		MapGenUtils.FlagPost post1 = null;
		for (U32 i = 0; i < system.numFlags; ++i) {
			if ((system.flags[i].type & MapGenUtils.FLAG_PATHON) == 0) {
				continue;
			}

			if (post1 != null) {
				connectPosts(post1, (system.flags[i]), system);
			}

			post1 = (system.flags[i]);
		}
	}

	//other
	void init() { }

	void removeFromArray(S32 nx, S32 ny, S32[] tempX, S32[] tempY, ref S32 tempIndex) {
		S32 skip = 0;
		for (S32 index = 0; index < tempIndex; ++index) {
			if (tempX[index] == nx && tempY[index] == ny)
				++skip;
			else if (skip > 0) {
				tempX[index - skip] = tempX[index];
				tempY[index - skip] = tempY[index];
			}
		}

		tempIndex -= skip;
	}

	bool isInArray(S32[] arrX, S32[] arrY, S32 index, S32 nx, S32 ny) {
		while (index > 0) {
			--index;
			if ((arrX[index] == nx) && (arrY[index] == ny))
				return true;
		}

		return false;
	}

	bool isOverlapping(S32[] arrX, S32[] arrY, S32 index, S32 nx, S32 ny) {
		throw new NotImplementedException();

		return false;
	}

	void checkNewXY(S32[] tempX, S32[] tempY, ref S32 tempIndex, S32[] finalX, S32[] finalY, S32 finalIndex,
		_terrainInfo terrain, MapGenUtils.GenSystem system, S32 newX, S32 newY) {
		if (newX >= system.size || newY >= system.size) {
			return;
		}

		if (tempIndex >= 256) {
			return;
		}

		if (isInArray(finalX, finalY, finalIndex, newX, newY)) {
			return;
		}

		if (!SpaceEmpty(system, newX, newY, terrain.overlap, 1)) {
			return;
		}

		tempX[tempIndex] = newX;
		tempY[tempIndex] = newY;
		++tempIndex;
	}

	void connectPosts(MapGenUtils.FlagPost post1, MapGenUtils.FlagPost post2,
		MapGenUtils.GenSystem system) {
		S32 xPos = post1.xPos;
		S32 yPos = post1.yPos;
		var rand = MapGenUtils.rand;
		while (true) {
			if (system.objectMap[xPos, yPos] != MapGenUtils.GENMAP_TAKEN)
				system.objectMap[xPos, yPos] = (byte)MapGenUtils.GENMAP_PATH;
			if (xPos == post2.xPos) {
				if (yPos == post2.yPos) {
					return;
				} else {
					if (yPos > post2.yPos) {
						S32 rVal = rand.Next(4);
						if (rVal == 0)
							++xPos;
						else if (rVal == 1)
							--xPos;
						else
							--yPos;
					} else {
						S32 rVal = rand.Next(4);
						if (rVal == 0)
							++xPos;
						else if (rVal == 1)
							--xPos;
						else
							++yPos;
					}
				}
			} else if (yPos == post2.yPos) {
				if (xPos > post2.xPos) {
					S32 rVal = rand.Next(4);
					if (rVal == 0)
						++yPos;
					else if (rVal == 1)
						--yPos;
					else
						--xPos;
				} else {
					S32 rVal = rand.Next(4);
					if (rVal == 0)
						++yPos;
					else if (rVal == 1)
						--yPos;
					else
						++xPos;
				}
			} else {
				if (xPos > post2.xPos) {
					if (yPos > post2.xPos) {
						S32 rVal = rand.Next(2);
						if (rVal == 0)
							--yPos;
						else
							--xPos;
					} else {
						S32 rVal = rand.Next(2);
						if (rVal == 0)
							++yPos;
						else
							--xPos;
					}
				} else {
					if (yPos > post2.xPos) {
						S32 rVal = rand.Next(2);
						if (rVal == 0)
							--yPos;
						else
							++xPos;
					} else {
						S32 rVal = rand.Next(2);
						if (rVal == 0)
							++yPos;
						else
							++xPos;
					}
				}
			}
		}
	}
}
