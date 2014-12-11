using System;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace RandREng.MsBuildTasks
{
    public class RegexTask : Task
    {
        private string pattern = null;
        private string input = null;
        private string capture = null;
        private string matchGroup = null;
        private string fileName = null;
        private string backRefName = null;

        [Required]
        public string Pattern
        {
            get { return pattern; }
            set { pattern = value; }
        }

        public string Input
        {
            get { return input; }
            set { input = value; }
        }

        public string MatchGroup
        {
            get { return matchGroup; }
            set { matchGroup = value; }
        }

        public string FileName
        {
            get { return this.fileName; }
            set { this.fileName = value; }
        }

        public string BackRefName
        {
            get { return this.backRefName; }
            set { this.backRefName = value; }
        }

        [Output]
        public string Capture
        {
            get { return capture; }
            set { capture = value; }
        }

        public override bool Execute()
        {
            Regex regex = new Regex(this.pattern);

            if (string.IsNullOrEmpty(input) && string.IsNullOrEmpty(fileName))
            {
                Log.LogError("Must specify Input or Filename");
                return false;
            }

            if (!string.IsNullOrEmpty(fileName))
            {
                if (System.IO.File.Exists(fileName))
                {
                    try
                    {
                        using (System.IO.TextReader streamReader = new System.IO.StreamReader(fileName))
                        {
                            this.input = streamReader.ReadToEnd();
                        }
                    }
                    catch (Exception e)
                    {
                        Log.LogError(string.Format("Error Reading File {0} {1}", e.ToString(), e.Message));
                        return false;
                    }
                }
                else
                {
                    Log.LogError("File does not exist");
                    return false;
                }
            }
            try
            {
                Match match = regex.Match(this.input);
                if (null != match && match.Success)
                {
                    if (!string.IsNullOrEmpty(this.matchGroup))
                    {
                        this.capture = match.Groups[this.matchGroup].Value;
                    }
                    else if (!string.IsNullOrEmpty(this.backRefName))
                    {
                        this.capture = match.Result("${" + backRefName + "}");
                    }
                    else
                    {
                        this.capture = match.Value;
                    }
                }
            }
            catch
            {
                Log.LogError("Error in regex evaluation");
                return false;
            }
            return true;
        }
    }
}
