using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Ultima.Data;

namespace UOAIO.Assets;

internal static class AssetSourceManager
{
	private static readonly Lazy<IArtDataSource> _art = new Lazy<IArtDataSource>(CreateArt);

	private static readonly Lazy<IGumpDataSource> _gumps = new Lazy<IGumpDataSource>(CreateGumps);

	private static readonly Lazy<ISoundDataSource> _sounds = new Lazy<ISoundDataSource>(CreateSounds);

	private static readonly Lazy<IAnimationDataSource> _animations = new Lazy<IAnimationDataSource>(CreateAnimations);

	public static IArtDataSource Art => AssetSourceManager._art.Value;

	public static IGumpDataSource Gumps => AssetSourceManager._gumps.Value;

	public static ISoundDataSource Sounds => AssetSourceManager._sounds.Value;

	public static IAnimationDataSource Animations => AssetSourceManager._animations.Value;

	private static T SafeCreate<T>(Func<T> factory) where T : class
	{
		try
		{
			return factory();
		}
		catch
		{
			return null;
		}
	}

	private static IArtDataSource CreateArt()
	{
		List<IArtDataSource> list = new List<IArtDataSource>();
		UooArtDataSource uooArtDataSource = AssetSourceManager.SafeCreate(() => UooArtDataSource.TryCreate());
		if (uooArtDataSource != null)
		{
			list.Add(uooArtDataSource);
		}
		UopArtDataSource uopArtDataSource = AssetSourceManager.SafeCreate(() => UopArtDataSource.TryCreate());
		if (uopArtDataSource != null)
		{
			list.Add(uopArtDataSource);
		}
		MulArtDataSource mulArtDataSource = AssetSourceManager.SafeCreate(() => MulArtDataSource.TryCreate());
		if (mulArtDataSource != null)
		{
			list.Add(mulArtDataSource);
		}
		return new CompositeArtDataSource(list);
	}

	private static IGumpDataSource CreateGumps()
	{
		List<IGumpDataSource> list = new List<IGumpDataSource>();
		UooGumpDataSource uooGumpDataSource = AssetSourceManager.SafeCreate(() => UooGumpDataSource.TryCreate());
		if (uooGumpDataSource != null)
		{
			list.Add(uooGumpDataSource);
		}
		UopGumpDataSource uopGumpDataSource = AssetSourceManager.SafeCreate(() => UopGumpDataSource.TryCreate());
		if (uopGumpDataSource != null)
		{
			list.Add(uopGumpDataSource);
		}
		MulGumpDataSource mulGumpDataSource = AssetSourceManager.SafeCreate(() => MulGumpDataSource.TryCreate());
		if (mulGumpDataSource != null)
		{
			list.Add(mulGumpDataSource);
		}
		return new CompositeGumpDataSource(list);
	}

	private static ISoundDataSource CreateSounds()
	{
		List<ISoundDataSource> list = new List<ISoundDataSource>();
		UopSoundDataSource uopSoundDataSource = AssetSourceManager.SafeCreate(() => UopSoundDataSource.TryCreate());
		if (uopSoundDataSource != null)
		{
			list.Add(uopSoundDataSource);
		}
		MulSoundDataSource mulSoundDataSource = AssetSourceManager.SafeCreate(() => MulSoundDataSource.TryCreate());
		if (mulSoundDataSource != null)
		{
			list.Add(mulSoundDataSource);
		}
		return new CompositeSoundDataSource(list);
	}

	private static IAnimationDataSource CreateAnimations()
	{
		UooAnimationDataSource uooAnimationDataSource = AssetSourceManager.SafeCreate(() => UooAnimationDataSource.TryCreate());
		if (uooAnimationDataSource != null)
		{
			return uooAnimationDataSource;
		}
		return NullAnimationDataSource.Instance;
	}

	public static void Shutdown()
	{
		if (AssetSourceManager._animations.IsValueCreated)
		{
			AssetSourceManager._animations.Value.Dispose();
		}
		if (AssetSourceManager._sounds.IsValueCreated)
		{
			AssetSourceManager._sounds.Value.Dispose();
		}
		if (AssetSourceManager._gumps.IsValueCreated)
		{
			AssetSourceManager._gumps.Value.Dispose();
		}
		if (AssetSourceManager._art.IsValueCreated)
		{
			AssetSourceManager._art.Value.Dispose();
		}
	}
}

internal interface IArtDataSource : IDisposable
{
	bool TryReadLand(int landId, out byte[] data);

	bool TryReadItem(int itemId, out byte[] data);
}

internal interface IGumpDataSource : IDisposable
{
	bool Exists(int gumpId);

	bool TryRead(int gumpId, out byte[] data);
}

internal interface ISoundDataSource : IDisposable
{
	bool TryRead(int soundId, out byte[] data);
}

internal interface IAnimationDataSource : IDisposable
{
	bool IsUoo { get; }

	bool TryLoadIndex(int index, out Entry3D[] entries, out int count);

	bool TryGetFrames(int lookup, out UooAnimationFrameData[] frames, out int maxHeight);
}

internal struct UooAnimationFrameData
{
	public short CenterX;

	public short CenterY;

	public short Width;

	public short Height;

	public ushort[] Pixels;
}

internal sealed class CompositeArtDataSource : IArtDataSource
{
	private readonly List<IArtDataSource> _sources;

	public CompositeArtDataSource(List<IArtDataSource> sources)
	{
		this._sources = sources ?? new List<IArtDataSource>();
	}

	public bool TryReadLand(int landId, out byte[] data)
	{
		foreach (IArtDataSource source in this._sources)
		{
			if (source.TryReadLand(landId, out data))
			{
				return true;
			}
		}
		data = null;
		return false;
	}

	public bool TryReadItem(int itemId, out byte[] data)
	{
		foreach (IArtDataSource source in this._sources)
		{
			if (source.TryReadItem(itemId, out data))
			{
				return true;
			}
		}
		data = null;
		return false;
	}

	public void Dispose()
	{
		foreach (IArtDataSource source in this._sources)
		{
			source.Dispose();
		}
		this._sources.Clear();
	}
}

internal sealed class CompositeGumpDataSource : IGumpDataSource
{
	private readonly List<IGumpDataSource> _sources;

	public CompositeGumpDataSource(List<IGumpDataSource> sources)
	{
		this._sources = sources ?? new List<IGumpDataSource>();
	}

	public bool Exists(int gumpId)
	{
		foreach (IGumpDataSource source in this._sources)
		{
			if (source.Exists(gumpId))
			{
				return true;
			}
		}
		return false;
	}

	public bool TryRead(int gumpId, out byte[] data)
	{
		foreach (IGumpDataSource source in this._sources)
		{
			if (source.TryRead(gumpId, out data))
			{
				return true;
			}
		}
		data = null;
		return false;
	}

	public void Dispose()
	{
		foreach (IGumpDataSource source in this._sources)
		{
			source.Dispose();
		}
		this._sources.Clear();
	}
}

internal sealed class CompositeSoundDataSource : ISoundDataSource
{
	private readonly List<ISoundDataSource> _sources;

	public CompositeSoundDataSource(List<ISoundDataSource> sources)
	{
		this._sources = sources ?? new List<ISoundDataSource>();
	}

	public bool TryRead(int soundId, out byte[] data)
	{
		foreach (ISoundDataSource source in this._sources)
		{
			if (source.TryRead(soundId, out data))
			{
				return true;
			}
		}
		data = null;
		return false;
	}

	public void Dispose()
	{
		foreach (ISoundDataSource source in this._sources)
		{
			source.Dispose();
		}
		this._sources.Clear();
	}
}

internal static class DataPath
{
	public static string TryResolve(string relativePath)
	{
		if (Engine.FileManager == null || string.IsNullOrEmpty(Engine.FileManager.FilePath))
		{
			return null;
		}
		string path = Path.Combine(Engine.FileManager.FilePath, relativePath);
		if (File.Exists(path))
		{
			return path;
		}
		return null;
	}

	public static string TryResolveUoo(string fileName)
	{
		string text = DataPath.TryResolve(fileName);
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		text = DataPath.TryResolve(Path.Combine("uoo", fileName));
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		return DataPath.TryResolve(Path.Combine("UOO", fileName));
	}
}

internal sealed class UopGumpDataSource : IGumpDataSource
{
	private readonly Archive _archive;

	private UopGumpDataSource(Archive archive)
	{
		this._archive = archive;
	}

	public static UopGumpDataSource TryCreate()
	{
		string text = DataPath.TryResolve("gumpartLegacyMUL.uop");
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		return new UopGumpDataSource(new Archive(text));
	}

	private static string GetPath(int gumpId)
	{
		return $"build/gumpartlegacymul/{gumpId:00000000}.tga";
	}

	public bool Exists(int gumpId)
	{
		return gumpId != 0 && this._archive.FileExists(UopGumpDataSource.GetPath(gumpId));
	}

	public bool TryRead(int gumpId, out byte[] data)
	{
		if (gumpId == 0)
		{
			data = null;
			return false;
		}
		return this._archive.TryReadFile(UopGumpDataSource.GetPath(gumpId), out data);
	}

	public void Dispose()
	{
		this._archive.Dispose();
	}
}

internal sealed class UopArtDataSource : IArtDataSource
{
	private readonly Archive _archive;

	private UopArtDataSource(Archive archive)
	{
		this._archive = archive;
	}

	public static UopArtDataSource TryCreate()
	{
		string text = DataPath.TryResolve("artLegacyMUL.uop");
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		return new UopArtDataSource(new Archive(text));
	}

	private static string GetPath(int tileId)
	{
		return $"build/artlegacymul/{tileId:00000000}.tga";
	}

	public bool TryReadLand(int landId, out byte[] data)
	{
		return this._archive.TryReadFile(UopArtDataSource.GetPath(landId & 0x3FFF), out data);
	}

	public bool TryReadItem(int itemId, out byte[] data)
	{
		return this._archive.TryReadFile(UopArtDataSource.GetPath(16384 + (itemId & 0xFFFF)), out data);
	}

	public void Dispose()
	{
		this._archive.Dispose();
	}
}

internal sealed class UopSoundDataSource : ISoundDataSource
{
	private readonly Archive _archive;

	private UopSoundDataSource(Archive archive)
	{
		this._archive = archive;
	}

	public static UopSoundDataSource TryCreate()
	{
		string text = DataPath.TryResolve("soundLegacyMUL.uop");
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		return new UopSoundDataSource(new Archive(text));
	}

	private static string GetPath(int soundId)
	{
		return $"build/soundlegacymul/{soundId:00000000}.dat";
	}

	public bool TryRead(int soundId, out byte[] data)
	{
		if (soundId < 0 || soundId >= 4096)
		{
			data = null;
			return false;
		}
		return this._archive.TryReadFile(UopSoundDataSource.GetPath(soundId), out data);
	}

	public void Dispose()
	{
		this._archive.Dispose();
	}
}

internal abstract class MulIndexedDataSource : IDisposable
{
	protected readonly Stream Index;

	protected readonly Stream Data;

	private readonly object _syncRoot = new object();

	protected MulIndexedDataSource(Stream index, Stream data)
	{
		this.Index = index;
		this.Data = data;
	}

	protected bool TryReadEntry(int entry, out int lookup, out int length, out int extra)
	{
		lookup = -1;
		length = 0;
		extra = 0;
		if (entry < 0 || this.Index == null)
		{
			return false;
		}
		long num = (long)entry * 12L;
		lock (this._syncRoot)
		{
			if (num < 0 || num + 12L > this.Index.Length)
			{
				return false;
			}
			this.Index.Seek(num, SeekOrigin.Begin);
			using BinaryReader binaryReader = new BinaryReader(this.Index, System.Text.Encoding.Default, leaveOpen: true);
			lookup = binaryReader.ReadInt32();
			length = binaryReader.ReadInt32();
			extra = binaryReader.ReadInt32();
			if (lookup < 0 || length <= 0)
			{
				return false;
			}
			return true;
		}
	}

	protected bool TryReadData(int lookup, int length, out byte[] data)
	{
		data = null;
		if (lookup < 0 || length <= 0 || this.Data == null)
		{
			return false;
		}
		lock (this._syncRoot)
		{
			if (lookup + length > this.Data.Length)
			{
				return false;
			}
			byte[] array = new byte[length];
			this.Data.Seek(lookup, SeekOrigin.Begin);
			int num = 0;
			while (num < length)
			{
				int num2 = this.Data.Read(array, num, length - num);
				if (num2 <= 0)
				{
					break;
				}
				num += num2;
			}
			if (num != length)
			{
				return false;
			}
			data = array;
			return true;
		}
	}

	public virtual void Dispose()
	{
		this.Index?.Dispose();
		this.Data?.Dispose();
	}
}

internal sealed class MulGumpDataSource : MulIndexedDataSource, IGumpDataSource
{
	private MulGumpDataSource(Stream index, Stream data)
		: base(index, data)
	{
	}

	public static MulGumpDataSource TryCreate()
	{
		string text = DataPath.TryResolve(Config.GetFile((int)Files.GumpIdx));
		string text2 = DataPath.TryResolve(Config.GetFile((int)Files.GumpMul));
		if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(text2))
		{
			return null;
		}
		return new MulGumpDataSource(File.OpenRead(text), File.OpenRead(text2));
	}

	public bool Exists(int gumpId)
	{
		if (gumpId <= 0)
		{
			return false;
		}
		return this.TryReadEntry(gumpId, out var lookup, out var length, out var extra);
	}

	public bool TryRead(int gumpId, out byte[] data)
	{
		data = null;
		if (!this.TryReadEntry(gumpId, out var lookup, out var length, out var extra))
		{
			return false;
		}
		if (!this.TryReadData(lookup, length, out var data2))
		{
			return false;
		}
		int num = (extra >> 16) & 0xFFFF;
		int num2 = extra & 0xFFFF;
		if (num <= 0 || num2 <= 0)
		{
			return false;
		}
		byte[] array = new byte[data2.Length + 8];
		Buffer.BlockCopy(BitConverter.GetBytes(num), 0, array, 0, 4);
		Buffer.BlockCopy(BitConverter.GetBytes(num2), 0, array, 4, 4);
		Buffer.BlockCopy(data2, 0, array, 8, data2.Length);
		data = array;
		return true;
	}
}

internal sealed class MulArtDataSource : MulIndexedDataSource, IArtDataSource
{
	private MulArtDataSource(Stream index, Stream data)
		: base(index, data)
	{
	}

	public static MulArtDataSource TryCreate()
	{
		string text = DataPath.TryResolve(Config.GetFile((int)Files.ArtIdx));
		string text2 = DataPath.TryResolve(Config.GetFile((int)Files.ArtMul));
		if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(text2))
		{
			return null;
		}
		return new MulArtDataSource(File.OpenRead(text), File.OpenRead(text2));
	}

	public bool TryReadLand(int landId, out byte[] data)
	{
		int entry = landId & 0x3FFF;
		if (!this.TryReadEntry(entry, out var lookup, out var length, out var extra))
		{
			data = null;
			return false;
		}
		return this.TryReadData(lookup, length, out data);
	}

	public bool TryReadItem(int itemId, out byte[] data)
	{
		int entry = 16384 + (itemId & 0xFFFF);
		if (!this.TryReadEntry(entry, out var lookup, out var length, out var extra))
		{
			data = null;
			return false;
		}
		return this.TryReadData(lookup, length, out data);
	}
}

internal sealed class MulSoundDataSource : MulIndexedDataSource, ISoundDataSource
{
	private MulSoundDataSource(Stream index, Stream data)
		: base(index, data)
	{
	}

	public static MulSoundDataSource TryCreate()
	{
		string text = DataPath.TryResolve(Config.GetFile((int)Files.SoundIdx));
		string text2 = DataPath.TryResolve(Config.GetFile((int)Files.SoundMul));
		if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(text2))
		{
			return null;
		}
		return new MulSoundDataSource(File.OpenRead(text), File.OpenRead(text2));
	}

	public bool TryRead(int soundId, out byte[] data)
	{
		if (soundId < 0 || soundId >= 4096)
		{
			data = null;
			return false;
		}
		if (!this.TryReadEntry(soundId, out var lookup, out var length, out var extra))
		{
			data = null;
			return false;
		}
		return this.TryReadData(lookup, length, out data);
	}
}

internal sealed class UooIndexedReader : IDisposable
{
	private const uint MagicNumber = 511683437u;

	private readonly FileStream _stream;

	private readonly BinaryReader _reader;

	private readonly Dictionary<uint, UooEntry> _entries;

	private readonly object _syncRoot = new object();

	public IEnumerable<uint> Ids => this._entries.Keys;

	public UooIndexedReader(string path)
	{
		this._stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		this._reader = new BinaryReader(this._stream);
		this._entries = this.ReadEntries();
	}

	private Dictionary<uint, UooEntry> ReadEntries()
	{
		Dictionary<uint, UooEntry> dictionary = new Dictionary<uint, UooEntry>();
		lock (this._syncRoot)
		{
			this._stream.Seek(0L, SeekOrigin.Begin);
			uint num = this._reader.ReadUInt32();
			if (num != 511683437)
			{
				throw new FormatException("Invalid UOO magic.");
			}
			this._reader.ReadUInt32();
			while (this._stream.Position + 8 <= this._stream.Length)
			{
				uint num2 = this._reader.ReadUInt32();
				uint num3 = this._reader.ReadUInt32();
				if (num3 == 0)
				{
					break;
				}
				long position = this._stream.Position;
				dictionary[num2] = new UooEntry
				{
					Offset = position,
					Length = (int)num3
				};
				this._stream.Seek(num3, SeekOrigin.Current);
			}
		}
		return dictionary;
	}

	public bool TryReadPayload(uint id, out byte[] payload)
	{
		payload = null;
		if (!this._entries.TryGetValue(id, out var value))
		{
			return false;
		}
		lock (this._syncRoot)
		{
			this._stream.Seek(value.Offset, SeekOrigin.Begin);
			payload = this._reader.ReadBytes(value.Length);
			return payload.Length == value.Length;
		}
	}

	public void Dispose()
	{
		this._reader.Dispose();
		this._stream.Dispose();
	}

	private struct UooEntry
	{
		public long Offset;

		public int Length;
	}
}

internal struct UooSpriteData
{
	public ushort Width;

	public ushort Height;

	public ushort MinX;

	public ushort MinY;

	public ushort MaxX;

	public ushort MaxY;

	public ushort[] Pixels;
}

internal sealed class UooArtReader : IDisposable
{
	private readonly UooIndexedReader _reader;

	private readonly Dictionary<uint, UooSpriteData> _cache = new Dictionary<uint, UooSpriteData>();

	public UooArtReader(string path)
	{
		this._reader = new UooIndexedReader(path);
	}

	public bool TryGetSprite(uint id, out UooSpriteData sprite)
	{
		if (this._cache.TryGetValue(id, out sprite))
		{
			return true;
		}
		if (!this._reader.TryReadPayload(id, out var payload))
		{
			sprite = default(UooSpriteData);
			return false;
		}
		using MemoryStream input = new MemoryStream(payload, writable: false);
		using BinaryReader binaryReader = new BinaryReader(input);
		UooSpriteData uooSpriteData = default(UooSpriteData);
		uooSpriteData.Width = binaryReader.ReadUInt16();
		uooSpriteData.Height = binaryReader.ReadUInt16();
		uooSpriteData.MinX = binaryReader.ReadUInt16();
		uooSpriteData.MinY = binaryReader.ReadUInt16();
		uooSpriteData.MaxX = binaryReader.ReadUInt16();
		uooSpriteData.MaxY = binaryReader.ReadUInt16();
		int num = binaryReader.ReadInt32();
		if (num <= 0 || num > payload.Length)
		{
			sprite = default(UooSpriteData);
			return false;
		}
		byte[] array = binaryReader.ReadBytes(num);
		int num2 = uooSpriteData.Width * uooSpriteData.Height * 2;
		byte[] array2 = new byte[num2];
		using (DeflateStream deflateStream = new DeflateStream(new MemoryStream(array, writable: false), CompressionMode.Decompress))
		{
			int num3 = 0;
			while (num3 < num2)
			{
				int num4 = deflateStream.Read(array2, num3, num2 - num3);
				if (num4 <= 0)
				{
					break;
				}
				num3 += num4;
			}
			if (num3 != num2)
			{
				sprite = default(UooSpriteData);
				return false;
			}
		}
		ushort[] array3 = new ushort[uooSpriteData.Width * uooSpriteData.Height];
		Buffer.BlockCopy(array2, 0, array3, 0, array2.Length);
		uooSpriteData.Pixels = array3;
		this._cache[id] = uooSpriteData;
		sprite = uooSpriteData;
		return true;
	}

	public void Dispose()
	{
		this._cache.Clear();
		this._reader.Dispose();
	}
}

internal sealed class UooGumpDataSource : IGumpDataSource
{
	private readonly UooArtReader _reader;

	private readonly Dictionary<int, byte[]> _cache = new Dictionary<int, byte[]>();

	private UooGumpDataSource(UooArtReader reader)
	{
		this._reader = reader;
	}

	public static UooGumpDataSource TryCreate()
	{
		string text = DataPath.TryResolveUoo("gumps.uoo");
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		return new UooGumpDataSource(new UooArtReader(text));
	}

	public bool Exists(int gumpId)
	{
		if (gumpId <= 0)
		{
			return false;
		}
		return this.TryRead(gumpId, out var data);
	}

	public bool TryRead(int gumpId, out byte[] data)
	{
		if (gumpId <= 0)
		{
			data = null;
			return false;
		}
		if (this._cache.TryGetValue(gumpId, out data))
		{
			return true;
		}
		if (!this._reader.TryGetSprite((uint)gumpId, out var sprite))
		{
			data = null;
			return false;
		}
		data = UooEncoding.EncodeGump(sprite);
		this._cache[gumpId] = data;
		return true;
	}

	public void Dispose()
	{
		this._cache.Clear();
		this._reader.Dispose();
	}
}

internal sealed class UooArtDataSource : IArtDataSource
{
	private readonly UooArtReader _landReader;

	private readonly UooArtReader _itemReader;

	private readonly Dictionary<int, byte[]> _landCache = new Dictionary<int, byte[]>();

	private readonly Dictionary<int, byte[]> _itemCache = new Dictionary<int, byte[]>();

	private UooArtDataSource(UooArtReader landReader, UooArtReader itemReader)
	{
		this._landReader = landReader;
		this._itemReader = itemReader;
	}

	public static UooArtDataSource TryCreate()
	{
		string text = DataPath.TryResolveUoo("landtiles.uoo");
		string text2 = DataPath.TryResolveUoo("art.uoo");
		if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(text2))
		{
			return null;
		}
		return new UooArtDataSource(new UooArtReader(text), new UooArtReader(text2));
	}

	public bool TryReadLand(int landId, out byte[] data)
	{
		int num = landId & 0x3FFF;
		if (this._landCache.TryGetValue(num, out data))
		{
			return true;
		}
		if (!this._landReader.TryGetSprite((uint)num, out var sprite))
		{
			data = null;
			return false;
		}
		data = UooEncoding.EncodeLand(sprite);
		this._landCache[num] = data;
		return true;
	}

	public bool TryReadItem(int itemId, out byte[] data)
	{
		int num = itemId & 0xFFFF;
		if (this._itemCache.TryGetValue(num, out data))
		{
			return true;
		}
		if (!this._itemReader.TryGetSprite((uint)num, out var sprite))
		{
			data = null;
			return false;
		}
		data = UooEncoding.EncodeItem(sprite);
		this._itemCache[num] = data;
		return true;
	}

	public void Dispose()
	{
		this._landCache.Clear();
		this._itemCache.Clear();
		this._landReader.Dispose();
		this._itemReader.Dispose();
	}
}

internal static class UooEncoding
{
	private static readonly int[] LandLineOffsets = new int[44]
	{
		21, 20, 19, 18, 17, 16, 15, 14, 13, 12,
		11, 10, 9, 8, 7, 6, 5, 4, 3, 2,
		1, 0, 0, 1, 2, 3, 4, 5, 6, 7,
		8, 9, 10, 11, 12, 13, 14, 15, 16, 17,
		18, 19, 20, 21
	};

	private static readonly int[] LandLineLengths = new int[44]
	{
		1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
		11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
		21, 22, 22, 21, 20, 19, 18, 17, 16, 15,
		14, 13, 12, 11, 10, 9, 8, 7, 6, 5,
		4, 3, 2, 1
	};

	public static byte[] EncodeGump(UooSpriteData sprite)
	{
		int width = sprite.Width;
		int height = sprite.Height;
		List<int> list = new List<int>(height);
		List<ushort> list2 = new List<ushort>(width * height);
		for (int i = 0; i < height; i++)
		{
			list.Add((list2.Count >> 1) + height);
			int num = i * width;
			int num2 = 0;
			while (num2 < width)
			{
				ushort num3 = sprite.Pixels[num + num2];
				int num4 = num2 + 1;
				while (num4 < width && sprite.Pixels[num + num4] == num3)
				{
					num4++;
				}
				int num5 = num4 - num2;
				list2.Add(num3);
				list2.Add((ushort)num5);
				num2 = num4;
			}
		}
		byte[] array = new byte[8 + height * 4 + list2.Count * 2];
		Buffer.BlockCopy(BitConverter.GetBytes(width), 0, array, 0, 4);
		Buffer.BlockCopy(BitConverter.GetBytes(height), 0, array, 4, 4);
		for (int j = 0; j < height; j++)
		{
			Buffer.BlockCopy(BitConverter.GetBytes(list[j]), 0, array, 8 + j * 4, 4);
		}
		for (int k = 0; k < list2.Count; k++)
		{
			Buffer.BlockCopy(BitConverter.GetBytes(list2[k]), 0, array, 8 + height * 4 + k * 2, 2);
		}
		return array;
	}

	public static byte[] EncodeItem(UooSpriteData sprite)
	{
		int width = sprite.Width;
		int height = sprite.Height;
		List<ushort> list = new List<ushort>(height);
		List<ushort> list2 = new List<ushort>(width * height * 2);
		for (int i = 0; i < height; i++)
		{
			list.Add((ushort)list2.Count);
			int num = i * width;
			int num2 = 0;
			int num3 = 0;
			while (num2 < width)
			{
				while (num2 < width && sprite.Pixels[num + num2] == 0)
				{
					num2++;
				}
				if (num2 >= width)
				{
					break;
				}
				int num4 = num2;
				while (num2 < width && sprite.Pixels[num + num2] != 0)
				{
					num2++;
				}
				int num5 = num2 - num4;
				int num6 = num4 - num3;
				if (num6 < 0)
				{
					num6 = 0;
				}
				list2.Add((ushort)num6);
				list2.Add((ushort)num5);
				for (int j = 0; j < num5; j++)
				{
					list2.Add(sprite.Pixels[num + num4 + j]);
				}
				num3 = num4 + num5;
			}
			list2.Add(0);
			list2.Add(0);
		}
		byte[] array = new byte[8 + height * 2 + list2.Count * 2];
		Buffer.BlockCopy(BitConverter.GetBytes(0), 0, array, 0, 4);
		Buffer.BlockCopy(BitConverter.GetBytes((short)width), 0, array, 4, 2);
		Buffer.BlockCopy(BitConverter.GetBytes((short)height), 0, array, 6, 2);
		for (int k = 0; k < height; k++)
		{
			Buffer.BlockCopy(BitConverter.GetBytes((short)list[k]), 0, array, 8 + k * 2, 2);
		}
		for (int l = 0; l < list2.Count; l++)
		{
			Buffer.BlockCopy(BitConverter.GetBytes((short)list2[l]), 0, array, 8 + height * 2 + l * 2, 2);
		}
		return array;
	}

	public static byte[] EncodeLand(UooSpriteData sprite)
	{
		byte[] array = new byte[2048];
		ushort[] array2 = new ushort[1024];
		int num = 0;
		for (int i = 0; i < 44; i++)
		{
			int num2 = UooEncoding.LandLineOffsets[i];
			int num3 = UooEncoding.LandLineLengths[i];
			for (int j = 0; j < num3; j++)
			{
				int num4 = num2 + j;
				ushort num5 = 0;
				if (i >= 0 && i < sprite.Height && num4 >= 0 && num4 < sprite.Width)
				{
					num5 = sprite.Pixels[i * sprite.Width + num4];
				}
				array2[num++] = num5;
			}
		}
		Buffer.BlockCopy(array2, 0, array, 0, array.Length);
		return array;
	}
}

internal sealed class UooAnimationDirection
{
	private readonly byte[] _compressedData;

	private readonly uint _frameCount;

	private UooAnimationFrameData[] _frames;

	private int _maxHeight;

	public uint FrameCount => this._frameCount;

	public int MaxHeight => this._maxHeight;

	public UooAnimationDirection(uint frameCount, byte[] compressedData)
	{
		this._frameCount = frameCount;
		this._compressedData = compressedData;
		this._frames = null;
		this._maxHeight = 0;
	}

	public UooAnimationFrameData[] GetFrames()
	{
		if (this._frames != null)
		{
			return this._frames;
		}
		using DeflateStream deflateStream = new DeflateStream(new MemoryStream(this._compressedData, writable: false), CompressionMode.Decompress);
		using MemoryStream memoryStream = new MemoryStream();
		deflateStream.CopyTo(memoryStream);
		memoryStream.Seek(0L, SeekOrigin.Begin);
		using BinaryReader binaryReader = new BinaryReader(memoryStream);
		UooAnimationFrameData[] array = new UooAnimationFrameData[(int)this._frameCount];
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			UooAnimationFrameData uooAnimationFrameData = default(UooAnimationFrameData);
			uooAnimationFrameData.CenterX = binaryReader.ReadInt16();
			uooAnimationFrameData.CenterY = binaryReader.ReadInt16();
			uooAnimationFrameData.Width = binaryReader.ReadInt16();
			uooAnimationFrameData.Height = binaryReader.ReadInt16();
			int num2 = binaryReader.ReadInt32();
			if (num2 < 0)
			{
				uooAnimationFrameData.Pixels = null;
				array[i] = uooAnimationFrameData;
				continue;
			}
			if (num2 > 0)
			{
				byte[] array2 = binaryReader.ReadBytes(num2 * 2);
				if (array2.Length != num2 * 2)
				{
					uooAnimationFrameData.Pixels = null;
					array[i] = uooAnimationFrameData;
					continue;
				}
				ushort[] array3 = new ushort[num2];
				Buffer.BlockCopy(array2, 0, array3, 0, array2.Length);
				uooAnimationFrameData.Pixels = array3;
				int num3 = uooAnimationFrameData.CenterY + uooAnimationFrameData.Height;
				if (num3 > num)
				{
					num = num3;
				}
			}
			else
			{
				uooAnimationFrameData.Pixels = null;
			}
			array[i] = uooAnimationFrameData;
		}
		this._frames = array;
		this._maxHeight = num;
		return this._frames;
	}
}

internal sealed class UooAnimationModel
{
	private readonly Dictionary<uint, Dictionary<uint, UooAnimationDirection>> _actions = new Dictionary<uint, Dictionary<uint, UooAnimationDirection>>();

	public byte[] ActionMappings { get; set; }

	public IEnumerable<uint> Actions => this._actions.Keys;

	public void AddDirection(uint action, uint direction, UooAnimationDirection value)
	{
		if (!this._actions.TryGetValue(action, out var value2))
		{
			value2 = new Dictionary<uint, UooAnimationDirection>();
			this._actions[action] = value2;
		}
		value2[direction] = value;
	}

	public bool TryGetDirection(uint action, uint direction, bool includeMappings, out UooAnimationDirection value)
	{
		if (includeMappings && this.ActionMappings != null && action < (uint)this.ActionMappings.Length)
		{
			byte b = this.ActionMappings[action];
			if (b != byte.MaxValue)
			{
				action = b;
			}
		}
		value = null;
		if (this._actions.TryGetValue(action, out var value2))
		{
			return value2.TryGetValue(direction, out value);
		}
		return false;
	}

	public bool HasAction(uint action)
	{
		return this._actions.ContainsKey(action);
	}
}

internal sealed class UooAnimationReader : IDisposable
{
	private readonly UooIndexedReader _reader;

	private readonly Dictionary<uint, UooAnimationModel> _cache = new Dictionary<uint, UooAnimationModel>();

	public IEnumerable<uint> ModelIds => this._reader.Ids;

	public UooAnimationReader(string path)
	{
		this._reader = new UooIndexedReader(path);
	}

	public bool TryGetModel(uint modelId, out UooAnimationModel model)
	{
		if (this._cache.TryGetValue(modelId, out model))
		{
			return true;
		}
		if (!this._reader.TryReadPayload(modelId, out var payload))
		{
			model = null;
			return false;
		}
		using MemoryStream input = new MemoryStream(payload, writable: false);
		using BinaryReader binaryReader = new BinaryReader(input);
		UooAnimationModel uooAnimationModel = new UooAnimationModel();
		uint num = binaryReader.ReadUInt32();
		int num2 = binaryReader.ReadInt32();
		if (num2 > 0)
		{
			uooAnimationModel.ActionMappings = binaryReader.ReadBytes(num2);
		}
		for (uint num3 = 0u; num3 < num; num3++)
		{
			uint num4 = binaryReader.ReadUInt32();
			uint num5 = binaryReader.ReadUInt32();
			for (uint num6 = 0u; num6 < num5; num6++)
			{
				uint num7 = binaryReader.ReadUInt32();
				uint num8 = binaryReader.ReadUInt32();
				uint num9 = binaryReader.ReadUInt32();
				byte[] compressedData = binaryReader.ReadBytes((int)num9);
				uooAnimationModel.AddDirection(num4, num7, new UooAnimationDirection(num8, compressedData));
			}
		}
		this._cache[modelId] = uooAnimationModel;
		model = uooAnimationModel;
		return true;
	}

	public bool TryGetFrames(uint modelId, uint action, uint direction, bool includeMappings, out UooAnimationFrameData[] frames, out int maxHeight)
	{
		frames = null;
		maxHeight = 0;
		if (!this.TryGetModel(modelId, out var model))
		{
			return false;
		}
		if (!model.TryGetDirection(action, direction, includeMappings, out var value))
		{
			return false;
		}
		frames = value.GetFrames();
		maxHeight = value.MaxHeight;
		return frames != null && frames.Length > 0;
	}

	public void Dispose()
	{
		this._cache.Clear();
		this._reader.Dispose();
	}
}

internal sealed class UooAnimationDataSource : IAnimationDataSource
{
	private readonly UooAnimationReader _reader;

	private readonly Entry3D[] _index;

	private readonly int _count;

	private readonly List<UooLookup> _lookups;

	public bool IsUoo => true;

	private UooAnimationDataSource(UooAnimationReader reader, Entry3D[] index, int count, List<UooLookup> lookups)
	{
		this._reader = reader;
		this._index = index;
		this._count = count;
		this._lookups = lookups;
	}

	public static UooAnimationDataSource TryCreate()
	{
		string text = DataPath.TryResolveUoo("anim.uoo");
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		UooAnimationReader uooAnimationReader = new UooAnimationReader(text);
		if (!UooAnimationDataSource.TryBuildIndex(uooAnimationReader, out var index, out var count, out var lookups))
		{
			uooAnimationReader.Dispose();
			return null;
		}
		return new UooAnimationDataSource(uooAnimationReader, index, count, lookups);
	}

	private static int ComputeRealId(int model, int action, int direction)
	{
		if (direction > 4)
		{
			direction -= (direction - 4) * 2;
		}
		int num = ((model >= 400) ? ((model - 400) * 175 + 35000) : ((model < 200) ? (model * 110) : ((model - 200) * 65 + 22000)));
		return num + action * 5 + direction;
	}

	private static bool TryBuildIndex(UooAnimationReader reader, out Entry3D[] entries, out int count, out List<UooLookup> lookups)
	{
		entries = null;
		count = 0;
		lookups = new List<UooLookup>();
		Dictionary<int, Entry3D> dictionary = new Dictionary<int, Entry3D>();
		Dictionary<string, int> dictionary2 = new Dictionary<string, int>(StringComparer.Ordinal);
		int num = -1;
		foreach (uint modelId in reader.ModelIds)
		{
			if (modelId > 4096)
			{
				continue;
			}
			if (!reader.TryGetModel(modelId, out var model))
			{
				continue;
			}
			int num2 = -1;
			foreach (uint action in model.Actions)
			{
				if ((int)action > num2)
				{
					num2 = (int)action;
				}
			}
			if (model.ActionMappings != null && model.ActionMappings.Length - 1 > num2)
			{
				num2 = model.ActionMappings.Length - 1;
			}
			for (int i = 0; i <= num2; i++)
			{
				uint num3 = (uint)i;
				if (model.ActionMappings != null && i < model.ActionMappings.Length)
				{
					byte b = model.ActionMappings[i];
					if (b != byte.MaxValue)
					{
						num3 = b;
					}
				}
				if (!model.HasAction(num3))
				{
					continue;
				}
				for (int j = 0; j < 5; j++)
				{
					if (!model.TryGetDirection(num3, (uint)j, includeMappings: false, out var value))
					{
						continue;
					}
					string text = modelId + ":" + num3 + ":" + j;
					if (!dictionary2.TryGetValue(text, out var value2))
					{
						value2 = lookups.Count;
						dictionary2[text] = value2;
						lookups.Add(new UooLookup
						{
							Model = modelId,
							Action = num3,
							Direction = (uint)j
						});
					}
					int num4 = UooAnimationDataSource.ComputeRealId((int)modelId, i, j);
					if (num4 < 0)
					{
						continue;
					}
					Entry3D value3 = default(Entry3D);
					value3.m_Lookup = value2;
					value3.m_Length = 1;
					value3.m_Extra = (int)Math.Min(value.FrameCount, 255u);
					dictionary[num4] = value3;
					if (num4 > num)
					{
						num = num4;
					}
				}
			}
		}
		if (num < 0)
		{
			return false;
		}
		Entry3D[] array = new Entry3D[num + 1];
		for (int k = 0; k < array.Length; k++)
		{
			array[k].m_Lookup = -1;
		}
		foreach (KeyValuePair<int, Entry3D> item in dictionary)
		{
			array[item.Key] = item.Value;
		}
		entries = array;
		count = array.Length;
		return true;
	}

	public bool TryLoadIndex(int index, out Entry3D[] entries, out int count)
	{
		entries = this._index;
		count = this._count;
		return true;
	}

	public bool TryGetFrames(int lookup, out UooAnimationFrameData[] frames, out int maxHeight)
	{
		frames = null;
		maxHeight = 0;
		if (lookup < 0 || lookup >= this._lookups.Count)
		{
			return false;
		}
		UooLookup uooLookup = this._lookups[lookup];
		return this._reader.TryGetFrames(uooLookup.Model, uooLookup.Action, uooLookup.Direction, includeMappings: false, out frames, out maxHeight);
	}

	public void Dispose()
	{
		this._reader.Dispose();
		this._lookups.Clear();
	}

	private struct UooLookup
	{
		public uint Model;

		public uint Action;

		public uint Direction;
	}
}

internal sealed class NullAnimationDataSource : IAnimationDataSource
{
	public static readonly NullAnimationDataSource Instance = new NullAnimationDataSource();

	public bool IsUoo => false;

	private NullAnimationDataSource()
	{
	}

	public bool TryLoadIndex(int index, out Entry3D[] entries, out int count)
	{
		entries = null;
		count = 0;
		return false;
	}

	public bool TryGetFrames(int lookup, out UooAnimationFrameData[] frames, out int maxHeight)
	{
		frames = null;
		maxHeight = 0;
		return false;
	}

	public void Dispose()
	{
	}
}
