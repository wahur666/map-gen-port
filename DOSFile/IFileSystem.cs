using System.Runtime.InteropServices;

namespace DACOM;

public interface IFileSystem : IComponentFactory {
}

[StructLayout(LayoutKind.Sequential)]
public struct LPSECURITY_ATTRIBUTES() {
	int nLength = 0;
	object lpSecurityDescriptor = null;
	bool bInheritHandle = false;
}

[StructLayout(LayoutKind.Sequential)]
public class DAFILEDESC : DAComDesc {
	public const uint GENERIC_READ = 0x80000000;
	public const uint FILE_SHARE_READ = 0x00000001;
	public const uint OPEN_EXISTING = 3;
	public const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;
	public const uint FILE_FLAG_SEQUENTIAL_SCAN = 0x08000000;


	public string lpImplementation = "";
	public string lpFileName = "";
	public uint dwDesiredAccess = 0;
	public uint dwShareMode = 0;
	public LPSECURITY_ATTRIBUTES lpSecurityAttributes = new LPSECURITY_ATTRIBUTES();
	public uint dwCreationDistribution = 0;
	public uint dwFlagsAndAttributes = 0;
	public object? hTemplateFile = null;
	public IFileSystem? lpParent = null;
	public object? hParent = null;
	public int hFindFirst;
	public const int INVALID_HANDLE_VALUE = -1;

	public DAFILEDESC(string? _file_name = null, string _interface_name = "IFileSystem") : base(_interface_name) {
		dwDesiredAccess = GENERIC_READ;
		dwShareMode = FILE_SHARE_READ;
		dwCreationDistribution = OPEN_EXISTING;
		dwFlagsAndAttributes = FILE_ATTRIBUTE_NORMAL |
		                       FILE_FLAG_SEQUENTIAL_SCAN;
		lpFileName = _file_name;
		Size = (uint)Marshal.SizeOf(typeof(DAFILEDESC));
		hFindFirst = INVALID_HANDLE_VALUE;
	}
};
