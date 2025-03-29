using U32 = uint;

namespace MapGen;

public class Globals {
	
	public uint MapNumPlayers { get; private set; }
	
	public void SetFileMaxPlayers(uint mapNumPlayers) {
		MapNumPlayers = mapNumPlayers;
	}
}
