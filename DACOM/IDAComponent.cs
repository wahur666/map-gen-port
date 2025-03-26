namespace DACOM;

using U32 = UInt32;

public interface IDAComponent {
	public static string IID_IDAComponent = "IDAComponent";

	// GenResult QueryInterface(const C8 *interface_name, object instance);
	GenResult QueryInterface(string interface_name, ref object instance);
	U32 AddRef();

	void DACOM_RELEASE(ref IDAComponent? x) {
		if (x is not null) {
			x.Release();
			x = null;
		}
	}

	U32 Release();

	// GenResult QueryOutgoingInterface (const C8 *connectionName, struct IDAConnectionPoint **connection);
	GenResult QueryOutgoingInterface(string connectionName, IDAConnectionPoint[] connection);
}
