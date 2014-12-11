#pragma once

DWORD LoadFileResource(UINT id, CString& data);

DWORD SaveResourceToFile(UINT id, LPCTSTR szFileName);

DWORD GetMsiDatabaseProperty(LPCTSTR szDatabasePath, LPCTSTR szProperty, CString& propertyValue);

DWORD GetMsiDatabaseInstallDir(LPCTSTR szDatabasePath, LPCTSTR szInstallDir, CString& installDirectory);

DWORD GetMsiProductInfo(LPCTSTR szProductCode, LPCTSTR szProperty, CString& propertyValue);

DWORD WinExecute(LPCTSTR szCommandLine, LPCTSTR szWorkingDirectory);

DWORD EnsureDirectoryExists(LPCTSTR szDirPath);

DWORD EnsureParentDirectoryExists(LPCTSTR szFilePath);

CString GetErrorMessage(DWORD dwErrorCode);

DWORD KillDirectory(LPCTSTR szDirName);

void RemoveTrailingBackslash(CString& path);

DWORD CopyFilesToDirectory(LPCTSTR szSource, LPCTSTR szDest);