namespace UOAIO;

public class GMessageBoxYesNo : GDragable
{
	private class GMBYNButton : GButtonNew
	{
		private GMessageBoxYesNo m_Owner;

		private bool m_Response;

		public GMBYNButton(GMessageBoxYesNo owner, int gumpID, int x, bool response)
			: base(gumpID, gumpID + 2, gumpID + 1, x, 75)
		{
			this.m_Owner = owner;
			this.m_Response = response;
			base.m_CanEnter = response;
		}

		protected override void OnClicked()
		{
			this.m_Owner.Signal(this.m_Response);
			Gumps.Destroy(this.m_Owner);
		}
	}

	private MBYesNoCallback m_Callback;

	public void Signal(bool response)
	{
		this.OnSignal(response);
		if (this.m_Callback != null)
		{
			this.m_Callback(this, response);
		}
	}

	protected virtual void OnSignal(bool response)
	{
	}

	public GMessageBoxYesNo(string prompt, bool modal, MBYesNoCallback callback)
		: base(2070, 0, 0)
	{
		this.m_Callback = callback;
		this.Center();
		base.m_CanClose = false;
		base.m_Children.Add(new GLabel(prompt, Engine.GetFont(1), Hues.Load(927), 33, 27));
		base.m_Children.Add(new GMBYNButton(this, 2071, 37, response: false));
		base.m_Children.Add(new GMBYNButton(this, 2074, 100, response: true));
		Gumps.Modal = this;
	}
}
