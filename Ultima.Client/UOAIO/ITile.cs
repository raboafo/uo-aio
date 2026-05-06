using System;

namespace UOAIO;

public interface ITile : ICell, IDisposable
{
	ushort ID { get; }
}
