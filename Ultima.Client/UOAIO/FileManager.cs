using System;
using System.IO;
using System.Windows.Forms;
using UOAIO.ShardRuntime;
using Veritas;

namespace UOAIO;

public class FileManager
{
	private Archive _archive;

	private string m_AppBasePath = "";

	private string m_FilePath = "";

	private bool m_Error;

	public string FilePath => this.m_FilePath;

	public bool Error => this.m_Error;

	public string ResolveMUL(Files File)
	{
		return Path.Combine(this.m_FilePath, Config.GetFile((int)File));
	}

	public string ResolveMUL(string Path)
	{
		return System.IO.Path.Combine(this.m_FilePath, Path);
	}

	public FileStream CreateUnique(string basePath, string extension)
	{
		string path = this.BasePath($"{basePath}{extension}");
		int num = 0;
		do
		{
			try
			{
				return new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
			}
			catch
			{
				path = $"{basePath}{++num}{extension}";
			}
		}
		while (num < 1000);
		throw new Exception(string.Format("Unable to create unique file (basePath='{0}', extension='{0}')", basePath, extension));
	}

	internal ArchivedFile GetArchivedFile(string path)
	{
		return this._archive.FindFile(path);
	}

	private string QueryPathFromUser()
	{
		using FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
		folderBrowserDialog.Description = "Select the directory that contains the shard client assets.";
		folderBrowserDialog.ShowNewFolderButton = false;
		string rootPath = Path.GetPathRoot(this.m_AppBasePath);
		if (!string.IsNullOrWhiteSpace(rootPath))
		{
			folderBrowserDialog.SelectedPath = rootPath;
		}

		if (folderBrowserDialog.ShowDialog() == DialogResult.OK && Directory.Exists(folderBrowserDialog.SelectedPath))
		{
			return folderBrowserDialog.SelectedPath;
		}

		return null;
	}

	private string ResolveShardAssetPath()
	{
		ShardDefinition activeShardRuntime = Engine.ActiveShardRuntime;
		if (activeShardRuntime != null && activeShardRuntime.Metadata != null &&
			activeShardRuntime.Metadata.TryGetValue(ShardMetadataKeys.ClientAssetPath, out string assetPath) &&
			!string.IsNullOrWhiteSpace(assetPath))
		{
			try
			{
				string fullPath = Path.GetFullPath(assetPath);
				if (Directory.Exists(fullPath))
				{
					return fullPath;
				}
			}
			catch
			{
			}
		}

		return this.QueryPathFromUser();
	}

	public FileManager()
	{
		this._archive = Archive.AcquireArchive("ultima");
		this.m_AppBasePath = ClientRuntimeEnvironment.AppBaseDirectory;
		this.m_FilePath = this.ResolveShardAssetPath();
		this.m_Error = this.m_FilePath == null;
	}

	public void Dispose()
	{
	}

	public string BasePath(string Path)
	{
		return ClientRuntimeEnvironment.AppPath(Path);
	}

	public string RuntimeDataPath(string Path)
	{
		return ClientRuntimeEnvironment.RuntimeDataPath(Path);
	}

	public FileStream CreateUniqueRuntime(string basePath, string extension)
	{
		string path = this.RuntimeDataPath($"{basePath}{extension}");
		int num = 0;
		do
		{
			try
			{
				string directory = System.IO.Path.GetDirectoryName(path);
				if (!string.IsNullOrWhiteSpace(directory))
				{
					Directory.CreateDirectory(directory);
				}

				return new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
			}
			catch
			{
				path = this.RuntimeDataPath($"{basePath}{++num}{extension}");
			}
		}
		while (num < 1000);
		throw new Exception(string.Format("Unable to create unique runtime file (basePath='{0}', extension='{0}')", basePath, extension));
	}

	public Stream OpenMUL(Files File)
	{
		return this.OpenRead(Path.Combine(this.m_FilePath, Config.GetFile((int)File)));
	}

	public Stream OpenMUL(string Path)
	{
		return this.OpenRead(System.IO.Path.Combine(this.m_FilePath, Path));
	}

	protected Stream OpenRead(string Path)
	{
		return File.OpenRead(Path);
	}
}
