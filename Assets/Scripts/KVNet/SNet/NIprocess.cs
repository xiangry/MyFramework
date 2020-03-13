using System;
using System.Collections.Generic;
using System.Text;

namespace SNet
{
	public interface NIprocess
	{
		bool ProcessEvent(Core.Net.Client c, Core.Net.Event.Type type, int error);
		bool Process(Core.Net.Client c, Core.Net.Package p, int messageid, NFunction f);
	}
}
