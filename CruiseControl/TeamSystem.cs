using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Net;

using Exortech.NetReflector;
using ThoughtWorks.CruiseControl.Core.Util;

using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.Client;
using ThoughtWorks.CruiseControl.Core;

namespace RandREng.CCNet.Plugin
{
    /// <summary>
    ///   Source Control Plugin for CruiseControl.NET that talks to VSTS Team Foundation Server.
    /// </summary>
    [ReflectorType("vsTeamSystem")]
    public class TeamSystem : ISourceControl
    {
        private const string DEFAULT_WORKSPACE_NAME = "CCNET";
        private const string DEFAULT_WORKSPACE_COMMENT = "Temporary CruiseControl.NET Workspace";

        /// <summary>
        ///   The name or URL of the team foundation server.  For example http://vstsb2:8080 or vstsb2 if it
        ///   has already been registered on the machine.
        /// </summary>
        [ReflectorProperty("server")]
        public string Server;

        /// <summary>
        ///   Array of SourceProject objects
        /// </summary>
        [ReflectorArray("sourceProjects")]
        public SourceProject[] SourceProjects;

        /// <summary>
        ///   Username that should be used.  Domain cannot be placed here, rather in domain property.
        /// </summary>
        [ReflectorProperty("username", Required = false)]
        public string Username;

        /// <summary>
        ///   The password in clear test of the domain user to be used.
        /// </summary>
        [ReflectorProperty("password", Required = false)]
        public string Password;

        /// <summary>
        ///  The domain of the user to be used.
        /// </summary>
        [ReflectorProperty("domain", Required = false)]
        public string Domain;

        /// <summary>
        ///   Name of the workspace to create.  This will revert to the DEFAULT_WORKSPACE_NAME if not passed.
        /// </summary>
        [ReflectorProperty("workspace", Required = false)]
        public string Workspace
        {
            get
            {
                if (workspaceName == null)
                {
                    workspaceName = DEFAULT_WORKSPACE_NAME;
                }
                return workspaceName;
            }
            set
            {
                workspaceName = value;
            }
        }

        /// <summary>
        ///   Flag indicating if workspace should be deleted every time or if it should be 
        ///   left (the default).  Leaving the workspace will mean that subsequent gets 
        ///   will only need to transfer the modified files, improving performance considerably.
        /// </summary>
        [ReflectorProperty("deleteWorkspace", Required = false)]
        public bool DeleteWorkspace = false;

        public Modification[] GetModifications(IIntegrationResult from, IIntegrationResult to)
        {
            Log.Debug("Checking Team Foundation Server for Modifications");
            Log.Debug("From: " + from.StartTime + " - To: " + to.StartTime);

            VersionSpec fromVersion = new DateVersionSpec(from.StartTime);
            VersionSpec toVersion = new DateVersionSpec(to.StartTime);

            List<Modification> modifications = new List<Modification>();

            foreach (SourceProject project in this.SourceProjects)
            {
                GetModifications(project.ProjectPath, fromVersion, toVersion, modifications);
            }

            Log.Debug(string.Format("Found {0} modifications", modifications.Count));

            return modifications.ToArray();
        }

        public void LabelSourceControl(IIntegrationResult result)
        {
            // This is called after the GetSource call, so is really no good.

            //Log.Debug(String.Format("Applying label \"{0}\"", result.Label));
            //foreach (SourceProject project in this.SourceProjects)
            //{
            //    PerformLabel(project, result);
            //}
        }

        public void GetSource(IIntegrationResult result)
        {
            foreach (SourceProject project in this.SourceProjects)
            {
                PerformLabel(project, result);
            }

            foreach (SourceProject project in this.SourceProjects)
            {
                GetSource(project, result);
            }
        }

        public void Initialize(IProject project)
        {
            // Do Nothing
        }

        public void Purge(IProject project)
        {
            // Not sure when this is called.
        }

        private void GetSource(SourceProject project, IIntegrationResult result)
        {
            if (project.CleanCopy)
            {
                // If we have said we want a clean copy, then delete old copy before getting.
                Log.Debug("Deleting " + project.WorkingDirectory);
                DeleteDirectory(project.WorkingDirectory);
            }

            Workspace[] workspaces = this.SourceControl.QueryWorkspaces(Workspace, this.SourceControl.AuthorizedUser, Workstation.Current.Name);
            Workspace workspace = null;

            if (workspaces.Length > 0)
            {
                // The workspace exists.  
                if (DeleteWorkspace)
                {
                    // We have asked for a new workspace every time, therefore delete the existing one.
                    Log.Debug("Removing existing workspace " + Workspace);
                    this.SourceControl.DeleteWorkspace(Workspace, this.SourceControl.AuthorizedUser);
                    workspaces = new Workspace[0];
                }
                else
                {
                    Log.Debug("Existing workspace detected - reusing");
                    workspace = workspaces[0];
                }
            }
            if (workspaces.Length == 0)
            {
                Log.Debug("Creating new workspace name: " + Workspace);
                workspace = this.SourceControl.CreateWorkspace(Workspace, this.SourceControl.AuthorizedUser, DEFAULT_WORKSPACE_COMMENT);
            }

            try
            {
                workspace.Map(project.ProjectPath, project.WorkingDirectory);

                Log.Debug(String.Format("Getting {0} to {1}", project.ProjectPath, project.WorkingDirectory));

                VersionSpec verSpec = LatestVersionSpec.Instance;
                if (project.ApplyLabel)
                    verSpec = new LabelVersionSpec(result.Label);
                else if (!String.IsNullOrEmpty(project.Label))
                    verSpec = new LabelVersionSpec(project.Label);

                GetRequest getReq = new GetRequest(new ItemSpec(project.ProjectPath, RecursionType.Full), verSpec);

                if (project.CleanCopy)
                {
                    Log.Debug("Forcing a Get Specific with the options \"get all files\" and \"overwrite read/write files\"");
                    workspace.Get(getReq, GetOptions.GetAll | GetOptions.Overwrite);
                }
                else
                {
                    Log.Debug("Performing a Get Latest");
                    workspace.Get(getReq, GetOptions.None);
                }
            }
            finally
            {
                if (workspace != null && DeleteWorkspace)
                {
                    Log.Debug("Deleting the workspace");
                    workspace.Delete();
                }
            }
        }

        private void GetModifications(string projectPath, VersionSpec fromVersion, VersionSpec toVersion, List<Modification> modifications)
        {
            IEnumerable changesets = this.SourceControl.QueryHistory(
                projectPath, VersionSpec.Latest, 0, RecursionType.Full, null, fromVersion, toVersion, Int32.MaxValue, true, false);

            // Each changeset contains multiple file modifications.  
            // Build up array of all CCNET modifications from all changesets.
            foreach (Changeset changeset in changesets)
            {
                GetModifcations(changeset, modifications);
            }
        }

        /// <summary>
        ///   Convert the passed changeset to an array of modifcations.
        /// </summary>
        private void GetModifcations(Changeset changeset, List<Modification> modifications)
        {
            string userName = changeset.Committer;
            string comment = changeset.Comment;
            int changeNumber = changeset.ChangesetId;

            // In VSTS, the version of the file is the same as the changeset number it was checked in with.
            string version = Convert.ToString(changeNumber);

            DateTime modifedTime = this.TFS.TimeZone.ToLocalTime(changeset.CreationDate);

            foreach (Change change in changeset.Changes)
            {
                Modification modification = new Modification();
                modification.UserName = userName;
                modification.Comment = comment;
                modification.ChangeNumber = changeNumber.ToString();
                modification.ModifiedTime = modifedTime;
                modification.Version = version;
                modification.Type = PendingChange.GetLocalizedStringForChangeType(change.ChangeType);

                // Populate fields from change item
                Item item = change.Item;
                if (item.ItemType == ItemType.File)
                {
                    // split into foldername and filename
                    int lastSlash = item.ServerItem.LastIndexOf('/');
                    modification.FileName = item.ServerItem.Substring(lastSlash + 1);
                    // patch to the following line submitted by Ralf Kretzschmar.
                    modification.FolderName = item.ServerItem.Substring(0, lastSlash);
                }
                else
                {
                    // TODO - what should filename be if dir??  Empty string or null?
                    modification.FileName = string.Empty;
                    modification.FolderName = item.ServerItem;
                }

                modifications.Add(modification);
            }
        }

        private void PerformLabel(SourceProject project, IIntegrationResult result)
        {
            if (project.ApplyLabel)
            {
                Log.Debug(String.Format("Applying label \"{0}\" on project \"{1}\"", result.Label, project.ProjectPath));

                string comment = result.StartTime.ToString() + " on " + project.ProjectPath;

                VersionControlLabel vcLabel = new VersionControlLabel(
                    this.SourceControl, result.Label, this.SourceControl.AuthorizedUser,
                    project.ProjectPath, comment);

                // Create Label Item Spec.
                ItemSpec itemSpec = new ItemSpec(project.ProjectPath, RecursionType.Full);

                LabelItemSpec[] labelItemSpec = new LabelItemSpec[] {  
                    new LabelItemSpec(itemSpec, new DateVersionSpec(result.StartTime), false)};

                this.SourceControl.CreateLabel(vcLabel, labelItemSpec, LabelChildOption.Replace);
            }
        }

        private void DeleteDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    MarkAllFilesReadWrite(path);
                    Directory.Delete(path, true);
                }
            }
            catch (Exception ex)
            {
                Log.Warning(String.Format("Unable to delete directory {0}.  The exception was {1}", path, ex.Message));
            }
        }

        private void MarkAllFilesReadWrite(string path)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(path);

            FileInfo[] files = dirInfo.GetFiles();
            foreach (FileInfo file in files)
            {
                file.IsReadOnly = false;
            }

            // Now recurse down the directories
            DirectoryInfo[] dirs = dirInfo.GetDirectories();
            foreach (DirectoryInfo dir in dirs)
            {
                this.MarkAllFilesReadWrite(dir.FullName);
            }
        }

        /// <summary>
        ///   Cached instance of TeamFoundationServer.
        /// </summary>
        private TfsTeamProjectCollection TFS
        {
            get
            {
                if (null == teamFoundationServer)
                {
                    teamFoundationServer = new TfsTeamProjectCollection(new Uri(this.Server), this.Credentials);
                }
                return teamFoundationServer;
            }
        }

        /// <summary>
        ///   Network credentials used to interact with TFS.
        /// </summary>
        private NetworkCredential Credentials
        {
            get
            {
                if (null == networkCredential)
                {
                    if (Username != null && Password != null)
                    {
                        if (Domain != null)
                            networkCredential = new NetworkCredential(Username, Password, Domain);
                        else
                            networkCredential = new NetworkCredential(Username, Password);
                    }
                    else
                    {
                        networkCredential = CredentialCache.DefaultNetworkCredentials;
                    }
                }
                return networkCredential;
            }
        }

        /// <summary>
        ///   The cached instace of the SourceControl object that we are connected to.
        /// </summary>
        private VersionControlServer SourceControl
        {
            get
            {
                if (null == sourceControl)
                {
                    sourceControl = (VersionControlServer)this.TFS.GetService(typeof(VersionControlServer));
                }
                return sourceControl;
            }
        }

        private string workspaceName = null;
        private VersionControlServer sourceControl = null;
        private NetworkCredential networkCredential = null;
        private TfsTeamProjectCollection teamFoundationServer = null;
    }
    [ReflectorType("sourceProject")]
    public class SourceProject
    {
        [ReflectorProperty("projectPath", Required = true)]
        public string ProjectPath;

        [ReflectorProperty("workingDirectory", Required = true)]
        public string WorkingDirectory;

        public bool applyLabel = false;
        [ReflectorProperty("applyLabel", Required = false)]
        public bool ApplyLabel
        {
            get
            {
                if (String.IsNullOrEmpty(this.Label))
                    return this.applyLabel;
                else
                    return false;

            }
            set
            {
                this.applyLabel = value;
            }
        }

        [ReflectorProperty("cleanCopy", Required = false)]
        public bool CleanCopy = false;

        [ReflectorProperty("getByLabel", Required = false)]
        public string Label = null;
    }
}
