using System;
using System.IO;
using SharpDX.Direct3D9;
using Veritas;

namespace UOAIO;

public class ShaderData
{
	private static readonly Version _requiredVersion;

	private static Version _deviceVersion;

	private PixelShader _pixelShader;

	private Texture _dataSurface;

	private Callback _renderCallback;

	private TextureTransparency _transparency;

	public static Version RequiredVersion => ShaderData._requiredVersion;

	public static Version DeviceVersion
	{
		get
		{
			return ShaderData._deviceVersion;
		}
		set
		{
			ShaderData._deviceVersion = value;
		}
	}

	public static bool IsSupported => ShaderData._deviceVersion != null && ShaderData._deviceVersion >= ShaderData._requiredVersion;

	public PixelShader PixelShader => this._pixelShader;

	public Texture DataSurface => this._dataSurface;

	public Callback RenderCallback
	{
		get
		{
			return this._renderCallback;
		}
		set
		{
			this._renderCallback = value;
		}
	}

	public TextureTransparency Transparency => this._transparency;

	public static void AssembleAndDumpShaders()
	{
		ArchivedFolder archivedFolder = Archive.AcquireArchive("ultima").FindFolder("play/shaders");
		foreach (ArchivedFile file in archivedFolder.Files)
		{
			if (!file.FileName.EndsWith(".psh"))
			{
				continue;
			}
			string fileName = file.FileName.Replace(".psh", ".cso");
			using Stream stream = file.Download();
			try
			{
				byte[] array = new byte[stream.Length];
				stream.Read(array, 0, array.Length);
				using ShaderBytecode shaderBytecode = (ShaderBytecode)ShaderBytecode.Assemble(array, ShaderFlags.OptimizationLevel1);
				shaderBytecode.Save(fileName);
			}
			catch (Exception ex)
			{
				Debug.Error(ex);
			}
		}
	}

	public ShaderData(string pixelShaderPath, Texture dataSurface, TextureTransparency transparency)
	{
		if (ShaderData.IsSupported)
		{
			ArchivedFile archivedFile = Engine.FileManager.GetArchivedFile("play/shaders/" + pixelShaderPath);
			if (archivedFile != null)
			{
				using Stream stream = archivedFile.Download();
				try
				{
					byte[] array = new byte[stream.Length];
					stream.Read(array, 0, array.Length);
					using ShaderBytecode function = new ShaderBytecode(array);
					this._pixelShader = new PixelShader(Engine.m_Device, function);
				}
				catch (Exception ex)
				{
					Debug.Error(ex);
				}
			}
			this._dataSurface = dataSurface;
		}
		this._transparency = transparency;
	}

	public ShaderData(PixelShader pixelShader, Texture dataSurface, TextureTransparency transparency)
	{
		if (ShaderData.IsSupported)
		{
			this._pixelShader = pixelShader;
			this._dataSurface = dataSurface;
		}
		this._transparency = transparency;
	}

	static ShaderData()
	{
		ShaderData._requiredVersion = new Version(2, 0);
	}
}
