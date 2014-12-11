using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Text.RegularExpressions;
using System.IO;

namespace RandREng.MsBuildTasks
{
    public class Version2Task : Task
    {
        private string assemblyVersion = null;
        private string companyName = null;
        private string copyRigtht = null;
        private string fileVersion = null;
        private string productName = null;

        [Required]
        public string AssemblyVersion
        {
            get { return assemblyVersion; }
            set { assemblyVersion = value; }
        }

        public string CompanyName
        {
            get { return this.companyName; }
            set { this.companyName = value; }
        }

        public string Copyright
        {
            get { return this.copyRigtht; }
            set { this.copyRigtht = value; }
        }

        public string FileVersion
        {
            get { return fileVersion; }
            set { fileVersion = value; }
        }

        public string ProductName
        {
            get { return this.productName; }
            set { this.productName = value; }
        }

        ITaskItem[] _files;
        [Required]
        public ITaskItem[] Files
        {
            set { _files = value; }
        }

        public override bool Execute()
        {
            try
            {
                if (null == this.assemblyVersion || 0 == this.assemblyVersion.Length)
                    throw new ArgumentException("The assembly version is required", "assemblyversion");


                if (null == this.fileVersion || 0 == this.fileVersion.Length)
                    this.fileVersion = this.assemblyVersion;

                Version version = new Version(this.assemblyVersion);
                Version fileVersion = new Version(this.fileVersion);
                if (_files.Length == 0)
                {
                    Log.LogWarning("No files found to process");
                    return true;
                }
                int cnt = 0;
                foreach (ITaskItem item in _files)
                {
                    string ext = Path.GetExtension(item.ItemSpec);
                    if(string.Compare(".cs",ext,true)==0)
                    {
                        UpdateAssemblyInfoFileCSharp(item,version,fileVersion);
                    }
                    cnt++;

                }
                if (cnt == 0)
                {
                    Log.LogWarning("No files were processed. Check that the file extension is supported");
                }
                else
                {
                    Log.LogMessage(cnt + " file(s) updated.");
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError(ex.Message);
            }
            return false;
        }

        private void UpdateAssemblyInfoFileCSharp(ITaskItem file, Version version, Version fileVersion)
        {
            EncodedFile encodedFile = new EncodedFile(file.ItemSpec);
            string content = encodedFile.Read();

            string versionString =
                "[assembly: AssemblyVersion(\"" + version.ToString() + "\")]";
            Regex regex = new Regex("\\[assembly:\\s*AssemblyVersion.+?\\]", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            content = regex.Replace(content, versionString);

            string fileVersionString =
                "[assembly: AssemblyFileVersion(\"" + fileVersion.ToString() + "\")]";
            regex = new Regex("\\[assembly:\\s*AssemblyFileVersion.+?\\]", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            content = regex.Replace(content, fileVersionString);

            if (!string.IsNullOrEmpty(this.CompanyName))
            {
                string replace = "[assembly: AssemblyCompany(\"" + this.CompanyName + "\")]";
                string search = "\\[assembly:\\s*AssemblyCompany.+?\\]";
                regex = new Regex(search, RegexOptions.Multiline | RegexOptions.IgnoreCase);
                content = regex.Replace(content, replace);
            }

            if (!string.IsNullOrEmpty(this.Copyright))
            {
                string replace = "[assembly: AssemblyCopyright(\"" + this.Copyright + "\")]";
                string search = "\\[assembly:\\s*AssemblyCopyright.+?\\]";
                regex = new Regex(search, RegexOptions.Multiline | RegexOptions.IgnoreCase);
                content = regex.Replace(content, replace);
            }

            if (!string.IsNullOrEmpty(this.ProductName))
            {
                string replace = "[assembly: AssemblyProduct(\"" + this.ProductName + "\")]";
                string search = "\\[assembly:\\s*AssemblyProduct.+?\\]";
                regex = new Regex(search, RegexOptions.Multiline | RegexOptions.IgnoreCase);
                content = regex.Replace(content, replace);
            }

            encodedFile.Write(content);
            LogWrite(file.ItemSpec);
        }
        private void LogWrite(string path)
        {
            Log.LogMessage("Version Updated: " + path);
        }



    }
}
