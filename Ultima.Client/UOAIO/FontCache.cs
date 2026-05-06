using System;
using System.Collections.Generic;

namespace UOAIO;

public class FontCache
{
	private class CacheKey
	{
		private string m_Text;

		private IHue m_Hue;

		private int m_Hash;

		public CacheKey(string text, IHue hue)
		{
			this.m_Text = text;
			this.m_Hue = hue;
			this.m_Hash = text.GetHashCode() ^ hue.GetHashCode();
		}

		public override int GetHashCode()
		{
			return this.m_Hash;
		}

		public override bool Equals(object x)
		{
			CacheKey cacheKey = (CacheKey)x;
			return this == cacheKey || (this.m_Hash == cacheKey.m_Hash && this.m_Hue.Equals(cacheKey.m_Hue) && this.m_Text == cacheKey.m_Text);
		}
	}

	private IFontFactory m_Factory;

	private Dictionary<CacheKey, Texture> m_Cached;

	public Texture this[string Key, IHue Hue]
	{
		get
		{
			if (Key == null)
			{
				Debug.Trace("FontCache[] crash averted where key == null");
				Key = "";
			}
			if (!Engine.m_Device.TestCooperativeLevel().Success)
			{
				return Texture.Empty;
			}
			CacheKey key = new CacheKey(Key, Hue);
			Texture value = null;
			if (!this.m_Cached.TryGetValue(key, out value))
			{
				value = ((!(Hue is Hues.DefaultHue)) ? Texture.Clone(this[Key, Hues.Default], Hue.ShaderData) : this.m_Factory.CreateInstance(Key));
				this.m_Cached.Add(key, value);
			}
			return value;
		}
	}

	public FontCache(IFontFactory Factory)
	{
		if (Factory == null)
		{
			throw new ArgumentNullException("Factory");
		}
		this.m_Factory = Factory;
		this.m_Cached = new Dictionary<CacheKey, Texture>(32);
	}

	public void Dispose()
	{
		IEnumerator<Texture> enumerator = this.m_Cached.Values.GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.Dispose();
		}
		this.m_Cached.Clear();
		this.m_Cached = null;
	}
}
