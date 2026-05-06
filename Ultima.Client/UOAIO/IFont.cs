using System.Collections.Generic;

namespace UOAIO;

public interface IFont
{
	Dictionary<WrapKey, string> WrapCache { get; }

	int GetStringWidth(string String);

	Texture GetString(string String, IHue Hue);
}
