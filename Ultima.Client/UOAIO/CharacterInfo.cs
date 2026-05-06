namespace UOAIO;

public class CharacterInfo
{
	public string Name { get; set; }

	public string Password { get; set; }

	public int Index { get; set; }

	public CharacterInfo()
	{
	}

	public CharacterInfo(string name, string password, int index)
	{
		this.Name = name;
		this.Password = password;
		this.Index = index;
	}
}
