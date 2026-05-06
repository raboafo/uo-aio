using System;
using System.Collections.Generic;

namespace UOAIO.Assets;

public sealed class DynamicCacheGraphicProvider : IGraphicProvider, IDisposable
{
	private IGraphicProvider _provider;

	private Dictionary<int, Texture> _land;

	private Dictionary<int, Texture> _items;

	private Dictionary<int, Texture> _gumps;

	private Dictionary<int, Texture> _textures;

	private Dictionary<int, Texture> _lights;

	private Dictionary<int, Frames> _anims;

	public DynamicCacheGraphicProvider(IGraphicProvider provider)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		this._provider = provider;
	}

	public Texture GetTerrainIsometric(int landId)
	{
		if (this._land == null)
		{
			this._land = new Dictionary<int, Texture>();
		}
		if (!this._land.TryGetValue(landId, out var value))
		{
			this._land.Add(landId, value = this._provider.GetTerrainIsometric(landId));
		}
		return value;
	}

	public Texture GetItem(int itemId)
	{
		if (this._items == null)
		{
			this._items = new Dictionary<int, Texture>();
		}
		if (!this._items.TryGetValue(itemId, out var value))
		{
			this._items.Add(itemId, value = this._provider.GetItem(itemId));
		}
		return value;
	}

	public Texture GetGump(int gumpId)
	{
		if (this._gumps == null)
		{
			this._gumps = new Dictionary<int, Texture>();
		}
		if (!this._gumps.TryGetValue(gumpId, out var value))
		{
			this._gumps.Add(gumpId, value = this._provider.GetGump(gumpId));
		}
		return value;
	}

	public Texture GetTerrainTexture(int textureId)
	{
		if (this._textures == null)
		{
			this._textures = new Dictionary<int, Texture>();
		}
		if (!this._textures.TryGetValue(textureId, out var value))
		{
			this._textures.Add(textureId, value = this._provider.GetTerrainTexture(textureId));
		}
		return value;
	}

	public Frames GetAnimation(int animationId)
	{
		if (this._anims == null)
		{
			this._anims = new Dictionary<int, Frames>();
		}
		if (!this._anims.TryGetValue(animationId, out var value))
		{
			this._anims.Add(animationId, value = this._provider.GetAnimation(animationId));
		}
		return value;
	}

	public Texture GetLight(int lightId)
	{
		if (this._lights == null)
		{
			this._lights = new Dictionary<int, Texture>();
		}
		if (!this._lights.TryGetValue(lightId, out var value))
		{
			this._lights.Add(lightId, value = this._provider.GetLight(lightId));
		}
		return value;
	}

	public void Dispose()
	{
		if (this._land != null)
		{
			foreach (Texture value in this._land.Values)
			{
				value.Dispose();
			}
			this._land.Clear();
			this._land = null;
		}
		if (this._items != null)
		{
			foreach (Texture value2 in this._items.Values)
			{
				value2.Dispose();
			}
			this._items.Clear();
			this._items = null;
		}
		if (this._gumps != null)
		{
			foreach (Texture value3 in this._gumps.Values)
			{
				value3.Dispose();
			}
			this._gumps.Clear();
			this._gumps = null;
		}
		if (this._textures != null)
		{
			foreach (Texture value4 in this._textures.Values)
			{
				value4.Dispose();
			}
			this._textures.Clear();
			this._textures = null;
		}
		if (this._lights != null)
		{
			foreach (Texture value5 in this._lights.Values)
			{
				value5.Dispose();
			}
			this._lights.Clear();
			this._lights = null;
		}
		if (this._anims != null)
		{
			foreach (Frames value6 in this._anims.Values)
			{
				value6.Dispose();
			}
			this._anims.Clear();
			this._anims = null;
		}
		if (this._provider != null)
		{
			this._provider.Dispose();
			this._provider = null;
		}
	}
}
