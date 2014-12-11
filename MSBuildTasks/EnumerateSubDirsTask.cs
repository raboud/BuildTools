using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.IO;

namespace RandREng.MsBuildTasks 
{
    public class EnumerateSubDirsTask : Task
    {
        string _rootDir;

        [Required]
        public string RootPath
        {
            get { return _rootDir; }
            set { _rootDir = value; }
        }

        private ITaskItem[] _subDirs;

        [Output]
        public ITaskItem[] SubDirectories
        {
            get { return _subDirs; }
        }

        public override bool Execute()
        {
            string[] dirs=Directory.GetDirectories(_rootDir);
            if (dirs.Length == 0)
            {
                _subDirs = new ITaskItem[0];
                return true;
            }
            _subDirs=new ITaskItem[dirs.Length];

            ArrayList results=new ArrayList();
            for(int i=0;i<dirs.Length;i++)
            {
                _subDirs[i]=new TaskItem(dirs[i]);
            }
            return true;

            
        }
    
    }
}
