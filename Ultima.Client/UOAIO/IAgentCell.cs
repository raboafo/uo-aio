using System;
using System.Collections.Generic;

namespace UOAIO;

public interface IAgentCell : IAgentView, ICell, IDisposable
{
	PhysicalAgent Agent { get; }

	List<ICell> Owner { get; set; }

	void Update();
}
