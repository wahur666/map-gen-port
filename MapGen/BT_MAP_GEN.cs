using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace MapGen;

public class TerrainInfo() {
	public string terrainArchType = "";
	public float probability = 0f;
	public uint minToPlace = 0;
	public uint maxToPlace = 0;
	public DMapGen.DMAP_FUNC numberFunc = DMapGen.DMAP_FUNC.LINEAR;
	public uint size = 0;
	public uint requiredToPlace = 0;
	public DMapGen.OVERLAP overlap = DMapGen.OVERLAP.NO_OVERLAP;
	public DMapGen.PLACEMENT placement = DMapGen.PLACEMENT.RANDOM;
}

[JsonConverter(typeof(BT_MAP_GEN_InfoConverter))]
public class Info() {
	public DMapGen.OVERLAP? overlap = null;
	public TerrainInfo? terrainInfo = null;
}

public class Macros() {
	public DMapGen.MACRO_OPERATION operation = DMapGen.MACRO_OPERATION.MC_PLACE_HABITABLE_PLANET;
	public uint range = 0;
	public bool active = false;
	public Info info = default;
}

public class _terrainTheme() {
	public const int MAX_TERRAIN = 20;
	public const int MAX_TYPES = 6;
	public const int MAX_MACROS = 15;
	public string[] systemKit = new string[MAX_TYPES];

	public string[] metalPlanets = new string[MAX_TYPES];
	public string[] gasPlanets = new string[MAX_TYPES];
	public string[] habitablePlanets = new string[MAX_TYPES];
	public string[] otherPlanets = new string[MAX_TYPES];

	public string[] moonTypes = new string[MAX_TYPES];

	public DMapGen.SECTOR_SIZE sizeOk = DMapGen.SECTOR_SIZE.SMALL_SIZE; //dependant on size setting
	public uint minSize = 0;
	public uint maxSize = 0;
	public DMapGen.DMAP_FUNC sizeFunc = DMapGen.DMAP_FUNC.LINEAR;

	public uint[] numHabitablePlanets = new uint[3]; //dependant on resource setting
	public uint[] numMetalPlanets = new uint[3]; //dependant on resource setting
	public uint[] numGasPlanets = new uint[3]; //dependant on resource setting
	public uint[] numOtherPlanets = new uint[3]; //dependant on resource setting

	public uint minMoonsPerPlanet = 0;
	public uint maxMoonsPerPlanet = 0;
	public DMapGen.DMAP_FUNC moonNumberFunc = 0;

	public uint[] numNuggetPatchesMetal = new uint[3]; //dependant on resource setting
	public uint[] numNuggetPatchesGas = new uint[3]; //dependant on resource setting

	public TerrainInfo[] terrain = new TerrainInfo[MAX_TERRAIN];
	public TerrainInfo[] nuggetMetalTypes = new TerrainInfo[MAX_TYPES];
	public TerrainInfo[] nuggetGasTypes = new TerrainInfo[MAX_TYPES];
	public bool okForPlayerStart = false;
	public bool okForRemoteSystem = false;
	public float[] density = new float[3]; //dependant on terrain setting
	public Macros[] macros = new Macros[MAX_MACROS];
}

public class BT_MAP_GEN {
	public const int MAX_THEMES = 30;
	public _terrainTheme[] themes = new _terrainTheme[MAX_THEMES];
	public bool MoonsEnabled { get; set; } = false;
}
