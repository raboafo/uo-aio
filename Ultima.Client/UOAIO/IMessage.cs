using System.Drawing;

namespace UOAIO;

public interface IMessage
{
	bool Visible { get; }

	float Alpha { get; }

	Rectangle Rectangle { get; }

	Rectangle ImageRect { get; }

	Rectangle OnBeginRender();
}
