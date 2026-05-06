using System;
using Veritas;

namespace UOAIO;

public class ImageCache : IDisposable
{
	private Texture _veritasLogo;

	private Texture _travelIcon;

	private Texture _lastTargetIcon;

	private Texture _playerHalo;

	private Texture _playerAlly;

	private Texture _playerEnemy;

	private Texture _targetCursorHighlight;

	private Texture _targetCursorLocal;

	public Texture VeritasLogo
	{
		get
		{
			if (this._veritasLogo == null)
			{
				try
				{
					this._veritasLogo = Engine.LoadArchivedTexture(Archive.AcquireArchive("shell"), "graphics/gold_dragon.png");
				}
				catch
				{
				}
			}
			return this._veritasLogo;
		}
	}

	public Texture TravelIcon => this.AcquireImage(ref this._travelIcon, "travel-icon.png");

	public Texture LastTargetIcon => this.AcquireImage(ref this._lastTargetIcon, "last-target-icon.png");

	public Texture PlayerHalo => this.AcquireImage(ref this._playerHalo, "halo.png");

	public Texture PlayerAlly => this.AcquireImage(ref this._playerAlly, "ally.png");

	public Texture PlayerEnemy => this.AcquireImage(ref this._playerEnemy, "enemy.png");

	public Texture TargetCursorHighlight => this.AcquireImage(ref this._targetCursorHighlight, "target-cursor-highlight.png");

	public Texture TargetCursorLocal => this.AcquireImage(ref this._targetCursorLocal, "target-cursor-local.png");

	public void Dispose()
	{
		this.Dispose(ref this._veritasLogo);
		this.Dispose(ref this._travelIcon);
		this.Dispose(ref this._lastTargetIcon);
		this.Dispose(ref this._playerHalo);
		this.Dispose(ref this._playerAlly);
		this.Dispose(ref this._playerEnemy);
		this.Dispose(ref this._targetCursorHighlight);
		this.Dispose(ref this._targetCursorLocal);
	}

	public void Dispose(ref Texture image)
	{
		if (image != null)
		{
			image.Dispose();
			image = null;
		}
	}

	private Texture AcquireAlpha(ref Texture image, string path)
	{
		if (image == null)
		{
			image = Engine.LoadImageAsAlpha(path);
		}
		return image;
	}

	private Texture AcquireImage(ref Texture image, string path)
	{
		if (image == null)
		{
			image = Engine.LoadArchivedTexture(path);
		}
		return image;
	}
}
