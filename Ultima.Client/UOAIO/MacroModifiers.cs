using System;

namespace UOAIO;

[Flags]
public enum MacroModifiers
{
	None = 0,
	All = 1,
	Ctrl = 2,
	Alt = 4,
	Shift = 8
}
