using System;
using UOAIO;

namespace Ultima.Data;

public static class Archives
{
	private static readonly Lazy<Archive> sound;

	private static readonly Lazy<Archive> tileArt;

	private static readonly Lazy<Archive> gumpArt;

	public static Archive Sound => Archives.sound.Value;

	public static Archive TileArt => Archives.tileArt.Value;

	public static Archive GumpArt => Archives.gumpArt.Value;

	private static Lazy<Archive> LazyArchive(string type)
	{
		return new Lazy<Archive>(() => new Archive(Engine.FileManager.ResolveMUL(type + ".uop")));
	}

	public static void Shutdown()
	{
		if (Archives.sound.IsValueCreated)
		{
			Archives.sound.Value.Dispose();
		}
		if (Archives.tileArt.IsValueCreated)
		{
			Archives.tileArt.Value.Dispose();
		}
		if (Archives.gumpArt.IsValueCreated)
		{
			Archives.gumpArt.Value.Dispose();
		}
	}

	static Archives()
	{
		Archives.sound = Archives.LazyArchive("soundLegacyMUL");
		Archives.tileArt = Archives.LazyArchive("artLegacyMUL");
		Archives.gumpArt = Archives.LazyArchive("gumpartLegacyMUL");
	}
}
