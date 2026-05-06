using System;

namespace UOAIO;

public interface ICell : IDisposable
{
	sbyte Z { get; }

	byte Height { get; }

	sbyte SortZ { get; set; }

	Type CellType { get; }
}
