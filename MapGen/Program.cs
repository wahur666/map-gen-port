using Newtonsoft.Json;

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
	    Globals globals = new Globals();
	    MapGen gen = new MapGen(globals, mapgen);
	    gen.GenerateMap(fullcqgame, 12345);
	    
	    Console.WriteLine("Hello, World!");
    }
}
