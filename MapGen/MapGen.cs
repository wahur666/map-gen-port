using U32 = uint;

namespace MapGen;

using U8 = byte;



public class MapGen: IMapGen {
	/* IMapGen methods */

	public void GenerateMap(FULLCQGAME game, U32 seed) {
		throw new NotImplementedException();
	}

	public U32 GetBestSystemNumber(FULLCQGAME game, U32 approxNumber) {
		throw new NotImplementedException();
	}

	public U32 GetPosibleSystemNumbers(FULLCQGAME game, List<uint> list) {
		throw new NotImplementedException();
	}
	
	//map gen stuff

	void initMap (GenStruct & map, const FULLCQGAME & game);

	void insertObject (char * object,Vector position,U32 playerID, U32 systemID, GenSystem * system);

	//Util funcs

	U32 GetRand(U32 min, U32 max, MAP_GEN_ENUM::DMAP_FUNC mapFunc);

	void GenerateSystems(GenStruct & map);
		
		void generateSystemsRandom(GenStruct & map);

		void generateSystemsRing(GenStruct & map);

		void generateSystemsStar(GenStruct & map);

	bool SystemsOverlap(GenStruct & map, GenSystem * system);

	void GetJumpgatePositions(GenStruct & map, GenSystem * sys1, GenSystem * sys2, U32 & jx1, U32 & jy1, U32 & jx2, U32 & jy2);

	bool CrossesAnotherSystem(GenStruct & map, GenSystem * sys1, GenSystem * sys2, U32 jx1, U32 jy1, U32 jx2, U32 jy2);

	bool CrossesAnotherLink(GenStruct & map, GenJumpgate * gate);

	bool LinesCross(S32 line1x1, S32 line1y1, S32 line1x2, S32 line1y2, S32 line2x1, S32 line2y1, S32 line2x2, S32 line2y2);

	void SelectThemes(GenStruct & map);

	void CreateSystems(GenStruct & map);

	void RunHomeMacros(GenStruct & map);

		bool findMacroPosition(GenSystem * system,S32 centerX,S32 centerY,U32 range,U32 size,MAP_GEN_ENUM::OVERLAP overlap,U32 & posX,U32 &posY);

		void getMacroCenterPos(GenSystem * system,U32 & x, U32 & y);

		void createPlanetFromList(S32 xPos, S32 yPos, GenSystem * system, U32 range ,char planetList[][GT_PATH]);

		void placeMacroTerrain(S32 centerX,S32 centerY,GenSystem * system,S32 range,BT_MAP_GEN::_terrainTheme::_terrainInfo * terrainInfo);

	void CreateJumpgates(GenStruct & map);

		void createRandomGates3(GenStruct & map);

			void createGateLevel2(GenStruct & map, U32 totalLevel, U32 levelSystems,U32 targetSystems,U32 gateNum, U32 currentGates[64], U32 score, 
							 U32 * bestGates, U32 &bestScore, U32 & bestGateNum, bool moreAllowed);

		void createRandomGates2(GenStruct & map);

			void createGateLevel(GenStruct & map, U32 totalLevel, U32 levelSystems,U32 targetSystems,U32 gateNum, U32 currentGates[64], U32 score, 
							 U32 * bestGates, U32 &bestScore, U32 & bestGateNum, bool moreAllowed);

			U32 scoreGate(GenStruct & map,U32 gateIndex);

			void markSystems(U32 & systemUnconnected,GenSystem * system, U32 & systemsVisited);

		void createRandomGates(GenStruct & map);

		void createRingGates(GenStruct & map);

		void createStarGates(GenStruct & map);

		void createJumpgatesForIndexs(GenStruct & map,U32 index1, U32 index2);

	void PopulateSystems(GenStruct & map, IPANIM * ipAnim);

	void PopulateSystem(GenStruct & map,GenSystem * system);

		void placePlanetsMoons(GenStruct & map, GenSystem * system,U32 planetPosX,U32 planetPosY);

	bool SpaceEmpty(GenSystem * system,U32 xPos,U32 yPos,MAP_GEN_ENUM::OVERLAP overlap,U32 size);

	void FillPosition(GenSystem * system,U32 xPos, U32 yPos,U32 size,MAP_GEN_ENUM::OVERLAP overlap);

	bool FindPosition(GenSystem * system,U32 width,MAP_GEN_ENUM::OVERLAP overlap, U32 & xPos, U32 & yPos);

//	bool ColisionWithObject(GenObj * obj,Vector vect,U32 rad,MAP_GEN_ENUM::OVERLAP overlap);

	void GenerateTerain(GenStruct & map, GenSystem * system);

	void PlaceTerrain(GenStruct & map,BT_MAP_GEN::_terrainTheme::_terrainInfo terrain,GenSystem *system);

	void PlaceRandomField(BT_MAP_GEN::_terrainTheme::_terrainInfo * terrain,U32 numToPlace,S32 startX, S32 startY,GenSystem * system);

	void PlaceSpottyField(BT_MAP_GEN::_terrainTheme::_terrainInfo * terrain,U32 numToPlace,S32 startX, S32 startY,GenSystem * system);

	void PlaceRingField(BT_MAP_GEN::_terrainTheme::_terrainInfo * terrain,GenSystem *system);

	void PlaceRandomRibbon(BT_MAP_GEN::_terrainTheme::_terrainInfo * terrain,U32 length,S32 startX, S32 startY,GenSystem * system);

	void BuildPaths(GenSystem * system);

	//other
	void init (void);

	void removeFromArray(U32 nx,U32 ny,U32 * tempX,U32 * tempY,U32 & tempIndex);

	bool isInArray(U32 * arrX, U32 * arrY, U32 index, U32 nx, U32 ny);

	bool isOverlapping(U32 * arrX, U32 * arrY, U32 index, U32 nx, U32 ny);

	void checkNewXY(U32 * tempX,U32 * tempY,U32 & tempIndex, U32 * finalX, U32 * finalY, U32 finalIndex, BT_MAP_GEN::_terrainTheme::_terrainInfo terrain,
				GenSystem * system,U32 newX,U32 newY);

	void connectPosts(FlagPost * post1, FlagPost * post2,GenSystem * system);


}
