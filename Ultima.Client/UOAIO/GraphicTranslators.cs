namespace UOAIO;

public static class GraphicTranslators
{
	public static readonly GraphicTranslator Art;

	public static readonly GraphicTranslator Bodies;

	public static readonly GraphicTranslator Corpse;

	public static readonly GraphicTranslator Gumps;

	public static readonly GraphicTranslator Sound;

	public static readonly GraphicTranslator Music;

	public static readonly GraphicTranslator Textures;

	public static readonly GraphicTranslator[] Actions;

	static GraphicTranslators()
	{
		Debug.Trace("GraphicTranslators called...");
		GraphicTranslators.Art = new GraphicTranslator("art.def");
		GraphicTranslators.Bodies = new GraphicTranslator("body.def");
		GraphicTranslators.Corpse = new GraphicTranslator("corpse.def");
		GraphicTranslators.Gumps = new GraphicTranslator("gump.def");
		GraphicTranslators.Sound = new GraphicTranslator("sound.def");
		GraphicTranslators.Music = new GraphicTranslator("music.def");
		GraphicTranslators.Textures = new GraphicTranslator("texterr.def");
		GraphicTranslators.Actions = new GraphicTranslator[5]
		{
			new GraphicTranslator("anim1.def"),
			new GraphicTranslator("anim2.def"),
			new GraphicTranslator("anim3.def"),
			new GraphicTranslator("anim4.def"),
			new GraphicTranslator("anim5.def")
		};
	}
}
