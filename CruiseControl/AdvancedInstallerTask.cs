using System;
using System.Collections.Generic;
using System.Text;
using Exortech.NetReflector;
using System.IO;

namespace ThoughtWorks.CruiseControl.Core.Tasks
{
    [ReflectorType("advancedinstaller")]
    public class AdvancedInstallerTask : ITask
    {
        public AdvancedInstallerTask()
        {

        }

        #region ITask Members

        public void Run(IIntegrationResult result)
        {
            result.BuildProgressInformation.SignalStartRunTask(String.Format("Building MSI package {0} with Advanced Installer", result.ProjectName));
            AdvancedInstallerResult aiResult = AttemptToBuild(result);
            
            result.AddTaskResult(aiResult);
        }        

        #endregion

        bool incrementVersion = false;
        [ReflectorProperty("IncrementVersion", Required=false)]
        public bool IncrementVersion
        {
            get
            {
                return incrementVersion;
            }
            set
            {
                incrementVersion = value;
            }
        }

        string outputDirectory = null;
        [ReflectorProperty("OutputDirectory", Required = false)]
        public string OutputDirectory
        {
            get { return outputDirectory; }
            set { outputDirectory = value; }
        }

        string repository = null;
        [ReflectorProperty("Repository", Required = true)]
        public string Repository
        {
            get { return repository; }
            set { repository = value; }
        }

        string aiVersion = null;
        [ReflectorProperty("AIVersion", Required = true)]
        public string AIVersion
        {
            get { return aiVersion; }
            set { aiVersion = value; }
        }

        private AdvancedInstallerResult AttemptToBuild(IIntegrationResult result)
        {
            AdvancedInstallerResult aiResult = new AdvancedInstallerResult();
            if (!String.IsNullOrEmpty(Repository))
            {
                string output;
                if (String.IsNullOrEmpty(outputDirectory))
                {
                    output = Path.Combine(Path.Combine(result.WorkingDirectory, "installer"), "output");
                }
                else
                {
                    output = outputDirectory;
                }

                if (Directory.Exists(output))
                {
                    Directory.Delete(output, true);
                }

                string[] aipFiles = GetAIPFiles(result.WorkingDirectory);
                aiResult.Data = String.Empty;
                for (int i = 0; i < aipFiles.Length && aiResult.CheckIfSuccess(); i++)
                {
                    BuildMSI(aipFiles[i], result, aiResult);
                }

                if (aiResult.CheckIfSuccess())
                {                    
                    try
                    {
                        string repositoryBuild = Path.Combine(Repository, result.ProjectName);
                        if (!Directory.Exists(repositoryBuild))
                        {
                            Directory.CreateDirectory(repositoryBuild);
                        }

                        repositoryBuild = Path.Combine(repositoryBuild, DateTime.Now.ToString("yyyy-MM-dd"));
                        if (!Directory.Exists(repositoryBuild))
                        {
                            Directory.CreateDirectory(repositoryBuild);
                        }
                        string[] files = Directory.GetFiles(output);
                        foreach (string file in files)
                        {
                            File.Copy(file, Path.Combine(repositoryBuild, Path.GetFileName(file)), true);
                        }
                    }
                    catch (IOException ex)
                    {
                        aiResult.HasFailed = true;
                        aiResult.Data = ex.Message;
                        throw new AdvancedInstallerCCNETException(this, ex.Message);
                    }
                }
            }
            else
            {
                aiResult.HasFailed = true;
                aiResult.Data = "Please set the repository directory.";
            }
            return aiResult;
        }

        public string[] GetAIPFiles(string projectDirectory)
        {
            List<string> aipFiles = new List<string>();

            if (!String.IsNullOrEmpty(projectDirectory))
            {
                string installerDirectory = Path.Combine(projectDirectory, "installer");
                string[] files = Directory.GetFiles(installerDirectory, "*.aip", SearchOption.TopDirectoryOnly);
                aipFiles.AddRange(files);
            }

            return aipFiles.ToArray();
        }

        protected void BuildMSI(string aipFile, IIntegrationResult result, AdvancedInstallerResult aiResult)
        {
            try
            {
                AdvancedInstaller ai = new AdvancedInstaller(aiVersion, aipFile);                
                aiResult.Data += String.Format("MSI Installer for {0} project created." + Environment.NewLine, result.ProjectName);
                aiResult.Data += ai.ReBuild();                
            }
            catch (AdvancedInstallerException ex)
            {
                throw new AdvancedInstallerCCNETException(this, ex.Message);
            }
        }
    }
}
