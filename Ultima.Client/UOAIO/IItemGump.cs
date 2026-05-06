namespace UOAIO;

public interface IItemGump
{
	Item Item { get; }

	int xOffset { get; }

	int yOffset { get; }

	int yBottom { get; }
}
