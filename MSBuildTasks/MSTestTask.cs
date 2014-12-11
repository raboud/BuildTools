/*------------------------------------------------------------------------------
 * Bugslayer Column   -  MSDN Magazine  -  John Robbins  -  john@wintellect.com
 * 
 * 
 * The original article about this tool can be found at:
 * http://msdn.microsoft.com/msdnmag/issues/06/03/Bugslayer/default.aspx
 * 
 * 
 * ---------------------------------------------------------------------------*/

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Globalization;
using System.IO;
using System.Xml.XPath;
using Microsoft.Win32;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using StringDictionary = System.Collections.Generic.Dictionary<string , string>;

namespace RandREng.MsBuildTasks
{
    /// <summary>
    /// A task for running MSTEST.EXE.
    /// </summary>
    /// <remarks>
    /// Note that this task will use timestamps in the same way as MSTEST.EXE 
    /// does.  However, since this class is getting the time before calling
    /// MSTEST.EXE it's possible that the time associated with the 
    /// <see cref="ResultsFile"/> is a second or two before the slop directory 
    /// time.
    /// </remarks>
    public class MSTestTask : ToolTask
    {
        #region Constants

        const String win32KeyName2010 = @"SOFTWARE\Microsoft\VisualStudio\10.0";
        const String win64KeyName2010 = @"SOFTWARE\Wow6432Node\Microsoft\VisualStudio\10.0";
        const String win32KeyName2008 = @"SOFTWARE\Microsoft\VisualStudio\9.0";
        const String win64KeyName2008 = @"SOFTWARE\Wow6432Node\Microsoft\VisualStudio\9.0";
        const String installDirValue = @"InstallDir";
        const String toolName = "MSTEST.EXE";
        
        #endregion        
        
        #region Private Variables
        // Holds the path to the VS.NET install directory.
        private String toolPath;
        // Holds the simple string properties.
        StringDictionary stringDictionary;
        // Holds the list of test containers.
        private ITaskItem [] testContainers;
        // The list of tests lists to run in metadata files.
        private ITaskItem [] testListLists;
        // The list of tests from a container or metadata file to run.
        private ITaskItem [] testList;
        // The list of detail items requested.
        private ITaskItem [] detailList;
        // The unique name value.
        private Boolean uniqueName;
        // The /noisolation switch value.
        private Boolean noIsolation;
        // Use the version of MSTest that came with VS2008.
        private Boolean useTest2008;
        // The final result file name.
        private String finalResultFile;
        #endregion

        #region Construction and Tool Location Setup.
        /// <summary>
        /// Constructs an instance of the <see cref="MSTestTask"/> class.
        /// </summary>
        public MSTestTask ( )
        {
			// Set the resource manager for this class.
            //this.TaskResources = Resources.ResourceManager;

            // Always allocate the dictionary for simple properties.
            stringDictionary = new StringDictionary();
            // Set the default unique name value.
            uniqueName = true;
            finalResultFile = String.Empty;

        }
		protected override int ExecuteTool(string pathToTool, string responseFileCommands, string commandLineCommands)
		{
			int results;
			results = base.ExecuteTool(pathToTool, responseFileCommands, commandLineCommands);
			return results;
		}
        public override bool Execute()
        {
			bool bRet = false;
			try
			{
				this.GenerateFullPathToTool();
				bRet = base.Execute();
			}
			catch (System.Exception ex)
			{
				Log.LogErrorFromException(ex, true);
			}
			return bRet;
		}

        /// <summary>
        /// Returns the path to the tool.
        /// </summary>
        /// <returns>
        /// The path to the tool.  If it does not exist, null.
        /// </returns>
        protected override string GenerateFullPathToTool ( )
        {
            string win64KeyName = win64KeyName2010;
            string win32KeyName = win32KeyName2010;

            if (this.UseTest2008)
            {
                win64KeyName = win64KeyName2008;
                win32KeyName = win32KeyName2008;
            }

            String foundPath = null;
            RegistryKey reg = null;
            try
            {
                // First look in the Win64 place.
                reg = Registry.LocalMachine.OpenSubKey ( win64KeyName );
                if ( null == reg )
                {
                    // Try the Win32 key.
                    reg = Registry.LocalMachine.OpenSubKey ( win32KeyName );
                }
                if ( null != reg )
                {
                    // Read the ProductDir string value.
                    String dir = reg.GetValue ( installDirValue ,
                                                String.Empty ) as String;
                    if ( dir.Length > 0 )
                    {
                        this.ToolPath = dir;
                        // Poke on the tool name.
                        foundPath = dir + toolName;
                    }
                }
            }
            finally
            {
                if ( null != reg )
                {
                    reg.Close ( );
                }
            }
            // Set the master tool location.
            toolPath = foundPath;
            return ( toolPath );
        }

        /// <summary>
        /// Returns the name of this tool.
        /// </summary>
        protected override string ToolName
        {
            get { return ( toolName ); }
        } 
        #endregion

        #region Public Properties

        [Output]
        public string FinalResultsFile
        {
            get { return finalResultFile; }
        }
        /// <summary>
        /// Gets or sets the test meta data file to use.
        /// </summary>
        /// <remarks>
        /// You have to set either <see cref="TestMetaData"/> or 
        /// <see cref="TestContainers"/> for this task to work.
        /// </remarks>
        public String TestMetaData
        {
            get { return ( GetKey ( "TestMetaData" ) ); }
            set { stringDictionary [ "TestMetaData" ] = value; }
        }

        /// <summary>
        /// Gets or sets the list of items you can pass to the 
        /// /testcontainer: 
        /// </summary>
        /// <remarks>
        /// You have to set either <see cref="TestMetaData"/> or 
        /// <see cref="TestContainers"/> for this task to work.
        /// </remarks>
        public ITaskItem [] TestContainers
        {
            get { return ( testContainers ); }
            set { testContainers = value; }
        }

        /// <summary>
        /// Gets or sets the file passed to /runconfig:
        /// </summary>
        /// <remarks>
        /// See <see cref="ResultsFile"/> for a discussion on how this file is
        /// inspected to form the final name.
        /// </remarks>
        public String RunConfig
        {
            get { return ( GetKey ( "RunConfig" ) ); }
            set 
			{
				stringDictionary [ "RunConfig" ] = value; 
			}
        }

        /// <summary>
        /// Gets or sets the unique naming of <see cref="ResultsFile"/> 
        /// if <see cref="RunConfig"/> is not used.
        /// </summary>
        /// The default value is true.
        /// <para>
        /// If true, a date and timestamp is appended to the name of the 
        /// <see cref="ResultsFile"/>.
        /// </para>
        /// <remarks>
        /// </remarks>
        public Boolean UniqueName
        {
            get { return ( uniqueName ); }
            set { uniqueName = value; }
        }

        /// <summary>
        /// Gets or sets the name of the output results file.
        /// </summary>
        /// <remarks>
        /// If a file extension is not specified, the standard .TRX will be 
        /// applied.
        /// <para>
        /// If directory information is present in the name, the directory will
        /// be created if it does not exist.
        /// </para>
        /// <para>
        /// If <see cref="UniqueName"/> is set to true, a date and timestamp 
        /// will be applied to the filename.
        /// </para>
        /// <para>
        /// If a <see cref="RunConfig"/> file is specified, that file will be 
        /// examined for timestamp and naming conventions.  The 
        /// <see cref="UniqueName"/> value is ignored.
        /// </para>
        /// </remarks>
        [Required]
        public String ResultsFile
        {
            get { return ( GetKey ( "ResultsFile" ) ); }
            set { stringDictionary [ "ResultsFile" ] = value; }
        }

        /// <summary>
        /// The meta data test lists to execute with /testlist:
        /// </summary>
        /// <remarks>
        /// This switch can only be used if <see cref="TestMetaData"/> is set.
        /// </remarks>
        public ITaskItem [] TestLists
        {
            get { return ( testListLists ); }
            set { testListLists = value; }
        }

        /// <summary>
        /// The tests to execute from a metadata file or test container.
        /// </summary>
        public ITaskItem [] Tests
        {
            get { return ( testList ); }
            set { testList = value; }
        }

        /// <summary>
        /// The value for the /noisolation switch.
        /// </summary>
        /// <remarks>
        /// Defaults to false.
        /// </remarks>
        public Boolean NoIsolation
        {
            get { return ( noIsolation ); }
            set { noIsolation = value; }
        }

        /// <summary>
        /// Any /detail options requested in the output.
        /// </summary>
        public ITaskItem [] Details
        {
            get { return ( detailList ); }
            set { detailList = value; }
        }

        /// <summary>
        /// Publish results to the Team Foundation Server.
        /// </summary>
        public String Publish
        {
            get { return ( GetKey ( "Publish" ) ); }
            set { stringDictionary [ "Publish" ] = value; }
        }

        /// <summary>
        /// The build identifier to be used to publish test results.
        /// </summary>
        public String PublishBuild
        {
            get { return ( GetKey ( "PublishBuild" ) ); }
            set { stringDictionary [ "PublishBuild" ] = value; }
        }

        /// <summary>
        /// The name of the test results file to publish. If none is specified, 
        /// use the file produced by the current test run.
        /// </summary>
        public String PublishResultsFile
        {
            get { return ( GetKey ( "PublishResultsFile" ) ); }
            set { stringDictionary [ "PublishResultsFile" ] = value; }
        }

        /// <summary>
        /// The name of the team project to which the build belongs. Specify 
        /// this when publishing test results.
        /// </summary>
        public String TeamProject
        {
            get { return ( GetKey ( "TeamProject" ) ); }
            set { stringDictionary [ "TeamProject" ] = value; }
        }

        /// <summary>
        /// The platform of the build against which to publish test results.
        /// </summary>
        public String Platform
        {
            get { return ( GetKey ( "Platform" ) ); }
            set { stringDictionary [ "Platform" ] = value; }
        }

        /// <summary>
        /// The flavor of the build against which to publish test results.
        /// </summary>
        public String Flavor
        {
            get { return ( GetKey ( "Flavor" ) ); }
            set { stringDictionary [ "Flavor" ] = value; }
        }

        /// <summary>
        /// Use the version of MSTest that came with VS2008.

        /// </summary>
        /// <remarks>
        /// Defaults to false.
        /// </remarks>
        public Boolean UseTest2008
        {
            get { return (useTest2008); }
            set { useTest2008 = value; }
        }
        
        #endregion

        #region Private Helper Methods
        // Why can't the Dictionary just return null/Empty if a key doesn't 
        // exist?
        private String GetKey ( String key )
        {
            String ret = String.Empty;
            if ( false == stringDictionary.TryGetValue ( key , out ret ) )
            {
                return ( null );
            }
            return ( ret );
        }

        private string BuildResultFileFromRunConfig ( string file )
        {
            // Get the pieces of the filename.
            String path = Path.GetDirectoryName ( file );
            String fileName = Path.GetFileNameWithoutExtension ( file );
            String ext = Path.GetExtension ( file );

            // Crack open the config file and look at the settings.
            XmlDocument xmlDoc = new XmlDocument ( );
            xmlDoc.Load ( GetKey ( "RunConfig" ) );
            // Get the nodes I need.  The baseName node is not always there so
            // I'm careful with it.
            XmlNode timeStampNode = null;
            XmlNode useDefaultNode = null;
            XmlNode baseNameNode = null;

            if (UseTest2008)
            {
                String testRunNamingSchemeNodeString = "//ns:TestRunConfiguration/ns:NamingScheme";
                String timeStampNodeString = testRunNamingSchemeNodeString + "/@appendTimeStamp";
                String useDefaultNodeString = testRunNamingSchemeNodeString + "/@useDefault";
                String baseNameNodeString = testRunNamingSchemeNodeString + "/@baseName";

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
                nsmgr.AddNamespace("ns", "http://microsoft.com/schemas/VisualStudio/TeamTest/2006");

                timeStampNode = xmlDoc.SelectSingleNode(timeStampNodeString, nsmgr);
                useDefaultNode = xmlDoc.SelectSingleNode(useDefaultNodeString, nsmgr);
                baseNameNode = xmlDoc.SelectSingleNode(baseNameNodeString, nsmgr);
            }
            else
            {
                String testRunNamingSchemeNodeString = "//Tests/TestRunConfiguration/testRunNamingScheme";
                String timeStampNodeString = testRunNamingSchemeNodeString + "/appendTimeStamp";
                String useDefaultNodeString = testRunNamingSchemeNodeString + "/useDefault";
                String baseNameNodeString = testRunNamingSchemeNodeString + "/baseName";

                timeStampNode = xmlDoc.SelectSingleNode(timeStampNodeString);
                useDefaultNode = xmlDoc.SelectSingleNode(useDefaultNodeString);
                baseNameNode = xmlDoc.SelectSingleNode(baseNameNodeString);
            }
            String baseName = null;
            if ( null != baseNameNode )
            {
                baseName = baseNameNode.InnerText;
            }
            bool useTimeStamp = true;
            if (timeStampNode != null)
            {
                useTimeStamp = Convert.ToBoolean ( timeStampNode.InnerText , CultureInfo.InvariantCulture );
            }

            bool useDefault = true;
            if (useDefaultNode != null)
            {
                useDefault = Convert.ToBoolean ( useDefaultNode.InnerText , CultureInfo.InvariantCulture );
            }

            // Now that I've read everything, process away!
            if ( true == useDefault )
            {
                StringBuilder sb = new StringBuilder ( );
                sb.AppendFormat ( "{0} {1}_{2}" ,
                                 fileName ,
                                 Environment.UserName ,
                                 Environment.MachineName );
                fileName = sb.ToString ( );
            }
            else if ( false == String.IsNullOrEmpty ( baseName ) )
            {
                StringBuilder sbBase = new StringBuilder ( );
                sbBase.AppendFormat ( "{0} {1}" , fileName , baseName );
                fileName = sbBase.ToString ( );
            }
            if ( true == useTimeStamp )
            {
                Boolean useUnderscores = false;
                if ( ( false == String.IsNullOrEmpty ( baseName ) ) &&
                      ( false == useDefault ) )
                {
                    useUnderscores = true;
                }
                fileName = AddTimeStampToFilename ( fileName , useUnderscores );
            }
            StringBuilder final = new StringBuilder ( );
            final.AppendFormat ( "{0}\\{1}{2}" , path , fileName , ext );
            return ( final.ToString ( ) );
        }

        private static String AddExtensionIfNecessary ( String file )
        {
            if ( Path.HasExtension ( file ) == false )
            {
                return ( Path.ChangeExtension ( file , ".trx" ) );
            }
            return ( file );
        }

        private static String AddTimeStampToFilename ( String file ,
                                                       Boolean useUnderscores )
        {
            // Get the pieces of the path.
            String path = Path.GetDirectoryName ( file );
            if ( false == String.IsNullOrEmpty ( path ) )
            {
                // Put the trailing backslash *back* on!
                path += "\\";
            }
            String fileName = Path.GetFileNameWithoutExtension ( file );
            String ext = Path.GetExtension ( file );

            // These underscores are here because if you specify a base name
            // in the .TESTRUNCONFIG, MSTest.exe formats the time a different
            // way.  Nothing like random filenames! ;)
            String underscore = String.Empty;
            if ( true == useUnderscores )
            {
                underscore = "_";
            }

            StringBuilder sb = new StringBuilder ( );
            DateTime currTime = DateTime.Now;
            sb.AppendFormat ( "{0} {2}{1}{2}" ,
                              fileName ,
                              currTime.ToString ( "yyyy-MM-dd HH_mm_ss" ,
                                            DateTimeFormatInfo.InvariantInfo ) ,
                              underscore );
            StringBuilder final = new StringBuilder ( );
            final.AppendFormat ( "{0}{1}{2}" ,
                                 path ,
                                 sb.ToString ( ) ,
                                 ext );
            return ( final.ToString ( ) );
        }

        #endregion

        #region Task Methods
        /// <summary>
        /// Called to validate the parameters.
        /// </summary>
        /// <returns>
        /// True if the parameters are good; false otherwise.
        /// </returns>
        protected override bool ValidateParameters ( )
        {
			if ((this.TestContainers == null) || (this.TestContainers.Length == 0))
			{
				Log.LogWarning("No Test To Run");
				return (false);
			}

			finalResultFile = GetKey ( "ResultsFile" );
            // Look to see if the passed in name looks reasonable.
            Boolean retValue = TaskHelpers.CheckFilePath ( finalResultFile , 
                                                           Log );
            if ( false == retValue )
            {
                return ( false );
            }
            // Figure out the full path of this guy.
            finalResultFile = Path.GetFullPath ( finalResultFile );
            // Slag off the directory name so I can make the directory if it 
            // does not exist.
            String path = Path.GetDirectoryName ( finalResultFile );
            retValue = TaskHelpers.SafeCreateDirectory ( path , Log );
            if ( true == retValue )
            {
                // No matter what I need to ensure the extension is on the file 
                // so I'll add it here.
                finalResultFile = AddExtensionIfNecessary ( finalResultFile );

                String runConfig = GetKey ( "RunConfig" );
                // If UniqueFile is true and there's no RunConfig, I've got to
                // add on the timestamp to make the file unique.
                if ( ( true == uniqueName ) &&
                     ( true == String.IsNullOrEmpty ( runConfig ) ) )
                {
                    finalResultFile = AddTimeStampToFilename ( finalResultFile ,
                                                               false );
                }
                // Is there a RunConfig?  If so, I'll read the name settings out 
                // of it.
                else if ( false == String.IsNullOrEmpty ( runConfig ) )
                {
                    finalResultFile =
                               BuildResultFileFromRunConfig ( finalResultFile );
                }
            }
            return ( retValue );
        }

        /// <summary>
        /// Builds up the command line for MSTEST.EXE.
        /// </summary>
        /// <returns>
        /// The command line to execute.
        /// </returns>
        protected override string GenerateCommandLineCommands ( )
        {
            ExtendedCommandLineBuilder builder =
                                             new ExtendedCommandLineBuilder ( );

            // Never show the logo.  There's already enough spew here. ;)
            builder.AppendSwitch ( "/nologo" );

            // Poke on the metadata file.
            builder.AppendSwitchIfNotNull ( "/testmetadata:" ,
                                            GetKey ( "TestMetaData" ) );

            // Do all the test containers.
            builder.AppendSwitchesIfNotNull ( "/testcontainer:" ,
                                              testContainers );

            // Do the run config.
            builder.AppendSwitchIfNotNull ( "/runconfig:" ,
                                            GetKey ( "RunConfig" ) );

            // Do the results file.
            builder.AppendSwitchIfNotNull ( "/resultsfile:" , finalResultFile );

            // Any tests lists, jump on the bandwagon!
            builder.AppendSwitchesIfNotNull ( "/testlist:" ,
                                              testListLists );

            // Any tests? Hop in, the water's fine.
            builder.AppendSwitchesIfNotNull ( "/test:" , testList );

            // Isolation?
            builder.AppendSwitchIfTrue ( "/noisolation" , noIsolation );

            // Any details?  Pile on!
            builder.AppendSwitchesIfNotNull ( "/detail:" , detailList );

            // All those Team System parameters.
            builder.AppendSwitchIfNotNull ( "/publish:" ,
                                            GetKey ( "Publish" ) );
            builder.AppendSwitchIfNotNull ( "/publishbuild:" ,
                                            GetKey ( "PublishBuild" ) );
            builder.AppendSwitchIfNotNull ( "/publishresultsfile:" ,
                                            GetKey ( "PublishResultsFile" ) );
            builder.AppendSwitchIfNotNull ( "/teamproject:" ,
                                            GetKey ( "TeamProject" ) );
            builder.AppendSwitchIfNotNull ( "/platform:" ,
                                            GetKey ( "Platform" ) );
            builder.AppendSwitchIfNotNull ( "/flavor:" , GetKey ( "Flavor" ) );
            // Whew!  Finally done.
            return ( builder.ToString ( ) );
        } 
        #endregion

        protected override bool HandleTaskExecutionErrors()
        {
            return true;
        }
    }

}
