using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using RandREng.MsBuildTasks.Acceptance;

namespace RandREng.MsBuildTasks
{
    public class AcceptanceTask : Task
    {
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

        public bool RestartTasks
        {
            get
            {
                return _Restart;
            }

            set
            {
                _Restart = value;
            }
        }
        private bool _Restart = false;

        public bool CreateNewBuildFolder
        {
            get
            {
                return _CreateNewBuildFolder;
            }
            set
            {
                _CreateNewBuildFolder = value;
            }
        }
        private bool _CreateNewBuildFolder = false;

        public bool FailCurrentTask
        {
            get
            {
                return _Fail;
            }
            set
            {
                _Fail = value;
            }
        }
        private bool _Fail = false;

        public bool StartNextTask
        {
            get
            {
                return _Start;
            }
            set
            {
                _Start = value;
            }
        }
        private bool _Start = false;

        public bool CompleteCurrentTask
        {
            get
            {
                return _Complete;
            }
            set
            {
                _Complete = value;
            }
        }
        private bool _Complete = false;
        #endregion

        #region Output Parameters
        [Output]
        public string StartedTask
        {
            get
            {
                return _StartedTask;
            }

            set
            {
                _StartedTask = value;
            }
        }
        private string _StartedTask = string.Empty;

        [Output]
        public string CompletedTask
        {
            get
            {
                return _CompletedTask;
            }

            set
            {
                _CompletedTask = value;
            }
        }
        private string _CompletedTask = string.Empty;

        [Output]
        public string FailedTask
        {
            get
            {
                return _FailedTask;
            }

            set
            {
                _FailedTask = value;
            }
        }
        private string _FailedTask = string.Empty;

        [Output]
        public string BuildFolder
        {
            get
            {
                return _BuildFolder;
            }

            set
            {
                _BuildFolder = value;
            }
        }
        private string _BuildFolder = string.Empty;
        #endregion

        public override bool Execute()
        {
            if (CreateNewBuildFolder)
            {
                DateTime now = DateTime.Now;
                BuildFolder = Path + "/build." + now.Month + "." + now.Day + "." + now.Year + "." + now.Hour + "." + now.Minute + "." + now.Second;
                Console.WriteLine("BuildFolder: {0}", BuildFolder);

                Directory.CreateDirectory(BuildFolder);
            }
            else
            {
                InitializeTaskManager();

                System.Console.WriteLine("FailCurrentTask: {0}", FailCurrentTask);
                System.Console.WriteLine("CompleteCurrentTask: {0}", CompleteCurrentTask);
                System.Console.WriteLine("StartNextTask: {0}", StartNextTask);

                if (!RestartTasks)
                {
                    if (FailCurrentTask)
                    {
                        FailTask();
                    }

                    if (CompleteCurrentTask)
                    {
                        CompleteTask();
                    }

                    if (StartNextTask)
                    {
                        StartTask();
                    }
                }
                else
                {
                    if (FailCurrentTask)
                    {
                        CompleteLastRunTask();
                    }

                    if (CompleteCurrentTask)
                    {
                        CompleteLastRunTask();
                    }

                    if (StartNextTask)
                    {
                        StartLastRunTask();
                    }
                }

                _TaskManager.FlushQueue();
            }
            return true;
        }

        private void InitializeTaskManager()
        {
            System.Console.WriteLine("Path: {0}", Path);

            _TaskManager = new TaskManager(Path);

            _TaskManager.RefreshQueue();
            _TaskManager.FlushQueue();
        }

        private void FailTask()
        {
            AcceptanceTaskItem item = _TaskManager.FailTask();

            if (item != null)
            {
                System.Console.WriteLine("FailedTask: {0}", item.Folder);

                _FailedTask = item.Folder;
            }
        }

        private void CompleteTask()
        {
            AcceptanceTaskItem item = _TaskManager.CompleteTask();

            if (item != null)
            {
                System.Console.WriteLine("CompletedTask: {0}", item.Folder);

                _CompletedTask = item.Folder;
            }
        }

        private void StartTask()
        {
            AcceptanceTaskItem item = _TaskManager.StartTask();

            if (item != null)
            {
                _StartedTask = item.Folder;
                System.Console.WriteLine("StartTask: {0}", _StartedTask);
            }            
        }

        private void CompleteLastRunTask()
        {
            AcceptanceTaskItem item = _TaskManager.CompleteLastRun();

            if (item != null)
            {                
                _CompletedTask = item.Folder;
                System.Console.WriteLine("CompleteLastRunTask: {0}", _CompletedTask);
            }
        }

        private void StartLastRunTask()
        {
            AcceptanceTaskItem item = _TaskManager.RestartLastRun();

            if (item != null)
            {
                _StartedTask = item.Folder;
                System.Console.WriteLine("StartLastRunTask: {0}", _StartedTask);
            }
        }

        TaskManager _TaskManager = null;
    }
}
