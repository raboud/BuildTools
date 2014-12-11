using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace RandREng.MsBuildTasks
{
	public class AddToPathTask : Task
	{
		private string _value;

		[Required]
		public string Value
		{
			get { return _value; }
			set { _value = value; }
		}

		public override bool Execute()
		{
			string path = _value + ";" ;
			path += Environment.GetEnvironmentVariable("Path");

			Environment.SetEnvironmentVariable("Path", path);
			return true;
		}
	}
}
