using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace ThoughtWorks.CruiseControl.Core.Tasks
{
    /// <summary>
    /// Class to manipulate Advanced Installer project file.
    /// </summary>
    public class AdvancedInstaller
    {
        string aiVersion;
        string aipFile;

        public AdvancedInstaller(string aiVersion, string aipFile)
        {
            this.aiVersion = aiVersion;
            this.aipFile = aipFile;
        }

        private string FormatArgument(string arg)
        {
            string ret;
            if (arg.Contains(" "))
            {
                ret = "\"" + arg + "\"";
            }
            else
            {
                ret = arg;
            }
            return ret;
        }

        #region Version
        public void SetVersion(int major, int minor, int revision)
        {
            DoEditCommand(String.Format("/SetVersion {0}.{1}.{2}", major, minor, revision));
        }        

        public void SetVersion(string fromFilePath)
        {
            if (String.IsNullOrEmpty(fromFilePath))
            {
                throw new NullReferenceException("The fromFilePath parameter is mandatory.");
            }

            DoEditCommand(String.Format("/SetVersion -fromfile {0}", fromFilePath));
        }

        public void IncrementVersion()
        {
            DoEditCommand("/SetVersion -increment");
        }
        
        #endregion

        #region Files and Folders
        public void AddFile(string targetFolderPath, string sourceFilePath)
        {
            if (String.IsNullOrEmpty(targetFolderPath))
            {
                throw new NullReferenceException("The targetFolderPath parameter is mandatory.");
            }

            if (String.IsNullOrEmpty(sourceFilePath))
            {
                throw new NullReferenceException("The sourceFilePath parameter is mandatory.");
            }

            
            DoEditCommand(String.Format("/AddFile {0} {1}", targetFolderPath, sourceFilePath));            
        }

        public void DeleteFile(string targetFilePath)
        {
            if (String.IsNullOrEmpty(targetFilePath))
            {
                throw new NullReferenceException("The targetFilePath parameter is mandatory.");
            }

            DoEditCommand(String.Format("/DelFile {0}", targetFilePath));
        }

        public void AddFolder(string targetFolderPath, string sourceFolderPath)
        {
            if (String.IsNullOrEmpty(targetFolderPath))
            {
                throw new NullReferenceException("The targetFolderPath parameter is mandatory.");
            }

            if (String.IsNullOrEmpty(sourceFolderPath))
            {
                throw new NullReferenceException("The sourceFolderPath parameter is mandatory.");
            }


            DoEditCommand(String.Format("/AddFolder {0} {1}", targetFolderPath, sourceFolderPath));
        }

        public void DeleteFolder(string targetFolderPath)
        {
            if (String.IsNullOrEmpty(targetFolderPath))
            {
                throw new NullReferenceException("The targetFolderPath parameter is mandatory.");
            }

            DoEditCommand(String.Format("/DelFolder {0}", targetFolderPath));
        }

        public void NewFileSortcut(string shortcutName, string folderPath, string targetFilePath)
        {
            NewFileSortcut(shortcutName, folderPath, targetFilePath, null);
        }

        public void NewFileSortcut(string shortcutName, string folderPath, string targetFilePath, string arguments)
        {
            if (String.IsNullOrEmpty(shortcutName))
            {
                throw new NullReferenceException("The shortcutName parameter is mandatory.");
            }

            if (String.IsNullOrEmpty(folderPath))
            {
                throw new NullReferenceException("The folderPath parameter is mandatory.");
            }

            if (String.IsNullOrEmpty(targetFilePath))
            {
                throw new NullReferenceException("The targetFilePath parameter is mandatory.");
            }

            string command = String.Format("/NewShortcut -name {0} -dir {1} -target {2}", shortcutName, folderPath, targetFilePath);
            if(!String.IsNullOrEmpty(arguments))
            {
                command += String.Format(" -arg {0}", arguments);
            }

            DoEditCommand(command);
        }

        public void DeleteFileShortcut(string shortcutName, string folderPath)
        {
            if (String.IsNullOrEmpty(shortcutName))
            {
                throw new NullReferenceException("The shortcutName parameter is mandatory.");
            }

            if (String.IsNullOrEmpty(folderPath))
            {
                throw new NullReferenceException("The folderPath parameter is mandatory.");
            }

            DoEditCommand(String.Format("/DelShortcut -name {0} -dir {1}", shortcutName, folderPath));
        }
        #endregion

        #region Registry
        public void NewRegistryEntry(string registryKey, string registryValueName, string registryData)
        {
            
        }

        public void NewRegistryEntry(string registryValueName, string registryData)
        {
            NewRegistryEntry(null, registryValueName, registryData, false);
        }

        public void NewRegistryEntry(string registryKey, string registryValueName, string registryData, bool is64bit)
        {
            if (String.IsNullOrEmpty(registryKey) && String.IsNullOrEmpty(registryValueName))
            {
                throw new NullReferenceException("The registryKey or registryValueName parameter are mandatory.");
            }

            string command = "/NewReg";
            if (!String.IsNullOrEmpty(registryKey))
            {
                command += String.Format(" -RegKey {0}", registryKey);
            }

            if (!String.IsNullOrEmpty(registryValueName))
            {
                command += String.Format(" -RegValue {0}", registryValueName);

                if (!String.IsNullOrEmpty(registryData))
                {
                    command += String.Format(" -Data {0}", registryData);
                }
            }

            if (is64bit)
            {
                command += " -64bit";
            }

            DoEditCommand(command);
        }

        public void AddRegistryKey(string sourceRegistryKey, string destRegistryKey)
        {
            AddRegistryKey(sourceRegistryKey, destRegistryKey, false);
        }

        public void AddRegistryKey(string sourceRegistryKey, string destRegistryKey, bool singleComponent)
        {
            if (String.IsNullOrEmpty(sourceRegistryKey))
            {
                throw new NullReferenceException("The sourceRegistryKey parameter is mandatory.");
            }

            if (String.IsNullOrEmpty(destRegistryKey))
            {
                throw new NullReferenceException("The destRegistryKey parameter is mandatory.");
            }

            string command = String.Format("/AddReg {0} {1}", sourceRegistryKey, destRegistryKey);
            if(singleComponent)
            {
                command += " -SingleComponent";
            }

            DoEditCommand(command);
        }

        public void DelRegistry(string registryKey, string registryValueName)
        {
            if (String.IsNullOrEmpty(registryKey) && String.IsNullOrEmpty(registryValueName))
            {
                throw new NullReferenceException("The registryKey or registryValueName parameter are mandatory.");
            }

            string command = "/DelReg";

            if (!String.IsNullOrEmpty(registryKey))
            {
                command += String.Format(" -RegKey {0}", registryKey);
            }

            if (!String.IsNullOrEmpty(registryValueName))
            {
                command += String.Format(" -RegValue {0}", registryValueName);
            }

            DoEditCommand(command);
        }
        #endregion

        #region Environment
        public void SetPackageName(string filePath)
        {
            SetPackageName(filePath, null);
        }

        public void SetPackageName(string filePath, string buildName)
        {
            if (String.IsNullOrEmpty(filePath))
            {
                throw new NullReferenceException("The filePath parameter is mandatory.");
            }

            string command = String.Format("/SetPackageName {0}", filePath);
            if(!String.IsNullOrEmpty(buildName))
            {
                command += String.Format(" -buildname {0}", buildName);
            }

            DoEditCommand(command);
        }

        public void SetProperty(string name, string value)
        {
            SetProperty(name, value, null);
        }

        public void SetProperty(string name, string value, string buildName)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new NullReferenceException("The name parameter is mandatory.");
            }

            if (String.IsNullOrEmpty(value))
            {
                throw new NullReferenceException("The value parameter is mandatory.");
            }

            string command = String.Format("/SetProperty {0}={1}", name, FormatArgument(value));            

            if (!String.IsNullOrEmpty(buildName))
            {
                command += String.Format(" -buildname {0}", buildName);
            }

            DoEditCommand(command);
        }

        public void DeleteProperty(string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new NullReferenceException("The name parameter is mandatory.");
            }

            DoEditCommand(String.Format("/DelProperty {0}", name));
        }

        public string GetProperty(string name)
        {
            string value = null;
            if (String.IsNullOrEmpty(name))
            {
                throw new NullReferenceException("The name parameter is mandatory.");
            }

            string o = DoEditCommand(String.Format("/GetProperty {0}", name));
            if (o != null)
            {
                value = o.Trim();
            }

            return value;
        }
        #endregion

        protected string DoEditCommand(string command)
        {
            return DoCommand(String.Format("/edit {0} {1}", FormatArgument(aipFile), command));
        }        

        public string Build()
        {
            return DoCommand(String.Format("/build {0}", FormatArgument(aipFile)));
        }

        public string ReBuild()
        {
            return DoCommand(String.Format("/rebuild {0}", FormatArgument(aipFile)));
        }

        protected string DoCommand(string command)
        {
            ProcessStartInfo pinfo = new ProcessStartInfo();
            pinfo.FileName = GetCommandLineTool();
            pinfo.Arguments = command;
            pinfo.RedirectStandardError = true;
            pinfo.RedirectStandardOutput = true;
            pinfo.WindowStyle = ProcessWindowStyle.Hidden;
            pinfo.UseShellExecute = false;

            Process p = Process.Start(pinfo);
            if (!p.Start())
            {
                throw new AdvancedInstallerException(aipFile, String.Format("Can't execute the Advanced Installer command : {0}", command));
            }

            p.WaitForExit();

            string returnMsg = p.StandardOutput.ReadToEnd();

            if (!String.IsNullOrEmpty(returnMsg))
            {
                string exceptionIdentificator = Environment.NewLine + "Exception - ";
                int exceptionPos = returnMsg.LastIndexOf(exceptionIdentificator);
                if (exceptionPos > -1)
                {
                    exceptionPos += exceptionIdentificator.Length;
                    int endExceptionPos = returnMsg.IndexOf(Environment.NewLine, exceptionPos);
                    if (endExceptionPos < 0)
                    {
                        endExceptionPos = returnMsg.Length;
                    }

                    string exceptionMessage = returnMsg.Substring(exceptionPos, endExceptionPos - exceptionPos);
                    throw new AdvancedInstallerException(aipFile, exceptionMessage);
                }
            }

            return returnMsg;
        }

        // Get command line tool for Advanced Installer 6.9 only. Because we don't know if newer version will be still compatible.
        protected string GetCommandLineTool()
        {
            string programFileDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            if (String.IsNullOrEmpty(programFileDirectory))
            {
                programFileDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            }

            return Path.Combine(programFileDirectory, String.Format("Caphyon\\Advanced Installer {0}\\AdvancedInstaller.com", this.aiVersion));
        }
    }
}
