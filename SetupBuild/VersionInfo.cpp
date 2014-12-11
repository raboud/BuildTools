#include "StdAfx.h"
#include "VersionInfo.h"
#include "Util.h"

struct VS_VERSIONINFO { 
  WORD  wLength; 
  WORD  wValueLength; 
  WORD  wType; 
  //WCHAR szKey[];
  //WORD  Padding1[]; 
  //VS_FIXEDFILEINFO Value; 
  //WORD  Padding2[]; 
  //WORD  Children[]; 
};

struct StringFileInfo { 
  WORD        wLength; 
  WORD        wValueLength; 
  WORD        wType; 
  //WCHAR       szKey[]; 
  //WORD        Padding[]; 
  //StringTable Children[]; 
};

struct StringTable { 
  WORD   wLength; 
  WORD   wValueLength; 
  WORD   wType; 
  //WCHAR  szKey[]; 
  //WORD   Padding[]; 
  //String Children[]; 
};

struct String
{ 
  WORD   wLength; 
  WORD   wValueLength; 
  WORD   wType; 
  //WCHAR  szKey[]; 
  //WORD   Padding[]; 
  //WORD   Value[]; 
};

struct VarFileInfo
{ 
  WORD  wLength; 
  WORD  wValueLength; 
  WORD  wType; 
  //WCHAR szKey[]; 
  //WORD  Padding[]; 
  //Var   Children[]; 
};

struct Var
{ 
  WORD  wLength; 
  WORD  wValueLength; 
  WORD  wType; 
  //WCHAR szKey[]; 
  //WORD  Padding[]; 
  //DWORD Value[]; 
};

INT AddPadding(LPBYTE& buffer, INT& size);
INT BuildVarStructure(LPBYTE& buffer, LPCWSTR szKey, DWORD dwVal);
INT BuildStringStructure(LPBYTE& buffer, LPCWSTR szKey, LPCWSTR szVal);
INT BuildVersionInfo(LPBYTE buffer, LPCWSTR szProductName, LPCWSTR szProductVersion, LPCWSTR szSetupName);

/*
Test code used to read version information

void Test(LPBYTE& data)
{
	String* pString = (String*) data;
	data += sizeof(String);
	PWSTR szkey = (PWSTR)data;	
	INT count = sizeof(WCHAR) * (lstrlenW(szkey) + 1);
	data += count;
	while(0 == *data)
	{
		data++;
		count++;
	}

	PWSTR szVal = (PWSTR)data;	
	count = sizeof(WCHAR) * (lstrlenW(szVal) + 1);
	data += count;
	while(0 == *data)
	{
		data++;
		count++;
	}
}

void ParseVersionInfo(LPBYTE data)
{
	VS_VERSIONINFO* pver = (VS_VERSIONINFO*) data;
	
	data += sizeof(VS_VERSIONINFO);
	PWSTR szkey = (PWSTR)data;
	
	int count = sizeof(WCHAR) * lstrlenW(szkey);
	data += count;
	while(0 == *data)
	{
		data++;
		count++;
	}
	
	VS_FIXEDFILEINFO* pFileInfo = (VS_FIXEDFILEINFO*)data;
	data += sizeof(VS_FIXEDFILEINFO);

	StringFileInfo* pStringFile = (StringFileInfo*) data;
	data += sizeof(StringFileInfo);
	szkey = (PWSTR)data;
	
	count = sizeof(WCHAR) * lstrlenW(szkey);
	data += count;
	while(0 == *data)
	{
		data++;
		count++;
	}

	StringTable* pStringTable = (StringTable*) data;
	data += sizeof(StringTable);
	szkey = (PWSTR)data;	
	count = sizeof(WCHAR) * lstrlenW(szkey);
	data += count;
	while(0 == *data)
	{
		data++;
		count++;
	}

	Test(data);
	Test(data);
	Test(data);
	Test(data);
	Test(data);
	Test(data);
	Test(data);
	Test(data);

	VarFileInfo* pVarFileInfo = (VarFileInfo*) data;
	data += sizeof(VarFileInfo);
	szkey = (PWSTR)data;	
	count = sizeof(WCHAR) * lstrlenW(szkey);
	data += count;
	while(0 == *data)
	{
		data++;
		count++;
	}

	Var* pVar = (Var*) data;
	data += sizeof(Var);
	szkey = (PWSTR)data;	
	count = sizeof(WCHAR) * lstrlenW(szkey);
	data += count;
	while(0 == *data)
	{
		data++;
		count++;
	}
	
	PDWORD pdwVal = (PDWORD)data;
}
*/

INT AddPadding(LPBYTE& buffer, INT& size)
{
	int padding = ((size + 3) & -4) - size;
	for (int i = 0; i < padding; i++, size++)
	{
		if (NULL != buffer)
		{
			*buffer = NULL;
			buffer++;
		}
	}
	return padding;
}

INT BuildVarStructure(LPBYTE& buffer, LPCWSTR szKey, DWORD dwVal)
{
	Var* pVar = (Var*)buffer;
	INT size = sizeof(Var);

	INT lenKey = lstrlenW(szKey) + 1;
	INT sizKey = sizeof(WCHAR) * lenKey;
	INT sizVal = sizeof(DWORD);

	if (NULL != buffer)
	{
		buffer += size;		
		CopyMemory(buffer, szKey, sizKey);
		buffer += sizKey;
	}

	size += sizKey;
	AddPadding(buffer, size);
	size += sizVal;

	if (NULL != buffer)
	{		
		CopyMemory(buffer, &dwVal, sizVal);
		buffer += sizVal;
	}

	if (NULL != pVar)
	{
		pVar->wType = 0;
		pVar->wLength = size;		
		pVar->wValueLength = sizVal;
	}
	return size;
}

INT BuildStringStructure(LPBYTE& buffer, LPCWSTR szKey, LPCWSTR szVal)
{
	String* pString = (String*)buffer;
	INT size = sizeof(String);

	INT lenKey = lstrlenW(szKey) + 1;
	INT lenVal = lstrlenW(szVal) + 1;

	INT sizKey = sizeof(WCHAR) * lenKey;
	INT sizVal = sizeof(WCHAR) * lenVal;

	if (NULL != buffer)
	{
		buffer += size;		
		CopyMemory(buffer, szKey, sizKey);
		buffer += sizKey;
	}

	size += sizKey;
	AddPadding(buffer, size);
	size += sizVal;

	if (NULL != buffer)
	{		
		CopyMemory(buffer, szVal, sizVal);
		buffer += sizVal;
	}

	if (NULL != pString)
	{
		pString->wType = 1;
		pString->wLength = size;		
		pString->wValueLength = lenVal;
	}
	return size;
}

INT BuildVersionInfo(LPBYTE buffer, LPCWSTR szProductName, LPCWSTR szProductVersion, LPCWSTR szSetupName)
{	
	// First extract the version parts
	int nMajor = 0;
	int nMinor = 0;
	int nBuild = 0;
	int nRevision = 0;
	
	INT iStart = 0;
	PWCHAR token = L".";
	CStringW strVer(szProductVersion);
	CStringW strPart = strVer.Tokenize(token, iStart);
	if (!strPart.IsEmpty())
	{
		nMajor = _tstoi(strPart);
		strPart = strVer.Tokenize(token, iStart);
	}
	
	if (!strPart.IsEmpty())
	{
		nMinor = _tstoi(strPart);
		strPart = strVer.Tokenize(token, iStart);
	}
	
	if (!strPart.IsEmpty())
	{
		nBuild = _tstoi(strPart);
		strPart = strVer.Tokenize(token, iStart);
	}

	if (!strPart.IsEmpty())
	{
		nRevision = _tstoi(strPart);
	}

	// If the version number in the MSI is only three parts, then build a 4 part version string.
	strVer.Format(L"%d.%d.%d.%d", nMajor, nMinor, nBuild, nRevision);

	VS_VERSIONINFO* pVerInfo = (VS_VERSIONINFO*)buffer;
	INT size = sizeof(VS_VERSIONINFO);

	PWCHAR szKey = L"VS_VERSION_INFO";
	INT lenKey = lstrlenW(szKey) + 1;
	INT sizKey = sizeof(WCHAR) * lenKey;

	if (NULL != buffer)
	{
		buffer += size;		
		CopyMemory(buffer, szKey, sizKey);
		buffer += sizKey;
	}

	size += sizKey;
	AddPadding(buffer, size);

	INT sizFixFileInfo = sizeof(VS_FIXEDFILEINFO);
	size += sizFixFileInfo;
	if (NULL != buffer)
	{
		VS_FIXEDFILEINFO* pFixedFileInfo = (VS_FIXEDFILEINFO*) buffer;
		pFixedFileInfo->dwSignature = 0xfeef04bd;			/* e.g. 0xfeef04bd */
		pFixedFileInfo->dwStrucVersion = 0x00010000;		/* e.g. 0x00000042 = "0.42" */
		pFixedFileInfo->dwFileVersionMS = MAKELONG(nMinor, nMajor);			/* e.g. 0x00030075 = "3.75" */
		pFixedFileInfo->dwFileVersionLS = MAKELONG(nRevision, nBuild);		/* e.g. 0x00000031 = "0.31" */
		pFixedFileInfo->dwProductVersionMS = MAKELONG(nMajor, nMinor);		/* e.g. 0x00030010 = "3.10" */
		pFixedFileInfo->dwProductVersionLS = MAKELONG(nBuild, nRevision);	/* e.g. 0x00000031 = "0.31" */
		pFixedFileInfo->dwFileFlagsMask = 0x17;     /* = 0x3F for version "0.42" */
		pFixedFileInfo->dwFileFlags = 0;            /* e.g. VFF_DEBUG | VFF_PRERELEASE */
		pFixedFileInfo->dwFileOS = VOS__WINDOWS32;  /* e.g. VOS_DOS_WINDOWS16 */
		pFixedFileInfo->dwFileType = VFT_APP;       /* e.g. VFT_DRIVER */
		pFixedFileInfo->dwFileSubtype = VFT2_UNKNOWN;	/* e.g. VFT2_DRV_KEYBOARD */
		pFixedFileInfo->dwFileDateMS = 0;           /* e.g. 0 */
		pFixedFileInfo->dwFileDateLS = 0;           /* e.g. 0 */
		buffer += sizFixFileInfo;		
	}

	AddPadding(buffer, size);

	StringFileInfo* pStringFile = (StringFileInfo*) buffer;
	INT sizeStringFileInfo = sizeof(StringFileInfo);
	
	szKey = L"StringFileInfo";
	lenKey = lstrlenW(szKey) + 1;
	sizKey = sizeof(WCHAR) * lenKey;

	if (NULL != buffer)
	{
		buffer += sizeStringFileInfo;		
		CopyMemory(buffer, szKey, sizKey);
		buffer += sizKey;
	}

	sizeStringFileInfo += sizKey;
	AddPadding(buffer, sizeStringFileInfo);

	StringTable* pStringTable = (StringTable*) buffer;
	INT sizeStringTable = sizeof(StringTable);

	szKey = L"040904b0";
	lenKey = lstrlenW(szKey) + 1;
	sizKey = sizeof(WCHAR) * lenKey;

	if (NULL != buffer)
	{
		buffer += sizeStringTable;		
		CopyMemory(buffer, szKey, sizKey);
		buffer += sizKey;
	}

	sizeStringTable += sizKey;
	AddPadding(buffer, sizeStringTable);

	CStringW fileDescription;
	fileDescription.Format(L"%s product installation", szProductName);

	sizeStringTable += BuildStringStructure(buffer, L"CompanyName", L"R & R Engineering, LLC");
	AddPadding(buffer, sizeStringTable);
	sizeStringTable += BuildStringStructure(buffer, L"FileDescription", fileDescription);
	AddPadding(buffer, sizeStringTable);
	sizeStringTable += BuildStringStructure(buffer, L"FileVersion", strVer);
	AddPadding(buffer, sizeStringTable);
	sizeStringTable += BuildStringStructure(buffer, L"InternalName", L"Setup");
	AddPadding(buffer, sizeStringTable);
	sizeStringTable += BuildStringStructure(buffer, L"LegalCopyright", L"Copyright (c) 2012 R & R Engineering, LLC");
	AddPadding(buffer, sizeStringTable);
	sizeStringTable += BuildStringStructure(buffer, L"OriginalFilename", szSetupName);
	AddPadding(buffer, sizeStringTable);
	sizeStringTable += BuildStringStructure(buffer, L"ProductName", szProductName);
	AddPadding(buffer, sizeStringTable);
	sizeStringTable += BuildStringStructure(buffer, L"ProductVersion", strVer);
	if (NULL != pStringTable)
	{
		pStringTable->wType = 1;
		pStringTable->wLength = sizeStringTable;
		pStringTable->wValueLength = 0;
	}
	if (NULL != pStringFile)
	{
		pStringFile->wType = 1;
		pStringFile->wLength = sizeStringFileInfo + sizeStringTable;
		pStringFile->wValueLength = 0;
	}
	
	size += sizeStringFileInfo + sizeStringTable;
	AddPadding(buffer, size);

	VarFileInfo* pVarFileInfo = (VarFileInfo*) buffer;
	INT sizeVarFileInfo = sizeof(VarFileInfo);

	szKey = L"VarFileInfo";
	lenKey = lstrlenW(szKey) + 1;
	sizKey = sizeof(WCHAR) * lenKey;

	if (NULL != buffer)
	{
		buffer += sizeVarFileInfo;		
		CopyMemory(buffer, szKey, sizKey);
		buffer += sizKey;
	}

	sizeVarFileInfo += sizKey;
	AddPadding(buffer, sizeVarFileInfo);
	sizeVarFileInfo += BuildVarStructure(buffer, L"Translation", 0x04B00409);
	size += sizeVarFileInfo;
	
	if (NULL != pVarFileInfo)
	{
		pVarFileInfo->wType = 1;
		pVarFileInfo->wLength = sizeVarFileInfo;
		pVarFileInfo->wValueLength = 0;
	}

	if (NULL != pVerInfo)
	{
		pVerInfo->wType = 0;
		pVerInfo->wLength = size;
		pVerInfo->wValueLength = sizFixFileInfo;
	}
	return size;
}

DWORD AddVersionResource(HANDLE hResource, LPCTSTR szSetupFilePath, LPCTSTR szMsiFilePath)
{
	CString productName;
	CString productVersion;
	DWORD dwError = GetMsiDatabaseProperty(szMsiFilePath, _T("ProductName"), productName);
	if (NO_ERROR != dwError)
		return dwError;

	dwError = GetMsiDatabaseProperty(szMsiFilePath, _T("ProductVersion"), productVersion);
	if (NO_ERROR != dwError)
		return dwError;

	// extract just the filename
	CString setupFile(szSetupFilePath);
	int index = setupFile.ReverseFind('\\');
	if (-1 != index)
		setupFile = setupFile.Mid(index + 1);

	LPCWSTR szSetupName = T2CW((LPCTSTR)setupFile);
	LPCWSTR szProductName = T2CW((LPCTSTR)productName);
	LPCWSTR szProductVersion = T2CW((LPCTSTR)productVersion);
	
	CLocalHeap heap;
	INT sizeVersionInfo = BuildVersionInfo(NULL, szProductName, szProductVersion, szSetupName);
	LPBYTE pVersionInfo = (LPBYTE)heap.Allocate(sizeVersionInfo);
	BuildVersionInfo(pVersionInfo, szProductName, szProductVersion, szSetupName);
	
	// Add the resource
	WORD wLanguage = MAKELANGID(LANG_ENGLISH, SUBLANG_ENGLISH_US);
	if (!UpdateResource(hResource, RT_VERSION, MAKEINTRESOURCE(1), wLanguage, pVersionInfo, sizeVersionInfo))
		dwError = GetLastError();

	heap.Free(pVersionInfo);
	return dwError;
}