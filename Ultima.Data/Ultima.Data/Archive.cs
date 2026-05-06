#define DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Ultima.Data;

public sealed class Archive : IDisposable
{
	private struct Entry
	{
		private readonly ulong key;

		private readonly SafeBuffer buffer;

		private readonly long offset;

		private readonly long length;

		public ulong Key => this.key;

		public Entry(ulong key, SafeBuffer buffer, long offset, long length)
		{
			this.key = key;
			this.buffer = buffer;
			this.offset = offset;
			this.length = length;
		}

		public unsafe T Open<T>(Func<Stream, T> func)
		{
			byte* pointer = null;
			this.buffer.AcquirePointer(ref pointer);
			try
			{
				using UnmanagedMemoryStream stream = new UnmanagedMemoryStream(pointer + this.offset, this.length);
				return func(stream);
			}
			finally
			{
				this.buffer.ReleasePointer();
			}
		}

		public static bool TryRead(MemoryMappedViewStream stream, BinaryReader reader, out Entry entry)
		{
			long fileOffset = reader.ReadInt64();
			int headerSize = reader.ReadInt32();
			int deflatedSize = reader.ReadInt32();
			int inflatedSize = reader.ReadInt32();
			ulong pathDigest = reader.ReadUInt64();
			int entryCrc32 = reader.ReadInt32();
			short compressionMethod = reader.ReadInt16();
			if (fileOffset == 0)
			{
				entry = default(Entry);
				return false;
			}
			entry = new Entry(pathDigest, stream.SafeMemoryMappedViewHandle, fileOffset + headerSize, deflatedSize);
			return true;
		}
	}

	private readonly MemoryMappedFile file;

	private readonly MemoryMappedViewStream stream;

	private readonly Dictionary<ulong, Entry> entries;

	private static Dictionary<MemoryMappedViewStream, BinaryReader> readers;

	private static void AssertZeroMsb(long value)
	{
		Debug.Assert(value >> 32 == 0);
	}

	public Archive(string path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		this.file = MemoryMappedFile.CreateFromFile(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), null, 0L, MemoryMappedFileAccess.CopyOnWrite, null, HandleInheritability.None, leaveOpen: false);
		this.stream = this.file.CreateViewStream(0L, 0L, MemoryMappedFileAccess.CopyOnWrite);
		this.entries = Archive.ReadEntries(this.stream).ToDictionary((Entry x) => x.Key);
	}

	public void Dispose()
	{
		this.stream.Dispose();
		this.file.Dispose();
	}

	public bool FileExists(string path)
	{
		ulong key = this.KeyForPath(path);
		return this.entries.ContainsKey(key);
	}

	public T Open<T>(string path, Func<Stream, T> func)
	{
		ulong key = this.KeyForPath(path);
		if (this.entries.TryGetValue(key, out var entry))
		{
			return entry.Open(func);
		}
		return default(T);
	}

	public bool TryReadFile(string path, out byte[] data)
	{
		data = this.Open(path, delegate(Stream stream)
		{
			byte[] array = new byte[stream.Length];
			stream.Fill(array, 0, array.Length);
			return array;
		});
		return data != null;
	}

	private ulong KeyForPath(string path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		return __jenkins_lookup3.digestOf(path.ToLowerInvariant());
	}

	private static IEnumerable<Entry> ReadEntries(MemoryMappedViewStream stream)
	{
		BinaryReader reader = null;
		if (!Archive.readers.TryGetValue(stream, out reader))
		{
			Dictionary<MemoryMappedViewStream, BinaryReader> dictionary = Archive.readers;
			BinaryReader value;
			reader = (value = new BinaryReader(stream, Encoding.Default));
			dictionary[stream] = value;
		}
		int magic = reader.ReadInt32();
		if (magic != 5265741)
		{
			throw new FormatException("Magic mismatch.");
		}
		int version = reader.ReadInt32();
		if (version != 5)
		{
			Debug.Print(version.ToString());
			throw new FormatException("Version mismatch.");
		}
		reader.ReadInt32();
		long index = reader.ReadInt64();
		while (index != 0)
		{
			Archive.AssertZeroMsb(index);
			reader.BaseStream.Seek(index, SeekOrigin.Begin);
			int count = reader.ReadInt32();
			index = reader.ReadInt64();
			int i = 0;
			while (i < count)
			{
				if (Entry.TryRead(stream, reader, out var entry))
				{
					yield return entry;
				}
				entry = default(Entry);
				int num = i + 1;
				i = num;
			}
		}
	}

	static Archive()
	{
		Archive.readers = new Dictionary<MemoryMappedViewStream, BinaryReader>();
	}
}
