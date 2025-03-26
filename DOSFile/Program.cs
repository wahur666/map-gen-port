using System.Text;
using DACOM;

namespace DOSFile;

public enum GENBASE_TYPE {
	GBT_FONT = 1,
	GBT_BUTTON,
	GBT_STATIC,
	GBT_EDIT,
	GBT_LISTBOX,
	GBT_DROPDOWN,
	GBT_VFXSHAPE,
	GBT_HOTBUTTON,
	GBT_SCROLLBAR,
	GBT_HOTSTATIC,
	GBT_SHIPSILBUTTON,
	GBT_COMBOBOX,
	GBT_SLIDER,
	GBT_TABCONTROL,
	GBT_ICON,
	GBT_QUEUECONTROL,
	GBT_ANIMATE,
	GBT_PROGRESS_STATIC,
	GBT_DIPLOMACYBUTTON
}

public class GENBASE_DATA // every non-game type must inherit from this
{
	public GENBASE_TYPE type = default;
}

public class ARCHDATATYPE {
	public string name = "";
	public GENBASE_DATA objData = new GENBASE_DATA();
	public uint dataSize = 0; // size of data chunk in bytes
}

//--------------------------------------------------------------------------//
//
public class ARCHDATA {
	public uint numArchetypes = 0;
	public ARCHDATATYPE[] type = [];
}

class Program {
	static void get_total_bytes(IFileSystem file, ref uint dataSize, ref uint numFiles) {
		// WIN32_FIND_DATA data;
		// HANDLE handle;
		//
		// if ((handle = file->FindFirstFile("*.*", &data)) != INVALID_HANDLE_VALUE)
		// {
		// 	do
		// 	{
		// 		// make sure this not a silly "." entry
		// 		if (data.cFileName[0] != '.' || strchr(data.cFileName, '\\') != 0)
		// 		{
		// 			if (data.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
		// 			{
		// 				if (strcmp(data.cFileName, "Parsed Files"))
		// 				{
		// 					// traverse subdirectory
		// 					if (file->SetCurrentDirectory(data.cFileName))
		// 					{
		// 						get_total_bytes(file, dataSize, numFiles);
		// 						file->SetCurrentDirectory("..");	// restore current directory
		// 					}
		// 				}
		// 			}
		// 			else
		// 			{
		// 				dataSize += data.nFileSizeLow;
		// 				numFiles++;
		// 			}
		// 		}
		//
		// 	} while (file->FindNextFile(handle, &data));
		//
		// 	file->FindClose(handle);
		// }
	}

	static uint calcCheckSum(string buffer, uint bufferSize, uint checkSum) {
		var b = Encoding.ASCII.GetBytes(buffer);
		for (int i = 0; i < bufferSize; i++) {
			checkSum += b[i];
		}

		return checkSum;
	}

	static void load_bytes(IFileSystem file, ARCHDATA archData, ref uint checkSum) {
		// WIN32_FIND_DATA data;
		// HANDLE handle;
		// DAFILEDESC fdesc=data.cFileName;
		//
		// if ((handle = file->FindFirstFile("*.*", &data)) != INVALID_HANDLE_VALUE)
		// {
		// 	do
		// 	{
		// 		// make sure this not a silly "." entry
		// 		if (data.cFileName[0] != '.' || strchr(data.cFileName, '\\') != 0)
		// 		{
		// 			if (data.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
		// 			{
		// 				// traverse subdirectory
		// 				if (strcmp(data.cFileName, "Parsed Files"))
		// 				{
		// 					if (file->SetCurrentDirectory(data.cFileName))
		// 					{
		// 						load_bytes(file, archData, checkSum);
		// 						file->SetCurrentDirectory("..");	// restore current directory
		// 					}
		// 				}
		// 			}
		// 			else
		// 			{
		// 				HANDLE hFile;
		//
		// 				if ((hFile = file->OpenChild(&fdesc)) != INVALID_HANDLE_VALUE)
		// 				{
		// 					U32 i = archData->numArchetypes++;
		// 					DWORD dwRead;
		//
		// 					if (i != 0)
		// 						archData->type[i].objData = (GENBASE_DATA *) (((U8 *)archData->type[i-1].objData) + archData->type[i-1].dataSize);
		//
		// 					strncpy(archData->type[i].name, data.cFileName, sizeof(archData->type[i].name)-1);
		// 					archData->type[i].dataSize = file->GetFileSize(hFile);
		// 					file->ReadFile(hFile, archData->type[i].objData, archData->type[i].dataSize, &dwRead, 0);
		// 					file->CloseHandle(hFile);
		// 					checkSum = calcCheckSum((U8 *)archData->type[i].objData, dwRead, checkSum);
		// 				}
		// 			}
		// 		}
		//
		// 	} while (file->FindNextFile(handle, &data));
		//
		// 	file->FindClose(handle);
		// }
	}


	static bool loadTypesData() {
		var DACOM = DACOManager.DACOM_Acquire();

		ARCHDATA archData = new ARCHDATA();
		// IDocument pDoc;
		IFileSystem pFile;
		DAFILEDESC fdesc = new("GenData.db");
		uint dataSize = 0;
		uint numFiles = 0;
		uint checkSum = 0;
		bool result = false;
		IFileSystem file = null, pMemFile;
		object hMapping = 0;
		object pImage = 0;
		uint size = 0;

		fdesc.lpImplementation = "DOS";
		var o = (object)file;
		if (DACOM.CreateInstance(fdesc, ref o) != GenResult.GR_OK)
		{
			fdesc.lpFileName = @".\DB\GenData.db";
			if (DACOM.CreateInstance(fdesc, ref o) != GenResult.GR_OK)
			{
				Console.WriteLine($"Could not access '{fdesc.lpFileName}'");
				goto Done;
			}
		}

		Console.WriteLine();
// 	hMapping = file->CreateFileMapping();
// 	pImage = file->MapViewOfFile(hMapping);
// 	size = file->GetFileSize();
//
// 	if (CQFLAGS.bNoGDI==0)
// 	{
// 		//
// 		// create a memory file
// 		//
// 		MEMFILEDESC mdesc = fdesc.lpFileName;
// 		mdesc.lpBuffer = pImage;
// 		mdesc.dwBufferSize = size;
// 		mdesc.dwFlags = 0;
// 		mdesc.dwDesiredAccess |= GENERIC_WRITE;
//
// 		if (CreateUTFMemoryFile(mdesc, pFile) != GR_OK)
// 			printf("Could not create memory file");
//
// 		//
// 		// create a document
// 		//
// 		DOCDESC ddesc = fdesc.lpFileName;
//
// 		ddesc.lpImplementation = "DOS";
// 		ddesc.dwShareMode = 0;		 // no sharing
// 		ddesc.dwCreationDistribution = CREATE_ALWAYS;
//
// 		ddesc.lpParent = pFile;
// 		// pFile->AddRef();
//
// 		if (DACOM->CreateInstance(&ddesc, pDoc) != GR_OK)
// 			printf("Could not create document");
//
// 		pMemFile = pFile;
// 	}
// 	else
// 	{
// 		//
// 		// create a memory file
// 		//
// 		MEMFILEDESC mdesc = fdesc.lpFileName;
// 		mdesc.lpBuffer = pImage;
// 		mdesc.dwBufferSize = size;
// 		mdesc.dwFlags = CMF_DONT_COPY_MEMORY;
//
// 		if (CreateUTFMemoryFile(mdesc, pMemFile) != GR_OK)
// 			printf("Could not create memory file");
// 	}
//
// 	get_total_bytes(pMemFile, dataSize, numFiles);
//
// 	archData = (ARCHDATA *) calloc(sizeof(ARCHDATA)+(sizeof(ARCHDATATYPE)*numFiles)+dataSize, 1);
//
// 	archData->type[0].objData = (GENBASE_DATA *) (((U8 *)archData) + sizeof(ARCHDATA)+(sizeof(ARCHDATATYPE)*numFiles)); // mark beginning of data
// 	load_bytes(pMemFile, archData, checkSum);
//
// 	pMemFile.free();
// 	file->UnmapViewOfFile(pImage);
// 	file->CloseHandle(hMapping);
//
		result = true;
	Done:
		return result;
	}


	static void Main(string[] args) {
		var checksum = calcCheckSum("Helloz", 6, 0);
		Console.WriteLine(checksum);
		DAComDesc d = new DAComDesc("my intercfae");
		var dacom = DACOManager.DACOM_Acquire();
		DOSFileSystem dosFileSystem = new DOSFileSystem();
		dosFileSystem.InitializeLibrary();
		loadTypesData();
		Console.WriteLine();
	}
}
