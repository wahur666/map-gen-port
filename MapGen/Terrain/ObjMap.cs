using System.Numerics;

namespace MapGen.Terrain;

public class ObjMap: IObjMap {
	public static int GRIDSIZE = 4096;
	public static int MAX_SYS_SIZE = GRIDSIZE * 64;

	public static int SQUARE_SIZE = 8192;
	public static int EEP = (MAX_SYS_SIZE / SQUARE_SIZE + 1);
	public static int MAX_MAP_REF_ARRAY_SIZE = EEP * EEP;

	public static uint OM_AIR = 0x00000001;
	public static uint OM_UNTOUCHABLE = 0x00000002;
	public static uint OM_TARGETABLE = 0x00000004;
	public static uint OM_RESERVED_SHADOW = 0xFF000000;
	public static uint OM_SHADOW = 0x00000008;
	public static uint OM_SYSMAP_FIRSTPASS = 0x00000010;
	public static uint OM_MIMIC = 0x00000020;
	public static uint OM_EXPLOSION = 0x00000040;
	
	public ObjMapNode AddObjectToMap(object obj, uint systemId, int squareId, uint flags) {
		throw new NotImplementedException();
	}

	public void RemoveObjectFromMap(object obj, uint systemId, int squareId) {
		throw new NotImplementedException();
	}

	public int GetMapSquare(uint systemId, Vector2 pos) {
		throw new NotImplementedException();
	}

	public int GetSquareNearPoint(uint systemId, Vector2 pos, int radius, List<int> refArray) {
		throw new NotImplementedException();
	}

	public ObjMapNode GetNodeList(uint systemId, int squareId) {
		throw new NotImplementedException();
	}

	public uint GetApparentPlayerID(ObjMapNode mapNode, uint allyMask) {
		throw new NotImplementedException();
	}

	public bool IsObjectInMap(object obj) {
		throw new NotImplementedException();
	}

	public void Init() {
		throw new NotImplementedException();
	}

	public void Uninit() {
		throw new NotImplementedException();
	}
}

public class ObjMapNode {
	public object obj;
	public uint dwMissionId;
	public uint flags;
	public ObjMapNode next;
}

