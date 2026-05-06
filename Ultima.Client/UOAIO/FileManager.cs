using System;
using System.IO;
using System.Security;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;
using Veritas;

namespace UOAIO;

public class FileManager
{
	private Archive _archive;

	private string m_BasePath = "";

	private string m_FilePath = "";

	private bool m_Error;

	private static readonly string[] RootKeyNames;

	private const string RegistryKeyPattern = "^HKEY_LOCAL_MACHINE\\\\Software(?:\\\\Wow6432Node)?\\\\(?:EA Games|Origin Worlds Online|Electronic Arts)\\\\(Ultima Online|EA Games)[^\\\\]*(?:\\\\(KR Legacy Beta|Ultima Online Classic|Ultima Online Stygian Abyss Classic))?(?:\\\\[23]d)?(?:\\\\1\\.0+(?:\\.0+)?)?$";

	private static readonly Regex RegistryKeyRegex;

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

	private static string GetPathFromRegistry()
	{
		string[] rootKeyNames = FileManager.RootKeyNames;
		foreach (string name in rootKeyNames)
		{
			try
			{
				using RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(name);
				if (registryKey == null)
				{
					continue;
				}
				string text = registryKey.GetValue("InstallDir") as string;
				if (!string.IsNullOrEmpty(text))
				{
					text = Path.GetFullPath(text);
					if (Directory.Exists(text))
					{
						return text;
					}
				}
				text = registryKey.GetValue("ExePath") as string;
				if (!string.IsNullOrEmpty(text))
				{
					text = Path.GetDirectoryName(text);
					if (Directory.Exists(text))
					{
						return text;
					}
				}
				if (FileManager.FindRegistryPathAux(registryKey, out text))
				{
					return text;
				}
			}
			catch (SecurityException)
			{
			}
		}
		return null;
	}

	private static bool FindRegistryPathAux(RegistryKey registryKey, out string path)
	{
		if (registryKey != null)
		{
			string[] subKeyNames = registryKey.GetSubKeyNames();
			foreach (string name in subKeyNames)
			{
				try
				{
					using RegistryKey registryKey2 = registryKey.OpenSubKey(name);
					if (registryKey2 == null || !FileManager.RegistryKeyRegex.IsMatch(registryKey2.Name))
					{
						continue;
					}
					path = registryKey2.GetValue("InstallDir") as string;
					if (!string.IsNullOrEmpty(path))
					{
						path = Path.GetFullPath(path);
						if (Directory.Exists(path))
						{
							return true;
						}
					}
					path = registryKey2.GetValue("ExePath") as string;
					if (!string.IsNullOrEmpty(path))
					{
						path = Path.GetDirectoryName(path);
						if (Directory.Exists(path))
						{
							return true;
						}
					}
					if (FileManager.FindRegistryPathAux(registryKey2, out path))
					{
						return true;
					}
				}
				catch (SecurityException)
				{
				}
			}
		}
		path = null;
		return false;
	}

	private string QueryPathFromUser()
	{
		using System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
		openFileDialog.CheckPathExists = true;
		openFileDialog.CheckFileExists = false;
		openFileDialog.FileName = "Client.exe";
		openFileDialog.Filter = "Client.exe|Client.exe";
		openFileDialog.Title = "Find your UO directory";
		openFileDialog.InitialDirectory = Path.GetPathRoot(this.m_BasePath);
		if (openFileDialog.ShowDialog() == DialogResult.OK)
		{
			return Path.GetDirectoryName(openFileDialog.FileName);
		}
		return null;
	}

	public FileManager()
	{
		this._archive = Archive.AcquireArchive("ultima");
		this.m_BasePath = Directory.GetCurrentDirectory();
		this.m_FilePath = FileManager.GetPathFromRegistry() ?? this.QueryPathFromUser();
		this.m_Error = this.m_FilePath == null;
	}

	public void Dispose()
	{
	}

	public string BasePath(string Path)
	{
		return System.IO.Path.Combine(this.m_BasePath, Path);
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

	static FileManager()
	{
		FileManager.RootKeyNames = new string[8] { "Software\\Electronic Arts\\EA Games\\Ultima Online Classic", "Software\\Electronic Arts\\EA Games\\Ultima Online Stygian Abyss Classic", "Software\\EA Games", "Software\\Origin Worlds Online", "SOFTWARE\\Wow6432Node\\Electronic Arts\\EA Games\\Ultima Online Classic", "SOFTWARE\\Wow6432Node\\Electronic Arts\\EA Games\\Ultima Online Stygian Abyss Classic", "Software\\Wow6432Node\\EA Games", "Software\\Wow6432Node\\Origin Worlds Online" };
		FileManager.RegistryKeyRegex = new Regex("^HKEY_LOCAL_MACHINE\\\\Software(?:\\\\Wow6432Node)?\\\\(?:EA Games|Origin Worlds Online|Electronic Arts)\\\\(Ultima Online|EA Games)[^\\\\]*(?:\\\\(KR Legacy Beta|Ultima Online Classic|Ultima Online Stygian Abyss Classic))?(?:\\\\[23]d)?(?:\\\\1\\.0+(?:\\.0+)?)?$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
	}
}
