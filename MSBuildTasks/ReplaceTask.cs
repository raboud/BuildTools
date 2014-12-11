using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace RandREng.MsBuildTasks
{
    public class ReplaceTask : Task
    {
        private string path = null;
        private string find = null;
        private string replace = null;

        [Required]
        public string Path
        {
            get { return path; }
            set { path = value; }
        }

        [Required]
        public string Find
        {
            get { return find; }
            set { find = value; }
        }

        [Required]
        public string Replace
        {
            get { return replace; }
            set { replace = value; }
        }

        public override bool Execute()
        {
            try
            {
                EncodedFile encodedFile = new EncodedFile(this.path);
                string content = encodedFile.Read();
                content = content.Replace(this.find, this.replace);
                encodedFile.Write(content);                
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError(ex.Message);
            }
            return false;
        }
    }
}
