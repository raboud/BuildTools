#pragma once

#include "InstallProperties.h"

void CheckPrerequisites(CInstallProperties& installProperties, CString& missing);
void CheckOSPrerequisites(CInstallProperties& installProperties, CString& missing);
