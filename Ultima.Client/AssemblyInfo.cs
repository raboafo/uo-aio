using System.Reflection;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
internal static class AssemblyInfo
{
	public const string Version = "4.0.0.0";

	public static string GetVeritasReference(string simpleName)
	{
		return simpleName + ", Version=4.0.0.0, Culture=neutral, PublicKeyToken=6cc7e8bd89c5c6bf";
	}
}
