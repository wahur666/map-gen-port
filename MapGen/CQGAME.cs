namespace MapGen;

using DWORD = UInt32;
using U32 = UInt32;
using DPID = UInt32;
using S32 = Int32;

enum DIFFICULTY
{
    NODIFFICULTY,
    EASY,
    AVERAGE,
    HARD
}

enum TYPE
{
    HUMAN,
    COMPUTER
}

enum COMP_CHALANGE
{
    EASY_CH,
    AVERAGE_CH,
    HARD_CH,
    IMPOSIBLE_CH,
    NIGHTMARE_CH,
}

enum STATE
{
    OPEN, // slot can be used, but not active at this time
    CLOSED, // host has disallowed this slot
    ACTIVE, // slot is being used by computer or human player, has not accepted game rules yet
    READY // slot is active and player has accepted rules
}

enum RACE
{
    NORACE,
    TERRAN,
    MANTIS,
    SOLARIAN,
    VYRIUM,
}

enum COLOR
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

enum TEAM
{
    NOTEAM,
    _1,
    _2,
    _3,
    _4
}

struct SLOT
{
    TYPE type;
    COMP_CHALANGE compChalange;
    STATE state;
    RACE race;
    COLOR color;
    TEAM team;
    U32 zoneSeat;
    DPID dpid; // id of player, 0 if computer player
}

enum GAMETYPE // need 2 bits
{
    KILL_UNITS = -2,
    KILL_HQ_PLATS,
    MISSION_DEFINED, // must be == 0
    KILL_PLATS_FABS
}

enum MONEY // need 2 bits
{
    LOW_MONEY = -2,
    MEDIUM_MONEY,
    HIGH_MONEY
}

enum MAPTYPE // need 2 bits
{
    SELECTED_MAP = -2,
    USER_MAP, // from saved game dir
    RANDOM_MAP
}

enum MAPSIZE // need 2 bits
{
    SMALL_MAP = -2,
    MEDIUM_MAP,
    LARGE_MAP
}

enum TERRAIN // need 2 bits
{
    LIGHT_TERRAIN = -2,
    MEDIUM_TERRAIN,
    HEAVY_TERRAIN
}

enum STARTING_UNITS // need 2 bits
{
    UNITS_MINIMAL = -2,
    UNITS_MEDIUM,
    UNITS_LARGE
}

enum VISIBILITYMODE // need 2 bits
{
    VISIBILITY_NORMAL = -1,
    VISIBILITY_EXPLORED,
    VISIBILITY_ALL
}

enum RANDOM_TEMPLATE //need 2 bits
{
    TEMPLATE_NEW_RANDOM = -2,
    TEMPLATE_RANDOM,
    TEMPLATE_RING,
    TEMPLATE_STAR,
}

enum COMMANDLIMIT // need 2 bits
{
    COMMAND_LOW = -2,
    COMMAND_NORMAL,
    COMMAND_MID,
    COMMAND_HIGH
}

public class OPTIONS
{
    U32 version = 1;
    GAMETYPE gameType = GAMETYPE.KILL_HQ_PLATS;
    S32 gameSpeed = 1; // need enough bits for -16 to 15
    bool regenOn = false;
    bool spectatorsOn = false;
    bool lockDiplomacyOn = false;
    U32 numSystems;
    MONEY money;
    MAPTYPE mapType;
    RANDOM_TEMPLATE templateType;
    MAPSIZE mapSize;
    TERRAIN terrain;
    STARTING_UNITS units;
    VISIBILITYMODE visibility;
    COMMANDLIMIT commandLimit;
}

public class CQGAME : OPTIONS
{
    protected const int PLAYERNAMESIZE = 34; // enough to hold 32 characters plus null
    protected const int MAPNAMESIZE = 128;
    protected const int MAX_PLAYERS = 8;

    U32 activeSlots = 1; // valid from 1 to MAX_PLAYERS // 		U32 activeSlots:8;		
    bool bHostBusy = false; // host is not on the final screen // 		U32 bHostBusy:1
    bool startCountdown = false; // 		U32 startCountdown:4;	

    SLOT[] slot = new SLOT[MAX_PLAYERS];
}
