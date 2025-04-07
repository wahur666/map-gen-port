using Newtonsoft.Json;

namespace MapGen;

class Program {
	public static string mapgenfile = "data/bt_map_gen-c2fwf.json";
	
    static void Main(string[] args) {
	    var settings = new JsonSerializerSettings {
		    Converters = new List<JsonConverter> { new BT_MAP_GEN_InfoConverter() }
	    };
	    mapgenfile = $"./{mapgenfile}";
	    Console.WriteLine($"Save file searched at {mapgenfile}");
	    BT_MAP_GEN? mapgen = null;
	    if (File.Exists(mapgenfile)) {
		    var text = File.ReadAllText(mapgenfile);
		    try {
			    mapgen = JsonConvert.DeserializeObject<BT_MAP_GEN>(text, settings);
		    } catch (Exception e) {
			    Console.WriteLine(e);
			    throw e;
		    }
	    } else {
		    throw new Exception("Unable to load file " + mapgenfile);
	    }

	    if (mapgen is null) {
		    throw new Exception("BT_MAP_GEN failed to load");
	    }

	    var allArchiypes = mapgen.themes
		    .SelectMany(x => x.terrain.Select(y => y.terrainArchType))
		    .Where(x => x != "")
		    .Distinct()
		    .ToList();
	    
	    allArchiypes.Sort();

	    Console.WriteLine("Available Archiypes: [\n  " + string.Join(",\n  ", allArchiypes) + "\n]");

	    /* Original + Code
	    Field!!AsteroidsHeavy,
	    Field!!AsteroidsLight,
	    Field!!AsteroidsMed,
	    Nebula!!Celsius(terran),
	    Nebula!!Cygnus(solarian),
	    Nebula!!Helious(terran),
	    Nebula!!Hyades(mantis),
	    Nebula!!Ion(solarian),
	    Nebula!!Lithium(mantis),
	    */
	    /* c2vu extra
	    BlackHole,
	     */
	    /* c2fwf extra
	    ANTIMATTER!!Antimatter,
	    ANTIMATTER!!mantis,
	    ANTIMATTER!!solarian,
	    ANTIMATTER!!terran,
	    BlackHole,
	    BlueStar,
	    GreenStar,
	    Nebula!!Antimatter,
	    Nebula!!Antimatter(mantis),
	    Nebula!!Antimatter(solarian),
	    Nebula!!Antimatter(terran),
	    RedStar,
	    YellowStar
		*/
	    
	    List<BASE_FIELD_DATA> baseFieldData = [
			new BASE_FIELD_DATA("ANTIMATTER!!Antimatter", ObjClass.OC_FIELD, FIELDCLASS.FC_ANTIMATTER),
			new BASE_FIELD_DATA("ANTIMATTER!!mantis", ObjClass.OC_FIELD, FIELDCLASS.FC_ANTIMATTER),
			new BASE_FIELD_DATA("ANTIMATTER!!solarian", ObjClass.OC_FIELD, FIELDCLASS.FC_ANTIMATTER),
			new BASE_FIELD_DATA("ANTIMATTER!!terran", ObjClass.OC_FIELD, FIELDCLASS.FC_ANTIMATTER),
			new BASE_FIELD_DATA("Field!!AsteroidsHeavy", ObjClass.OC_FIELD, FIELDCLASS.FC_ASTEROIDFIELD),
			new BASE_FIELD_DATA("Field!!AsteroidsLight", ObjClass.OC_FIELD, FIELDCLASS.FC_ASTEROIDFIELD),
			new BASE_FIELD_DATA("Field!!AsteroidsMed", ObjClass.OC_FIELD, FIELDCLASS.FC_ASTEROIDFIELD),
			new BASE_FIELD_DATA("Field!!Debris", ObjClass.OC_FIELD, FIELDCLASS.FC_ASTEROIDFIELD),
			new BASE_FIELD_DATA("Nebula!!Antimatter", ObjClass.OC_NEBULA, FIELDCLASS.FC_ANTIMATTER),
			new BASE_FIELD_DATA("Nebula!!Antimatter(mantis)", ObjClass.OC_NEBULA, FIELDCLASS.FC_ANTIMATTER),
			new BASE_FIELD_DATA("Nebula!!Antimatter(solarian)", ObjClass.OC_NEBULA, FIELDCLASS.FC_ANTIMATTER),
			new BASE_FIELD_DATA("Nebula!!Antimatter(terran)", ObjClass.OC_NEBULA, FIELDCLASS.FC_ANTIMATTER),
			new BASE_FIELD_DATA("Nebula!!Celsius(terran)", ObjClass.OC_NEBULA, FIELDCLASS.FC_NEBULA),
			new BASE_FIELD_DATA("Nebula!!Cygnus(solarian)", ObjClass.OC_NEBULA, FIELDCLASS.FC_NEBULA),
			new BASE_FIELD_DATA("Nebula!!Helious(terran)", ObjClass.OC_NEBULA, FIELDCLASS.FC_NEBULA),
			new BASE_FIELD_DATA("Nebula!!Hyades(mantis)", ObjClass.OC_NEBULA, FIELDCLASS.FC_NEBULA),
			new BASE_FIELD_DATA("Nebula!!Ion(solarian)", ObjClass.OC_NEBULA, FIELDCLASS.FC_NEBULA),
			new BASE_FIELD_DATA("Nebula!!Lithium(mantis)", ObjClass.OC_NEBULA, FIELDCLASS.FC_NEBULA),
			new BASE_FIELD_DATA("BlackHole", ObjClass.OC_NEBULA, FIELDCLASS.FC_NEBULA),
			new BASE_FIELD_DATA("BlueStar", ObjClass.OC_BLACKHOLE, FIELDCLASS.FC_OTHER),
			new BASE_FIELD_DATA("GreenStar", ObjClass.OC_BLACKHOLE, FIELDCLASS.FC_OTHER),
			new BASE_FIELD_DATA("RedStar", ObjClass.OC_BLACKHOLE, FIELDCLASS.FC_OTHER),
			new BASE_FIELD_DATA("YellowStar", ObjClass.OC_BLACKHOLE, FIELDCLASS.FC_OTHER),
	    ];
	    
	    FULLCQGAME fullcqgame = new FULLCQGAME();
	    fullcqgame.activeSlots = 3;
	    
	    fullcqgame.slot[0].type = TYPE.HUMAN;
	    fullcqgame.slot[0].compChalange = COMP_CHALANGE.AVERAGE_CH;
	    fullcqgame.slot[0].state = STATE.READY;
	    fullcqgame.slot[0].race = RACE.TERRAN;
	    fullcqgame.slot[0].color = COLOR.YELLOW;
	    fullcqgame.slot[0].team = TEAM.NOTEAM;
	    fullcqgame.slot[0].dpid = 12345;
	    
	    fullcqgame.slot[1].type = TYPE.COMPUTER;
	    fullcqgame.slot[1].compChalange = COMP_CHALANGE.HARD_CH;
	    fullcqgame.slot[1].state = STATE.READY;
	    fullcqgame.slot[1].race = RACE.MANTIS;
	    fullcqgame.slot[1].color = COLOR.RED;
	    fullcqgame.slot[1].team = TEAM.NOTEAM;
	    fullcqgame.slot[1].dpid = 0;

	    fullcqgame.slot[2].type = TYPE.COMPUTER;
	    fullcqgame.slot[2].compChalange = COMP_CHALANGE.EASY_CH;
	    fullcqgame.slot[2].state = STATE.READY;
	    fullcqgame.slot[2].race = RACE.SOLARIAN;
	    fullcqgame.slot[2].color = COLOR.BLUE;
	    fullcqgame.slot[2].team = TEAM.NOTEAM;
	    fullcqgame.slot[2].dpid = 0;
	    
	    fullcqgame.szMapName = "MyCustomMap";
	    fullcqgame.localSlot = 0;

	    fullcqgame.szPlayerNames[0] = "Player1";
	    fullcqgame.szPlayerNames[1] = "Player2";
	    fullcqgame.szPlayerNames[2] = "Player3";

	    fullcqgame.numSystems = 4;
	    fullcqgame.money = MONEY.LOW_MONEY;
	    fullcqgame.mapType = MAPTYPE.RANDOM_MAP;
	    fullcqgame.templateType = RANDOM_TEMPLATE.TEMPLATE_RANDOM;
	    fullcqgame.mapSize = MAPSIZE.SMALL_MAP;
	    fullcqgame.terrain = TERRAIN.LIGHT_TERRAIN;
	    
	    Globals globals = new Globals();
	    MapGen gen = new MapGen(globals, mapgen, baseFieldData);
	    gen.GenerateMap(fullcqgame, 12345);
	    
	    Console.WriteLine("Map generated!");
    }
}
