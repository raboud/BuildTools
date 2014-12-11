/*------------------------------------------------------------------------------
 * Bugslayer Column   -  MSDN Magazine  -  John Robbins  -  john@wintellect.com
 * ---------------------------------------------------------------------------*/
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace RandREng.MsBuildTasks
{
    /// <summary>
    /// Extends the <see cref="CommandLineBuilder"/> class with more options.
    /// </summary>
    class ExtendedCommandLineBuilder : CommandLineBuilder 
    {
        /// <summary>
        /// Initializes a new instance of 
        /// <see cref="ExtendedCommandLineBuilder"/>.
        /// </summary>
        public ExtendedCommandLineBuilder ( )
            : base ( )
        {
        }

        /// <summary>
        /// Appends the command line with the switch name for each item in the 
        /// task item specifications that act as string parameters with all 
        /// items deliminated with a single space character.
        /// </summary>
        /// <param name="switchName">
        /// The name of the switch to append to the command line. This value 
        /// cannot be a null reference.
        /// </param>
        /// <param name="parameters">
        /// An array of switch parameters to append to the command line. 
        /// Quotation marks will be added as necessary. If the array is a null 
        /// reference (Nothing in Visual Basic), then this method has no effect.
        /// </param>
        /// <remarks>
        /// If called like the following, where someData contains the values 'a'
        /// and 'b':
        /// <code>
        /// AppendSwitchesIfNotNull ( "/foo:" , someData );
        /// </code>
        /// The output would be:
        /// <code>
        /// /foo:a /foo:b
        /// </code>
        /// </remarks>
        public void AppendSwitchesIfNotNull ( String switchName ,
                                              ITaskItem[] parameters )
        {
            AppendSwitchesIfNotNull ( switchName , parameters , " " );
        }

        /// <summary>
        /// Appends the command line with the switch name for each item in the 
        /// task item specifications that act as string parameters with all 
        /// items deliminated by the requested delimeter.
        /// </summary>
        /// <param name="switchName">
        /// The name of the switch to append to the command line. This value 
        /// cannot be a null reference (Nothing in Visual Basic).
        /// </param>
        /// <param name="parameters">
        /// An array of switch parameters to append to the command line. 
        /// Quotation marks will be added as necessary. If the array is a null 
        /// reference (Nothing in Visual Basic), then this method has no effect.
        /// </param>
        /// <param name="delimeter">
        /// The delimiter that separates individual parameters. This value can 
        /// be empty, but it cannot be a null reference (Nothing in Visual 
        /// Basic).
        /// </param>
        public void AppendSwitchesIfNotNull ( String switchName ,
                                              ITaskItem [] parameters ,
                                              String delimeter )
        {
            AppendSwitchIfNotNull ( switchName , 
                                    parameters , 
                                    delimeter + switchName );
        }

        /// <summary>
        /// Appends to the command line a switch for each item in the 
        /// <paramref name="parameters"/> array delimited by a space.
        /// </summary>
        /// <param name="switchName">
        /// The name of the switch to append to the command line. This value 
        /// cannot be a null reference (Nothing in Visual Basic).
        /// </param>
        /// <param name="parameters">
        /// An array of switch parameters to append to the command line. 
        /// Quotation marks will be added as necessary. If the array is a null 
        /// reference (Nothing in Visual Basic), then this method has no effect.
        /// </param>
        /// <remarks>
        /// If called like the following:
        /// <code>
        /// AppendSwitchesIfNotNull ( "/foo:" , new String[] { "x" , "y" } );
        /// </code>
        /// The output would be:
        /// <code>
        /// /foo:x /foo:y
        /// </code>
        /// </remarks>
        public void AppendSwitchesIfNotNull ( String switchName ,
                                              String [] parameters )
        {
            AppendSwitchesIfNotNull ( switchName , parameters , " " );
        }

        /// <summary>
        /// Appends the command line with a switch for each string in the 
        /// array of string parameters. 
        /// </summary>
        /// <param name="switchName">
        /// The name of the switch to append to the command line. This value 
        /// cannot be a null reference (Nothing in Visual Basic).
        /// </param>
        /// <param name="parameters">
        /// An array of switch parameters to append to the command line. 
        /// Quotation marks will be added as necessary. If the array is a null 
        /// reference (Nothing in Visual Basic), then this method has no effect.
        /// </param>
        /// <param name="delimeter">
        /// The delimiter that separates individual parameters. This value can 
        /// be empty, but it cannot be a null reference (Nothing in Visual 
        /// Basic).
        /// </param>
        public void AppendSwitchesIfNotNull ( String switchName ,
                                              String [] parameters ,
                                              String delimeter )
        {
            AppendSwitchIfNotNull ( switchName ,
                                    parameters ,
                                    delimeter + switchName );
        }

        /// <summary>
        /// Appends the switch if the value is true.
        /// </summary>
        /// <param name="switchName">
        /// The name of the switch to append to the command line. This value 
        /// cannot be a null reference (Nothing in Visual Basic).
        /// </param>
        /// <param name="switchValue">
        /// If true, the <paramref name="switchName"/> is appended to the 
        /// string.
        /// </param>
        public void AppendSwitchIfTrue ( String switchName ,
                                         Boolean switchValue )
        {
            if ( true == switchValue )
            {
                AppendSwitch ( switchName );
            }
        }
    }
}
