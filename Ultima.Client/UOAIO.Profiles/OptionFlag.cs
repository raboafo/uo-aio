using System;

namespace UOAIO.Profiles;

[Flags]
public enum OptionFlag
{
	None = 0,
	AlwaysRun = 1,
	IncomingNames = 2,
	NotorietyHalos = 4,
	ProtectHeals = 8,
	ProtectCures = 0x10,
	ProtectPoisons = 0x20,
	ProtectBandages = 0x30,
	SiegeRuleset = 0x40,
	QueueTargets = 0x80,
	Scavenger = 0x100,
	Screenshots = 0x200,
	MiniHealth = 0x400,
	ContainerGrid = 0x800,
	SmoothWalk = 0x1000,
	KeyPassthrough = 0x2000,
	MoongateConfirmation = 0x4000,
	AlwaysLight = 0x8000,
	HotkeysEnabled = 0x10000,
	ClearHandsBeforeCast = 0x20000,
	ClearHandsBeforePot = 0x30000,
	HideTrees = 0x40000,
	PartyNotifications = 0x80000,
	Default = 0x9BF3E
}
