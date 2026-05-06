namespace UOAIO.Assets;

public interface IAssetProvider
{
	IAudioProvider AudioProvider { get; }

	IGraphicProvider GetGraphicProvider(int hue);
}
