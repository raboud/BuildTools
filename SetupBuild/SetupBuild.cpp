// SetupBuild.cpp : Defines the entry point for the application.
//

#include "StdAfx.h"
#include "SetupBuild.h"
#include "VersionInfo.h"
#include "getopt.h"

DWORD   GetConfigFilePath(CString& filePath);
DWORD   LoadConfig(LPCTSTR szFileName, CString& configData);
CString GetStringProperties(LPCTSTR szProperty, CString& data);
DWORD   AddResource(HANDLE hResource, LPCTSTR szResType, int resId, LPCTSTR szPath);
DWORD   AddResourceHelper(HANDLE hResource, LPCTSTR szResType, int resId, CString const & path);
DWORD   AddIconToResource(HANDLE hResource, int resId, LPBYTE pFileIcon, DWORD dwFileIconSize);
DWORD   RemoveSetupIcons(HMODULE hModule, HANDLE hResource);
void    help(void);

#pragma pack( push )
#pragma pack( 2 )
typedef struct
{
   BYTE   bWidth;               // Width, in pixels, of the image
   BYTE   bHeight;              // Height, in pixels, of the image
   BYTE   bColorCount;          // Number of colors in image (0 if >=8bpp)
   BYTE   bReserved;            // Reserved
   WORD   wPlanes;              // Color Planes
   WORD   wBitCount;            // Bits per pixel
   DWORD  dwBytesInRes;         // how many bytes in this resource?
   WORD   nID;                  // the ID
} GRPICONDIRENTRY, *LPGRPICONDIRENTRY;
#pragma pack( pop )


// #pragmas are used here to insure that the structure's
// packing in memory matches the packing of the EXE or DLL.
#pragma pack( push )
#pragma pack( 2 )
typedef struct 
{
   WORD            idReserved;   // Reserved (must be 0)
   WORD            idType;       // Resource type (1 for icons)
   WORD            idCount;      // How many images?
//   GRPICONDIRENTRY idEntries[];  // The entries for each image
} GRPICONDIR, *LPGRPICONDIR;
#pragma pack( pop )

typedef struct
{
    BYTE        bWidth;          // Width, in pixels, of the image
    BYTE        bHeight;         // Height, in pixels, of the image
    BYTE        bColorCount;     // Number of colors in image (0 if >=8bpp)
    BYTE        bReserved;       // Reserved ( must be 0)
    WORD        wPlanes;         // Color Planes
    WORD        wBitCount;       // Bits per pixel
    DWORD       dwBytesInRes;    // How many bytes in this resource?
    DWORD       dwImageOffset;   // Where in the file is this image?
} ICONDIRENTRY, *LPICONDIRENTRY;

typedef struct
{
    WORD           idReserved;   // Reserved (must be 0)
    WORD           idType;       // Resource Type (1 for icons)
    WORD           idCount;      // How many images?
    //ICONDIRENTRY   idEntries[];  // An entry for each image (idCount of 'em)
} ICONDIR, *LPICONDIR;


static bool verbose = false;

int _tmain(int argc, LPCTSTR* argv)
{
	/* NOTES:
	 *   1) Read configuration from file before parsing command-line
	 *      options so that the command-line options take precedence.
	 *   2) ...
	 */
    if (verbose) _ftprintf(stdout, _T("Setup Build Begin"));

	CString filePath, configData;
	GetConfigFilePath(filePath);

	CString targetPath;
	CString sourcePath;
	CString configPath;
	CString imagePath;

	if (verbose) _ftprintf(stdout, _T("Loading config file %s\r\n"), (LPCTSTR) filePath);
	DWORD retVal = LoadConfig(filePath, configData);
	if (NO_ERROR == retVal)
	{
		targetPath = GetStringProperties(_T("Setup"), configData);
		sourcePath = GetStringProperties(_T("Msi"), configData);
		configPath = GetStringProperties(_T("Properties"), configData);
		imagePath  = GetStringProperties(_T("Icon"), configData);
	}
	
	/* check arguments */
	while (true)
	{
		int c = getopt(argc, argv, _T("-hdo:i:p:s:"));
		if (c == -1) break;

		switch (c)
		{
			case 'h': 
				help(); 
				exit(0);

			case 'd': 
				verbose = true; 
				break;

			case 'i': 
				imagePath  = optarg; 
				break;

			case 'o': 
				targetPath = optarg; 
				break;

			case 'p': 
				configPath = optarg; 
				break;

			case  's' : 
				sourcePath = optarg; 
				break;
		}
	}

	HANDLE hResource = BeginUpdateResource(targetPath, FALSE);
	if (NULL == hResource) 
	{
		retVal = GetLastError();
		_ftprintf(stderr, _T("Error opening prototypical setup file.  %s\r\n"), (LPCTSTR)targetPath);
		return retVal;
	}

	if (sourcePath.IsEmpty())
	{
		_ftprintf(stderr, _T("Missing MSI file.\r\n"));
		return ERROR_BAD_ARGUMENTS;
	}

	retVal = AddResourceHelper(hResource, _T("FILES"), 1, sourcePath);
	if (NO_ERROR != retVal)
		return retVal;

	retVal = AddResourceHelper(hResource, _T("FILES"), 2, configPath);
	if (NO_ERROR != retVal)
		return retVal;

	retVal = AddResourceHelper(hResource, RT_GROUP_ICON, 1, imagePath);
	if (NO_ERROR != retVal)
		return retVal;

	retVal = AddVersionResource(hResource, targetPath, sourcePath);
	if (NO_ERROR != retVal)
		return retVal;

	EndUpdateResource(hResource, FALSE);

	if (verbose) _ftprintf(stdout, _T("Setup Build Complete\r\n"));
	return 0;
}

DWORD GetConfigFilePath(CString& filePath)
{
	DWORD dwSize = (MAX_PATH + 1) * sizeof(TCHAR);
	LPTSTR buffer = filePath.GetBuffer(dwSize);
	GetCurrentDirectory(dwSize, buffer);
//	GetModuleFileName(NULL, buffer, dwSize);
//	PathRemoveFileSpec(buffer);
//	SetCurrentDirectory(buffer);
	PathAppend(buffer, _T("SetupBuild.config"));
	filePath.ReleaseBuffer();
	return NO_ERROR;
}

DWORD LoadConfig(LPCTSTR szFileName, CString& configData)
{
	HANDLE hFile = CreateFile(szFileName, GENERIC_READ, 0, NULL,
		OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);

	if (INVALID_HANDLE_VALUE == hFile)
		return GetLastError();

	DWORD dwHighSize = 0;
	DWORD size = GetFileSize(hFile, &dwHighSize);	
	size += sizeof(TCHAR);
	LPTSTR buffer = configData.GetBuffer(size);
	ZeroMemory(buffer, size);
	DWORD bytesRead = 0;
	if (!ReadFile(hFile, buffer, size, &bytesRead, NULL))
	{
		CloseHandle(hFile);
		configData.ReleaseBuffer();
		return GetLastError();
	}
	CloseHandle(hFile);
	configData.ReleaseBuffer();
	return NO_ERROR;
}

CString GetStringProperties(LPCTSTR szProperty, CString& data)
{
	CString begin;
	begin.Append(_T("<"));
	begin.Append(szProperty);
	begin.Append(_T(">"));

	CString end;
	end.Append(_T("</"));
	end.Append(szProperty);
	end.Append(_T(">"));

	int indexBegin = data.Find(begin);
	int indexEnd = data.Find(end);

	CString result;
	if (-1 < indexBegin && -1 < indexEnd && indexBegin < indexEnd)
	{
		indexBegin += begin.GetLength();
		result = data.Mid(indexBegin, indexEnd - indexBegin);
	}
	return result;
}

DWORD AddResource(HANDLE hResource, LPCTSTR szResType, int resId, LPCTSTR szPath)
{
	CAtlFile file;
	HRESULT hr = file.Create(szPath, GENERIC_READ, FILE_SHARE_READ, OPEN_EXISTING);
	if (FAILED(hr))
		return hr;

	CAtlFileMappingBase fileMap;
	hr = fileMap.MapFile(file);
	if (FAILED(hr))
	{
		file.Close();
		return hr;
	}

	DWORD dwError = NO_ERROR;	
	LPBYTE pData = (LPBYTE)fileMap.GetData();
	DWORD  dwSize = (DWORD)fileMap.GetMappingSize();
	if (RT_GROUP_ICON == szResType)
	{
		// Icons need to be added differently
		dwError = AddIconToResource(hResource, resId, pData, dwSize);
	}
	else
	{
		// Remove the existing resource
		WORD wLanguage = MAKELANGID(LANG_ENGLISH, SUBLANG_ENGLISH_US);
		UpdateResource(hResource, szResType, MAKEINTRESOURCE(resId), wLanguage, NULL, 0);

		// Add the resource back
		if (!UpdateResource(hResource, szResType, MAKEINTRESOURCE(resId), wLanguage, pData, dwSize))
			dwError = GetLastError();
	}
	fileMap.Unmap();
	file.Close();
	return dwError;
}

DWORD AddResourceHelper(HANDLE hResource, LPCTSTR szResType, int resId, CString const & path)
{
	DWORD dwError = NO_ERROR;
	if (path.IsEmpty())
	{
		dwError = ERROR_BAD_ARGUMENTS;
		_ftprintf(stderr, _T("Missing path to resource.\r\n"));
	}
	else
	{
		if (verbose) _ftprintf(stdout, _T("Adding %s to exe\r\n"), (LPCTSTR)path);
		dwError = AddResource(hResource, szResType, resId, path);
	}

	if (NO_ERROR != dwError)
	{
		EndUpdateResource(hResource, TRUE);
		_ftprintf(stderr, _T("Error adding %s to exe.  %d\r\n"), (LPCTSTR)path, dwError);
	}
	return dwError;
}

BOOL CALLBACK EnumResNameProc(HMODULE hModule, LPCTSTR lpszType, LPTSTR lpszName, LONG_PTR lParam)
{
	HANDLE hResource = (HANDLE)lParam;
	if (RT_GROUP_ICON != lpszType && RT_ICON != lpszType)
		return TRUE; // Continue enumeration

	BOOL retVal = UpdateResource(hResource, lpszType, lpszName, MAKELANGID(LANG_ENGLISH, SUBLANG_ENGLISH_US), NULL, 0);
	return TRUE; // Continue enumeration
}

DWORD RemoveSetupIcons(HMODULE hModule, HANDLE hResource)
{
	if (!EnumResourceNames(hModule, RT_ICON, EnumResNameProc, (LONG_PTR) hResource))
		return GetLastError();

	if (!EnumResourceNames(hModule, RT_GROUP_ICON, EnumResNameProc, (LONG_PTR) hResource))
		return GetLastError();

	return NO_ERROR;	
}


DWORD AddIconToResource(HANDLE hResource, int resId, LPBYTE pFileIcon, DWORD dwFileIconSize)
{
	DWORD dwError = NO_ERROR;
	LPICONDIR pFileIconDir = (LPICONDIR)pFileIcon;
	if (1 != pFileIconDir->idType)
		return ERROR_INVALID_PARAMETER;	
	
	CLocalHeap heap;
	int nGroupIconSize = sizeof(GRPICONDIR) + (pFileIconDir->idCount * sizeof(GRPICONDIRENTRY));	
	LPBYTE pGroupIcon = (LPBYTE)heap.Allocate(nGroupIconSize);

	LPGRPICONDIR pGroupIconDir = (LPGRPICONDIR)(pGroupIcon);
	pGroupIconDir->idReserved  = pFileIconDir->idReserved;
	pGroupIconDir->idType      = pFileIconDir->idType;
	pGroupIconDir->idCount     = pFileIconDir->idCount;
	
	LPICONDIRENTRY pIconDirEntry = (LPICONDIRENTRY)(pFileIcon + sizeof(ICONDIR));
	LPGRPICONDIRENTRY pGroupIconDirEntry = (LPGRPICONDIRENTRY)(pGroupIcon + sizeof(GRPICONDIR));
	
	int iconResID = resId;
	WORD wLanguage = MAKELANGID(LANG_ENGLISH, SUBLANG_ENGLISH_US);
	for(int i = 0; i < pFileIconDir->idCount; i++, iconResID++, pIconDirEntry++, pGroupIconDirEntry++)
	{
		pGroupIconDirEntry->bWidth        = pIconDirEntry->bWidth;
		pGroupIconDirEntry->bHeight       = pIconDirEntry->bHeight;
		pGroupIconDirEntry->bColorCount   = pIconDirEntry->bColorCount;
		pGroupIconDirEntry->bReserved     = pIconDirEntry->bReserved;
		pGroupIconDirEntry->wPlanes       = pIconDirEntry->wPlanes;
		pGroupIconDirEntry->wBitCount     = pIconDirEntry->wBitCount;
		pGroupIconDirEntry->dwBytesInRes  = pIconDirEntry->dwBytesInRes;
		pGroupIconDirEntry->nID           = iconResID;

		pIconDirEntry->dwBytesInRes;
		LPBYTE pImage = pFileIcon + pIconDirEntry->dwImageOffset;
		
		if (!UpdateResource(hResource, RT_ICON, MAKEINTRESOURCE(iconResID), wLanguage, pImage, pIconDirEntry->dwBytesInRes))
		{
			dwError = GetLastError();
			heap.Free(pGroupIcon);
			return dwError;
		}
	}

	if (!UpdateResource(hResource, RT_GROUP_ICON, MAKEINTRESOURCE(resId), wLanguage, pGroupIcon, nGroupIconSize))
	{
		dwError = GetLastError();
	}

	heap.Free(pGroupIcon);
	return NO_ERROR;
}

/*****************************************************************************
* help
*/
void help()
{
	_tprintf(
		_T("Install wrapper builder\n")
		_T("Usage: SetupBuild [OPTION] [INPUT]\n")
		_T("   INPUT           set input MSI filename\n")
		_T("   -h              help menu (this screen)\n")
		_T("   -i filename     path to icon\n")
		_T("   -p filename     path to runtime configuration properties\n")
		_T("   -o filename     set output filename\n")
		_T("   -d              prints some debugging messages\n")
	);
}
