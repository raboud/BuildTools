using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace RandREng.MsBuildTasks
{
	public class GetEnvVarTask : Task
	{
		private string _variable;
		private string _capture = null;

		[Output]
		public string Capture
		{
			get { return _capture; }
			set { _capture = value; }
		}

		[Required]
		public string Variable
		{
			get { return _variable; }
			set { _variable = value; }
		}

		public override bool Execute()
		{
			if (string.IsNullOrEmpty(_variable))
			{
				Log.LogError("Must specify Variable Name");
				return false;
			}

			this._capture = Environment.GetEnvironmentVariable(_variable);
			return true;
		}

	}
}
