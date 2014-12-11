using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using Exortech.NetReflector;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Util;
using ThoughtWorks.CruiseControl;

namespace RandREng.CCNet.Plugin
{
	[ReflectorType("yearJulianVersionLabeller")]
	class YearJulianVersionLabeller : ILabeller, ITask
	{
		[ReflectorProperty("major", Required = true)]
		public int Major = 0;

		[ReflectorProperty("minor", Required = true)]
		public int Minor = 0;

		[ReflectorProperty("prefix", Required = false)]
		public string LabelPrefix = String.Empty;

		public YearJulianVersionLabeller()
		{
		}

		#region ILabeller Members

		public string Generate(IIntegrationResult resultFromLastBuild)
		{
			Version current = GetCurrent();
			if (resultFromLastBuild != null && resultFromLastBuild.Label != null)
			{
				Regex regex = new Regex("\\d+.\\d+.\\d+.\\d+");
				Match match = regex.Match(resultFromLastBuild.Label);
				if (null != match && match.Success)
				{
					// If the first three build numbers match, then we are building on the same day.  
					Version previous = new Version(match.Value);
					if (current.Major == previous.Major && current.Minor == previous.Minor && current.Build == previous.Build)
					{
						current = new Version(previous.Major, previous.Minor, previous.Build, previous.Revision + 1);
					}
				}
			}
			return LabelPrefix + current.ToString();
		}

		#endregion

		#region ITask Members

		public void Run(IIntegrationResult result)
		{
			result.Label = this.Generate(result);
		}

		#endregion

		private Version GetCurrent()
		{
			return new Version(this.Major, this.Minor, GetBuildNumber(DateTime.Now), 0);
		}

		private int GetBuildNumber(DateTime date)
		{
			int build = ((date.Year / 2000) * 10000) + ((date.Year % 10) * 1000) + date.DayOfYear;
			return build;
		}

	}
}
