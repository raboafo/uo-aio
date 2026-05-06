namespace UOAIO;

internal class TravelContext : UseContext
{
	private RunebookInfo m_Book;

	private RuneInfo m_Rune;

	private bool m_Recall;

	public RunebookInfo BookInfo => this.m_Book;

	public RuneInfo RuneInfo => this.m_Rune;

	public bool Recall => this.m_Recall;

	public TravelContext(RunebookInfo book, RuneInfo rune, bool recall)
		: base(book.Find(), isManual: false)
	{
		this.m_Book = book;
		this.m_Rune = rune;
		this.m_Recall = recall;
	}

	public override void OnDispatch()
	{
		if (base.toUse is Item book)
		{
			Network.Send(new PPE_InvokeRunebook(book, this.m_Rune, this.m_Recall));
		}
	}

	protected override void OnEnqueue()
	{
		Mobile player = World.Player;
		player.AddTextMessage(player.Name, "- " + this.m_Rune.Name + " -", Engine.DefaultFont, Hues.Load(53), unremovable: true);
		Party.SendAutomatedMessage("{0} to {1}", this.m_Recall ? "Recalling" : "Gating", this.m_Rune.Name);
	}
}
