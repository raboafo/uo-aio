using System;

namespace UOAIO.Assets;

public sealed class PhysicalGraphicProvider : IGraphicProvider, IDisposable
{
	private static readonly LightReader lightReader;

	private IHue _hue;

	public PhysicalGraphicProvider(IHue hue)
	{
		if (hue == null)
		{
			throw new ArgumentNullException("hue");
		}
		this._hue = hue;
	}

	public Texture GetTerrainIsometric(int landId)
	{
		return Engine.LandArt.ReadFromDisk(landId, this._hue);
	}

	public Texture GetItem(int itemId)
	{
		return Engine.ItemArt.ReadFromDisk(itemId, this._hue);
	}

	public Texture GetGump(int gumpId)
	{
		return Engine.m_Gumps.ReadFromDisk(gumpId, this._hue);
	}

	public Texture GetTerrainTexture(int textureId)
	{
		return Engine.TextureArt.ReadFromDisk(textureId, this._hue);
	}

	public Frames GetAnimation(int animationId)
	{
		return Engine.m_Animations.Create(animationId, this._hue);
	}

	public Texture GetLight(int lightId)
	{
		return PhysicalGraphicProvider.lightReader.ReadLight(lightId);
	}

	public void Dispose()
	{
	}

	static PhysicalGraphicProvider()
	{
		PhysicalGraphicProvider.lightReader = new LightReader();
	}
}
