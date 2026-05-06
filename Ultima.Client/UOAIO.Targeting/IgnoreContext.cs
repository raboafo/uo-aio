using UOAIO.Profiles;

namespace UOAIO.Targeting;

internal class IgnoreContext : ActionContext
{
	private Mobile m_Mobile;

	public IgnoreContext(Mobile mob)
	{
		this.m_Mobile = mob;
	}

	public override void OnDispatch()
	{
		this.m_Mobile.QueryStats();
	}

	public override void OnFinish()
	{
		IgnoreList ignoreList = Player.Current.IgnoreList;
		Character character = ignoreList[this.m_Mobile];
		if (character == null)
		{
			if (this.m_Mobile.HasName)
			{
				ignoreList.Characters.Add(new Character(this.m_Mobile));
				this.m_Mobile.IsIgnored = true;
				this.m_Mobile.AddTextMessage("", "- ignored -", Engine.DefaultFont, Hues.Load(144), unremovable: true);
				Engine.AddTextMessage("They have been added to the ignore list.", Engine.DefaultFont, Hues.Load(144));
			}
			else
			{
				Engine.AddTextMessage("Unable to friend that person.");
			}
		}
		else
		{
			ignoreList.Characters.Remove(character);
			this.m_Mobile.IsIgnored = false;
			this.m_Mobile.AddTextMessage("", "- no longer ignored -", Engine.DefaultFont, Hues.Load(63), unremovable: true);
			Engine.AddTextMessage("They have been removed from the ignore list.", Engine.DefaultFont, Hues.Load(63));
		}
	}
}
