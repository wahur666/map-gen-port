﻿namespace MapGen;

using U32 = UInt32;


public class FULLCQGAME : CQGAME
{
    
    const int PLAYERNAMESIZE = 34; // enough to hold 32 characters plus null
    const int MAPNAMESIZE = 128;
    
    public string szMapName = string.Empty;
    public U32     localSlot;			// slot for this player (0 to MAX_PLAYERS-1)

    public string[] szPlayerNames = new String[MAX_PLAYERS];   
}
