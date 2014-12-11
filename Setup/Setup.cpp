#include "StdAfx.h"
#include "Setup.h"
#include "Util.h"
#include "Prerequisites.h"
#include "InstallProperties.h"

INT   RunSetup();
DWORD GetProductInformation(CString& productCode, CString& installVersion, CString& installedVersion, CString& installedPath);
DWORD SystemShutdown();
DWORD SystemShutdown(bool logout);
DWORD SystemLogout();
BOOL  CheckRunOnce();
VOID  SetRunOnce();

int APIENTRY _tWinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPTSTR lpCmdLine, int nCmdShow)
{
	UNREFERENCED_PARAMETER(hInstance);
	UNREFERENCED_PARAMETER(hPrevInstance);
	UNREFERENCED_PARAMETER(lpCmdLine);
	UNREFERENCED_PARAMETER(nCmdShow);

	CoInitialize(NULL);
	INT retVal = RunSetup();
	CoUninitialize();
	return retVal;
}

INT RunSetup()
{		
	CString msg;
	CString error;
	CString msgTitle;
	msgTitle.LoadString(IDS_SETUP);

	// Load installation properties
	CInstallProperties installProps;
	DWORD dwLastError = installProps.LoadProperties();
	if (NO_ERROR != dwLastError)
	{
		error = GetErrorMessage(dwLastError);
		msg.Format(IDS_ERROR_LOADING_PROPS, (LPCTSTR)error);
		MessageBox(NULL, msg, msgTitle, MB_ICONERROR | MB_OK);
		return ERROR_INSTALL_FAILURE;
	}

	// Change the message box title
	msgTitle.Format(IDS_SETUP_PRODUCT, (LPCTSTR)installProps.Product);

	// Check for administrator
	if (!IsUserAnAdmin())
	{
		msg.LoadString(IDS_ERROR_ADMIN);
		MessageBox(NULL, msg, msgTitle, MB_ICONERROR | MB_OK);
		return ERROR_ACCESS_DENIED;
	}

	// See if the run once has been set
	if (CheckRunOnce())
	{
		msg.LoadString(IDS_ERROR_REBOOT);
		int results = MessageBox(NULL, msg, msgTitle, MB_ICONERROR | MB_YESNOCANCEL);
		if (IDYES != results)
			return ERROR_INSTALL_USEREXIT;

		SystemShutdown();
		return ERROR_SUCCESS_REBOOT_REQUIRED;
	}

	// Check for the OS Prerequisites
	CString osPrerequisites;
	CheckOSPrerequisites(installProps, osPrerequisites);	
	if (!osPrerequisites.IsEmpty())
	{
		msg.Format(IDS_OSPREREQUISITIES, (LPCTSTR)osPrerequisites);
		MessageBox(NULL, msg.GetString(), msgTitle, MB_ICONERROR | MB_OK);
		return ERROR_INSTALL_FAILURE;
	}

	// Check for Prerequisites
	CString prerequisites;
	CheckPrerequisites(installProps, prerequisites);	
	if (!prerequisites.IsEmpty())
	{
		msg.Format(IDS_PREREQUISITIES, (LPCTSTR)prerequisites);
		MessageBox(NULL, msg.GetString(), msgTitle, MB_ICONERROR | MB_OK);
		return ERROR_INSTALL_FAILURE;
	}

	// Get Product Code and version and also check to see if this product is currently installed.
	CString productCode, installVersion, installedVersion, installedPath;
	dwLastError = GetProductInformation(productCode, installVersion, installedVersion, installedPath);
	if (NO_ERROR != dwLastError)
	{
		error = GetErrorMessage(dwLastError);
		msg.Format(IDS_ERROR_PRODUCT_INFO, (LPCTSTR)error);
		MessageBox(NULL, msg, msgTitle, MB_ICONERROR | MB_OK);
		return dwLastError;
	}

	// Ask user if they want to uninstall previous version if exists
	BOOL productExists = !installedVersion.IsEmpty();
	if (productExists)
	{
		msg.Format(IDS_UPGRAGE_MSG, (LPCTSTR)installProps.Product, (LPCTSTR)installedVersion, (LPCTSTR)installVersion);
		int retVal = MessageBox(NULL, msg.GetString(), msgTitle, MB_ICONQUESTION | MB_YESNOCANCEL);
		if (retVal != IDYES)
			return ERROR_INSTALL_USEREXIT;
	}
	
	// Get the "C:\Documents and Settings\All Users\Application Data\" folder name
	TCHAR szCommonAppData[MAX_PATH];
	HRESULT hr = SHGetFolderPath(NULL, CSIDL_COMMON_APPDATA, (HANDLE)-1, SHGFP_TYPE_CURRENT, szCommonAppData);
	if (FAILED(hr))
	{
		error = GetErrorMessage(dwLastError);
		msg.Format(IDS_ERROR_COMMON_APPDATA, (LPCTSTR)error);
		MessageBox(NULL, msg, msgTitle, MB_ICONERROR | MB_OK);
		return dwLastError;
	}
	
	CString commonAppData(szCommonAppData);
	RemoveTrailingBackslash(commonAppData);

	CString strCommonAppData;
	LPCTSTR szAppend = _T("%s\\%s");
	strCommonAppData.Format(szAppend, (LPCTSTR)commonAppData, (LPCTSTR)installProps.Company);

	CString installMediaDir;
	LPCTSTR szInstallMedia = _T("InstallMedia");	
	installMediaDir.Format(szAppend, (LPCTSTR)strCommonAppData, szInstallMedia);

	CString installDir;
	installDir.Format(szAppend, (LPCTSTR)installMediaDir, (LPCTSTR)installProps.Product);

	CString installMedia;
	installMedia.Format(szAppend, (LPCTSTR)installDir, (LPCTSTR)installProps.MsiFileName);
	
	// Makesure our directory exists
	dwLastError = EnsureDirectoryExists(installDir);
	if (NO_ERROR != dwLastError)
	{		
		error = GetErrorMessage(dwLastError);
		msg.Format(IDS_ERROR_CREATE_DIR, (LPCTSTR)installDir, (LPCTSTR)error);
		MessageBox(NULL, msg, msgTitle, MB_ICONERROR | MB_OK);
		return dwLastError;
	}

	CString installLogname(installProps.Product);
	installLogname.Append(_T("Install.log"));
	installLogname.Remove(' ');

	CString uninstallLogname(installProps.Product);
	uninstallLogname.Append(_T("Uninstall.log"));
	uninstallLogname.Remove(' ');

	CString command;
	if (!productExists)
	{
		command.Format(_T("MsiExec.exe /i \"%s\" /lv %s"), (LPCTSTR)installMedia, (LPCTSTR)installLogname);
	}
	else
	{		
		command.Format(_T("MsiExec.exe /x %s /qb /lv %s REBOOT=ReallySuppress"), (LPCTSTR)productCode, (LPCTSTR)uninstallLogname);
		
		// Uninstall the previous product
		dwLastError = WinExecute(command, installDir);
		if (ERROR_SUCCESS_REBOOT_REQUIRED == dwLastError)
		{
			msg.LoadString(IDS_ERROR_REBOOT);
			int results = MessageBox(NULL, msg, msgTitle, MB_ICONERROR | MB_YESNOCANCEL);
			if (IDYES != results)
				return ERROR_INSTALL_USEREXIT;

			SetRunOnce();
			SystemShutdown();
			return ERROR_SUCCESS_REBOOT_REQUIRED;
		}

		if (ERROR_INSTALL_USEREXIT == dwLastError)
		{
			msg.LoadString(IDS_MSG_SETUP_CANCELED);
			MessageBox(NULL, msg.GetString(), msgTitle, MB_ICONINFORMATION | MB_OK);
			return dwLastError;
		}
		
		if (NO_ERROR != dwLastError)
		{
			msg.Format(IDS_MSG_UNINSTALL_FAILED, (LPCTSTR)installProps.Product, dwLastError);
			MessageBox(NULL, msg.GetString(), msgTitle, MB_ICONINFORMATION | MB_OK);
			return dwLastError;
		}

		command.Format(_T("MsiExec.exe /i \"%s\" /lv %s INSTALLDIR=\"%s\""), (LPCTSTR)installMedia, (LPCTSTR)installLogname, (LPCTSTR)installedPath);
	}

	// Extract our MSI
	dwLastError = SaveResourceToFile(IDR_FILES_MSI, installMedia);
	if (NO_ERROR != dwLastError)
	{
		error = GetErrorMessage(dwLastError);
		msg.Format(IDS_ERROR_EXTRACT_MSI, (LPCTSTR)error, (LPCTSTR)installMedia);
		MessageBox(NULL, msg, msgTitle, MB_ICONERROR | MB_OK);
		return dwLastError;
	}

	// Do the install
	dwLastError = WinExecute(command, installDir);

	if (ERROR_INSTALL_USEREXIT == dwLastError)
	{
		// Kill everything in the install dir
		KillDirectory(installDir);

		// remove these directories, these calls may fail if they aren't empty and that is OK.
		RemoveDirectory(installMediaDir);
		RemoveDirectory(strCommonAppData);
	}

	if (NO_ERROR != dwLastError)
		return dwLastError;

	bool rebootAfterInstall = false;
	bool logoutAfterInstall = false;
	if (installProps.RebootAfterInstall)
	{
		msg.LoadString(IDS_MSG_REBOOT);
		int results = MessageBox(NULL, msg, msgTitle, MB_ICONQUESTION | MB_YESNOCANCEL);
		if (IDYES == results)
			rebootAfterInstall = true;
	}
	else if (installProps.LogoutAfterInstall)
	{
		msg.LoadString(IDS_MSG_LOGOUT);
		int results = MessageBox(NULL, msg, msgTitle, MB_ICONQUESTION | MB_YESNOCANCEL);
		if (IDYES == results)
			logoutAfterInstall = true;
	}

	// I want to move the installation media to the install directory.  
	// That way the MSI can clean it up later on uninstall.
	CString installLocation;
	dwLastError = GetMsiProductInfo(productCode, INSTALLPROPERTY_INSTALLLOCATION, installLocation);
	if (NO_ERROR == dwLastError)
	{
		RemoveTrailingBackslash(installLocation);

		CString newInstallDir;
		newInstallDir.Format(szAppend, (LPCTSTR)installLocation, szInstallMedia);
		
		// Move the entire InstallMedia directory
		dwLastError = CopyFilesToDirectory(installDir, newInstallDir);
		if (NO_ERROR == dwLastError)
		{
			// All of the files were successfully copied, so remove the directory
			KillDirectory(installDir);

			// The will cause windows installer to update the installation location
			CString newMediaPath;
			newMediaPath.Format(szAppend, (LPCTSTR)newInstallDir, (LPCTSTR)installProps.MsiFileName);
			command.Format(_T("MsiExec.exe /qn /fv \"%s\""), (LPCTSTR)newMediaPath);
			dwLastError = WinExecute(command.GetString(), newInstallDir);
		}

		// remove these directories, these calls may fail if they aren't empty and that is OK.
		RemoveDirectory(installMediaDir);
		RemoveDirectory(strCommonAppData);
	}

	if (installProps.RebootAfterInstall)
	{
		if (rebootAfterInstall)
			dwLastError = SystemShutdown();
		else
			dwLastError = ERROR_INSTALL_USEREXIT;
	}
	else if (installProps.LogoutAfterInstall)
	{
		if (logoutAfterInstall)
			dwLastError = SystemLogout();
		else
			dwLastError = ERROR_INSTALL_USEREXIT;
	}
	return dwLastError;
}

DWORD GetProductInformation(CString& productCode, CString& installVersion, CString& installedVersion, CString& installedPath)
{
	CString szTempFilePath;
	DWORD dwLength = (MAX_PATH + 1) * sizeof(TCHAR);
	LPTSTR szBuffer = szTempFilePath.GetBuffer(dwLength);
	DWORD retVal = GetTempPath(dwLength, szBuffer);
	if (0 == retVal)
	{
		szTempFilePath.ReleaseBuffer();
		return GetLastError();
	}
	PathAppend(szBuffer, _T("Temp.msi"));
	szTempFilePath.ReleaseBuffer();

	DWORD dwError = SaveResourceToFile(IDR_FILES_MSI, szTempFilePath);
	if (dwError != NO_ERROR)
		return dwError;

	dwError = GetMsiDatabaseProperty(szTempFilePath, _T("ProductCode"), productCode);
	dwError = GetMsiDatabaseProperty(szTempFilePath, _T("ProductVersion"), installVersion);
	dwError = GetMsiProductInfo(productCode, INSTALLPROPERTY_VERSIONSTRING, installedVersion);
	dwError = GetMsiProductInfo(productCode, INSTALLPROPERTY_INSTALLLOCATION, installedPath);
	
	DeleteFile(szTempFilePath);
	return NO_ERROR;
}

DWORD SystemShutdown()
{
	return SystemShutdown(false);
}

DWORD SystemLogout()
{
	return SystemShutdown(true);
}

DWORD SystemShutdown(bool logout)
{
	HANDLE hToken; 
	TOKEN_PRIVILEGES tkp; // Get a token for this process.     
	if (!OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, &hToken)) 
		return GetLastError(); 

	// Get the LUID for the shutdown privilege.  
	LookupPrivilegeValue(NULL, SE_SHUTDOWN_NAME, &tkp.Privileges[0].Luid); 

	tkp.PrivilegeCount = 1;  // one privilege to set    
	tkp.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED; 

	// Get the shutdown privilege for this process.  
	AdjustTokenPrivileges(hToken, FALSE, &tkp, 0, (PTOKEN_PRIVILEGES)NULL, 0);

	if (GetLastError() != NO_ERROR) 
		return GetLastError(); 

	DWORD dwExitFlag = EWX_REBOOT;
	if (logout)
		dwExitFlag = EWX_LOGOFF;

	// Shut down the system and force all applications to close. 
	if (!ExitWindowsEx(dwExitFlag | EWX_FORCE, SHTDN_REASON_MAJOR_SOFTWARE | SHTDN_REASON_MINOR_INSTALLATION | SHTDN_REASON_FLAG_PLANNED)) 
		return GetLastError(); 

	return NO_ERROR;
}

VOID SetRunOnce()
{
	TCHAR szModulePath[MAX_PATH + 1];
	GetModuleFileName(NULL, szModulePath, sizeof(szModulePath));

	TCHAR szFileName[MAX_PATH + 1];
	StrCpyN(szFileName, szModulePath, MAX_PATH);
	PathStripPath(szFileName);

	CRegKey regKey;
	regKey.Create(HKEY_LOCAL_MACHINE, _T("Software\\Microsoft\\Windows\\CurrentVersion\\RunOnce"), NULL, 0, KEY_WRITE);
	regKey.SetStringValue(szFileName, szModulePath);
	regKey.Close();
}

BOOL CheckRunOnce()
{
	TCHAR szModulePath[MAX_PATH + 1];
	GetModuleFileName(NULL, szModulePath, sizeof(szModulePath));

	TCHAR szFileName[MAX_PATH + 1];
	StrCpyN(szFileName, szModulePath, MAX_PATH);
	PathStripPath(szFileName);

	CRegKey regKey;
	LONG retVal = regKey.Open(HKEY_LOCAL_MACHINE, _T("Software\\Microsoft\\Windows\\CurrentVersion\\RunOnce"), KEY_READ);
	if (ERROR_SUCCESS != retVal)
		return FALSE;
	
	ULONG nChars = 0;
	retVal = regKey.QueryStringValue(szFileName, NULL, &nChars);
	regKey.Close();
	return (0 < nChars);
}