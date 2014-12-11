#include "StdAfx.h"
#include "Util.h"
#include "Resource.h"

DWORD LoadFileResource(UINT id, CString& data)
{
	HMODULE hModule = GetModuleHandle(NULL);
	HRSRC hRsrc = FindResource(hModule, MAKEINTRESOURCE(id), _T("FILES"));
	if (NULL == hRsrc)
		return GetLastError();

	HGLOBAL hResData = LoadResource(hModule, hRsrc);
	if (NULL == hRsrc)
		return GetLastError();

	LPVOID pData = LockResource(hResData);
	if (NULL == pData)
		return GetLastError();

    /* UTF-8 is also Unicode but it is similar to MBCS instead of wide
     * character strings. The W2T function does not properly convert
     * UTF-8 to UTF-16, which is what we really want.
     */
	USES_CONVERSION;
	data = W2T((LPWSTR)pData);	//All file resources should be unicode
	UnlockResource(hResData);
	return NO_ERROR;
}

DWORD CreateDirectory(LPCTSTR szDirPath)
{
	DWORD dwLastError = NO_ERROR;
	if (PathFileExists(szDirPath))
		return dwLastError;

	TCHAR backslash = '\\';
	LPTSTR pFound = StrRChr(szDirPath, NULL, backslash);
	if (NULL == pFound)
		return ERROR_BAD_PATHNAME;

	*pFound = NULL;
	dwLastError = CreateDirectory(szDirPath);
	*pFound = backslash;

	if (NO_ERROR != dwLastError)
		return dwLastError;

	if (!CreateDirectory(szDirPath, NULL))
		return GetLastError();

	return dwLastError;
}

DWORD EnsureParentDirectoryExists(LPCTSTR szFilePath)
{
	TCHAR szDirPath[MAX_PATH + 1];
	StrCpyN(szDirPath, szFilePath, MAX_PATH);

	if (!PathIsDirectory(szDirPath))
		PathRemoveFileSpec(szDirPath);

	return EnsureDirectoryExists(szDirPath);
}

DWORD EnsureDirectoryExists(LPCTSTR szDirPath)
{
	return CreateDirectory(szDirPath);
}

DWORD SaveResourceToFile(UINT id, LPCTSTR szFileName)
{
	HMODULE hModule = GetModuleHandle(NULL);
	HRSRC hRsrc = FindResource(hModule, MAKEINTRESOURCE(id), _T("FILES"));
	if (NULL == hRsrc)
		return GetLastError();

	HGLOBAL hResData = LoadResource(hModule, hRsrc);
	if (NULL == hRsrc)
		return GetLastError();

	LPVOID pData = LockResource(hResData);
	if (NULL == pData)
		return GetLastError();

	DWORD exeSize = SizeofResource(hModule, hRsrc);	
		
	DWORD dwLastError = EnsureParentDirectoryExists(szFileName);
	if (NO_ERROR != dwLastError)
		return dwLastError;

	HANDLE hFile = CreateFile(szFileName, GENERIC_WRITE, 0, NULL,
		CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);

	if (INVALID_HANDLE_VALUE == hFile)
		return GetLastError();

	DWORD bytesWritten = 0;
	if (!WriteFile(hFile, pData, exeSize, &bytesWritten, NULL))
	{
		CloseHandle(hFile);
		return GetLastError();
	}
	CloseHandle(hFile);
	return NO_ERROR;
}

VOID GetMsiRecordString(MSIHANDLE hRecord, int iField, CString& strValue)
{
	DWORD dwLength = 0;	
	LPTSTR szBuffer = strValue.GetBuffer();
	DWORD dwLastError = MsiRecordGetString(hRecord, iField, szBuffer, &dwLength);
	if (ERROR_MORE_DATA == dwLastError)
	{
		dwLength = (dwLength + 1) * sizeof(TCHAR); // plus the null
		szBuffer = strValue.GetBuffer(dwLength);
		dwLastError = MsiRecordGetString(hRecord, iField, szBuffer, &dwLength);
	}
	strValue.ReleaseBuffer();
}

DWORD GetMsiDatabaseInstallDir(LPCTSTR szDatabasePath, LPCTSTR szFolder, CString& installDirectory)
{
	HWND hwnd = NULL;
	INSTALLUILEVEL dwUILevel = MsiSetInternalUI(INSTALLUILEVEL_NONE, &hwnd);

	PMSIHANDLE hProduct;
	DWORD retVal = MsiOpenPackage(szDatabasePath, &hProduct);
	if (NO_ERROR != retVal)
		return retVal;

	retVal = MsiDoAction(hProduct, _T("CostInitialize"));
	if (NO_ERROR != retVal)
		return retVal;

	retVal = MsiDoAction(hProduct, _T("FileCost"));
	if (NO_ERROR != retVal)
		return retVal;

    retVal = MsiDoAction(hProduct, _T("CostFinalize"));
	if (NO_ERROR != retVal)
		return retVal;
	
	TCHAR szInstallDir[MAX_PATH + 1];
	DWORD pcchPathBuf = sizeof(szInstallDir);
	retVal = MsiGetTargetPath(hProduct, szFolder, szInstallDir, &pcchPathBuf);
	if (NO_ERROR != retVal)
		return retVal;

	MsiSetInternalUI(dwUILevel, &hwnd);
	installDirectory = szInstallDir;
	return NO_ERROR;
}

DWORD GetMsiDatabaseProperty(LPCTSTR szDatabasePath, LPCTSTR szProperty, CString& propertyValue)
{
	PMSIHANDLE hDatabase;
	DWORD retVal = MsiOpenDatabase(szDatabasePath, MSIDBOPEN_READONLY, &hDatabase);
	if (NO_ERROR != retVal)
		return retVal;

	CString query;
	query.Format(_T("SELECT Value FROM Property WHERE Property='%s'"), szProperty);

	PMSIHANDLE hView = NULL;
	retVal = MsiDatabaseOpenView(hDatabase, query.GetString(), &hView);
	if (NO_ERROR != retVal)
		return retVal;

	retVal = MsiViewExecute(hView, 0);
	if (NO_ERROR != retVal)
		return retVal;

	PMSIHANDLE hRecord = NULL;
	retVal = MsiViewFetch(hView, &hRecord);
	if (NO_ERROR != retVal)
		return retVal;

	GetMsiRecordString(hRecord, 1, propertyValue);
	return NO_ERROR;
}

DWORD GetMsiProductInfo(LPCTSTR szProductCode, LPCTSTR szProperty, CString& propertyValue)
{
	DWORD dwLength = 0;	
	propertyValue.Empty();	
	LPTSTR szBuffer = propertyValue.GetBuffer();
	DWORD retVal = MsiGetProductInfo(szProductCode, szProperty, szBuffer, &dwLength);
	if (ERROR_MORE_DATA == retVal)
	{
		dwLength = (dwLength + 1) * sizeof(TCHAR); // plus the null
		szBuffer = propertyValue.GetBuffer(dwLength);
		retVal = MsiGetProductInfo(szProductCode, szProperty, szBuffer, &dwLength);
	}
	
	if (ERROR_UNKNOWN_PRODUCT == retVal)
		retVal = NO_ERROR;

	propertyValue.ReleaseBuffer();
	return retVal;
}

DWORD WinExecute(LPCTSTR szCommandLine, LPCTSTR szWorkingDirectory)
{
	STARTUPINFO StartupInfo = { 0 };
    PROCESS_INFORMATION ProcessInfo = { 0 };
    StartupInfo.cb = sizeof(StartupInfo);
	StartupInfo.dwFlags = STARTF_USESHOWWINDOW;
	StartupInfo.wShowWindow = SW_HIDE;

    // Start the child process. 
    if (!CreateProcess(NULL, (LPTSTR)szCommandLine,
			NULL, NULL, FALSE, 0, NULL, szWorkingDirectory, &StartupInfo, &ProcessInfo))
    {
		return GetLastError();
    }

    // Wait until child process exits.
    WaitForSingleObject(ProcessInfo.hProcess, INFINITE);

	DWORD dwExitCode = 0;
	if (!GetExitCodeProcess(ProcessInfo.hProcess, &dwExitCode))
		return GetLastError();

    // Close process and thread handles. 
    CloseHandle(ProcessInfo.hProcess);
    CloseHandle(ProcessInfo.hThread);
	return dwExitCode;
}

CString GetErrorMessage(DWORD dwErrorCode)
{
	// Get the System error message
	LPVOID lpMsgBuf = NULL;
	DWORD retVal = FormatMessage( 
		FORMAT_MESSAGE_ALLOCATE_BUFFER | 
		FORMAT_MESSAGE_FROM_SYSTEM | 
		FORMAT_MESSAGE_IGNORE_INSERTS,
		NULL,
		dwErrorCode,
		MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), // Default language
		(LPTSTR) &lpMsgBuf,
		0,
		NULL );
	
	CString errorString;

	// Check the return value
	if (0 == retVal)
	{
		errorString.Format(IDS_ERROR_CODE, dwErrorCode);
	}
	else
	{
		errorString = (LPCTSTR)lpMsgBuf;
		errorString.Trim();

		// Free the buffer.
		LocalFree(lpMsgBuf);
	}
	return errorString;
}

DWORD KillDirectory(LPCTSTR szDirName)
{
	CString strDir(szDirName);
	RemoveTrailingBackslash(strDir);

	DWORD dwLastError = NO_ERROR;
	if (!PathFileExists(strDir))
		return dwLastError;

	CString search;
	search.Format(_T("%s\\*.*"), (LPCTSTR)strDir);

	WIN32_FIND_DATA findFileData;
	HANDLE hFind = FindFirstFile(search, &findFileData);
	if (hFind != INVALID_HANDLE_VALUE)
	{
		do
		{
			// Build the path string
			CString path;
			path.Format(_T("%s\\%s"), (LPCTSTR)strDir, findFileData.cFileName);

			// Check for a directory
			if (FILE_ATTRIBUTE_DIRECTORY == (findFileData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY))
			{
				// Make sure it isn't either "." or ".."
				if (0 != lstrcmp(_T("."), findFileData.cFileName) && 0 != lstrcmp(_T(".."), findFileData.cFileName))
				{
					// Remove the child directory
					dwLastError = KillDirectory(path);
					if (NO_ERROR != dwLastError)
						break;
				}
			}
			else
			{
				// Ensure normal attributs
				SetFileAttributes(path, FILE_ATTRIBUTE_NORMAL);

				// Remove the file
				if (!DeleteFile(path))
				{
					dwLastError = GetLastError();
					break;
				}
			}
		} while (FindNextFile(hFind, &findFileData));

		FindClose(hFind);
	}

	if (NO_ERROR == dwLastError)
	{
		// Delete the directory
		if (!RemoveDirectory(strDir))
			dwLastError = GetLastError();
	}

	return dwLastError;

}

void RemoveTrailingBackslash(CString& path)
{
	int index = path.ReverseFind('\\');
	if (index + 1 == path.GetLength())
		path = path.Mid(0, index);
}

DWORD CopyFilesToDirectory(LPCTSTR szSource, LPCTSTR szDest)
{
	DWORD dwLastError = NO_ERROR;
	if (!PathFileExists(szSource))
		return dwLastError;

	dwLastError = EnsureDirectoryExists(szDest);
	if (NO_ERROR != dwLastError)
		return dwLastError;

	CString strDest(szDest);
	CString strSource(szSource);
	RemoveTrailingBackslash(strDest);
	RemoveTrailingBackslash(strSource);	

	CString search;
	search.Format(_T("%s\\*.*"), (LPCTSTR)strSource);

	WIN32_FIND_DATA findFileData;
	HANDLE hFind = FindFirstFile(search, &findFileData);
	if (hFind == INVALID_HANDLE_VALUE)
	{
		dwLastError = GetLastError();
		return dwLastError;
	}

	do
	{
		// Look for files in this directory
		if (FILE_ATTRIBUTE_DIRECTORY != (findFileData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY))
		{
			// Build the source path
			CString sourceFile;
			sourceFile.Format(_T("%s\\%s"), (LPCTSTR)strSource, findFileData.cFileName);

			// Build the destination path
			CString destFile;
			destFile.Format(_T("%s\\%s"), (LPCTSTR)strDest, findFileData.cFileName);

			// Copy the file
			if (!CopyFile(sourceFile, destFile, FALSE))
			{
				dwLastError = GetLastError();
				break;
			}
		}
	} while (FindNextFile(hFind, &findFileData));

	FindClose(hFind);
	return dwLastError;
}