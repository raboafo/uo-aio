namespace UOAIO;

public class PaperdollImage
{
	private int _gumpId;

	private int? _hueId;

	private float _alpha;

	public int GumpId => this._gumpId;

	public int? HueId => this._hueId;

	public float Alpha => this._alpha;

	public PaperdollImage(int gumpId, int? hueId, float alpha)
	{
		this._gumpId = gumpId;
		this._hueId = hueId;
		this._alpha = alpha;
	}
}
