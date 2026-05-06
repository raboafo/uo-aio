namespace UOAIO;

public interface IResizable
{
	int Width { get; set; }

	int Height { get; set; }

	int MinHeight { get; }

	int MaxHeight { get; }

	int MinWidth { get; }

	int MaxWidth { get; }
}
