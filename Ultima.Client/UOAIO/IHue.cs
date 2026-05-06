using System;
using UOAIO.Assets;

namespace UOAIO;

public interface IHue : IGraphicProvider, IDisposable
{
	Palette Palette { get; }

	ShaderData ShaderData { get; }

	ushort Pixel(ushort input);

	int Pixel32(int input);

	unsafe void CopyPixels(void* pvSrc, void* pvDest, int Pixels);

	unsafe void CopyEncodedLine(ushort* pSrc, ushort* pSrcEnd, ushort* pDest, ushort* pEnd);

	int HueID();
}
