namespace UOAIO;

public class Palette
{
	private int _hueId;

	private ushort[] _colors;

	public int HueId => this._hueId;

	public ushort[] Colors => this._colors;

	public Palette(int hueId, ushort[] colors)
	{
		this._hueId = hueId;
		this._colors = colors;
	}
}
