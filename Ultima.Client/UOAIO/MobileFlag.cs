using System;

namespace UOAIO;

[Flags]
public enum MobileFlag
{
	None = 0,
	Frozen = 1,
	Female = 2,
	Poisoned = 4,
	YellowHits = 8,
	FactionShop = 0x10,
	Warmode = 0x40,
	Hidden = 0x80,
	ValidMask = 0xDF,
	InvalidMask = -224
}
