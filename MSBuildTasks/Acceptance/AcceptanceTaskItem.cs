using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace RandREng.MsBuildTasks.Acceptance
{
    [XmlRoot("task")]
    public class AcceptanceTaskItem
    {
        public AcceptanceTaskItem()
        {
        }

        [XmlAttribute("id")]
        public int Id
        {
            get
            {
                return _Id;
            }

            set
            {
                _Id = value;
            }
        }
        private int _Id;

        [XmlAttribute("folder")]
        public string Folder
        {
            get
            {
                return _Folder;
            }

            set
            {
                _Folder = value;
            }
        }
        private string _Folder;

        [XmlAttribute("state")]
        public TaskState State
        {
            get
            {
                return _State;
            }

            set
            {
                _State = value;
            }
        }
        private TaskState _State = TaskState.Queued;
    }
}
