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
	[ReflectorType("rollingThreeDigitLabeller")]
	class RollingThreeDigitLabeller : ILabeller, ITask
	{
		[ReflectorProperty("major", Required = true)]
		public int Major = 0;

		[ReflectorProperty("minor", Required = true)]
		public int Minor = 0;

		[ReflectorProperty("build", Required = true)]
		public int Build = 0;

		[ReflectorProperty("prefix", Required = false)]
		public string LabelPrefix = String.Empty;

		public RollingThreeDigitLabeller()
		{
		}

		public string Generate(IIntegrationResult previousResult)
		{
			Version current = GetCurrent();
			if (previousResult != null && previousResult.Label != null)
			{
				Regex regex = new Regex("\\d+.\\d+.\\d+");
				Match match = regex.Match(previousResult.Label);
				if (null != match && match.Success)
				{
					// If the first three build numbers match, then we are building on the same day.  
					Version previous = new Version(match.Value);
					if (current.Major == previous.Major && current.Minor == previous.Minor)
					{
						current = new Version(previous.Major, previous.Minor, previous.Build + 1, 0);
					}
				}
			}
			return string.Format("{0} - {1}", LabelPrefix, current.ToString());
		}

		public void Run(IIntegrationResult result)
		{
			result.Label = this.Generate(result);
		}

		private Version GetCurrent()
		{
			return new Version(this.Major, this.Minor, this.Build, 0);
		}
	}
}
