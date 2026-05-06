using System;
using System.Windows.Forms;

namespace UOAIO;

public class GQuestionMenu : GBackground
{
	private int m_Serial;

	private int m_MenuID;

	private GVSlider m_Slider;

	private GQuestionMenuEntry[] m_Entries;

	public int Serial => this.m_Serial;

	public int MenuID => this.m_MenuID;

	protected internal override void OnMouseWheel(int delta)
	{
		if (this.m_Slider != null)
		{
			this.m_Slider.OnMouseWheel(delta);
		}
	}

	protected internal override void OnMouseDown(int x, int y, MouseButtons mb)
	{
		base.BringToTop();
	}

	public GQuestionMenu(int serial, int menuID, string question, AnswerEntry[] answers)
		: base(9204, Engine.ScreenWidth / 2, 100, 50, 50, HasBorder: true)
	{
		base.m_CanDrag = true;
		base.m_QuickDrag = true;
		this.m_Serial = serial;
		this.m_MenuID = menuID;
		GWrappedLabel gWrappedLabel = new GWrappedLabel(question, Engine.GetFont(1), Hues.Load(1109), base.OffsetX + 4, base.OffsetY + 4, base.UseWidth - 8);
		base.m_Children.Add(gWrappedLabel);
		this.m_Entries = new GQuestionMenuEntry[answers.Length];
		GBackground gBackground = new GQuestionBackground(this.m_Entries, base.UseWidth - 8, base.UseHeight - 8 - gWrappedLabel.Height - 4, base.OffsetX + 4, gWrappedLabel.Y + gWrappedLabel.Height + 4);
		gBackground.SetMouseOverride(this);
		int offsetX = gBackground.OffsetX;
		int num = gBackground.OffsetY;
		int useWidth = gBackground.UseWidth;
		for (int i = 0; i < answers.Length; i++)
		{
			GQuestionMenuEntry gQuestionMenuEntry = new GQuestionMenuEntry(offsetX, num, useWidth, answers[i]);
			gBackground.Children.Add(gQuestionMenuEntry);
			gQuestionMenuEntry.Radio.ParentOverride = gBackground;
			this.m_Entries[i] = gQuestionMenuEntry;
			num += gQuestionMenuEntry.Height + 4;
		}
		gBackground.Height = num - 4 - gBackground.OffsetY + (gBackground.Height - gBackground.UseHeight);
		this.Height = this.Height - base.UseHeight + 4 + gWrappedLabel.Height + 4 + gBackground.Height + 4;
		int num2 = (int)((double)Engine.ScreenHeight * 0.75);
		if (this.Height > num2)
		{
			this.Height = num2;
			gBackground.Height = base.UseHeight - 8 - gWrappedLabel.Height - 4;
		}
		num -= 4;
		num -= gBackground.OffsetY;
		if (num > gBackground.UseHeight)
		{
			int num3 = num;
			gBackground.Width += 19;
			this.Width += 19;
			offsetX = gBackground.OffsetX + gBackground.UseWidth - 15;
			num = gBackground.OffsetY;
			gBackground.Children.Add(new GImage(257, offsetX, num));
			gBackground.Children.Add(new GImage(255, offsetX, num + gBackground.UseHeight - 32));
			for (int j = num + 30; j + 32 < gBackground.UseHeight; j += 30)
			{
				gBackground.Children.Add(new GImage(256, offsetX, j));
			}
			this.m_Slider = new GVSlider(254, offsetX + 1, num + 1 + 12, 13, gBackground.UseHeight - 2 - 24, 0.0, 0.0, num3 - gBackground.UseHeight, 1.0);
			this.m_Slider.OnValueChange = OnScroll;
			this.m_Slider.ScrollOffset = 20.0;
			gBackground.Children.Add(this.m_Slider);
			gBackground.Children.Add(new GHotspot(offsetX, num, 15, gBackground.UseHeight, this.m_Slider));
		}
		GButtonNew gButtonNew = new GButtonNew(243, 242, 241, 0, gBackground.Y + gBackground.Height + 4);
		GButtonNew gButtonNew2 = new GButtonNew(249, 247, 248, 0, gButtonNew.Y);
		gButtonNew.Clicked += Cancel_Clicked;
		gButtonNew2.Clicked += Okay_Clicked;
		gButtonNew.X = base.OffsetX + base.UseWidth - 4 - gButtonNew.Width;
		gButtonNew2.X = gButtonNew.X - 4 - gButtonNew2.Width;
		base.m_Children.Add(gButtonNew);
		base.m_Children.Add(gButtonNew2);
		this.Height += 4 + gButtonNew.Height;
		base.m_Children.Add(gBackground);
		this.Center();
	}

	protected internal override void OnMouseUp(int x, int y, MouseButtons mb)
	{
		if ((mb & MouseButtons.Right) != MouseButtons.None)
		{
			this.Cancel();
		}
	}

	private void Cancel_Clicked(object sender, EventArgs e)
	{
		this.Cancel();
	}

	private void Cancel()
	{
		Network.Send(new PQuestionMenuCancel(this.m_Serial, this.m_MenuID));
		Gumps.Destroy(this);
	}

	private void Okay_Clicked(object sender, EventArgs e)
	{
		for (int i = 0; i < this.m_Entries.Length; i++)
		{
			if (this.m_Entries[i].Radio.State)
			{
				AnswerEntry answer = this.m_Entries[i].Answer;
				Network.Send(new PQuestionMenuResponse(this.m_Serial, this.m_MenuID, answer.Index, answer.ItemID, 0));
				Gumps.Destroy(this);
				break;
			}
		}
	}

	private void OnScroll(double vNew, double vOld, Gump g)
	{
		int num = (int)vNew;
		for (int i = 0; i < this.m_Entries.Length; i++)
		{
			this.m_Entries[i].Y = this.m_Entries[i].yBase - num;
		}
	}
}
