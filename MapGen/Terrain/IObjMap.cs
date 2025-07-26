using System.Numerics;

namespace MapGen.Terrain;

public interface IObjMap {
	public ObjMapNode AddObjectToMap(object obj, uint systemId, int squareId, uint flags);
	public void RemoveObjectFromMap(object obj, uint systemId, int squareId);
	public int GetMapSquare(uint systemId, Vector2 pos);
	public int GetSquareNearPoint(uint systemId, Vector2 pos, int radius, List<int> refArray);
	public ObjMapNode GetNodeList(uint systemId, int squareId);
	public uint GetApparentPlayerID(ObjMapNode mapNode, uint allyMask);
	public bool IsObjectInMap(object obj);
	public void Init();
	public void Uninit();
}
