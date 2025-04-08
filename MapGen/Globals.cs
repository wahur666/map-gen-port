
namespace MapGen;

public class Globals {

	public bool MoonsEnabled { get; set; } = false;
	public int MapNumPlayers { get; private set; }
	
	public void SetFileMaxPlayers(int mapNumPlayers) {
		MapNumPlayers = mapNumPlayers;
	}
}
