#include "StdAfx.h"
#include "InstallProperties.h"
#include "Util.h"
#include "Resource.h"

CInstallProperties::CInstallProperties()
{
	CheckMinimumOSXP = FALSE;
	CheckMinimumOSXP2003 = FALSE;
	CheckMinimumMDAC28 = FALSE;
	CheckMinimumFramework2 = FALSE;
	CheckMinimumIE6 = FALSE;
	CheckMinimumIIS6 = FALSE;
	CheckMinimumASPNET2 = FALSE;
	CheckMinimumSQLExpressSP1 = FALSE;
	RebootAfterInstall = FALSE;
	LogoutAfterInstall = FALSE;
}

CInstallProperties::~CInstallProperties()
{
}

DWORD CInstallProperties::LoadProperties()
{
	CString data;
	DWORD dwError = LoadFileResource(IDR_FILES_PROPERTIES, data);
	if (NO_ERROR != dwError)
		return dwError;

	Product = GetStringProperties(_T("Product"), data);
	Company = GetStringProperties(_T("Company"), data);
	CheckMinimumOSXP = GetBOOLProperties(_T("CheckMinimumOSXP"), data);
	CheckMinimumOSXP2003 = GetBOOLProperties(_T("CheckMinimumOSXP2003"), data);
	CheckMinimumMDAC28 = GetBOOLProperties(_T("CheckMinimumMDAC28"), data);
	CheckMinimumFramework2 = GetBOOLProperties(_T("CheckMinimumFramework2"), data);
	CheckMinimumIE6 = GetBOOLProperties(_T("CheckMinimumIE6"), data);
	CheckMinimumIIS6 = GetBOOLProperties(_T("CheckMinimumIIS6"), data);
	CheckMinimumASPNET2 = GetBOOLProperties(_T("CheckMinimumASPNET2"), data);
	CheckMinimumSQLExpressSP1 = GetBOOLProperties(_T("CheckMinimumSQLExpressSP1"), data);
	MsiFileName = GetStringProperties(_T("MsiFileName"), data);
	RebootAfterInstall = GetBOOLProperties(_T("RebootAfterInstall"), data);;
	LogoutAfterInstall = GetBOOLProperties(_T("LogoutAfterInstall"), data);;
	return NO_ERROR;
}

BOOL CInstallProperties::GetBOOLProperties(LPCTSTR szProperty, CString& data)
{
	CString result = GetStringProperties(szProperty, data);
	if (0 == result.CompareNoCase(_T("TRUE")))
		return TRUE;
	return FALSE;
}

CString CInstallProperties::GetStringProperties(LPCTSTR szProperty, CString& data)
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
