using DOSFile;
using U32 = uint;

namespace DACOM;

public class DOSFileSystem: IFileSystem {
	public string interface_name = "IFileSystem";

	private static DAComponent<DOSFileSystem> pFirstSystem = null;
	
	public GenResult QueryInterface(string interface_name, ref object instance) {
		throw new NotImplementedException();
	}

	public U32 AddRef() {
		throw new NotImplementedException();
	}

	public U32 Release() {
		throw new NotImplementedException();
	}

	public GenResult QueryOutgoingInterface(string connectionName, IDAConnectionPoint[] connection) {
		throw new NotImplementedException();
	}

	public GenResult CreateInstance(DAComDesc? descriptor, ref object instance) {
		throw new NotImplementedException();
	}
	
	
	public bool InitializeLibrary() {
		var DACOM = DACOManager.DACOM_Acquire();
		pFirstSystem = new DAComponent<DOSFileSystem>();
		if (pFirstSystem.I is not null)
		{
			DACOM.RegisterComponent(pFirstSystem.I, interface_name, DACOManager.DACOM_LOW_PRIORITY);
		}

		IComponentFactory lpSystem = BaseUTF.CreateBaseUTF();
		DACOM.RegisterComponent(lpSystem, interface_name);

		// lpSystem = CreateMemFileFactory();
		// if (lpSystem)
		// {
			// DACOM.RegisterComponent(lpSystem, interface_name, DACOManager.DACOM_NORMAL_PRIORITY+1);
		// }

		// lpSystem = CreateSearchPathFactory();
		// if (lpSystem)
		// {
			// DACOM.RegisterComponent(lpSystem, "ISearchPath", DACOManager.DACOM_NORMAL_PRIORITY+1);
			// lpSystem->Release();
		// }

		return true;
	}

}
