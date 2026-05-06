using System;

namespace UOAIO;

[Flags]
public enum ItemFlag
{
	CanMove = 0x20,
	Hidden = 0x80
}
