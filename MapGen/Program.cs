﻿using Newtonsoft.Json;

namespace MapGen;

class Program {
	public static string mapgenfile = "data/bt_map_gen.json";
	
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

	    // Console.WriteLine("Available Archiypes: [\n  " + string.Join(",\n  ", allArchiypes) + "\n]");

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
			new ("ANTIMATTER!!Antimatter", ObjClass.OC_FIELD, FIELDCLASS.FC_ANTIMATTER),
			new ("ANTIMATTER!!mantis", ObjClass.OC_FIELD, FIELDCLASS.FC_ANTIMATTER),
			new ("ANTIMATTER!!solarian", ObjClass.OC_FIELD, FIELDCLASS.FC_ANTIMATTER),
			new ("ANTIMATTER!!terran", ObjClass.OC_FIELD, FIELDCLASS.FC_ANTIMATTER),
			new ("Field!!AsteroidsHeavy", ObjClass.OC_FIELD, FIELDCLASS.FC_ASTEROIDFIELD),
			new ("Field!!AsteroidsLight", ObjClass.OC_FIELD, FIELDCLASS.FC_ASTEROIDFIELD),
			new ("Field!!AsteroidsMed", ObjClass.OC_FIELD, FIELDCLASS.FC_ASTEROIDFIELD),
			new ("Field!!Debris", ObjClass.OC_FIELD, FIELDCLASS.FC_ASTEROIDFIELD),
			new ("Nebula!!Antimatter", ObjClass.OC_NEBULA, FIELDCLASS.FC_ANTIMATTER),
			new ("Nebula!!Antimatter(mantis)", ObjClass.OC_NEBULA, FIELDCLASS.FC_ANTIMATTER),
			new ("Nebula!!Antimatter(solarian)", ObjClass.OC_NEBULA, FIELDCLASS.FC_ANTIMATTER),
			new ("Nebula!!Antimatter(terran)", ObjClass.OC_NEBULA, FIELDCLASS.FC_ANTIMATTER),
			new ("Nebula!!Celsius(terran)", ObjClass.OC_NEBULA, FIELDCLASS.FC_NEBULA),
			new ("Nebula!!Cygnus(solarian)", ObjClass.OC_NEBULA, FIELDCLASS.FC_NEBULA),
			new ("Nebula!!Helious(terran)", ObjClass.OC_NEBULA, FIELDCLASS.FC_NEBULA),
			new ("Nebula!!Hyades(mantis)", ObjClass.OC_NEBULA, FIELDCLASS.FC_NEBULA),
			new ("Nebula!!Ion(solarian)", ObjClass.OC_NEBULA, FIELDCLASS.FC_NEBULA),
			new ("Nebula!!Lithium(mantis)", ObjClass.OC_NEBULA, FIELDCLASS.FC_NEBULA),
			new ("BlackHole", ObjClass.OC_NEBULA, FIELDCLASS.FC_NEBULA),
			new ("BlueStar", ObjClass.OC_BLACKHOLE, FIELDCLASS.FC_OTHER),
			new ("GreenStar", ObjClass.OC_BLACKHOLE, FIELDCLASS.FC_OTHER),
			new ("RedStar", ObjClass.OC_BLACKHOLE, FIELDCLASS.FC_OTHER),
			new ("YellowStar", ObjClass.OC_BLACKHOLE, FIELDCLASS.FC_OTHER),
	    ];
	    
	    FULLCQGAME fullcqgame = new FULLCQGAME();
	    
	    fullcqgame.Slots.Add(new Slot {
		    type = TYPE.HUMAN,
		    compChalange = COMP_CHALANGE.AVERAGE_CH,
		    state = STATE.READY,
		    race = RACE.TERRAN,
		    color = COLOR.YELLOW,
		    team = TEAM.NOTEAM,
		    dpid = 12345
	    });
	    
	    fullcqgame.Slots.Add(new Slot {
		    type = TYPE.COMPUTER,
		    compChalange = COMP_CHALANGE.HARD_CH,
		    state = STATE.READY,
		    race = RACE.MANTIS,
		    color = COLOR.RED,
		    team = TEAM.NOTEAM,
		    dpid = 0
	    });

	    fullcqgame.Slots.Add(new Slot {
		    type = TYPE.COMPUTER,
		    compChalange = COMP_CHALANGE.EASY_CH,
		    state = STATE.READY,
		    race = RACE.SOLARIAN,
		    color = COLOR.BLUE,
		    team = TEAM.NOTEAM,
		    dpid = 0
	    });
	    
	    fullcqgame.szMapName = "MyCustomMap";
	    fullcqgame.localSlot = 0;

	    fullcqgame.szPlayerNames[0] = "Player1";
	    fullcqgame.szPlayerNames[1] = "Player2";
	    fullcqgame.szPlayerNames[2] = "Player3";

	    fullcqgame.numSystems = 9;
	    fullcqgame.money = MONEY.LOW_MONEY;
	    fullcqgame.mapType = MAPTYPE.RANDOM_MAP;
	    fullcqgame.templateType = RANDOM_TEMPLATE.TEMPLATE_RING;
	    fullcqgame.mapSize = MAPSIZE.SMALL_MAP;
	    fullcqgame.terrain = TERRAIN.LIGHT_TERRAIN;

	    if (fullcqgame.templateType == RANDOM_TEMPLATE.TEMPLATE_RING &&
	        fullcqgame.numSystems % fullcqgame.ActiveSlots != 0) {
		    throw new Exception($"Number of systems must be a multiple of the number of players. Players: {fullcqgame.ActiveSlots}, Systems: {fullcqgame.numSystems}");
	    } 
	    if (fullcqgame.templateType == RANDOM_TEMPLATE.TEMPLATE_STAR && (fullcqgame.numSystems - 1) % fullcqgame.ActiveSlots != 0) {
		    throw new Exception($"Number of systems must be a multiple of the number of players plus 1. Players: {fullcqgame.ActiveSlots}, Systems: {fullcqgame.numSystems}");
	    }

	    mapgen.MoonsEnabled = false;
	    MapGen gen = new MapGen(mapgen, baseFieldData);

	    var numSystems = MapGen.GetPossibleSystemNumbers(fullcqgame);
	    
	    gen.GenerateMap(fullcqgame, 12345);
	    
	    
	    Console.WriteLine("Map generated!");
    }
}
