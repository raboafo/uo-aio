using System.Diagnostics;

namespace UOAIO;

public class RenderProfile
{
	public int _pushes;

	public int _draws;

	public int _tex0;

	public int _tex1;

	public int _psh;

	public Stopwatch _drawTime;

	public Stopwatch _composeTime;

	public Stopwatch _acquireTime;

	public Stopwatch _storeTime;

	public Stopwatch _pushTime;

	public Stopwatch _worldTime;

	public Stopwatch _gumpTime;

	public RenderProfile()
	{
		this._drawTime = new Stopwatch();
		this._composeTime = new Stopwatch();
		this._acquireTime = new Stopwatch();
		this._storeTime = new Stopwatch();
		this._pushTime = new Stopwatch();
		this._worldTime = new Stopwatch();
		this._gumpTime = new Stopwatch();
	}

	public void Reset()
	{
		this._drawTime.Reset();
		this._composeTime.Reset();
		this._acquireTime.Reset();
		this._storeTime.Reset();
		this._pushTime.Reset();
		this._worldTime.Reset();
		this._gumpTime.Reset();
		this._pushes = 0;
		this._draws = 0;
		this._tex0 = 0;
		this._tex1 = 0;
		this._psh = 0;
	}

	public override string ToString()
	{
		return $"{{ compose = {(this._composeTime.Elapsed - this._storeTime.Elapsed - this._acquireTime.Elapsed).TotalSeconds / this._drawTime.Elapsed.TotalSeconds:P}; acquire={this._storeTime.Elapsed.TotalSeconds / this._drawTime.Elapsed.TotalSeconds:P}; store = {this._acquireTime.Elapsed.TotalSeconds / this._drawTime.Elapsed.TotalSeconds:P}; push = {this._pushTime.Elapsed.TotalSeconds / this._drawTime.Elapsed.TotalSeconds:P}; }}\n{{ world = {this._worldTime.Elapsed.TotalSeconds / this._drawTime.Elapsed.TotalSeconds:P}; gumps = {this._gumpTime.Elapsed.TotalSeconds / this._drawTime.Elapsed.TotalSeconds:P}; }}";
	}
}
