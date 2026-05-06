using UOAIO.Profiles;

namespace UOAIO.Targeting;

internal class FriendContext : ActionContext
{
	private Mobile m_Mobile;

	public FriendContext(Mobile mob)
	{
		this.m_Mobile = mob;
	}

	public override void OnDispatch()
	{
		this.m_Mobile.QueryStats();
	}

	public override void OnFinish()
	{
		Friends friends = Player.Current.Friends;
		Character character = friends[this.m_Mobile];
		if (character != null)
		{
			friends.Characters.Remove(character);
			this.m_Mobile.IsFriend = false;
			this.m_Mobile.AddTextMessage("", "- unfriended -", Engine.DefaultFont, Hues.Load(144), unremovable: true);
			Engine.AddTextMessage("They have been removed from the friends list.", Engine.DefaultFont, Hues.Load(144));
		}
		else if (this.m_Mobile.HasName)
		{
			friends.Characters.Add(new Character(this.m_Mobile));
			this.m_Mobile.IsFriend = true;
			if (ClientFormatEx.FriendLocEnabled)
			{
				GRadar.RegisterTrackable(this.m_Mobile);
			}
			this.m_Mobile.AddTextMessage("", "- friended -", Engine.DefaultFont, Hues.Load(63), unremovable: true);
			Engine.AddTextMessage("They have been added to the friends list.", Engine.DefaultFont, Hues.Load(63));
		}
		else
		{
			Engine.AddTextMessage("Unable to friend that person.");
		}
	}
}
