#pragma once

class CInstallProperties
{
public:
	CString Product;
	CString Company;
	BOOL    CheckMinimumOSXP;
	BOOL    CheckMinimumOSXP2003;
	BOOL    CheckMinimumMDAC28;
	BOOL    CheckMinimumFramework2;
	BOOL    CheckMinimumIE6;
	BOOL    CheckMinimumIIS6;
	BOOL    CheckMinimumASPNET2;
	BOOL    CheckMinimumSQLExpressSP1;
	BOOL    RebootAfterInstall;
	BOOL    LogoutAfterInstall;
	CString MsiFileName;

public:
	CInstallProperties();
	~CInstallProperties();

	DWORD   LoadProperties();

private:

	BOOL    GetBOOLProperties(LPCTSTR szProperty, CString& data);
	CString GetStringProperties(LPCTSTR szProperty, CString& data);	
};
