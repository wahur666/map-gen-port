using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace MapGen;

using U32 = UInt32;
using SINGLE = float;

public class _terrainInfo() {
	public const int GT_PATH = 32;
	public string terrainArchType = "";
	public SINGLE probability = 0f;
	public U32 minToPlace = 0;
	public U32 maxToPlace = 0;
	public DMapGen.DMAP_FUNC numberFunc = DMapGen.DMAP_FUNC.LINEAR;
	public U32 size = 0;
	public U32 requiredToPlace = 0;
	public DMapGen.OVERLAP overlap = DMapGen.OVERLAP.NO_OVERLAP;
	public DMapGen.PLACEMENT placement = DMapGen.PLACEMENT.RANDOM;
}

[JsonConverter(typeof(BT_MAP_GEN_InfoConverter))]
public class _info() {
	public DMapGen.OVERLAP? overlap = null;
	public _terrainInfo? terrainInfo = null;
}

public class _macros() {
	public DMapGen.MACRO_OPERATION operation = DMapGen.MACRO_OPERATION.MC_PLACE_HABITABLE_PLANET;
	public U32 range = 0;
	public bool active = false;
	public _info info = default;
}

public class _terrainTheme() {
	public const int MAX_TERRAIN = 20;
	public const int MAX_TYPES = 6;
	public const int MAX_MACROS = 15;
	public const int GT_PATH = 32;
	public string[] systemKit = new string[MAX_TYPES];

	public string[] metalPlanets = new string[MAX_TYPES];
	public string[] gasPlanets = new string[MAX_TYPES];
	public string[] habitablePlanets = new string[MAX_TYPES];
	public string[] otherPlanets = new string[MAX_TYPES];

	// public byte[,] moonTypes = new byte[MAX_TYPES,GT_PATH];

	public DMapGen.SECTOR_SIZE sizeOk = DMapGen.SECTOR_SIZE.SMALL_SIZE; //dependant on size setting
	public U32 minSize = 0;
	public U32 maxSize = 0;
	public DMapGen.DMAP_FUNC sizeFunc = DMapGen.DMAP_FUNC.LINEAR;

	public U32[] numHabitablePlanets = new U32[3]; //dependant on resource setting
	public U32[] numMetalPlanets = new U32[3]; //dependant on resource setting
	public U32[] numGasPlanets = new U32[3]; //dependant on resource setting
	public U32[] numOtherPlanets = new U32[3]; //dependant on resource setting

	public U32 minMoonsPerPlanet = 0;
	public U32 maxMoonsPerPlanet = 0;
	public DMapGen.DMAP_FUNC moonNumberFunc = 0;

	public U32[] numNuggetPatchesMetal = new U32[3]; //dependant on resource setting
	public U32[] numNuggetPatchesGas = new U32[3]; //dependant on resource setting

	public _terrainInfo[] terrain = new _terrainInfo[MAX_TERRAIN];
	public _terrainInfo[] nuggetMetalTypes = new _terrainInfo[MAX_TYPES];
	public _terrainInfo[] nuggetGasTypes = new _terrainInfo[MAX_TYPES];
	public bool okForPlayerStart = false;
	public bool okForRemoteSystem = false;
	public SINGLE[] density = new SINGLE[3]; //dependant on terrain setting
	public _macros[] macros = new _macros[MAX_MACROS];
}

public class BT_MAP_GEN {
	public const int MAX_THEMES = 30;
	public _terrainTheme[] themes = new _terrainTheme[MAX_THEMES];
}
