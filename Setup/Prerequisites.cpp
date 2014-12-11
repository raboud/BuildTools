#include "Stdafx.h"
#include "resource.h"
#include "InstallProperties.h"

void CheckOSVersion(CInstallProperties& installProperties, CString& missing);
void CheckMdac(CInstallProperties& installProperties, CString& missing);
void CheckFramework(CInstallProperties& installProperties, CString& missing);
void CheckIE6(CInstallProperties& installProperties, CString& missing);
void CheckIIS(CInstallProperties& installProperties, CString& missing);
void CheckASPNet(CInstallProperties& installProperties, CString& missing);
void CheckSQLEpress(CInstallProperties& installProperties, CString& missing);

void AppendMissing(CString& missing, UINT res);
void GetVersion(LPCTSTR version, INT& major, INT& minor);
BOOL IsASPNetInstalled();
BOOL IsASPNetEnabled();
BOOL IsIISInstalled();
BOOL IsSQLEpressInstalled();
BOOL IsWinXPSP2OrLater(); 
BOOL IsWin2003SP1OrLater();
BOOL GetIISMajorVersion(DWORD& dwVersion);
BOOL IsIIS6ManagementCompatibilityInstall();

void CheckPrerequisites(CInstallProperties& installProperties, CString& missing)
{
	CheckMdac(installProperties, missing);
	CheckFramework(installProperties, missing);
	CheckIE6(installProperties, missing);
	CheckIIS(installProperties, missing);
	CheckASPNet(installProperties, missing);
	CheckSQLEpress(installProperties, missing);
}

void CheckOSPrerequisites(CInstallProperties& installProperties, CString& missing)
{
	CheckOSVersion(installProperties, missing);
}

void AppendMissing(CString& missing, UINT resId)
{
	CString res;
	res.LoadString(resId);
	if (!missing.IsEmpty())
		missing.Append(_T("\r\n"));	
	missing.Append(res);
}

void CheckOSVersion(CInstallProperties& installProperties, CString& missing)
{
	if (installProperties.CheckMinimumOSXP)
	{
		if (!IsWinXPSP2OrLater())
		{
			AppendMissing(missing, IDS_WINDOW_XP);
		}
	}

	if (installProperties.CheckMinimumOSXP2003)
	{
		if (!IsWin2003SP1OrLater())
		{
			AppendMissing(missing, IDS_WINDOW_2003);
		}
	}
}

void GetVersion(LPCTSTR version, INT& major, INT& minor)
{
	major = 0; minor = 0;
	_stscanf_s(version, _T("%d.%d"), &major, &minor);
}

BOOL CheckMdacValueKey(CRegKey& regKey, LPCTSTR szKeyValue)
{
	TCHAR szMdacVerion[255];
	ULONG nChars = sizeof(szMdacVerion);
	LONG retVal = regKey.QueryStringValue(szKeyValue, szMdacVerion, &nChars);
	if (NO_ERROR == retVal)
	{
		INT major, minor;
		GetVersion(szMdacVerion, major, minor);
		if (major >= 2 && minor >= 80)
			return TRUE;
	}
	return FALSE;
}

void CheckMdac(CInstallProperties& installProperties, CString& missing)
{
	if (!installProperties.CheckMinimumMDAC28)
		return;

	// Dont check MDAC if Vista
	OSVERSIONINFO  os = { 0 };
	os.dwOSVersionInfoSize = sizeof(OSVERSIONINFO);
	GetVersionEx(&os);
	if (os.dwMajorVersion >= 6)
		return;

	CRegKey regKey;
	BOOL prereqMet = FALSE;
	LONG retVal = regKey.Open(HKEY_LOCAL_MACHINE, _T("SOFTWARE\\Microsoft\\DataAccess"), KEY_READ);
	if (NO_ERROR == retVal)
	{
		if (CheckMdacValueKey(regKey, _T("FullInstallVer")))
		{
			prereqMet = true;
		}

		if (!prereqMet)
		{
			if (CheckMdacValueKey(regKey, _T("Version")))
			{
				prereqMet = true;
			}
		}
	}

	if (!prereqMet)
		AppendMissing(missing, IDS_MDAC);
}

void CheckFramework(CInstallProperties& installProperties, CString& missing)
{
	if (!installProperties.CheckMinimumFramework2)
		return;

	CRegKey regKey;
	BOOL prereqMet = FALSE;
	LONG retVal = regKey.Open(HKEY_LOCAL_MACHINE, _T("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v2.0.50727"), KEY_READ);
	if (NO_ERROR == retVal)
	{
		DWORD dwInstalled = 0;
		retVal = regKey.QueryDWORDValue(_T("Install"), dwInstalled);
		if (NO_ERROR == retVal)
		{
			if (dwInstalled == 1)
				prereqMet = true;			
		}
	}

	if (!prereqMet)
		AppendMissing(missing, IDS_FRAMEWORK2);
}

void CheckIE6(CInstallProperties& installProperties, CString& missing)
{
	if (!installProperties.CheckMinimumIE6)
		return;

	CRegKey regKey;
	BOOL prereqMet = FALSE;
	LONG retVal = regKey.Open(HKEY_LOCAL_MACHINE, _T("SOFTWARE\\Microsoft\\Internet Explorer"), KEY_READ);
	if (NO_ERROR == retVal)
	{
		TCHAR szVerion[255];
		ULONG nChars = sizeof(szVerion);
		retVal = regKey.QueryStringValue(_T("Version"), szVerion, &nChars);
		if (NO_ERROR == retVal)
		{
			INT major, minor;
			GetVersion(szVerion, major, minor);
			if (major >= 6)
				prereqMet = true;			
		}
	}

	if (!prereqMet)
		AppendMissing(missing, IDS_IE6);
}

void CheckIIS(CInstallProperties& installProperties, CString& missing)
{
	if (!installProperties.CheckMinimumIIS6)
		return;

	if (!IsIISInstalled())
		AppendMissing(missing, IDS_IIS);
}

void CheckASPNet(CInstallProperties& installProperties, CString& missing)
{
	if (!installProperties.CheckMinimumASPNET2)
		return;

	if (!IsIISInstalled() || !IsASPNetInstalled())
	{
		AppendMissing(missing, IDS_ASPNET);
		return;
	}

	if (!IsIIS6ManagementCompatibilityInstall())
	{
		AppendMissing(missing, IDS_IIS6_MNGR_COMP);
		return;
	}

	if (!IsASPNetEnabled())
	{
		AppendMissing(missing, IDS_ASPNET_ENABLED);
	}
}

void CheckSQLEpress(CInstallProperties& installProperties, CString& missing)
{
	if (!installProperties.CheckMinimumSQLExpressSP1)
		return;

	if (!IsSQLEpressInstalled())
	{
		AppendMissing(missing, IDS_SQL_EXPRESS);
	}
}

BOOL GetIISMajorVersion(DWORD& dwVersion)
{
	CRegKey regKey;
	BOOL prereqMet = FALSE;
	LONG retVal = regKey.Open(HKEY_LOCAL_MACHINE, _T("SOFTWARE\\Microsoft\\InetStp"), KEY_READ);
	if (NO_ERROR == retVal)
	{
		retVal = regKey.QueryDWORDValue(_T("MajorVersion"), dwVersion);
		if (NO_ERROR == retVal)
			return TRUE;			
	}
	return FALSE;
}

BOOL IsIISInstalled()
{
	DWORD dwVersion = 0;
	if (!GetIISMajorVersion(dwVersion))
		return FALSE;

	if (dwVersion >= 6)
		return TRUE;

	return FALSE;
}

BOOL IsASPNetInstalled()
{	
	DWORD dwVersion = 0;
	if (!GetIISMajorVersion(dwVersion))
		return FALSE;

	if (dwVersion >= 7)
		return TRUE;

	WCHAR szComputerName[MAX_COMPUTERNAME_LENGTH + 1];
	DWORD dwBufferSize = sizeof(szComputerName);
	GetComputerNameW(szComputerName, &dwBufferSize);

	WCHAR szAdsPath[1024];
	StringCbPrintfW(szAdsPath, sizeof(szAdsPath), L"IIS://%s/W3SVC", szComputerName);

	CComPtr<IADs> pADs = NULL;
	HRESULT hr = ADsGetObject(szAdsPath, IID_IADs, (void**)&pADs);
	if (FAILED(hr))
		return FALSE;
	
	CComVariant vValue;
	hr = pADs->Get(L"InProcessIsapiApps", &vValue);
	if (FAILED(hr))
		return FALSE;

	USES_CONVERSION;
	CString aspnet = _T("\\v2.0.50727\\aspnet_isapi.dll");
	CComSafeArray<VARIANT> varArray;
	varArray.Attach(vValue.parray);
	DWORD dwCount = varArray.GetCount();
	for (DWORD i = 0; i < dwCount; i++)
	{
		CComVariant vTest = varArray.GetAt(i);
		CString dll = OLE2T(vTest.bstrVal);
		dll.MakeLower();
		int pos = dll.Find(aspnet);
		if (-1 != pos && pos == dll.GetLength() - aspnet.GetLength())
		{
			varArray.Detach();
            return TRUE;
		}
	}
	varArray.Detach();
	return FALSE;
}

BOOL IsASPNetEnabled()
{	
	WCHAR szComputerName[MAX_COMPUTERNAME_LENGTH + 1];
	DWORD dwBufferSize = sizeof(szComputerName);
	GetComputerNameW(szComputerName, &dwBufferSize);

	WCHAR szAdsPath[1024];
	StringCbPrintfW(szAdsPath, sizeof(szAdsPath), L"IIS://%s/W3SVC", szComputerName);

	CComPtr<IADs> pADs = NULL;
	HRESULT hr = ADsGetObject(szAdsPath, IID_IADs, (void**)&pADs);
	if (FAILED(hr))
		return FALSE;
	
	CComVariant vValue;
	hr = pADs->Get(L"WebSvcExtRestrictionList", &vValue);
	if (FAILED(hr))
		return FALSE;

	USES_CONVERSION;
	BOOL bEnabled = FALSE;
	CComSafeArray<VARIANT> varArray;
	varArray.Attach(vValue.parray);
	DWORD dwCount = varArray.GetCount();
	for (DWORD i = 0; i < dwCount; i++)
	{
		CComVariant vTest = varArray.GetAt(i);
		CString dll = OLE2T(vTest.bstrVal);
		dll.MakeLower();
		int pos1 = dll.Find(_T("\\v2.0.50727\\aspnet_isapi.dll"));
		int pos2 = dll.Find(_T("1,"));
		if (0 < pos1 && 0 == pos2)
		{
            bEnabled = TRUE;
			break;
		}
	}
	varArray.Detach();
	return bEnabled;
}

BOOL IsIIS6ManagementCompatibilityInstall()
{	
	WCHAR szComputerName[MAX_COMPUTERNAME_LENGTH + 1];
	DWORD dwBufferSize = sizeof(szComputerName);
	GetComputerNameW(szComputerName, &dwBufferSize);

	WCHAR szAdsPath[1024];
	StringCbPrintfW(szAdsPath, sizeof(szAdsPath), L"IIS://%s/W3SVC", szComputerName);

	CComPtr<IADs> pADs = NULL;
	HRESULT hr = ADsGetObject(szAdsPath, IID_IADs, (void**)&pADs);
	if (FAILED(hr))
		return FALSE;

	return TRUE;
}


BOOL IsSQLEpressInstalled()
{
	USES_CONVERSION;

	CComPtr<IWbemLocator> pLocator;
    HRESULT hr = CoCreateInstance(CLSID_WbemLocator, 0, CLSCTX_INPROC_SERVER, IID_IWbemLocator, (void**)&pLocator);
	if (FAILED(hr))
		return FALSE;
	
	CComPtr<IWbemServices> pServices;	
	CComBSTR resource = _T("root\\Microsoft\\SqlServer\\ComputerManagement");

    // Connect to the root\cimv2 namespace
    hr = pLocator->ConnectServer(resource, NULL, NULL, 0, NULL, 0, 0, &pServices);
	if (FAILED(hr))
		return FALSE;

	// The example that I took this for indicated that I need to CoInitializeSecurity
	// which I'm currently not doing.
	hr = CoSetProxyBlanket(
       pServices,                   // Indicates the proxy to set
       RPC_C_AUTHN_WINNT,           // RPC_C_AUTHN_xxx
       RPC_C_AUTHZ_NONE,            // RPC_C_AUTHZ_xxx
       NULL,                        // Server principal name 
       RPC_C_AUTHN_LEVEL_CALL,      // RPC_C_AUTHN_LEVEL_xxx 
       RPC_C_IMP_LEVEL_IMPERSONATE, // RPC_C_IMP_LEVEL_xxx
       NULL,                        // client identity
       EOAC_NONE                    // proxy capabilities 
    );

	if (FAILED(hr))
		return FALSE;

	CComBSTR queryLang = _T("WQL");
	CComBSTR query = _T("Select * From SqlServiceAdvancedProperty Where SQLServiceType = 1 and ServiceName = 'MSSQL$SQLEXPRESS' and (PropertyName = 'SKUNAME' or PropertyName = 'SPLEVEL')");

    CComPtr<IEnumWbemClassObject> pEnumerator;
    hr = pServices->ExecQuery(
        queryLang, 
        query,
        WBEM_FLAG_FORWARD_ONLY | WBEM_FLAG_RETURN_IMMEDIATELY, 
        NULL, &pEnumerator);

	if (FAILED(hr))
		return FALSE;

	BOOL checkEdition = FALSE;
	BOOL checkSpLevel = FALSE;

	ULONG uReturn = 0;
    while (pEnumerator)
    {
		CComPtr<IWbemClassObject> pClassObject;
        hr = pEnumerator->Next(WBEM_INFINITE, 1, &pClassObject, &uReturn);
        if (0 == uReturn)
            break;
       
        // Get the value of the Name property
		CString sServiceName;
		CComVariant vServiceName;
        hr = pClassObject->Get(L"ServiceName", 0, &vServiceName, 0, 0);
		if (SUCCEEDED(hr))
			sServiceName = OLE2T(vServiceName.bstrVal);

		if (0 == sServiceName.CompareNoCase(_T("MSSQL$SQLEXPRESS")))
		{
			CString sPropertyName;
			CComVariant vPropertyName;
			hr = pClassObject->Get(L"PropertyName", 0, &vPropertyName, 0, 0);
			if (SUCCEEDED(hr))
				sPropertyName = OLE2T(vPropertyName.bstrVal);

			if (0 == sPropertyName.CompareNoCase(_T("SKUNAME")))
			{
				CString sSKUName;
				CComVariant vSKUName;
				hr = pClassObject->Get(L"PropertyStrValue", 0, &vSKUName, 0, 0);
				if (SUCCEEDED(hr))
					sSKUName = OLE2T(vSKUName.bstrVal);

				sSKUName.MakeLower();
				if (-1 != sSKUName.Find(_T("express edition")))
				{
					checkEdition = TRUE;
				}
			}
			else if (0 == sPropertyName.CompareNoCase(_T("SPLEVEL")))
			{
				CComVariant vSPLevel;
				hr = pClassObject->Get(L"PropertyNumValue", 0, &vSPLevel, 0, 0);
				if (SUCCEEDED(hr))
				{
					if (vSPLevel.lVal >= 1)
						checkSpLevel = TRUE;
				}
			}
        }

		if (checkEdition & checkSpLevel)
			return TRUE;
    }
	return FALSE;
}

BOOL IsWinXPSP2OrLater()
{
	OSVERSIONINFOEX osvi;   
	ZeroMemory(&osvi, sizeof(OSVERSIONINFOEX));
	osvi.dwOSVersionInfoSize = sizeof(OSVERSIONINFOEX);
	osvi.dwMajorVersion = 5; //Windows Server 2003, Windows XP, or Windows 2000
	osvi.dwMinorVersion = 1; //Windows XP
	osvi.wServicePackMajor = 2; //Service Pack 2

	DWORDLONG dwlConditionMask = 0;
	VER_SET_CONDITION(dwlConditionMask, VER_MAJORVERSION, VER_GREATER_EQUAL );
	VER_SET_CONDITION(dwlConditionMask, VER_MINORVERSION, VER_GREATER_EQUAL );
	VER_SET_CONDITION(dwlConditionMask, VER_SERVICEPACKMAJOR, VER_GREATER_EQUAL );
	VER_SET_CONDITION(dwlConditionMask, VER_SERVICEPACKMINOR, VER_GREATER_EQUAL );
	return VerifyVersionInfo(&osvi, VER_MAJORVERSION | VER_MINORVERSION | VER_SERVICEPACKMAJOR | VER_SERVICEPACKMINOR, dwlConditionMask);
}

BOOL IsWin2003SP1OrLater() 
{
	OSVERSIONINFOEX osvi;
	ZeroMemory(&osvi, sizeof(OSVERSIONINFOEX));
	osvi.dwOSVersionInfoSize = sizeof(OSVERSIONINFOEX);
	osvi.dwMajorVersion = 5; //Windows Server 2003, Windows XP, or Windows 2000
	osvi.dwMinorVersion = 2; //Windows Server 2003
	osvi.wServicePackMajor = 1; //Service Pack 1
	osvi.wProductType = VER_NT_SERVER;

	// Initialize the condition mask.
	DWORDLONG dwlConditionMask = 0;
	VER_SET_CONDITION(dwlConditionMask, VER_MAJORVERSION, VER_GREATER_EQUAL );
	VER_SET_CONDITION(dwlConditionMask, VER_MINORVERSION, VER_GREATER_EQUAL );
	VER_SET_CONDITION(dwlConditionMask, VER_SERVICEPACKMAJOR, VER_GREATER_EQUAL );
	VER_SET_CONDITION(dwlConditionMask, VER_SERVICEPACKMINOR, VER_GREATER_EQUAL );
	VER_SET_CONDITION(dwlConditionMask, VER_PRODUCT_TYPE, VER_EQUAL );
	return VerifyVersionInfo(&osvi, VER_MAJORVERSION | VER_MINORVERSION | VER_SERVICEPACKMAJOR | VER_SERVICEPACKMINOR | VER_PRODUCT_TYPE, dwlConditionMask);
}