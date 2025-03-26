using System.Runtime.InteropServices;
using DACOM;

namespace DOSFile;

using HANDLE = int;

[StructLayout(LayoutKind.Explicit)]
public class UTF_SHARING
{
	[FieldOffset(0)]public byte	read;				// count of files open with read access
	[FieldOffset(1)]public byte	write;				// count of files open with write access
	[FieldOffset(2)]public byte	readSharing;		// count of files open with read sharing on
	[FieldOffset(3)]public byte	writeSharing;		// count of files open with write sharing on
	[FieldOffset(0)] private uint combined; 
	public static UTF_SHARING operator+(UTF_SHARING local, UTF_SHARING access)
	{
		local.read += access.read;
		local.write += access.write;
		if (access.readSharing > 0)
			local.readSharing += (byte)(access.read + access.write);
		if (access.writeSharing > 0)
			local.writeSharing += (byte)(access.read + access.write);
		return local;
	}

	public static UTF_SHARING operator-(UTF_SHARING local, UTF_SHARING access)
	{
		local.read -= access.read;
		local.write -= access.write;
		if (access.readSharing != 0)
			local.readSharing -= (byte)(access.read + access.write);
		if (access.writeSharing != 0)
			local.writeSharing -= (byte)(access.read + access.write);
		return local;
	}

	bool isCompatible (UTF_SHARING access) {
		bool result = false;

		if (read > 0 && access.readSharing == 0)
			goto Done;
		if (write > 0 && access.writeSharing == 0)
			goto Done;
		if (access.read > 0  && (read+write != readSharing))
			goto Done;
		if (access.write > 0 && (read+write != writeSharing))
			goto Done;

		result = true;	// else compatible

		Done:
		return result;
	}	
	// returns TRUE if proposed access meets the current sharing environment

	public bool IsEmpty()
	{
		return combined == 0;
	}

	public void SetEmpty()
	{
		combined = 0;
	}
};



public class UTF_DIR_ENTRY()
{
	int		dwNext = 0;				// offset to next entry within directory
	private int dwName = 0;				// offset into name buffer of an ASCIIZ string

	int		dwAttributes = 0;		// directory / file / read-only, etc
	UTF_SHARING	Sharing;			// dynamic sharing state
	
	int		dwDataOffset = 0;		// file data (for file), or directory offset to child directory entry
	int		dwSpaceAllocated =0 ;	// disk space allocated for file (may be larger than used)
	int		dwSpaceUsed = 0;		// disk space actually used by the file
	int		dwUncompressedSize =0 ;	// uncompressed file size

	int		DOSCreationTime = 0;
	int		DOSLastAccessTime =0;
	int		DOSLastWriteTime=0;
};

public class BaseUTF : IFileSystem {
	public string interface_name = "";
	public string szFilename = "";
	public int dwAccess = 0; // The mode for the file
	public int dwLastError = 0;
	public IFileSystem pParent = null;
	public int iRootIndex = 0; // point where non-root begins (index of last '\\'+1)
	public int hParentFile;
	public BaseUTF pParentUTF = null;  
	public UTF_DIR_ENTRY pBaseDirEntry;       

	public const int ERROR_NOT_SUPPORTED = 50;
	public const int INVALID_HANDLE_VALUE = -1;
	public const int MAX_PATH = 260;
	
	public BaseUTF() {
		szFilename = "\\";
		hParentFile = INVALID_HANDLE_VALUE;
	}
	
	public static IFileSystem CreateBaseUTF() {
		return new DAComponent<BaseUTF>().I;
	}

	public GenResult QueryInterface(string interface_name, ref object instance) {
		throw new NotImplementedException();
	}

	public uint AddRef() {
		throw new NotImplementedException();
	}

	public uint Release() {
		throw new NotImplementedException();
	}

	public GenResult QueryOutgoingInterface(string connectionName, IDAConnectionPoint[] connection) {
		throw new NotImplementedException();
	}
	
	public virtual HANDLE openChild (DAFILEDESC lpDesc, UTF_DIR_ENTRY pEntry) {
		throw new NotImplementedException();
	}

	public static void memcpy(ref string dest, string src, int count) {
		dest = src[..count];
	}
	
	HANDLE OpenChild (DAFILEDESC lpInfo)
	{
		HANDLE handle;
	
		if (pParent is null)
		{
			dwLastError = ERROR_NOT_SUPPORTED;
			return INVALID_HANDLE_VALUE;
		}

		// short cut this whole thing if user passed in a findFirstHandle
		if (pParentUTF is not null && GETFFHANDLE(lpInfo) != INVALID_HANDLE_VALUE)
		{
			handle = pParentUTF.openChild(lpInfo, pBaseDirEntry);
		}
		else
		{
			string buffer = "";
			string lpSaved = lpInfo.lpFileName;
			memcpy(ref buffer, szFilename, iRootIndex);
			if (GetAbsolutePath(buffer+iRootIndex, lpInfo.lpFileName, MAX_PATH - iRootIndex) == 0)
			{
				dwLastError = ERROR_FILE_NOT_FOUND;
				return INVALID_HANDLE_VALUE;
			}

			if (pParentUTF is not null)
			{
				lpInfo.lpFileName = buffer+iRootIndex;
				handle = pParentUTF.openChild(lpInfo, pBaseDirEntry);
			}
			else
			{
				lpInfo.lpFileName = buffer;
				handle = pParent.OpenChild(lpInfo);
			}

			lpInfo.lpFileName = lpSaved;
		}
	
		if (handle == INVALID_HANDLE_VALUE)
			dwLastError = pParent.GetLastError();

		return handle;
	}

	int GETFFHANDLE(DAFILEDESC x) {
		return x.Size == (uint)Marshal.SizeOf(typeof(DAFILEDESC)) ? x.hFindFirst : INVALID_HANDLE_VALUE;
	}


	static bool CHECKDESCSIZE(object x) {
		if (x is DAFILEDESC desc) {
			// (uint)Marshal.SizeOf(typeof(DAComDesc))
			return desc.Size == (uint)Marshal.SizeOf(typeof(DAFILEDESC)) ||
			       desc.Size == ((uint)Marshal.SizeOf(typeof(DAFILEDESC)) - sizeof(uint));
		}

		return false;
	}

	public GenResult CreateInstance(DAComDesc? descriptor, ref object instance) {
		DAFILEDESC? lpInfo = (DAFILEDESC?)descriptor;
		GenResult result = GenResult.GR_OK;
		BaseUTF pNewSystem = null;

		if (lpInfo?.InterfaceName is null) {
			result = GenResult.GR_GENERIC;
			goto Done;
		}

		// If unsupported interface requested, fail call
		//

		if (CHECKDESCSIZE(lpInfo) == false || lpInfo.InterfaceName == interface_name) {
			result = GenResult.GR_INTERFACE_UNSUPPORTED;
			goto Done;
		}

		//
		// Can't handle this request (we already have a parent)
		//

		if (lpInfo.lpParent is not null && pParent is not null) {
			result = GenResult.GR_GENERIC;
			goto Done;
		}

		// 
		// if we are an open directory, see if we can find child inside us
		// 	

	if (hParentFile == INVALID_HANDLE_VALUE && pParent is not null)
	{
		object handle;

// 		if ((handle = OpenChild(lpInfo)) == INVALID_HANDLE_VALUE)
// 		{
// 		  //
// 		  // OpenChild() failed; system could not be created
// 		  // See if	file is really a directory
//   		  //
// 			DWORD dwAttribs;
// 			UTF_DIR_ENTRY * pNewBaseDirEntry = 0;
//
// 			if ((pNewSystem = new DAComponent<BaseUTF>) == 0)
// 			{
// 				result = GR_OUT_OF_MEMORY;
// 				goto Done;
// 			}
//
// 			if (lpInfo->lpFileName)
// 			{
// 				memcpy(pNewSystem->szFilename, szFilename, iRootIndex);
// 		 		if (GetAbsolutePath(pNewSystem->szFilename+iRootIndex, lpInfo->lpFileName, MAX_PATH - iRootIndex) == 0)
// 				{
// 					delete pNewSystem;
// 					pNewSystem = 0;
// 					result = GR_FILE_ERROR;
// 					goto Done;
// 				}
// 			}
//
// 			if (pParentUTF)
// 			{
// 				if ((pNewBaseDirEntry = pParentUTF->getDirectoryEntryForChild(pNewSystem->szFilename+iRootIndex, pBaseDirEntry, GETFFHANDLE(lpInfo))) != 0)
// 				{
// 					// verify that filename matched the findFirst handle
// 					if (GETFFHANDLE(lpInfo)!=INVALID_HANDLE_VALUE && (lpInfo->lpFileName==0 || stricmp(pParentUTF->getNameBuffer()+pNewBaseDirEntry->dwName, lpInfo->lpFileName) != 0))
// 					{
// 						delete pNewSystem;
// 						pNewSystem=0;
// 						result = GR_INVALID_PARMS;
// 						goto Done;
// 					}
// 					dwAttribs = pNewBaseDirEntry->dwAttributes;
// 				}
// 				else
// 					dwAttribs = 0xFFFFFFFF;
// 			}
// 			else
// 			{
// 				dwAttribs = pParent->GetFileAttributes(pNewSystem->szFilename);
// 			}
//
// 			if (dwAttribs == 0xFFFFFFFF || (dwAttribs&FILE_ATTRIBUTE_DIRECTORY) == 0)
// 			{
// 				delete pNewSystem;
// 				pNewSystem=0;
//
// 				result = GR_FILE_ERROR;
// 				goto Done;
// 			}
// 			// 
// 			// else it is a directory, add a "\\" to the end of the name
// 			//
// 			if ((pNewSystem->iRootIndex = strlen(pNewSystem->szFilename)) != 0)
// 				if (pNewSystem->szFilename[pNewSystem->iRootIndex-1] == UTF_SWITCH_CHAR)
// 					pNewSystem->iRootIndex--;
//
// 			pNewSystem->szFilename[pNewSystem->iRootIndex] = UTF_SWITCH_CHAR;
// 			pNewSystem->hParentFile = handle;
// 			pNewSystem->pParent = pParent;
// 			pNewSystem->dwAccess = dwAccess & lpInfo->dwDesiredAccess;
// 			// pParent->AddRef();
// 			if (pParentUTF && pNewBaseDirEntry)
// 			{
// 				pNewSystem->pBaseDirEntry = pNewBaseDirEntry;
// 				pNewSystem->pParentUTF = pParentUTF;
// 			}
// 			goto Done;
// 		}
//
// 		// else we successfully opened the child 
//
// 		{
// 			// need some other implementation
//
// 			lpInfo->lpParent = pParent;
// 			lpInfo->hParent   = handle;
// 			// pParent->AddRef();			// child file system will now reference parent directly
// 			if ((result = DACOM->CreateInstance(lpInfo, (void **) &pNewSystem)) != GR_OK)
// 			{
// 				pParent->Release();
// 				pParent->CloseHandle(handle);
// 				lpInfo->lpParent = 0;
// 				lpInfo->hParent   = 0;
// 				goto Done;
// 			}
// 			lpInfo->lpParent = 0;
// 			lpInfo->hParent   = 0;
// 			goto Done;
// 		}
	}
//
// 	// else we are not a directory
//
// 	if (lpInfo->lpParent)
// 	{
// 		if (lpInfo->lpImplementation != NULL &&
// 			strcmp(lpInfo->lpImplementation, implementation_name))
// 		{
// 			result = GR_GENERIC;
// 			goto Done;
// 		}
// 		else	// dont create a UTF without being asked
// 		if (lpInfo->lpImplementation == NULL &&
// 			lpInfo->dwCreationDistribution != OPEN_EXISTING)
// 		{
// 			result = GR_GENERIC;
// 			goto Done;
// 		}
//
// 		// implies that we don't have a parent system
// 		// 
// 		// create a new instance of UTF
// 		//
//
// 		if (lpInfo->dwDesiredAccess == GENERIC_READ &&
// 			(lpInfo->dwShareMode & ~FILE_SHARE_READ) == 0)
// 		{
// 			pNewSystem = CreateUTF();
// 		}
// 		else
// 			pNewSystem = CreateSharedUTF(lpInfo->dwShareMode);
//
// 		if (pNewSystem == 0)
// 		{
// 			result = GR_OUT_OF_MEMORY;
// 			goto Done;
// 		}
//
// 		strncpy(pNewSystem->szFilename, lpInfo->lpFileName, sizeof(szFilename));
// 		pNewSystem->dwAccess = lpInfo->dwDesiredAccess;
// 		pNewSystem->hParentFile = lpInfo->hParent;
// 		pNewSystem->pParent = lpInfo->lpParent;
//
// 		if (pNewSystem->init(lpInfo) == 0)
// 		{
// 			// prevent releasing resources we didn't take over
// 			// lpInfo->lpParent->AddRef();
// 			pNewSystem->hParentFile = INVALID_HANDLE_VALUE;
// 			pNewSystem->Release();
// 			pNewSystem = 0;
// 			result = GR_FILE_ERROR;
// 		}
// 	
// 		goto Done;	
// 	}
//
//
// 	// request to create a UTF system from nothing
// 	if (pParent == 0)
// 	{
// 		if (lpInfo->lpImplementation != NULL &&
// 			strcmp(lpInfo->lpImplementation, implementation_name))
// 		{
// 			result = GR_GENERIC;
// 			goto Done;
// 		}
// 		else	// dont create a UTF without being asked
// 		if (lpInfo->lpImplementation == NULL &&
// 			lpInfo->dwCreationDistribution != OPEN_EXISTING)
// 		{
// 			result = GR_GENERIC;
// 			goto Done;
// 		}
//
// 		LPCTSTR lpSaved = lpInfo->lpImplementation;
// 		DWORD dwSavedAccess = lpInfo->dwDesiredAccess;
//
// 		if (lpInfo->dwDesiredAccess == GENERIC_READ &&
// 			(lpInfo->dwShareMode & ~FILE_SHARE_READ) == 0)
// 		{
// 			pNewSystem = CreateUTF();
// 		}
// 		else
// 			pNewSystem = CreateSharedUTF(lpInfo->dwShareMode);
//
// 		if (pNewSystem == 0)
// 		{
// 			result = GR_OUT_OF_MEMORY;
// 			goto Done;
// 		}
//
// 		lpInfo->lpImplementation = "DOS";
// 		lpInfo->dwDesiredAccess |= GENERIC_READ;
// 		if ((result = DACOM->CreateInstance(lpInfo, (void **) &pNewSystem->pParent)) != GR_OK)
// 		{
// 			delete pNewSystem;
// 			pNewSystem = 0;
// 			result = GR_FILE_ERROR;
// 			lpInfo->lpImplementation = lpSaved;
// 			lpInfo->dwDesiredAccess = dwSavedAccess;
// 			goto Done;
// 		}
// 		pNewSystem->hParentFile = 0;
// 		lpInfo->lpImplementation = lpSaved;
// 		lpInfo->dwDesiredAccess = dwSavedAccess;
// 		strncpy(pNewSystem->szFilename, lpInfo->lpFileName, sizeof(szFilename));
// 		pNewSystem->dwAccess = lpInfo->dwDesiredAccess;
//
// 		if (pNewSystem->init(lpInfo) == 0)
// 		{
// 			pNewSystem->Release();
// 			pNewSystem = 0;
// 			result = GR_FILE_ERROR;
// 		}
// 		goto Done;	
// 	}
// 	else
// 	{
// 		// attempt to create the child instance from within
//
// 		HANDLE			handle;
// 		//
// 		// Associate file handle with new file system
// 		//
//    
// 		handle = OpenChild(lpInfo);
//
// 		if (handle == INVALID_HANDLE_VALUE)
// 		{
// 		  //
// 		  // CreateFile() failed; system could not be created
// 		  // See if	file is really a directory
//   		  //
// 			DWORD dwAttribs;
// 			UTF_DIR_ENTRY * pNewBaseDirEntry = 0;
//
// 			if ((pNewSystem = new DAComponent<BaseUTF>) == 0)
// 			{
// 				result = GR_OUT_OF_MEMORY;
// 				goto Done;
// 			}
//
// 			if (lpInfo->lpFileName)
// 			{
// 				if (GetAbsolutePath(pNewSystem->szFilename, lpInfo->lpFileName, MAX_PATH) == 0)
// 				{
// 					delete pNewSystem;
// 					pNewSystem = 0;
// 					result = GR_FILE_ERROR;
// 					goto Done;
// 				}
// 			}
//
// 			if ((pNewBaseDirEntry = getDirectoryEntryForChild(pNewSystem->szFilename, 0, GETFFHANDLE(lpInfo))) != 0)
// 			{
// 				// verify that filename matched the findFirst handle
// 				if (GETFFHANDLE(lpInfo)!=INVALID_HANDLE_VALUE && (lpInfo->lpFileName==0 || stricmp(getNameBuffer()+pNewBaseDirEntry->dwName, lpInfo->lpFileName) != 0))
// 				{
// 					delete pNewSystem;
// 					pNewSystem=0;
// 					result = GR_INVALID_PARMS;
// 					goto Done;
// 				}
// 				dwAttribs = pNewBaseDirEntry->dwAttributes;
// 			}
// 			else
// 				dwAttribs = GetFileAttributes(pNewSystem->szFilename);
//
// 			if (dwAttribs == 0xFFFFFFFF || (dwAttribs&FILE_ATTRIBUTE_DIRECTORY) == 0)
// 			{
// 				delete pNewSystem;
// 				pNewSystem = 0;
// 				result = GR_FILE_ERROR;
// 				goto Done;
// 			}
// 		
// 			// 
// 			// else it is a directory
// 			//
//
// 			if ((pNewSystem->iRootIndex = strlen(pNewSystem->szFilename)) != 0)
// 				if (pNewSystem->szFilename[pNewSystem->iRootIndex-1] == UTF_SWITCH_CHAR)
// 					pNewSystem->iRootIndex--;
//
// 			pNewSystem->szFilename[pNewSystem->iRootIndex] = UTF_SWITCH_CHAR;
// 			pNewSystem->hParentFile = handle;
// 			pNewSystem->pParent = this;
// 			pNewSystem->dwAccess = dwAccess & lpInfo->dwDesiredAccess;
// 			AddRef();
// 			// only if we are a read-only UTF file will the following work
// 			if ((pNewSystem->pBaseDirEntry = pNewBaseDirEntry) != 0)
// 				pNewSystem->pParentUTF = this;
// 			goto Done;
// 		}
//
// 		// else child file was opened
//
// 		{
// 			// need another implementation
//
// 			lpInfo->lpParent = this;
// 			lpInfo->hParent   = handle;
// 			AddRef();			// child file system will now reference us 
// 			if ((result = DACOM->CreateInstance(lpInfo, (void **) &pNewSystem)) != GR_OK)
// 			{
// 				Release();
// 				CloseHandle(handle);
// 				lpInfo->lpParent = 0;
// 				lpInfo->hParent   = 0;
// 				goto Done;
// 			}
// 			lpInfo->lpParent = 0;
// 			lpInfo->hParent   = 0;
//
// 			goto Done;
// 		}
// 	}
//
		Done:
		instance = pNewSystem;

		return result;
	}
}
