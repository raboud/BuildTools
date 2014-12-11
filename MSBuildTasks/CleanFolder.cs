using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace RandREng.MsBuildTasks
{
    public class CleanFolder : Task
    {
        public CleanFolder()
        {
        }

        #region Input Parameters
        [Required]
        public string Path
        {
            get
            {
                return _Path;
            }
            set
            {
                _Path = value;
            }
        }
        private string _Path = string.Empty;
        #endregion

        public override bool Execute()
        {
            if (Directory.Exists(Path))
            {
                foreach (string file in Directory.GetFiles(Path, "*", SearchOption.AllDirectories))
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }

                foreach(string directory in Directory.GetDirectories(Path, "*", SearchOption.TopDirectoryOnly))
                {
                    Directory.Delete(directory, true);                    
                }
            }
            return true;
        }
    }
}
