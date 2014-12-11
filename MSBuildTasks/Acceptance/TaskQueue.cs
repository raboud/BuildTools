using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace RandREng.MsBuildTasks.Acceptance
{
    [XmlRoot("queue")]
    public class TaskQueue
    {
        public TaskQueue()
        {
        }

        #region Methods
        public AcceptanceTaskItem PushTask(string folder)
        {
            AcceptanceTaskItem rc = null;

            if (!TaskExists(folder))
            {
                rc = new AcceptanceTaskItem();

                int id = -1;
                foreach (AcceptanceTaskItem task in _Tasks)
                {
                    if (task.Id > id)
                    {
                        id = task.Id;
                    }
                }

                id++;
                rc.Id = id;
                rc.Folder = Path.GetFullPath(folder);
                rc.State = TaskState.Queued;

                _Tasks.Add(rc);
            }

            return rc;
        }

        public bool TaskExists(string folder)
        {
            bool rc = false;

            foreach (AcceptanceTaskItem task in _Tasks)
            {
                if (task.Folder == Path.GetFullPath(folder))
                {
                    rc = true;
                    break;
                }
            }

            return rc;
        }
        #endregion

        #region Properties
        [XmlElement("task")]
        public List<AcceptanceTaskItem> Tasks
        {
            get
            {
                return _Tasks;
            }

            set
            {
                _Tasks = value;
            }
        }
        private List<AcceptanceTaskItem> _Tasks = new List<AcceptanceTaskItem>();

        [XmlIgnore]
        public static TaskQueue EmptyQueue
        {
            get
            {
                TaskQueue rc = new TaskQueue();
                return rc;
            }
        }
        #endregion

        #region Utility
        public static TaskQueue LoadFromFile(string path, string file)
        {
            string xml = File.ReadAllText(path + Path.DirectorySeparatorChar + file);
            return Serializer.Deserialize<TaskQueue>(xml);
        }

        public static void SaveToFile(string path, string file, TaskQueue queue)
        {
            string xml = Serializer.Serialize(queue);
            File.WriteAllText(path + Path.DirectorySeparatorChar + file, xml);
        }
        #endregion
    }
}
