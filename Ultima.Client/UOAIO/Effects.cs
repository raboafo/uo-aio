using System;
using System.Collections.Generic;

namespace UOAIO;

public class Effects
{
	private static RenderEffect[] _itemEffects;

	private List<Fade> m_Lock;

	private List<Effect> m_List;

	private List<IParticle> m_Particles;

	private List<Fade> m_Fades;

	private TransformedColoredTextured[] m_Screen;

	private int m_Temperature = 10;

	private bool m_DrawTemperature;

	private bool m_Locked;

	private bool m_Invalidated;

	private int m_GlobalLight;

	public bool DrawTemperature
	{
		get
		{
			return this.m_DrawTemperature;
		}
		set
		{
			this.m_DrawTemperature = value;
		}
	}

	public int ParticleCount => this.m_Particles.Count;

	public int Temperature
	{
		get
		{
			return this.m_Temperature;
		}
		set
		{
			this.m_Temperature = value;
		}
	}

	public bool Locked => this.m_Locked;

	public int GlobalLight
	{
		get
		{
			return this.m_GlobalLight;
		}
		set
		{
			this.m_GlobalLight = value;
		}
	}

	public static RenderEffect GetItemEffect(int itemId)
	{
		if (Effects._itemEffects == null)
		{
			Effects._itemEffects = Effects.GetItemEffects();
		}
		return Effects._itemEffects[itemId & 0x3FFF];
	}

	private static RenderEffect[] GetItemEffects()
	{
		RenderEffect[] array = new RenderEffect[16384];
		RenderEffect renderEffect = new RenderEffect(1f, DrawBlendType.Additive, new RenderEffectGlow(0.75f, 1989360));
		RenderEffect renderEffect2 = new RenderEffect(1f, DrawBlendType.Additive, new RenderEffectGlow(0.75f, 15736350));
		RenderEffect renderEffect3 = new RenderEffect(0.75f, DrawBlendType.Normal, new RenderEffectGlow(1f, 12616492));
		for (int i = 14000; i < 14089; i++)
		{
			array[i] = RenderEffects.AdditiveGlow;
		}
		for (int j = 14089; j < 14120; j++)
		{
			array[j] = renderEffect3;
		}
		for (int k = 14120; k < 14133; k++)
		{
			array[k] = RenderEffects.Additive;
		}
		for (int l = 14138; l < 14154; l++)
		{
			array[l] = renderEffect;
		}
		for (int m = 14154; m < 14170; m++)
		{
			array[m] = renderEffect2;
		}
		for (int n = 14170; n < 14217; n++)
		{
			array[n] = renderEffect;
		}
		array[14239] = new RenderEffect(1f, DrawBlendType.Additive, new RenderEffectGlow(1f, 9203350));
		return array;
	}

	public Effects()
	{
		this.m_List = new List<Effect>();
		this.m_Particles = new List<IParticle>();
		this.m_Fades = new List<Fade>();
		this.m_Lock = new List<Fade>();
		this.m_Screen = VertexConstructor.Create();
	}

	private void UpdateScreen()
	{
		this.m_Screen[3].X = -0.5f + (float)Engine.GameX;
		this.m_Screen[3].Y = -0.5f + (float)Engine.GameY;
		this.m_Screen[1].X = -0.5f + (float)Engine.GameX + (float)Engine.ScreenWidth;
		this.m_Screen[1].Y = -0.5f + (float)Engine.GameY;
		this.m_Screen[0].X = -0.5f + (float)Engine.GameX + (float)Engine.ScreenWidth;
		this.m_Screen[0].Y = -0.5f + (float)Engine.GameY + (float)Engine.ScreenHeight;
		this.m_Screen[2].X = -0.5f + (float)Engine.GameX;
		this.m_Screen[2].Y = -0.5f + (float)Engine.GameY + (float)Engine.ScreenHeight;
	}

	public void Offset(int xDelta, int yDelta)
	{
		int count = this.m_Particles.Count;
		int num = 0;
		while (num < count)
		{
			IParticle particle = this.m_Particles[num];
			if (!particle.Offset(xDelta, yDelta))
			{
				this.m_Particles.RemoveAt(num);
				particle.Destroy();
				count = this.m_Particles.Count;
			}
			else
			{
				num++;
			}
		}
	}

	public void ClearParticles()
	{
		int count = this.m_Particles.Count;
		for (int i = 0; i < count; i++)
		{
			this.m_Particles[i].Invalidate();
		}
	}

	public void ClearParticle()
	{
		if (this.m_Particles.Count > 0)
		{
			this.m_Particles[0].Invalidate();
		}
	}

	public void Add(IParticle p)
	{
		this.m_Particles.Add(p);
		this.m_Invalidated = true;
	}

	public void Add(Effect e)
	{
		this.m_List.Add(e);
		e.OnStart();
	}

	public void Add(Fade f)
	{
		if (this.m_Locked)
		{
			this.m_Lock.Add(f);
		}
		else
		{
			this.m_Fades.Add(f);
		}
	}

	public void Lock()
	{
		this.m_Locked = true;
	}

	public void Unlock()
	{
		this.m_Locked = false;
		for (int i = 0; i < this.m_Lock.Count; i++)
		{
			this.m_Fades.Add(this.m_Lock[i]);
		}
		this.m_Fades.Clear();
	}

	public void RenderLights()
	{
		int count = this.m_List.Count;
		int i = 0;
		if (count > 0)
		{
			for (; i < count; i++)
			{
				Effect effect = this.m_List[i];
				effect.RenderLight();
			}
		}
	}

	public void Draw()
	{
		this.UpdateScreen();
		int count = this.m_List.Count;
		int num = 0;
		bool flag = false;
		if (count > 0)
		{
			while (num < count)
			{
				Effect effect = this.m_List[num];
				if (!effect.Slice())
				{
					effect.OnStop();
					this.m_List.RemoveAt(num);
					EffectList children = effect.Children;
					int count2 = children.Count;
					for (int i = 0; i < count2; i++)
					{
						this.m_List.Add(children[i]);
						children[i].OnStart();
					}
					count = this.m_List.Count;
				}
				else
				{
					if (effect is LightningEffect)
					{
						flag = true;
					}
					num++;
				}
			}
		}
		count = this.m_Particles.Count;
		num = 0;
		if (count > 0)
		{
			while (num < count)
			{
				IParticle particle = this.m_Particles[num];
				if (!particle.Slice())
				{
					this.m_Particles.RemoveAt(num);
					particle.Destroy();
					count = this.m_Particles.Count;
					this.m_Invalidated = false;
					continue;
				}
				if (this.m_Invalidated)
				{
					count = this.m_Particles.Count;
					this.m_Invalidated = false;
				}
				num++;
			}
		}
		if (this.m_DrawTemperature)
		{
			if (this.Temperature > 25)
			{
				Renderer.SetTexture(null);
				Renderer.PushAlpha(((float)this.m_Temperature - 25f) / 102f);
				this.m_Screen[0].Color = (this.m_Screen[1].Color = (this.m_Screen[2].Color = (this.m_Screen[3].Color = Renderer.GetQuadColor(16728096))));
				Renderer.DrawQuadPrecalc(this.m_Screen);
				Renderer.PopAlpha();
			}
			else if (this.Temperature < 10)
			{
				Renderer.SetTexture(null);
				Renderer.PushAlpha((float)Math.Abs(this.m_Temperature - 10) / 118f);
				this.m_Screen[0].Color = (this.m_Screen[1].Color = (this.m_Screen[2].Color = (this.m_Screen[3].Color = Renderer.GetQuadColor(4243711))));
				Renderer.DrawQuadPrecalc(this.m_Screen);
				Renderer.PopAlpha();
			}
		}
		int num2 = this.m_GlobalLight;
		Mobile player = World.Player;
		if (player != null)
		{
			num2 -= player.LightLevel;
		}
		if (flag)
		{
			num2 /= 2;
			if (num2 > 0)
			{
				num2 -= Engine.Random.Next(num2);
			}
			int num3 = Engine.Random.Next(4);
			if (num3 > 0)
			{
				Renderer.SetTexture(null);
				Renderer.PushAlpha((float)((double)num3 / 31.0));
				this.m_Screen[0].Color = (this.m_Screen[1].Color = (this.m_Screen[2].Color = (this.m_Screen[3].Color = Renderer.GetQuadColor(16777215))));
				Renderer.DrawQuadPrecalc(this.m_Screen);
				Renderer.PopAlpha();
			}
		}
		if (num2 < 0)
		{
			num2 = 0;
		}
		else if (num2 > 31)
		{
			num2 = 31;
		}
		count = this.m_Fades.Count;
		num = 0;
		if (count <= 0)
		{
			return;
		}
		Renderer.SetTexture(null);
		double Alpha = 0.0;
		while (num < count)
		{
			Fade fade = this.m_Fades[num];
			if (fade.Evaluate(ref Alpha))
			{
				Renderer.PushAlpha((float)Alpha);
				this.m_Screen[0].Color = (this.m_Screen[1].Color = (this.m_Screen[2].Color = (this.m_Screen[3].Color = Renderer.GetQuadColor(fade.Color))));
				Renderer.DrawQuadPrecalc(this.m_Screen);
				Renderer.PopAlpha();
				num++;
			}
			else
			{
				fade.OnFadeComplete();
				this.m_Fades.RemoveAt(num);
				count = this.m_Fades.Count;
			}
		}
	}
}
