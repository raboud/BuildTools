#include "StdAfx.h"
#include "Util.h"

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