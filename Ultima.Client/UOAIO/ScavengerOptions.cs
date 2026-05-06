using System;

namespace UOAIO;

[Flags]
public enum ScavengerOptions
{
	None = 0,
	Reagents = 1,
	Munitions = 2,
	Bolas = 4,
	Artifacts = 8,
	Default = 0xD
}
