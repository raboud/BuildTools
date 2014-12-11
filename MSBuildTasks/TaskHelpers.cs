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

namespace RandREng.MsBuildTasks
{
    /// <summary>
    /// Common helper class used by this assembly.
    /// </summary>
    static internal class TaskHelpers
    {
        /// <summary>
        /// Checks if the file path is valid.
        /// </summary>
        /// <param name="fileName">
        /// The filename to check.
        /// </param>
        /// <param name="log">
        /// The <see cref="TaskLoggingHelper"/> to use for logging errors.
        /// </param>
        /// <returns>
        /// True if valid, false otherwise.
        /// </returns>
        internal static Boolean CheckFilePath ( String fileName , 
                                                TaskLoggingHelper log )
        {
            Boolean retValue = true;
            String errMsg = String.Empty;
            try
            {
                errMsg = Path.GetDirectoryName ( fileName );
            }
            catch ( ArgumentException exAE )
            {
                errMsg = exAE.Message;
                retValue = false;
            }
            catch ( PathTooLongException exPTLE )
            {
                errMsg = exPTLE.Message;
                retValue = false;
            }
            if ( false == retValue )
            {
                log.LogErrorFromResources ( "InvalidPathChars" , errMsg );
            }
            return ( retValue );
        }

        /// <summary>
        /// Creates a directory, but reports errors to the log if fails.
        /// </summary>
        /// <param name="path">
        /// The path to create.
        /// </param>
        /// <param name="log">
        /// The <see cref="TaskLoggingHelper"/> to use for logging errors.
        /// </param>
        /// <returns>
        /// True if valid, false otherwise.
        /// </returns>
        internal static Boolean SafeCreateDirectory ( String path ,
                                                      TaskLoggingHelper log )
        {
            Boolean retValue = true;
            String errMsg = String.Empty;
            if ( false == Directory.Exists ( path ) )
            {
                try
                {
                    Directory.CreateDirectory ( path );
                }
                catch ( UnauthorizedAccessException exUAE )
                {
                    errMsg = exUAE.Message;
                    retValue = false;
                }
                catch ( ArgumentNullException exANE )
                {
                    errMsg = exANE.Message;
                    retValue = false;
                }
                catch ( ArgumentException exAE )
                {
                    errMsg = exAE.Message;
                    retValue = false;
                }
                catch ( PathTooLongException exPTLE )
                {
                    errMsg = exPTLE.Message;
                    retValue = false;
                }
                catch ( DirectoryNotFoundException exDNFE )
                {
                    errMsg = exDNFE.Message;
                    retValue = false;
                }
                catch ( IOException exIO )
                {
                    errMsg = exIO.Message;
                    retValue = false;
                }
                catch ( NotSupportedException exNSE )
                {
                    errMsg = exNSE.Message;
                    retValue = false;
                }
            }
            if ( false == retValue )
            {
                // Let the user know.
                log.LogErrorFromResources ( "DirectoryCreationError" , errMsg );
            }
            return ( retValue );
        }

    }
}
