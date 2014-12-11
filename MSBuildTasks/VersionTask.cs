using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace RandREng.MsBuildTasks
{
    public class VersionTask : Task
    {
        private string assemblyVersion = null;
        private string companyName = null;
        private string copyRigtht = null;
        private string fileVersion = null;
        private string path = null;
        private string productName = null;
        private bool recursive = false;

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

        [Required]
        public string Path
        {
            get { return path; }
            set { path = value; }
        }

        public string ProductName
        {
            get { return this.productName; }
            set { this.productName = value; }
        }

        public bool Recursive
        {
            get { return recursive; }
            set { recursive = value; }
        }

        public override bool Execute()
        {
            try
            {
                if (null == this.assemblyVersion || 0 == this.assemblyVersion.Length)
                    throw new ArgumentException("The assembly version is required", "assemblyversion");

                if (null == this.path || 0 == this.path.Length)
                    throw new ArgumentException("The file path is required", "path");

                if (null == this.fileVersion || 0 == this.fileVersion.Length)
                    this.fileVersion = this.assemblyVersion;

                Version version = new Version(this.assemblyVersion);
                Version fileVersion = new Version(this.fileVersion);
                UpdateVersion(this.path, version, fileVersion);
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError(ex.Message);
            }
            return false;
        }

        private void UpdateVersion(string path, Version version, Version fileVersion)
        {
            UpdateFiles(path, version, fileVersion);

            if (this.recursive)
            {
                string[] directories = Directory.GetDirectories(path);
                foreach (string directory in directories)
                    UpdateVersion(directory, version, fileVersion);
            }
        }

        private void UpdateFiles(string path, Version version, Version fileVersion)
        {
            UpdateVersionInfoFiles(path, fileVersion);
            UpdateAssemblyInfoFiles(path, version, fileVersion);
            UpdateVisualBasicProjectFiles(path, fileVersion);
        }

        private void UpdateAssemblyInfoFilesCSharp(string[] files, Version version, Version fileVersion)
        {
            foreach (string file in files)
            {
                EncodedFile encodedFile = new EncodedFile(file);
                string content = encodedFile.Read();

                string versionString =
                    "[assembly: AssemblyVersion(\"" + version.ToString() + "\")]";
                Regex regex = new Regex("\\[assembly:\\s*AssemblyVersion.+?\\]", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                content = regex.Replace(content, versionString);

                string fileVersionString =
                    "[assembly: AssemblyFileVersion(\"" + fileVersion.ToString() + "\")]";
                regex = new Regex("\\[assembly:\\s*AssemblyFileVersion.+?\\]", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                content = regex.Replace(content, fileVersionString);

                if ( !string.IsNullOrEmpty( this.CompanyName ) )
                {
                    string replace = "[assembly: AssemblyCompany(\"" + this.CompanyName + "\")]";
                    string search = "\\[assembly:\\s*AssemblyCompany.+?\\]";
                    regex = new Regex( search, RegexOptions.Multiline | RegexOptions.IgnoreCase );
                    content = regex.Replace( content, replace );
                }

                if ( !string.IsNullOrEmpty( this.Copyright ) )
                {
                    string replace = "[assembly: AssemblyCopyright(\"" + this.Copyright + "\")]";
                    string search = "\\[assembly:\\s*AssemblyCopyright.+?\\]";
                    regex = new Regex( search, RegexOptions.Multiline | RegexOptions.IgnoreCase );
                    content = regex.Replace( content, replace );
                }

                if (!string.IsNullOrEmpty(this.ProductName))
                {
                    string replace = "[assembly: AssemblyProduct(\"" + this.ProductName + "\")]";
                    string search = "\\[assembly:\\s*AssemblyProduct.+?\\]";
                    regex = new Regex( search, RegexOptions.Multiline | RegexOptions.IgnoreCase );
                    content = regex.Replace( content, replace );
                }

                encodedFile.Write(content);
                LogWrite(file);
            }
        }

        private void UpdateAssemblyInfoFilesCPlusPlus(string[] files, Version version, Version fileVersion)
        {
            foreach (string file in files)
            {
                EncodedFile encodedFile = new EncodedFile(file);
                string content = encodedFile.Read();

                string versionString =
                    "[assembly: AssemblyVersionAttribute(\"" + version.ToString() + "\")]";

                Regex regex = new Regex( "\\[assembly:\\s*AssemblyVersionAttribute.+?\\]", RegexOptions.Multiline | RegexOptions.IgnoreCase );
                content = regex.Replace(content, versionString);

                string fileVersionString =
                    "[assembly: AssemblyFileVersionAttribute(\"" + fileVersion.ToString() + "\")]";

                regex = new Regex( "\\[assembly:\\s*AssemblyFileVersionAttribute.+?\\]", RegexOptions.Multiline | RegexOptions.IgnoreCase );
                content = regex.Replace(content, fileVersionString);

                if ( !string.IsNullOrEmpty( this.CompanyName ) )
                {
                    string replace = "[assembly: AssemblyCompanyAttribute(\"" + this.CompanyName + "\")]";
                    string search = "\\[assembly:\\s*AssemblyCompanyAttribute.+?\\]";
                    regex = new Regex( search, RegexOptions.Multiline | RegexOptions.IgnoreCase );
                    content = regex.Replace( content, replace );
                }

                if ( !string.IsNullOrEmpty( this.Copyright ) )
                {
                    string replace = "[assembly: AssemblyCopyrightAttribute(\"" + this.Copyright + "\")]";
                    string search = "\\[assembly:\\s*AssemblyCopyrightAttribute.+?\\]";
                    regex = new Regex( search, RegexOptions.Multiline | RegexOptions.IgnoreCase );
                    content = regex.Replace( content, replace );
                }

                if ( !string.IsNullOrEmpty( this.ProductName ) )
                {
                    string replace = "[assembly: AssemblyProductAttribute(\"" + this.ProductName + "\")]";
                    string search = "\\[assembly:\\s*AssemblyProductAttribute.+?\\]";
                    regex = new Regex( search, RegexOptions.Multiline | RegexOptions.IgnoreCase );
                    content = regex.Replace( content, replace );
                }

                encodedFile.Write(content);
                LogWrite(file);
            }
        }

        private void UpdateAssemblyInfoFilesVB(string[] files, Version version, Version fileVersion)
        {
            foreach (string file in files)
            {
                EncodedFile encodedFile = new EncodedFile(file);
                string content = encodedFile.Read();

                string versionString =
                    "<assembly: AssemblyVersion(\"" + version.ToString() + "\")>";

                Regex regex = new Regex("<assembly:\\s*AssemblyVersion.+?>", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                content = regex.Replace(content, versionString);

                string fileVersionString =
                    "<assembly: AssemblyFileVersion(\"" + fileVersion.ToString() + "\")>";
                regex = new Regex("\\<assembly:\\s*AssemblyFileVersion.+?\\>", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                content = regex.Replace(content, fileVersionString);

                if ( !string.IsNullOrEmpty( this.CompanyName ) )
                {
                    string replace = "<assembly: AssemblyCopyright(\"" + this.CompanyName + "\")>";
                    string search = "\\<assembly:\\s*AssemblyCopyright.+?\\>";
                    regex = new Regex( search, RegexOptions.Multiline | RegexOptions.IgnoreCase );
                    content = regex.Replace( content, replace );
                }

                if ( !string.IsNullOrEmpty( this.Copyright ) )
                {
                    string replace = "<assembly: AssemblyCompany(\"" + this.Copyright + "\")>";
                    string search = "\\<assembly:\\s*AssemblyCompany.+?\\>";
                    regex = new Regex( search, RegexOptions.Multiline | RegexOptions.IgnoreCase );
                    content = regex.Replace( content, replace );
                }

                if ( !string.IsNullOrEmpty( this.ProductName ) )
                {
                    string replace = "<assembly: AssemblyProduct(\"" + this.ProductName + "\")>";
                    string search = "\\<assembly:\\s*AssemblyProduct.+?\\>";
                    regex = new Regex( search, RegexOptions.Multiline | RegexOptions.IgnoreCase );
                    content = regex.Replace( content, replace );
                }

                encodedFile.Write(content);
                LogWrite(file);
            }
        }

        private void UpdateAssemblyInfoFiles(string path, Version version, Version fileVersion)
        {
            string[] files = Directory.GetFiles(path, "AssemblyInfo.cs");
            UpdateAssemblyInfoFilesCSharp(files, version, fileVersion);

            files = Directory.GetFiles(path, "AssemblyVersionInfo.cs");
            UpdateAssemblyInfoFilesCSharp(files, version, fileVersion);

            files = Directory.GetFiles(path, "AssemblyInfo.cpp");
            UpdateAssemblyInfoFilesCPlusPlus(files, version, fileVersion);

            files = Directory.GetFiles(path, "AssemblyVersionInfo.cpp");
            UpdateAssemblyInfoFilesCPlusPlus(files, version, fileVersion);

            files = Directory.GetFiles(path, "AssemblyInfo.vb");
            UpdateAssemblyInfoFilesVB(files, version, fileVersion);

            files = Directory.GetFiles(path, "AssemblyVersionInfo.vb");
            UpdateAssemblyInfoFilesVB(files, version, fileVersion);
        }

        private void UpdateVisualBasicProjectFiles(string path, Version version)
        {
            string[] files = Directory.GetFiles(path, "*.vbp");
            foreach (string file in files)
            {
                EncodedFile encodedFile = new EncodedFile(file);
                string content = encodedFile.Read();

                //MajorVer=4
                //MinorVer=0
                //RevisionVer=1

                Regex regex = new Regex("MajorVer=\\d+", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                content = regex.Replace(content, "MajorVer=" + version.Major.ToString());

                regex = new Regex("MinorVer=\\d+", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                content = regex.Replace(content, "MinorVer=" + version.Minor.ToString());

                regex = new Regex("RevisionVer=\\d+", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                content = regex.Replace(content, "RevisionVer=" + version.Revision.ToString());

                //if ( !string.IsNullOrEmpty( this.CompanyName ) )
                //{
                //    regex = new Regex( "VersionCompanyName=\\d+", RegexOptions.Multiline | RegexOptions.IgnoreCase );
                //    content = regex.Replace( content, "VersionCompanyName=\"" + this.CompanyName + "\"" );
                //}

                //if ( !string.IsNullOrEmpty( this.Copyright ) )
                //{
                //    regex = new Regex( "VersionLegalCopyright=\\d+", RegexOptions.Multiline | RegexOptions.IgnoreCase );
                //    content = regex.Replace( content, "VersionLegalCopyright=\"" + this.Copyright + "\"" );
                //}

                //if ( !string.IsNullOrEmpty( this.ProductName ) )
                //{
                //    regex = new Regex( "VersionProductName=.+?\\>\"", RegexOptions.Multiline | RegexOptions.IgnoreCase );
                //    content = regex.Replace( content, "VersionProductName=\"" + this.ProductName + "\"");
                //}

                encodedFile.Write(content);
                LogWrite(file);
            }
        }

        private void UpdateVersionInfoFiles(string path, Version version)
        {
            // *.vbp;AssemblyInfo.cs;VersionInfo.h;
            string[] files = Directory.GetFiles(path, "Version?Info.h");
            foreach (string file in files)
            {
                EncodedFile encodedFile = new EncodedFile(file);
                string content = encodedFile.Read();

                string versionString =
                    version.Major.ToString() +
                    "." +
                    version.Minor.ToString() +
                    "." +
                    version.Build.ToString() +
                    "." +
                    version.Revision.ToString();

                Regex regex = new Regex("#define\\s+FILE_MAJOR.+", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                content = regex.Replace(content, "#define FILE_MAJOR " + version.Major.ToString());

                regex = new Regex("#define\\s+FILE_MINOR.+", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                content = regex.Replace(content, "#define FILE_MINOR " + version.Minor.ToString());

                regex = new Regex("#define\\s+FILE_BUILD.+", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                content = regex.Replace(content, "#define FILE_BUILD " + version.Build.ToString());

                regex = new Regex("#define\\s+FILE_PRIVATE.+", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                content = regex.Replace(content, "#define FILE_PRIVATE " + version.Revision.ToString());

                regex = new Regex("#define\\s+FILE_VER_STRING.+", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                content = regex.Replace(content, "#define FILE_VER_STRING \"" + versionString + "\"");

                regex = new Regex("#define\\s+PRODUCT_MAJOR.+", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                content = regex.Replace(content, "#define PRODUCT_MAJOR " + version.Major.ToString());

                regex = new Regex("#define\\s+PRODUCT_MINOR.+", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                content = regex.Replace(content, "#define PRODUCT_MINOR " + version.Minor.ToString());

                regex = new Regex("#define\\s+PRODUCT_BUILD.+", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                content = regex.Replace(content, "#define PRODUCT_BUILD " + version.Build.ToString());

                regex = new Regex("#define\\s+PRODUCT_PRIVATE.+", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                content = regex.Replace(content, "#define PRODUCT_PRIVATE " + version.Revision.ToString());

                regex = new Regex("#define\\s+PRODUCT_VER_STRING.+", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                content = regex.Replace(content, "#define PRODUCT_VER_STRING \"" + versionString + "\"");

                if ( !string.IsNullOrEmpty( this.CompanyName ) )
                {
                    regex = new Regex( "#define\\s+COMPANYNAME.+", RegexOptions.Multiline | RegexOptions.IgnoreCase );
                    content = regex.Replace( content, "#define COMPANYNAME \"" + this.CompanyName + "\"" );
                }

                if ( !string.IsNullOrEmpty( this.Copyright ) )
                {
                    regex = new Regex( "#define\\s+COPYRIGHT.+", RegexOptions.Multiline | RegexOptions.IgnoreCase );
                    content = regex.Replace( content, "#define COPYRIGHT \"" + this.Copyright + "\"" );
                }

                if ( !string.IsNullOrEmpty( this.ProductName ) )
                {
                    regex = new Regex( "#define\\s+PRODUCTNAME.+", RegexOptions.Multiline | RegexOptions.IgnoreCase );
                    content = regex.Replace( content, "#define PRODUCTNAME \"" + this.ProductName + "\"" );
                }

				regex = new Regex("#define\\s+VINFO_HI_FILEVERSION.+", RegexOptions.Multiline | RegexOptions.IgnoreCase);
				content = regex.Replace(content, "#define VINFO_HI_FILEVERSION " + version.Major.ToString());

				regex = new Regex("#define\\s+VINFO_LO_FILEVERSION.+", RegexOptions.Multiline | RegexOptions.IgnoreCase);
				content = regex.Replace(content, "#define VINFO_LO_FILEVERSION " + version.Minor.ToString());

				regex = new Regex("#define\\s+VINFO_HI_FILEBUILD.+", RegexOptions.Multiline | RegexOptions.IgnoreCase);
				content = regex.Replace(content, "#define VINFO_HI_FILEBUILD " + version.Build.ToString());

				regex = new Regex("#define\\s+VINFO_LO_FILEBUILD.+", RegexOptions.Multiline | RegexOptions.IgnoreCase);
				content = regex.Replace(content, "#define VINFO_LO_FILEBUILD " + version.Revision.ToString());

				//regex = new Regex("#define\\s+FILE_VER_STRING.+", RegexOptions.Multiline | RegexOptions.IgnoreCase);
				//content = regex.Replace(content, "#define FILE_VER_STRING \"" + versionString + "\"");

				regex = new Regex("#define\\s+VINFO_HI_PRODUCTVERSION.+", RegexOptions.Multiline | RegexOptions.IgnoreCase);
				content = regex.Replace(content, "#define VINFO_HI_PRODUCTVERSION " + version.Major.ToString());

				regex = new Regex("#define\\s+VINFO_LO_PRODUCTVERSION.+", RegexOptions.Multiline | RegexOptions.IgnoreCase);
				content = regex.Replace(content, "#define VINFO_LO_PRODUCTVERSION " + version.Minor.ToString());

				regex = new Regex("#define\\s+VINFO_HI_PRODUCTBUILD.+", RegexOptions.Multiline | RegexOptions.IgnoreCase);
				content = regex.Replace(content, "#define VINFO_HI_PRODUCTBUILD " + version.Build.ToString());

				regex = new Regex("#define\\s+VINFO_LO_PRODUCTBUILD.+", RegexOptions.Multiline | RegexOptions.IgnoreCase);
				content = regex.Replace(content, "#define VINFO_LO_PRODUCTBUILD " + version.Revision.ToString());

				//regex = new Regex("#define\\s+PRODUCT_VER_STRING.+", RegexOptions.Multiline | RegexOptions.IgnoreCase);
				//content = regex.Replace(content, "#define PRODUCT_VER_STRING \"" + versionString + "\"");

				if (!string.IsNullOrEmpty(this.CompanyName))
				{
					regex = new Regex("#define\\s+VINFO_STRING_COMPANYNAME.+", RegexOptions.Multiline | RegexOptions.IgnoreCase);
					content = regex.Replace(content, "#define VINFO_STRING_COMPANYNAME \"" + this.CompanyName + "\"");
				}

				if (!string.IsNullOrEmpty(this.Copyright))
				{
					regex = new Regex("#define\\s+VINFO_STRING_COPYRIGHT.+", RegexOptions.Multiline | RegexOptions.IgnoreCase);
					content = regex.Replace(content, "#define VINFO_STRING_COPYRIGHT \"" + this.Copyright + "\"");
				}

				if (!string.IsNullOrEmpty(this.ProductName))
				{
					regex = new Regex("#define\\s+VINFO_STRING_PRODUCTNAME.+", RegexOptions.Multiline | RegexOptions.IgnoreCase);
					content = regex.Replace(content, "#define VINFO_STRING_PRODUCTNAME \"" + this.ProductName + "\"");
				}

                encodedFile.Write(content);
                LogWrite(file);
            }
        }

        private void LogWrite(string path)
        {
            Log.LogMessage("Version Updated: " + path.Substring(this.path.Length));
        }
    }
}
