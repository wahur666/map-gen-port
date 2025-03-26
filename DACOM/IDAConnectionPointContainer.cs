namespace DACOM;

using BOOL32 = int;
// typedef BOOL32  (__stdcall * CONNCONTAINER_ENUM_PROC) (struct IDAConnectionPointContainer * container, struct IDAConnectionPoint *connPoint, void *context);
// public delegate bool ConnectionEnumProc(IDAConnectionPoint connPoint, IDAComponent client, object context);


using CONNCONTAINER_ENUM_PROC = Func<bool, IDAConnectionPointContainer, IDAConnectionPoint, object>;
public interface IDAConnectionPointContainer: IDAComponent {
	// GenResult FindConnectionPoint (const C8 *connectionName, struct IDAConnectionPoint **connPoint) = 0;
	GenResult FindConnectionPoint (string connectionName, IDAConnectionPoint[] connPoint);
	BOOL32 EnumerateConnectionPoints(CONNCONTAINER_ENUM_PROC proc, object context=null);
}
