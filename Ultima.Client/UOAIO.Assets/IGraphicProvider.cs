using System;

namespace UOAIO.Assets;

public interface IGraphicProvider : IDisposable
{
	Texture GetItem(int itemId);

	Texture GetGump(int gumpId);

	Texture GetTerrainTexture(int textureId);

	Texture GetTerrainIsometric(int landId);

	Texture GetLight(int lightId);

	Frames GetAnimation(int animationId);
}
