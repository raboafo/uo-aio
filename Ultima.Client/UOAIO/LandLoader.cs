namespace UOAIO;

public class LandLoader : ILoader
{
	private int m_LandID;

	public LandLoader(int LandID)
	{
		this.m_LandID = LandID;
	}

	public void Load()
	{
		MapLighting.CheckStretchTable();
		if (!MapLighting.m_AlwaysStretch[this.m_LandID & 0x3FFF])
		{
			Hues.Default.GetTerrainIsometric(this.m_LandID);
		}
		int texture = Map.GetTexture(this.m_LandID);
		if (texture > 0 && texture < 16384)
		{
			Hues.Default.GetTerrainTexture(texture);
		}
	}
}
