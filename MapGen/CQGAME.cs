namespace MapGen;


public enum DIFFICULTY
{
    NODIFFICULTY,
    EASY,
    AVERAGE,
    HARD
}

public enum TYPE
{
    HUMAN,
    COMPUTER
}

public enum COMP_CHALANGE
{
    EASY_CH,
    AVERAGE_CH,
    HARD_CH,
    IMPOSIBLE_CH,
    NIGHTMARE_CH,
}

public enum STATE
{
    OPEN, // slot can be used, but not active at this time
    CLOSED, // host has disallowed this slot
    ACTIVE, // slot is being used by computer or human player, has not accepted game rules yet
    READY // slot is active and player has accepted rules
}

public enum RACE
{
    NORACE,
    TERRAN,
    MANTIS,
    SOLARIAN,
    VYRIUM,
}

public enum COLOR
{
    UNDEFINEDCOLOR, // used for computer players
    YELLOW,
    RED,
    BLUE,
    PINK,
    GREEN,
    ORANGE,
    PURPLE,
    AQUA
}

public enum TEAM
{
    NOTEAM,
    _1,
    _2,
    _3,
    _4
}

public class Slot
{
    public TYPE type;
    public COMP_CHALANGE compChalange;
    public STATE state;
    public RACE race;
    public COLOR color;
    public TEAM team;
    public uint zoneSeat;
    public int dpid; // id of player, 0 if computer player
}

public enum GAMETYPE // need 2 bits
{
    KILL_UNITS = -2,
    KILL_HQ_PLATS,
    MISSION_DEFINED, // must be == 0
    KILL_PLATS_FABS
}

public enum MONEY // need 2 bits
{
    LOW_MONEY = -2,
    MEDIUM_MONEY,
    HIGH_MONEY
}

public enum MAPTYPE // need 2 bits
{
    SELECTED_MAP = -2,
    USER_MAP, // from saved game dir
    RANDOM_MAP
}

public enum MAPSIZE // need 2 bits
{
    SMALL_MAP = -2,
    MEDIUM_MAP,
    LARGE_MAP
}

public enum TERRAIN // need 2 bits
{
    LIGHT_TERRAIN = -2,
    MEDIUM_TERRAIN,
    HEAVY_TERRAIN
}

public enum STARTING_UNITS // need 2 bits
{
    UNITS_MINIMAL = -2,
    UNITS_MEDIUM,
    UNITS_LARGE
}

public enum VISIBILITYMODE // need 2 bits
{
    VISIBILITY_NORMAL = -1,
    VISIBILITY_EXPLORED,
    VISIBILITY_ALL
}

public enum RANDOM_TEMPLATE //need 2 bits
{
	/// <summary>
	/// Pure random
	/// </summary>
    TEMPLATE_NEW_RANDOM = -2,
    TEMPLATE_RANDOM,
    TEMPLATE_RING,
    TEMPLATE_STAR,
}

public enum COMMANDLIMIT // need 2 bits
{
    COMMAND_LOW = -2,
    COMMAND_NORMAL,
    COMMAND_MID,
    COMMAND_HIGH
}

public class OPTIONS
{
    public  uint version = 1;
    public  GAMETYPE gameType = GAMETYPE.KILL_HQ_PLATS;
    public  int gameSpeed = 1; // need enough bits for -16 to 15
    public  bool regenOn = false;
    public  bool spectatorsOn = false;
    public  bool lockDiplomacyOn = false;
    public  int numSystems;
    public  MONEY money;
    public  MAPTYPE mapType;
    public  RANDOM_TEMPLATE templateType;
    public  MAPSIZE mapSize;
    public  TERRAIN terrain;
    public  STARTING_UNITS units;
    public  VISIBILITYMODE visibility;
    public  COMMANDLIMIT commandLimit;
}

public class CQGAME : OPTIONS
{
    public const int PLAYERNAMESIZE = 34; // enough to hold 32 characters plus null
    public const int MAPNAMESIZE = 128;
    public static readonly int MAX_PLAYERS = 8;

    public List<Slot> Slots = [];
    
    public int ActiveSlots {
	    get {
		    return Slots.Count(slot => slot.state is STATE.READY or STATE.ACTIVE);
	    }
    }
}
