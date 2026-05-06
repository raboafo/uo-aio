using System;
using System.Collections.Generic;

namespace UOAIO.Assets;

public sealed class StaticCacheGraphicProvider : IGraphicProvider, IDisposable
{
	private IGraphicProvider _provider;

	private Texture[] _land;

	private Texture[] _items;

	private Texture[] _gumps;

	private Texture[] _textures;

	private Texture[] _lights;

	private Dictionary<int, Frames> _anims;

	public StaticCacheGraphicProvider(IGraphicProvider provider)
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
			this._land = new Texture[16384];
		}
		landId &= 0x3FFF;
		Texture texture = this._land[landId];
		if (texture == null)
		{
			texture = (this._land[landId] = this._provider.GetTerrainIsometric(landId));
		}
		return texture;
	}

	public Texture GetItem(int itemId)
	{
		if (this._items == null)
		{
			this._items = new Texture[65536];
		}
		itemId &= 0xFFFF;
		Texture texture = this._items[itemId];
		if (texture == null)
		{
			texture = (this._items[itemId] = this._provider.GetItem(itemId));
		}
		return texture;
	}

	public Texture GetGump(int gumpId)
	{
		if (this._gumps == null)
		{
			this._gumps = new Texture[65536];
		}
		gumpId &= 0xFFFF;
		Texture texture = this._gumps[gumpId];
		if (texture == null)
		{
			texture = (this._gumps[gumpId] = this._provider.GetGump(gumpId));
		}
		return texture;
	}

	public Texture GetTerrainTexture(int textureId)
	{
		if (this._textures == null)
		{
			this._textures = new Texture[16384];
		}
		textureId &= 0x3FFF;
		Texture texture = this._textures[textureId];
		if (texture == null)
		{
			texture = (this._textures[textureId] = this._provider.GetTerrainTexture(textureId));
		}
		return texture;
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
			this._lights = new Texture[100];
		}
		if (lightId >= 0 && lightId < this._lights.Length)
		{
			return this._lights[lightId] ?? (this._lights[lightId] = this._provider.GetLight(lightId));
		}
		return null;
	}

	public void Dispose()
	{
		if (this._land != null)
		{
			for (int i = 0; i < this._land.Length; i++)
			{
				if (this._land[i] != null)
				{
					this._land[i].Dispose();
				}
			}
			this._land = null;
		}
		if (this._items != null)
		{
			for (int j = 0; j < this._items.Length; j++)
			{
				if (this._items[j] != null)
				{
					this._items[j].Dispose();
				}
			}
			this._items = null;
		}
		if (this._gumps != null)
		{
			for (int k = 0; k < this._gumps.Length; k++)
			{
				if (this._gumps[k] != null)
				{
					this._gumps[k].Dispose();
				}
			}
			this._gumps = null;
		}
		if (this._textures != null)
		{
			for (int l = 0; l < this._textures.Length; l++)
			{
				if (this._textures[l] != null)
				{
					this._textures[l].Dispose();
				}
			}
			this._textures = null;
		}
		if (this._anims != null)
		{
			foreach (Frames value in this._anims.Values)
			{
				value.Dispose();
			}
			this._anims.Clear();
			this._anims = null;
		}
		if (this._lights != null)
		{
			for (int m = 0; m < this._lights.Length; m++)
			{
				if (this._lights[m] != null)
				{
					this._lights[m].Dispose();
				}
			}
			this._lights = null;
		}
		if (this._provider != null)
		{
			this._provider.Dispose();
			this._provider = null;
		}
	}
}
