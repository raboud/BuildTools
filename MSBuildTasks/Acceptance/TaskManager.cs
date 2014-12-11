using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace RandREng.MsBuildTasks.Acceptance
{
    public class TaskManager
    {
        public TaskManager(string path)
        {
            _Path = path;
            InitializeQueue();
            RefreshQueue();
        }

        #region Utilities
        private void InitializeQueue()
        {
            if (File.Exists(_Path + Path.DirectorySeparatorChar + FILE))
            {
                _Queue = TaskQueue.LoadFromFile(_Path, FILE);
            }
            else
            {
                _Queue = TaskQueue.EmptyQueue;

                AcceptanceTaskItem lastTask = new AcceptanceTaskItem();
                lastTask.Folder = LastRunFolder;
                lastTask.Id = -1;
                lastTask.State = TaskState.LastRunNever;

                _Queue.Tasks.Add(lastTask);
                TaskQueue.SaveToFile(_Path, FILE, _Queue);
            }
        }
        #endregion

        #region Methods
        public AcceptanceTaskItem RestartLastRun()
        {
            AcceptanceTaskItem rc = null;

            if (_Queue.Tasks.Count > 0)
            {
                if (!IsTaskRunning && IsLastRunAvailable)
                {
                    if (!IsLastRunning)
                    {
                        rc = LastRunTask;
                        rc.State = TaskState.LastRunPending;
                    }
                }
            }

            return rc;
        }

        public AcceptanceTaskItem CompleteLastRun()
        {
            AcceptanceTaskItem rc = null;

            if (_Queue.Tasks.Count > 0)
            {
                if (IsLastRunning)
                {
                    rc = LastRunTask;
                    rc.State = TaskState.LastRunComplete;
                }
            }

            return rc;
        }

        public AcceptanceTaskItem LastRunTask
        {
            get
            {
                List<AcceptanceTaskItem> rc = this[TaskState.LastRunComplete];
                rc.AddRange(this[TaskState.LastRunPending]);
                rc.AddRange(this[TaskState.LastRunNever]);

                return (rc.Count > 0) ? rc[0] : null;
            }
        }

        public bool IsLastRunAvailable
        {
            get
            {
                return (LastRunTask != null && LastRunTask.State == TaskState.LastRunComplete);
            }
        }

        public bool IsLastRunning
        {
            get
            {
                return (LastRunTask != null && LastRunTask.State == TaskState.LastRunPending);
            }
        }

        public AcceptanceTaskItem RestartFailedTask()
        {
            AcceptanceTaskItem rc = null;

            if (_Queue.Tasks.Count > 0)
            {
                if (!IsTaskRunning)
                {
                    if (IsTaskFailed)
                    {
                        rc = FailedTasks[0];

                        foreach (AcceptanceTaskItem task in FailedTasks)
                        {
                            if (task.Id < rc.Id)
                            {
                                rc = task;
                            }
                        }
                    }
                }
            }

            if (rc != null)
            {
                rc.State = TaskState.Running;
            }

            return rc;
        }

        public AcceptanceTaskItem StartTask()
        {
            AcceptanceTaskItem rc = null;

            if (_Queue.Tasks.Count > 0)
            {
                if (!IsTaskRunning)
                {
                    if (IsTaskQueued)
                    {
                        rc = QueuedTasks[0];

                        foreach (AcceptanceTaskItem task in QueuedTasks)
                        {
                            if (task.Id < rc.Id)
                            {
                                rc = task;
                            }
                        }
                    }
                }
            }

            if (rc != null)
            {
                rc.State = TaskState.Running;
            }

            return rc;
        }

        public AcceptanceTaskItem FailTask()
        {
            AcceptanceTaskItem rc = null;

            if (IsTaskRunning)
            {
                rc = RunningTask;
                rc.State = TaskState.Failed;
            }

            return rc;
        }

        public AcceptanceTaskItem CompleteTask()
        {
            AcceptanceTaskItem rc = null;

            if (IsTaskRunning)
            {
                rc = RunningTask;
                rc.State = TaskState.Complete;

                if(Directory.Exists(LastRunFolder))
                {
                    Directory.Delete(LastRunFolder, true);
                }

                Directory.Move(rc.Folder, LastRunFolder);
                File.SetAttributes(LastRunFolder + Path.DirectorySeparatorChar + MARKER, FileAttributes.Normal);
                File.Delete(LastRunFolder + Path.DirectorySeparatorChar + MARKER);

                LastRunTask.State = TaskState.LastRunComplete;
            }

            return rc;
        }

        private string LastRunFolder
        {
            get
            {
                return _Path + "/complete";
            }
        }

        public bool IsTaskRunning
        {
            get
            {
                bool rc = (RunningTask != null);
                rc = (rc || IsLastRunning);

                return rc;
            }
        }

        public bool IsTaskQueued
        {
            get
            {
                bool rc = (QueuedTasks.Count > 0);
                return rc;
            }
        }

        public bool IsTaskFailed
        {
            get
            {
                bool rc = (FailedTasks.Count > 0);
                return rc;
            }
        }

        private List<AcceptanceTaskItem> this[TaskState index]
        {
            get
            {
                List<AcceptanceTaskItem> rc = new List<AcceptanceTaskItem>();

                foreach (AcceptanceTaskItem task in _Queue.Tasks)
                {
                    if (task.State == index)
                    {
                        rc.Add(task);
                    }
                }

                return rc;
            }
        }

        public List<AcceptanceTaskItem> FailedTasks
        {
            get
            {
                return this[TaskState.Failed];
            }
        }

        public List<AcceptanceTaskItem> QueuedTasks
        {
            get
            {
                return this[TaskState.Queued];
            }
        }

        public AcceptanceTaskItem RunningTask
        {
            get
            {
                AcceptanceTaskItem rc = null;

                foreach (AcceptanceTaskItem task in _Queue.Tasks)
                {
                    if (task.State == TaskState.Running)
                    {
                        rc = task;
                        break;
                    }
                }

                return rc;
            }
        }

        public void RefreshQueue()
        {
            string[] folders = Directory.GetDirectories(_Path, "*", SearchOption.TopDirectoryOnly);

            foreach (string folder in folders)
            {
                if (File.Exists(folder + Path.DirectorySeparatorChar + MARKER))
                {
                    _Queue.PushTask(folder);
                }
            }
        }

        public void FlushQueue()
        {
            TaskQueue.SaveToFile(_Path, FILE, _Queue);
        }
        #endregion

        #region Properties
        private static string FILE = "queue.xml";
        private static string MARKER = "directory.marker";
        private string _Path = string.Empty;
        private TaskQueue _Queue;
        #endregion
    }
}
