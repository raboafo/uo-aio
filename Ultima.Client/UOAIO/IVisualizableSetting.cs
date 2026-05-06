namespace UOAIO;

public interface IVisualizableSetting
{
	string LabelKey { get; }

	bool Enabled { get; set; }

	void Draw(int x, int y);
}
