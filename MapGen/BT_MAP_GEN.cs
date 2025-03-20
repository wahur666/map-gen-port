using System.Runtime.InteropServices;

namespace MapGen;

using U32 = UInt32;
using SINGLE = float;

public struct _terrainInfo() {
	const int GT_PATH = 32;
	byte[] terrainArchType = new byte[GT_PATH];
	SINGLE probability = 0f;
	U32 minToPlace = 0;
	U32 maxToPlace = 0;
	DMapGen.DMAP_FUNC numberFunc = DMapGen.DMAP_FUNC.LINEAR;
	U32 size = 0;
	U32 requiredToPlace = 0;
	DMapGen.OVERLAP overlap = DMapGen.OVERLAP.NO_OVERLAP;
	DMapGen.PLACEMENT placement = DMapGen.PLACEMENT.RANDOM;
}

[StructLayout(LayoutKind.Explicit)]
public struct _info {
	[FieldOffset(0)] public _terrainInfo terrainInfo;
	[FieldOffset(0)] public DMapGen.OVERLAP overlap;
}

public struct _macros() {
	public DMapGen.MACRO_OPERATION operation = DMapGen.MACRO_OPERATION.MC_PLACE_HABITABLE_PLANET;
	public U32 range = 0;
	public bool active = false;
	public _info info;
}

public struct _terrainTheme() {
	public static readonly int MAX_TERRAIN = 20;
	public static readonly int MAX_TYPES = 6;
	public static readonly int MAX_MACROS = 15;
	public static readonly int GT_PATH = 32;
	public byte systemKit = 0;

	public byte[,] metalPlanets = new byte[MAX_TYPES, GT_PATH];
	public byte[,] gasPlanets = new byte[MAX_TYPES,GT_PATH];
	public byte[,] habitablePlanets = new byte[MAX_TYPES,GT_PATH];
	public byte[,] otherPlanets = new byte[MAX_TYPES,GT_PATH];

	public byte[,] moonTypes = new byte[MAX_TYPES,GT_PATH];

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
	public SINGLE[] desitiy = new SINGLE[3]; //dependant on terrain setting
	public _macros[] macros = new _macros[MAX_MACROS];
}

public class BT_MAP_GEN {
	public static readonly int MAX_THEMES = 30;
	public _terrainTheme[] themes = new _terrainTheme[MAX_THEMES];
}
