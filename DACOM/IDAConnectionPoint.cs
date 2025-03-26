namespace DACOM;

using U32 = UInt32;
using BOOL32 = int;
using CONNECTION_ENUM_PROC = Func<bool, IDAConnectionPoint, IDAComponent, object>;

// typedef BOOL32  (__stdcall * CONNECTION_ENUM_PROC) (struct IDAConnectionPoint * connPoint, struct IDAComponent *client, void *context);

public interface IDAConnectionPoint: IDAComponent {
		// U32 GetOutgoingInterface (C8 *interfaceName, U32 bufferLength);
		U32 GetOutgoingInterface (string interfaceName, U32 bufferLength);
		GenResult GetContainer (IDAConnectionPointContainer[] container);
		GenResult Advise (IDAComponent component, ref U32 handle);
		GenResult Unadvise (U32 handle);
		BOOL32 EnumerateConnections(CONNECTION_ENUM_PROC proc, object context = null);
}
