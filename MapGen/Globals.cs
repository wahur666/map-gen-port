
namespace MapGen;

public class Globals {
	
	public int MapNumPlayers { get; private set; }
	
	public void SetFileMaxPlayers(int mapNumPlayers) {
		MapNumPlayers = mapNumPlayers;
	}
}
