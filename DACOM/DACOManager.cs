using System.Runtime.InteropServices;
using DACOM;

namespace DACOM;

using U32 = UInt32;
using DACOMENUMCALLBACK = Func<bool, IComponentFactory, string, uint, object>;
using C8 = Char;
using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public class DAComDesc {
	public uint Size { get; protected set; }

	private string? _interfaceName;

	public string? InterfaceName {
		get => _interfaceName;
		set {
			_interfaceName = value;
			Size = (uint)Marshal.SizeOf(typeof(DAComDesc));
		}
	}

	public DAComDesc(string? interfaceName = null) {
		InterfaceName = interfaceName;
	}
}

/// Self-initializing structure used by clients to request a desired DA COM
/// implementation. AGGDESC is used by components that support aggregation.
/// A successfull call to CreateInstance() using an AGGDESC will return a pointer
/// to an IAggregateComponent interface. Use QueryInterface() to retrieve more
/// specific interface pointers. 
[StructLayout(LayoutKind.Sequential)]
public class AggDesc : DAComDesc {
	public IDAComponent? Outer { get; set; }
	public IDAComponent? Inner { get; set; }
	public string? Description { get; set; }

	public AggDesc(string? interfaceName = null, string? description = null)
		: base(interfaceName) {
		Outer = null;
		Inner = null;
		Description = description;
		Size = (uint)Marshal.SizeOf(typeof(AggDesc));
	}
}

/// Abstract class from which all class factories inherit
public interface IComponentFactory : IDAComponent {
	public static string IID_IComponentFactory = "IComponentFactory";
	GenResult CreateInstance(DAComDesc? descriptor, ref object? instance);
}

/// Abstract class from which all aggregatable classes inherit
interface IAggregateComponent : IDAComponent {
	public static string IID_IAggregateComponent = "IAggregateComponent";
	GenResult Initialize();
};
//--------------------------------------------------------------------------//
//---------------------------ICOManager Interface---------------------------//
//--------------------------------------------------------------------------//

//-------------------------------------
// Priorities for RegisterComponent
//-------------------------------------

public interface ICOManager : IComponentFactory {
	GenResult RegisterComponent(IComponentFactory? component, string? interface_name,
		U32 priority = DACOManager.DACOM_NORMAL_PRIORITY);

	GenResult UnregisterComponent(IComponentFactory component, string interface_name = "");
	GenResult EnumerateComponents(string interface_name, DACOMENUMCALLBACK callback, object context);
	GenResult ShutDown();
	GenResult SetINIConfig(string info, U32 flags = 0);
}

public class AddedLibrary() {
	object instance; // Win32 DLL instance
	string base_name; // Short library filename prefix
	AddedLibrary next; // Used by LList template
	AddedLibrary prev;
}

public class RegisteredObject() {
	public IComponentFactory component; // Component interface
	public string interface_name; // Name of interface
	public U32 priority; // Implementation priority
	public AddedLibrary library; // may be null if unknown

	public RegisteredObject next; // Used by LList template
	public RegisteredObject prev;
}



public class DACOManager : ICOManager {
	public static string DACOManager_interface_name = "ICOManager";

	public static uint DACOM_HIGH_PRIORITY = 0xC0000000;
	public const uint DACOM_NORMAL_PRIORITY = 0x80000000;
	public static uint DACOM_LOW_PRIORITY = 0x40000000;
	public static uint DACOM_INI_WRITABLE = 0x00000001;
	public static uint DACOM_INI_STRING = 0x00000002;

	private static readonly DACOManager Instance = new DACOManager();

	private uint _referenceCount;
	private bool _initialized;
	private uint _registrationCount;
	private AddedLibrary _pCurrentLibrary;

	private LinkedList<RegisteredObject> _objectList;
	private LinkedList<AddedLibrary> libraryList;
	private IDAComponent innerParser;
	
	public DACOManager() {
		_referenceCount = 1;
		_initialized = false;
		_pCurrentLibrary = null;
		innerParser = null;
		_objectList = [];
		libraryList = [];
	}


	/// All clients of DACOM (including component objects as well as the
	/// application itself) must call DACOM_Acquire() to obtain an instance
	/// pointer to the DA Component Manager
	public static ICOManager DACOM_Acquire() {
		return Instance;
	}

	public GenResult QueryInterface(string interface_name, ref object instance) {
		if (instance is null) {
			throw new ArgumentException("instance cannot be null");
		}

		instance = null;
		if (interface_name == "ICOManager") {
			instance = this;
			return GenResult.GR_OK;
		}

		if (innerParser is not null) {
			throw new NotImplementedException();
		}

		return GenResult.GR_INTERFACE_UNSUPPORTED;
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
		GenResult result = GenResult.GR_INTERFACE_UNSUPPORTED;
		if (instance is null) {
			// throw new ArgumentException("instance cannot be null");
			Console.WriteLine("Instance is null!!!");
		}

		instance = null;
		if (descriptor?.InterfaceName is null ||
		    descriptor.InterfaceName == DACOManager_interface_name) {
			result = GenResult.GR_OK;
			instance = true;
		} else {
			foreach (var obj in _objectList) {
				if (obj.interface_name == descriptor.InterfaceName) {
					result = obj.component.CreateInstance(descriptor, ref instance);
					if (result == GenResult.GR_OK) {
						break;
					}
				}
			}
		}

		return result;
	}

	public GenResult RegisterComponent(IComponentFactory? component, string? interface_name,
		U32 priority = DACOManager.DACOM_NORMAL_PRIORITY) {
		if (component is null || interface_name is null) {
			return GenResult.GR_INVALID_PARMS;
		}

		RegisteredObject obj = null;
		foreach (var obj1 in _objectList) {
			obj = obj1;
			if (priority >= obj.priority) {
				break;
			}
		}

		var newObj = new RegisteredObject();
		newObj.priority = priority;
		newObj.component = component;
		newObj.interface_name = interface_name;
		newObj.library = _pCurrentLibrary;
		if (_objectList.Count == 0) {
			_objectList.AddFirst(newObj);
		} else {
			_objectList.AddBefore(_objectList.Find(obj), newObj);
		}
		_registrationCount += 1;
		return GenResult.GR_OK;
	}

	public GenResult UnregisterComponent(IComponentFactory component, string interface_name = "") {
		throw new NotImplementedException();
	}

	public GenResult EnumerateComponents(string interface_name, DACOMENUMCALLBACK callback, object context) {
		throw new NotImplementedException();
	}

	public GenResult ShutDown() {
		throw new NotImplementedException();
	}

	public GenResult SetINIConfig(string info, U32 flags = 0) {
		throw new NotImplementedException();
	}

	public GenResult AddLibrary(string dllFileName) {
		throw new NotImplementedException();
	}
	
	public GenResult RemoveLibrary(string dllFileName) {
        throw new NotImplementedException();
    }

	public bool LoadAllFromDirectory() {
		throw new NotImplementedException();
	}

	public bool Initialize() {
		throw new NotImplementedException();
	}

	public static bool GetAbsolutePath() {
		throw new NotImplementedException();
	}
	
	void RegisterHeap (ICOManager pManager)
	{
		throw new NotImplementedException();
	}
}

