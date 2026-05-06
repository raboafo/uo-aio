namespace UOAIO;

public interface IMessageOwner : IPoint2D
{
	int MessageX { get; set; }

	int MessageY { get; set; }

	int MessageFrame { get; set; }

	void OnSingleClick();

	void OnDoubleClick();

	void OnTarget();
}
