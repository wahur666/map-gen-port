using System.Runtime.InteropServices;
using System.Text;

namespace DACOM;

class Program {
	
	public static IntPtr StringToCString(string str)
	{
		// Convert a C# string to a C-style string (char*)
		return Marshal.StringToHGlobalAnsi(str);
	}

	public static string CStringToString(IntPtr cString)
	{
		// Convert C-style string (char*) back to C# string
		return Marshal.PtrToStringAnsi(cString);
	}
	
	
	static void Main(string[] args) {
		DAComDesc d = new DAComDesc("my intercfae");
		var dacom = DACOManager.DACOM_Acquire();
		Console.WriteLine();
	}
}
